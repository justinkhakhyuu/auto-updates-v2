using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

string log = Path.Combine(Path.GetTempPath(), "BlackCorpsUpdater.log");
void Log(string msg) => File.AppendAllText(log, $"[{DateTime.Now:HH:mm:ss}] {msg}\n");

Log("Updater started. Args: " + string.Join(" | ", args));

// Args: <mainPid> <sourceDir> <targetDir> <exeToRestart>
if (args.Length < 4) { Log("Not enough args, exiting."); return; }

int    mainPid    = int.Parse(args[0]);
string sourceDir  = args[1];
string targetDir  = args[2];
string exeRestart = args[3];

Log($"PID={mainPid} source={sourceDir} target={targetDir} exe={exeRestart}");

// Wait for main process to exit
try
{
    var proc = Process.GetProcessById(mainPid);
    Log("Waiting for main process to exit...");
    proc.WaitForExit(10000);
    Log("Main process exited.");
}
catch { Log("Main process already gone."); }

Thread.Sleep(500);

// Copy files
Log("Copying files...");
foreach (string srcFile in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
{
    string relative = Path.GetRelativePath(sourceDir, srcFile);
    string destFile = Path.Combine(targetDir, relative);
    string destDir  = Path.GetDirectoryName(destFile)!;
    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
    try { File.Copy(srcFile, destFile, overwrite: true); Log($"Copied: {relative}"); }
    catch (Exception ex) { Log($"Skip {relative}: {ex.Message}"); }
}

Thread.Sleep(300);

// Find exe to launch
string exePath = exeRestart;
if (!File.Exists(exePath))
{
    Log($"Exe not found at: {exePath}, searching targetDir...");
    exePath = Directory.GetFiles(targetDir, "*.exe")
                       .FirstOrDefault(f => !f.Contains("Updater")) ?? exeRestart;
    Log($"Found fallback exe: {exePath}");
}
else
{
    Log($"Exe found at: {exePath}");
}

// Launch
try
{
    Log($"Launching: {exePath}");
    var psi = new ProcessStartInfo
    {
        FileName         = exePath,
        UseShellExecute  = true,
        WorkingDirectory = Path.GetDirectoryName(exePath)
    };
    Process.Start(psi);
    Log("Launch successful.");
}
catch (Exception ex) { Log($"Launch FAILED: {ex.Message}"); }
