using System;
using System.Runtime.InteropServices;
using System.Threading;
using ShayanUI;

namespace BlackCorps.App;

internal static class AimLag
{
    private static IntPtr handle = IntPtr.Zero;
    private static Thread lagThread;
    private static bool isActive = false;
    private static CancellationTokenSource cts;

    private const string FILTER = "(inbound or outbound) and udp.PayloadLength >= 48";
    private static readonly object INVALID = new IntPtr(-1);

    public static bool IsActive => isActive;

    public static bool Start()
    {
        if (isActive) return false;

        try
        {
            handle = WinDivert.WinDivertOpen(FILTER, WinDivert.WinDivertLayer.Network, 0, 0);
            if (handle == IntPtr.Zero || handle == (IntPtr)INVALID)
            {
                return false;
            }

            isActive = true;
            cts = new CancellationTokenSource();
            lagThread = new Thread(() => LagLoop(cts.Token)) { IsBackground = true };
            lagThread.Start();

            // Suppress notification for keybind activation
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static void Stop()
    {
        if (!isActive) return;

        isActive = false;
        cts?.Cancel();

        try
        {
            lagThread?.Join(500);
        }
        catch { }

        if (handle != IntPtr.Zero && handle != new IntPtr(-1))
        {
            WinDivert.WinDivertClose(handle);
            handle = IntPtr.Zero;
        }

        // Suppress notification for keybind deactivation
    }

    private static void LagLoop(CancellationToken ct)
    {
        byte[] packet = new byte[WinDivert.MAX_PACKET_SIZE];
        WinDivert.WinDivertAddress addr;

        while (!ct.IsCancellationRequested && isActive)
        {
            try
            {
                int recvLen;
                if (!WinDivert.WinDivertRecv(handle, packet, packet.Length, out recvLen, out addr))
                    continue;

                // Delay 100ms then re-send the packet (creates aim lag)
                Thread.Sleep(100);
                int sendLen;
                WinDivert.WinDivertSend(handle, packet, recvLen, out sendLen, ref addr);
            }
            catch
            {
                Thread.Sleep(1);
            }
        }
    }
}
