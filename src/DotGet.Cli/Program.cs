using DotGet.Cli.Logging;
using DotGet.Core.Commands;
using Microsoft.Extensions.CommandLineUtils;

namespace DotGet.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            Logger logger = new Logger();
            var app = new CommandLineApplication();
            app.Name = "dotget";
            app.FullName = ".NET Core Tools Global Installer";
            app.Description = "Install and use command line tools built on .NET Core";
            app.HelpOption("-h|--help");
            app.VersionOption("-v|--version", GetAssemblyVersion());

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

                CommandArgument source = c.Argument("<SOURCE>", "The tool to install. Can be a NuGet package");

                c.OnExecute(() =>
                {
                    logger = new Logger(verboseOption.HasValue());
                    if (string.IsNullOrWhiteSpace(source.Value))
                    {
                        logger.LogError("<SOURCE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    InstallCommand installCommand = new InstallCommand(source.Value, logger);
                    return installCommand.Execute() ? 0 : 1;
                });
            });

            app.Command("update", c =>
            {
                c.Description = "Updates a .NET Core tool";
                c.HelpOption("-h|--help");

                CommandArgument source = c.Argument("<SOURCE>", "The tool to update.");

                c.OnExecute(() =>
                {
                    logger = new Logger(verboseOption.HasValue());
                    if (string.IsNullOrWhiteSpace(source.Value))
                    {
                        logger.LogError("<SOURCE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    UpdateCommand updateCommand = new UpdateCommand(source.Value, logger);
                    return updateCommand.Execute() ? 0 : 1;
                });
            });

            app.Command("list", c =>
            {
                c.Description = "Lists all installed .NET Core tools";
                c.HelpOption("-h|--help");

                c.OnExecute(() =>
                {
                    logger = new Logger(verboseOption.HasValue());
                    ListCommand listCommand = new ListCommand(logger);
                    return listCommand.Execute() ? 0 : 1;
                });
            });

            app.Command("uninstall", c =>
            {
                c.Description = "Uninstalls a .NET Core tool";
                c.HelpOption("-h|--help");

                CommandArgument source = c.Argument("<SOURCE>", "The tool to uninstall.");

                c.OnExecute(() =>
                {
                    logger = new Logger(verboseOption.HasValue());
                    if (string.IsNullOrWhiteSpace(source.Value))
                    {
                        logger.LogError("<SOURCE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    UninstallCommand uninstallCommand = new UninstallCommand(source.Value, logger);
                    return uninstallCommand.Execute() ? 0 : 1;
                });
            });

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                logger.LogWarning(ex.Message);
                app.ShowHelp();
                return 1;
            }
        }

        static string GetAssemblyVersion() => typeof(Program).Assembly.GetName().Version.ToString();
    }
}
