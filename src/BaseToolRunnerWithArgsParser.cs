using CommandLine;
using CommandLine.Text;
using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.Core;
using MaSch.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MaSch.CommandLineTools
{
    public abstract class BaseToolRunnerWithArgsParser : IToolRunner
    {
        private readonly VerbAttribute _verbAttribute;
        protected readonly IConsoleService Console;

        IRunnerOptions IToolRunner.Options => Options;
        public RunnerOptions Options { get; }

        public BaseToolRunnerWithArgsParser()
        {
            _verbAttribute = GetType().GetCustomAttribute<VerbAttribute>()
                ?? throw new InvalidOperationException($"A class of type \"{nameof(BaseToolRunnerWithArgsParser)}\" needs to have a \"{nameof(VerbAttribute)}\" attached to it.");

            Options = new RunnerOptions();
            ServiceContext.GetService(out Console);
            Configure(Options);
        }

        public ExitCode Run(string[] args)
        {
            var parser = new Parser(ConfigureParser);
            var parserResult = parser.ParseArguments(args, Options.GetHandeledOptions());
            return parserResult.MapResult(
                options => OnParsed(options),
                errors => OnNotParsed(parserResult, errors));
        }

        protected void Configure(RunnerOptions options)
        {
            OnConfigure(options);
            options.AddOptionHandler<ExitCodeOptions>(x =>
            {
                Console.WriteLine("Possible Exit Codes:");
                OnHandleExitCodes();
                Console.WriteLine();
                return ExitCode.Okay;
            });
        }
        protected abstract void OnConfigure(RunnerOptions options);
        protected abstract void OnHandleExitCodes();

        protected virtual void ConfigureParser(ParserSettings options)
        {
            options.HelpWriter = null;
            options.EnableDashDash = true;
        }

        protected virtual ExitCode OnParsed(object options)
        {
            var getOptionHandler = typeof(RunnerOptions).GetMethod(nameof(Options.GetOptionHanlder))!;
            var func = getOptionHandler.MakeGenericMethod(options.GetType()).Invoke(Options, new object[0])!;
            return (ExitCode)(func.GetType().GetMethod("Invoke")!.Invoke(func, new object[] { options })!);
        }

        protected virtual ExitCode OnNotParsed<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            WriteHelpText(result, errors);
            return errors.IsVersion() || errors.IsHelp() ? ExitCode.Okay : ExitCode.ArgumentParseError;
        }

        protected virtual void WriteHelpText<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            HelpText helpText;
            if (errors.IsVersion())
            {
                helpText = HelpText.AutoBuild(result);
                AdjustHelpText(result, helpText);
            }
            else
            {
                helpText = HelpText.AutoBuild(result, h =>
                {
                    AdjustHelpText(result, h);
                    return h;
                });
            }
            Console.WriteLine(helpText);
        }

        protected virtual void AdjustHelpText<T>(ParserResult<T> result, HelpText h)
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = new HeadingInfo(Options.DisplayName ?? _verbAttribute.Name, Options.Version?.ToString(3));
            h.Copyright = new CopyrightInfo(Options.Author, Options.Year);
            h.MaximumDisplayWidth = Console.BufferSize.Width;
        }

        protected void WriteCommonExitCodes()
            => WriteExitCodeList("General", x => x <= 0);
        protected void WriteExitCodeList(string title, ExitCode toolCommand)
            => WriteExitCodeList(title, toolCommand, ExitCode.ToolCommandMask);
        protected void WriteExitCodeList(string title, ExitCode category, ExitCode mask)
            => WriteExitCodeList(title, x => (x & mask) == category);
        protected void WriteExitCodeList(string title, Func<ExitCode, bool> exitCodePredicate)
        {
            var exitCodes = (from field in typeof(ExitCode).GetFields(BindingFlags.Public | BindingFlags.Static)
                             let attr = field.GetCustomAttribute<ReturnableExitCodeAttribute>()
                             where attr != null
                             let code = (ExitCode)field.GetValue(null)!
                             where exitCodePredicate(code)
                             let decimalCode = ((int)code).ToString()
                             let hexCode = "0x" + Convert.ToString((int)code, 16).PadLeft(8, '0')
                             select (decimalCode, hexCode, description: attr.Description)).ToArray();
            Console.WriteLine();
            Console.WriteLine($"{title}:");
            if (exitCodes.Length == 0)
                Console.WriteLine("  In this category no exit codes are returned.");
            else
                Console.WriteLine(exitCodes.ToColumnsString(x => x.decimalCode, x => x.hexCode, x => x.description));
        }

        public class RunnerOptions : IRunnerOptions
        {
            private readonly Dictionary<Type, object> _optionHandlers = new Dictionary<Type, object>();

            public int Year { get; set; } = 2020;
            public string Author { get; set; } = "Marc Schmidt";
            public string? DisplayName { get; set; }
            public Version Version { get; set; } = Program.Version;

            public void AddOptionHandler<T>(Func<T, ExitCode> handler) => _optionHandlers.Add(typeof(T), Guard.NotNull(handler, nameof(handler)));
            public Func<T, ExitCode> GetOptionHanlder<T>() => _optionHandlers.TryGetValue(typeof(T), out var r) ? (Func<T, ExitCode>)r : throw new KeyNotFoundException($"A option handler for type \"{typeof(T).FullName}\" was not found.");
            public Type[] GetHandeledOptions() => _optionHandlers.Keys.ToArray();
        }
    }

    [Verb("exitcodes", HelpText = "Display exit codes that this tool can return.")]
    public class ExitCodeOptions { }
}
