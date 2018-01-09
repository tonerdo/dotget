using System;
using System.Collections.Generic;
using System.IO;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class InstallCommand : ICommand
    {
        private string _source;
        private ILogger _logger;
        private Resolver _resolver;

        public InstallCommand(string source, ILogger logger) : this(null, source, logger) { }

        internal InstallCommand(Resolver resolver, string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
            _resolver = resolver ?? new ResolverFactory(_source, ResolutionType.Install, _logger).GetResolver();
        }

        public bool Execute()
        {
            if (_resolver == null)
            {
                _logger.LogError($"No resolver found for {_source}");
                return false;
            }

            if (_resolver.CheckInstalled())
            {
                _logger.LogWarning($"{_resolver.GetFullSource()} is already installed.");
                return true;
            }

            if (!_resolver.Exists())
            {
                _logger.LogError($"Unable to find {_resolver.GetFullSource()}");
                return false;
            }

            _logger.LogInformation($"Installing {_resolver.GetFullSource()}");
            if (!_resolver.Resolve())
            {
                _logger.LogError($"Unable to resolve {_resolver.GetFullSource()}");
                return false;
            }

            _logger.LogSuccess($"{_resolver.GetFullSource()} installed successfully!");
            return true;
        }
    }
}