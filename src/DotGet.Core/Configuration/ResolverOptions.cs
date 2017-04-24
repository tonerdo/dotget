using System.Collections.Generic;

namespace DotGet.Core.Configuration
{
    internal class ResolverOptions : Dictionary<string, string>
    {
        public ResolverOptions() { }
        public ResolverOptions(Dictionary<string, string> options)
        {
            foreach (KeyValuePair<string, string> pair in options)
                this.Add(pair.Key, pair.Value);
        }
    }
}