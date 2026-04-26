// BlackCorps.Updater — Args: <mainPid> <zipUrl> <targetExePath>
using System.IO.Compression;
using System.Net.Http;

string logFile = Path.Combine(Path.GetTempPath(), "BlackCorpsUpdater.log");
void Log(string msg)
{
    File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss}] {msg}\n");
    Console.WriteLine(msg);
}

if (args.Length < 3)
{
    Log("ERROR: Missing arguments");
    return;
}

int    mainPid   = int.Parse(args[0]);
string zipUrl    = args[1];
string targetExe = args[2];
string targetDir = Path.GetDirectoryName(targetExe)!;
string tempDir   = Path.Combine(Path.GetTempPath(), "BlackCorpsUpdate");
string zipPath   = Path.Combine(tempDir, "update.zip");
string unzipDir  = Path.Combine(tempDir, "unpacked");

Log($"Updater started. PID={mainPid}, Target={targetExe}");

try
{
    // Force kill main exe immediately
    Log("Force closing main app...");
    try
    {
        var p = System.Diagnostics.Process.GetProcessById(mainPid);
        p.Kill();
        p.WaitForExit(3000);
    }
    catch { }
    await Task.Delay(500);

    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
    Directory.CreateDirectory(tempDir);

    Log("Downloading update...");
    using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(20) };
    using var response = await http.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();

    long total = response.Content.Headers.ContentLength ?? 100_000_000L;
    long received = 0;
    var buf = new byte[131072];
    var lastPrint = DateTime.MinValue;

    Log($"Total size: {total / 1048576}MB");

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var stream = await response.Content.ReadAsStreamAsync())
    {
        int read;
        while ((read = await stream.ReadAsync(buf, 0, buf.Length)) > 0)
        {
            await fs.WriteAsync(buf, 0, read);
            received += read;
            if ((DateTime.UtcNow - lastPrint).TotalMilliseconds >= 300)
            {
                lastPrint = DateTime.UtcNow;
                int pct = (int)Math.Min(received * 100L / total, 99);
                string status = $"Downloading... {pct}% ({received / 1048576}MB / {total / 1048576}MB)";
                Log(status);
            }
        }
    }
    Log("Download complete.");

    Log("Extracting...");
    if (Directory.Exists(unzipDir)) Directory.Delete(unzipDir, true);
    ZipFile.ExtractToDirectory(zipPath, unzipDir);

    Log("Deleting old exe...");
    for (int i = 0; i < 10; i++)
    {
        try { if (File.Exists(targetExe)) File.Delete(targetExe); break; }
        catch { await Task.Delay(500); }
    }

    Log("Installing...");
    Log($"Source: {unzipDir}");
    Log($"Target: {targetDir}");
    Log($"Files to copy: {Directory.GetFiles(unzipDir, "*", SearchOption.AllDirectories).Length}");

    foreach (string src in Directory.GetFiles(unzipDir, "*", SearchOption.AllDirectories))
    {
        string fileName = Path.GetFileName(src);
        Log($"Found file: {fileName}");

        // Skip copying the updater itself (it's already running)
        if (fileName.Equals("BlackCorps.Updater.exe", StringComparison.OrdinalIgnoreCase))
        {
            Log($"Skipping updater: {fileName}");
            continue;
        }

        string dest = Path.Combine(targetDir, Path.GetRelativePath(unzipDir, src));
        Log($"Copying {src} -> {dest}");

        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        for (int i = 0; i < 5; i++)
        {
            try
            {
                File.Copy(src, dest, overwrite: true);
                Log($"Copied successfully: {fileName}");
                break;
            }
            catch (Exception ex)
            {
                Log($"Copy failed (attempt {i + 1}): {ex.Message}");
                await Task.Delay(500);
            }
        }
    }

    Log("Restarting app...");
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName         = targetExe,
        UseShellExecute  = true,
        WorkingDirectory = targetDir
    });

    // Set flag to prevent update loop on next run
    string flagPath = Path.Combine(Path.GetTempPath(), "BlackCorpsJustUpdated.flag");
    File.WriteAllText(flagPath, "updated");

    Log("Update complete! Closing in 2 seconds...");
    await Task.Delay(2000);
}
catch (Exception ex)
{
    Log($"UPDATE FAILED: {ex.Message}");
    Log($"Stack trace: {ex.StackTrace}");
    Log("Press any key to exit...");
    Console.ReadKey();
}
