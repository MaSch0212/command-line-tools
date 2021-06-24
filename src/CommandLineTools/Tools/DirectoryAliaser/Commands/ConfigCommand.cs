using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.Console;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Console.Controls;
using MaSch.Console.Controls.Table;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    [CliCommand("config", HelpText = "Configures the directory aliaser", ParentCommand = typeof(DirectoryAliaserTool))]
    public class ConfigCommand : CommandBase
    {
        [CliCommandOption('a', "auto-var", Required = false, HelpText = "Sets a value indicating whether an environment variable should be created when adding a new directory alias.")]
        public bool? AutoInstallVariable { get; set; }

        protected override int OnExecuteCommand(CliExecutionContext context)
        {
            if (AutoInstallVariable == null)
            {
                Console.WriteLine("The directory aliaser is configured as follows:");
                Console.WriteLine();
                new TableControl(Console)
                {
                    Margin = new(3, 0, 0, 0),
                    Columns =
                    {
                        new() { Header = "Name", WidthMode = ColumnWidthMode.Auto },
                        new() { Header = "Option", WidthMode = ColumnWidthMode.Auto },
                        new() { Header = "Value", WidthMode = ColumnWidthMode.Auto },
                    },
                    Rows =
                    {
                        new() { Values = { nameof(AutoInstallVariable), "--auto-var (-a)", Config.IsAutoVariableEnabled.ToString() } },
                    },
                }.Render();
                Console.WriteLine();
                return (int)ExitCode.Okay;
            }
            else
            {
                if (AutoInstallVariable != null)
                    Config.IsAutoVariableEnabled = AutoInstallVariable.Value;
                ConfigChanged = true;
            }

            return (int)ExitCode.Okay;
        }
    }
}
