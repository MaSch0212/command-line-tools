using MaSch.Console;
using MaSch.Console.Cli.Runtime;
using MaSch.Core;
using System;

namespace MaSch.CommandLineTools
{
    public abstract class ToolCommandBase : ICliCommandExecutor
    {
        protected internal static bool IsExecutedFromScript
            => string.Equals(Environment.GetEnvironmentVariable("MASCH_CLT_ISSCRIPTCONTEXT", EnvironmentVariableTarget.Process), "true", StringComparison.OrdinalIgnoreCase);

        protected IConsoleService Console { get; private set; }

        protected ToolCommandBase()
        {
            Console = ServiceContext.GetService<IConsoleService>();
        }

        public virtual int ExecuteCommand(CliExecutionContext context)
        {
            Console = context.Console;

            return OnExecuteCommand(context);
        }

        protected abstract int OnExecuteCommand(CliExecutionContext context);
    }
}
