using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.CommandAliaser.Models;
using MaSch.Core;
using MaSch.Core.Extensions;
using MaSch.Console;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using static MaSch.CommandLineTools.Tools.CommandAliaser.Constants;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Utilities
{
    public static class CommandsUtility
    {
        public static Dictionary<string, Command> LoadCommands(string path)
        {
            var commandsFilePath = Path.Combine(path, CommandsFileName);
            if (File.Exists(commandsFilePath))
            {
                var json = File.ReadAllText(commandsFilePath);
                var result = JsonConvert.DeserializeObject<Dictionary<string, Command>>(json);
                result.ForEach(x => x.Value.Alias = x.Key);
                return result;
            }
            else
                return new Dictionary<string, Command>();
        }

        public static bool SaveCommands(string path, Dictionary<string, Command> commands)
        {
            var commandsFilePath = Path.Combine(path, CommandsFileName);
            try
            {
                var json = JsonConvert.SerializeObject(commands, Formatting.Indented);
                Directory.CreateDirectory(path);
                File.WriteAllText(commandsFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                ServiceContext.GetService<IConsoleService>().WriteLineWithColor($"Could not save commands file under \"{commandsFilePath}\": {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        public static bool TryGetCommand(this Dictionary<string, Command> commands, string? alias, [NotNullWhen(true)] out Command? command)
        {
            if (commands.Keys.TryFirst(x => string.Equals(x, alias, System.StringComparison.OrdinalIgnoreCase), out var key))
            {
                command = commands[key];
                return true;
            }
            command = null;
            return false;
        }

        public static string GetScriptFilePath(string path, string alias, Tool tool)
        {
            return tool switch
            {
                Tool.PowerShell => Path.Combine(path, $"{alias}.ps1"),
                Tool.Cmd => Path.Combine(path, $"{alias}.cmd"),
                _ => throw new ArgumentException($"The tool \"{tool}\" is unknown.", nameof(tool))
            };
        }

        public static bool WriteScriptFile(string path, string? alias, Tool scriptTool, string? command, string? description, Tool? commandTool)
        {
            string scriptPath = string.Empty;
            try
            {
                scriptPath = GetScriptFilePath(path, alias, scriptTool);
                var scriptContent = scriptTool switch
                {
                    Tool.PowerShell => TemplateUtility.GetPowerShellCommandScript(alias ?? string.Empty, command ?? string.Empty, description ?? string.Empty, commandTool),
                    Tool.Cmd        => TemplateUtility.GetCmdCommandScript(alias ?? string.Empty, command ?? string.Empty, description ?? string.Empty, commandTool),
                    _               => throw new ArgumentException($"The tool \"{scriptTool}\" is unknown.", nameof(scriptTool))
                };
                Directory.CreateDirectory(path);
                File.WriteAllText(scriptPath, scriptContent);
                return true;
            }
            catch (Exception ex)
            {
                ServiceContext.GetService<IConsoleService>().WriteLineWithColor($"Could not save script file under \"{scriptPath}\": {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        public static bool DeleteScriptFile(string path, string alias, Tool scriptTool)
            => DeleteScriptFile(() => GetScriptFilePath(path, alias, scriptTool));
        public static bool DeleteScriptFile(string filePath)
            => DeleteScriptFile(() => filePath);
        private static bool DeleteScriptFile(Func<string> filePathFunc)
        {
            string filePath = string.Empty;
            try
            {
                filePath = filePathFunc();
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                ServiceContext.GetService<IConsoleService>().WriteLineWithColor($"Could not delete file \"{filePath}\": {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }
    }
}
