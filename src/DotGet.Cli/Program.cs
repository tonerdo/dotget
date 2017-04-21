using Microsoft.Extensions.CommandLineUtils;

namespace DotGet.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "dotget";
            app.FullName = "DotGet Global Package Installer";
            app.Description = "Install and use command line tools built on .NET Core";
            app.HelpOption("-h|--help");
            app.VersionOption("-v|--version", "1.0.0");

            if (args.Length == 0)
                app.ShowHelp();

            return app.Execute(args);
        }
    }
}
