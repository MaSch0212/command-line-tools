using MaSch.CommandLineTools.Common;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Utilities
{
    public static class TemplateUtility
    {
        private static readonly Regex AllArgsRegex = new Regex(@"(?<!\w)(\$args(?![\[])|%\*)(?!\w)");
        private static readonly Regex SpecificCmdArgsRegex = new Regex(@"(?<!\w)%(?<nr>\d+)(?!\w)");
        private static readonly Regex SpecificPowerShellArgsRegex = new Regex(@"(?<!\w)\$args\[(?<nr>\d+)\](?!\w)");

        public static string GetPowerShellCommandScript(string alias, string command, string description, TerminalTool? targetTool)
        {
            var result = new StringBuilder()
                .AppendLine($"# Alias:{alias}")
                .AppendLine($"# Description:{description}")
                .AppendLine($"# Tool:{targetTool?.ToString() ?? "<universal>"}")
                .AppendLine()
                .AppendLine($"# Command:");

            if (targetTool is null || targetTool == TerminalTool.PowerShell)
            {
                command = AllArgsRegex.Replace(command, "$args");
                command = SpecificCmdArgsRegex.Replace(command, x => $"$args[{int.Parse(x.Groups["nr"].Value) - 1}]");
                result.AppendLine(command);
            }
            else if (targetTool == TerminalTool.Cmd)
            {
                result.AppendLine($"cmd /C \"`\"$PSScriptRoot\\{alias}.cmd`\"\" $args");
            }
            else
            {
                throw new ArgumentException($"The tool \"{targetTool}\" is not supported.", nameof(targetTool));
            }

            return result.ToString();
        }

        public static string GetCmdCommandScript(string alias, string command, string description, TerminalTool? targetTool)
        {
            var result = new StringBuilder()
                .AppendLine("@echo off")
                .AppendLine()
                .AppendLine($"REM Alias:{alias}")
                .AppendLine($"REM Description:{description}")
                .AppendLine($"REM Tool:{targetTool?.ToString() ?? "<universal>"}")
                .AppendLine()
                .AppendLine($"REM Command:");

            if (targetTool is null || targetTool == TerminalTool.Cmd)
            {
                command = AllArgsRegex.Replace(command, "%*");
                command = SpecificPowerShellArgsRegex.Replace(command, x => $"%{int.Parse(x.Groups["nr"].Value) + 1}");
                result.AppendLine(command);
            }
            else if (targetTool == TerminalTool.PowerShell)
            {
                result.AppendLine($"powershell -ExecutionPolicy ByPass -File \"%~dp0\\{alias}.ps1\" %*");
            }
            else
            {
                throw new ArgumentException($"The tool \"{targetTool}\" is not supported.", nameof(targetTool));
            }

            return result.ToString();
        }
    }
}
