using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text.RegularExpressions;
using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.CommandLineTools.Services;
using MaSch.Console;

namespace MaSch.CommandLineTools.Tools.Sudo.Services
{
    [SupportedOSPlatform("windows")]
    public class SudoService : ISudoService
    {
        private readonly IConsoleService _console;
        private readonly IParentProcessService _parentProcessService;
        private readonly IOsService _osService;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        public void VerifyAdminRole(string commandName, ExitCode failExitCode)
        {
            var adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Value;
            if (!WindowsIdentity.GetCurrent().UserClaims.Any(x => x.Value == adminSid))
                throw new ApplicationExitException(failExitCode, $"You must be an administrator to run {commandName}");
        }

        public SudoService(IConsoleService console, IParentProcessService parentProcessService, IOsService osService)
        {
            _console = console;
            _parentProcessService = parentProcessService;
            _osService = osService;
        }

        public ExitCode Run(string? tool, Func<TerminalTool, string> argsFunc)
        {
            var t = GetTool(tool, out string toolName, ExitCode.SuRunUnknownTool);
            return t switch
            {
                TerminalTool.PowerShell => Run(toolName, $"-noprofile -Command {argsFunc(t)} \nexit $LASTEXITCODE", ExitCode.SuRunUserDeclined),
                TerminalTool.Cmd => Run(toolName, $"/c {argsFunc(t)} && exit ^%ERRORLEVEL^%", ExitCode.SuRunUserDeclined),
                _ => ExitCode.SudoRunUnknownTool,
            };
        }

        public ExitCode Run(string fileName, string arguments, ExitCode declinedExitCode)
        {
            var parentProcessId = _parentProcessService.GetParentProcess()?.Id ?? Process.GetCurrentProcess().Id;
            if (_osService.IsRoot())
                return Do(fileName, arguments, parentProcessId);

            _console.CancelKeyPress += IgnoreCancel;

            int watcherId;
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = GetCommandLineToolsFile(),
                        ArgumentList =
                        {
                            "sudo", "do",
                            "-f", fileName,
                            "-p", parentProcessId.ToString(),
                            "-a", "--", arguments,
                        },
                        Verb = "runas",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    },
                };

                try
                {
                    process.Start();
                }
                catch (Win32Exception ex) when (ex.ErrorCode == -2147467259)
                {
                    return declinedExitCode;
                }

                process.WaitForExit();
                watcherId = process.ExitCode;

                process = Process.GetProcesses().FirstOrDefault(x => x.Id == watcherId);
                process?.WaitForExit();
            }
            finally
            {
                _console.CancelKeyPress -= IgnoreCancel;
            }

            var exitCodeFile = Path.Combine(Path.GetTempPath(), $"sudo-watch-{watcherId}.tmp");
            if (File.Exists(exitCodeFile))
            {
                var exitCode = int.Parse(File.ReadAllText(exitCodeFile));
                try
                {
                    File.Delete(exitCodeFile);
                }
                catch
                {
                }

                return (ExitCode)exitCode;
            }

            return ExitCode.Okay;

            static void IgnoreCancel(object? sender, ConsoleCancelEventArgs e) => e.Cancel = true;
        }

        public ExitCode Do(string fileName, string arguments, int parentProcessId)
        {
            FreeConsole();
            AttachConsole((uint)parentProcessId);

            var p1 = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
            })!;

            var p2 = Process.Start(new ProcessStartInfo
            {
                FileName = GetCommandLineToolsFile(),
                ArgumentList =
                {
                    "sudo", "watch",
                    "-p", p1.Id.ToString(),
                },
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            })!;

            if (p1.WaitForExit(2000))
            {
                p2.Kill();
                WriteExitCodeFile(p2.Id, p1.ExitCode);
            }

            return (ExitCode)p2.Id;
        }

        public string GetLastCmdCommand()
        {
            FreeConsole();
            AttachConsole((uint)(_parentProcessService.GetParentProcess()?.Id ?? Environment.ProcessId));

            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "doskey",
                Arguments = "/HISTORY",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            })!;
            p.WaitForExit();

            var history = Regex.Split(p.StandardOutput.ReadToEnd(), "[\\r\\n]+").Where(x => !string.IsNullOrEmpty(x)).ToArray();
            return history.Skip(history.Length - 2).First();
        }

        public TerminalTool GetTool(string? toolName, out string actualToolName, ExitCode unknownExitCode)
        {
            actualToolName = toolName ?? _parentProcessService.GetParentProcess()?.ProcessName ?? "powershell";
            return actualToolName.ToTool() ?? throw new ApplicationExitException(unknownExitCode, $"The tool \"{actualToolName}\" is unknown and currently cannot be used for su or sudo.");
        }

        public void WriteExitCodeFile(int watchPid, int exitCode)
        {
            try
            {
                File.WriteAllText(GetExitCodeFile(watchPid), exitCode.ToString());
            }
            catch
            {
            }
        }

        private static string GetExitCodeFile(int watchPid)
            => Path.Combine(Path.GetTempPath(), $"sudo-watch-{watchPid}.tmp");

        private static string GetCommandLineToolsFile()
        {
            return Path.Combine(AppContext.BaseDirectory, Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + ".exe");
        }
    }
}
