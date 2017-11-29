using System.IO;

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
    }
}