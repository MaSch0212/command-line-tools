using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.Sudo.Commands;
using MaSch.CommandLineTools.Tools.Sudo.Services;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Console.Cli.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Tools.Sudo
{
    [SupportedOSPlatform("windows")]
    [CliCommand("sudo", HelpText = "Executes a commands in an elevated process.")]
    [CliMetadata(DisplayName = "Super User Do", Version = "1.1.0", Author = "Marc Schmidt", Year = "2021")]
    [CltTool(nameof(RegisterSubCommands), nameof(WriteExitCodeInfo))]
    public class SudoTool : CltToolBase
    {
        public static void RegisterSubCommands(CliApplicationBuilder builder)
        {
            builder.ConfigureServices(s =>
                s.AddSingleton<ISudoService, SudoService>());

            builder.WithCommand<RunCommand>()
                   .WithCommand<DoCommand>()
                   .WithCommand<WatchCommand>()
                   .WithCommand<RepeatCommand>();
        }

        public static void WriteExitCodeInfo(IConsoleService console)
        {
            WriteCommonExitCodes(console);
            WriteExitCodeList(console, "Run (Default)", ExitCode.SudoRun);
            console.WriteLine("   If no error occures the exit code of the called command is returned.");
            WriteExitCodeList(console, "Please", ExitCode.SudoPlease);
            console.WriteLine("   If no error occures the exit code of the called command is returned.");
        }
    }
}
