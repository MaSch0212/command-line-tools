using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Core.Extensions;
using static MaSch.CommandLineTools.Tools.CommandAliaser.Constants;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Utilities
{
    public static class InstallUtility
    {
        public static ExitCode Install(IConsoleService console, InstallOptions options)
        {
            var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';').ToList() ?? new List<string>();
            bool changed = false;

            if (!options.Global && !options.Local && !options.User)
                options.Global = options.Local = options.User = true;

            if (options.User)
                AddUser();
            if (options.Local)
                AddLocal();
            if (options.Global)
                AddGlobal();

            if (changed)
            {
                Environment.SetEnvironmentVariable("PATH", string.Join(";", paths), EnvironmentVariableTarget.User);
                console.WriteLineWithColor("Successfully saved Path environment variable.", ConsoleColor.Green);
            }

            return ExitCode.Okay;

            void AddUser() => Add(UserCommandsPath, (false, LocalCommandsPath), (false, GlobalCommandsPath));
            void AddLocal() => Add(LocalCommandsPath, (true, UserCommandsPath), (false, GlobalCommandsPath));
            void AddGlobal() => Add(GlobalCommandsPath, (true, LocalCommandsPath), (true, UserCommandsPath));
            void Add(string path, params (bool before, string path)[] relatives)
            {
                Directory.CreateDirectory(path);
                if (paths.Contains(path, StringComparer.OrdinalIgnoreCase))
                {
                    console.WriteLineWithColor($"X {path}", ConsoleColor.Yellow);
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

                console.WriteLine($"+ {path}");

                changed = true;
            }
        }

        public static ExitCode Uninstall(IConsoleService console, UninstallOptions options)
        {
            var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';').ToList() ?? new List<string>();
            bool changed = false;

            if (!options.Global && !options.Local && !options.User)
                options.Global = options.Local = options.User = true;

            if (options.User)
                RemoveUser();
            if (options.Local)
                RemoveLocal();
            if (options.Global)
                RemoveGlobal();

            if (changed)
            {
                Environment.SetEnvironmentVariable("PATH", string.Join(";", paths), EnvironmentVariableTarget.User);
                console.WriteLineWithColor("Successfully saved Path environment variable.", ConsoleColor.Green);
            }

            return ExitCode.Okay;

            void RemoveUser() => Remove(UserCommandsPath);
            void RemoveLocal() => Remove(LocalCommandsPath);
            void RemoveGlobal() => Remove(GlobalCommandsPath);
            void Remove(string path)
            {
                if (paths.TryRemove(path, StringComparer.OrdinalIgnoreCase))
                {
                    console.WriteLine($"- {path}");
                    changed = true;
                }
                else
                {
                    console.WriteLineWithColor($"X {path}", ConsoleColor.Yellow);
                }
            }
        }
    }
}
