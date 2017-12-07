using System;
using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal abstract class Resolver
    {
        public abstract bool CanResolve(string source);
        public abstract bool DidResolve(string path);
        public abstract string GetSource(string path);
        public abstract string GetFullSource(string path);
        public abstract string Resolve(string source, ResolutionType resolutionType, ILogger logger);
    }
}