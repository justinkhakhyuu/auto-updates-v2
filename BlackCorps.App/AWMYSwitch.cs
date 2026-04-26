using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ShayanUI;
using PANDA;

namespace BlackCorps.App;

internal static class AWMYSwitch
{
    private static P4NDA mem = new P4NDA();
    private static List<long> savedAddresses = new List<long>();
    private static Dictionary<long, byte[]> originalBytes = new Dictionary<long, byte[]>();
    private static bool isEnabled = false;

    private const string PROCESS = "HD-Player";
    private const string SEARCH_PATTERN = "FF FF FF FF 00 00 00 00 ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 05 00 00 00 01 00 00 00 B4 C8 D6 3F 01 00 00 00 00 00 00 00 B4 C8 D6 3F 00 00 00 00 B4 C8 D6 3F 00 00 80 3F 00 00 80 3F CD CC CC 3D 00 00 00 00 00 00 5C 43 00 00 90 42 00 00 B4 42 96 00 00 00 00 00 00 00 00 00 00 3F 00 00 80 3E 00 00 00 00 04 00 00 00 00 00 80 3F 00 00 20 41 00 00 34 42 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 8F C2 35 3F 9A 99 99 3F 00 00 80 3F 00 00 00 00 00 00 80 3F 00 00 80 3F 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 40 3F 00 00 00 00 00 00 80 3F 00 00 80 3F 00 00 80 3F ?? ?? ?? ?? 01 00 00 00 0A D7 23 3C CD CC CC 3D 9A 99 19 3F 1F 85 6B 3F 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3F 00 00 00 3F";
    private const string REPLACE_PATTERN = "FF FF FF FF 00 00 00 00 ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 05 00 00 00 01 00 00 00 B4 C8 D6 3F 01 00 00 00 00 00 00 00 B4 C8 D6 3F 00 00 00 00 B4 C8 D6 3F 00 00 80 3F 00 00 80 3F CD CC CC 3D 00 00 00 00 00 00 5C 43 00 00 90 42 00 00 B4 42 96 00 00 00 00 00 00 00 00 00 00 3B 00 00 80 3A 00 00 00 00 04 00 00 00 00 00 80 3F 00 00 20 41 00 00 34 42 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 8F C2 35 3F 9A 99 99 3F 00 00 80 3F 00 00 00 00 00 00 80 3F 00 00 80 3F 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 40 3F 00 00 00 00 00 00 80 3F 00 00 80 3F 00 00 80 3F ?? ?? ?? ?? 01 00 00 00 0A D7 23 3C CD CC CC 3D 9A 99 19 3F 1F 85 6B 3F 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3F 00 00 00 3F";

    public static bool IsEnabled() => isEnabled;

    public static async Task<bool> Initialize()
    {
        if (savedAddresses.Count > 0) return true;

        if (!mem.SetProcess(PROCESS))
        {
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("AWM-Y Switch", "Open Emulator - HD-Player Not Found", NotificationType.Info, 3000);
            return false;
        }

        var startTime = DateTime.Now;

        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("AWM-Y Switch", "Loading...", NotificationType.Info, 1800);

        var scan = await mem.AoBScan(SEARCH_PATTERN);

        if (!scan.Any())
        {
            var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("AWM-Y Switch", $"Pattern Not Found ({timeTaken}s)", NotificationType.Info, 3000);
            return false;
        }

        savedAddresses.AddRange(scan);

        var timeTakenSuccess = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("AWM-Y Switch", $"Loaded Successfully! ({timeTakenSuccess}s)", NotificationType.Info, 1800);

        return true;
    }

    public static bool Enable()
    {
        if (savedAddresses.Count == 0) return false;

        try
        {
            foreach (var address in savedAddresses)
            {
                if (!originalBytes.ContainsKey(address))
                {
                    originalBytes[address] = mem.ReadBytes(address, SEARCH_PATTERN.Split(' ').Length);
                }

                mem.WriteBytes(address, REPLACE_PATTERN.Split(' ').Select(s => byte.Parse(s)).ToArray());
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
                if (originalBytes.ContainsKey(address))
                {
                    mem.WriteBytes(address, originalBytes[address]);
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

    public static void Clear()
    {
        savedAddresses.Clear();
        originalBytes.Clear();
        isEnabled = false;
        Disable();
    }
}
