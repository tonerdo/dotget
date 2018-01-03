using System;
using DotGet.Core.Logging;

namespace DotGet.Cli.Logging
{
    class Logger : ILogger
    {
        public LogLevel Level { get; set; } = LogLevel.Error | LogLevel.Info | LogLevel.Success | LogLevel.Warning;

        public void LogError(string message)
        {
            if (Level.HasFlag(LogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public void LogInformation(string message)
        {
            if (Level.HasFlag(LogLevel.Info))
                Console.WriteLine(message);
        }

        public void LogSuccess(string message)
        {
            if (Level.HasFlag(LogLevel.Success))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public void LogVerbose(string message)
        {
            if (Level.HasFlag(LogLevel.Verbose))
                Console.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            if (Level.HasFlag(LogLevel.Warning))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public void AllowVerbose() => Level = Level | LogLevel.Verbose;
    }
}