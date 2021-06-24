using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.Sudo;
using MaSch.CommandLineTools.Utilities;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Tools.Su
{
    [SupportedOSPlatform("windows")]
    [CliCommand(
        "su",
        HelpText = "Elevates the current command line to administrative rights.",
        DisplayName = "Super User",
        Version = "1.1.0",
        Author = "Marc Schmidt",
        Year = "2021")]
    public class SuTool : CltToolBase, ICliCommandExecutor
    {
        [CliCommandOption('t', "tool", Required = false, HelpText = "The command line tool to start (default is the tool that started this process - falls back to 'powershell' if parent process cannot be retrieved).")]
        public string? CommandLineToolName { get; set; }

        public override void RegisterSubCommands(CliApplicationBuilder builder)
        {
            // No sub commands.
        }

        public override void WriteExitCodeInfo(IConsoleService console)
        {
            WriteCommonExitCodes(console);
            WriteExitCodeList(console, "Run (Default)", ExitCode.SuRun);
        }

        public int ExecuteCommand(CliExecutionContext context)
        {
            if (OsUtility.IsRoot())
                return (int)ExitCode.SuRunAlreadyElevated;

            SudoController.VerifyAdminRole("su", ExitCode.SuRunNoAdmin);

            var tool = SudoController.GetTool(CommandLineToolName, out string toolName, ExitCode.SuRunUnknownTool);
            return (int)(tool switch
            {
                TerminalTool.PowerShell => SudoController.Run(toolName, "-nologo", ExitCode.SuRunUserDeclined),
                TerminalTool.Cmd => SudoController.Run(toolName, "/k", ExitCode.SuRunUserDeclined),
                _ => ExitCode.SuRunUnknownTool,
            });
        }
    }
}
