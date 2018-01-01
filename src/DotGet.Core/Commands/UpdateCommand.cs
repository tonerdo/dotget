using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class UpdateCommand : ICommand
    {
        private string _source;
        private ILogger _logger;

        public UpdateCommand(string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
        }

        public bool Execute()
        {
            // TODO: Check all source infos for source name
            ResolverFactory resolverFactory = new ResolverFactory(_source, ResolutionType.Update, _logger);
            Resolver resolver = resolverFactory.GetResolver();
            if (resolver == null)
                throw new Exception("No resolver found");

            if (!resolver.Resolve())
                throw new Exception("Failed to resolve source");

            SourceInfo sourceInfo = resolver.GetSourceInfo();
            // TODO: Write {sourceInfo} to {sourceInfo.Name}.info.json
            // in {sourceInfo.Directory} folder

            return true;
        }
    }
}