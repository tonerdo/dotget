using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using DotGet.Core.Helpers;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class UninstallCommand : ICommand
    {
        private string _source;
        private ILogger _logger;

        public UninstallCommand(string source, ILogger logger)
        {
            _source = source;
            _logger = logger;
        }

        public bool Execute()
        {
            string[] files = Directory.GetFiles(Globals.GlobalBinDirectory);
            foreach (var file in files)
            {
                string command = File.ReadAllLines(file).ToList().Last();
                string path = CommandHelper.GetPathFromCommand(command);
                Resolver resolver = ResolverFactory.GetResolverForPath(path);

                if (resolver.GetSource(path) == _source
                    || resolver.GetFullSource(path) == _source)
                {
                    if (resolver.Remove(resolver.GetFullSource(path), _logger))
                    {
                        File.Delete(file);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}