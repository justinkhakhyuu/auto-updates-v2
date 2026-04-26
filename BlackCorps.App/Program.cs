using System.Diagnostics;
using System.IO;

namespace BlackCorps.App;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Updater mode: --updater-mode <mainPid> <sourceDir> <targetDir> <exeToRestart>
        if (args.Length == 5 && args[0] == "--updater-mode")
        {
            RunUpdaterMode(args);
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm());
    }

    static void RunUpdaterMode(string[] args)
    {
        int    mainPid    = int.Parse(args[1]);
        string sourceDir  = args[2];
        string targetDir  = args[3];
        string exeRestart = args[4];

        // Wait for main process to exit
        try
        {
            var proc = Process.GetProcessById(mainPid);
            proc.WaitForExit(10000);
        }
        catch { }

        System.Threading.Thread.Sleep(500);

        // Copy new files into the app folder
        foreach (string srcFile in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDir, srcFile);
            string destFile = Path.Combine(targetDir, relative);
            string destDir  = Path.GetDirectoryName(destFile)!;
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
            try { File.Copy(srcFile, destFile, overwrite: true); }
            catch { }
        }

        System.Threading.Thread.Sleep(300);

        // Relaunch the main app
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName         = exeRestart,
                UseShellExecute  = true,
                WorkingDirectory = Path.GetDirectoryName(exeRestart)
            });
        }
        catch { }
    }
}