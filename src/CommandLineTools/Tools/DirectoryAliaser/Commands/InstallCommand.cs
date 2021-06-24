using MaSch.CommandLineTools.Common;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    [CliCommand("install", HelpText = "Adds all directory aliases as environment variables starting with 'cdx_'", ParentCommand = typeof(DirectoryAliaserTool))]
    public class InstallCommand : CommandBase
    {
        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            foreach (var path in Config.Paths)
            {
                foreach (var alias in path.Aliases)
                {
                    AddEnvironmentVariable(alias, path.Path);
                }
            }

            return (int)ExitCode.Okay;
        }
    }
}
