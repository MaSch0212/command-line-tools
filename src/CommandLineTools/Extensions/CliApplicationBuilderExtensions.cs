using MaSch.Console.Cli;
using MaSch.Console.Cli.Runtime;
using System;

namespace MaSch.CommandLineTools.Extensions
{
    public static class CliApplicationBuilderExtensions
    {
        private static readonly ICliCommandInfoFactory CommandInfoFactory = new CliCommandInfoFactory();

        public static CliApplicationBuilder WithTool<TTool>(this CliApplicationBuilder builder)
            where TTool : ICltTool
        {
            var toolInstance = Activator.CreateInstance<TTool>();
            var command = CommandInfoFactory.Create(toolInstance);
            command.AddChildCommand(CommandInfoFactory.Create<ExitCodeCommand>());
            builder.WithCommand(command);
            toolInstance.RegisterSubCommands(builder);
            return builder;
        }
    }
}
