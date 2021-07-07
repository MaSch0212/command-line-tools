using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using System;
using System.IO;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    [CliCommand("open", IsDefault = true, HelpText = "Sets the current directory to the directory that has a given alias.", ParentCommand = typeof(DirectoryAliaserTool))]
    public class OpenCommand : CopyCommandBase
    {
        [CliCommandValue(0, "alias", Required = true, HelpText = "The alias to open.")]
        public override string? Alias { get; set; }

        [CliCommandValue(1, "subdirectory", Required = false, HelpText = "The sub directory to open.")]
        public override string? SubDirectory { get; set; }

        public OpenCommand(IConsoleService console)
            : base(console)
        {
        }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), "cdx-open.tmp");
            return (int)HandleCopy(ExitCode.CdxOpenMissingAlias, ExitCode.CdxOpenAliasNotFound, x =>
            {
                File.WriteAllText(tmpFile, x);
                if (!IsExecutedFromScript)
                {
                    Console.WriteLineWithColor(
                        "This open command was not executed with a supported script. " +
                        "Changing the current directory in a console/terminal session is only possible with a compatible script. " +
                        "If you want to write your own script, add the '--from-script' parameter and handle the directory change using the path written " +
                        $"to the file 'cdx-open.tmp' in the temp path of the current user (currently \"{Path.GetTempPath()}\").",
                        ConsoleColor.Red);
                    Console.WriteLineWithColor("If you just wanted to show the path behind the given alias, use the 'cdx copy' command.", ConsoleColor.Yellow);
                    return ExitCode.CdxOpenNotFromSupportedScript;
                }

                return ExitCode.Okay;
            });
        }
    }
}
