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
                throw new Exception("No resolver found");

            if (resolver.CheckInstalled())
                throw new Exception("Source already installed. Try updating!");

            if (!resolver.Resolve())
                throw new Exception("Failed to resolve source");

            return true;
        }
    }
}