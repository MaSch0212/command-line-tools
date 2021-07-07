using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.CommandAliaser.Commands;
using MaSch.CommandLineTools.Tools.CommandAliaser.Services;
using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Console.Cli.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace MaSch.CommandLineTools.Tools.CommandAliaser
{
    [CliCommand("alias", HelpText = "Manages command aliases. Aliases to execute commands can be added, removed and listed.")]
    [CliMetadata(DisplayName = "Command Aliaser", Version = "1.1.0", Author = "Marc Schmidt", Year = "2021")]
    [CltTool(nameof(RegisterSubCommands), nameof(WriteExitCodeInfo))]
    public class CommandAliaserTool : CltToolBase
    {
        public static readonly string CommandsPath = Path.Combine(AppContext.BaseDirectory, "commands");
        public static readonly string CommandsFileName = ".commands.json";

        public static void RegisterSubCommands(CliApplicationBuilder builder)
        {
            builder.ConfigureServices(s =>
                s.AddSingleton<ITemplateService, TemplateService>()
                 .AddSingleton<ICommandsService, CommandsService>());

            builder.WithCommand<ListCommand>()
                   .WithCommand<AddCommand>()
                   .WithCommand<RemoveCommand>()
                   .WithCommand<InstallCommand>()
                   .WithCommand<UninstallCommand>()
                   .WithCommand<CleanupCommand>();
        }

        public static void WriteExitCodeInfo(IConsoleService console)
        {
            WriteCommonExitCodes(console);
            WriteExitCodeList(console, "List (Default)", ExitCode.AliasList);
            WriteExitCodeList(console, "Add", ExitCode.AliasAdd);
            WriteExitCodeList(console, "Remove", ExitCode.AliasRemove);
            WriteExitCodeList(console, "Install", ExitCode.AliasInstall);
            WriteExitCodeList(console, "Uninstall", ExitCode.AliasUninstall);
            WriteExitCodeList(console, "Cleanup", ExitCode.AliasCleanup);
        }
    }
}
