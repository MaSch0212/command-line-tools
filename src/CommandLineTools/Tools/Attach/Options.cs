using CommandLine;

#pragma warning disable SA1649 // File name should match first type name

namespace MaSch.CommandLineTools.Tools.Attach
{
    [Verb("run", isDefault: true, HelpText = "Attaches console output to a given process.")]
    public class RunOptions
    {
        [Option('p', "process", Required = true, HelpText = "The process to which to attach.")]
        public int ProcessId { get; set; }

        [Option('f', "file", Required = true, HelpText = "The file to execute.")]
        public string FileName { get; set; } = string.Empty;

        [Option('a', "arguments", Required = false, HelpText = "The arguments to pass to the file to execute.")]
        public string Arguments { get; set; } = string.Empty;
    }
}
