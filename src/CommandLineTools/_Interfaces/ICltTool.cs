using MaSch.Console;
using MaSch.Console.Cli;

namespace MaSch.CommandLineTools
{
    public interface ICltTool
    {
        void RegisterSubCommands(CliApplicationBuilder builder);
        void WriteExitCodeInfo(IConsoleService console);
    }
}
