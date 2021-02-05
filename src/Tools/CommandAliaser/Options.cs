using CommandLine;
using System.Collections.Generic;

namespace MaSch.CommandLineTools.Tools.CommandAliaser
{
    public interface IScopedOptions
    {
        bool Global { get; set; }
        bool Local { get; set; }
        bool User { get; set; }
    }

    [Verb("list", true, HelpText = "Lists all command aliases.")]
    public class ListOptions : IScopedOptions
    {
        [Option('g', "global", HelpText = "Only show global commands.")]
        public bool Global { get; set; }

        [Option('l', "local", HelpText = "Only show local commands.")]
        public bool Local { get; set; }

        [Option('u', "user", HelpText = "Only show user commands.")]
        public bool User { get; set; }
    }

    [Verb("add", HelpText = "Add new command alias.")]
    public class AddOptions : IScopedOptions
    {
        [Value(0, Required = true, MetaName = "alias", HelpText = "The alias to add.", MetaValue = "STRING")]
        public string? Alias { get; set; }

        [Value(1, Required = false, MetaName = "command", HelpText = "The command to execute for the alias.", MetaValue = "STRING")]
        public string? Command { get; set; }

        [Option('d', "description", HelpText = "The Description for the command (shown when listing all commands).")]
        public string? Description { get; set; }

        [Option('g', "global", SetName = "global", HelpText = "Add the command as a global command.")]
        public bool Global { get; set; }

        [Option('l', "local", SetName = "local", HelpText = "Add the command as a local command.")]
        public bool Local { get; set; }

        [Option('u', "user", SetName = "user", HelpText = "Add the command as a user command.")]
        public bool User { get; set; }

        [Option('t', "tool", HelpText = "The tool to use for this command. If not provided it is assumed that the command can be executed in all tools.")]
        public string? Tool { get; set; }

        [Option('f', "force", HelpText = "Forces the creation of the alias even if it already exists.")]
        public bool Force { get; set; }
    }

    [Verb("remove", HelpText = "Remove existing command alias.")]
    public class RemoveOptions : IScopedOptions
    {
        [Value(0, Required = true, MetaName = "alias", HelpText = "The alias to remove.", MetaValue = "STRING")]
        public string? Alias { get; set; }

        [Option('g', "global", SetName = "single", HelpText = "Remove from global commands.")]
        public bool Global { get; set; }

        [Option('l', "local", SetName = "single", HelpText = "Remove from local commands.")]
        public bool Local { get; set; }

        [Option('u', "user", SetName = "single", HelpText = "Remove from user commands.")]
        public bool User { get; set; }

        [Option('f', "force", HelpText = "Forces the deletion of the alias.")]
        public bool Force { get; set; }
    }

    [Verb("install", HelpText = "Adjusts the Path variable to include aliases.")]
    public class InstallOptions : IScopedOptions
    {
        [Option('g', "global", SetName = "global", HelpText = "Install global path.")]
        public bool Global { get; set; }

        [Option('l', "local", SetName = "local", HelpText = "Install local path.")]
        public bool Local { get; set; }

        [Option('u', "user", SetName = "user", HelpText = "Install user path.")]
        public bool User { get; set; }
    }

    [Verb("uninstall", HelpText = "Remove the aliases paths from the Path variable.")]
    public class UninstallOptions : IScopedOptions
    {
        [Option('g', "global", SetName = "global", HelpText = "Uninstall global path.")]
        public bool Global { get; set; }

        [Option('l', "local", SetName = "local", HelpText = "Uninstall local path.")]
        public bool Local { get; set; }

        [Option('u', "user", SetName = "user", HelpText = "Uninstall user path.")]
        public bool User { get; set; }
    }

    [Verb("cleanup", HelpText = "Cleans up the command folders. Recreates script files that should exist and removes those that should not.")]
    public class CleanupOptions : IScopedOptions
    {
        [Option('g', "global", SetName = "global", HelpText = "Clean up global path.")]
        public bool Global { get; set; }

        [Option('l', "local", SetName = "local", HelpText = "Clean up local path.")]
        public bool Local { get; set; }

        [Option('u', "user", SetName = "user", HelpText = "Clean up user path.")]
        public bool User { get; set; }
    }
}
