using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGet.Core.Configuration;
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
            ResolverFactory resolverFactory = new ResolverFactory(string.Empty, ResolutionType.None, _logger);
            Resolver[] resolvers = resolverFactory.GetAllResolvers();

            int count = 0;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Installed tools:");
            stringBuilder.AppendLine();

            foreach (var resolver in resolvers)
            {
                foreach (var tool in resolver.GetInstalled())
                {
                    stringBuilder.AppendLine(string.Empty.PadLeft(4) + tool);
                    count++;
                }
            }

            if (count == 0)
                _logger.LogInformation("No .NET Core tool installed");
            else
                _logger.LogInformation(stringBuilder.ToString());

            return true;
        }
    }
}