using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShayanUI;

namespace BlackCorps.App;

internal static class UpdateChecker
{
    private const string VersionUrl  = "https://raw.githubusercontent.com/justinkhakhyuu/auto-updates-v2/main/version.txt";
    private const string ZipUrl      = "https://raw.githubusercontent.com/justinkhakhyuu/auto-updates-v2/main/BlackCorps.zip";
    private const string CurrentVer  = "1.9";

    private static readonly string JustUpdatedFlag = Path.Combine(
        Path.GetTempPath(), "BlackCorpsJustUpdated.flag");

    public static bool WasJustUpdated()
    {
        if (!File.Exists(JustUpdatedFlag)) return false;
        File.Delete(JustUpdatedFlag);
        return true;
    }

    public static async Task CheckAndApplyAsync(Form owner, Action<string>? statusUpdate = null)
    {
        if (WasJustUpdated()) return;

        try
        {
            void Status(string msg) => owner.BeginInvoke(() => statusUpdate?.Invoke(msg));

            Status("Checking for updates...");

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            string latest = (await http.GetStringAsync(VersionUrl)).Trim();

            if (!IsNewer(latest, CurrentVer)) return;

            Status($"Update v{latest} found — launching updater...");
            ShayanNotificationManager.Show("Update", $"v{latest} available — updating...", NotificationType.Info, 3000);

            string appExe    = Process.GetCurrentProcess().MainModule!.FileName;
            string appDir    = Path.GetDirectoryName(appExe)!;
            string updaterExe = Path.Combine(appDir, "BlackCorps.Updater.exe");

            if (!File.Exists(updaterExe))
            {
                owner.BeginInvoke(() => MessageBox.Show(
                    "BlackCorps.Updater.exe not found in app folder.\nCannot update.",
                    "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                return;
            }

            File.WriteAllText(JustUpdatedFlag, "updated");

            var psi = new ProcessStartInfo
            {
                FileName        = updaterExe,
                UseShellExecute = true,
                CreateNoWindow  = false
            };
            psi.ArgumentList.Add(Environment.ProcessId.ToString());
            psi.ArgumentList.Add(ZipUrl);
            psi.ArgumentList.Add(appExe);
            Process.Start(psi);

            await Task.Delay(1000);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            owner.BeginInvoke(() => MessageBox.Show($"Update failed: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
        }
    }

    private static bool IsNewer(string latest, string current)
    {
        if (Version.TryParse(latest, out var l) && Version.TryParse(current, out var c))
            return l > c;
        return string.Compare(latest, current, StringComparison.Ordinal) > 0;
    }
}
