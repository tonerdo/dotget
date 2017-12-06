using System;
using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal abstract class Resolver
    {
        protected string Source { get; private set; }
        protected Options Options { get; private set; }
        protected ILogger Logger { get; set; }
        protected ResolutionType ResolutionType { get; set; }

        public Resolver(string source, ResolutionType resolutionType, ILogger logger)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            this.Source = source;
            this.Logger = logger;
            this.ResolutionType = resolutionType;
            this.Options = this.BuildOptions();
        }

        public abstract bool CanResolve();
        public abstract Options BuildOptions();
        public abstract bool DidResolve(string path);
        public abstract string GetSource(string path);
        public abstract string GetFullSource(string path);
        public abstract string Resolve();
    }
}