using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System.Reflection;

namespace MaSch.CommandLineTools
{
    [CliCommand("exitcodes", HelpText = "Display exit codes that this tool can return.")]
    [CliParserOptions(IgnoreAdditionalValues = true, IgnoreUnknownOptions = true)]
    public class ExitCodeCommand : ICliExecutable
    {
        private readonly IConsoleService _console;

        public ExitCodeCommand(IConsoleService console)
        {
            _console = console;
        }

        public int ExecuteCommand(CliExecutionContext context)
        {
            _console.WriteLine("Possible Exit Codes:");

            var toolType = context.Command.ParentCommand!.CommandType;
            var toolAttr = toolType.GetCustomAttribute<CltToolAttribute>();
            if (toolAttr?.WriteExitCodesMethodName is not null)
            {
                var mi = toolType.GetMethod(toolAttr.WriteExitCodesMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                mi?.Invoke(null, new object[] { _console });
            }

            _console.WriteLine();

            return (int)ExitCode.Okay;
        }
    }
}
