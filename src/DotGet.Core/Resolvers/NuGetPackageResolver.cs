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

        public override string GetFullSource()
        {
            string package = ResolverOptions["package"];
            if (ResolverOptions.TryGetValue("version", out string version))
                return $"{package} ({version})";

            return package;
        }

        public override bool CanResolve()
            => !Source.Contains("/") && !Source.Contains(@"\") && !Source.StartsWith(".");

        public override bool CheckInstalled()
        {
            if (ResolutionType == ResolutionType.Install)
            {
                bool exists = Directory.Exists(Path.Combine(SpecialFolders.Lib, ResolverOptions["package"]));
                if (exists)
                    ResolverOptions["version"] = GetInstalledPackageInfo().Version;

                return exists;
            }

            if (ResolutionType == ResolutionType.Remove)
            {
                bool exists = Directory.Exists(Path.Combine(SpecialFolders.Lib, Source));
                if (exists)
                    ResolverOptions["version"] = GetInstalledPackageInfo().Version;
            }

            return Directory.Exists(Path.Combine(SpecialFolders.Lib, Source));
        }

        public override bool CheckUpdated()
        {
            string version = GetAllPackageVersions().Last();
            ResolverOptions["version"] = version;
            return GetInstalledPackageInfo().Version == version;
        }

        public override bool Exists()
        {
            string[] versions = GetAllPackageVersions();
            if (versions == null)
                return false;

            if (ResolverOptions.TryGetValue("version", out string version))
                return Array.Exists(versions, v => v == version);

            ResolverOptions["version"] = versions.Last();
            return true;
        }

        public override bool Resolve()
        {
            string package = ResolverOptions["package"];
            string version = ResolverOptions["version"];

            string packageDirectory = Path.Combine(SpecialFolders.Lib, package);
            if (ResolutionType == ResolutionType.Update)
                Directory.Delete(packageDirectory, true);

            string url = _baseUrl + package + "/" + version + "/" + package + "." + version + ".nupkg";
            Stream nupkg = GetNuPkgStream(url);

            if (nupkg == null)
                return Fail($"Could not find nupkg for {package} ({version})");

            ZipArchive zipArchive = new ZipArchive(nupkg);
            zipArchive.ExtractToDirectory(packageDirectory);

            string toolsDirectory = Path.Combine(packageDirectory, "tools");
            if (!Directory.Exists(toolsDirectory))
                return Fail($"Executable not found for {package} ({version})");

            string appDirectory = Directory
                                    .GetDirectories(toolsDirectory, "netcoreapp*", SearchOption.TopDirectoryOnly)
                                    .LastOrDefault();

            if (appDirectory == null)
                return Fail($"Executable not found for {package} ({version})");

            var executables = GetExecutableAssemblies(appDirectory);
            var commands = GetCommands(executables);

            for (int i = 0; i < executables.Count(); i++)
                CreatePlatformExecutable(executables.ElementAt(i), commands.ElementAt(i));

            UpdateInstalledPackageInfo(string.Join(",", commands));
            return true;
        }

        public override IEnumerable<string> GetInstalled()
        {
            foreach (var directory in new DirectoryInfo(SpecialFolders.Lib).GetDirectories())
            {
                FileInfo specFile = directory
                                    .GetFiles("*.nuspec", SearchOption.TopDirectoryOnly)
                                    .FirstOrDefault();

                if (specFile != null)
                {
                    ResolverOptions["package"] = directory.Name;
                    NuSpec nuSpec = GetInstalledPackageInfo();
                    yield return $"{nuSpec.Id} ({nuSpec.Version})";
                }
            }
        }

        public override bool Remove()
        {
            NuSpec nuSpec = GetInstalledPackageInfo();
            string extension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : "";

            try
            {
                foreach (var command in nuSpec.Commands.Split(','))
                {
                    Logger.LogInformation($"Removing command '{command}'");
                    File.Delete(Path.Combine(SpecialFolders.Bin, command + extension));
                }

                Directory.Delete(Path.Combine(SpecialFolders.Lib, ResolverOptions["package"]), true);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogVerbose(ex.ToString());
                return false;
            }
        }

        private bool Fail(string message = "")
        {
            Directory.Delete(Path.Combine(SpecialFolders.Lib, ResolverOptions["package"]), true);
            if (message != string.Empty)
                Logger.LogWarning(message);

            return false;
        }

        private Stream GetNuPkgStream(string url)
        {
            try
            {
                WebClient webClient = new WebClient();
                byte[] buffer = webClient.DownloadData(url);
                return new MemoryStream(buffer);
            }
            catch (Exception ex)
            {
                Logger.LogVerbose(ex.ToString());
                return null;
            }
        }

        private string[] GetAllPackageVersions()
        {
            string package = ResolverOptions["package"];

            try
            {
                WebClient webClient = new WebClient();
                string json = webClient.DownloadString(_baseUrl + package + "/" + "index.json");
                VersionResponse response = JsonConvert.DeserializeObject<VersionResponse>(json);
                return response.Versions;
            }
            catch (Exception ex)
            {
                Logger.LogVerbose(ex.ToString());
                return null;
            }
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
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            Logger.LogInformation($"Registering command '{command}'");
            if (isWindows)
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
            if (!isWindows)
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