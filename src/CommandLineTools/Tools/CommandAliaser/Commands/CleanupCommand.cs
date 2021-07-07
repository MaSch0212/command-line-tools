using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.CommandAliaser.Models;
using MaSch.CommandLineTools.Tools.CommandAliaser.Services;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Core.Extensions;
using System;
using System.IO;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Commands
{
    [CliCommand(
        "cleanup",
        HelpText = "Cleans up the command folders. Recreates script files that should exist and removes those that should not.",
        ParentCommand = typeof(CommandAliaserTool))]
    public class CleanupCommand : CommandBase
    {
        private readonly IConsoleService _console;
        private readonly ICommandsService _commandsService;

        public override bool IsScopeExcluse { get; } = false;

        [CliCommandOption('g', "global", HelpText = "Clean up global path.")]
        public override bool Global { get; set; }

        [CliCommandOption('l', "local", HelpText = "Clean up local path.")]
        public override bool Local { get; set; }

        [CliCommandOption('u', "user", HelpText = "Clean up user path.")]
        public override bool User { get; set; }

        public CleanupCommand(IConsoleService console, ICommandsService commandsService)
        {
            _console = console;
            _commandsService = commandsService;
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            bool createSucceess = true, deleteSuccess = true;

            if (!Global && !Local && !User)
                Global = Local = User = true;

            if (Global)
                DoCleanup("Global", GlobalCommandsPath);
            if (Local)
                DoCleanup("List", LocalCommandsPath);
            if (User)
                DoCleanup("User", UserCommandsPath);

            return (int)((createSucceess, deleteSuccess) switch
            {
                (true, true) => ExitCode.Okay,
                (true, false) => ExitCode.AliasCleanupFailedDeleteScript,
                (false, true) => ExitCode.AliasCleanupFailedCreateScript,
                _ => ExitCode.AliasCleanupMultipleErrors,
            });

            void DoCleanup(string scopeName, string path)
            {
                bool cs = true, ds = true;

                _console.WriteLine($"Cleaning up \"{scopeName}\"...");
                var commands = _commandsService.LoadCommands(path);

                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
                {
                    if (Path.GetFileName(file) == CommandsFileName)
                        continue;
                    if (!Path.GetExtension(file).In(".ps1", ".cmd") || !_commandsService.TryGetCommand(commands, Path.GetFileNameWithoutExtension(file), out _))
                    {
                        if (_commandsService.DeleteScriptFile(file))
                            _console.WriteLineWithColor($"Successfully deletes file \"{Path.GetFileName(file)}\".", ConsoleColor.Green);
                        else
                            ds = false;
                    }
                }

                foreach (var command in commands.Values)
                {
                    cs &= EnsureScript(path, command, TerminalTool.PowerShell);
                    cs &= EnsureScript(path, command, TerminalTool.Cmd);
                }

                createSucceess &= cs;
                deleteSuccess &= ds;

                if (cs && ds)
                    _console.WriteLineWithColor($"Successfully cleaned up \"{scopeName}\".", ConsoleColor.Green);
                else
                    _console.WriteLineWithColor($"Partially cleaned up \"{scopeName}\".", ConsoleColor.Yellow);
                _console.WriteLine();
            }

            bool EnsureScript(string path, Command command, TerminalTool tool)
            {
                if (!File.Exists(_commandsService.GetScriptFilePath(path, command.Alias!, tool)))
                {
                    if (_commandsService.WriteScriptFile(path, command.Alias, tool, command.CommandText, command.Description, command.Tool))
                        _console.WriteLineWithColor($"Successfully created \"{tool}\"-script for alias \"{command.Alias}\".", ConsoleColor.Green);
                    else
                        return false;
                }

                return true;
            }
        }
    }
}
