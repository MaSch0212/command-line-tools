using MaSch.CommandLineTools.Tools.Sudo.Services;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Tools.Sudo.Commands
{
    [SupportedOSPlatform("windows")]
    [CliCommand("do", Hidden = true, ParentCommand = typeof(SudoTool))]
    public class DoCommand : ToolCommandBase
    {
        private readonly ISudoService _sudoService;

        [CliCommandOption('f', "file", Required = true)]
        public string FileName { get; set; } = string.Empty;

        [CliCommandOption('a', "args", Required = false)]
        public string Arguments { get; set; } = string.Empty;

        [CliCommandOption('p', "process", Required = true)]
        public int ParentProcessId { get; set; }

        public DoCommand(ISudoService sudoService)
        {
            _sudoService = sudoService;
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            return (int)_sudoService.Do(FileName, Arguments, ParentProcessId);
        }
    }
}
