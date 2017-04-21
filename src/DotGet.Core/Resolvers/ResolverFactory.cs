using System.Linq;
using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal class ResolverFactory
    {
        private Resolver[] _resolvers;

        public ResolverFactory(string tool, ResolverOptions options, ILogger logger)
        {
            _resolvers = new Resolver[] { new NuGetPackageResolver(tool, options, logger) };
        }

        public Resolver GetResolver() => _resolvers.FirstOrDefault(r => r.CanResolve());
    }
}