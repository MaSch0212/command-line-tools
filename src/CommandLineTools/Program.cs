using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.CommandLineTools.Services;
using MaSch.CommandLineTools.Tools.Attach;
using MaSch.CommandLineTools.Tools.CommandAliaser;
using MaSch.CommandLineTools.Tools.DirectoryAliaser;
using MaSch.CommandLineTools.Tools.RobocopyRunner;
using MaSch.CommandLineTools.Tools.Su;
using MaSch.CommandLineTools.Tools.Sudo;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;

namespace MaSch.CommandLineTools
{
    public static class Program
    {
        internal static string ConfigurationDirectory { get; set; }
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaSch", "CommandLineTools");

        private static readonly IConsoleService _console = new ConsoleService();

        public static int Main(string[] args)
        {
            try
            {
                Directory.CreateDirectory(ConfigurationDirectory);

                var app = new CliApplicationBuilder()
                    .ConfigureServices(ConfigureServices)
                    .ConfigureOptions(ConfigureOptions)
                    .ConfigureTools()
                    .Build();

                _console.CancelKeyPress += (s, e) => app.ServiceProvider.GetRequiredService<IProcessService>().KillAllStartedProcesses();

                return app.Run(args);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        private static void ConfigureOptions(CliApplicationOptions o)
        {
            o.CliName = ToolCommandBase.IsExecutedFromScript ? string.Empty : "clt";
            o.Name = "MaSch Command Line Tools";
            o.Author = "Marc Schmidt";
            o.Year = "2021";
            o.ParseErrorExitCode = (int)ExitCode.ArgumentParseError;
        }

        private static void ConfigureServices(IServiceCollection s)
        {
            s.AddSingleton<IConsoleService>(_console)
             .AddSingleton<ICliHelpPage, HelpPage>()
             .AddSingleton<IProcessService, ProcessService>();

            if (OperatingSystem.IsLinux())
            {
                s.AddSingleton<IOsService, LinuxOsService>();
            }
            else if (OperatingSystem.IsWindows())
            {
                s.AddSingleton<IOsService, WindowsOsService>()
                 .AddSingleton<IParentProcessService, ParentProcessService>();
            }
        }

        private static CliApplicationBuilder ConfigureTools(this CliApplicationBuilder b)
        {
            b = b.WithTool<DirectoryAliaserTool>()
                 .WithTool<CommandAliaserTool>();

            if (OperatingSystem.IsWindows())
            {
                b = b.WithTool<AttachTool>()
                     .WithTool<RobocopyTool>()
                     .WithTool<SuTool>()
                     .WithTool<SudoTool>();
            }

            return b;
        }

        private static int HandleError(Exception ex)
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
                        _console.WriteLine(msg);
                    }
                    else
                    {
                        var color = eex.ExitCode.IsSuccess()
                            ? ConsoleColor.Green
                            : eex.ExitCode.IsWarning()
                                ? ConsoleColor.Yellow
                                : ConsoleColor.Red;
                        _console.WriteLineWithColor(msg, color);
                    }
                }

                return (int)eex.ExitCode;
            }
            else
            {
                _console.WriteLineWithColor($"An unhandeled exception occurred:{Environment.NewLine}  - {ex.ToString().Indent(4, false)}", ConsoleColor.Red);
                return (int)ExitCode.UnknownError;
            }
        }
    }
}
