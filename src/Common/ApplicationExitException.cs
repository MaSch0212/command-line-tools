using System;

namespace MaSch.CommandLineTools.Common
{
    public class ApplicationExitException : Exception
    {
        public ExitCode ExitCode { get; }

        public ApplicationExitException(ExitCode exitCode)
        {
            ExitCode = exitCode;
        }

        public ApplicationExitException(ExitCode exitCode, string? message) : base(message)
        {
            ExitCode = exitCode;
        }

        public ApplicationExitException(ExitCode exitCode, string? message, Exception? innerException) : base(message, innerException)
        {
            ExitCode = exitCode;
        }
    }
}
