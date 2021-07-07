using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MaSch.CommandLineTools.Services
{
    [SupportedOSPlatform("linux")]
    public class LinuxOsService : IOsService
    {
        [DllImport("libc")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "P/Invoke method")]
        private static extern uint getuid();

        public bool IsRoot()
        {
            return getuid() == 0;
        }
    }
}