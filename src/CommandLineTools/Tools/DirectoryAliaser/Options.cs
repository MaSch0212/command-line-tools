using CommandLine;
using System;
using System.Collections.Generic;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser
{
    public interface ICopyOptions
    {
        string? Alias { get; set; }
        string? SubDirectory { get; set; }
    }

    [Verb("open", true, HelpText = "Sets the current directory to the directory that has a given alias.")]
    public class OpenOptions : ICopyOptions
    {
        [Value(0, Required = true, MetaName = "alias", HelpText = "The alias to open.", MetaValue = "STRING")]
        public string? Alias { get; set; }

        [Value(1, Required = false, MetaName = "subdirectory", HelpText = "The sub directory to open.", MetaValue = "STRING")]
        public string? SubDirectory { get; set; }

        [Option("from-script", Hidden = true)]
        public bool WasStartedFromScript { get; set; }
    }

    [Verb("copy", HelpText = "Sends the directory that has a given alias to the output stream.")]
    public class CopyOptions : ICopyOptions
    {
        [Value(0, Required = true, MetaName = "alias", HelpText = "The alias to copy.", MetaValue = "STRING")]
        public string? Alias { get; set; }

        [Value(1, Required = false, MetaName = "subdirectory", HelpText = "The sub directory to copy.", MetaValue = "STRING")]
        public string? SubDirectory { get; set; }
    }

    [Verb("add", HelpText = "Add new directory aliases.")]
    public class AddOptions
    {
        [Value(0, Required = true, MetaName = "aliases", HelpText = "The aliases to add.", MetaValue = "STRING [STRING]...", Min = 1)]
        public IEnumerable<string>? Aliases { get; set; }

        [Option('d', "directory", Required = false, HelpText = "The directory to which the aliases should be added. By default the current directory is used.")]
        public string Directory { get; set; } = Environment.CurrentDirectory;
    }

    [Verb("remove", HelpText = "Remove existing directory aliases")]
    public class RemoveOptions
    {
        [Value(0, Required = false, MetaName = "aliases", HelpText = "The aliases to remove. If none are given, all aliases for the given directory are removed.", MetaValue = "STRING [STRING]...")]
        public IEnumerable<string>? Aliases { get; set; }

        [Option('d', "directory", Required = false, HelpText = "The directory from which the aliases should be removed. By default the current directory is used.")]
        public string? Directory { get; set; }
    }

    [Verb("list", HelpText = "List all directory aliases")]
    public class ListOptions
    {
    }

    [Verb("install", HelpText = "Adds all directory aliases as environment variables starting with 'cdx_'")]
    public class InstallOptions
    {
    }

    [Verb("uninstall", HelpText = "Removes all directory aliases from environment variables")]
    public class UninstallOptions
    {
    }

    [Verb("config", HelpText = "Configures the directory aliaser")]
    public class ConfigOptions
    {
        [Option('a', "auto-var", Required = false, HelpText = "Sets a value indicating whether an environment variable should be created when adding a new directory alias.")]
        public bool? AutoInstallVariable { get; set; }
    }
}
