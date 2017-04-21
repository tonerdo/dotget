using System;

namespace DotGet.Cli.Logging
{
    [Flags]
    enum LogLevel
    {
        Error = 1,
        Info = 2,
        Success = 4,
        Verbose = 8,
        Warning = 16
    }
}