using System;
using DotGet.Core.Logging;

namespace DotGet.Cli.Logging
{
    class Logger : ILogger
    {
        public LogLevel Level { get; set; }

        public void LogError(string data)
        {
            if (Level.HasFlag(LogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(data);
                Console.ResetColor();
            }
        }

        public void LogInformation(string data)
        {
            if (Level.HasFlag(LogLevel.Info))
                Console.WriteLine(data);
        }

        public void LogSuccess(string data)
        {
            if (Level.HasFlag(LogLevel.Success))
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(data);
                Console.ResetColor();
            }
        }

        public void LogVerbose(string data)
        {
            if (Level.HasFlag(LogLevel.Verbose))
                Console.WriteLine(data);
        }

        public void LogWarning(string data)
        {
            if (Level.HasFlag(LogLevel.Success))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(data);
                Console.ResetColor();
            }
        }
    }
}