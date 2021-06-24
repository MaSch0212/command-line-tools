using MaSch.Console.Cli;
using MaSch.Console.Cli.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Commands
{
    public abstract class CommandBase : ToolCommandBase, ICliValidatable
    {
        protected static readonly string GlobalScopeName = "Global";
        protected static readonly string LocalScopeName = "Local";
        protected static readonly string UserScopeName = "User";

        protected static readonly string GlobalCommandsPath = Path.Combine(CommandAliaserTool.CommandsPath, "global");
        protected static readonly string LocalCommandsPath = Path.Combine(CommandAliaserTool.CommandsPath, $"local-{Environment.MachineName.ToLower()}");
        protected static readonly string UserCommandsPath = Path.Combine(LocalCommandsPath, Environment.UserName.ToLower());
        protected static string CommandsFileName => CommandAliaserTool.CommandsFileName;

        public abstract bool IsScopeExcluse { get; }
        public abstract bool Global { get; set; }
        public abstract bool Local { get; set; }
        public abstract bool User { get; set; }

        public bool ValidateOptions(CliExecutionContext context, [MaybeNullWhen(true)] out IEnumerable<CliError> errors)
        {
            var errorList = new List<CliError>();
            var result = true;

            if (IsScopeExcluse)
            {
                var isFaulty = false;
                if (Global)
                    isFaulty = Local || User;
                else if (Local)
                    isFaulty = Global || User;
                else if (User)
                    isFaulty = Global || Local;

                if (isFaulty)
                {
                    errorList.Add(new CliError("The scope is mutually exclusive. You can only choose one of the scopes: global, local or user.", context.Command));
                    result = false;
                }
            }

            result &= OnValidateOptions(context, errorList);
            errors = errorList;
            return result;
        }

        protected virtual bool OnValidateOptions(CliExecutionContext context, IList<CliError> errors)
        {
            return true;
        }
    }
}
