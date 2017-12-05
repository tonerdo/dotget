using System;
using System.Linq;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal class ResolverFactory
    {
        private Resolver[] _resolvers;

        public ResolverFactory() : this(string.Empty, ResolutionType.None, null) { }

        public ResolverFactory(string source, ResolutionType resolutionType, ILogger logger)
        {
            _resolvers = new Resolver[] { new NuGetPackageResolver(source, resolutionType, logger) };
        }

        public Resolver GetResolver()
        {
            return _resolvers.FirstOrDefault(r => r.CanResolve())
                ?? throw new Exception("No suitable resolver found");
        }

        public Resolver GetResolver(string command)
        {
            return _resolvers.FirstOrDefault(r => r.DidResolve(command))
                ?? throw new Exception("No suitable resolver found");
        }
    }
}