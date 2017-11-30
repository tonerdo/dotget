using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DotGet.Core
{
    internal static class Globals
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static readonly string GlobalNuGetDirectory
            = Path.Combine(Environment.GetEnvironmentVariable(IsWindows ? "USERPROFILE" : "HOME"), ".nuget");

        public static readonly string GlobalBinDirectory
            = Path.Combine(GlobalNuGetDirectory, "bin");
    }
}