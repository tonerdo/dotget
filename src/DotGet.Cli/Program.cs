using DotGet.Cli.Logging;
using DotGet.Core.Commands;
using DotGet.Core.Configuration;
using Microsoft.Extensions.CommandLineUtils;

namespace DotGet.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            Logger logger = new Logger() { Level = LogLevel.Error | LogLevel.Info | LogLevel.Success | LogLevel.Warning };
            var app = new CommandLineApplication();
            app.Name = "dotget";
            app.FullName = ".NET Core Tools Global Installer";
            app.Description = "Install and use command line tools built on .NET Core";
            app.HelpOption("-h|--help");
            app.VersionOption("-v|--version", "1.0.0");

            CommandOption verboseOption = app.Option("--verbose", "Enable verbose output", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            app.Command("install", c =>
            {
                c.Description = "Installs a .NET Core tool";
                c.HelpOption("-h|--help");

                CommandArgument toolArg = c.Argument("<TOOL>", "The tool to install. Can be a NuGet package");

                c.OnExecute(() =>
                {
                    UpdateLoggerIfVerbose(verboseOption, logger);
                    CommandOptions installOptions = new CommandOptions();
                    InstallCommand installCommand = new InstallCommand(toolArg.Value, installOptions, logger);
                    installCommand.Execute();
                    return 0;
                });
            });

            return app.Execute(args);
        }

        static void UpdateLoggerIfVerbose(CommandOption verboseOption, Logger logger)
        {
            if (verboseOption.HasValue())
                logger.Level = logger.Level | LogLevel.Verbose;
        }
    }
}
