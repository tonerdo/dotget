using System;
using DotGet.Core.Configuration;

namespace DotGet.Core.Resolvers
{
    internal class NuGetPackageResolver : Resolver
    {
        public NuGetPackageResolver(string source, ResolverOptions options) : base(source, options) { }

        public override bool CanResolve() => !Source.Contains("/") && !Source.Contains(@"\") && !Source.StartsWith(".");

        public override (bool, string) Resolve()
        {
            throw new NotImplementedException();
        }
    }
}