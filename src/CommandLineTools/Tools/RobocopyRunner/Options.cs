using MaSch.Console.Cli.Configuration;
using System.Collections.Generic;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name

namespace MaSch.CommandLineTools.Tools.RobocopyRunner
{
    public interface IRobocopyOptions
    {
        [CliCommandOption("args", HelpOrder = -100, HelpText = "Additional arguments that are passed to robocopy. For a list of all available arguments run 'robocopy /?'.")]
        IEnumerable<string>? AdditionalRobocopyArguments { get; set; }

        [CliCommandValue(0, "source", Required = true, HelpText = "Specifies the path to the source directory.")]
        string Source { get; set; }

        [CliCommandValue(1, "destination", Required = true, HelpText = "Specifies the path to the destination directory.")]
        string Destination { get; set; }

        [CliCommandOption('S', "non-empty-subdirs", HelpText = "Copies subdirectories. This option automatically excludes empty directories.")]
        bool CopyNonEmptySubdirectories { get; set; }
        [CliCommandOption('s', "empty-subdirs", HelpText = "Copies subdirectories. This option automatically includes empty directories.")]
        bool CopyEmptySubdirectories { get; set; }
        [CliCommandOption('L', "level", HelpText = "Copies only the top n levels of the source directory tree.")]
        int? MaxSubdirectoryTreeLevel { get; set; }
        [CliCommandOption("d-copy", HelpText = "Specifies what to copy in directories. The valid values for this option are:\n    D - Data\n    A - Attributes\n    T - Time stamps\nThe default value for this option is DA (data and attributes).")]
        string? DirectoryPropertiesToCopy { get; set; }
        [CliCommandOption('p', "purge", HelpText = "Deletes destination files and directories that no longer exist in the source. Using this option with the /e option and a destination directory, allows the destination directory security settings to not be overwritten.")]
        bool Purge { get; set; }
        [CliCommandOption('m', "mirror", HelpText = "Mirrors a directory tree (equivalent to /e plus /purge). Using this option with the /e option and a destination directory, overwrites the destination directory security settings.")]
        bool Mirror { get; set; }
        [CliCommandOption("256", HelpText = "Turns off support for paths longer than 256 characters.")]
        bool DisableLongPaths { get; set; }
        [CliCommandOption('w', "watch-number", HelpText = "Monitors the source, and runs again when more than n changes are detected.")]
        int? MonitorForChangeCount { get; set; }
        [CliCommandOption('W', "watch-time", HelpText = "Monitors the source, and runs again in m minutes, if changes are detected.")]
        int? MonitorForChangeTime { get; set; }
        [CliCommandOption('T', "thread-count", HelpText = "Creates multi-threaded copies with n threads. n must be an integer between 1 and 128. This parameter cannot be used with the --efsraw parameter.")]
        int? ThreadCount { get; set; }
        [CliCommandOption('t', "multi-thread", HelpText = "Creates multi-threaded copies with 8 threads. This parameter cannot be used with the --efsraw parameter.")]
        bool RunMultiThreaded { get; set; }
        [CliCommandOption("symbolic-links", HelpText = "Don't follow symbolic links and instead create a copy of the link.")]
        bool CopySymbolicLinks { get; set; }

        [CliCommandOption('X', "xd", HelpText = "Excludes directories that match the specified names and paths.")]
        IEnumerable<string>? ExcludedDirectories { get; set; }
        [CliCommandOption("xx", HelpText = "Excludes extra files and directories.")]
        bool ExcludeExtraFilesAndDirectories { get; set; }
        [CliCommandOption("xl", HelpText = "Excludes \"lonely\" files and directories.")]
        bool ExcludeLonelyFilesAndDirectories { get; set; }
        [CliCommandOption("max-age", HelpText = "Specifies the maximum file age (to exclude files older than n days or date).")]
        string? MaxAge { get; set; }
        [CliCommandOption("min-age", HelpText = "Specifies the minimum file age (exclude files newer than n days or date).")]
        string? MinAge { get; set; }
        [CliCommandOption("max-lad", HelpText = "Specifies the maximum last access date (excludes files unused since n).")]
        string? MaxLastAccessDate { get; set; }
        [CliCommandOption("min-lad", HelpText = "Specifies the minimum last access date (excludes files used since n) If n is less than 1900, n specifies the number of days. Otherwise, n specifies a date in the format YYYYMMDD.")]
        string? MinLastAccessDate { get; set; }
        [CliCommandOption("xj", HelpText = "Excludes junction points, which are normally included by default.")]
        bool ExcludeJunctions { get; set; }
        [CliCommandOption("dst", HelpText = "Compensates for one-hour DST time differences.")]
        bool CompensateDstTimeDiffs { get; set; }
        [CliCommandOption("xjd", HelpText = "Excludes junction points for directories.")]
        bool ExcludeDirectoryJunctions { get; set; }

        [CliCommandOption('r', "retry-count", HelpText = "Specifies the number of retries on failed copies. The default value of n is 1,000,000 (one million retries).")]
        int? RetryCount { get; set; }
        [CliCommandOption('R', "retry-wait", HelpText = "Specifies the wait time between retries, in seconds. The default value of n is 30 (wait time 30 seconds).")]
        int? RetryWaitTime { get; set; }
        [CliCommandOption("tbd", HelpText = "Specifies that the system will wait for share names to be defined (retry error 67).")]
        bool WaitForShareNames { get; set; }
    }

    public interface IRobocopyCopyMoveOptions : IRobocopyOptions
    {
        [CliCommandValue(2, "file", Required = false, HelpText = "Specifies the file or files to be copied. Wildcard characters (* or ?) are supported. If you don't specify this parameter, *. is used as the default value.")]
        IEnumerable<string>? Files { get; set; }

        [CliCommandOption('z', "restartable", HelpText = "Copies files in restartable mode.")]
        bool CopyInRestartableMode { get; set; }
        [CliCommandOption('b', "backup", HelpText = "Copies files in Backup mode.")]
        bool CopyFilesInBackupoMode { get; set; }
        [CliCommandOption("zb", HelpText = "Uses restartable mode. If access is denied, this option uses Backup mode.")]
        bool CopyInRestartableOrBackupMode { get; set; }
        [CliCommandOption("efs-raw", HelpText = "Copies all encrypted files in EFS RAW mode.")]
        bool CopyEncryptedInEfsRawMode { get; set; }
        [CliCommandOption("copy", HelpText = "Specifies which file properties to copy. The valid values for this option are:\n    D - Data\n    A - Attributes\n    T - Time stamps\n    S - NTFS access control list (ACL)\n    O - Owner information\n    U - Auditing information\nThe default value for this option is DAT (data, attributes, and time stamps).")]
        string? FilePropertiesToCopy { get; set; }
        [CliCommandOption("sec", HelpText = "Copies files with security (equivalent to --copy DATS).")]
        bool CopyFilesWithSecurity { get; set; }
        [CliCommandOption("copy-all", HelpText = "Copies all file information (equivalent to --copy DATSOU).")]
        bool CopyAllFileInformation { get; set; }
        [CliCommandOption("no-copy", HelpText = "Copies no file information (useful with --purge).")]
        bool CopyNoFileInformation { get; set; }
        [CliCommandOption("sec-fix", HelpText = "Fixes file security on all files, even skipped ones.")]
        bool FixFileSecurityForAll { get; set; }
        [CliCommandOption("tim-fix", HelpText = "Fixes file times on all files, even skipped ones.")]
        bool FixFileTimesForAll { get; set; }
        [CliCommandOption("a+", HelpText = "Adds the specified attributes to copied files. The valid values for this option are:\n    R - Read only\n    A - Archive\n    S - System\n    H - Hidden\n    C - Compressed\n    N - Not content indexed\n    E - Encrypted\n    T - Temporary")]
        string? FileAttributesToAdd { get; set; }
        [CliCommandOption("a-", HelpText = "Removes the specified attributes from copied files. The valid values for this option are:\n    R - Read only\n    A - Archive\n    S - System\n    H - Hidden\n    C - Compressed\n    N - Not content indexed\n    E - Encrypted\n    T - Temporary")]
        string? FileAttributesToRemove { get; set; }
        [CliCommandOption("fat", HelpText = "Creates destination files by using 8.3 character-length FAT file names only.")]
        bool UseFatFileNames { get; set; }

        [CliCommandOption('a', "archive", HelpText = "Copies only files for which the Archive attribute is set.")]
        bool CopyOnlyArchiveFiles { get; set; }
        [CliCommandOption('A', "archive-reset", HelpText = "Copies only files for which the Archive attribute is set, and resets the Archive attribute.")]
        bool CopyOnlyArchiveFilesAndReset { get; set; }
        [CliCommandOption("ia", HelpText = "Includes only files for which any of the specified attributes are set. The valid values for this option are:\n    R - Read only\n    A - Archive\n    S - System\n    H - Hidden\n    C - Compressed\n    N - Not content indexed\n    E - Encrypted\n    T - Temporary\n    O - Offline")]
        string? IncludedFileAttributes { get; set; }
        [CliCommandOption("xa", HelpText = "Excludes files for which any of the specified attributes are set. The valid values for this option are:\n    R - Read only\n    A - Archive\n    S - System\n    H - Hidden\n    C - Compressed\n    N - Not content indexed\n    E - Encrypted\n    T - Temporary\n    O - Offline")]
        string? ExcludedFileAttributes { get; set; }
        [CliCommandOption('x', "xf", HelpText = "Excludes files that match the specified names or paths. Wildcard characters (* and ?) are supported.")]
        IEnumerable<string>? ExcludedFiles { get; set; }
        [CliCommandOption("xc", HelpText = "Excludes changed files.")]
        bool ExcludeChangedFiles { get; set; }
        [CliCommandOption("xn", HelpText = "Excludes newer files.")]
        bool ExcludeNewerFiles { get; set; }
        [CliCommandOption("xo", HelpText = "Excludes older files.")]
        bool ExcludeOlderFiles { get; set; }
        [CliCommandOption("is", HelpText = "Includes the same files.")]
        bool IncludeSameFiles { get; set; }
        [CliCommandOption("it", HelpText = "Includes modified files.")]
        bool IncludeModifiedFiles { get; set; }
        [CliCommandOption("max", HelpText = "Specifies the maximum file size (to exclude files bigger than n bytes).")]
        long? MaxFileSize { get; set; }
        [CliCommandOption("min", HelpText = "Specifies the minimum file size (to exclude files smaller than n bytes).")]
        long? MinFileSize { get; set; }
        [CliCommandOption("fft", HelpText = "Assumes FAT file times (two-second precision).")]
        bool AssumeFatFileTimes { get; set; }
        [CliCommandOption("xjf", HelpText = "Excludes junction points for files.")]
        bool ExcludeFileJunctions { get; set; }

        [CliCommandOption('P', "progress", HelpText = "Shows a progress bar instead of the robocopy log. WARNING: This will do a dry-run with the /MT flag before copying/moving files.")]
        bool ShowTotalProgress { get; set; }
        [CliCommandOption("no-mt-dryrun", HelpText = "Disables the /MT option for the dry-run that is executed by --progress.")]
        bool DisableMtInTotalProgressDryRun { get; set; }
        [CliCommandOption("verbose", HelpText = "Enables verbose logging when the --progress parameter is used.")]
        public bool VerboseLogging { get; set; }
    }

    [CliCommand("copy", IsDefault = true, HelpText = "Copies files using robocopy.", ParentCommand = typeof(RobocopyTool))]
    public class CopyOptions : IRobocopyCopyMoveOptions
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Source { get; set; }
        public string Destination { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IEnumerable<string>? Files { get; set; }

        public bool CopyNonEmptySubdirectories { get; set; }
        public bool CopyEmptySubdirectories { get; set; }
        public int? MaxSubdirectoryTreeLevel { get; set; }
        public bool CopyInRestartableMode { get; set; }
        public bool CopyFilesInBackupoMode { get; set; }
        public bool CopyInRestartableOrBackupMode { get; set; }
        public bool CopyEncryptedInEfsRawMode { get; set; }
        public string? FilePropertiesToCopy { get; set; }
        public string? DirectoryPropertiesToCopy { get; set; }
        public bool CopyFilesWithSecurity { get; set; }
        public bool CopyAllFileInformation { get; set; }
        public bool CopyNoFileInformation { get; set; }
        public bool FixFileSecurityForAll { get; set; }
        public bool FixFileTimesForAll { get; set; }
        public bool Purge { get; set; }
        public bool Mirror { get; set; }
        public string? FileAttributesToAdd { get; set; }
        public string? FileAttributesToRemove { get; set; }
        public bool UseFatFileNames { get; set; }
        public bool DisableLongPaths { get; set; }
        public int? MonitorForChangeCount { get; set; }
        public int? MonitorForChangeTime { get; set; }
        public int? ThreadCount { get; set; }
        public bool RunMultiThreaded { get; set; }
        public bool CopySymbolicLinks { get; set; }
        public bool CopyOnlyArchiveFiles { get; set; }
        public bool CopyOnlyArchiveFilesAndReset { get; set; }
        public string? IncludedFileAttributes { get; set; }
        public string? ExcludedFileAttributes { get; set; }
        public IEnumerable<string>? ExcludedFiles { get; set; }
        public IEnumerable<string>? ExcludedDirectories { get; set; }
        public bool ExcludeChangedFiles { get; set; }
        public bool ExcludeNewerFiles { get; set; }
        public bool ExcludeOlderFiles { get; set; }
        public bool ExcludeExtraFilesAndDirectories { get; set; }
        public bool ExcludeLonelyFilesAndDirectories { get; set; }
        public bool IncludeSameFiles { get; set; }
        public bool IncludeModifiedFiles { get; set; }
        public long? MaxFileSize { get; set; }
        public long? MinFileSize { get; set; }
        public string? MaxAge { get; set; }
        public string? MinAge { get; set; }
        public string? MaxLastAccessDate { get; set; }
        public string? MinLastAccessDate { get; set; }
        public bool ExcludeJunctions { get; set; }
        public bool AssumeFatFileTimes { get; set; }
        public bool CompensateDstTimeDiffs { get; set; }
        public bool ExcludeDirectoryJunctions { get; set; }
        public bool ExcludeFileJunctions { get; set; }
        public int? RetryCount { get; set; }
        public int? RetryWaitTime { get; set; }
        public bool WaitForShareNames { get; set; }

        public IEnumerable<string>? AdditionalRobocopyArguments { get; set; }

        public bool ShowTotalProgress { get; set; }
        public bool DisableMtInTotalProgressDryRun { get; set; }
        public bool VerboseLogging { get; set; }
    }

    [CliCommand("move", HelpText = "Moves files using robocopy.", ParentCommand = typeof(RobocopyTool))]
    public class MoveOptions : IRobocopyCopyMoveOptions
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Source { get; set; }
        public string Destination { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IEnumerable<string>? Files { get; set; }

        public bool CopyNonEmptySubdirectories { get; set; }
        public bool CopyEmptySubdirectories { get; set; }
        public int? MaxSubdirectoryTreeLevel { get; set; }
        public bool CopyInRestartableMode { get; set; }
        public bool CopyFilesInBackupoMode { get; set; }
        public bool CopyInRestartableOrBackupMode { get; set; }
        public bool CopyEncryptedInEfsRawMode { get; set; }
        public string? FilePropertiesToCopy { get; set; }
        public string? DirectoryPropertiesToCopy { get; set; }
        public bool CopyFilesWithSecurity { get; set; }
        public bool CopyAllFileInformation { get; set; }
        public bool CopyNoFileInformation { get; set; }
        public bool FixFileSecurityForAll { get; set; }
        public bool FixFileTimesForAll { get; set; }
        public bool Purge { get; set; }
        public bool Mirror { get; set; }
        [CliCommandOption('e', "exclude-dirs", HelpText = "Moves only files. (Uses /MOV robocopy command instead of /MOVE)")]
        public bool ExcludeDirectories { get; set; }
        public string? FileAttributesToAdd { get; set; }
        public string? FileAttributesToRemove { get; set; }
        public bool UseFatFileNames { get; set; }
        public bool DisableLongPaths { get; set; }
        public int? MonitorForChangeCount { get; set; }
        public int? MonitorForChangeTime { get; set; }
        public int? ThreadCount { get; set; }
        public bool RunMultiThreaded { get; set; }
        public bool CopySymbolicLinks { get; set; }
        public bool CopyOnlyArchiveFiles { get; set; }
        public bool CopyOnlyArchiveFilesAndReset { get; set; }
        public string? IncludedFileAttributes { get; set; }
        public string? ExcludedFileAttributes { get; set; }
        public IEnumerable<string>? ExcludedFiles { get; set; }
        public IEnumerable<string>? ExcludedDirectories { get; set; }
        public bool ExcludeChangedFiles { get; set; }
        public bool ExcludeNewerFiles { get; set; }
        public bool ExcludeOlderFiles { get; set; }
        public bool ExcludeExtraFilesAndDirectories { get; set; }
        public bool ExcludeLonelyFilesAndDirectories { get; set; }
        public bool IncludeSameFiles { get; set; }
        public bool IncludeModifiedFiles { get; set; }
        public long? MaxFileSize { get; set; }
        public long? MinFileSize { get; set; }
        public string? MaxAge { get; set; }
        public string? MinAge { get; set; }
        public string? MaxLastAccessDate { get; set; }
        public string? MinLastAccessDate { get; set; }
        public bool ExcludeJunctions { get; set; }
        public bool AssumeFatFileTimes { get; set; }
        public bool CompensateDstTimeDiffs { get; set; }
        public bool ExcludeDirectoryJunctions { get; set; }
        public bool ExcludeFileJunctions { get; set; }
        public int? RetryCount { get; set; }
        public int? RetryWaitTime { get; set; }
        public bool WaitForShareNames { get; set; }

        public IEnumerable<string>? AdditionalRobocopyArguments { get; set; }

        public bool ShowTotalProgress { get; set; }
        public bool DisableMtInTotalProgressDryRun { get; set; }
        public bool VerboseLogging { get; set; }
    }

    [CliCommand("create", HelpText = "Copied a directory structure using robocopy.", ParentCommand = typeof(RobocopyTool))]
    public class CreateOptions : IRobocopyOptions
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Source { get; set; }
        public string Destination { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public bool CopyNonEmptySubdirectories { get; set; }
        public bool CopyEmptySubdirectories { get; set; }
        public int? MaxSubdirectoryTreeLevel { get; set; }
        public string? DirectoryPropertiesToCopy { get; set; }
        public bool Purge { get; set; }
        public bool Mirror { get; set; }
        public bool DisableLongPaths { get; set; }
        public int? MonitorForChangeCount { get; set; }
        public int? MonitorForChangeTime { get; set; }
        public int? ThreadCount { get; set; }
        public bool RunMultiThreaded { get; set; }
        public bool CopySymbolicLinks { get; set; }
        public IEnumerable<string>? ExcludedDirectories { get; set; }
        public bool ExcludeExtraFilesAndDirectories { get; set; }
        public bool ExcludeLonelyFilesAndDirectories { get; set; }
        public string? MaxAge { get; set; }
        public string? MinAge { get; set; }
        public string? MaxLastAccessDate { get; set; }
        public string? MinLastAccessDate { get; set; }
        public bool ExcludeJunctions { get; set; }
        public bool CompensateDstTimeDiffs { get; set; }
        public bool ExcludeDirectoryJunctions { get; set; }
        public int? RetryCount { get; set; }
        public int? RetryWaitTime { get; set; }
        public bool WaitForShareNames { get; set; }

        public IEnumerable<string>? AdditionalRobocopyArguments { get; set; }
    }
}
