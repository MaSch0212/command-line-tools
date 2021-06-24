using MaSch.CommandLineTools.Common;
using MaSch.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser.Commands
{
    public abstract class CopyCommandBase : CommandBase, ICopyCommand
    {
        public abstract string? Alias { get; set; }
        public abstract string? SubDirectory { get; set; }

        protected ExitCode HandleCopy(ExitCode missingAliasCode, ExitCode aliasNotFoundCode, Func<string, ExitCode> action)
        {
            if (string.IsNullOrWhiteSpace(Alias))
            {
                Console.WriteLineWithColor($"An alias needs to be provided.{Environment.NewLine}", ConsoleColor.Red);
                PrintList();
                return missingAliasCode;
            }

            var aliasSplit = Alias.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var alias = aliasSplit[0];
            var subDir = Path.Combine(aliasSplit.Skip(1).Append(SubDirectory ?? string.Empty).ToArray());

            var path = GetPathFromAlias(alias);
            if (path == null)
            {
                Console.WriteLineWithColor($"The alias \"{alias}\" is not registered to any directory.{Environment.NewLine}", ConsoleColor.Red);
                PrintList();
                return aliasNotFoundCode;
            }

            return action(Path.Combine(path.Path, subDir));
        }
    }

    public interface ICopyCommand
    {
        string? Alias { get; set; }
        string? SubDirectory { get; set; }
    }
}
