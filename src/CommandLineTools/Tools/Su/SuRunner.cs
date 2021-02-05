using CommandLine;
using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.Sudo;
using MaSch.CommandLineTools.Utilities;
using System;

namespace MaSch.CommandLineTools.Tools.Su
{
    [Verb("su", HelpText = "Elevates the current command line to administrative rights.")]
    public class SuRunner : BaseToolRunnerWithArgsParser
    {
        protected override void OnConfigure(RunnerOptions options)
        {
            options.DisplayName = "Super User";
            options.Version = new Version(1, 0, 3);
            options.Author = "Marc Schmidt";
            options.Year = 2020;

            options.AddOptionHandler<RunOptions>(HandleRun);
        }

        protected override void OnHandleExitCodes()
        {
            WriteCommonExitCodes();
            WriteExitCodeList("Run (Default)", ExitCode.SuRun);
        }

        private ExitCode HandleRun(RunOptions options)
        {
            if (OsUtility.IsRoot())
                return ExitCode.SuRunAlreadyElevated;

            SudoController.VerifyAdminRole("su", ExitCode.SuRunNoAdmin);

            var tool = SudoController.GetTool(options.CommandLineToolName, out string toolName, ExitCode.SuRunUnknownTool);
            return tool switch
            {
                Tool.PowerShell => SudoController.Run(toolName, "-nologo", ExitCode.SuRunUserDeclined),
                Tool.Cmd => SudoController.Run(toolName, "/k", ExitCode.SuRunUserDeclined),
                _ => ExitCode.SuRunUnknownTool
            };
        }
    }
}
