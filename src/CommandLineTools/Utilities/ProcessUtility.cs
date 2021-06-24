using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MaSch.CommandLineTools.Utilities
{
    public static class ProcessUtility
    {
        private static readonly List<Process> _runningProcesses = new();

        public static int RunProcess(string processName, string parameters, Action<string?>? onNewOutput, Action<string?>? onNewError)
            => RunProcess(processName, parameters, null, onNewOutput, onNewError);

        public static int RunProcess(string processName, string parameters, string? workingDirectory, Action<string?>? onNewOutput, Action<string?>? onNewError)
        {
            if (string.IsNullOrEmpty(workingDirectory))
                workingDirectory = Environment.CurrentDirectory;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(processName, parameters)
                {
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };
            var output = new StringBuilder();
            process.OutputDataReceived += (s, e) => onNewOutput?.Invoke(e.Data);
            process.ErrorDataReceived += (s, e) => onNewError?.Invoke(e.Data);

            try
            {
                _runningProcesses.Add(process);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            finally
            {
                _runningProcesses.Remove(process);
            }

            return process.ExitCode;
        }

        public static void KillAllStartedProcesses()
        {
            foreach (var p in _runningProcesses.ToArray())
            {
                try
                {
                    p.Kill();
                    _runningProcesses.Remove(p);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error while stopping process with id {p?.Id}: " + ex);
                }
            }
        }
    }
}
