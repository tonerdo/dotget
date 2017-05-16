using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class InstallCommand
    {
        private string _tool;
        private CommandOptions _options;
        private ILogger _logger;

        internal ResolutionType ResolutionType;

        public InstallCommand(string tool, CommandOptions options, ILogger logger)
        {
            _tool = tool;
            _options = options;
            _logger = logger;
            ResolutionType = ResolutionType.Install;
        }

        private string BuildBinContents(string dllPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $"dotnet {dllPath} %*";

            return $"#!/usr/bin/env bash \ndotnet {dllPath} \"$@\"";
        }

        private string BuildBinFilename(string dllPath)
        {
            string filename = Path.GetFileNameWithoutExtension(dllPath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return filename + ".cmd";

            return filename;
        }

        private string BuildEtcContents()
        {
            Dictionary<string, string> etc = _options;
            if (!etc.ContainsKey("tool"))
                etc.Add("tool", _tool);

            return Newtonsoft.Json.JsonConvert.SerializeObject(etc);
        }

        public void Execute()
        {
            Resolver resolver = new ResolverFactory(_tool, new ResolverOptions(_options), this.ResolutionType, _logger).GetResolver();
            (bool success, string dllPath) = resolver.Resolve();
            if (!success)
            {
                _logger.LogError($"Failed to install {_tool}!");
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

            string binFileName = BuildBinFilename(dllPath);
            File.WriteAllText(Path.Combine(etcDirectory, binFileName), BuildEtcContents());
            File.WriteAllText(Path.Combine(binDirectory, binFileName), BuildBinContents(dllPath));
            // TODO: make unix bin file executable

            string report = this.ResolutionType == ResolutionType.Install ? "installed" : "updated";
            _logger.LogSuccess($"{_tool} successfully {report}!");
        }
    }
}