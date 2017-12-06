using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGet.Core.Helpers;
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
            string[] files = Directory.GetFiles(Globals.GlobalBinDirectory);

            if (files.Length == 0)
            {
                _logger.LogInformation("No .NET Core tools installed!");
                return true;
            }

            Console.WriteLine("Globally installed .NET Core Tools");
            Console.WriteLine("==================================");

            foreach (var file in files)
            {
                string command = File.ReadAllLines(file).ToList().Last();
                string path = CommandHelper.GetPathFromCommand(command);
                Resolver resolver = new ResolverFactory().GetResolver(path);
                Console.WriteLine(resolver.GetFullSource(path) + " => " + Path.GetFileNameWithoutExtension(command));
            }

            return true;
        }
    }
}