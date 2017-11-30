using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Logging;

namespace DotGet.Core.Commands
{
    public class UninstallCommand : ICommand
    {
        private string _source;
        private ILogger _logger;

        public UninstallCommand(string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
        }

        public Dictionary<string, string> GetEtc(string path)
        {
            string json = File.ReadAllText(path);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        private string GetBinExtension()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ".cmd";

            return string.Empty;
        }

        public bool Execute()
        {
            string globalNugetDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            globalNugetDirectory = Path.Combine(globalNugetDirectory, ".nuget");

            string etcDirectory = Path.Combine(globalNugetDirectory, "etc");
            string binDirectory = Path.Combine(globalNugetDirectory, "bin");
            _logger.LogInformation($"Uninstalling {_source}...");

            string[] etcFiles = Directory.GetFiles(etcDirectory);
            Dictionary<string, string> etc = null;
            string toolEtc = null;

            foreach (string filePath in etcFiles)
            {
                etc = GetEtc(filePath);
                if (etc["tool"] == _source)
                {
                    toolEtc = filePath;
                    break;
                }
            }

            if (toolEtc == null)
            {
                _logger.LogError($"No tool with name: {_source}, is installed");
                return false;
            }

            File.Delete(toolEtc);
            File.Delete(Path.Combine(binDirectory, GetBinExtension()));
            _logger.LogSuccess($"{_source} uninstalled successfully");
            return true;
        }
    }
}