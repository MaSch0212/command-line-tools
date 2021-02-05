using CommandLine;
using System.Collections.Generic;

namespace MaSch.CommandLineTools.Tools.Sudo
{
    [Verb("run", isDefault: true, HelpText = "Runs the given command in an elevated process redirecting output to the current terminal instance.")]
    public class RunOptions
    {
        [Option("tool", Required = false, HelpText = "The command line tool to start (default is the tool that started this process - falls back to 'powershell' if parent process cannot be retrieved).")]
        public string? CommandLineToolName { get; set; }

        [Value(0, Required = true, Min = 1, MetaName = "command", MetaValue = "STRING [STRING]...")]
        public IEnumerable<string> Commands { get; set; } = new string[0];
    }

    [Verb("do", isDefault: false, Hidden = true)]
    public class DoOptions
    {
        [Option('f', Required = true)]
        public string FileName { get; set; } = string.Empty;

        [Option('a', Required = false)]
        public string Arguments { get; set; } = string.Empty;

        [Option('p', Required = true)]
        public int ParentProcessId { get; set; }
    }

    [Verb("watch", isDefault: false, Hidden = true)]
    public class WatchOptions
    {
        [Option('p', Required = true)]
        public int ProcessId { get; set; }
    }

    [Verb("please", isDefault: false, HelpText = "Repeats the last command of this command line as su.")]
    public class RepeatOptions
    {
        [Option('t', "tool", Required = false, HelpText = "The command line tool to start (default is the tool that started this process - falls back to 'powershell' if parent process cannot be retrieved).")]
        public string? CommandLineToolName { get; set; }

        [Option("command", Required = false, Hidden = true)]
        public string? Command { get; set; }
    }
}
