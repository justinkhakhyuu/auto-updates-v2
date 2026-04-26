using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ShayanUI;
using PANDA;

namespace BlackCorps.App;

internal class AimbotAI
{
    private static P4NDA mem = new P4NDA();
    private static Thread aimThread;
    private static bool isRunning = false;

    // Config
    private const string PROCESS = "HD-Player";
    private const string AOB_PATTERN = "FF FF FF FF 00 00 00 00 00 00 00 00 FF FF FF FF ?? ?? ?? ?? 00 00 00 00 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 01 01 01";
    private const long READ_OFFSET = 0x2EC;
    private const long WRITE_OFFSET = 0x2E8;

    private static List<long> baseAddresses = new List<long>();
    private static Dictionary<long, int> patchedMemory = new Dictionary<long, int>();

    private static DateTime lastRW = DateTime.MinValue;
    private const int RW_DELAY_MS = 10;

    public bool IsInitialized() => isRunning && aimThread != null && aimThread.IsAlive;

    public async Task<bool> InitAimbot()
    {
        if (isRunning) return true;

        var startTime = DateTime.Now;

        if (!mem.SetProcess(PROCESS))
        {
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Aimbot Collider", "Open Emulator - HD-Player Not Found", NotificationType.Info, 3000);
            return false;
        }

        baseAddresses.Clear();
        patchedMemory.Clear();

        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("Aimbot Collider", "Applying...", NotificationType.Info, 1800);

        var scan = await mem.AoBScan(AOB_PATTERN);

        if (!scan.Any())
        {
            var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Aimbot Collider", $"Pattern Not Found ({timeTaken}s)", NotificationType.Info, 3000);
            return false;
        }

        baseAddresses.AddRange(scan);

        isRunning = true;
        aimThread = new Thread(MainLoop) { IsBackground = true };
        aimThread.Start();

        var timeTakenSuccess = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("Aimbot Collider", $"Applied Successfully! ({timeTakenSuccess}s)", NotificationType.Info, 1800);

        return true;
    }

    private void MainLoop()
    {
        while (isRunning)
        {
            try
            {
                // RW limiter
                if ((DateTime.Now - lastRW).TotalMilliseconds < RW_DELAY_MS)
                {
                    Thread.Sleep(1);
                    continue;
                }

                lastRW = DateTime.Now;

                foreach (long baseAddr in baseAddresses)
                {
                    long readAddr = baseAddr + READ_OFFSET;
                    long writeAddr = baseAddr + WRITE_OFFSET;

                    byte[] readBytes = mem.ReadBytes(readAddr, 4);
                    if (readBytes == null) continue;

                    int headValue = BitConverter.ToInt32(readBytes, 0);
                    if (headValue == 0) continue;

                    mem.WriteBytes(writeAddr, BitConverter.GetBytes(headValue));
                }

                Thread.Sleep(1);
            }
            catch
            {
                Thread.Sleep(20);
            }
        }

        patchedMemory.Clear();
    }

    public void Stop()
    {
        isRunning = false;
        try { aimThread?.Join(300); } catch { }
        patchedMemory.Clear();
    }
}
