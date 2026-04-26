using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AotForms
{
    internal static class WinAPI
    {
        // ===== KEYBOARD & MOUSE INPUT =====

        // Overload 1: Accepts integer IDs (0x01, 0x14, etc.)
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        // Overload 2: Accepts System.Windows.Forms.Keys (Keys.LButton, Keys.F1, etc.)
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        internal static extern bool ReleaseCapture();


        // ===== WINDOW MANAGEMENT =====

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        // ===== GRAPHICS & DESIGN =====

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        internal static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);


        // ===== CONSTANTS =====

        public const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
        public const uint WDA_NONE = 0x00000000;
        public const int GWL_EXSTYLE = -20;
        public const long WS_EX_APPWINDOW = 0x00040000L;
        public const long WS_EX_TOOLWINDOW = 0x00000080L;
        public const UInt32 SWP_NOACTIVATE = 0x0010;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        internal const int WM_NCLBUTTONDOWN = 0xA1;
        internal const int HT_CAPTION = 0x2;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);


        // ===== STRUCTS & DELEGATES =====

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        // ===== CUSTOM UTILITIES =====

        internal static uint Convert1(byte[] bytes)
        {
            int flag = Environment.TickCount & 0x1;
            uint memAddr = BitConverter.ToUInt32(bytes, 0);
            if (flag == 0)
            {
                memAddr = (memAddr ^ 0xF0F0F0F0) ^ 0xF0F0F0F0;
            }
            return memAddr;
        }
    }
}