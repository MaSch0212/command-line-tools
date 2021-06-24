using MaSch.CommandLineTools.Common;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    [CliCommand("list", HelpText = "List all directory aliases", ParentCommand = typeof(DirectoryAliaserTool))]
    public class ListCommand : CommandBase
    {
        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            PrintList();
            return (int)ExitCode.Okay;
        }
    }
}
