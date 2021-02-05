using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using MaSch.CommandLineTools.Common;
using MaSch.CommandLineTools.Extensions;
using MaSch.CommandLineTools.Utilities;
using MaSch.Console;
using MaSch.Console.Controls;
using MaSch.Core.Extensions;

namespace MaSch.CommandLineTools.Tools.RobocopyRunner
{
    [Verb("rcx", HelpText = "Provides a different way to execute robocopy. Also contains some robocopy extensions.")]
    public class RobocopyRunner : BaseToolRunnerWithArgsParser
    {
        private static readonly (int code, string description)[] RobocopyExitCodes =
        {
            (0, "No files were copied. No failure was encountered. No files were mismatched. The files already exist in the destination directory; therefore, the copy operation was skipped."),
            (1, "All files were copied successfully."),
            (2, "There are some additional files in the destination directory that are not present in the source directory. No files were copied."),
            (3, "Some files were copied. Additional files were present. No failure was encountered."),
            (5, "Some files were copied. Some files were mismatched. No failure was encountered."),
            (6, "Additional files and mismatched files exist. No files were copied and no failures were encountered. This means that the files already exist in the destination directory."),
            (7, "Files were copied, a file mismatch was present, and additional files were present."),
            (8, "Several files did not copy."),
        };

        private static readonly Regex FileRegex = new Regex(@"(?<size>(?<=\s+)\d+(?=\t))\s+(?<file>.+(?=$))", RegexOptions.Compiled);
        private static readonly Regex PercentRegex = new Regex(@"[\d\.\,]+(?=%)", RegexOptions.Compiled);

        protected override void OnConfigure(RunnerOptions options)
        {
            options.Author = "Marc Schmidt";
            options.Year = 2020;
            options.DisplayName = "Robocopy Runner";
            options.Version = new Version(1, 0, 1);

            options.AddOptionHandler<CopyOptions>(Handle);
            options.AddOptionHandler<MoveOptions>(Handle);
            options.AddOptionHandler<CreateOptions>(Handle);
        }

        protected override void AdjustHelpText<T>(ParserResult<T> result, HelpText h)
        {
            base.AdjustHelpText(result, h);

            var interfaceProperties = result.TypeInfo.Current.GetInterfaces().SelectMany(x => x.GetProperties());
            var options = (from p in result.TypeInfo.Current.GetProperties()
                           let o = p.GetCustomAttribute<OptionAttribute>(true) ?? interfaceProperties.FirstOrDefault(x => x.Name == p.Name)?.GetCustomAttribute<OptionAttribute>(true)
                           where o != null
                           select (shortName: o.ShortName, longName: o.LongName)).ToArray();
            h.OptionComparison = (a, b) =>
            {
                if (a.IsOption != b.IsOption)
                {
                    return (a.IsOption ? 1 : 0) - (b.IsOption ? 1 : 0);
                }
                else if (!a.IsOption && !b.IsOption)
                {
                    return a.Index.CompareTo(b.Index);
                }
                else
                {
                    var aIndex = options.IndexOf((a.ShortName, a.LongName));
                    var bIndex = options.IndexOf((b.ShortName, b.LongName));
                    return aIndex < 0 && bIndex < 0 ? a.Index.CompareTo(b.Index) : (aIndex < 0 ? int.MaxValue : aIndex).CompareTo(bIndex < 0 ? int.MaxValue : bIndex);
                }
            };
        }

        protected override void OnHandleExitCodes()
        {
            WriteExitCodeList("General", x => x < 0);
            WriteExitCodeList("Copy (Default)", ExitCode.RobocopyCopy);
            Console.WriteLine("  If no error occures the exit code of robocopy is returned.");
            WriteExitCodeList("Move", ExitCode.RobocopyMove);
            Console.WriteLine("  If no error occures the exit code of robocopy is returned.");
            WriteExitCodeList("Create", ExitCode.RobocopyCreate);
            Console.WriteLine("  If no error occures the exit code of robocopy is returned.");

            Console.WriteLine();
            Console.WriteLine("robocopy Exit Codes:");
            Console.WriteLine(RobocopyExitCodes.ToColumnsString(x => x.code.ToString(), x => x.description));
        }

        private ExitCode Handle(IRobocopyOptions options)
        {
            var allArgs = new StringBuilder();
            allArgs.Append($"\"{options.Source}\"");
            allArgs.Append($" \"{options.Destination}\"");

            var cmo = options as IRobocopyCopyMoveOptions;
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

            if (options is CreateOptions)
                allArgs.Append(" /create");

            AddArg(allArgs, "s", options.CopyNonEmptySubdirectories);
            AddArg(allArgs, "e", options.CopyEmptySubdirectories);
            AddArg(allArgs, "lev", options.MaxSubdirectoryTreeLevel);
            AddArg(allArgs, "dcopy", options.DirectoryPropertiesToCopy);
            AddArg(allArgs, "purge", options.Purge);
            AddArg(allArgs, "mir", options.Mirror);
            AddArg(allArgs, "256", options.DisableLongPaths);
            AddArg(allArgs, "mon", options.MonitorForChangeCount);
            AddArg(allArgs, "mot", options.MonitorForChangeTime);
            if (options.ThreadCount != null)
                AddArg(allArgs, "mt", options.ThreadCount);
            else
                AddArg(allArgs, "mt", options.RunMultiThreaded);
            AddArg(allArgs, "sl", options.CopySymbolicLinks);

            AddArg(allArgs, "xd", options.ExcludedDirectories);
            AddArg(allArgs, "xx", options.ExcludeExtraFilesAndDirectories);
            AddArg(allArgs, "xl", options.ExcludeLonelyFilesAndDirectories);
            AddArg(allArgs, "maxage", options.MaxAge);
            AddArg(allArgs, "minage", options.MinAge);
            AddArg(allArgs, "maxlad", options.MaxLastAccessDate);
            AddArg(allArgs, "minlad", options.MinLastAccessDate);
            AddArg(allArgs, "xj", options.ExcludeJunctions);
            AddArg(allArgs, "dst", options.CompensateDstTimeDiffs);
            AddArg(allArgs, "xjd", options.ExcludeDirectoryJunctions);

            AddArg(allArgs, "r", options.RetryCount);
            AddArg(allArgs, "w", options.RetryWaitTime);
            AddArg(allArgs, "tbd", options.WaitForShareNames);

            if (!options.AdditionalRobocopyArguments.IsNullOrEmpty())
                allArgs.Append($" \"{string.Join("\" \"", options.AdditionalRobocopyArguments)}\"");

            if (cmo?.ShowTotalProgress == true)
                return (ExitCode)RunRobocopyWithProgress(cmo, allArgs.ToString());
            else
                return (ExitCode)ProcessUtility.RunProcess("robocopy", allArgs.ToString(), x => Console.WriteLine(x), x => Console.WriteLineWithColor(x, ConsoleColor.Red));
        }

        private int RunRobocopyWithProgress(IRobocopyCopyMoveOptions options, string arguments)
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

            var p1 = new ProgressControl(Console)
            {
                Progress = double.NaN,
                ShowStatusText = true,
                StatusText = "Determining files to copy...",
                ProgressBarWidth = 50,
                UseOneLineOnly = false,
                Status = ProgressControlStatus.Loading,
            };
            var p2 = new ProgressControl(Console)
            {
                Progress = double.NaN,
                ShowStatusText = true,
                ProgressBarWidth = 50,
                UseOneLineOnly = false,
                Status = ProgressControlStatus.Loading,
            };

            int result = 0;
            using (new ConsoleScope(Console, false, false, false))
            {
                Console.IsCursorVisible = false;
                Console.ReserveBufferLines(4);
                p1.Show();
                Console.CancelKeyPress += OnCancelKeyPressed;

                var testArgs = arguments + " /NP /NC /BYTES /NJH /NJS /L";
                if (!options.RunMultiThreaded && options.ThreadCount == null && !options.DisableMtInTotalProgressDryRun)
                    testArgs += " /MT";
                if (options.VerboseLogging)
                    WriteLine("Executing: robocopy " + testArgs, ConsoleColor.DarkGray);
                ProcessUtility.RunProcess("robocopy", testArgs, OnNewOutput_Test, error => WriteLine(error, ConsoleColor.Red));

                if (totalBytes > 0)
                {
                    fileCountPadding = (int)Math.Ceiling(Math.Log10(fileCount + 1));

                    WriteLine($"Found a total of {fileCount} file(s) in {folders.Distinct().Count()} folder(s) with a total size of {GetFormattedByteSize(totalBytes, 2, CultureInfo.InvariantCulture)} to copy.", null);
                    p1.ShowStatusText = false;
                    using (ConsoleSynchronizer.Scope())
                        Console.CursorPosition.Y--;
                    p2.Show();
                    maxStatusTextLength = p2.MaxStatusTextLength - ((fileCountPadding * 2) + 3);

                    var runArgs = arguments + " /NC /BYTES /NJH /NJS";
                    if (options.VerboseLogging)
                        WriteLine("Executing: robocopy " + runArgs, ConsoleColor.DarkGray);
                    result = ProcessUtility.RunProcess("robocopy", runArgs, OnNewOutput_Copy, error => WriteLine(error, ConsoleColor.Red));

                    p2.Hide(true);
                }
                else
                {
                    WriteLine($"Found no files to copy.", null);
                }

                p1.Hide(true);
                Console.CancelKeyPress -= OnCancelKeyPressed;
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
                        Console.WriteLineWithColor(text, color.Value);
                    else
                        Console.WriteLine(text);
                    Console.ReserveBufferLines(4);
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
                Console.IsCursorVisible = true;
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
