using System;
using System.Collections.Generic;
using System.IO;

using DotGet.Core.Helpers;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class InstallCommand : ICommand
    {
        private string _tool;
        private ILogger _logger;

        public InstallCommand(string tool, ILogger logger)
        {
            _tool = tool;
            _logger = logger;
        }

        public bool Execute()
        {
            Resolver resolver = new ResolverFactory(_tool, ResolutionType.Install, _logger).GetResolver();
            string path = string.Empty;

            try
            {
                path = resolver.Resolve();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }

            string bin = Path.Combine(Globals.GlobalNuGetDirectory, "bin");
            if (!Directory.Exists(bin))
                Directory.CreateDirectory(bin);

            string filename = Path.Combine(bin, CommandHelper.BuildBinFilename(path));

            if (File.Exists(filename))
            {
                _logger.LogError($"{_tool} is already installed. Try updating it instead!");
                return false;
            }

            File.WriteAllText(filename, CommandHelper.BuildBinContents(path));
            // TODO: make unix bin file executable
            return true;
        }
    }
}