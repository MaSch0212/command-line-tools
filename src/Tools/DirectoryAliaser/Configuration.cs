using MaSch.Core.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser
{
    public class Configuration
    {
        public List<PathConfiguration> Paths { get; set; }
        public bool IsAutoVariableEnabled { get; set; }

        public Configuration()
        {
            Paths = new List<PathConfiguration>();
        }

        [JsonConstructor]
        public Configuration(List<PathConfiguration?>? paths)
        {
            Paths = paths?.Where(x => x?.Normalize() == true).Select(x => x!).ToList() ?? new List<PathConfiguration>();
        }
    }

    public class PathConfiguration
    {
        public string Path { get; set; }
        public List<string> Aliases { get; set; }

        public PathConfiguration(string path)
        {
            Path = path;
            Aliases = new List<string>();
        }

        [JsonConstructor]
        public PathConfiguration(string path, List<string?>? aliases)
        {
            Path = path;
            Aliases = aliases?.Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).ToList() ?? new List<string>();
        }

        public bool Normalize()
        {
            if (string.IsNullOrWhiteSpace(Path) || Aliases.IsNullOrEmpty())
                return false;
            Aliases.RemoveWhere(x => string.IsNullOrEmpty(x));
            return Aliases.Count > 0;
        }
    }
}
