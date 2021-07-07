using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Tools.CommandAliaser.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MaSch.CommandLineTools.Tools.CommandAliaser.Services
{
    public interface ICommandsService
    {
        bool DeleteScriptFile(string filePath);
        bool DeleteScriptFile(string path, string alias, TerminalTool scriptTool);
        string GetScriptFilePath(string path, string? alias, TerminalTool tool);
        Dictionary<string, Command> LoadCommands(string path);
        bool SaveCommands(string path, Dictionary<string, Command> commands);
        bool TryGetCommand(Dictionary<string, Command> commands, string? alias, [NotNullWhen(true)] out Command? command);
        bool WriteScriptFile(string path, string? alias, TerminalTool scriptTool, string? command, string? description, TerminalTool? commandTool);
    }
}