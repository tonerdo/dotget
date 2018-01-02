using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Xml.Linq;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;

using Newtonsoft.Json;

namespace DotGet.Core.Resolvers
{
    internal class NuGetPackageResolver : Resolver
    {
        private class VersionResponse
        {
            public string[] Versions { get; set; }
        }

        private class NuSpec
        {
            public string Id { get; set; }
            public string Version { get; set; }
            public string Commands { get; set; }
        }

        private readonly string _baseUrl = "https://api.nuget.org/v3-flatcontainer/";

        public NuGetPackageResolver(string source, ResolutionType resolutionType, ILogger logger) : base(source, resolutionType, logger) { }

        public override ResolverOptions BuildOptions()
        {
            ResolverOptions options = new ResolverOptions();

            string[] parts = Source.Split('@');
            options.Add("package", parts[0].ToLowerInvariant());

            if (parts.Length > 1)
                options.Add("version", parts[1].ToLowerInvariant());

            return options;
        }

        public override bool CanResolve()
            => !Source.Contains("/") && !Source.Contains(@"\") && !Source.StartsWith(".");

        public override bool CheckInstalled()
        {
            if (ResolutionType == ResolutionType.Install)
                return Directory.Exists(Path.Combine(SpecialFolders.Lib, ResolverOptions["package"]));

            return Directory.Exists(Path.Combine(SpecialFolders.Lib, Source));
        }

        public override bool CheckUpdated()
            => GetInstalledPackageInfo().Version == GetLatestPackageVersion();

        public override bool Resolve()
        {
            string package = ResolverOptions["package"];

            if (!ResolverOptions.TryGetValue("version", out string version))
            {
                version = GetLatestPackageVersion();
                if (version == null)
                    return false;
            }

            string packageDirectory = Path.Combine(SpecialFolders.Lib, package);

            if (ResolutionType == ResolutionType.Update)
                Directory.Delete(packageDirectory, true);

            string url = _baseUrl + package + "/" + version + "/" + package + "." + version + ".nupkg";
            Stream nupkg = GetNuPkgStream(url);

            if (nupkg == null)
                return false;

            ZipArchive zipArchive = new ZipArchive(nupkg);
            zipArchive.ExtractToDirectory(packageDirectory);

            string toolsDirectory = Path.Combine(packageDirectory, "tools");
            if (!Directory.Exists(toolsDirectory))
                return false;

            string appDirectory = Directory
                                    .GetDirectories(toolsDirectory, "netcoreapp*", SearchOption.TopDirectoryOnly)
                                    .LastOrDefault();

            if (appDirectory == null)
                return false;

            var executables = GetExecutableAssemblies(appDirectory);
            var commands = GetCommands(executables);

            for (int i = 0; i < executables.Count(); i++)
                CreatePlatformExecutable(executables.ElementAt(i), commands.ElementAt(i));

            UpdateInstalledPackageInfo(string.Join(",", commands));
            return true;
        }

        public override bool Remove()
        {
            NuSpec nuSpec = GetInstalledPackageInfo();
            string extension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : "";

            try
            {
                foreach (var command in nuSpec.Commands.Split(','))
                    File.Delete(Path.Combine(SpecialFolders.Bin, command + extension));

                Directory.Delete(Path.Combine(SpecialFolders.Lib, ResolverOptions["package"]), true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Stream GetNuPkgStream(string url)
        {
            WebRequest webRequest = WebRequest.Create(url);
            WebResponse webResponse = webRequest.GetResponse();
            if (((HttpWebResponse)webResponse).StatusCode != HttpStatusCode.OK)
                return null;

            return webResponse.GetResponseStream();
        }

        private string GetLatestPackageVersion()
        {
            string package = ResolverOptions["package"];

            WebRequest webRequest = WebRequest.Create(_baseUrl + package + "/" + "index.json");
            WebResponse webResponse = webRequest.GetResponse();
            if (((HttpWebResponse)webResponse).StatusCode != HttpStatusCode.OK)
                return null;

            StreamReader reader = new StreamReader(webResponse.GetResponseStream());
            string json = reader.ReadToEnd();

            VersionResponse response = JsonConvert.DeserializeObject<VersionResponse>(json);
            return response.Versions.Last();
        }

        private XElement GetMetadataElement()
        {
            string package = ResolverOptions["package"];
            string xml = File.ReadAllText(Path.Combine(SpecialFolders.Lib, package, $"{package}.nuspec"));
            XElement root = XDocument.Parse(xml).Root;
            XNamespace xNamespace = root.GetDefaultNamespace();
            return root.Element(xNamespace + "metadata");
        }

        private NuSpec GetInstalledPackageInfo()
        {
            XElement metadata = GetMetadataElement();
            XNamespace xNamespace = metadata.GetDefaultNamespace();
            return new NuSpec()
            {
                Id = metadata.Element(xNamespace + "id")?.Value,
                Version = metadata.Element(xNamespace + "version")?.Value,
                Commands = metadata.Element(xNamespace + "commands")?.Value
            };
        }

        private void UpdateInstalledPackageInfo(string commands)
        {
            string package = ResolverOptions["package"];
            XElement metadata = GetMetadataElement();
            XNamespace xNamespace = metadata.GetDefaultNamespace();
            metadata.Add(
                new XElement(xNamespace + "commands", commands)
            );
            metadata.Document.Save(Path.Combine(SpecialFolders.Lib, package, $"{package}.nuspec"));
        }

        private IEnumerable<string> GetExecutableAssemblies(string appDirectory)
        {
            string[] assemblies = Directory.GetFiles(appDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            AssemblyLoadContext assemblyLoadContext = AssemblyLoadContext.Default;

            foreach (var assembly in assemblies)
            {
                if (assemblyLoadContext.LoadFromAssemblyPath(assembly).EntryPoint != null)
                    yield return assembly;
            }
        }

        private IEnumerable<string> GetCommands(IEnumerable<string> executables)
        {
            foreach (var executable in executables)
                yield return Path.GetFileNameWithoutExtension(executable);
        }

        private void CreatePlatformExecutable(string executable, string command)
        {
            string contents = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                command += ".cmd";
                contents = $"dotnet {executable} %*";
            }
            else
            {
                contents = "#!/usr/bin/env bash";
                contents += Environment.NewLine;
                contents += $"dotnet {executable} $@";
            }

            File.WriteAllText(Path.Combine(SpecialFolders.Bin, command), contents);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process process = new Process();
                process.StartInfo.FileName = "chmod";
                process.StartInfo.Arguments = $"+x {Path.Combine(SpecialFolders.Bin, command)}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }
        }
    }
}