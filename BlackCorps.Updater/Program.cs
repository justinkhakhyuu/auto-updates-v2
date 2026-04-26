// BlackCorps.Updater — Args: <mainPid> <zipUrl> <targetExePath>
using System.IO.Compression;
using System.Net.Http;

if (args.Length < 3) return;

int    mainPid   = int.Parse(args[0]);
string zipUrl    = args[1];
string targetExe = args[2];
string targetDir = Path.GetDirectoryName(targetExe)!;
string tempDir   = Path.Combine(Path.GetTempPath(), "BlackCorpsUpdate");
string zipPath   = Path.Combine(tempDir, "update.zip");
string unzipDir  = Path.Combine(tempDir, "unpacked");

try
{
    // Force kill main exe immediately
    Console.WriteLine("Force closing main app...");
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

    Console.WriteLine("Downloading update...");
    using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
    using var response = await http.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();

    long total    = response.Content.Headers.ContentLength ?? 70_000_000L;
    long received = 0;
    var  buf      = new byte[65536];
    var  lastPrint = DateTime.MinValue;

    using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
    using (var stream = await response.Content.ReadAsStreamAsync())
    {
        int read;
        while ((read = await stream.ReadAsync(buf, 0, buf.Length)) > 0)
        {
            await fs.WriteAsync(buf, 0, read);
            received += read;
            if ((DateTime.UtcNow - lastPrint).TotalMilliseconds >= 500)
            {
                lastPrint = DateTime.UtcNow;
                int pct = (int)Math.Min(received * 100L / total, 99);
                Console.Write($"\rDownloading... {pct}% ({received / 1048576}MB / {total / 1048576}MB)  ");
            }
        }
    }
    Console.WriteLine("\nDownload complete.");

    Console.WriteLine("Extracting...");
    if (Directory.Exists(unzipDir)) Directory.Delete(unzipDir, true);
    ZipFile.ExtractToDirectory(zipPath, unzipDir);

    Console.WriteLine("Deleting old exe...");
    for (int i = 0; i < 10; i++)
    {
        try { if (File.Exists(targetExe)) File.Delete(targetExe); break; }
        catch { await Task.Delay(500); }
    }

    Console.WriteLine("Installing...");
    foreach (string src in Directory.GetFiles(unzipDir, "*", SearchOption.AllDirectories))
    {
        string dest = Path.Combine(targetDir, Path.GetRelativePath(unzipDir, src));
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        for (int i = 0; i < 5; i++)
        {
            try { File.Copy(src, dest, overwrite: true); break; }
            catch { await Task.Delay(500); }
        }
    }

    Console.WriteLine("Restarting app...");
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName         = targetExe,
        UseShellExecute  = true,
        WorkingDirectory = targetDir
    });

    Console.WriteLine("Update complete! Closing in 2 seconds...");
    await Task.Delay(2000);
}
catch (Exception ex)
{
    Console.WriteLine($"\nUpdate FAILED: {ex.Message}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
