using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.CommandLineTools.Tools.Attach;
using MaSch.CommandLineTools.Tools.CommandAliaser;
using MaSch.CommandLineTools.Tools.DirectoryAliaser;
using MaSch.CommandLineTools.Tools.RobocopyRunner;
using MaSch.CommandLineTools.Tools.Su;
using MaSch.CommandLineTools.Tools.Sudo;
using MaSch.CommandLineTools.Utilities;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Core;
using MaSch.Core.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MaSch.CommandLineTools
{
    public static class Program
    {
        internal static string ConfigurationDirectory { get; set; }
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaSch", "CommandLineTools");

        public static int Main(string[] args)
        {
            var console = new ConsoleService();
            console.CancelKeyPress += (s, e) => ProcessUtility.KillAllStartedProcesses();

            try
            {
                Directory.CreateDirectory(ConfigurationDirectory);

                ServiceContext.AddService<IConsoleService>(console);

                var helpPage = new HelpPage();
                var appBuilder = new CliApplicationBuilder()
                    .WithHelpPage(helpPage)
                    .Configure(o =>
                    {
                        o.CliName = ToolCommandBase.IsExecutedFromScript ? string.Empty : "clt";
                        o.Name = "MaSch Command Line Tools";
                        o.Author = "Marc Schmidt";
                        o.Year = "2021";
                        o.ConsoleService = console;
                    })
                    .WithTool<DirectoryAliaserTool>()
                    .WithTool<CommandAliaserTool>();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    appBuilder = appBuilder
                        .WithTool<AttachTool>()
                        .WithTool<RobocopyTool>()
                        .WithTool<SuTool>()
                        .WithTool<SudoTool>();
                }

                var app = appBuilder.Build();
                foreach (var command in app.Commands.GetRootCommands())
                {
                    if (command.OptionsInstance is IHelpPageMutator mutator)
                        helpPage.WithMutator(mutator);
                }

                return app.Run(args);
            }
            catch (Exception ex)
            {
                var eex = ex as ApplicationExitException ?? (ex as TargetInvocationException)?.InnerException as ApplicationExitException;
                if (eex != null)
                {
                    if (!string.IsNullOrEmpty(eex.Message))
                    {
                        var msg = eex.Message;
                        if (eex.InnerException != null)
                            msg += $"   - {eex.Message.Indent(4, false)}";

                        if (eex.ExitCode.IsInfo())
                        {
                            console.WriteLine(msg);
                        }
                        else
                        {
                            var color = eex.ExitCode.IsSuccess()
                                ? ConsoleColor.Green
                                : eex.ExitCode.IsWarning()
                                    ? ConsoleColor.Yellow
                                    : ConsoleColor.Red;
                            console.WriteLineWithColor(msg, color);
                        }
                    }

                    return (int)eex.ExitCode;
                }
                else
                {
                    console.WriteLineWithColor($"An unhandeled exception occurred:{Environment.NewLine}  - {ex.ToString().Indent(4, false)}", ConsoleColor.Red);
                    return (int)ExitCode.UnknownError;
                }
            }
        }
    }
}
