using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CommandLine;
using CommandLine.Text;
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
using MaSch.Core;
using MaSch.Core.Extensions;
using static System.Environment;
using ExitCode = MaSch.CommandLineTools.Common.ExitCode;

namespace MaSch.CommandLineTools
{
    public static class Program
    {
        private static readonly List<Type> _tools = new List<Type>();

        internal static string ConfigurationDirectory { get; set; } = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), "MaSch", "CommandLineTools");
        internal static Version Version { get; set; } = typeof(Program).Assembly.GetName().Version!;

        public static int Main(string[] args)
        {
            var console = new ConsoleService();
            console.CancelKeyPress += (s, e) => ProcessUtility.KillAllStartedProcesses();

            try
            {
                Directory.CreateDirectory(ConfigurationDirectory);

                ServiceContext.AddService<IConsoleService>(console);

                _tools.Add(typeof(DirectoryAliaserRunner));
                _tools.Add(typeof(CommandAliaserRunner));

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _tools.Add(typeof(AttachRunner));
                    _tools.Add(typeof(RobocopyRunner));
                    _tools.Add(typeof(SuRunner));
                    _tools.Add(typeof(SudoRunner));
                }

                var parser = new Parser(options =>
                {
                    options.HelpWriter = null;
                    options.IgnoreUnknownArguments = true;
                });
                var parserResult = parser.ParseArguments(args, _tools.ToArray());

                return (int)parserResult.MapResult(
                    options => RunTool(options, args),
                    errors =>
                    {
                        return (errors.IsHelp() || errors.IsVersion()) && parserResult.TypeInfo.Current != typeof(NullInstance)
                            ? RunTool(Activator.CreateInstance(parserResult.TypeInfo.Current)!, args)
                            : DisplayHelp(parserResult, errors);
                    });
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
                            msg += $"  - {eex.Message.Indent(4, false)}";

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
                    console.WriteLineWithColor($"An unhandeled exception occurred:{NewLine}  - {ex.ToString().Indent(4, false)}", ConsoleColor.Red);
                    return (int)ExitCode.UnknownError;
                }
            }
        }

        private static ExitCode RunTool(object runner, string[] args)
        {
            return ((IToolRunner)runner).Run(args.Skip(1).ToArray());
        }

        private static ExitCode DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            var console = ServiceContext.GetService<IConsoleService>();
            HelpText helpText;
            if (errors.IsVersion())
            {
                helpText = HelpText.AutoBuild(result);
                AdjustHelpText(helpText);
                AddToolVersions(helpText);
            }
            else
            {
                helpText = HelpText.AutoBuild(result, h =>
                {
                    AdjustHelpText(h);
                    return h;
                });
            }

            console.WriteLine(helpText);
            return errors.IsVersion() || errors.IsHelp() ? ExitCode.Okay : ExitCode.ArgumentParseError;

            void AdjustHelpText(HelpText h)
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = new HeadingInfo("MaSch Command Line Tools", Version.ToString(3));
                h.Copyright = new CopyrightInfo("Marc Schmidt", 2020);
                h.MaximumDisplayWidth = console.BufferSize.Width;
            }
        }

        private static void AddToolVersions(HelpText helpText)
        {
            helpText.AddPreOptionsLine("\nTools:\n");

            var toolInstances = (from tt in _tools
                                 let ti = Activator.CreateInstance(tt) as IToolRunner
                                 where ti != null
                                 let verbName = tt.GetCustomAttribute<VerbAttribute>()?.Name
                                 let name = ti.Options.DisplayName ?? tt.Name
                                 let version = ti.Options.Version.ToString(3)
                                 let copyright = new CopyrightInfo(ti.Options.Author, ti.Options.Year)
                                 select (name, verbName, version, copyright)).ToArray();
            helpText.AddPreOptionsLine(toolInstances.ToColumnsString(x => x.name, x => x.verbName, x => x.version, x => x.copyright));

            helpText.AddPreOptionsLine(string.Empty);
        }
    }
}
