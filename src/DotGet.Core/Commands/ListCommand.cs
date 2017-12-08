using System;
using System.Collections.Generic;
using System.IO;

using ConsoleTables;
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
                _logger.LogInformation("No .NET Core tool installed!");
                return true;
            }

            ConsoleTable consoleTable = new ConsoleTable("Name", "Full Name", "Command");

            foreach (var file in files)
            {
                string command = CommandHelper.GetCommandFromFile(file);
                string path = CommandHelper.GetPathFromCommand(command);
                Resolver resolver = ResolverFactory.GetResolverForPath(path);
                consoleTable.AddRow(
                    resolver.GetSource(path),
                    resolver.GetFullSource(path),
                    Path.GetFileNameWithoutExtension(command)
                );
            }

            _logger.LogInformation($"{files.Length} tool(s) installed");
            consoleTable.Write(Format.Alternative);

            return true;
        }
    }
}