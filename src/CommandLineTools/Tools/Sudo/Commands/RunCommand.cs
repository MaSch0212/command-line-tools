using MaSch.CommandLineTools.Common;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace MaSch.CommandLineTools.Tools.Sudo.Commands
{
    [SupportedOSPlatform("windows")]
    [CliCommand(
        "run",
        IsDefault = true,
        HelpText = "Runs the given command in an elevated process redirecting output to the current terminal instance.",
        ParentCommand = typeof(SudoTool))]
    public class RunCommand : ToolCommandBase
    {
        [CliCommandOption("tool", Required = false, HelpText = "The command line tool to start (default is the tool that started this process - falls back to 'powershell' if parent process cannot be retrieved).")]
        public string? CommandLineToolName { get; set; }

        [CliCommandValue(0, "command", Required = true)]
        public IEnumerable<string> Commands { get; set; } = Array.Empty<string>();

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            SudoController.VerifyAdminRole("sudo", ExitCode.SudoRunNoAdmin);

            var allArgs = Environment.GetCommandLineArgs();
            IEnumerable<string> args = allArgs.Skip(string.Equals(allArgs[2], "run", StringComparison.OrdinalIgnoreCase) ? 3 : 2);

            var ddIdx = args.IndexOf("--");
            var tIdx = args.IndexOf(x => string.Equals(x, "--tool", StringComparison.OrdinalIgnoreCase));
            if (ddIdx >= 0)
                args = args.Take(ddIdx).Concat(args.Skip(ddIdx + 1));
            if (tIdx >= 0 && (ddIdx < 0 || tIdx < ddIdx))
                args = args.Take(tIdx).Concat(args.Skip(tIdx + 2));

            return (int)SudoController.Run(CommandLineToolName, x => x switch
            {
                TerminalTool.PowerShell => string.Join(" ", args.Select(SerializePS)),
                _ => string.Join(" ", args.Select(SerializeCmd)),
            });

            static string SerializePS(string a)
            {
                if (Regex.IsMatch(a, @"\s"))
                    return $"'{a}'";
                return Regex.Replace(a, "[>&]", "`$0");
            }

            static string SerializeCmd(string a)
            {
                if (Regex.IsMatch(a, @"\s"))
                    return $"\"{a}\"";
                return a;
            }
        }
    }
}
