using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Console.Controls;
using MaSch.Console.Controls.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    public abstract class CommandBase : ToolCommandBase
    {
        private static readonly string ConfigFilePath = Path.Combine(Program.ConfigurationDirectory, "cdx.json");

        protected static readonly char[] IllegalAliasCharacters = new[] { '\\', '/' };

        protected Configuration Config { get; private set; }
        protected bool ConfigChanged { get; set; }

        [CliCommandOption("from-script", Hidden = true)]
        public bool WasStartedFromScript { get; set; }

        protected CommandBase()
        {
            Config = new Configuration();
        }

        public override int ExecuteCommand(CliExecutionContext context)
        {
            if (File.Exists(ConfigFilePath))
                Config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigFilePath)) ?? Config;

            var result = base.ExecuteCommand(context);

            if (ConfigChanged)
                File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Config, Formatting.Indented));

            return result;
        }

        protected PathConfiguration? GetPathFromAlias(string alias) => Config.Paths.FirstOrDefault(x => x.Aliases.Contains(alias, StringComparer.OrdinalIgnoreCase));
        protected PathConfiguration? GetPathFromPath(string path) => Config.Paths.FirstOrDefault(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase));

        protected void PrintList()
        {
            if (Config.Paths.Count == 0)
            {
                using var scope = new ConsoleColorScope(Console, ConsoleColor.Yellow);
                Console.WriteLine("There are currently no aliases registered. You can register a new alias by using the 'add' verb.");
                Console.WriteLine("Execute 'cdx add --help' for more information.");
            }
            else
            {
                Console.WriteLine("The following aliases are available:");
                Console.WriteLine();
                new TableControl(Console)
                {
                    Margin = new(3, 0, 0, 0),
                    ShowColumnHeaders = false,
                    Columns =
                    {
                        new() { WidthMode = ColumnWidthMode.Auto, MaxWidth = Console.BufferSize.Width / 3 },
                        new() { WidthMode = ColumnWidthMode.Star },
                    },
                    Rows = Config.Paths.OrderBy(x => x.Path).Select(x => new Row { Values = { string.Join(", ", x.Aliases), x.Path } }).ToArray(),
                }.Render();
                Console.WriteLine();
            }
        }

        protected void AddEnvironmentVariable(string name, string path)
        {
            var varName = $"cdx_{name}";
            Environment.SetEnvironmentVariable(varName, path, EnvironmentVariableTarget.User);
            Console.WriteLineWithColor($"Environment variable \"{varName}\" has been added.", ConsoleColor.Green);
        }

        protected void RemoveEnvironmentVariable(string name)
        {
            var varName = $"cdx_{name}";
            Environment.SetEnvironmentVariable(varName, null, EnvironmentVariableTarget.User);
            Console.WriteLineWithColor($"Environment variable \"{varName}\" has been removed.", ConsoleColor.Green);
        }
    }
}
