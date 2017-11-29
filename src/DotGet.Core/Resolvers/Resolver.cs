using System;
using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal abstract class Resolver
    {
        protected string Tool { get; private set; }
        protected Options Options { get; private set; }
        protected ILogger Logger { get; set; }
        protected ResolutionType ResolutionType { get; set; }

        public Resolver(string tool, ResolutionType resolutionType, ILogger logger)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            this.Tool = tool;
            this.Logger = logger;
            this.ResolutionType = resolutionType;
            this.Options = this.BuildOptions();
        }

        public abstract bool CanResolve();
        public abstract Options BuildOptions();
        public abstract bool DidResolve(string command);
        public abstract string GetToolName(string command);
        public abstract string Resolve();
    }
}