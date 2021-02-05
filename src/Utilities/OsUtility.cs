using System.Runtime.InteropServices;
using System.Security.Principal;

namespace MaSch.CommandLineTools.Utilities
{
    public static class OsUtility
    {
        [DllImport("libc")]
        public static extern uint getuid();

        public static bool IsRoot()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                bool isAdmin;
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                return isAdmin;
            }
            else
            {
                return getuid() == 0;
            }
        }
    }
}
