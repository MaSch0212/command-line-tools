using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.Sudo.Services;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Tools.Sudo.Commands
{
    [SupportedOSPlatform("windows")]
    [CliCommand("please", HelpText = "Repeats the last command of this command line as su.", ParentCommand = typeof(SudoTool))]
    public class RepeatCommand : ToolCommandBase
    {
        private readonly ISudoService _sudoService;

        [CliCommandOption('t', "tool", Required = false, HelpText = "The command line tool to start (default is the tool that started this process - falls back to 'powershell' if parent process cannot be retrieved).")]
        public string? CommandLineToolName { get; set; }

        [CliCommandOption("command", Required = false, Hidden = true)]
        public string? Command { get; set; }

        public RepeatCommand(ISudoService sudoService)
        {
            _sudoService = sudoService;
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            _sudoService.VerifyAdminRole("sudo", ExitCode.SudoRunNoAdmin);

            return (int)_sudoService.Run(CommandLineToolName, x => x switch
            {
                TerminalTool.Cmd => Command ?? _sudoService.GetLastCmdCommand(),
                _ => Command ?? "Write-Host 'Last command could not be determined!' -Foreground Red",
            });
        }
    }
}
