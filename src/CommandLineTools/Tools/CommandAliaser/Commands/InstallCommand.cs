using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Commands
{
    [CliCommand("install", HelpText = "Adjusts the Path variable to include aliases.", ParentCommand = typeof(CommandAliaserTool))]
    public class InstallCommand : CommandBase
    {
        public override bool IsScopeExcluse { get; } = false;

        [CliCommandOption('g', "global", HelpText = "Install global path.")]
        public override bool Global { get; set; }

        [CliCommandOption('l', "local", HelpText = "Install local path.")]
        public override bool Local { get; set; }

        [CliCommandOption('u', "user", HelpText = "Install user path.")]
        public override bool User { get; set; }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';').ToList() ?? new List<string>();
            bool changed = false;

            if (!Global && !Local && !User)
                Global = Local = User = true;

            if (User)
                AddUser();
            if (Local)
                AddLocal();
            if (Global)
                AddGlobal();

            if (changed)
            {
                Environment.SetEnvironmentVariable("PATH", string.Join(";", paths), EnvironmentVariableTarget.User);
                Console.WriteLineWithColor("Successfully saved Path environment variable.", ConsoleColor.Green);
            }

            return (int)ExitCode.Okay;

            void AddUser() => Add(UserCommandsPath, (false, LocalCommandsPath), (false, GlobalCommandsPath));
            void AddLocal() => Add(LocalCommandsPath, (true, UserCommandsPath), (false, GlobalCommandsPath));
            void AddGlobal() => Add(GlobalCommandsPath, (true, LocalCommandsPath), (true, UserCommandsPath));
            void Add(string path, params (bool Before, string Path)[] relatives)
            {
                Directory.CreateDirectory(path);
                if (paths.Contains(path, StringComparer.OrdinalIgnoreCase))
                {
                    Console.WriteLineWithColor($"X {path}", ConsoleColor.Yellow);
                    return;
                }

                int idx = -1;
                foreach (var (b, p) in relatives)
                {
                    int i = paths.IndexOf(p, StringComparer.OrdinalIgnoreCase);
                    if (i >= 0)
                    {
                        idx = i + (b ? 1 : 0);
                        break;
                    }
                }

                if (idx < 0)
                    paths.Add(path);
                else
                    paths.Insert(idx, path);

                Console.WriteLine($"+ {path}");

                changed = true;
            }
        }
    }
}
