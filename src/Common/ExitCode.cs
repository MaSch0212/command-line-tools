using System;
using System.ComponentModel;

namespace MaSch.CommandLineTools.Common
{
    [Flags]
    public enum ExitCode
    {
        [ReturnableExitCode("The arguments passed to the application could not be parsed correctly.")]
        ArgumentParseError = -2,
        [ReturnableExitCode("An unknown error occurred.")]
        UnknownError = -1,
        [ReturnableExitCode("The application exited without errors.")]
        Okay = 0,

        Info = 0b0001 << 16,
        Warning = 0b0010 << 16,
        Error = 0b0011 << 16,
        Success = 0b0100 << 16,

        CdxCodes = 1 << 20,
        CdxOpen = CdxCodes | 1 << 8,
        [ReturnableExitCode("No alias was provided.")]
        CdxOpenMissingAlias = CdxOpen | Error | 1,
        [ReturnableExitCode("The provided alias does not exist.")]
        CdxOpenAliasNotFound = CdxOpen | Error | 2,
        [ReturnableExitCode("The command was not executed from a supported script.")]
        CdxOpenNotFromSupportedScript = CdxOpen | Error | 3,
        CdxCopy = CdxCodes | 2 << 8,
        [ReturnableExitCode("No alias was provided.")]
        CdxCopyMissingAlias = CdxCopy | Error | 1,
        [ReturnableExitCode("The provided alias does not exist.")]
        CdxCopyAliasNotFound = CdxCopy | Error | 2,
        CdxAdd = CdxCodes | 3 << 8,
        [ReturnableExitCode("No alias was provided.")]
        CdxAddMissingAlias = CdxAdd | Error | 1,
        [ReturnableExitCode("No directory was provided and the current directory could not be retrieved.")]
        CdxAddMissingDirectory = CdxAdd | Error | 2,
        [ReturnableExitCode("The alias name contains illegal characters.")]
        CdxAddInvalidAliasName = CdxAdd | Error | 3,
        CdxRemove = CdxCodes | 4 << 8,
        CdxList = CdxCodes | 5 << 8,
        CdxInstall = CdxCodes | 6 << 8,
        CdxUninstall = CdxCodes | 7 << 8,
        CdxConfig = CdxCodes | 8 << 8,

        SuCodes = 2 << 20,
        SuRun = SuCodes | 1 << 8,
        [ReturnableExitCode("The current process already has elevated access.")]
        SuRunAlreadyElevated = SuRun | Success | 1,
        [ReturnableExitCode("The current user is not an administrator and cannot run elevated processes.")]
        SuRunNoAdmin = SuRun | Error | 1,
        [ReturnableExitCode("The user declined the elevation request.")]
        SuRunUserDeclined = SuRun | Error | 2,
        [ReturnableExitCode("The provided tool is unknown.")]
        SuRunUnknownTool = SuRun | Error | 3,

        SudoCodes = 3 << 20,
        SudoRun = SudoCodes | 1 << 8,
        [ReturnableExitCode("The current user is not an administrator and cannot run elevated processes.")]
        SudoRunNoAdmin = SudoRun | Error | 1,
        [ReturnableExitCode("The user declined the elevation request.")]
        SudoRunUserDeclined = SudoRun | Error | 2,
        [ReturnableExitCode("The provided tool is unknown.")]
        SudoRunUnknownTool = SudoRun | Error | 3,
        SudoPlease = SudoCodes | 2 << 8,

        AliasCodes = 4 << 20,
        AliasList = AliasCodes | 1 << 8,
        AliasAdd = AliasCodes | 2 << 8,
        [ReturnableExitCode("The provided alias already exists in the defined scope.")]
        AliasAddExists = AliasAdd | Error | 1,
        [ReturnableExitCode("One or more script files could not be created.")]
        AliasAddFailedCreateScript = AliasAdd | Error | 2,
        [ReturnableExitCode("The commands file could not be saved.")]
        AliasAddFailedModifyJson = AliasAdd | Error | 3,
        AliasRemove = AliasCodes | 3 << 8,
        [ReturnableExitCode("The provided alias does not exist in any of the defined scopes.")]
        AliasRemoveNotExists = AliasRemove | Error | 1,
        [ReturnableExitCode("One or more script files could not be removed.")]
        AliasRemoveFailedDeleteScript = AliasRemove | Error | 2,
        [ReturnableExitCode("The commands file could not be saved.")]
        AliasRemoveFailedModifyJson = AliasRemove | Error | 3,
        AliasInstall = AliasCodes | 4 << 8,
        AliasUninstall = AliasCodes | 5 << 8,
        AliasCleanup = AliasCodes | 6 << 8,
        [ReturnableExitCode("Multiple errors occurred while cleaning up. Please resolve the issues and execute cleanup again.")]
        AliasCleanupMultipleErrors = AliasCleanup | Error | 1,
        [ReturnableExitCode("One or more script files could not be created.")]
        AliasCleanupFailedCreateScript = AliasCleanup | Error | 2,
        [ReturnableExitCode("One or more script files could not be removed.")]
        AliasCleanupFailedDeleteScript = AliasCleanup | Error | 3,

        AttachCodes = 5 << 20,
        AttachRun = AttachCodes | 1 << 8,

        RobocopyCodes = 6 << 20,
        RobocopyCopy = RobocopyCodes | 1 << 8,
        RobocopyMove = RobocopyCodes | 2 << 8,
        RobocopyCreate = RobocopyCodes | 3 << 8,

        // Masks:
        ToolMask = 0xFFF << 20,
        CodeTypeMask = 0xF << 16,
        CommandMask = 0xFF << 8,
        ToolCommandMask = ToolMask | CommandMask,
        ErrorMask = 0xFF,
        ToolCommandErrorMask = ToolCommandMask | ErrorMask,
    }

    public static class ExitCodeExtensions
    {
        private const int TypeMask = 0b1111 << 16;

        public static bool IsInfo(this ExitCode code) => ((int)code & TypeMask) == (int)ExitCode.Info;
        public static bool IsSuccess(this ExitCode code) => ((int)code & TypeMask) == (int)ExitCode.Success;
        public static bool IsWarning(this ExitCode code) => ((int)code & TypeMask) == (int)ExitCode.Warning;
        public static bool IsError(this ExitCode code) => ((int)code & TypeMask) == (int)ExitCode.Error;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ReturnableExitCodeAttribute : Attribute
    {
        public string Description { get; }

        public ReturnableExitCodeAttribute(string description)
        {
            Description = description;
        }
    }
}
