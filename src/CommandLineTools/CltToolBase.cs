using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Console.Controls;
using MaSch.Console.Controls.Table;
using System;
using System.Linq;
using System.Reflection;

namespace MaSch.CommandLineTools
{
    public abstract class CltToolBase : ICltTool
    {
        public abstract void RegisterSubCommands(CliApplicationBuilder builder);
        public abstract void WriteExitCodeInfo(IConsoleService console);

        protected static void WriteCommonExitCodes(IConsoleService console)
            => WriteExitCodeList(console, "General", x => x <= 0);
        protected static void WriteExitCodeList(IConsoleService console, string title, ExitCode toolCommand)
            => WriteExitCodeList(console, title, toolCommand, ExitCode.ToolCommandMask);
        protected static void WriteExitCodeList(IConsoleService console, string title, ExitCode category, ExitCode mask)
            => WriteExitCodeList(console, title, x => (x & mask) == category);
        protected static void WriteExitCodeList(IConsoleService console, string title, Func<ExitCode, bool> exitCodePredicate)
        {
            var exitCodes = (from field in typeof(ExitCode).GetFields(BindingFlags.Public | BindingFlags.Static)
                             let attr = field.GetCustomAttribute<ReturnableExitCodeAttribute>()
                             where attr != null
                             let code = (ExitCode)field.GetValue(null)!
                             where exitCodePredicate(code)
                             let decimalCode = ((int)code).ToString()
                             let hexCode = "0x" + Convert.ToString((int)code, 16).PadLeft(8, '0')
                             select (decimalCode, hexCode, description: attr.Description)).ToArray();
            console.WriteLine();
            console.WriteLine($"{title}:");
            if (exitCodes.Length == 0)
            {
                console.WriteLine("   In this category no exit codes are returned.");
            }
            else
            {
                new TableControl(console)
                {
                    Margin = new(3, 0, 0, 0),
                    ShowColumnHeaders = false,
                    Columns =
                    {
                        new() { WidthMode = ColumnWidthMode.Auto },
                        new() { WidthMode = ColumnWidthMode.Auto },
                        new() { WidthMode = ColumnWidthMode.Star },
                    },
                    Rows = exitCodes.Select(x => new Row { Values = { x.decimalCode, x.hexCode, x.description } }).ToArray(),
                }.Render();
            }
        }
    }
}
