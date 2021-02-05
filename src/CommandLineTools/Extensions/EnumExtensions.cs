using MaSch.CommandLineTools.Common;

namespace MaSch.CommandLineTools.Extensions
{
    public static class EnumExtensions
    {
        public static Tool? ToTool(this string toolName)
        {
            return toolName.ToLower() switch
            {
                var x when x == "powershell" || x == "pwsh" || x == "ps" => Tool.PowerShell,
                "cmd" => Tool.Cmd,
                _ => (Tool?)null,
            };
        }
    }
}
