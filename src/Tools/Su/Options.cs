using CommandLine;

namespace MaSch.CommandLineTools.Tools.Su
{
    [Verb("run", isDefault: true, HelpText = "Runs a new elevated command line process inside the current terminal instance.")]
    public class RunOptions
    {
        [Option('t', "tool", Required = false, HelpText = "The command line tool to start (default is the tool that started this process - falls back to 'powershell' if parent process cannot be retrieved).")]
        public string? CommandLineToolName { get; set; }
    }
}
