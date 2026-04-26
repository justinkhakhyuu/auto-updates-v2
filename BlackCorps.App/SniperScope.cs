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

internal static class SniperScope
{
    private static P4NDA mem = new P4NDA();
    private static List<long> savedAddresses = new List<long>();
    private static Dictionary<long, byte[]> originalBytes = new Dictionary<long, byte[]>();
    private static bool isEnabled = false;
    private static bool macroActive = false;
    private static Thread macroThread;
    private static CancellationTokenSource macroCts;

    private const string PROCESS = "HD-Player";
    private const string SCOPE_SEARCH_PATTERN = "FF FF FF FF 08 00 00 00 00 00 60 40 CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 00 00 00 00 00 00 00 00 00 00 F0 41 00 00 48 42 00 00 00 3F 33 33 13 40 00 00 B0 3F 00 00 80 3F 01";
    private const string SCOPE_REPLACE_PATTERN = "FF FF FF FF 08 00 00 00 00 00 60 40 CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF 33 33 13 40 00 00 B0 3F 00 00 80 3F 01";
    private const int REPLACE_OFFSET = 0x18; // 24 bytes

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public static bool IsEnabled() => isEnabled;
    public static bool IsMacroActive() => macroActive;

    public static async Task<bool> Initialize()
    {
        if (savedAddresses.Count > 0) return true;

        if (!mem.SetProcess(PROCESS))
        {
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Sniper Scope", "Open Emulator - HD-Player Not Found", NotificationType.Info, 3000);
            return false;
        }

        var startTime = DateTime.Now;

        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("Sniper Scope", "Loading...", NotificationType.Info, 1800);

        var scan = await mem.AoBScan(SCOPE_SEARCH_PATTERN);

        if (!scan.Any())
        {
            var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Sniper Scope", $"Pattern Not Found ({timeTaken}s)", NotificationType.Info, 3000);
            return false;
        }

        savedAddresses.AddRange(scan);

        var timeTakenSuccess = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("Sniper Scope", $"Loaded Successfully! ({timeTakenSuccess}s)", NotificationType.Info, 1800);

        return true;
    }

    public static bool Enable()
    {
        if (savedAddresses.Count == 0) return false;

        try
        {
            foreach (var address in savedAddresses)
            {
                // Save original bytes before replacing
                if (!originalBytes.ContainsKey(address))
                {
                    originalBytes[address] = mem.ReadBytes(address, SCOPE_SEARCH_PATTERN.Split(' ').Length);
                }

                // Replace pattern - the difference is at offset +24 (0x18)
                mem.WriteBytes(address + REPLACE_OFFSET, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            }

            isEnabled = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool Disable()
    {
        if (savedAddresses.Count == 0) return false;

        try
        {
            foreach (var address in savedAddresses)
            {
                // Restore original bytes
                if (originalBytes.ContainsKey(address))
                {
                    mem.WriteBytes(address + REPLACE_OFFSET, new byte[] { originalBytes[address][REPLACE_OFFSET], originalBytes[address][REPLACE_OFFSET + 1], originalBytes[address][REPLACE_OFFSET + 2], originalBytes[address][REPLACE_OFFSET + 3] });
                }
            }

            isEnabled = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void StartMacro()
    {
        if (macroActive) return;

        macroCts = new CancellationTokenSource();
        macroActive = true;
        macroThread = new Thread(() => MacroLoop(macroCts.Token)) { IsBackground = true };
        macroThread.Start();
    }

    public static void StopMacro()
    {
        macroActive = false;
        macroCts?.Cancel();
        try { macroThread?.Join(500); } catch { }
        Disable();
    }

    private static void MacroLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && macroActive)
        {
            try
            {
                if (savedAddresses.Count > 0)
                {
                    Enable();
                }
                Thread.Sleep(10);
            }
            catch
            {
                Thread.Sleep(50);
            }
        }
    }

    public static void Clear()
    {
        savedAddresses.Clear();
        originalBytes.Clear();
        isEnabled = false;
        StopMacro();
    }
}
