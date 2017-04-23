using System;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Commands
{
    public class ListCommand
    {
        private CommandOptions _options;
        private ILogger _logger;

        public ListCommand(CommandOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }

        public void Execute()
        {
            string globalNugetDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            globalNugetDirectory = Path.Combine(globalNugetDirectory, ".nuget");

            string etcDirectory = Path.Combine(globalNugetDirectory, "etc");
            _logger.LogInformation(etcDirectory);
        }
    }
}