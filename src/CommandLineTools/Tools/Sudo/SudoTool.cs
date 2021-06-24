using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.Sudo.Commands;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Console.Cli.Configuration;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Tools.Sudo
{
    [SupportedOSPlatform("windows")]
    [CliCommand(
        "sudo",
        HelpText = "Executes a commands in an elevated process.",
        DisplayName = "Super User Do",
        Version = "1.1.0",
        Author = "Marc Schmidt",
        Year = "2021",
        Executable = false)]
    public class SudoTool : CltToolBase
    {
        public override void RegisterSubCommands(CliApplicationBuilder builder)
        {
            builder.WithCommand<RunCommand>()
                   .WithCommand<DoCommand>()
                   .WithCommand<WatchCommand>()
                   .WithCommand<RepeatCommand>();
        }

        public override void WriteExitCodeInfo(IConsoleService console)
        {
            WriteCommonExitCodes(console);
            WriteExitCodeList(console, "Run (Default)", ExitCode.SudoRun);
            console.WriteLine("   If no error occures the exit code of the called command is returned.");
            WriteExitCodeList(console, "Please", ExitCode.SudoPlease);
            console.WriteLine("   If no error occures the exit code of the called command is returned.");
        }
    }
}
