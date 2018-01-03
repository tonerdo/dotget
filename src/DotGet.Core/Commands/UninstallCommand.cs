using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

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

        public bool Execute()
        {
            ResolverFactory resolverFactory = new ResolverFactory(_source, ResolutionType.Remove, _logger);
            Resolver resolver = resolverFactory.GetResolver();
            if (resolver == null)
            {
                _logger.LogError($"No resolver found for {_source}");
                return false;
            }

            if (!resolver.CheckInstalled())
            {
                _logger.LogError($"{_source} isn't installed");
                return false;
            }

            _logger.LogInformation($"Uninstalling {resolver.GetFullSource()}");
            if (!resolver.Remove())
            {
                _logger.LogError($"{_source} couldn't be uninstalled.");
                return false;
            }

            _logger.LogSuccess($"{_source} was uninstalled.");
            return true;
        }
    }
}