using System;
using System.Collections.Generic;
using System.IO;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

using Newtonsoft.Json;

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
            ResolverFactory resolverFactory = new ResolverFactory(_source, ResolutionType.Install, _logger);
            Resolver resolver = resolverFactory.GetResolver();
            if (resolver == null)
            {
                _logger.LogError($"No resolver found for {_source}");
                return false;
            }

            if (resolver.CheckInstalled())
            {
                _logger.LogWarning($"{resolver.GetFullSource()} is already installed.");
                return true;
            }

            if (!resolver.Exists())
            {
                _logger.LogError($"Unable to find {resolver.GetFullSource()}");
                return false;
            }

            _logger.LogInformation($"Installing {resolver.GetFullSource()}");
            if (!resolver.Resolve())
            {
                _logger.LogError($"Unable to resolve {resolver.GetFullSource()}");
                return false;
            }

            _logger.LogSuccess($"{resolver.GetFullSource()} installed successfully!");
            return true;
        }
    }
}