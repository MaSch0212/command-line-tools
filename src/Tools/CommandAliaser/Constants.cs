using System;
using System.IO;

namespace MaSch.CommandLineTools.Tools.CommandAliaser
{
    public static class Constants
    {
        public static readonly string GlobalScopeName = nameof(IScopedOptions.Global);
        public static readonly string LocalScopeName = nameof(IScopedOptions.Local);
        public static readonly string UserScopeName = nameof(IScopedOptions.User);

        public static readonly string CommandsPath = Path.Combine(AppContext.BaseDirectory, "commands");
        public static readonly string GlobalCommandsPath = Path.Combine(CommandsPath, "global");
        public static readonly string LocalCommandsPath = Path.Combine(CommandsPath, $"local-{Environment.MachineName.ToLower()}");
        public static readonly string UserCommandsPath = Path.Combine(LocalCommandsPath, Environment.UserName.ToLower());
        public static readonly string CommandsFileName = ".commands.json";
    }
}
