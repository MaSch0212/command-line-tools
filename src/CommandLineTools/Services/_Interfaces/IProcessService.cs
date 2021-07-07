using System;

namespace MaSch.CommandLineTools.Services
{
    public interface IProcessService
    {
        void KillAllStartedProcesses();
        int RunProcess(string processName, string parameters, Action<string?>? onNewOutput, Action<string?>? onNewError);
        int RunProcess(string processName, string parameters, string? workingDirectory, Action<string?>? onNewOutput, Action<string?>? onNewError);
    }
}