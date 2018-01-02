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
                throw new Exception("No resolver found");

            if (!resolver.CheckInstalled())
                throw new Exception("Source not already installed.");

            if (!resolver.Remove())
                throw new Exception("Failed to remove source");

            return true;
        }
    }
}