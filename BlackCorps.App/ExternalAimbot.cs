using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Celestial;
using ShayanUI;

namespace BlackCorps.App;

internal static class ExternalAimbot
{
    private static Cosmic AIMX = new Cosmic();
    private static Dictionary<long, int> OriginalValues1 = new Dictionary<long, int>();
    private static Dictionary<long, int> OriginalValues2 = new Dictionary<long, int>();
    private static Dictionary<long, int> OriginalValues3 = new Dictionary<long, int>();
    private static Dictionary<long, int> OriginalValues4 = new Dictionary<long, int>();

    private const string AimbotScan1 = "00 00 00 00 00 FF FF FF FF FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";
    private const long chestoffset1 = 0x91;
    private const long headoffset2 = 0x95;

    public static async Task EnableAimbotExternal()
    {
        var startTime = DateTime.Now;
        
        if (!AIMX.SetProcess(new[] { "HD-Player" }))
        {
            if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("Aimbot External", "HD-Player is not running", NotificationType.Info, 1800);
            return;
        }

        OriginalValues1.Clear();
        OriginalValues2.Clear();
        OriginalValues3.Clear();
        OriginalValues4.Clear();

        if (!MainMenu.MuteNotifications)
            ShayanNotificationManager.Show("Aimbot External", "Scanning memory...", NotificationType.Info, 1800);
        
        var result = await AIMX.AoBScan(AimbotScan1);

        if (result.Count() != 0)
        {
            foreach (var CurrentAddress in result)
            {
                long addressToSave = CurrentAddress + chestoffset1;
                byte[] currentBytes = new byte[4];
                AIMX.ReadMemory(addressToSave, currentBytes);
                int currentValue = BitConverter.ToInt32(currentBytes, 0);
                OriginalValues1[addressToSave] = currentValue;

                long addressToSave9 = CurrentAddress + headoffset2;
                byte[] currentBytes9 = new byte[4];
                AIMX.ReadMemory(addressToSave9, currentBytes9);
                int currentValue9 = BitConverter.ToInt32(currentBytes9, 0);
                OriginalValues2[addressToSave9] = currentValue9;

                long headbytes = CurrentAddress + headoffset2;
                long chestbytes = CurrentAddress + chestoffset1;
                byte[] bytes = new byte[4];
                AIMX.ReadMemory(headbytes, bytes);
                int Read = BitConverter.ToInt32(bytes, 0);
                byte[] bytes2 = new byte[4];
                AIMX.ReadMemory(chestbytes, bytes2);
                int Read2 = BitConverter.ToInt32(bytes2, 0);
                AIMX.WriteMemory(chestbytes, BitConverter.GetBytes(Read));
                AIMX.WriteMemory(headbytes, BitConverter.GetBytes(Read2));

                long addressToSave1 = CurrentAddress + chestoffset1;
                byte[] currentBytes1 = new byte[4];
                AIMX.ReadMemory(addressToSave1, currentBytes1);
                int currentValue1 = BitConverter.ToInt32(currentBytes1, 0);
                OriginalValues3[addressToSave1] = currentValue1;
                long addressToSave19 = CurrentAddress + headoffset2;
                byte[] currentBytes19 = new byte[4];
                AIMX.ReadMemory(addressToSave19, currentBytes19);
                int currentValue19 = BitConverter.ToInt32(currentBytes19, 0);
                OriginalValues3[addressToSave19] = currentValue19;
            }
            
            var endTime = DateTime.Now;
            var timeTaken = (endTime - startTime).TotalSeconds.ToString("F2");
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Aimbot External", $"Applied in {timeTaken}s", NotificationType.Info, 1800);
        }
        else
        {
            if (!MainMenu.MuteNotifications)
                ShayanNotificationManager.Show("Aimbot External", "AOB scan failed - no addresses found", NotificationType.Info, 1800);
        }
    }

    public static void DisableAimbotExternal()
    {
        foreach (var entry in OriginalValues1)
        {
            AIMX.WriteMemory(entry.Key, BitConverter.GetBytes(entry.Value));
        }
        foreach (var entry in OriginalValues2)
        {
            AIMX.WriteMemory(entry.Key, BitConverter.GetBytes(entry.Value));
        }
    }

    public static void EnableAimbotExternalOn()
    {
        foreach (var entry in OriginalValues3)
        {
            AIMX.WriteMemory(entry.Key, BitConverter.GetBytes(entry.Value));
        }
        foreach (var entry in OriginalValues4)
        {
            AIMX.WriteMemory(entry.Key, BitConverter.GetBytes(entry.Value));
        }
    }
}
