using MaSch.CommandLineTools.Common;
using System;

namespace MaSch.CommandLineTools
{
    public interface IToolRunner
    {
        IRunnerOptions Options { get; }

        ExitCode Run(string[] args);
    }

    public interface IRunnerOptions
    {
        int Year { get; set; }
        string Author { get; set; }
        string? DisplayName { get; set; }
        Version Version { get; set; }
    }
}
