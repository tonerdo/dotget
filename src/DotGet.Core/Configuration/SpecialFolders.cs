using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DotGet.Core.Configuration
{
    public static class SpecialFolders
    {
        static SpecialFolders()
        {
            if(!Directory.Exists(DotGet))
                Directory.CreateDirectory(DotGet);
            
            if(!Directory.Exists(Lib))
                Directory.CreateDirectory(Lib);
            
            if(!Directory.Exists(Bin))
                Directory.CreateDirectory(Bin);
        }

        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static readonly string DotGet
            = Path.Combine(Environment.GetEnvironmentVariable(IsWindows ? "USERPROFILE" : "HOME"), ".dotget");

        public static readonly string Lib = Path.Combine(DotGet, "lib");

        public static readonly string Bin = Path.Combine(DotGet, "bin");
    }
}