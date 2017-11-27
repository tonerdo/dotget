using System;
using System.Collections.Generic;
using System.IO;

using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class InstallCommand : ICommand
    {
        private string _tool;
        private ILogger _logger;

        internal ResolutionType ResolutionType;

        public InstallCommand(string tool, ILogger logger)
        {
            _tool = tool;
            _logger = logger;
            ResolutionType = ResolutionType.Install;
        }

        private string BuildBinContents(string path)
        {
            if (Globals.IsWindows)
                return $"dotnet {path} %*";

            return $"#!/usr/bin/env bash \ndotnet {path} \"$@\"";
        }

        private string BuildBinFilename(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            return Globals.IsWindows ? filename + ".cmd" : filename;
        }

        public bool Execute()
        {
            Resolver resolver = new ResolverFactory(_tool, this.ResolutionType, _logger).GetResolver();
            string path = string.Empty;

            try
            {
                path = resolver.Resolve();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }

            string bin = Path.Combine(Globals.GlobalNuGetDirectory, "bin");
            if (!Directory.Exists(bin))
                Directory.CreateDirectory(bin);

            string filename = Path.Combine(bin, BuildBinFilename(path));

            if (File.Exists(filename))
            {
                _logger.LogError($"{_tool} is already installed. Try updating it instead!");
                return false;
            }

            File.WriteAllText(filename, BuildBinContents(path));
            // TODO: make unix bin file executable
            return true;
        }
    }
}