using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.Sudo;
using MaSch.CommandLineTools.Tools.Sudo.Services;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Tools.Attach
{
    [SupportedOSPlatform("windows")]
    [CliCommand("attach", HelpText = "Attaches console output to a given process.")]
    [CliMetadata(DisplayName = "Attach", Version = "1.1.0", Author = "Marc Schmidt", Year = "2021")]
    [CltTool(null, nameof(WriteExitCodeInfo))]
    public class AttachTool : CltToolBase, ICliExecutable
    {
        private readonly ISudoService _sudoService;

        [CliCommandOption('p', "process", Required = true, HelpText = "The process to which to attach.")]
        public int ProcessId { get; set; }

        [CliCommandOption('f', "file", Required = true, HelpText = "The file to execute.")]
        public string FileName { get; set; } = string.Empty;

        [CliCommandOption('a', "arguments", Required = false, HelpText = "The arguments to pass to the file to execute.")]
        public string Arguments { get; set; } = string.Empty;

        public AttachTool(ISudoService sudoService)
        {
            _sudoService = sudoService;
        }

        public int ExecuteCommand(CliExecutionContext context)
        {
            return (int)_sudoService.Do(FileName, Arguments, ProcessId);
        }

        public static void WriteExitCodeInfo(IConsoleService console)
        {
            WriteCommonExitCodes(console);
            WriteExitCodeList(console, "Run (Default)", ExitCode.AttachRun);
        }
    }
}
