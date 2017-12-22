using System;
using DotGet.Core.Logging;

namespace DotGet.Cli.Logging
{
    class Logger : ILogger
    {
        public LogLevel Level { get; set; } = LogLevel.Error | LogLevel.Info | LogLevel.Success | LogLevel.Warning;

        public void LogError(string data)
        {
            if (Level.HasFlag(LogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(data);
                Console.ResetColor();
            }
        }

        public void LogInformation(string data)
        {
            if (Level.HasFlag(LogLevel.Info))
                Console.WriteLine(data);
        }

        public void LogProgress(string data)
        {
            if (Level.HasFlag(LogLevel.Info))
                Console.Write($"{data}... ");
        }

        public void LogResult(string data)
        {
            if (Level.HasFlag(LogLevel.Info))
                Console.WriteLine($"{data}.");
        }

        public void LogSuccess(string data)
        {
            if (Level.HasFlag(LogLevel.Success))
            {
                Console.ForegroundColor = ConsoleColor.Green;
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
            if (Level.HasFlag(LogLevel.Warning))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(data);
                Console.ResetColor();
            }
        }

        public void AllowVerbose() => Level = Level | LogLevel.Verbose;
    }
}