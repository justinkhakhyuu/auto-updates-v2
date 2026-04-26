using ShayanUI;
using System.Runtime.InteropServices;

namespace BlackCorps.App;

public partial class MainMenu : Form
{
    private bool  _dragging;
    private Point _dragStart;

    // Keybind registry: key → (checkbox, last toggle time)
    private readonly Dictionary<Keys, (ShayanCheckBox cb, string name)> _keybinds = new();
    private readonly Dictionary<Keys, DateTime> _lastToggle = new();
    private const int ToggleCooldownMs = 300;

    // Mouse button keybinds
    private readonly Dictionary<int, (ShayanCheckBox cb, string name)> _mouseKeybinds = new();
    private readonly Dictionary<Keys, bool> _keyPressed = new();
    private readonly Dictionary<int, bool> _mouseKeyPressed = new();

    // Mute setting for notification sounds
    internal static bool MuteNotifications = false;

    // AimbotAI instance for collider
    internal static AimbotAI? AimbotColliderInstance;

    // Global hook handles
    private IntPtr _keyboardHook = IntPtr.Zero;
    private IntPtr _mouseHook = IntPtr.Zero;
    private LowLevelKeyboardProc _keyboardProc;
    private LowLevelMouseProc _mouseProc;

    [DllImport("user32.dll")]
    public static extern uint SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int HC_ACTION = 0;

    public MainMenu()
    {
        InitializeComponent();
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.UserPaint |
                      ControlStyles.DoubleBuffer, true);
        ConfigPipe.Initialize();
        SetupGlobalHooks();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        RemoveGlobalHooks();
        ConfigPipe.Close();
        base.OnFormClosed(e);
    }

    private void SetupGlobalHooks()
    {
        _keyboardProc = KeyboardHookProc;
        _mouseProc = MouseHookProc;

        IntPtr hModule = GetModuleHandle(null);
        _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, hModule, 0);
        _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, hModule, 0);
    }

    private void RemoveGlobalHooks()
    {
        if (_keyboardHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }
        if (_mouseHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }
    }

    private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Keys key = (Keys)vkCode;

            if ((int)wParam == WM_KEYDOWN)
            {
                FireKeyDown(key);
            }
            else if ((int)wParam == WM_KEYUP)
            {
                FireKeyUp(key);
            }
        }
        return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int message = (int)wParam;
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

            if (message == WM_LBUTTONDOWN)
            {
                FireMouseButtonDown(WM_LBUTTONDOWN);
            }
            else if (message == WM_LBUTTONUP)
            {
                FireMouseButtonUp(WM_LBUTTONDOWN);
            }
            else if (message == WM_RBUTTONDOWN)
            {
                FireMouseButtonDown(WM_RBUTTONDOWN);
            }
            else if (message == WM_RBUTTONUP)
            {
                FireMouseButtonUp(WM_RBUTTONDOWN);
            }
            else if (message == WM_MBUTTONDOWN)
            {
                FireMouseButtonDown(WM_MBUTTONDOWN);
            }
            else if (message == WM_MBUTTONUP)
            {
                FireMouseButtonUp(WM_MBUTTONDOWN);
            }
        }
        return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    internal void WireConfigPipe(ShayanCheckBox cb, string configKey)
    {
        cb.CheckedChanged += (s, e) =>
        {
            ConfigPipe.Send(configKey, cb.Checked);
        };
    }

    // Called from Designer to register a keybind box with its checkbox
    internal void RegisterKeybind(TextBox kb, ShayanCheckBox cb, string name)
    {
        kb.Tag = (object)false;
        kb.Click += (s, e) =>
        {
            kb.Text      = "...";
            kb.ForeColor = Color.White;
            kb.Tag       = (object)true;
            kb.Focus();
        };
        kb.KeyDown += (s, e) =>
        {
            if (!(bool)kb.Tag) return;
            e.SuppressKeyPress = true;
            kb.Tag = (object)false;

            if (e.KeyCode == Keys.Escape)
            {
                // Remove old binding if any
                var oldKey = _keybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
                if (oldKey != default) _keybinds.Remove(oldKey);
                var oldMouse = _mouseKeybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
                if (oldMouse != default) _mouseKeybinds.Remove(oldMouse);
                kb.Text      = "None";
                kb.ForeColor = Color.FromArgb(138, 43, 226);
                System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                    ShayanNotificationManager.Show("Keybind", $"{name} keybind cleared",
                        NotificationType.Info, 1800)));
                return;
            }

            // Regular keyboard keybind
            if (_keybinds.TryGetValue(e.KeyCode, out var existing) && existing.cb != cb)
            {
                kb.Text      = "None";
                kb.ForeColor = Color.FromArgb(138, 43, 226);
                System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                    ShayanNotificationManager.Show("Keybind Conflict",
                        $"{e.KeyCode} already used by {existing.name}",
                        NotificationType.Info, 2500)));
                return;
            }

            // Remove any old binding for this checkbox
            var prevKey = _keybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
            if (prevKey != default) _keybinds.Remove(prevKey);
            var prevMouse = _mouseKeybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
            if (prevMouse != default) _mouseKeybinds.Remove(prevMouse);

            _keybinds[e.KeyCode] = (cb, name);
            kb.Text      = e.KeyCode.ToString();
            kb.ForeColor = Color.FromArgb(180, 255, 180);
            System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                ShayanNotificationManager.Show("Keybind",
                    $"{name} bound to: {e.KeyCode}",
                    NotificationType.Success, 1800)));
        };
        // Handle mouse button clicks on the keybind box
        kb.MouseDown += (s, e) =>
        {
            if (!(bool)kb.Tag) return;
            kb.Tag = (object)false;

            int mouseButton = e.Button == MouseButtons.Left ? WM_LBUTTONDOWN :
                              e.Button == MouseButtons.Right ? WM_RBUTTONDOWN : WM_MBUTTONDOWN;

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                if (_mouseKeybinds.TryGetValue(mouseButton, out var existing) && existing.cb != cb)
                {
                    kb.Text      = "None";
                    kb.ForeColor = Color.FromArgb(138, 43, 226);
                    System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                        ShayanNotificationManager.Show("Keybind Conflict",
                            $"{e.Button} already used by {existing.name}",
                            NotificationType.Info, 2500)));
                    return;
                }

                var prevKey = _keybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
                if (prevKey != default) _keybinds.Remove(prevKey);
                var prevMouse = _mouseKeybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
                if (prevMouse != default) _mouseKeybinds.Remove(prevMouse);

                _mouseKeybinds[mouseButton] = (cb, name);
                kb.Text      = e.Button.ToString();
                kb.ForeColor = Color.FromArgb(180, 255, 180);
                System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                    ShayanNotificationManager.Show("Keybind",
                        $"{name} bound to: {e.Button}",
                        NotificationType.Success, 1800)));
            }
        };
        kb.LostFocus += (s, e) =>
        {
            if ((bool)kb.Tag)
            {
                kb.Text      = "None";
                kb.ForeColor = Color.FromArgb(138, 43, 226);
                kb.Tag       = (object)false;
            }
        };
    }

    // Called from Designer to register a macro keybind box with its checkbox
    internal void RegisterMacroKeybind(TextBox kb, ShayanCheckBox cb, string name)
    {
        kb.Tag = (object)false;
        kb.Click += (s, e) =>
        {
            kb.Text      = "...";
            kb.ForeColor = Color.White;
            kb.Tag       = (object)true;
            kb.Focus();
        };
        kb.KeyDown += (s, e) =>
        {
            if (!(bool)kb.Tag) return;
            e.SuppressKeyPress = true;
            kb.Tag = (object)false;

            if (e.KeyCode == Keys.Escape)
            {
                var oldKey = _keybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
                if (oldKey != default) _keybinds.Remove(oldKey);
                var oldMouse = _mouseKeybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
                if (oldMouse != default) _mouseKeybinds.Remove(oldMouse);
                kb.Text      = "None";
                kb.ForeColor = Color.FromArgb(138, 43, 226);
                System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                    ShayanNotificationManager.Show("Macro Keybind", $"{name} keybind cleared",
                        NotificationType.Info, 1800)));
                return;
            }

            // Regular keyboard keybind
            if (_keybinds.TryGetValue(e.KeyCode, out var existingKey) && existingKey.cb != cb)
            {
                kb.Text      = "None";
                kb.ForeColor = Color.FromArgb(138, 43, 226);
                System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                    ShayanNotificationManager.Show("Keybind Conflict",
                        $"{e.KeyCode} already used by {existingKey.name}",
                        NotificationType.Info, 2500)));
                return;
            }

            var prevKeyK = _keybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
            if (prevKeyK != default) _keybinds.Remove(prevKeyK);
            var prevMouseK = _mouseKeybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
            if (prevMouseK != default) _mouseKeybinds.Remove(prevMouseK);

            _keybinds[e.KeyCode] = (cb, name);
            kb.Text      = e.KeyCode.ToString();
            kb.ForeColor = Color.FromArgb(180, 255, 180);
            System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                ShayanNotificationManager.Show("Macro Keybind",
                    $"{name} macro bound to: {e.KeyCode}",
                    NotificationType.Success, 1800)));
        };
        // Handle mouse button clicks on the keybind box
        kb.MouseDown += (s, e) =>
        {
            if (!(bool)kb.Tag) return;
            kb.Tag = (object)false;

            int mouseButton = e.Button == MouseButtons.Left ? WM_LBUTTONDOWN :
                              e.Button == MouseButtons.Right ? WM_RBUTTONDOWN : WM_MBUTTONDOWN;

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                if (_mouseKeybinds.TryGetValue(mouseButton, out var existing) && existing.cb != cb)
                {
                    kb.Text      = "None";
                    kb.ForeColor = Color.FromArgb(138, 43, 226);
                    System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                        ShayanNotificationManager.Show("Keybind Conflict",
                            $"{e.Button} already used by {existing.name}",
                            NotificationType.Info, 2500)));
                    return;
                }

                var prevKey = _keybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
                if (prevKey != default) _keybinds.Remove(prevKey);
                var prevMouse = _mouseKeybinds.FirstOrDefault(kv => kv.Value.cb == cb).Key;
                if (prevMouse != default) _mouseKeybinds.Remove(prevMouse);

                _mouseKeybinds[mouseButton] = (cb, name);
                kb.Text      = e.Button.ToString();
                kb.ForeColor = Color.FromArgb(180, 255, 180);
                System.Threading.Tasks.Task.Run(() => this.Invoke(() =>
                    ShayanNotificationManager.Show("Macro Keybind",
                        $"{name} macro bound to: {e.Button}",
                        NotificationType.Success, 1800)));
            }
        };
        kb.LostFocus += (s, e) =>
        {
            if ((bool)kb.Tag)
            {
                kb.Text      = "None";
                kb.ForeColor = Color.FromArgb(138, 43, 226);
                kb.Tag       = (object)false;
            }
        };
    }

    internal bool _suppressNotify = false;

    internal void FireKeyDown(Keys key)
    {
        if (!_keybinds.TryGetValue(key, out var entry)) return;
        if (_keyPressed.ContainsKey(key) && _keyPressed[key]) return;

        _keyPressed[key] = true;
        _suppressNotify = true;
        entry.cb.Checked = true;
        _suppressNotify = false;

        if (!MuteNotifications)
            ShayanNotificationManager.Show(entry.name, "Activated", NotificationType.Info, 1500);
    }

    internal void FireKeyUp(Keys key)
    {
        if (!_keybinds.TryGetValue(key, out var entry)) return;
        if (!_keyPressed.ContainsKey(key) || !_keyPressed[key]) return;

        _keyPressed[key] = false;
        _suppressNotify = true;
        entry.cb.Checked = false;
        _suppressNotify = false;

        if (!MuteNotifications)
            ShayanNotificationManager.Show(entry.name, "Deactivated", NotificationType.Info, 1500);
    }

    internal void FireMouseButtonDown(int mouseButton)
    {
        if (!_mouseKeybinds.TryGetValue(mouseButton, out var entry)) return;
        if (_mouseKeyPressed.ContainsKey(mouseButton) && _mouseKeyPressed[mouseButton]) return;

        _mouseKeyPressed[mouseButton] = true;
        _suppressNotify = true;
        entry.cb.Checked = true;
        _suppressNotify = false;

        if (!MuteNotifications)
            ShayanNotificationManager.Show(entry.name, "Activated", NotificationType.Info, 1500);
    }

    internal void FireMouseButtonUp(int mouseButton)
    {
        if (!_mouseKeybinds.TryGetValue(mouseButton, out var entry)) return;
        if (!_mouseKeyPressed.ContainsKey(mouseButton) || !_mouseKeyPressed[mouseButton]) return;

        _mouseKeyPressed[mouseButton] = false;
        _suppressNotify = true;
        entry.cb.Checked = false;
        _suppressNotify = false;

        if (!MuteNotifications)
            ShayanNotificationManager.Show(entry.name, "Deactivated", NotificationType.Info, 1500);
    }

    internal void FireKeybind(Keys key)
    {
        // Legacy method - kept for compatibility but not used with global hooks
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _dragging  = true;
            _dragStart = e.Location;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragging = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
            Location = new Point(
                Location.X + e.X - _dragStart.X,
                Location.Y + e.Y - _dragStart.Y);
    }
}

