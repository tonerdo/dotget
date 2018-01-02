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
            return true;
        }
    }
}