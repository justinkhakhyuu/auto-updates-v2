using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShayanUI;

namespace BlackCorps.App;

internal static class UpdateChecker
{
    private const string VersionUrl  = "https://raw.githubusercontent.com/justinkhakhyuu/auto-updates-v2/main/version.txt";
    private const string ZipUrl      = "https://raw.githubusercontent.com/justinkhakhyuu/auto-updates-v2/main/BlackCorps.zip";
    private const string CurrentVer  = "1.0";

    // Flag file written by updater before relaunch — prevents update loop
    private static readonly string JustUpdatedFlag = Path.Combine(
        Path.GetTempPath(), "BlackCorpsJustUpdated.flag");

    public static bool WasJustUpdated()
    {
        if (!File.Exists(JustUpdatedFlag)) return false;
        File.Delete(JustUpdatedFlag);
        return true;
    }

    private static async Task<bool> WaitForInternetAsync(Action<string>? statusUpdate, int maxWaitSeconds = 30)
    {
        for (int elapsed = 0; elapsed < maxWaitSeconds; elapsed++)
        {
            try
            {
                using var ping = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                await ping.GetAsync("https://raw.githubusercontent.com");
                return true;
            }
            catch { }

            int remaining = maxWaitSeconds - elapsed - 1;
            statusUpdate?.Invoke($"Waiting for internet connection... ({remaining}s)");
            await Task.Delay(1000);
        }
        return false;
    }

    // Called by the loader — statusUpdate sets the loader's status label text
    public static async Task CheckAndApplyAsync(Form owner, Action<string>? statusUpdate = null)
    {
        if (WasJustUpdated()) return;

        try
        {
            void Status(string msg) => owner.Invoke(() => statusUpdate?.Invoke(msg));

            Status("Checking for updates...");
            bool online = await WaitForInternetAsync(msg => owner.Invoke(() => statusUpdate?.Invoke(msg)));
            if (!online)
            {
                owner.Invoke(() =>
                {
                    MessageBox.Show(
                        "No internet connection detected.\nPlease check your internet and try again.",
                        "BLACK CORPS — Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(0);
                });
                return;
            }

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            string latest = (await http.GetStringAsync(VersionUrl)).Trim();

            if (!IsNewer(latest, CurrentVer)) return;

            Status($"Update v{latest} found. Downloading...");
            ShayanNotificationManager.Show("Update", $"v{latest} available — downloading...", NotificationType.Info, 3000);

            string tempDir  = Path.Combine(Path.GetTempPath(), "BlackCorpsUpdate");
            string zipPath  = Path.Combine(tempDir, "update.zip");
            string unzipDir = Path.Combine(tempDir, "unpacked");

            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            // Download with real progress
            Status($"Downloading update...");
            using var response = await http.GetAsync(ZipUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                Status($"Download failed: {response.StatusCode}");
                return;
            }

            long total   = response.Content.Headers.ContentLength ?? -1;
            long received = 0;

            using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var buf = new byte[81920];
                int read;
                while ((read = await stream.ReadAsync(buf)) > 0)
                {
                    await fs.WriteAsync(buf.AsMemory(0, read));
                    received += read;
                    if (total > 0)
                    {
                        int pct = (int)(received * 100 / total);
                        Status($"Downloading update... {pct}%  ({received / 1048576}MB / {total / 1048576}MB)");
                    }
                    else
                    {
                        Status($"Downloading update... {received / 1048576}MB");
                    }
                }
            }

            Status("Extracting update...");
            if (Directory.Exists(unzipDir)) Directory.Delete(unzipDir, true);
            ZipFile.ExtractToDirectory(zipPath, unzipDir);

            string appExe   = Process.GetCurrentProcess().MainModule!.FileName;
            string appDir   = Path.GetDirectoryName(appExe)!; // Use actual exe directory, not temp extraction dir
            int    pid      = Environment.ProcessId;

            // Copy THIS exe to temp so it can overwrite itself
            string selfTemp = Path.Combine(tempDir, Path.GetFileName(appExe));
            File.Copy(appExe, selfTemp, overwrite: true);

            // Write flag so relaunched instance skips update check
            File.WriteAllText(JustUpdatedFlag, "updated");

            // Launch self from temp in updater mode
            var psi = new ProcessStartInfo
            {
                FileName        = selfTemp,
                UseShellExecute = false,
                CreateNoWindow  = true
            };
            psi.ArgumentList.Add("--updater-mode");
            psi.ArgumentList.Add(pid.ToString());
            psi.ArgumentList.Add(unzipDir);
            psi.ArgumentList.Add(appDir); // Actual exe directory
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
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "BlackCorpsUpdateError.log"), ex.ToString());
        }
    }

    private static bool IsNewer(string latest, string current)
    {
        if (Version.TryParse(latest, out var l) && Version.TryParse(current, out var c))
            return l > c;
        return string.Compare(latest, current, StringComparison.Ordinal) > 0;
    }
}
