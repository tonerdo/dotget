using System;
using DotGet.Core.Configuration;

namespace DotGet.Core.Resolvers
{
    internal abstract class Resolver
    {
        protected string Tool { get; private set; }
        protected ResolverOptions Options { get; private set; }
        public Resolver(string tool, ResolverOptions options)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            this.Tool = tool;
            this.Options = options;
        }
        public abstract bool CanResolve();
        public abstract (bool, string) Resolve();
    }
}