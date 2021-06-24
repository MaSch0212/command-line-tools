using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Core.Extensions;
using System;
using System.Collections.Generic;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    [CliCommand("remove", HelpText = "Remove existing directory aliases", ParentCommand = typeof(DirectoryAliaserTool))]
    public class RemoveCommand : CommandBase
    {
        [CliCommandValue(0, "aliases", Required = false, HelpText = "The aliases to remove. If none are given, all aliases for the given directory are removed.")]
        public IEnumerable<string>? Aliases { get; set; }

        [CliCommandOption('d', "directory", Required = false, HelpText = "The directory from which the aliases should be removed. By default the current directory is used.")]
        public string? Directory { get; set; }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            PathConfiguration? path = null;
            if (!string.IsNullOrWhiteSpace(Directory) || Aliases.IsNullOrEmpty())
            {
                path = GetPathFromPath(Directory ?? Environment.CurrentDirectory);
                if (path == null)
                {
                    Console.WriteLineWithColor($"The path \"{Directory}\" does not have any registered aliases.", ConsoleColor.Yellow);
                    return (int)ExitCode.Okay;
                }
            }

            if (Aliases.IsNullOrEmpty())
            {
                Config.Paths.Remove(path!);
                ConfigChanged = true;
                Console.WriteLineWithColor($"All aliases have been removed from \"{path!.Path}\".", ConsoleColor.Green);
            }
            else
            {
                foreach (var alias in Aliases)
                {
                    var p = path ?? GetPathFromAlias(alias);
                    var aliasIndex = p?.Aliases.IndexOf(x => string.Equals(x, alias, StringComparison.OrdinalIgnoreCase));
                    if (p == null || !aliasIndex.HasValue)
                    {
                        Console.WriteLineWithColor($"The alias \"{alias}\" is not reigstered to {(string.IsNullOrWhiteSpace(Directory) ? "any paths" : $"path \"{Directory}\"")}.", ConsoleColor.Yellow);
                    }
                    else
                    {
                        var realAlias = p.Aliases[aliasIndex.Value];
                        p.Aliases.RemoveAt(aliasIndex.Value);
                        ConfigChanged = true;
                        Console.WriteLineWithColor($"The alias \"{alias}\" was removed from path \"{p.Path}\".", ConsoleColor.Green);
                        if (Config.IsAutoVariableEnabled)
                            RemoveEnvironmentVariable(realAlias);
                    }
                }
            }

            return (int)ExitCode.Okay;
        }
    }
}
