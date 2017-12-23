using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using DotGet.Core.Exceptions;
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
            _logger.LogProgress($"Checking if {_source} is already installed");
            if (!CommandHelper.IsInstalled(_source))
            {
                _logger.LogResult("fail");
                return false;
            }

            _logger.LogResult("ok");

            string[] files = Directory.GetFiles(Globals.GlobalBinDirectory);
            foreach (var file in files)
            {
                string command = CommandHelper.GetCommandFromFile(file);
                string path = CommandHelper.GetPathFromCommand(command);
                Resolver resolver = ResolverFactory.GetResolverForPath(path);

                if (resolver.GetSource(path) == _source
                    || resolver.GetFullSource(path) == _source)
                {
                    try
                    {
                        _logger.LogProgress($"Removing {_source}");
                        if (resolver.Remove(resolver.GetFullSource(path), _logger))
                        {
                            _logger.LogResult("done");
                            _logger.LogProgress($"Deleting executable for {_source}");
                            File.Delete(file);
                            return true;
                        }
                        else
                        {
                            _logger.LogResult("fail");
                            return false;
                        }
                    }
                    catch (ResolverException ex)
                    {
                        _logger.LogResult("fail");
                        _logger.LogError(ex.Message);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogResult("fail");
                        _logger.LogVerbose(ex.ToString());
                        return false;
                    }

                }
            }

            return false;
        }
    }
}