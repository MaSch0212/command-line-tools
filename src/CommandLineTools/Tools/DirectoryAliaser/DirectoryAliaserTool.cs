using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Console.Cli.Configuration;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser
{
    [CliCommand("cdx", HelpText = "Manages directory aliases. Aliases to directories can be added, removed and requested.")]
    [CliMetadata(DisplayName = "Directory Aliaser", Version = "1.3.0", Author = "Marc Schmidt", Year = "2021")]
    [CltTool(nameof(RegisterSubCommands), nameof(WriteExitCodeInfo))]
    public class DirectoryAliaserTool : CltToolBase
    {
        public static void RegisterSubCommands(CliApplicationBuilder builder)
        {
            builder.WithCommand<OpenCommand>()
                   .WithCommand<CopyCommand>()
                   .WithCommand<AddCommand>()
                   .WithCommand<RemoveCommand>()
                   .WithCommand<ListCommand>()
                   .WithCommand<InstallCommand>()
                   .WithCommand<UninstallCommand>()
                   .WithCommand<ConfigCommand>();
        }

        public static void WriteExitCodeInfo(IConsoleService console)
        {
            WriteCommonExitCodes(console);
            WriteExitCodeList(console, "Open (Default)", ExitCode.CdxOpen);
            WriteExitCodeList(console, "Copy", ExitCode.CdxCopy);
            WriteExitCodeList(console, "Add", ExitCode.CdxAdd);
            WriteExitCodeList(console, "Remove", ExitCode.CdxRemove);
            WriteExitCodeList(console, "List", ExitCode.CdxList);
            WriteExitCodeList(console, "Install", ExitCode.CdxInstall);
            WriteExitCodeList(console, "Uninstall", ExitCode.CdxUninstall);
            WriteExitCodeList(console, "Config", ExitCode.CdxConfig);
        }
    }
}
