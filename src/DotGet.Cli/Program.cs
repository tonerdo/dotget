using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;

using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json.Linq;

using DotGet.Cli.Logging;
using DotGet.Core.Commands;
using DotGet.Core.Configuration;
using System.Diagnostics;

namespace DotGet.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            Logger logger = new Logger();
            var app = new CommandLineApplication();
            app.Name = "dotnet get";
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
                        return Update(logger) ? 0 : 1;

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

        static bool Update(Logger logger)
        {
            logger.LogInformation("Updating dotGet");
            WebClient webClient = new WebClient();
            webClient.Headers.Add("User-Agent", ".NET Core");
            string json = null;
            dynamic jObject = null;

            try
            {
                json = webClient.DownloadString("https://api.github.com/repos/tonerdo/dotget/releases/latest");
                jObject = JObject.Parse(json);
            }
            catch (System.Exception ex)
            {
                logger.LogVerbose(ex.ToString());
                logger.LogError($"dotGet failed to update.");
                return false;
            }

            MemoryStream memoryStream = null;
            string version = GetAssemblyVersion();
            string tagName = jObject.tag_name;
            string url = $"https://github.com/tonerdo/dotget/releases/download/{tagName}/dotget.{tagName}.zip";

            if ($"v{version}".StartsWith(tagName))
            {
                logger.LogSuccess($"dotGet ({version}) is already up to date.");
                return true;
            }

            try
            {
                memoryStream = new MemoryStream(webClient.DownloadData(url));
            }
            catch (System.Exception ex)
            {
                logger.LogVerbose(ex.ToString());
                logger.LogError($"dotGet failed to update.");
                return false;
            }

            new ZipArchive(memoryStream).ExtractToDirectory(SpecialFolders.DotGet, true);
            string distFolder = Path.Combine(SpecialFolders.DotGet, "dist");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                File.Copy(Path.Combine(distFolder, "Runners", "dotnet-get.cmd"), Path.Combine(SpecialFolders.Bin, "dotnet-get.cmd"), true);
            else
            {
                File.Copy(Path.Combine(distFolder, "Runners", "dotnet-get.sh"), Path.Combine(SpecialFolders.Bin, "dotnet-get"), true);
                Process process = new Process();
                process.StartInfo.FileName = "chmod";
                process.StartInfo.Arguments = $"+x {Path.Combine(SpecialFolders.Bin, "dotnet-get")}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }

            logger.LogSuccess($"dotGet was updated successfully!");
            return true;
        }
    }
}
