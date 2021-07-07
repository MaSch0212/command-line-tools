using MaSch.CommandLineTools.Common;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Services
{
    public interface ITemplateService
    {
        string GetCmdCommandScript(string alias, string command, string description, TerminalTool? targetTool);
        string GetPowerShellCommandScript(string alias, string command, string description, TerminalTool? targetTool);
    }
}