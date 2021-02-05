using CommandLine;
using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.Sudo;
using System;

namespace MaSch.CommandLineTools.Tools.Attach
{
    [Verb("attach", isDefault: false, HelpText = "Attaches console output to a given process.")]
    public class AttachRunner : BaseToolRunnerWithArgsParser
    {
        protected override void OnConfigure(RunnerOptions options)
        {
            options.DisplayName = "Attach";
            options.Version = new Version(1, 0, 0);
            options.Author = "Marc Schmidt";
            options.Year = 2020;

            options.AddOptionHandler<RunOptions>(HandleRun);
        }

        protected override void OnHandleExitCodes()
        {
            WriteCommonExitCodes();
            WriteExitCodeList("Run (Default)", ExitCode.AttachRun);
        }

        private ExitCode HandleRun(RunOptions options)
        {
            return SudoController.Do(options.FileName, options.Arguments, options.ProcessId);
        }
    }
}
