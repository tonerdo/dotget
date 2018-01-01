using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;

namespace DotGet.Core.Resolvers
{
    internal class NuGetPackageResolver : Resolver
    {
        public NuGetPackageResolver(string source, ResolutionType resolutionType, ILogger logger) : base(source, resolutionType, logger)
        {
        }

        public override ResolverOptions BuildOptions()
        {
            throw new NotImplementedException();
        }

        public override bool CanResolve()
        {
            throw new NotImplementedException();
        }

        public override bool CheckInstalled()
        {
            throw new NotImplementedException();
        }

        public override SourceInfo GetSourceInfo()
        {
            throw new NotImplementedException();
        }

        public override bool Remove()
        {
            throw new NotImplementedException();
        }

        public override bool Resolve()
        {
            throw new NotImplementedException();
        }
    }
}