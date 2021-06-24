using MaSch.CommandLineTools.Common;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    [CliCommand("uninstall", HelpText = "Removes all directory aliases from environment variables", ParentCommand = typeof(DirectoryAliaserTool))]
    public class UninstallCommand : CommandBase
    {
        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            foreach (var path in Config.Paths)
            {
                foreach (var alias in path.Aliases)
                {
                    RemoveEnvironmentVariable(alias);
                }
            }

            return (int)ExitCode.Okay;
        }
    }
}
