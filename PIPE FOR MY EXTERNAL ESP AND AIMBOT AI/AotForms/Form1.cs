using Client;
using Guna.UI2.WinForms;
using Reborn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AotForms.AimVisible;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AotForms
{
    public partial class Form1 : Form
    {
        IntPtr mainHandle;

        public Form1(IntPtr handle)
        {
            InitializeComponent();
            mainHandle = handle;


            // UI Enhancements
            this.DoubleBuffered = true;
            UIHelper.ApplyGlow(this);
        }

        public static api KeyAuthApp = new api(

           name: "INTERNAL", // App name
            ownerid: "a7DtayK5gr",
            secret: "62f77446fb115fbebec425dda7a8cac162d5b0e5356b6518972ef0100cda79be",
           version: "1.0"
 );
        private async void Form1_Load(object sender, EventArgs e)
        {
            // Auto-login without showing login menu
            this.Hide();
            this.Opacity = 0;
            this.ShowInTaskbar = false;

            KeyAuthApp.init();
            KeyAuthApp.check();
            var processes = Process.GetProcessesByName("HD-Player");

            if (processes.Length != 1)
            {
                // Don't show message box, just exit silently
                Environment.Exit(0);
                return;
            }
            var process = processes[0];
            string mainModulePath = Path.GetDirectoryName(process.MainModule?.FileName);

            if (string.IsNullOrEmpty(mainModulePath))
            {
                Environment.Exit(0);
                return;
            }
            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");
            if (!File.Exists(adbPath))
            {
                Environment.Exit(0);
                return;
            }
            var adb = new Adb(adbPath);
            await adb.Kill();
            if (!await adb.Start())
            {
                Environment.Exit(0);
                return;
            }
            var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            if (moduleAddr == 0)
            {
                Environment.Exit(0);
                return;
            }
            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);

            // Skip login - load main menu directly with threads
            var esp = new ESP();
            await esp.Start();
            new Thread(Data.Work) { IsBackground = true }.Start(); // ESP - kept
            new Thread(AimVisible.Work) { IsBackground = true }.Start(); // Aimbot Visible - kept
            // Disabled to reduce lag - controlled by external exe
            // new Thread(AimbotV2.Work) { IsBackground = true }.Start();
            // new Thread(SilentC.Work) { IsBackground = true }.Start();
            // new Thread(teliv2.Work) { IsBackground = true }.Start();
            // new Thread(TeleKil.Work) { IsBackground = true }.Start();
            // new Thread(UpPlayer.Work) { IsBackground = true }.Start();
            Main ML = new Main(mainHandle);
            ML.Show();
            this.Hide();
            // Disabled welcome notification - no UI shown
            // Notification.Show("WELCOME", Color.Yellow);
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
        private async void guna2Button1_Click(object sender, EventArgs e)
        {
        }
        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {
        }
        private void usernametxt_TextChanged(object sender, EventArgs e)
        {
        }
        private void passwordtxt_TextChanged(object sender, EventArgs e)
        {
        }
        private async void guna2Button7_Click(object sender, EventArgs e)
        {

        }
        private async void guna2Button6_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void statuslbl_Click(object sender, EventArgs e)
        {

        }

        private void passwordTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void shayanButton1_Click(object sender, EventArgs e)
        {
            statuslbl.Text = "Logging";
            KeyAuthApp.login(usernameTextBox.Text, passwordTextBox.Text);
            if (KeyAuthApp.response.success)
            {
                KeyAuthApp.login(usernameTextBox.Text, passwordTextBox.Text);
                if (KeyAuthApp.response.success)
                {
                    var esp = new ESP();
                    await esp.Start();
                    new Thread(Data.Work) { IsBackground = true }.Start(); // ESP - kept
                    new Thread(AimVisible.Work) { IsBackground = true }.Start(); // Aimbot Visible - kept
                    // Disabled to reduce lag - controlled by external exe
                    // new Thread(AimbotV2.Work) { IsBackground = true }.Start();
                    // new Thread(SilentC.Work) { IsBackground = true }.Start();
                    // new Thread(teliv2.Work) { IsBackground = true }.Start();
                    // new Thread(TeleKil.Work) { IsBackground = true }.Start();
                    // new Thread(UpPlayer.Work) { IsBackground = true }.Start();
                    Main ML = new Main(mainHandle);
                    ML.Show();
                    this.Hide();
                    Notification.Show("WELCOME", Color.Yellow);
                }
                else
                {
                    MessageBox.Show("Login failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void shayanButton2_Click(object sender, EventArgs e)
        {
            statuslbl.Text = "Status: Verifying User...";

            await Task.Delay(300);
            var loginTask1 = Task.Run(() => KeyAuthApp.register(usernameTextBox.Text, passwordTextBox.Text, key.Text));
            await loginTask1;
            if (KeyAuthApp.response.success)
            {
                KeyAuthApp.login(usernameTextBox.Text, passwordTextBox.Text);
                if (KeyAuthApp.response.success)
                {
                    var esp = new ESP();
                    await esp.Start();

                    new Thread(Data.Work) { IsBackground = true }.Start(); // ESP - kept
                    new Thread(AimVisible.Work) { IsBackground = true }.Start(); // Aimbot Visible - kept
                    // Disabled to reduce lag - controlled by external exe
                    // new Thread(AimbotV2.Work) { IsBackground = true }.Start();
                    // new Thread(SilentC.Work) { IsBackground = true }.Start();
                    // new Thread(teliv2.Work) { IsBackground = true }.Start();
                    // new Thread(TeleKil.Work) { IsBackground = true }.Start();
                    // new Thread(UpPlayer.Work) { IsBackground = true }.Start();
                    Main ML = new Main(mainHandle);
                    ML.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Login failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
