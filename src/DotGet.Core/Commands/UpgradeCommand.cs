using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Exceptions;
using DotGet.Core.Helpers;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class UpgradeCommand : ICommand
    {
        private string _source;
        private ILogger _logger;

        public UpgradeCommand(string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
        }

        public bool Execute()
        {
            Resolver resolver = new ResolverFactory(_source, ResolutionType.Upgrade, _logger).GetResolver();
            string path = string.Empty;

            try
            {
                path = resolver.Resolve();
            }
            catch (ResolverException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }

            string filename = Path.Combine(Globals.GlobalBinDirectory, CommandHelper.BuildBinFilename(path));

            if (!File.Exists(filename))
            {
                _logger.LogError($"{_source} is not installed.");
                return false;
            }

            File.WriteAllText(filename, CommandHelper.BuildBinContents(path));
            return true;
        }
    }
}