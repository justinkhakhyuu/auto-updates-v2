using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using ShayanUI;

namespace BlackCorps.App;

internal static class WinDivertHelper
{
    private const string SYSTEM32_PATH = @"C:\Windows\System32";
    private const string WINDIVERT_DLL = "WinDivert.dll";
    private const string WINDIVERT_SYS = "WinDivert64.sys";

    public static bool CheckWinDivertFiles()
    {
        string dllPath = Path.Combine(SYSTEM32_PATH, WINDIVERT_DLL);
        string sysPath = Path.Combine(SYSTEM32_PATH, WINDIVERT_SYS);

        return File.Exists(dllPath) && File.Exists(sysPath);
    }

    public static bool CopyWinDivertFiles()
    {
        try
        {
            if (!IsAdministrator())
            {
                ShayanNotificationManager.Show("WinDivert", "Admin privileges required to copy files", NotificationType.Info, 3000);
                return false;
            }

            string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
            string sourceDll = Path.Combine(sourceDir, WINDIVERT_DLL);
            string sourceSys = Path.Combine(sourceDir, WINDIVERT_SYS);
            string destDll = Path.Combine(SYSTEM32_PATH, WINDIVERT_DLL);
            string destSys = Path.Combine(SYSTEM32_PATH, WINDIVERT_SYS);

            if (!File.Exists(sourceDll) || !File.Exists(sourceSys))
            {
                ShayanNotificationManager.Show("WinDivert", "Source files not found in application directory", NotificationType.Info, 3000);
                return false;
            }

            // Copy DLL
            if (File.Exists(destDll))
            {
                File.Delete(destDll);
            }
            File.Copy(sourceDll, destDll);

            // Copy SYS
            if (File.Exists(destSys))
            {
                File.Delete(destSys);
            }
            File.Copy(sourceSys, destSys);

            ShayanNotificationManager.Show("WinDivert", "Files copied to System32 successfully", NotificationType.Success, 2000);
            return true;
        }
        catch (Exception ex)
        {
            ShayanNotificationManager.Show("WinDivert", $"Failed to copy files: {ex.Message}", NotificationType.Info, 3000);
            return false;
        }
    }

    private static bool IsAdministrator()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
