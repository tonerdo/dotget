using System.IO;
using System.Linq;

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
    }
}