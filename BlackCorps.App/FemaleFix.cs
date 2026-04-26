using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ShayanUI;
using PANDA;

namespace BlackCorps.App;

internal static class FemaleFix
{
    private static P4NDA mem = new P4NDA();
    private static List<long> savedAddresses = new List<long>();
    private static Dictionary<long, byte[]> originalBytes = new Dictionary<long, byte[]>();
    private static bool isEnabled = false;

    private const string PROCESS = "HD-Player";
    private const string SEARCH_PATTERN = "6d 00 00 00 f2 a9 06 06 d3 72 17 06 41 23 05 06 45 23 05 06 47 23 05 06 48 23 05 06 49 23 05 06 4a 23 05 06 4c 23 05 06 4d 23 05 06 4e 23 05 06 50 23 05 06 51 23 05 06 52 23 05 06 53 23 05 06 54 23 05 06 55 23 05 06 56 23 05 06 58 23 05 06 59 23 05 06 5a 23 05 06 5b 23 05 06 5c 23 05 06 84 65 14 06 86 65 14 06 87 65 14 06 88 65 14 06 89 65 14 06 8a 65 14 06 8b 65 14 06 8d 65 14 06 8e 65 14 06 91 65 14 06 92 65 14 06 93 65 14 06 95 65 14 06 96 65 14 06 97 65 14 06 98 65 14 06 99 65 14 06 9a 65 14 06 9c 65 14 06 9d 65 14 06 9e 65 14 06 9f 65 14 06 a0 65 14 06 a1 65 14 06 a2 65 14 06 a3 65 14 06 a4 65 14 06 a5 65 14 06 a6 65 14 06 a7 65 14 06 a8 65 14 06 c2 a7 23 06 c3 a7 23 06 c4 a7 23 06 a9 65 14 06 ac 65 14 06 ad 65 14 06 ae 65 14 06 78 b3 12 06 7f 65 14 06 71 23 05 06 72 23 05 06 b3 65 14 06 46 23 05 06 4f 23 05 06 85 65 14 06 9b 65 14 06 4b 23 05 06 57 23 05 06 8c 65 14 06 94 65 14 06 8f 65 14 06 ab 65 14 06 90 65 14 06";
    private const string REPLACE_PATTERN = "6d 00 00 00 f2 a9 06 06 d3 72 17 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 84 65 14 06 86 65 14 06 87 65 14 06 88 65 14 06 89 65 14 06 8a 65 14 06 8b 65 14 06 8d 65 14 06 8e 65 14 06 91 65 14 06 92 65 14 06 93 65 14 06 95 65 14 06 96 65 14 06 97 65 14 06 98 65 14 06 99 65 14 06 9a 65 14 06 9c 65 14 06 9d 65 14 06 9e 65 14 06 9f 65 14 06 a0 65 14 06 a1 65 14 06 a2 65 14 06 a3 65 14 06 a4 65 14 06 a5 65 14 06 a6 65 14 06 a7 65 14 06 a8 65 14 06 c2 a7 23 06 c3 a7 23 06 c4 a7 23 06 a9 65 14 06 ac 65 14 06 ad 65 14 06 ae 65 14 06 78 b3 12 06 85 65 14 06 85 65 14 06 72 23 05 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 85 65 14 06 57 65 14 06 8c 65 14 06 8f 65 14 06 8f 65 14 06 ab 65 14 06 aa 65 14 06";

    public static bool IsEnabled() => isEnabled;

    public static async Task<bool> Initialize()
    {
        if (savedAddresses.Count > 0) return true;

        if (!mem.SetProcess(PROCESS))
        {
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Female Fix", "Open Emulator - HD-Player Not Found", NotificationType.Info, 3000);
            return false;
        }

        var startTime = DateTime.Now;

        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("Female Fix", "Loading...", NotificationType.Info, 1800);

        var scan = await mem.AoBScan(SEARCH_PATTERN);

        if (!scan.Any())
        {
            var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Female Fix", $"Pattern Not Found ({timeTaken}s)", NotificationType.Info, 3000);
            return false;
        }

        savedAddresses.AddRange(scan);

        var timeTakenSuccess = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("Female Fix", $"Loaded Successfully! ({timeTakenSuccess}s)", NotificationType.Info, 1800);

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
