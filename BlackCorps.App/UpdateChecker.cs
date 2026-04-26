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
    private const string CurrentVer  = "1.3";

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
            void Status(string msg) => owner.Invoke(() => statusUpdate?.Invoke(msg));

            Status("Checking for updates...");

            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            string latest = (await http.GetStringAsync(VersionUrl)).Trim();

            if (!IsNewer(latest, CurrentVer)) return;

            Status($"Update v{latest} found. Downloading...");
            ShayanNotificationManager.Show("Update", $"v{latest} available — downloading...", NotificationType.Info, 3000);

            string tempDir  = Path.Combine(Path.GetTempPath(), "BlackCorpsUpdate");
            string zipPath  = Path.Combine(tempDir, "update.zip");
            string unzipDir = Path.Combine(tempDir, "unpacked");

            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            Status("Downloading update... 0%");
            using (var response = await http.GetAsync(ZipUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                long total = response.Content.Headers.ContentLength ?? -1;
                long received = 0;
                var buf = new byte[65536];
                using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var stream = await response.Content.ReadAsStreamAsync();
                int read;
                while ((read = await stream.ReadAsync(buf, 0, buf.Length)) > 0)
                {
                    await fs.WriteAsync(buf, 0, read);
                    received += read;
                    if (total > 0)
                    {
                        int pct = (int)(received * 100L / total);
                        Status($"Downloading update... {pct}%  ({received / 1048576}MB / {total / 1048576}MB)");
                    }
                    else
                    {
                        Status($"Downloading update... {received / 1048576}MB");
                    }
                }
            }

            Status("Extracting update...");
            ZipFile.ExtractToDirectory(zipPath, unzipDir);

            string appExe   = Process.GetCurrentProcess().MainModule!.FileName;
            string appDir   = Path.GetDirectoryName(appExe)!;
            int    pid      = Environment.ProcessId;

            string selfTemp = Path.Combine(tempDir, Path.GetFileName(appExe));
            File.Copy(appExe, selfTemp, overwrite: true);

            File.WriteAllText(JustUpdatedFlag, "updated");

            var psi = new ProcessStartInfo
            {
                FileName        = selfTemp,
                UseShellExecute = false,
                CreateNoWindow  = true
            };
            psi.ArgumentList.Add("--updater-mode");
            psi.ArgumentList.Add(pid.ToString());
            psi.ArgumentList.Add(unzipDir);
            psi.ArgumentList.Add(appDir);
            psi.ArgumentList.Add(appExe);
            Process.Start(psi);

            Status($"v{latest} ready — restarting...");
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Update", $"v{latest} ready. Restarting...", NotificationType.Info, 2000);

            await Task.Delay(1800);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            owner.Invoke(() => MessageBox.Show($"Update failed: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
        }
    }

    private static bool IsNewer(string latest, string current)
    {
        if (Version.TryParse(latest, out var l) && Version.TryParse(current, out var c))
            return l > c;
        return string.Compare(latest, current, StringComparison.Ordinal) > 0;
    }
}
