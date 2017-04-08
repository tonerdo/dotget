using System;
using DotGet.Core.Configuration;

namespace DotGet.Core.Resolvers
{
    internal abstract class Resolver
    {
        protected string Source { get; private set; }
        protected ResolverOptions Options { get; private set; }
        public Resolver(string source, ResolverOptions options)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            this.Source = source;
            this.Options = options;
        }
        public abstract bool CanResolve();
        public abstract string Resolve();
    }
}