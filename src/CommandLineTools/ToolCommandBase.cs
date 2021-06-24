using MaSch.Console;
using MaSch.Console.Cli.Runtime;
using MaSch.Core;

namespace MaSch.CommandLineTools
{
    public abstract class ToolCommandBase : ICliCommandExecutor
    {
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
