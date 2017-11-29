using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

namespace DotGet.Core.Commands
{
    public class ListCommand : ICommand
    {
        private ILogger _logger;

        public ListCommand(ILogger logger)
        {
            _logger = logger;
        }

        public bool Execute()
        {
            string bin = Path.Combine(Globals.GlobalNuGetDirectory, "bin");
            string[] files = Directory.GetFiles(bin);

            if (files.Length == 0)
            {
                _logger.LogInformation("No tools installed!");
                return true;
            }

            foreach (var file in files)
            {
                string command = File.ReadAllLines(file).ToList().Last();
                Resolver resolver = new ResolverFactory().GetResolver(command);
                Console.WriteLine(resolver.GetToolName(command) + " => " + Path.GetFileNameWithoutExtension(command));
            }

            return true;
        }
    }
}