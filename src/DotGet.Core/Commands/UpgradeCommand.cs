using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Helpers;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class UpgradeCommand : ICommand
    {
        private string _tool;
        private ILogger _logger;

        public UpgradeCommand(string tool, ILogger logger)
        {
            _tool = tool;
            _logger = logger;
        }

        public bool Execute()
        {
            Resolver resolver = new ResolverFactory(_tool, ResolutionType.Upgrade, _logger).GetResolver();
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

            string filename = Path.Combine(Globals.GlobalBinDirectory, CommandHelper.BuildBinFilename(path));

            if (!File.Exists(filename))
            {
                _logger.LogError($"{_tool} is not installed.");
                return false;
            }

            File.WriteAllText(filename, CommandHelper.BuildBinContents(path));
            return true;
        }
    }
}