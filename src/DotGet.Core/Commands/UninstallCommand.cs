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
        private Resolver _resolver;

        public UninstallCommand(string source, ILogger logger) : this(null, source, logger) { }

        internal UninstallCommand(Resolver resolver, string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
            _resolver = resolver ?? new ResolverFactory(_source, ResolutionType.Remove, _logger).GetResolver();
        }

        public bool Execute()
        {
            if (_resolver == null)
            {
                _logger.LogError($"No resolver found for {_source}");
                return false;
            }

            if (!_resolver.CheckInstalled())
            {
                _logger.LogError($"{_source} isn't installed");
                return false;
            }

            _logger.LogInformation($"Uninstalling {_resolver.GetFullSource()}");
            if (!_resolver.Remove())
            {
                _logger.LogError($"{_source} couldn't be uninstalled.");
                return false;
            }

            _logger.LogSuccess($"{_source} was uninstalled.");
            return true;
        }
    }
}