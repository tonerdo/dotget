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
    public class UpdateCommand : ICommand
    {
        private string _source;
        private ILogger _logger;

        public UpdateCommand(string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
        }

        public bool Execute()
        {
            if (!CommandHelper.IsInstalled(_source))
            {
                _logger.LogError($"{_source} is not already installed.");
                return false;
            }

            Resolver resolver = ResolverFactory.GetResolverForSource(_source);
            string path = string.Empty;

            try
            {
                path = resolver.Resolve(_source, ResolutionType.Update, _logger);
            }
            catch (ResolverException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogVerbose(ex.Message);
                _logger.LogVerbose(ex.StackTrace);
                return false;
            }

            string filename = Path.Combine(Globals.GlobalBinDirectory, CommandHelper.BuildBinFilename(path));
            File.WriteAllText(filename, CommandHelper.BuildBinContents(path));
            return true;
        }
    }
}