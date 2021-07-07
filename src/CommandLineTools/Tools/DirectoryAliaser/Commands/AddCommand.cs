using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    [CliCommand("add", HelpText = "Add new directory aliases.", ParentCommand = typeof(DirectoryAliaserTool))]
    public class AddCommand : CommandBase
    {
        [CliCommandValue(0, "aliases", Required = true, HelpText = "The aliases to add.")]
        public IEnumerable<string>? Aliases { get; set; }

        [CliCommandOption('d', "directory", Required = false, HelpText = "The directory to which the aliases should be added. By default the current directory is used.")]
        public string Directory { get; set; } = Environment.CurrentDirectory;

        public AddCommand(IConsoleService console)
            : base(console)
        {
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            if (Aliases.IsNullOrEmpty())
                throw new ApplicationExitException(ExitCode.CdxAddMissingAlias, "At least one alias needs to be provided.");
            if (string.IsNullOrWhiteSpace(Directory))
                throw new ApplicationExitException(ExitCode.CdxAddMissingDirectory, "A directory needs to be provided.");

            var path = GetPathFromPath(Directory);
            if (path == null)
            {
                path = new PathConfiguration(Directory);
                Config.Paths.Add(path);
            }

            var invalidAliases = Aliases.Where(x => x.Intersect(IllegalAliasCharacters).Any()).ToArray();
            if (invalidAliases.Length > 0)
            {
                Console.WriteLineWithColor($"The following aliases contain illeagl characters: \"{string.Join("\", \"", invalidAliases)}\"", ConsoleColor.Red);
                Console.WriteLineWithColor($"Illegal characters are: {string.Join(" ", IllegalAliasCharacters)}", ConsoleColor.Yellow);
                return (int)ExitCode.CdxAddInvalidAliasName;
            }

            foreach (var alias in Aliases)
            {
                var p = GetPathFromAlias(alias);
                if (p != null)
                {
                    Console.WriteLineWithColor($"The alias \"{alias}\" is already registered with path \"{p.Path}\".", ConsoleColor.Yellow);
                }
                else
                {
                    path.Aliases.Add(alias);
                    ConfigChanged = true;
                    Console.WriteLineWithColor($"The alias \"{alias}\" was registered to path \"{path.Path}\".", ConsoleColor.Green);
                    if (Config.IsAutoVariableEnabled)
                        AddEnvironmentVariable(alias, path.Path);
                }
            }

            return (int)ExitCode.Okay;
        }
    }
}
