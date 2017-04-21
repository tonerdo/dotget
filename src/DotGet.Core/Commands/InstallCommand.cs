using System;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Configuration;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class InstallCommand
    {
        private string _source;
        private CommandOptions _options;
        public InstallCommand(string source, CommandOptions options)
        {
            _source = source;
            _options = options;
        }

        private ResolverOptions BuildResolverOptions()
        {
            ResolverOptions resolverOptions = new ResolverOptions();
            foreach (var option in _options)
                resolverOptions.Add(option.Key, option.Value);
            
            return resolverOptions;
        }

        private string GetBinContents(string dllPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $"dotnet {dllPath} %*";

            return $"#!/usr/bin/env bash \n dotnet {dllPath} \"$@\"";
        }

        private string GetBinFilename(string dllPath)
        {
            string filename = Path.GetFileNameWithoutExtension(dllPath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return filename + ".cmd";

            return filename;
        }

        private string GetEtcContents(string dllPath)
        {
            string contents = $"bin=:={GetBinFilename(dllPath)}\n";
            foreach (var option in _options)
                contents += $"{option.Key}=:={option.Value}\n";

            return contents;
        }

        public void Execute()
        {
            Resolver resolver = new ResolverFactory(_source, BuildResolverOptions()).GetResolver();
            (bool success, string dllPath) = resolver.Resolve();
            if (!success)
            {
                Console.WriteLine("Failed to install {0}!", _source);
                return;
            }

            string globalNugetDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            globalNugetDirectory = Path.Combine(globalNugetDirectory, ".nuget");

            string etcDirectory = Path.Combine(globalNugetDirectory, "etc");
            if (!Directory.Exists(etcDirectory))
                Directory.CreateDirectory(etcDirectory);

            string binDirectory = Path.Combine(globalNugetDirectory, "bin");
            if (!Directory.Exists(binDirectory))
                Directory.CreateDirectory(binDirectory);

            File.WriteAllText(Path.Combine(etcDirectory, _source), GetEtcContents(dllPath));
            File.WriteAllText(Path.Combine(binDirectory, GetBinFilename(dllPath)), GetBinContents(dllPath));
            // TODO: make unix bin file executable

            Console.WriteLine("{0} successfully installed!", _source);
        }
    }
}