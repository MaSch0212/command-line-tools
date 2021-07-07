using MaSch.CommandLineTools.Common;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Console.Controls;
using MaSch.Console.Controls.Table;
using MaSch.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MaSch.CommandLineTools.Tools.RobocopyRunner
{
    [CliCommand("rcx", HelpText = "Provides a different way to execute robocopy. Also contains some robocopy extensions.")]
    [CliMetadata(DisplayName = "Robocopy Runner", Version = "1.1.0", Author = "Marc Schmidt", Year = "2021")]
    [CltTool(nameof(RegisterSubCommands), nameof(WriteExitCodeInfo))]
    public class RobocopyTool : CltToolBase, IOptionOrderMutator
    {
        private static readonly (int Code, string Description)[] RobocopyExitCodes =
        {
            (0, "No files were copied. No failure was encountered. No files were mismatched. The files already exist in the destination directory; therefore, the copy operation was skipped."),
            (1, "All files were copied successfully."),
            (2, "There are some additional files in the destination directory that are not present in the source directory. No files were copied."),
            (3, "Some files were copied. Additional files were present. No failure was encountered."),
            (5, "Some files were copied. Some files were mismatched. No failure was encountered."),
            (6, "Additional files and mismatched files exist. No files were copied and no failures were encountered. This means that the files already exist in the destination directory."),
            (7, "Files were copied, a file mismatch was present, and additional files were present."),
            (8, "Several files did not copy."),
        };

        public bool CanMutate(CliError error)
        {
            return error.AffectedCommand?.ParentCommand?.OptionsInstance == this;
        }

        public IEnumerable<ICliCommandOptionInfo> OrderOptions(CliError error, IEnumerable<ICliCommandOptionInfo> options)
        {
            var properties = error.AffectedCommand!.CommandType.GetTypeInfo().GetProperties().Select(x => x.Name).ToList();
            return options.OrderBy(x => properties.IndexOf(x.PropertyName));
        }

        public static void RegisterSubCommands(CliApplicationBuilder builder)
        {
            builder.WithCommand(typeof(CopyOptions), typeof(RobocopyExecutor))
                   .WithCommand(typeof(MoveOptions), typeof(RobocopyExecutor))
                   .WithCommand(typeof(CreateOptions), typeof(RobocopyExecutor));
        }

        public static void WriteExitCodeInfo(IConsoleService console)
        {
            WriteExitCodeList(console, "General", x => x < 0);
            WriteExitCodeList(console, "Copy (Default)", ExitCode.RobocopyCopy);
            console.WriteLine("   If no error occures the exit code of robocopy is returned.");
            WriteExitCodeList(console, "Move", ExitCode.RobocopyMove);
            console.WriteLine("   If no error occures the exit code of robocopy is returned.");
            WriteExitCodeList(console, "Create", ExitCode.RobocopyCreate);
            console.WriteLine("   If no error occures the exit code of robocopy is returned.");

            console.WriteLine();
            console.WriteLine("robocopy Exit Codes:");
            new TableControl(console)
            {
                Margin = new(3, 0, 0, 0),
                ShowColumnHeaders = false,
                Columns =
                {
                    new() { WidthMode = ColumnWidthMode.Auto },
                    new() { WidthMode = ColumnWidthMode.Star },
                },
                Rows = RobocopyExitCodes.Select(x => new Row { Values = { x.Code.ToString(), x.Description } }).ToArray(),
            }.Render();
        }
    }
}
