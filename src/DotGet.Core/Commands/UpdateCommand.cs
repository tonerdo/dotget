using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Configuration;
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
            ResolverFactory resolverFactory = new ResolverFactory(_source, ResolutionType.Update, _logger);
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

            if (resolver.CheckUpdated())
            {
                _logger.LogSuccess($"{resolver.GetFullSource()} is up to date!");
                return true;
            }

            _logger.LogInformation($"Updating {_source}");
            if (!resolver.Resolve())
            {
                _logger.LogError($"Unable to update {_source}");
                return false;
            }

            _logger.LogSuccess($"{_source} updated successfully!");
            return true;
        }
    }
}