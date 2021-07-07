using MaSch.CommandLineTools.Common;
using System;

namespace MaSch.CommandLineTools.Tools.Sudo.Services
{
    public interface ISudoService
    {
        ExitCode Do(string fileName, string arguments, int parentProcessId);
        string GetLastCmdCommand();
        TerminalTool GetTool(string? toolName, out string actualToolName, ExitCode unknownExitCode);
        ExitCode Run(string fileName, string arguments, ExitCode declinedExitCode);
        ExitCode Run(string? tool, Func<TerminalTool, string> argsFunc);
        void VerifyAdminRole(string commandName, ExitCode failExitCode);
        void WriteExitCodeFile(int watchPid, int exitCode);
    }
}