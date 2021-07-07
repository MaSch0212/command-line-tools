using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Commands
{
    [CliCommand("uninstall", HelpText = "Remove the aliases paths from the Path variable.", ParentCommand = typeof(CommandAliaserTool))]
    public class UninstallCommand : CommandBase
    {
        private readonly IConsoleService _console;

        public override bool IsScopeExcluse { get; } = false;

        [CliCommandOption('g', "global", HelpText = "Uninstall global path.")]
        public override bool Global { get; set; }

        [CliCommandOption('l', "local", HelpText = "Uninstall local path.")]
        public override bool Local { get; set; }

        [CliCommandOption('u', "user", HelpText = "Uninstall user path.")]
        public override bool User { get; set; }

        public UninstallCommand(IConsoleService console)
        {
            _console = console;
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';').ToList() ?? new List<string>();
            bool changed = false;

            if (!Global && !Local && !User)
                Global = Local = User = true;

            if (User)
                RemoveUser();
            if (Local)
                RemoveLocal();
            if (Global)
                RemoveGlobal();

            if (changed)
            {
                Environment.SetEnvironmentVariable("PATH", string.Join(";", paths), EnvironmentVariableTarget.User);
                _console.WriteLineWithColor("Successfully saved Path environment variable.", ConsoleColor.Green);
            }

            return (int)ExitCode.Okay;

            void RemoveUser() => Remove(UserCommandsPath);
            void RemoveLocal() => Remove(LocalCommandsPath);
            void RemoveGlobal() => Remove(GlobalCommandsPath);
            void Remove(string path)
            {
                if (paths.TryRemove(path, StringComparer.OrdinalIgnoreCase))
                {
                    _console.WriteLine($"- {path}");
                    changed = true;
                }
                else
                {
                    _console.WriteLineWithColor($"X {path}", ConsoleColor.Yellow);
                }
            }
        }
    }
}
