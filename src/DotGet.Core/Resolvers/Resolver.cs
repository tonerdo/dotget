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
        protected ResolutionType ResolutionType { get; set; }

        public Resolver(string tool, ResolverOptions options, ResolutionType resolutionType, ILogger logger)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            this.Tool = tool;
            this.Options = options;
            this.Logger = logger;
            this.ResolutionType = ResolutionType.Install;
        }
        public abstract bool CanResolve();
        public abstract (bool, string) Resolve();
    }
}