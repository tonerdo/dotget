using System;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal abstract class Resolver
    {
        protected string Source { get; private set; }
        protected ResolutionType ResolutionType { get; private set; }
        protected ILogger Logger { get; private set; }
        protected ResolverOptions ResolverOptions { get; private set; }

        public Resolver(string source, ResolutionType resolutionType, ILogger logger)
        {
            this.Source = source;
            this.ResolutionType = resolutionType;
            this.Logger = logger;
            this.ResolverOptions = BuildOptions();
        }

        public abstract ResolverOptions BuildOptions();
        public abstract bool CanResolve();
        public abstract bool CheckInstalled();
        public abstract bool Resolve();
        public abstract bool Remove();
    }
}