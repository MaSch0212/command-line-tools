using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Services;
using MaSch.CommandLineTools.Tools.Sudo.Services;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Tools.Su
{
    [SupportedOSPlatform("windows")]
    [CliCommand("su", HelpText = "Elevates the current command line to administrative rights.")]
    [CliMetadata(DisplayName = "Super User", Version = "1.1.0", Author = "Marc Schmidt", Year = "2021")]
    [CltTool(null, nameof(WriteExitCodeInfo))]
    public class SuTool : CltToolBase, ICliExecutable
    {
        private readonly ISudoService _sudoService;
        private readonly IOsService _osService;

        [CliCommandOption('t', "tool", Required = false, HelpText = "The command line tool to start (default is the tool that started this process - falls back to 'powershell' if parent process cannot be retrieved).")]
        public string? CommandLineToolName { get; set; }

        public SuTool(ISudoService sudoService, IOsService osService)
        {
            _sudoService = sudoService;
            _osService = osService;
        }

        public int ExecuteCommand(CliExecutionContext context)
        {
            if (_osService.IsRoot())
                return (int)ExitCode.SuRunAlreadyElevated;

            _sudoService.VerifyAdminRole("su", ExitCode.SuRunNoAdmin);

            var tool = _sudoService.GetTool(CommandLineToolName, out string toolName, ExitCode.SuRunUnknownTool);
            return (int)(tool switch
            {
                TerminalTool.PowerShell => _sudoService.Run(toolName, "-nologo", ExitCode.SuRunUserDeclined),
                TerminalTool.Cmd => _sudoService.Run(toolName, "/k", ExitCode.SuRunUserDeclined),
                _ => ExitCode.SuRunUnknownTool,
            });
        }

        public static void WriteExitCodeInfo(IConsoleService console)
        {
            WriteCommonExitCodes(console);
            WriteExitCodeList(console, "Run (Default)", ExitCode.SuRun);
        }
    }
}
