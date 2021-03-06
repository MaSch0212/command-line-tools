﻿using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.CommandAliaser.Services;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Console.Controls;
using MaSch.Console.Controls.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Commands
{
    [CliCommand("list", IsDefault = true, HelpText = "Lists all command aliases.", ParentCommand = typeof(CommandAliaserTool))]
    public class ListCommand : CommandBase
    {
        private readonly IConsoleService _console;
        private readonly ICommandsService _commandsService;

        public override bool IsScopeExcluse { get; } = false;

        [CliCommandOption('g', "global", HelpText = "Only show global commands.")]
        public override bool Global { get; set; }

        [CliCommandOption('l', "local", HelpText = "Only show local commands.")]
        public override bool Local { get; set; }

        [CliCommandOption('u', "user", HelpText = "Only show user commands.")]
        public override bool User { get; set; }

        public ListCommand(IConsoleService console, ICommandsService commandsService)
        {
            _console = console;
            _commandsService = commandsService;
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';').ToList() ?? new List<string>();

            if (!Global && !Local && !User)
                Global = Local = User = true;

            _console.WriteLine("The following aliases are available:");
            _console.WriteLine();

            if (Global)
                PrintList("Global", GlobalCommandsPath);
            if (Local)
                PrintList("List", LocalCommandsPath);
            if (User)
                PrintList("User", UserCommandsPath);

            return (int)ExitCode.Okay;

            void PrintList(string scopeName, string path)
            {
                bool isInstalled = paths.Contains(path, StringComparer.OrdinalIgnoreCase);
                _console.Write($"{scopeName} (");
                _console.WriteWithColor(isInstalled ? "Installed" : "Not Installed", isInstalled ? ConsoleColor.Green : ConsoleColor.Red);
                _console.WriteLine(")");

                var commands = _commandsService.LoadCommands(path);
                if (commands.Count > 0)
                {
                    new TableControl(_console)
                    {
                        Margin = new(3, 0, 0, 0),
                        ShowColumnHeaders = false,
                        Columns =
                        {
                            new() { WidthMode = ColumnWidthMode.Auto },
                            new() { WidthMode = ColumnWidthMode.Star },
                        },
                        Rows = commands.Select(x => new Row { Values = { x.Key, x.Value.Description ?? "<No Description>" } }).ToArray(),
                    }.Render();
                }
                else
                {
                    _console.WriteLine("   There are currently no aliases registered in this scope.");
                }

                _console.WriteLine();
            }
        }
    }
}
