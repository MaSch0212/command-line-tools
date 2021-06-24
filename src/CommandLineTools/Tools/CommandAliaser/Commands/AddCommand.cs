using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.CommandLineTools.Tools.CommandAliaser.Models;
using MaSch.CommandLineTools.Tools.CommandAliaser.Utilities;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Console.Controls;
using System;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Commands
{
    [CliCommand("add", HelpText = "Add new command alias.", ParentCommand = typeof(CommandAliaserTool))]
    public class AddCommand : CommandBase
    {
        public override bool IsScopeExcluse { get; } = true;

        [CliCommandValue(0, "alias", Required = true, HelpText = "The alias to add.")]
        public string? Alias { get; set; }

        [CliCommandValue(1, "command", Required = false, HelpText = "The command to execute for the alias.")]
        public string? Command { get; set; }

        [CliCommandOption('d', "description", HelpText = "The Description for the command (shown when listing all commands).")]
        public string? Description { get; set; }

        [CliCommandOption('g', "global", HelpText = "Add the command as a global command.")]
        public override bool Global { get; set; }

        [CliCommandOption('l', "local", HelpText = "Add the command as a local command.")]
        public override bool Local { get; set; }

        [CliCommandOption('u', "user", HelpText = "Add the command as a user command.")]
        public override bool User { get; set; }

        [CliCommandOption('t', "tool", HelpText = "The tool to use for this command. If not provided it is assumed that the command can be executed in all tools.")]
        public string? Tool { get; set; }

        [CliCommandOption('f', "force", HelpText = "Forces the creation of the alias even if it already exists.")]
        public bool Force { get; set; }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            if (Description == null)
            {
                Console.Write("Description: ");
                Description = Console.ReadLine();
            }

            if (!Local && !Global && !User)
            {
                var result = SelectControl.Show(Console, SelectControl.OneSelectionMode.LeftRight, "Command Scope: ", 1, "User", "Local", "Global");
                User = result.Index == 0;
                Local = result.Index == 1;
                Global = result.Index == 2;
            }

            var path = User ? UserCommandsPath : Local ? LocalCommandsPath : GlobalCommandsPath;
            var scopeName = User ? UserScopeName : Local ? LocalScopeName : GlobalScopeName;
            var commands = CommandsUtility.LoadCommands(path);

            if (commands.TryGetCommand(Alias, out var command))
            {
                if (!Force)
                    throw new ApplicationExitException(ExitCode.AliasAddExists, $"The alias \"{Alias}\" already exists for scope \"{scopeName}\". Use the --force parameter to override the existing alias.");
                commands.Remove(command.Alias!);
            }

            while (string.IsNullOrWhiteSpace(Command))
            {
                Console.Write("Command: ");
                Command = Console.ReadLine();
            }

            var tool = Tool?.ToTool();
            commands.Add(Alias!, new Command
            {
                Description = Description,
                Tool = tool,
                CommandText = Command,
            });

            if (!CommandsUtility.SaveCommands(path, commands))
                return (int)ExitCode.AliasAddFailedModifyJson;

            var success = true;
            success &= CommandsUtility.WriteScriptFile(path, Alias!, TerminalTool.PowerShell, Command, Description, tool);
            success &= CommandsUtility.WriteScriptFile(path, Alias!, TerminalTool.Cmd, Command, Description, tool);

            if (success)
                Console.WriteLineWithColor($"Successfully added alias \"{Alias}\" to scope \"{scopeName}\".", ConsoleColor.Green);
            else
                Console.WriteLineWithColor($"Partially added alias \"{Alias}\" to scope \"{scopeName}\".", ConsoleColor.Yellow);

            return (int)(success ? ExitCode.Okay : ExitCode.AliasAddFailedCreateScript);
        }
    }
}
