using MaSch.CommandLineTools.Services;
using MaSch.Console;
using MaSch.Console.Cli.Runtime;
using MaSch.Console.Controls;
using MaSch.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MaSch.CommandLineTools.Tools.RobocopyRunner
{
    public class RobocopyExecutor : ICliExecutor<IRobocopyOptions>
    {
        private static readonly Regex FileRegex = new(@"(?<size>(?<=\s+)\d+(?=\t))\s+(?<file>.+(?=$))", RegexOptions.Compiled);
        private static readonly Regex PercentRegex = new(@"[\d\.\,]+(?=%)", RegexOptions.Compiled);

        private readonly IConsoleService _console;
        private readonly IProcessService _processService;

        public RobocopyExecutor(IConsoleService console, IProcessService processService)
        {
            _console = console;
            _processService = processService;
        }

        public int ExecuteCommand(CliExecutionContext context, IRobocopyOptions parameters)
        {
            var allArgs = new StringBuilder();
            allArgs.Append($"\"{parameters.Source}\"");
            allArgs.Append($" \"{parameters.Destination}\"");

            var cmo = parameters as IRobocopyCopyMoveOptions;
            if (cmo != null)
            {
                if (!cmo.Files.IsNullOrEmpty())
                    allArgs.Append($" \"{string.Join("\" \"", cmo.Files)}\"");

                if (cmo is MoveOptions mo)
                    allArgs.Append($" /{(mo.ExcludeDirectories ? "mov" : "move")}");

                AddArg(allArgs, "z", cmo.CopyInRestartableMode);
                AddArg(allArgs, "b", cmo.CopyFilesInBackupoMode);
                AddArg(allArgs, "zb", cmo.CopyInRestartableOrBackupMode);
                AddArg(allArgs, "efsraw", cmo.CopyEncryptedInEfsRawMode);
                AddArg(allArgs, "copy", cmo.FilePropertiesToCopy);
                AddArg(allArgs, "sec", cmo.CopyFilesWithSecurity);
                AddArg(allArgs, "copyall", cmo.CopyAllFileInformation);
                AddArg(allArgs, "nocopy", cmo.CopyNoFileInformation);
                AddArg(allArgs, "secfix", cmo.FixFileSecurityForAll);
                AddArg(allArgs, "timfix", cmo.FixFileTimesForAll);
                AddArg(allArgs, "a+", cmo.FileAttributesToAdd);
                AddArg(allArgs, "a-", cmo.FileAttributesToRemove);
                AddArg(allArgs, "fat", cmo.UseFatFileNames);

                AddArg(allArgs, "a", cmo.CopyOnlyArchiveFiles);
                AddArg(allArgs, "m", cmo.CopyOnlyArchiveFilesAndReset);
                AddArg(allArgs, "ia", cmo.IncludedFileAttributes);
                AddArg(allArgs, "xa", cmo.ExcludedFileAttributes);
                AddArg(allArgs, "xf", cmo.ExcludedFiles);
                AddArg(allArgs, "xc", cmo.ExcludeChangedFiles);
                AddArg(allArgs, "xn", cmo.ExcludeNewerFiles);
                AddArg(allArgs, "xo", cmo.ExcludeOlderFiles);
                AddArg(allArgs, "is", cmo.IncludeSameFiles);
                AddArg(allArgs, "it", cmo.IncludeModifiedFiles);
                AddArg(allArgs, "max", cmo.MaxFileSize);
                AddArg(allArgs, "min", cmo.MinFileSize);
                AddArg(allArgs, "fft", cmo.AssumeFatFileTimes);
                AddArg(allArgs, "xjf", cmo.ExcludeFileJunctions);
            }

            if (parameters is CreateOptions)
                allArgs.Append(" /create");

            AddArg(allArgs, "s", parameters.CopyNonEmptySubdirectories);
            AddArg(allArgs, "e", parameters.CopyEmptySubdirectories);
            AddArg(allArgs, "lev", parameters.MaxSubdirectoryTreeLevel);
            AddArg(allArgs, "dcopy", parameters.DirectoryPropertiesToCopy);
            AddArg(allArgs, "purge", parameters.Purge);
            AddArg(allArgs, "mir", parameters.Mirror);
            AddArg(allArgs, "256", parameters.DisableLongPaths);
            AddArg(allArgs, "mon", parameters.MonitorForChangeCount);
            AddArg(allArgs, "mot", parameters.MonitorForChangeTime);
            if (parameters.ThreadCount != null)
                AddArg(allArgs, "mt", parameters.ThreadCount);
            else
                AddArg(allArgs, "mt", parameters.RunMultiThreaded);
            AddArg(allArgs, "sl", parameters.CopySymbolicLinks);

            AddArg(allArgs, "xd", parameters.ExcludedDirectories);
            AddArg(allArgs, "xx", parameters.ExcludeExtraFilesAndDirectories);
            AddArg(allArgs, "xl", parameters.ExcludeLonelyFilesAndDirectories);
            AddArg(allArgs, "maxage", parameters.MaxAge);
            AddArg(allArgs, "minage", parameters.MinAge);
            AddArg(allArgs, "maxlad", parameters.MaxLastAccessDate);
            AddArg(allArgs, "minlad", parameters.MinLastAccessDate);
            AddArg(allArgs, "xj", parameters.ExcludeJunctions);
            AddArg(allArgs, "dst", parameters.CompensateDstTimeDiffs);
            AddArg(allArgs, "xjd", parameters.ExcludeDirectoryJunctions);

            AddArg(allArgs, "r", parameters.RetryCount);
            AddArg(allArgs, "w", parameters.RetryWaitTime);
            AddArg(allArgs, "tbd", parameters.WaitForShareNames);

            if (!parameters.AdditionalRobocopyArguments.IsNullOrEmpty())
                allArgs.Append($" \"{string.Join("\" \"", parameters.AdditionalRobocopyArguments)}\"");

            if (cmo?.ShowTotalProgress == true)
                return RunRobocopyWithProgress(_console, cmo, allArgs.ToString());
            else
                return _processService.RunProcess("robocopy", allArgs.ToString(), x => _console.WriteLine(x), x => _console.WriteLineWithColor(x, ConsoleColor.Red));
        }

        private int RunRobocopyWithProgress(IConsoleService console, IRobocopyCopyMoveOptions options, string arguments)
        {
            long totalBytes = 0;
            long copiedBytes = 0;
            long currentBytes = 0;
            string? currentFolder = null;
            long fileCount = 0;
            long currentFile = 0;
            int fileCountPadding = 0;
            int maxStatusTextLength = 0;
            var folders = new LinkedList<string>();

            var p1 = new ProgressControl(console)
            {
                Progress = double.NaN,
                ShowStatusText = true,
                StatusText = "Determining files to copy...",
                ProgressBarWidth = 50,
                UseOneLineOnly = false,
                Status = ProgressControlStatus.Loading,
            };
            var p2 = new ProgressControl(console)
            {
                Progress = double.NaN,
                ShowStatusText = true,
                ProgressBarWidth = 50,
                UseOneLineOnly = false,
                Status = ProgressControlStatus.Loading,
            };

            int result = 0;
            using (new ConsoleScope(console, false, false, false))
            {
                console.IsCursorVisible = false;
                console.ReserveBufferLines(4);
                p1.Show();
                console.CancelKeyPress += OnCancelKeyPressed;

                var testArgs = arguments + " /NP /NC /BYTES /NJH /NJS /L";
                if (!options.RunMultiThreaded && options.ThreadCount == null && !options.DisableMtInTotalProgressDryRun)
                    testArgs += " /MT";
                if (options.VerboseLogging)
                    WriteLine("Executing: robocopy " + testArgs, ConsoleColor.DarkGray);
                _processService.RunProcess("robocopy", testArgs, OnNewOutput_Test, error => WriteLine(error, ConsoleColor.Red));

                if (totalBytes > 0)
                {
                    fileCountPadding = (int)Math.Ceiling(Math.Log10(fileCount + 1));

                    WriteLine($"Found a total of {fileCount} file(s) in {folders.Distinct().Count()} folder(s) with a total size of {GetFormattedByteSize(totalBytes, 2, CultureInfo.InvariantCulture)} to copy.", null);
                    p1.ShowStatusText = false;
                    using (ConsoleSynchronizer.Scope())
                        console.CursorPosition.Y--;
                    p2.Show();
                    maxStatusTextLength = p2.MaxStatusTextLength - ((fileCountPadding * 2) + 3);

                    var runArgs = arguments + " /NC /BYTES /NJH /NJS";
                    if (options.VerboseLogging)
                        WriteLine("Executing: robocopy " + runArgs, ConsoleColor.DarkGray);
                    result = _processService.RunProcess("robocopy", runArgs, OnNewOutput_Copy, error => WriteLine(error, ConsoleColor.Red));

                    p2.Hide(true);
                }
                else
                {
                    WriteLine($"Found no files to copy.", null);
                }

                p1.Hide(true);
                console.CancelKeyPress -= OnCancelKeyPressed;
            }

            return result;

            void WriteLine(string? text, ConsoleColor? color)
            {
                var p2Visible = p2!.IsVisible;
                if (p2Visible)
                    p2!.Hide(true);
                p1!.Hide(true);

                using (ConsoleSynchronizer.Scope())
                {
                    if (color.HasValue)
                        console.WriteLineWithColor(text, color.Value);
                    else
                        console.WriteLine(text);
                    console.ReserveBufferLines(4);
                }

                p1!.Show();
                if (p2Visible)
                    p2!.Show();
            }

            void OnNewOutput_Test(string? data)
            {
                if (options.VerboseLogging)
                    WriteLine(data, ConsoleColor.DarkGray);
                var fileMatch = FileRegex.Match(data ?? string.Empty);
                if (fileMatch.Success)
                {
                    var fileName = fileMatch.Groups["file"].Value;
                    if (fileName.Contains(Path.DirectorySeparatorChar))
                        folders!.Add(Path.GetDirectoryName(fileName));
                    if (!fileName.EndsWith(Path.DirectorySeparatorChar))
                    {
                        fileCount++;
                        totalBytes += long.Parse(fileMatch.Groups["size"].Value);
                    }
                }
            }

            void OnNewOutput_Copy(string? data)
            {
                var fileMatch = FileRegex.Match(data ?? string.Empty);
                if (fileMatch.Success)
                {
                    var fileName = fileMatch.Groups["file"].Value;
                    if (fileName.EndsWith(Path.DirectorySeparatorChar))
                    {
                        currentFolder = fileName;
                    }
                    else
                    {
                        if (!fileName.Contains(Path.DirectorySeparatorChar) && currentFolder != null)
                            fileName = Path.Combine(currentFolder, fileName);
                        currentBytes = long.Parse(fileMatch.Groups["size"].Value);

                        currentFile++;
                        var statusPrefix = $"{currentFile.ToString().PadLeft(fileCountPadding)}/{fileCount}: ";
                        if (fileName.Length > maxStatusTextLength)
                        {
                            p2!.StatusText = statusPrefix + "..." + fileName[(fileName.Length - maxStatusTextLength + 2)..];
                        }
                        else
                        {
                            p2!.StatusText = statusPrefix + fileName;
                        }

                        p2!.Progress = 0;
                    }
                }

                var percentMatch = PercentRegex.Match(data ?? string.Empty);
                if (percentMatch.Success)
                {
                    p2!.Progress = double.Parse(percentMatch.Value.Replace(",", "."), CultureInfo.InvariantCulture) / 100D;
                    p1!.Progress = (double)(copiedBytes + (p2!.Progress * currentBytes)) / totalBytes;
                    if (p2!.Progress >= 0.999D)
                    {
                        copiedBytes += currentBytes;
                        currentBytes = 0;
                    }
                }

                if (!fileMatch.Success && !percentMatch.Success && !string.IsNullOrWhiteSpace(data))
                    WriteLine(data, ConsoleColor.Yellow);
                else if (fileMatch.Success && options.VerboseLogging)
                    WriteLine(data, ConsoleColor.DarkGray);
            }

            void OnCancelKeyPressed(object? sender, ConsoleCancelEventArgs e)
            {
                WriteLine("Copy has been canceled by the user!", ConsoleColor.Yellow);
                p2?.Hide(true);
                p1?.Hide(true);
                console.IsCursorVisible = true;
            }
        }

        private static void AddArg(StringBuilder sb, string argName, bool value)
        {
            if (value)
                sb.Append($" /{argName}");
        }

        private static void AddArg(StringBuilder sb, string argName, string? value)
        {
            if (value != null)
                sb.Append($" /{argName}:\"{value}\"");
        }

        private static void AddArg<T>(StringBuilder sb, string argName, T? value)
            where T : struct, IFormattable
        {
            if (value != null)
                sb.Append($" /{argName}:{value.Value.ToString(null, CultureInfo.InvariantCulture)}");
        }

        private static void AddArg(StringBuilder sb, string argName, IEnumerable<string>? value)
        {
            if (!value.IsNullOrEmpty())
                sb.Append($" /{argName} \"{string.Join("\" \"", value)}\"");
        }

        private static readonly string[] ByteSizeSuffixes = new[] { "Byte(s)", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        private static string GetFormattedByteSize(double byteCount, int decimalPlaces, IFormatProvider formatProvider)
        {
            int i = 0;
            while (byteCount >= 1024 && i < ByteSizeSuffixes.Length - 1)
            {
                byteCount /= 1024D;
                i++;
            }

            return $"{byteCount.ToString($"#,##0.{new string('0', decimalPlaces)}", formatProvider)} {ByteSizeSuffixes[i]}";
        }
    }
}
