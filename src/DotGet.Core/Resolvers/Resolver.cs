using System;
using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal abstract class Resolver
    {
        protected string Tool { get; private set; }
        protected ResolverOptions Options { get; private set; }
        protected ILogger Logger { get; set; }

        public Resolver(string tool, ResolverOptions options, ILogger logger)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            this.Tool = tool;
            this.Options = options;
            this.Logger = logger;
        }
        public abstract bool CanResolve();
        public abstract (bool, string) Resolve();
    }
}