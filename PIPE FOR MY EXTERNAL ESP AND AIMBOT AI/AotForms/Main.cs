using Broken3ifrit;
using Guna.UI2.WinForms;
using MagıcMemory;
using MAZEX_S_R_MEMM;
using NARZOPAPA;
using ShayanUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AotForms
{
    public partial class Main : Form
    {


        IntPtr mainHandle;

        IntPtr hWnd;
        bool keybind1 = false;
        bool shift;

        bool aimbothks;
        bool esphks;
        bool confighks;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private Keys HideKey;
        private bool waitingForHidehackKey = false;
        private bool waitingForAimbothackKey = false;
        private bool formularioVisible = true;
        private const int WM_HOTKEY = 0x0312;
        private const int MYACTION_HOTKEY_ID = 1;
        private const int Hide_HOTKEY_ID = 2;
        public Main(IntPtr handle)
        {
            mainHandle = handle;
            NARZO NARZO = new NARZO();
            InitializeComponent();
            // StartHotkeyListener(); // Disabled - menu controlled by external exe

            // Hide menu completely - controlled by external exe
            this.Hide();
            this.Opacity = 0;
            this.ShowInTaskbar = false;

            // UI Enhancements
            this.DoubleBuffered = true;
            UIHelper.ApplyGlow(this);
        }

        private void Kh_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey) shift = false;
        }
        private async void Kh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey) shift = true;
            if (e.KeyCode == Keys.F1)
            {
                if (Config.FixEsp)
                {
                    Core.Entities = new();
                    InternalMemory.Cache = new();
                }
                else { }
            }




            if (e.KeyCode == Keys.Insert)
            {
                Config.Notif();
                if (keybind1 == false)
                {
                    this.Hide();

                    keybind1 = true;
                }
                else
                {
                    this.Show();

                    SetStreamMode();

                    void SetStreamMode()
                    {
                        foreach (var obj in Application.OpenForms)
                        {
                            var form = obj as Form;

                            if (Config.StreamMode)
                            {
                                SetWindowDisplayAffinity(form.Handle, WDA_NONE);
                                SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);

                            }
                            else
                            {



                            }
                        }
                    }

                    keybind1 = false;
                }
            }
        }











        private void enableaimcheckbox_Click(object sender, EventArgs e)
        {
            Config.enableAimBot = enableaimcheckbox.Checked;
            status.Text = "Enabled";
            Notification.Show("FUNCTION: ON", Color.Yellow);
        }

        private void upplayercheckbox_Click(object sender, EventArgs e)
        {
        }

        private void telekillcheckbox_Click(object sender, EventArgs e)
        {
        }

        private void norecoilcheckbox_Click(object sender, EventArgs e)
        {
            Config.NoRecoil = norecoilcheckbox.Checked;
            status.Text = "NoRecoil Enabled";
            Notification.Show("SPINBOT: ON", Color.Yellow);
        }

        private void ignoreknockedcheckbox_Click(object sender, EventArgs e)
        {
            Config.IgnoreKnocked = ignoreknockedcheckbox.Checked;
            status.Text = "IgnoreKnocked Enabled";
            Notification.Show("IGNOREKNOCK: ON", Color.Yellow);
        }

        private void showcirclefovcheckbox_Click(object sender, EventArgs e)
        {
            Config.FOVEnabled = showcirclefovcheckbox.Checked;
            Notification.Show("SHOWFAV: ON", Color.Yellow);
        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            var distance = guna2TrackBar1.Value;

            label7.Text = $"AimFov: {distance}";

            Config.AimFov = distance;
        }
        static IntPtr FindRenderWindow(IntPtr parent)
        {
            IntPtr renderWindow = IntPtr.Zero;
            WinAPI.EnumChildWindows(parent, (hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                WinAPI.GetWindowText(hWnd, sb, sb.Capacity);
                string windowName = sb.ToString();
                if (!string.IsNullOrEmpty(windowName))
                {
                    if (windowName != "HD-Player")
                    {
                        renderWindow = hWnd;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return renderWindow;
        }
        private async void enablehookbtn_Click(object sender, EventArgs e)
        {

            var processes = Process.GetProcessesByName("HD-Player");

            if (processes.Length != 1)
            {
                MessageBox.Show("Open emulator.");
                return;
            }

            var process = processes[0];
            string mainModulePath = Path.GetDirectoryName(process.MainModule?.FileName);

            if (string.IsNullOrEmpty(mainModulePath))
            {
                MessageBox.Show("Reinstall emulator.");
                return;
            }

            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");
            if (!File.Exists(adbPath))
            {
                MessageBox.Show("Adb not Found. Reinstall emulator.");
                return;
            }



            var adb = new Adb(adbPath);
            await adb.Kill();
            //await Task.Delay(500);
            if (!await adb.Start())
            {
                MessageBox.Show("Adb Error");
                Environment.Exit(0);
                return;
            }
            //await Task.Delay(500);
            var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            if (moduleAddr == 0)
            {
                MessageBox.Show("Go to Lobby then Logout then try to apply.");
                Environment.Exit(0);
                return;
            }

            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);
        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = (string)comboBox.SelectedItem;

            switch (selectedItem)
            {
                case "Rage":
                    Config.AimBotRage = true;
                    Config.SILENT = false;
                    Config.AimbotVisible = false;
                    Config.SilentAim = false;
                    break;

                case "SilentLite":
                    Config.AimBotRage = false;
                    Config.SILENT = false;
                    Config.AimbotVisible = false;
                    Config.SilentAim = true;
                    break;

                case "Silent360":
                    Config.AimBotRage = false;
                    Config.SILENT = true;
                    Config.AimbotVisible = false;
                    Config.SilentAim = false;

                    break;
                case "Visible":
                    Config.AimBotRage = false;
                    Config.SILENT = false;
                    Config.AimbotVisible = true;
                    Config.SilentAim = false;
                    break;
            }
        }

        private void esplinecheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPLine = esplinecheckbox.Checked;
            Notification.Show("ESP LINE: ON", Color.Yellow);
        }

        private void espboxcheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPBox = espboxcheckbox.Checked;
        }

        private void espcornercheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPBox2 = espcornercheckbox.Checked;
        }

        private void esphealthcheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPHealth = esphealthcheckbox.Checked;
        }
        private bool isAutoRefreshEnabled = false;
        private CancellationTokenSource cancellationTokenSource;
        private void guna2CustomCheckBox1_Click(object sender, EventArgs e)
        {
            isAutoRefreshEnabled = !isAutoRefreshEnabled;

            if (isAutoRefreshEnabled)
            {
                StartAutoRefresh();
            }
            else
            {
                StopAutoRefresh();
            }
        }
        private void StartAutoRefresh()
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Core.Entities = new();
                    InternalMemory.Cache = new();
                    Thread.Sleep(4000); // Prevent high CPU usage
                }
            }, token);
        }

        private void StopAutoRefresh()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
        private void espnamecheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPName = espnamecheckbox.Checked;
        }

        private void guna2ComboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = (string)comboBox.SelectedItem;

            switch (selectedItem)
            {
                case "ClosestToCrosshair":
                    Config.TargetingMode = TargetingMode.ClosestToCrosshair;
                    break;

                case "Target360":
                    Config.TargetingMode1 = TargetingMode.Target360;
                    break;

                case "ClosestToPlayer":
                    Config.TargetingMode2 = TargetingMode.ClosestToPlayer;
                    break;

                case "LowestHealth":
                    Config.TargetingMode3 = TargetingMode.LowestHealth;
                    break;
            }
        }

        private void espdistancecheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPDistance = espdistancecheckbox.Checked;
        }

        private void espfillboxcheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPFillBox = espfillboxcheckbox.Checked;
        }
        private void NoCache()
        {
            InternalMemory.Cache = new();
            Core.Entities = new();
            Thread.Sleep(1000);
        }
        private void UpdateEntities()
        {
            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsTeam != Bool3.False) continue;

                TreeNode entityNode = new TreeNode(entity.Name);

                entityNode.Nodes.Add(new TreeNode($"IsKnown: {entity.IsKnown}"));
                entityNode.Nodes.Add(new TreeNode($"IsTeam: {entity.IsTeam}"));
                entityNode.Nodes.Add(new TreeNode($"Head: {entity.Head}"));
                entityNode.Nodes.Add(new TreeNode($"Root: {entity.Root}"));
                entityNode.Nodes.Add(new TreeNode($"Health: {entity.Health}"));
                entityNode.Nodes.Add(new TreeNode($"IsDead: {entity.IsDead}"));
                entityNode.Nodes.Add(new TreeNode($"IsKnocked: {entity.IsKnocked}"));
            }
            Thread.Sleep(1000);
        }
        private void updateentitybtn_Click(object sender, EventArgs e)
        {
            UpdateEntities();
        }
        bool resetCacheChecked = false;
        System.Timers.Timer timer = new System.Timers.Timer(10000); // 15 seconds
        void SetupTimer()
        {
            timer.Elapsed += (sender, e) =>
            {
                resetCacheChecked = true;
            };
            timer.AutoReset = true;
            timer.Start();
        }
        private void resetcachebtn_Click(object sender, EventArgs e)
        {
            NoCache();
        }
        static void ResetCache()
        {
            Core.Entities = new();
            InternalMemory.Cache = new();
        }

        private void guna2CustomCheckBox3_Click(object sender, EventArgs e)
        {
            if (resetCacheChecked)
            {
                Core.HaveMatrix = true;
                SetupTimer();
                ResetCache();
                resetCacheChecked = false;
            }
        }


        private void anticheatbtn_Click(object sender, EventArgs e)
        {

        }


        private void linecolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                linecolorpicker.FillColor = picker.Color;
                Config.ESPLineColor = picker.Color;
            }
        }

        private void boxcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                boxcolorpicker.FillColor = picker.Color;
                Config.ESPBoxColor = picker.Color;
            }
        }

        private void cornerboxcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                cornerboxcolorpicker.FillColor = picker.Color;
                Config.ESPBoxColor = picker.Color;
            }
        }

        private void fillboxcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                fillboxcolorpicker.FillColor = picker.Color;
                Config.ESPFillBoxColor = picker.Color;
            }
        }

        private void namecolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                namecolorpicker.FillColor = picker.Color;
                Config.ESPNameColor = picker.Color;
            }
        }

        private void distancecolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                distancecolorpicker.FillColor = picker.Color;
                Config.ESPDistanceColor = picker.Color;
            }
        }

        private void guna2CustomCheckBox2_Click(object sender, EventArgs e)
        {
            Config.ESPSkeleton = guna2CustomCheckBox2.Checked;
        }

        private void healthcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                healthcolorpicker.FillColor = picker.Color;
                Config.ESPHealthColor = picker.Color;
            }
        }

        private void skeletoncolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                skeletoncolorpicker.FillColor = picker.Color;
                Config.ESPSkeletonColor = picker.Color;
            }
        }

        private void targetaimcolorpicker_Click(object sender, EventArgs e)
        {

        }
        private List<long> a = new List<long>();
        private Dictionary<long, string> m = new Dictionary<long, string>();



        private async void guna2CustomCheckBox4_Click(object sender, EventArgs e)
        {
        }

        private void guna2CustomCheckBox5_Click(object sender, EventArgs e)
        {
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void Main_Load(object sender, EventArgs e)
        {

            //var processes = Process.GetProcessesByName("HD-Player");

            //if (processes.Length != 1)
            //{
            //    MessageBox.Show("Open emulator.");
            //    return;
            //}

            //var process = processes[0];
            //string mainModulePath = Path.GetDirectoryName(process.MainModule?.FileName);

            //if (string.IsNullOrEmpty(mainModulePath))
            //{
            //    MessageBox.Show("Reinstall emulator.");
            //    return;
            //}

            //var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");
            //if (!File.Exists(adbPath))
            //{
            //    MessageBox.Show("Adb not Found. Reinstall emulator.");
            //    return;
            //}



            //var adb = new Adb(adbPath);
            //await adb.Kill();
            ////await Task.Delay(500);
            //if (!await adb.Start())
            //{
            //    MessageBox.Show("Adb Error");
            //    Environment.Exit(0);
            //    return;
            //}
            ////await Task.Delay(500);
            //var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            //if (moduleAddr == 0)
            //{
            //    MessageBox.Show("Go to Lobby then Logout then try to apply.");
            //    Environment.Exit(0);
            //    return;
            //}

            //Offsets.Il2Cpp = moduleAddr;
            //Core.Handle = FindRenderWindow(mainHandle);

            //new Thread(Data.Work) { IsBackground = true }.Start();
            //new Thread(AimbotV2.Work) { IsBackground = true }.Start();
            //new Thread(UpPlayer.Work) { IsBackground = true }.Start();
            //new Thread(TeleKil.Work) { IsBackground = true }.Start();
        }

        private void guna2TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {


        }

        private async void guna2CustomCheckBox6_Click(object sender, EventArgs e)
        {


        }

        private async void guna2CustomCheckBox9_Click(object sender, EventArgs e)
        {
        }

        private async void guna2CustomCheckBox8_Click(object sender, EventArgs e)
        {


        }

        private async void guna2Button1_Click_1(object sender, EventArgs e)
        {
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox6_Click_1(object sender, EventArgs e)
        {
        }

        private async void guna2CustomCheckBox8_Click_1(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox7_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox9_Click_1(object sender, EventArgs e)
        {
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void guna2TrackBar2_Scroll(object sender, ScrollEventArgs e)
        {
            var distance = guna2TrackBar2.Value;

            label5.Text = $"Smoothness: {distance}";

            Config.AimBotSmooth = distance;
        }

        private void guna2CustomCheckBox4_Click_1(object sender, EventArgs e)
        {

        }
        [DllImport("user32.dll")]
        static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        const uint WDA_NONE = 0x00000000;
        const uint WDA_MONITOR = 0x00000001;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        private void guna2CustomCheckBox5_Click_1(object sender, EventArgs e)
        {
            Config.sound = guna2CustomCheckBox2.Checked;
        }

        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {
            Config.Notif();
            try
            {
                var processes = Process.GetProcessesByName("HD-Player");

                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }


            }
            catch (Exception ex)
            {

            }
        }
        // Run this once when the program starts
        private void StartHotkeyListener()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    // We use & 1 to detect a single "press" rather than "holding down"
                    if (Config.EnemyPullKey != 0 && (WinAPI.GetAsyncKeyState(Config.EnemyPullKey) & 1) != 0)
                    {
                        Config.teliv2 = !Config.teliv2;

                        // Sync the checkbox on the UI so you can see the state
                        this.Invoke(new MethodInvoker(delegate
                        {
                            guna2CustomCheckBox5.Checked = Config.teliv2;
                        }));

                        // Start or Stop the logic based on the new state
                        if (Config.teliv2)
                            AotForms.teliv2.Work();
                        else
                            AotForms.teliv2.Stop();
                    }
                    Thread.Sleep(10);
                }
            });
        }

        private void guna2CustomCheckBox4_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox5_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox6_Click_2(object sender, EventArgs e)
        {
            Config.teli = guna2CustomCheckBox6.Checked;
        }

        private void guna2CustomCheckBox7_Click_1(object sender, EventArgs e)
        {
            Config.UpPlayer = guna2CustomCheckBox7.Checked;
        }

        private void guna2CustomCheckBox8_Click_2(object sender, EventArgs e)
        {

        }

        private void shayanComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void shayanCheckBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void shayanCheckBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void shayanComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2TrackBar3_Scroll(object sender, ScrollEventArgs e)
        {

        }


        private void shayanButton1_Click(object sender, EventArgs e)
        {
            shayanButton1.Text = "...";

            Task.Run(() =>
            {
                // Give the user a tiny moment to release the mouse click
                Thread.Sleep(200);

                bool found = false;
                while (!found)
                {
                    for (int i = 0x03; i <= 0xFE; i++) // Start at 0x03 to skip Mouse Left/Right
                    {
                        if ((WinAPI.GetAsyncKeyState(i) & 0x8000) != 0)
                        {
                            Config.EnemyPullKey = i;
                            string keyName = ((System.Windows.Forms.Keys)i).ToString();

                            this.Invoke(new MethodInvoker(delegate
                            {
                                shayanButton1.Text = "Key: " + keyName;
                            }));

                            found = true;
                            break;
                        }
                    }
                    Thread.Sleep(10);
                }
            });
        }

        private void shayanCheckBox1_CheckedChanged_2(object sender, EventArgs e)
        {
        }

        private void guna2TrackBar3_Scroll_1(object sender, ScrollEventArgs e)
        {
            // This updates the speed in real-time as you slide it
            Config.SpinSpeed = (float)spinSpeedTrackBar.Value;

            // Optional: Update a label to show the current speed
            status.Text = "Speed: " + spinSpeedTrackBar.Value.ToString();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void status_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox5_Click_3(object sender, EventArgs e)
        {
            Config.teliv2 = guna2CustomCheckBox5.Checked;

            if (Config.teliv2)
            {
                teliv2.Work(); // Starts fresh
                Notification.Show("ENEMY PULL: ON", Color.Yellow);
            }
            else
            {
                teliv2.Stop(); // Ends cleanly
                Notification.Show("ENEMY PULL: OFF", Color.Yellow);
            }
        }

        private void guna2CustomCheckBox8_Click_3(object sender, EventArgs e)
        {

            Config.spinbot = guna2CustomCheckBox8.Checked;

            if (Config.spinbot)
            {
                RapidSpin.Activate();
                Notification.Show("SPINBOT: ON", Color.Yellow);
            }
            else
            {
                RapidSpin.Deactivate();
                Notification.Show("SPINBOT: OFF", Color.Yellow);
            }
        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox10_Click(object sender, EventArgs e)
        {
            // 'this' refers to your Main Form
            this.TopMost = guna2CustomCheckBox10.Checked;

            if (this.TopMost)
                Notification.Show("PINNED TO TOP", Color.Yellow);
            else
                Notification.Show("UNPINNED", Color.Gray);
        }
    }
}
