using System.Diagnostics;
using System.IO;
using System.Linq;

using DotGet.Core.Resolvers;

namespace DotGet.Core.Helpers
{
    internal static class CommandHelper
    {
        public static string BuildBinContents(string path)
        {
            if (Globals.IsWindows)
                return $"dotnet {path} %*";

            return $"#!/usr/bin/env bash \ndotnet {path} \"$@\"";
        }

        public static string BuildBinFilename(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            return Globals.IsWindows ? filename + ".cmd" : filename;
        }

        public static string GetCommandFromFile(string file)
            => File.ReadAllLines(file).ToList().Last();

        public static string GetPathFromCommand(string command)
        {
            string[] parts = command.Split(' ');
            string path = parts[1];
            if (parts.Length > 3)
                path = string.Join(string.Empty, parts.ToList().GetRange(1, parts.Length - 2));

            if (Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar)
                path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return path;
        }

        public static bool MakeUnixExecutable(string filename)
        {
            Process process = new Process();
            process.StartInfo.FileName = "chmod";
            process.StartInfo.Arguments = $"+x {filename}";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        public static bool IsInstalled(string source)
        {
            string[] files = Directory.GetFiles(Globals.GlobalBinDirectory);
            foreach (var file in files)
            {
                string command = CommandHelper.GetCommandFromFile(file);
                string path = CommandHelper.GetPathFromCommand(command);
                Resolver resolver = ResolverFactory.GetResolverForPath(path);
                if (resolver.GetSource(path) == source
                    || resolver.GetFullSource(path) == source)
                    return true;
            }

            return false;
        }
    }
}