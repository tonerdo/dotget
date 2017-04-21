using System;

namespace DotGet.Cli.Logging
{
    [Flags]
    enum LogLevel
    {
        Error,
        Info,
        Success,
        Verbose,
        Warning
    }
}