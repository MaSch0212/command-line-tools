using MaSch.CommandLineTools.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Models
{
    public class Command
    {
        [JsonIgnore]
        public string? Alias { get; set; }

        public string? Description { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Tool? Tool { get; set; }

        public string? CommandText { get; set; }
    }
}
