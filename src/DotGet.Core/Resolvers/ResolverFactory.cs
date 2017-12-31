using System;
using System.Linq;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal class ResolverFactory
    {
        private Resolver[] _resolvers;

        public ResolverFactory()
        {
            _resolvers = new Resolver[] { new NuGetPackageResolver() };
        }
    }
}