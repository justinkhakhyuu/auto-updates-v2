using System.Security.Cryptography;
using System.Text;
using KeyAuth;

namespace BlackCorps.App;

public partial class LoginForm : Form
{
    // ── Credentials file path ──────────────────────────────────
    private static readonly string CredFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BlackCorps", "remember.dat");

    // ── KeyAuth instance — fill in your credentials ────────────
    public static readonly api KeyAuthApp = new api(
        name:    "INTERNAL",
        ownerid: "a7DtayK5gr",
        version: "1.0"
    );

    public LoginForm()
    {
        InitializeComponent();
        ApplyGlowTitle();
        this.Load += async (s, e) =>
        {
            FixTextBoxInnerColor();
            await InitKeyAuth();
            LoadRemembered();
        };
    }

    private async Task InitKeyAuth()
    {
        try
        {
            loginButton.Enabled = false;
            loginButton.Text    = "Connecting...";
            await KeyAuthApp.init();
            loginButton.Enabled = true;
            loginButton.Text    = "LOGIN";
        }
        catch
        {
            if (!MainMenu.MuteNotifications)
                ShayanUI.ShayanNotificationManager.Show("Connection", "Failed to reach auth server.", ShayanUI.NotificationType.Info, 4000);
            loginButton.Text = "LOGIN";
            loginButton.Enabled = true;
        }
    }

    private void FixTextBoxInnerColor()
    {
        var bg = Color.FromArgb(10, 10, 30);
        foreach (var stb in new[] { usernameTextBox, passwordTextBox })
        {
            stb.BackColor = bg;
            foreach (Control child in stb.Controls)
            {
                child.BackColor = bg;
                child.ForeColor = Color.White;
            }
        }
    }

    private void ApplyGlowTitle()
    {
        titleLabel.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using var font   = new Font("Segoe UI", 28F, FontStyle.Bold);
            var sf   = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var rect = new RectangleF(0, 0, titleLabel.Width, titleLabel.Height);
            using var shadow = new SolidBrush(Color.FromArgb(80, 138, 43, 226));
            e.Graphics.DrawString("BLACK CORPS", font, shadow, new RectangleF(0, 2, titleLabel.Width, titleLabel.Height), sf);
            using var main = new SolidBrush(Color.White);
            e.Graphics.DrawString("BLACK CORPS", font, main, rect, sf);
        };
    }

    // ── Remember Me ────────────────────────────────────────────
    private void LoadRemembered()
    {
        if (!File.Exists(CredFile)) return;
        try
        {
            var lines = File.ReadAllLines(CredFile);
            if (lines.Length < 2) return;
            usernameTextBox.Text = Unprotect(lines[0]);
            passwordTextBox.Text = Unprotect(lines[1]);
            rememberMeCheckBox.Checked = true;
        }
        catch { /* corrupt file — ignore */ }
    }

    private void SaveRemembered(string username, string password)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CredFile)!);
            File.WriteAllLines(CredFile, new[] { Protect(username), Protect(password) });
        }
        catch { }
    }

    private void ClearRemembered()
    {
        try { if (File.Exists(CredFile)) File.Delete(CredFile); }
        catch { }
    }

    // DPAPI — encrypted per-user, per-machine
    private static string Protect(string s)   => Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(s), null, DataProtectionScope.CurrentUser));
    private static string Unprotect(string s) => Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(s), null, DataProtectionScope.CurrentUser));

    // ── Login ──────────────────────────────────────────────────
    private async void LoginButton_Click(object sender, EventArgs e)
    {
        string username = usernameTextBox.Text.Trim();
        string password = passwordTextBox.Text;

        if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password))
        {
            if (!MainMenu.MuteNotifications)
                ShayanUI.ShayanNotificationManager.Show("Login Error", "Please enter your username and password.", ShayanUI.NotificationType.Info, 3000);
            return;
        }
        if (string.IsNullOrWhiteSpace(username))
        {
            if (!MainMenu.MuteNotifications)
                ShayanUI.ShayanNotificationManager.Show("Login Error", "Username cannot be empty.", ShayanUI.NotificationType.Info, 3000);
            return;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            if (!MainMenu.MuteNotifications)
                ShayanUI.ShayanNotificationManager.Show("Login Error", "Password cannot be empty.", ShayanUI.NotificationType.Info, 3000);
            return;
        }

        loginButton.Enabled = false;
        loginButton.Text    = "Logging in...";

        try
        {
            await KeyAuthApp.login(username, password);
        }
        catch
        {
            if (!MainMenu.MuteNotifications)
                ShayanUI.ShayanNotificationManager.Show("Error", "Connection failed. Try again.", ShayanUI.NotificationType.Info, 3000);
            loginButton.Enabled = true;
            loginButton.Text    = "LOGIN";
            return;
        }

        loginButton.Enabled = true;
        loginButton.Text    = "LOGIN";

        if (!KeyAuthApp.response.success)
        {
            string msg = KeyAuthApp.response.message switch
            {
                "incorrect_password" or "incorrect password" => "Incorrect password.",
                "user_not_found"     or "user not found"     => "Username not found.",
                "banned"                                      => "Your account has been banned.",
                "expired"                                     => "Your subscription has expired.",
                _                                             => KeyAuthApp.response.message ?? "Login failed."
            };
            if (!MainMenu.MuteNotifications)
                ShayanUI.ShayanNotificationManager.Show("Login Failed", msg, ShayanUI.NotificationType.Info, 3500);
            return;
        }

        // Success
        if (rememberMeCheckBox.Checked)
            SaveRemembered(username, password);
        else
            ClearRemembered();

        if (!MainMenu.MuteNotifications)
            ShayanUI.ShayanNotificationManager.Show("Welcome", $"Welcome back, {username}!", ShayanUI.NotificationType.Info, 1800);

        await Task.Delay(400);
        this.Hide();
        var loader = new LoaderForm(() =>
        {
            var menu = new MainMenu();
            menu.Show();
        });
        loader.Show();
    }
}
