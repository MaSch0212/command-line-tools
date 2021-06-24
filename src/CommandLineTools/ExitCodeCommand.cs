using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;

namespace MaSch.CommandLineTools
{
    [CliCommand("exitcodes", HelpText = "Display exit codes that this tool can return.")]
    [CliParserOptions(IgnoreAdditionalValues = true, IgnoreUnknownOptions = true)]
    public class ExitCodeCommand : ICliCommandExecutor
    {
        public int ExecuteCommand(CliExecutionContext context)
        {
            context.Console.WriteLine("Possible Exit Codes:");

            var toolInstance = (ICltTool)context.Command.ParentCommand!.OptionsInstance!;
            toolInstance.WriteExitCodeInfo(context.Console);

            context.Console.WriteLine();

            return (int)ExitCode.Okay;
        }
    }
}
