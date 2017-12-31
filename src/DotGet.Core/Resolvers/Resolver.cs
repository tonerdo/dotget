using System;
using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal abstract class Resolver
    {
        public abstract bool CanResolve();
        public abstract bool CheckInstalled();
        public abstract bool Resolve();
        public abstract bool Remove();
    }
}