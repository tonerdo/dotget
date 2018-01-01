namespace DotGet.Core.Configuration
{
    internal class SourceInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Directory { get; set; }
        public string[] Commands { get; set; }
    }
}