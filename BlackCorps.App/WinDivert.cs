using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace BlackCorps.App;

internal static class WinDivert
{
    private const string DLL_NAME = "WinDivert.dll";

    static WinDivert()
    {
        // Try to load WinDivert.dll from application directory first
        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        string dllPath = Path.Combine(appDir, DLL_NAME);
        
        if (File.Exists(dllPath))
        {
            SetDllDirectory(appDir);
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPath);

    #region P/Invoke Declarations

    [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr WinDivertOpen(
        string filter,
        WinDivertLayer layer,
        short priority,
        ulong flags);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool WinDivertClose(
        IntPtr handle);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool WinDivertRecv(
        IntPtr handle,
        [Out] byte[] packet,
        int packetLen,
        out int recvLen,
        out WinDivertAddress addr);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool WinDivertSend(
        IntPtr handle,
        byte[] packet,
        int packetLen,
        out int sendLen,
        ref WinDivertAddress addr);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr WinDivertHelperCompileFilter(
        string filter,
        int length,
        out string error);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void WinDivertHelperFreeFilter(
        IntPtr filter);

    #endregion

    #region Constants

    public const int WINDIVERT_LAYER_NETWORK = 0;
    public const int WINDIVERT_LAYER_NETWORK_FORWARD = 1;
    public const int WINDIVERT_LAYER_FLOW = 2;
    public const int WINDIVERT_LAYER_SOCKET = 3;
    public const int WINDIVERT_LAYER_REFLECT = 4;

    public const int MAX_PACKET_SIZE = 0xFFFF;

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct WinDivertAddress
    {
        public ulong Timestamp;
        public uint Reserved;
        public ushort Reserved2;
        public byte Layer;
        public byte Event;
        public byte Reserved3;
        public byte Reserved4;
        public uint Sniffed;
        public byte Outbound;
        public byte Loopback;
        public ushort Impostor;
        public uint IPv6;
        public byte Reserved5;
        public byte Reserved6;
        public byte Reserved7;
        public byte Reserved8;
    }

    public enum WinDivertLayer
    {
        Network = 0,
        NetworkForward = 1,
        Flow = 2,
        Socket = 3,
        Reflect = 4
    }

    #endregion
}
