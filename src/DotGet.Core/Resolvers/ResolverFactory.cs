using System;
using System.Linq;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal static class ResolverFactory
    {
        private static Resolver[] _resolvers;

        static ResolverFactory()
        {
            _resolvers = new Resolver[] { new NuGetPackageResolver() };
        }

        public static Resolver GetResolverForSource(string source)
        {
            return _resolvers.FirstOrDefault(r => r.CanResolve(source))
                ?? throw new Exception("No suitable resolver found");
        }

        public static Resolver GetResolverForPath(string path)
        {
            return _resolvers.FirstOrDefault(r => r.DidResolve(path))
                ?? throw new Exception("No suitable resolver found");
        }
    }
}