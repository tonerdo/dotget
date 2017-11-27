using System;
using System.Linq;
using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal class ResolverFactory
    {
        private Resolver[] _resolvers;

        public ResolverFactory(string tool, ResolverOptions options, ResolutionType resolutionType, ILogger logger)
        {
            _resolvers = new Resolver[] { new NuGetPackageResolver(tool, options, resolutionType, logger) };
        }

        public Resolver GetResolver()
        {
            return _resolvers.FirstOrDefault(r => r.CanResolve())
                ?? throw new Exception("No suitable resolver found");
        }
    }
}