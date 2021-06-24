using MaSch.CommandLineTools.Common;

namespace MaSch.CommandLineTools.Extensions
{
    public static class EnumExtensions
    {
        public static TerminalTool? ToTool(this string toolName)
        {
            return toolName.ToLower() switch
            {
                var x when x == "powershell" || x == "pwsh" || x == "ps" => TerminalTool.PowerShell,
                "cmd" => TerminalTool.Cmd,
                _ => (TerminalTool?)null,
            };
        }
    }
}
