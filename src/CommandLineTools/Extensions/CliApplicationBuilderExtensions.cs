using MaSch.CommandLineTools.Common;
using MaSch.Console.Cli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;

namespace MaSch.CommandLineTools.Extensions
{
    public static class CliApplicationBuilderExtensions
    {
        public static CliApplicationBuilder WithTool<TTool>(this CliApplicationBuilder builder)
            where TTool : CltToolBase
        {
            var toolType = typeof(TTool);
            var toolAttr = toolType.GetCustomAttribute<CltToolAttribute>(true);
            if (toolAttr is null)
                throw new InvalidOperationException($"The type \"{toolType.FullName}\" needs to have a CltToolAttribute.");

            var command = builder.CommandFactory.Create<TTool>();

            if (toolAttr.WriteExitCodesMethodName is not null)
            {
                command.AddChildCommand(builder.CommandFactory.Create<ExitCodeCommand>());
                builder.ConfigureServices(s => s.TryAddScoped<ExitCodeCommand>());
            }

            builder.WithCommand(command);

            if (toolAttr.CreatorMethodName is not null)
            {
                var mi = toolType.GetMethod(toolAttr.CreatorMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                mi?.Invoke(null, new object[] { builder });
            }

            if (toolType.IsAssignableTo(typeof(IHelpPageMutator)))
                builder.ConfigureServices(s => s.AddScoped<IHelpPageMutator>(x => (IHelpPageMutator)x.GetRequiredService(toolType)));

            return builder;
        }
    }
}
