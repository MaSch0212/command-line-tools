using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    [CliCommand("copy", HelpText = "Sends the directory that has a given alias to the output stream.", ParentCommand = typeof(DirectoryAliaserTool))]
    public class CopyCommand : CopyCommandBase
    {
        [CliCommandValue(0, "alias", Required = true, HelpText = "The alias to copy.")]
        public override string? Alias { get; set; }

        [CliCommandValue(1, "subdirectory", Required = false, HelpText = "The sub directory to copy.")]
        public override string? SubDirectory { get; set; }

        public CopyCommand(IConsoleService console)
            : base(console)
        {
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            return (int)HandleCopy(ExitCode.CdxCopyMissingAlias, ExitCode.CdxCopyAliasNotFound, x =>
            {
                Console.WriteLine(x);
                return ExitCode.Okay;
            });
        }
    }
}
