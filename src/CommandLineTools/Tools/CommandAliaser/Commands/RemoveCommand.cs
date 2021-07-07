using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.CommandAliaser.Services;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Commands
{
    [CliCommand("remove", HelpText = "Remove existing command alias.", ParentCommand = typeof(CommandAliaserTool))]
    public class RemoveCommand : CommandBase
    {
        private readonly IConsoleService _console;
        private readonly ICommandsService _commandsService;

        public override bool IsScopeExcluse { get; } = false;

        [CliCommandValue(0, "alias", Required = true, HelpText = "The alias to remove.")]
        public string? Alias { get; set; }

        [CliCommandOption('g', "global", HelpText = "Remove from global commands.")]
        public override bool Global { get; set; }

        [CliCommandOption('l', "local", HelpText = "Remove from local commands.")]
        public override bool Local { get; set; }

        [CliCommandOption('u', "user", HelpText = "Remove from user commands.")]
        public override bool User { get; set; }

        [CliCommandOption('f', "force", HelpText = "Forces the deletion of the alias.")]
        public bool Force { get; set; }

        public RemoveCommand(IConsoleService console, ICommandsService commandsService)
        {
            _console = console;
            _commandsService = commandsService;
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            if (!Global && !Local && !User)
                Global = Local = User = true;

            int removeCount = 0;
            bool completeSuccess = true;
            if (Global)
                completeSuccess &= RemoveCommand("Global", GlobalCommandsPath);
            if (Local)
                completeSuccess &= RemoveCommand("Local", LocalCommandsPath);
            if (User)
                completeSuccess &= RemoveCommand("User", UserCommandsPath);

            if (removeCount == 0)
            {
                _console.WriteLineWithColor($"The alias \"{Alias}\" does not exist in any scope.", ConsoleColor.Red);
                return (int)ExitCode.AliasRemoveNotExists;
            }

            return (int)(completeSuccess ? ExitCode.Okay : ExitCode.AliasRemoveFailedDeleteScript);

            bool RemoveCommand(string scopeName, string path)
            {
                var commands = _commandsService.LoadCommands(path);
                if (_commandsService.TryGetCommand(commands, Alias, out var command))
                {
                    commands.Remove(command.Alias!);

                    if (!_commandsService.SaveCommands(path, commands))
                        throw new ApplicationExitException(ExitCode.AliasRemoveFailedModifyJson);

                    bool success = true;
                    success &= _commandsService.DeleteScriptFile(path, command.Alias!, TerminalTool.PowerShell);
                    success &= _commandsService.DeleteScriptFile(path, command.Alias!, TerminalTool.Cmd);

                    removeCount++;
                    if (success)
                        _console.WriteLineWithColor($"Successfully removed command \"{command.Alias}\" from scope \"{scopeName}\".", ConsoleColor.Green);
                    else
                        _console.WriteLineWithColor($"Partially removed command \"{command.Alias}\" from scope \"{scopeName}\".", ConsoleColor.Yellow);
                    return success;
                }

                _console.WriteLineWithColor($"The alias \"{Alias}\" does not exist in the scope \"{scopeName}\".", ConsoleColor.Yellow);
                return true;
            }
        }
    }
}
