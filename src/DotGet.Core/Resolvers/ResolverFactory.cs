using System;
using System.Linq;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal class ResolverFactory
    {
        private Resolver[] _resolvers;

        public ResolverFactory(string source, ResolutionType resolutionType, ILogger logger)
        {
            _resolvers = new Resolver[]
            { 
                new NuGetPackageResolver(source, resolutionType, logger)
            };
        }

        public Resolver GetResolver() => _resolvers.FirstOrDefault(r => r.CanResolve());
        public Resolver[] GetAllResolvers() => _resolvers;
    }
}