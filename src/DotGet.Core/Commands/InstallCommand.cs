using System;
using System.Collections.Generic;
using System.IO;

using DotGet.Core.Exceptions;
using DotGet.Core.Helpers;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class InstallCommand : ICommand
    {
        private string _source;
        private ILogger _logger;

        public InstallCommand(string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
        }

        public bool Execute()
        {
            Resolver resolver = ResolverFactory.GetResolverForSource(_source);
            string path = string.Empty;

            try
            {
                path = resolver.Resolve(_source, ResolutionType.Install, _logger);
            }
            catch (ResolverException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }

            if (!Directory.Exists(Globals.GlobalBinDirectory))
                Directory.CreateDirectory(Globals.GlobalBinDirectory);

            string filename = Path.Combine(Globals.GlobalBinDirectory, CommandHelper.BuildBinFilename(path));

            if (File.Exists(filename))
            {
                _logger.LogError($"{_source} is already installed. Try updating it instead!");
                return false;
            }

            File.WriteAllText(filename, CommandHelper.BuildBinContents(path));
            // TODO: make unix bin file executable
            return true;
        }
    }
}