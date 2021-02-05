using MaSch.CommandLineTools.Common;
using MaSch.Core.Extensions;
using System;
using System.Linq;
using CommandLine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MaSch.CommandLineTools.Tools.Sudo
{
    [Verb("sudo", HelpText = "Executes a commands in an elevated process.")]
    public class SudoRunner : BaseToolRunnerWithArgsParser
    {
        protected override void OnConfigure(RunnerOptions options)
        {
            options.DisplayName = "Super User Do";
            options.Version = new Version(1, 0, 4);
            options.Author = "Marc Schmidt";
            options.Year = 2020;

            options.AddOptionHandler<RunOptions>(HandleRun);
            options.AddOptionHandler<DoOptions>(HandleDo);
            options.AddOptionHandler<WatchOptions>(HandleWatch);
            options.AddOptionHandler<RepeatOptions>(HandleRepeat);
        }

        protected override void ConfigureParser(ParserSettings options)
        {
            base.ConfigureParser(options);
            options.IgnoreUnknownArguments = true;
        }

        protected override void OnHandleExitCodes()
        {
            WriteCommonExitCodes();
            WriteExitCodeList("Run (Default)", ExitCode.SudoRun);
            Console.WriteLine("  If no error occures the exit code of the called command is returned.");
            WriteExitCodeList("Please", ExitCode.SudoPlease);
            Console.WriteLine("  If no error occures the exit code of the called command is returned.");
        }

        private ExitCode HandleRun(RunOptions options)
        {
            SudoController.VerifyAdminRole("sudo", ExitCode.SudoRunNoAdmin);

            var allArgs = Environment.GetCommandLineArgs();
            IEnumerable<string> args = allArgs.Skip(string.Equals(allArgs[2], "run", StringComparison.OrdinalIgnoreCase) ? 3 : 2);

            var ddIdx = args.IndexOf("--");
            var tIdx = args.IndexOf(x => string.Equals(x, "--tool", StringComparison.OrdinalIgnoreCase));
            if (ddIdx >= 0)
                args = args.Take(ddIdx).Concat(args.Skip(ddIdx + 1));
            if (tIdx >= 0 && (ddIdx < 0 || tIdx < ddIdx))
                args = args.Take(tIdx).Concat(args.Skip(tIdx + 2));

            return Run(options.CommandLineToolName, x => x switch
            {
                Tool.PowerShell => string.Join(" ", args.Select(SerializePS)),
                _ => string.Join(" ", args.Select(SerializeCmd))
            });

            static string SerializePS(string a)
            {
                if (Regex.IsMatch(a, @"\s"))
                    return $"'{a}'";
                return Regex.Replace(a, "[>&]", "`$0");
            }

            static string SerializeCmd(string a)
            {
                if (Regex.IsMatch(a, @"\s"))
                    return $"\"{a}\"";
                return a;
            }
        }

        private ExitCode HandleDo(DoOptions options)
        {
            return SudoController.Do(options.FileName, options.Arguments, options.ParentProcessId);
        }

        private ExitCode HandleWatch(WatchOptions options)
        {
            var process = Process.GetProcessById(options.ProcessId);
            process.EnableRaisingEvents = true;
            process.WaitForExit();
            SudoController.WriteExitCodeFile(Process.GetCurrentProcess().Id, process.ExitCode);
            return ExitCode.Okay;
        }

        private ExitCode HandleRepeat(RepeatOptions options)
        {
            SudoController.VerifyAdminRole("sudo", ExitCode.SudoRunNoAdmin);

            return Run(options.CommandLineToolName, x => x switch
            {
                Tool.Cmd => options.Command ?? SudoController.GetLastCmdCommand(),
                _ => options.Command ?? "Write-Host 'Last command could not be determined!' -Foreground Red"
            });
        }

        private ExitCode Run(string? tool, Func<Tool, string> argsFunc)
        {
            var t = SudoController.GetTool(tool, out string toolName, ExitCode.SuRunUnknownTool);
            return t switch
            {
                Tool.PowerShell => SudoController.Run(toolName, $"-noprofile -Command {argsFunc(t)} \nexit $LASTEXITCODE", ExitCode.SuRunUserDeclined),
                Tool.Cmd => SudoController.Run(toolName, $"/c {argsFunc(t)} && exit ^%ERRORLEVEL^%", ExitCode.SuRunUserDeclined),
                _ => ExitCode.SudoRunUnknownTool
            };
        }
    }
}
