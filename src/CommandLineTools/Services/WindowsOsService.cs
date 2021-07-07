using System.Runtime.Versioning;
using System.Security.Principal;

namespace MaSch.CommandLineTools.Services
{
    [SupportedOSPlatform("windows")]
    public class WindowsOsService : IOsService
    {
        public bool IsRoot()
        {
            bool isAdmin;
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return isAdmin;
        }
    }
}
