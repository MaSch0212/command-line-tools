using System;
using System.Diagnostics;

namespace MaSch.CommandLineTools.Services
{
    public interface IParentProcessService
    {
        Process? GetParentProcess();
        Process? GetParentProcess(int id);
        Process? GetParentProcess(IntPtr handle);
    }
}