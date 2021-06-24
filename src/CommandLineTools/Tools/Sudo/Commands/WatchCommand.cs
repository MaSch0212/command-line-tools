using MaSch.CommandLineTools.Common;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Tools.Sudo.Commands
{
    [SupportedOSPlatform("windows")]
    [CliCommand("watch", Hidden = true, ParentCommand = typeof(SudoTool))]
    public class WatchCommand : ToolCommandBase
    {
        [CliCommandOption('p', "process", Required = true)]
        public int ProcessId { get; set; }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            var process = Process.GetProcessById(ProcessId);
            process.EnableRaisingEvents = true;
            process.WaitForExit();
            SudoController.WriteExitCodeFile(Environment.ProcessId, process.ExitCode);
            return (int)ExitCode.Okay;
        }
    }
}
