using System;
using System.IO;
using System.Linq;
using CommandLine;
using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.Console;
using MaSch.Core.Extensions;
using Newtonsoft.Json;
using static System.Environment;
using ExitCode = MaSch.CommandLineTools.Common.ExitCode;

namespace MaSch.CommandLineTools.Tools.DirectoryAliaser
{
    [Verb("cdx", HelpText = "Manages directory aliases. Aliases to directories can be added, removed and requested.")]
    public class DirectoryAliaserRunner : BaseToolRunnerWithArgsParser
    {
        private static readonly string ConfigFilePath = Path.Combine(Program.ConfigurationDirectory, "cdx.json");
        private static readonly char[] IllegalAliasCharacters = new[] { '\\', '/' };
        private Configuration _config;
        private bool _configChanged;

        public DirectoryAliaserRunner()
        {
            _config = new Configuration();
        }

        protected override void OnConfigure(RunnerOptions options)
        {
            options.Author = "Marc Schmidt";
            options.Year = 2020;
            options.DisplayName = "Directory Aliaser";
            options.Version = new Version(1, 2, 0);

            options.AddOptionHandler<OpenOptions>(HandleOpen);
            options.AddOptionHandler<CopyOptions>(HandleCopy);
            options.AddOptionHandler<AddOptions>(HandleAdd);
            options.AddOptionHandler<RemoveOptions>(HandleRemove);
            options.AddOptionHandler<ListOptions>(HandleList);
            options.AddOptionHandler<InstallOptions>(HandleInstall);
            options.AddOptionHandler<UninstallOptions>(HandleUninstall);
            options.AddOptionHandler<ConfigOptions>(HandleConfig);
        }

        protected override void ConfigureParser(ParserSettings options)
        {
            base.ConfigureParser(options);
            options.IgnoreUnknownArguments = true;
        }

        protected override ExitCode OnParsed(object options)
        {
            if (File.Exists(ConfigFilePath))
                _config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigFilePath));

            try
            {
                return base.OnParsed(options);
            }
            finally
            {
                if (_configChanged)
                    File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
        }

        protected override void OnHandleExitCodes()
        {
            WriteCommonExitCodes();
            WriteExitCodeList("Open (Default)", ExitCode.CdxOpen);
            WriteExitCodeList("Copy", ExitCode.CdxCopy);
            WriteExitCodeList("Add", ExitCode.CdxAdd);
            WriteExitCodeList("Remove", ExitCode.CdxRemove);
            WriteExitCodeList("List", ExitCode.CdxList);
            WriteExitCodeList("Install", ExitCode.CdxInstall);
            WriteExitCodeList("Uninstall", ExitCode.CdxUninstall);
            WriteExitCodeList("Config", ExitCode.CdxConfig);
        }

        private ExitCode HandleOpen(OpenOptions options)
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), "cdx-open.tmp");
            return HandleICopy(options, ExitCode.CdxOpenMissingAlias, ExitCode.CdxOpenAliasNotFound, x =>
            {
                File.WriteAllText(tmpFile, x);
                if (!options.WasStartedFromScript)
                {
                    Console.WriteLineWithColor(
                        "This open command was not executed with a supported script. " +
                        "Changing the current directory in a console/terminal session is only possible with a compatible script. " +
                        "If you want to write your own script, add the '--from-script' parameter and handle the directory change using the path written " +
                        $"to the file 'cdx-open.tmp' in the temp path of the current user (currently \"{Path.GetTempPath()}\").", ConsoleColor.Red);
                    Console.WriteLineWithColor("If you just wanted to show the path behind the given alias, use the 'cdx copy' command.", ConsoleColor.Yellow);
                    return ExitCode.CdxOpenNotFromSupportedScript;
                }

                return ExitCode.Okay;
            });
        }

        private ExitCode HandleCopy(CopyOptions options) => HandleICopy(options, ExitCode.CdxCopyMissingAlias, ExitCode.CdxCopyAliasNotFound, x =>
        {
            Console.WriteLine(x);
            return ExitCode.Okay;
        });

        private ExitCode HandleICopy(ICopyOptions options, ExitCode missingAliasCode, ExitCode aliasNotFoundCode, Func<string, ExitCode> action)
        {
            if (string.IsNullOrWhiteSpace(options.Alias))
            {
                Console.WriteLineWithColor($"An alias needs to be provided.{NewLine}", ConsoleColor.Red);
                PrintList();
                return missingAliasCode;
            }

            var aliasSplit = options.Alias.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var alias = aliasSplit[0];
            var subDir = Path.Combine(aliasSplit.Skip(1).Append(options.SubDirectory ?? string.Empty).ToArray());

            var path = GetPathFromAlias(alias);
            if (path == null)
            {
                Console.WriteLineWithColor($"The alias \"{alias}\" is not registered to any directory.{NewLine}", ConsoleColor.Red);
                PrintList();
                return aliasNotFoundCode;
            }

            return action(Path.Combine(path.Path, subDir));
        }

        private ExitCode HandleAdd(AddOptions options)
        {
            if (options.Aliases.IsNullOrEmpty())
                throw new ApplicationExitException(ExitCode.CdxAddMissingAlias, "At least one alias needs to be provided.");
            if (string.IsNullOrWhiteSpace(options.Directory))
                throw new ApplicationExitException(ExitCode.CdxAddMissingDirectory, "A directory needs to be provided.");

            var path = GetPathFromPath(options.Directory);
            if (path == null)
            {
                path = new PathConfiguration(options.Directory);
                _config.Paths.Add(path);
            }

            var invalidAliases = options.Aliases.Where(x => x.Intersect(IllegalAliasCharacters).Any()).ToArray();
            if (invalidAliases.Length > 0)
            {
                Console.WriteLineWithColor($"The following aliases contain illeagl characters: \"{string.Join("\", \"", invalidAliases)}\"", ConsoleColor.Red);
                Console.WriteLineWithColor($"Illegal characters are: {string.Join(" ", IllegalAliasCharacters)}", ConsoleColor.Yellow);
                return ExitCode.CdxAddInvalidAliasName;
            }

            foreach (var alias in options.Aliases)
            {
                var p = GetPathFromAlias(alias);
                if (p != null)
                {
                    Console.WriteLineWithColor($"The alias \"{alias}\" is already registered with path \"{p.Path}\".", ConsoleColor.Yellow);
                }
                else
                {
                    path.Aliases.Add(alias);
                    _configChanged = true;
                    Console.WriteLineWithColor($"The alias \"{alias}\" was registered to path \"{path.Path}\".", ConsoleColor.Green);
                    if (_config.IsAutoVariableEnabled)
                        AddEnvironmentVariable(alias, path.Path);
                }
            }

            return ExitCode.Okay;
        }

        private ExitCode HandleRemove(RemoveOptions options)
        {
            PathConfiguration? path = null;
            if (!string.IsNullOrWhiteSpace(options.Directory) || options.Aliases.IsNullOrEmpty())
            {
                path = GetPathFromPath(options.Directory ?? CurrentDirectory);
                if (path == null)
                {
                    Console.WriteLineWithColor($"The path \"{options.Directory}\" does not have any registered aliases.", ConsoleColor.Yellow);
                    return ExitCode.Okay;
                }
            }

            if (options.Aliases.IsNullOrEmpty())
            {
                _config.Paths.Remove(path!);
                _configChanged = true;
                Console.WriteLineWithColor($"All aliases have been removed from \"{path!.Path}\".", ConsoleColor.Green);
            }
            else
            {
                foreach (var alias in options.Aliases)
                {
                    var p = path ?? GetPathFromAlias(alias);
                    var aliasIndex = p?.Aliases.IndexOf(x => string.Equals(x, alias, StringComparison.OrdinalIgnoreCase));
                    if (p == null || !aliasIndex.HasValue)
                    {
                        Console.WriteLineWithColor($"The alias \"{alias}\" is not reigstered to {(string.IsNullOrWhiteSpace(options.Directory) ? "any paths" : $"path \"{options.Directory}\"")}.", ConsoleColor.Yellow);
                    }
                    else
                    {
                        var realAlias = p.Aliases[aliasIndex.Value];
                        p.Aliases.RemoveAt(aliasIndex.Value);
                        _configChanged = true;
                        Console.WriteLineWithColor($"The alias \"{alias}\" was removed from path \"{p.Path}\".", ConsoleColor.Green);
                        if (_config.IsAutoVariableEnabled)
                            RemoveEnvironmentVariable(realAlias);
                    }
                }
            }

            return ExitCode.Okay;
        }

        private ExitCode HandleList(ListOptions options)
        {
            PrintList();
            return ExitCode.Okay;
        }

        private ExitCode HandleInstall(InstallOptions options)
        {
            foreach (var path in _config.Paths)
            {
                foreach (var alias in path.Aliases)
                {
                    AddEnvironmentVariable(alias, path.Path);
                }
            }

            return ExitCode.Okay;
        }

        private ExitCode HandleUninstall(UninstallOptions options)
        {
            foreach (var path in _config.Paths)
            {
                foreach (var alias in path.Aliases)
                {
                    RemoveEnvironmentVariable(alias);
                }
            }

            return ExitCode.Okay;
        }

        private ExitCode HandleConfig(ConfigOptions options)
        {
            if (options.AutoInstallVariable == null)
            {
                Console.WriteLine("The directory aliaser is configured as follows:");
                Console.WriteLine();
                Console.WriteLine(new (string name, string option, string value)[]
                    {
                        ("Name", "Option", "Value"),
                        ("----", "------", "-----"),
                        (nameof(options.AutoInstallVariable), "--auto-var (-a)", _config.IsAutoVariableEnabled.ToString()),
                    }
                    .ToColumnsString(x => x.name, x => x.option, x => x.value));
                Console.WriteLine();
                return ExitCode.Okay;
            }
            else
            {
                if (options.AutoInstallVariable != null)
                    _config.IsAutoVariableEnabled = options.AutoInstallVariable.Value;
                _configChanged = true;
            }

            return ExitCode.Okay;
        }

        private PathConfiguration? GetPathFromAlias(string alias) => _config.Paths.FirstOrDefault(x => x.Aliases.Contains(alias, StringComparer.OrdinalIgnoreCase));
        private PathConfiguration? GetPathFromPath(string path) => _config.Paths.FirstOrDefault(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase));

        private void PrintList()
        {
            if (_config.Paths.Count == 0)
            {
                using var scope = new ConsoleColorScope(Console, ConsoleColor.Yellow);
                Console.WriteLine("There are currently no aliases registered. You can register a new alias by using the 'add' verb.");
                Console.WriteLine("Execute 'cdx add --help' for more information.");
            }
            else
            {
                Console.WriteLine("The following aliases are available:");
                Console.WriteLine();
                Console.WriteLine(_config.Paths.OrderBy(x => x.Path).ToColumnsString(x => string.Join(", ", x.Aliases), x => x.Path));
                Console.WriteLine();
            }
        }

        private void AddEnvironmentVariable(string name, string path)
        {
            var varName = $"cdx_{name}";
            SetEnvironmentVariable(varName, path, EnvironmentVariableTarget.User);
            Console.WriteLineWithColor($"Environment variable \"{varName}\" has been added.", ConsoleColor.Green);
        }

        private void RemoveEnvironmentVariable(string name)
        {
            var varName = $"cdx_{name}";
            SetEnvironmentVariable(varName, null, EnvironmentVariableTarget.User);
            Console.WriteLineWithColor($"Environment variable \"{varName}\" has been removed.", ConsoleColor.Green);
        }
    }
}
