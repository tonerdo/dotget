using System.Collections.Generic;

namespace DotGet.Core.Configuration
{
    public class CommandOptions : Dictionary<string, string>
    {
        public CommandOptions() { }
        public CommandOptions(Dictionary<string, string> options)
        {
            foreach (KeyValuePair<string, string> pair in options)
                this.Add(pair.Key, pair.Value);
        }
    }
}