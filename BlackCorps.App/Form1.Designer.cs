using ShayanUI;

namespace BlackCorps.App;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;

    private ShayanBorderless shayanBorderless;
    private ShayanHeader     shayanHeader;
    private ShayanTextBox    usernameTextBox;
    private ShayanTextBox    passwordTextBox;
    private ShayanButton     loginButton;
    private ShayanCheckBox   rememberMeCheckBox;
    private Label            titleLabel;
    private Label            subtitleLabel;
    private Label            usernameLabel;
    private Label            passwordLabel;
    private Label            noAccountLabel;
    private LinkLabel        forgotPasswordLink;
    private LinkLabel        signUpLink;
    private Panel            rainPanel;
    private System.Windows.Forms.Timer rainTimer;
    private Random           rnd = new();
    private List<RainDrop>   rainDrops = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        rainTimer?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.shayanBorderless    = new ShayanBorderless();
        this.shayanHeader        = new ShayanHeader();
        this.titleLabel          = new Label();
        this.subtitleLabel       = new Label();
        this.usernameLabel       = new Label();
        this.passwordLabel       = new Label();
        this.usernameTextBox     = new ShayanTextBox();
        this.passwordTextBox     = new ShayanTextBox();
        this.loginButton         = new ShayanButton();
        this.rememberMeCheckBox  = new ShayanCheckBox();
        this.forgotPasswordLink  = new LinkLabel();
        this.noAccountLabel      = new Label();
        this.signUpLink          = new LinkLabel();
        this.rainPanel           = new Panel();
        this.rainTimer           = new System.Windows.Forms.Timer();

        this.SuspendLayout();

        // ShayanBorderless - handles drag, rounding, blur natively
        this.shayanBorderless.TargetForm           = this;
        this.shayanBorderless.BorderRadius         = 18;
        this.shayanBorderless.EnableBlur           = true;
        this.shayanBorderless.DisableTransparency  = false;
        this.shayanBorderless.Transparency         = 0.97;
        this.shayanBorderless.BlurColor            = Color.FromArgb(10, 10, 30);
        this.shayanBorderless.BlurOpacity          = 210;

        // Header
        this.shayanHeader.Dock  = DockStyle.Top;
        this.shayanHeader.Size  = new Size(500, 40);
        this.shayanHeader.Title = "BLACK CORPS";

        // Rain panel (background, full fill behind everything)
        this.rainPanel.BackColor = Color.FromArgb(10, 10, 30);
        this.rainPanel.Dock      = DockStyle.Fill;
        this.rainPanel.Paint    += RainPanel_Paint;

        // Title
        this.titleLabel.AutoSize  = false;
        this.titleLabel.Font      = new Font("Segoe UI", 28F, FontStyle.Bold);
        this.titleLabel.ForeColor = Color.Transparent;
        this.titleLabel.BackColor = Color.Transparent;
        this.titleLabel.Location  = new Point(30, 48);
        this.titleLabel.Size      = new Size(440, 50);
        this.titleLabel.Text      = "";
        this.titleLabel.TextAlign = ContentAlignment.MiddleCenter;

        // Subtitle
        this.subtitleLabel.AutoSize  = false;
        this.subtitleLabel.Font      = new Font("Segoe UI", 9F);
        this.subtitleLabel.ForeColor = Color.FromArgb(160, 160, 190);
        this.subtitleLabel.BackColor = Color.Transparent;
        this.subtitleLabel.Location  = new Point(30, 100);
        this.subtitleLabel.Size      = new Size(440, 20);
        this.subtitleLabel.Text      = "Sign in to your account";
        this.subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;

        // Divider under subtitle
        var divider = new Panel
        {
            BackColor = Color.FromArgb(35, 138, 43, 226),
            Location  = new Point(30, 125),
            Size      = new Size(440, 1)
        };

        // Username label
        this.usernameLabel.AutoSize  = true;
        this.usernameLabel.Font      = new Font("Segoe UI", 8F, FontStyle.Bold);
        this.usernameLabel.ForeColor = Color.FromArgb(138, 43, 226);
        this.usernameLabel.BackColor = Color.Transparent;
        this.usernameLabel.Location  = new Point(30, 142);
        this.usernameLabel.Text      = "USERNAME";

        // Username textbox
        this.usernameTextBox.Location         = new Point(30, 162);
        this.usernameTextBox.Size             = new Size(440, 38);
        this.usernameTextBox.TabIndex         = 0;
        this.usernameTextBox.PlaceholderText  = "Enter your username";
        this.usernameTextBox.BorderRadius     = 10;
        this.usernameTextBox.BorderColor      = Color.FromArgb(60, 138, 43, 226);
        this.usernameTextBox.BackColor        = Color.FromArgb(10, 10, 30);
        this.usernameTextBox.ForeColor        = Color.White;

        // Password label
        this.passwordLabel.AutoSize  = true;
        this.passwordLabel.Font      = new Font("Segoe UI", 8F, FontStyle.Bold);
        this.passwordLabel.ForeColor = Color.FromArgb(138, 43, 226);
        this.passwordLabel.BackColor = Color.Transparent;
        this.passwordLabel.Location  = new Point(30, 215);
        this.passwordLabel.Text      = "PASSWORD";

        // Password textbox
        this.passwordTextBox.Location              = new Point(30, 235);
        this.passwordTextBox.Size                  = new Size(440, 38);
        this.passwordTextBox.TabIndex              = 1;
        this.passwordTextBox.PlaceholderText       = "Enter your password";
        this.passwordTextBox.UseSystemPasswordChar = true;
        this.passwordTextBox.BorderRadius          = 10;
        this.passwordTextBox.BorderColor           = Color.FromArgb(60, 138, 43, 226);
        this.passwordTextBox.BackColor             = Color.FromArgb(10, 10, 30);
        this.passwordTextBox.ForeColor             = Color.White;

        // Remember me
        this.rememberMeCheckBox.AutoSize     = false;
        this.rememberMeCheckBox.Font         = new Font("Segoe UI", 9F);
        this.rememberMeCheckBox.ForeColor    = Color.FromArgb(160, 160, 190);
        this.rememberMeCheckBox.BackColor    = Color.Transparent;
        this.rememberMeCheckBox.CheckedColor = Color.FromArgb(138, 43, 226);
        this.rememberMeCheckBox.Location     = new Point(30, 288);
        this.rememberMeCheckBox.Size         = new Size(130, 26);
        this.rememberMeCheckBox.Text         = "Remember me";

        // Forgot password
        this.forgotPasswordLink.AutoSize  = true;
        this.forgotPasswordLink.Font      = new Font("Segoe UI", 9F);
        this.forgotPasswordLink.ForeColor = Color.FromArgb(138, 43, 226);
        this.forgotPasswordLink.LinkColor = Color.FromArgb(138, 43, 226);
        this.forgotPasswordLink.Location  = new Point(338, 292);
        this.forgotPasswordLink.TabIndex  = 2;
        this.forgotPasswordLink.Text      = "Forgot password?";

        // Login button
        this.loginButton.Location     = new Point(30, 332);
        this.loginButton.Size         = new Size(440, 46);
        this.loginButton.TabIndex     = 3;
        this.loginButton.Text         = "LOGIN";
        this.loginButton.BorderRadius = 12;
        this.loginButton.BorderColor  = Color.FromArgb(138, 43, 226);
        this.loginButton.GlassColor   = Color.FromArgb(138, 43, 226);
        this.loginButton.ForeColor    = Color.White;
        this.loginButton.Font         = new Font("Segoe UI", 11F, FontStyle.Bold);
        this.loginButton.Click       += new EventHandler(LoginButton_Click);

        // No account label
        this.noAccountLabel.AutoSize  = true;
        this.noAccountLabel.Font      = new Font("Segoe UI", 9F);
        this.noAccountLabel.ForeColor = Color.FromArgb(130, 130, 160);
        this.noAccountLabel.BackColor = Color.Transparent;
        this.noAccountLabel.Location  = new Point(130, 394);
        this.noAccountLabel.Text      = "Don't have an account?";

        // Sign up link
        this.signUpLink.AutoSize  = true;
        this.signUpLink.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
        this.signUpLink.ForeColor = Color.FromArgb(138, 43, 226);
        this.signUpLink.LinkColor = Color.FromArgb(138, 43, 226);
        this.signUpLink.Location  = new Point(300, 394);
        this.signUpLink.TabIndex  = 4;
        this.signUpLink.Text      = "Sign up";

        // Form
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode       = AutoScaleMode.Font;
        this.BackColor           = Color.FromArgb(10, 10, 30);
        this.ClientSize          = new Size(500, 430);
        this.FormBorderStyle     = FormBorderStyle.None;
        this.StartPosition       = FormStartPosition.CenterScreen;
        this.Text                = "Login - Black Corps";

        // Add to rainPanel (rendered on top of rain)
        this.rainPanel.Controls.Add(this.titleLabel);
        this.rainPanel.Controls.Add(this.subtitleLabel);
        this.rainPanel.Controls.Add(divider);
        this.rainPanel.Controls.Add(this.usernameLabel);
        this.rainPanel.Controls.Add(this.usernameTextBox);
        this.rainPanel.Controls.Add(this.passwordLabel);
        this.rainPanel.Controls.Add(this.passwordTextBox);
        this.rainPanel.Controls.Add(this.rememberMeCheckBox);
        this.rainPanel.Controls.Add(this.forgotPasswordLink);
        this.rainPanel.Controls.Add(this.loginButton);
        this.rainPanel.Controls.Add(this.noAccountLabel);
        this.rainPanel.Controls.Add(this.signUpLink);

        this.Controls.Add(this.rainPanel);
        this.Controls.Add(this.shayanHeader);

        this.ResumeLayout(false);
        this.PerformLayout();

        InitRain();
    }

    private void InitRain()
    {
        for (int i = 0; i < 55; i++)
            rainDrops.Add(NewDrop(true));

        rainTimer.Interval = 20;
        rainTimer.Tick += (s, e) =>
        {
            for (int i = 0; i < rainDrops.Count; i++)
            {
                rainDrops[i].Y += rainDrops[i].Speed;
                if (rainDrops[i].Y > 450) rainDrops[i] = NewDrop(false);
            }
            rainPanel.Invalidate();
        };
        rainTimer.Start();
    }

    private RainDrop NewDrop(bool randomY)
    {
        return new RainDrop
        {
            X     = rnd.Next(0, 500),
            Y     = randomY ? rnd.Next(-30, 450) : -rnd.Next(5, 30),
            Len   = rnd.Next(8, 24),
            Speed = (float)(rnd.NextDouble() * 2.5 + 1.0),
            Alpha = rnd.Next(18, 75)
        };
    }

    private void RainPanel_Paint(object sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        foreach (var d in rainDrops)
        {
            using var pen = new Pen(Color.FromArgb(d.Alpha, 138, 43, 226), 1.1f);
            e.Graphics.DrawLine(pen, d.X, d.Y, d.X, d.Y + d.Len);
        }
    }
}
