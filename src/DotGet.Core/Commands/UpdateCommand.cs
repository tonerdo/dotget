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
        private Resolver _resolver;

        public UpdateCommand(string source, ILogger logger) : this(null, source, logger) { }

        internal UpdateCommand(Resolver resolver, string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
            _resolver = resolver ?? new ResolverFactory(_source, ResolutionType.Update, _logger).GetResolver();
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

            if (_resolver.CheckUpdated())
            {
                _logger.LogSuccess($"{_resolver.GetFullSource()} is up to date!");
                return true;
            }

            _logger.LogInformation($"Updating {_source}");
            if (!_resolver.Resolve())
            {
                _logger.LogError($"Unable to update {_source}");
                return false;
            }

            _logger.LogSuccess($"{_source} updated successfully!");
            return true;
        }
    }
}