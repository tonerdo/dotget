using DotGet.Core.Commands;
using DotGet.Core.Configuration;
using Microsoft.Extensions.CommandLineUtils;

namespace DotGet.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "dotget";
            app.FullName = ".NET Core Tools Global Installer";
            app.Description = "Install and use command line tools built on .NET Core";
            app.HelpOption("-h|--help");
            app.VersionOption("-v|--version", "1.0.0");

            app.Command("install", c => {
                c.Description = "Installs a .NET Core tool";
                c.HelpOption("-h|--help");

                CommandArgument toolArg = c.Argument("<TOOL>", "The tool to install. Can be a NuGet package");

                c.OnExecute(() => {
                    CommandOptions installOptions = new CommandOptions();
                    InstallCommand installCommand = new InstallCommand(toolArg.Value, installOptions);
                    installCommand.Execute();
                    return 0;
                });
            });

            if (args.Length == 0)
                app.ShowHelp();

            return app.Execute(args);
        }
    }
}
