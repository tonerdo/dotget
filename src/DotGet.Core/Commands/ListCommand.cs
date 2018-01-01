using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ConsoleTables;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

using Newtonsoft.Json;

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
            List<SourceInfo> sourceInfos = new List<SourceInfo>();
            ConsoleTable consoleTable = new ConsoleTable("Name", "Full Name", "Command(s)");
            DirectoryInfo[] directories = new DirectoryInfo(SpecialFolders.Lib).GetDirectories();

            foreach (var directory in directories)
            {
                FileInfo fileInfo = directory.GetFiles("*.info.json", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (fileInfo != null)
                {
                    string json = File.ReadAllText(fileInfo.FullName);
                    sourceInfos.Add(JsonConvert.DeserializeObject<SourceInfo>(json));
                }
            }

            foreach (var sourceInfo in sourceInfos)
            {
                consoleTable.AddRow(
                    sourceInfo.Name,
                    sourceInfo.FullName,
                    string.Join(", ", sourceInfo.Commands)
                );
            }

            consoleTable.Write(Format.Alternative);
            return true;
        }
    }
}