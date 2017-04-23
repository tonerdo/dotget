using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Commands
{
    public class UninstallCommand
    {
        private string _tool;
        private CommandOptions _options;
        private ILogger _logger;

        public UninstallCommand(string tool, CommandOptions options, ILogger logger)
        {
            _options = options;
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

        public void Execute()
        {
            string globalNugetDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            globalNugetDirectory = Path.Combine(globalNugetDirectory, ".nuget");

            string etcDirectory = Path.Combine(globalNugetDirectory, "etc");
            string binDirectory = Path.Combine(globalNugetDirectory, "bin");
            _logger.LogInformation("Uninstalling {_tool}...");

            string[] etcFiles = Directory.GetFiles(etcDirectory);
            Dictionary<string, string> etc = null;
            string toolEtc = null;
            foreach (string filePath in etcFiles)
            {
                etc = GetEtc(filePath);
                if (etc["tool"] == _tool)
                {
                    toolEtc = filePath;
                    break;
                }
            }

            if (etc == null)
                _logger.LogError("No tool with name: {_tool} is installed");
            
            File.Delete(toolEtc);
            File.Delete(Path.Combine(binDirectory, GetBinExtension()));
            _logger.LogSuccess($"{_tool} uninstalled successfully");
        }
    }
}