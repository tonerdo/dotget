using System.Collections.Generic;

namespace DotGet.Core.Configuration
{
    internal class SourceInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Directory { get; set; }
        public List<string> Commands { get; set; } = new List<string>();
    }
}