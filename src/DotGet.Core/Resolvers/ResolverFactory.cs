using System.Linq;
using DotGet.Core.Configuration;

namespace DotGet.Core.Resolvers
{
    internal class ResolverFactory
    {
        private Resolver[] _resolvers;

        public ResolverFactory(string source, ResolverOptions options)
        {
            _resolvers = new Resolver[] { new NuGetPackageResolver(source, options) };
        }

        public Resolver GetResolver() => _resolvers.FirstOrDefault(r => r.CanResolve());
    }
}