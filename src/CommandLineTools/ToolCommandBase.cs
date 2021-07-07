using MaSch.Console.Cli.Runtime;
using System;

namespace MaSch.CommandLineTools
{
    public abstract class ToolCommandBase : ICliExecutable
    {
        protected internal static bool IsExecutedFromScript
            => string.Equals(Environment.GetEnvironmentVariable("MASCH_CLT_ISSCRIPTCONTEXT", EnvironmentVariableTarget.Process), "true", StringComparison.OrdinalIgnoreCase);

        public virtual int ExecuteCommand(CliExecutionContext context)
        {
            return OnExecuteCommand(context);
        }

        protected abstract int OnExecuteCommand(CliExecutionContext context);
    }
}
