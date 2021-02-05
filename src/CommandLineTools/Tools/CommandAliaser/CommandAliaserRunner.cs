using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.CommandLineTools.Tools.CommandAliaser.Models;
using MaSch.CommandLineTools.Tools.CommandAliaser.Utilities;
using MaSch.Console;
using MaSch.Console.Controls;
using MaSch.Core.Extensions;
using static MaSch.CommandLineTools.Tools.CommandAliaser.Constants;

namespace MaSch.CommandLineTools.Tools.CommandAliaser
{
    [Verb("alias", HelpText = "Manages command aliases. Aliases to execute commands can be added, removed and listed.")]
    public class CommandAliaserRunner : BaseToolRunnerWithArgsParser
    {
        protected override void OnConfigure(RunnerOptions options)
        {
            options.Author = "Marc Schmidt";
            options.Year = 2020;
            options.DisplayName = "Command Aliaser";
            options.Version = new Version(1, 0, 1);

            options.AddOptionHandler<ListOptions>(HandleList);
            options.AddOptionHandler<AddOptions>(HandleAdd);
            options.AddOptionHandler<RemoveOptions>(HandleRemove);
            options.AddOptionHandler<InstallOptions>(HandleInstall);
            options.AddOptionHandler<UninstallOptions>(HandleUninstall);
            options.AddOptionHandler<CleanupOptions>(HandleCleanup);
        }

        protected override void OnHandleExitCodes()
        {
            WriteCommonExitCodes();
            WriteExitCodeList("List (Default)", ExitCode.AliasList);
            WriteExitCodeList("Add", ExitCode.AliasAdd);
            WriteExitCodeList("Remove", ExitCode.AliasRemove);
            WriteExitCodeList("Install", ExitCode.AliasInstall);
            WriteExitCodeList("Uninstall", ExitCode.AliasUninstall);
            WriteExitCodeList("Cleanup", ExitCode.AliasCleanup);
        }

        private ExitCode HandleList(ListOptions options)
        {
            var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';').ToList() ?? new List<string>();

            if (!options.Global && !options.Local && !options.User)
                options.Global = options.Local = options.User = true;

            Console.WriteLine("The following aliases are available:");
            Console.WriteLine();

            if (options.Global)
                PrintList("Global", GlobalCommandsPath);
            if (options.Local)
                PrintList("List", LocalCommandsPath);
            if (options.User)
                PrintList("User", UserCommandsPath);

            return ExitCode.Okay;

            void PrintList(string scopeName, string path)
            {
                bool isInstalled = paths.Contains(path, StringComparer.OrdinalIgnoreCase);
                Console.Write($"{scopeName} (");
                Console.WriteWithColor(isInstalled ? "Installed" : "Not Installed", isInstalled ? ConsoleColor.Green : ConsoleColor.Red);
                Console.WriteLine(")");

                var commands = CommandsUtility.LoadCommands(path);
                if (commands.Count > 0)
                    Console.WriteLine(commands.ToColumnsString(x => x.Key, x => x.Value.Description));
                else
                    Console.WriteLine("  There are currently no aliases registered in this scope.");
                Console.WriteLine();
            }
        }

        private ExitCode HandleAdd(AddOptions options)
        {
            if (options.Description == null)
            {
                Console.Write("Description: ");
                options.Description = Console.ReadLine();
            }

            if (!options.Local && !options.Global && !options.User)
            {
                var result = SelectControl.Show(Console, SelectControl.OneSelectionMode.LeftRight, "Command Scope: ", 1, "User", "Local", "Global");
                options.User = result.Index == 0;
                options.Local = result.Index == 1;
                options.Global = result.Index == 2;
            }

            var path = options.User ? UserCommandsPath : options.Local ? LocalCommandsPath : GlobalCommandsPath;
            var scopeName = options.User ? UserScopeName : options.Local ? LocalScopeName : GlobalScopeName;
            var commands = CommandsUtility.LoadCommands(path);

            if (commands.TryGetCommand(options.Alias, out var command))
            {
                if (!options.Force)
                    throw new ApplicationExitException(ExitCode.AliasAddExists, $"The alias \"{options.Alias}\" already exists for scope \"{scopeName}\". Use the --force parameter to override the existing alias.");
                commands.Remove(command.Alias!);
            }

            while (string.IsNullOrWhiteSpace(options.Command))
            {
                Console.Write("Command: ");
                options.Command = Console.ReadLine();
            }

            var tool = options.Tool?.ToTool();
            commands.Add(options.Alias!, new Command
            {
                Description = options.Description,
                Tool = tool,
                CommandText = options.Command,
            });

            if (!CommandsUtility.SaveCommands(path, commands))
                return ExitCode.AliasAddFailedModifyJson;

            var success = true;
            success &= CommandsUtility.WriteScriptFile(path, options.Alias!, Tool.PowerShell, options.Command, options.Description, tool);
            success &= CommandsUtility.WriteScriptFile(path, options.Alias!, Tool.Cmd, options.Command, options.Description, tool);

            if (success)
                Console.WriteLineWithColor($"Successfully added alias \"{options.Alias}\" to scope \"{scopeName}\".", ConsoleColor.Green);
            else
                Console.WriteLineWithColor($"Partially added alias \"{options.Alias}\" to scope \"{scopeName}\".", ConsoleColor.Yellow);

            return success ? ExitCode.Okay : ExitCode.AliasAddFailedCreateScript;
        }

        private ExitCode HandleRemove(RemoveOptions options)
        {
            if (!options.Global && !options.Local && !options.User)
                options.Global = options.Local = options.User = true;

            int removeCount = 0;
            bool completeSuccess = true;
            if (options.Global)
                completeSuccess &= RemoveCommand("Global", GlobalCommandsPath);
            if (options.Local)
                completeSuccess &= RemoveCommand("Local", LocalCommandsPath);
            if (options.User)
                completeSuccess &= RemoveCommand("User", UserCommandsPath);

            if (removeCount == 0)
            {
                Console.WriteLineWithColor($"The alias \"{options.Alias}\" does not exist in any scope.", ConsoleColor.Red);
                return ExitCode.AliasRemoveNotExists;
            }

            return completeSuccess ? ExitCode.Okay : ExitCode.AliasRemoveFailedDeleteScript;

            bool RemoveCommand(string scopeName, string path)
            {
                var commands = CommandsUtility.LoadCommands(path);
                if (commands.TryGetCommand(options.Alias, out var command))
                {
                    commands.Remove(command.Alias!);

                    if (!CommandsUtility.SaveCommands(path, commands))
                        throw new ApplicationExitException(ExitCode.AliasRemoveFailedModifyJson);

                    bool success = true;
                    success &= CommandsUtility.DeleteScriptFile(path, command.Alias!, Tool.PowerShell);
                    success &= CommandsUtility.DeleteScriptFile(path, command.Alias!, Tool.Cmd);

                    removeCount++;
                    if (success)
                        Console.WriteLineWithColor($"Successfully removed command \"{command.Alias}\" from scope \"{scopeName}\".", ConsoleColor.Green);
                    else
                        Console.WriteLineWithColor($"Partially removed command \"{command.Alias}\" from scope \"{scopeName}\".", ConsoleColor.Yellow);
                    return success;
                }

                Console.WriteLineWithColor($"The alias \"{options.Alias}\" does not exist in the scope \"{scopeName}\".", ConsoleColor.Yellow);
                return true;
            }
        }

        private ExitCode HandleInstall(InstallOptions options) => InstallUtility.Install(Console, options);
        private ExitCode HandleUninstall(UninstallOptions options) => InstallUtility.Uninstall(Console, options);

        private ExitCode HandleCleanup(CleanupOptions options)
        {
            bool createSucceess = true, deleteSuccess = true;

            if (!options.Global && !options.Local && !options.User)
                options.Global = options.Local = options.User = true;

            if (options.Global)
                DoCleanup("Global", GlobalCommandsPath);
            if (options.Local)
                DoCleanup("List", LocalCommandsPath);
            if (options.User)
                DoCleanup("User", UserCommandsPath);

            return (createSucceess, deleteSuccess) switch
            {
                (true, true) => ExitCode.Okay,
                (true, false) => ExitCode.AliasCleanupFailedDeleteScript,
                (false, true) => ExitCode.AliasCleanupFailedCreateScript,
                _ => ExitCode.AliasCleanupMultipleErrors
            };

            void DoCleanup(string scopeName, string path)
            {
                bool cs = true, ds = true;

                Console.WriteLine($"Cleaning up \"{scopeName}\"...");
                var commands = CommandsUtility.LoadCommands(path);

                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
                {
                    if (Path.GetFileName(file) == CommandsFileName)
                        continue;
                    if (!Path.GetExtension(file).In(".ps1", ".cmd") || !commands.TryGetCommand(Path.GetFileNameWithoutExtension(file), out _))
                    {
                        if (CommandsUtility.DeleteScriptFile(file))
                            Console.WriteLineWithColor($"Successfully deletes file \"{Path.GetFileName(file)}\".", ConsoleColor.Green);
                        else
                            ds = false;
                    }
                }

                foreach (var command in commands.Values)
                {
                    cs &= EnsureScript(path, command, Tool.PowerShell);
                    cs &= EnsureScript(path, command, Tool.Cmd);
                }

                createSucceess &= cs;
                deleteSuccess &= ds;

                if (cs && ds)
                    Console.WriteLineWithColor($"Successfully cleaned up \"{scopeName}\".", ConsoleColor.Green);
                else
                    Console.WriteLineWithColor($"Partially cleaned up \"{scopeName}\".", ConsoleColor.Yellow);
                Console.WriteLine();
            }

            bool EnsureScript(string path, Command command, Tool tool)
            {
                if (!File.Exists(CommandsUtility.GetScriptFilePath(path, command.Alias!, tool)))
                {
                    if (CommandsUtility.WriteScriptFile(path, command.Alias, tool, command.CommandText, command.Description, command.Tool))
                        Console.WriteLineWithColor($"Successfully created \"{tool}\"-script for alias \"{command.Alias}\".", ConsoleColor.Green);
                    else
                        return false;
                }

                return true;
            }
        }
    }
}
