using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BlackCorps.App;

internal static class InjectionHelper
{
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll")]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll")]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    public static void Inject()
    {
        try
        {
            if (Process.GetProcessesByName("HD-Player").Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("HD-Player is not running. Start the emulator first.");
                return;
            }

            CopyEmbeddedDllToTemp();
            string tempPath = Path.GetTempPath();
            InjectDll("BlackCorps.App.cimgui.dll", Path.Combine(tempPath, "cimgui.dll"));
            System.Threading.Thread.Sleep(200);
            InjectDll("BlackCorps.App.AotBst.dll", Path.Combine(tempPath, "AotBst.dll"));

            // Show 7-second delay popup to allow DLLs to initialize
            System.Windows.Forms.MessageBox.Show("Please wait 7 seconds for DLLs to initialize...", "Initializing");
            System.Threading.Thread.Sleep(7000);
            System.Windows.Forms.MessageBox.Show("Initialization complete. You can now use ESP and Aimbot features.", "Ready");
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
        }
    }

    private static void CopyEmbeddedDllToTemp()
    {
        string resourceName = "BlackCorps.App.Client.dll";
        string destinationPath = Path.Combine(Path.GetTempPath(), "Client.dll");

        using (var stream = typeof(InjectionHelper).Assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null) return;
            if (File.Exists(destinationPath)) File.Delete(destinationPath);
            using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }
    }

    private static void InjectDll(string resourceName, string tempPath)
    {
        ExtractEmbeddedResource(resourceName, tempPath);

        var processes = Process.GetProcessesByName("HD-Player");
        if (processes.Length == 0) return;

        var target = processes[0];
        IntPtr hProcess = OpenProcess(0x001F0FFF, false, target.Id);
        IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
        IntPtr allocMem = VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)(tempPath.Length + 1), 0x1000, 0x04);

        WriteProcessMemory(hProcess, allocMem, Encoding.ASCII.GetBytes(tempPath + "\0"), (uint)(tempPath.Length + 1), out _);
        CreateRemoteThread(hProcess, IntPtr.Zero, IntPtr.Zero, loadLibraryAddr, allocMem, 0, IntPtr.Zero);
    }

    private static void ExtractEmbeddedResource(string resourceName, string outputPath)
    {
        using (var stream = typeof(InjectionHelper).Assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null) return;
            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }
    }
}
