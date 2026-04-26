using ShayanUI;
using System.Drawing.Drawing2D;

namespace BlackCorps.App;

public class LoaderForm : Form
{
    private ShayanBorderless shayanBorderless;
    private Label            statusLabel;
    private Label            pctLabel;
    private System.Windows.Forms.Timer rainTimer;
    private System.Windows.Forms.Timer sequenceTimer;
    private System.Windows.Forms.Timer fillTimer;
    private List<RainDrop>   rain = new();
    private Random           rnd  = new();

    private int    _step       = 0;
    private float  _progress   = 0f;
    private float  _targetProg = 0f;
    private Action _onComplete;

    private static readonly (string msg, float prog)[] Steps =
    {
        ("Initializing BLACK CORPS...",          8f),
        ("Checking for updates...",             22f),
        ("Verifying license key...",            38f),
        ("Scanning for crack processes...",     52f),
        ("Checking anti-cheat environment...",  66f),
        ("Loading configuration...",            80f),
        ("Preparing injection modules...",      93f),
        ("All checks passed. Launching...",    100f),
    };

    public LoaderForm(Action onComplete)
    {
        _onComplete = onComplete;
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.UserPaint |
                      ControlStyles.DoubleBuffer, true);
        BuildUI();
        StartRain();
        StartSequence();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.None;

        // Rain
        foreach (var d in rain)
        {
            using var pen = new Pen(Color.FromArgb(d.Alpha, 100, 30, 180), 1f);
            g.DrawLine(pen, d.X, d.Y, d.X, d.Y + d.Len);
        }

        // Circle ring
        int cx = this.Width / 2, cy = 160;
        int radius = 80, thick = 9;
        var rect = new Rectangle(cx - radius, cy - radius, radius * 2, radius * 2);

        // Track
        using var trackPen = new Pen(Color.FromArgb(25, 20, 58), (float)thick);
        g.DrawEllipse(trackPen, rect);

        if (_progress > 0f)
        {
            float sweep = (_progress / 100f) * 360f;

            // Glow
            using var glowPen = new Pen(Color.FromArgb(30, 138, 43, 226), thick + 10f);
            glowPen.StartCap = LineCap.Round;
            glowPen.EndCap   = LineCap.Round;
            g.DrawArc(glowPen, rect, -90f, sweep);

            // Arc
            using var arcPen = new Pen(Color.FromArgb(138, 43, 226), (float)thick);
            arcPen.StartCap = LineCap.Round;
            arcPen.EndCap   = LineCap.Round;
            g.DrawArc(arcPen, rect, -90f, sweep);

            // Tip dot
            double endRad = ((-90f + sweep) * Math.PI) / 180.0;
            float  tipX   = cx + radius * (float)Math.Cos(endRad);
            float  tipY   = cy + radius * (float)Math.Sin(endRad);
            using var tipB = new SolidBrush(Color.FromArgb(230, 190, 255));
            g.FillEllipse(tipB, tipX - 5f, tipY - 5f, 10f, 10f);
        }
    }

    private void BuildUI()
    {
        this.shayanBorderless = new ShayanBorderless();
        this.FormBorderStyle  = FormBorderStyle.None;
        this.StartPosition    = FormStartPosition.CenterScreen;
        this.ClientSize       = new Size(420, 400);
        this.BackColor        = Color.FromArgb(8, 6, 22);
        this.TopMost          = true;

        shayanBorderless.TargetForm          = this;
        shayanBorderless.BorderRadius        = 20;
        shayanBorderless.EnableBlur          = true;
        shayanBorderless.DisableTransparency = false;
        shayanBorderless.Transparency        = 0.97;
        shayanBorderless.BlurColor           = Color.FromArgb(8, 6, 22);
        shayanBorderless.BlurOpacity         = 245;

        // Percent label (inside ring)
        pctLabel = new Label
        {
            AutoSize  = false,
            Text      = "0%",
            Font      = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Location  = new Point(this.Width / 2 - 50, 120),
            Size      = new Size(100, 80),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Title
        var titleLabel = new Label
        {
            AutoSize  = false,
            Text      = "BLACK CORPS",
            Font      = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Location  = new Point(0, 262),
            Size      = new Size(420, 36),
            TextAlign = ContentAlignment.MiddleCenter
        };
        titleLabel.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode     = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using var f    = new Font("Segoe UI", 17F, FontStyle.Bold);
            var sf  = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var rc  = new RectangleF(0, 0, titleLabel.Width, titleLabel.Height);
            using var glow  = new SolidBrush(Color.FromArgb(50, 138, 43, 226));
            e.Graphics.DrawString("BLACK CORPS", f, glow, new RectangleF(0, 2, titleLabel.Width, titleLabel.Height), sf);
            using var white = new SolidBrush(Color.White);
            e.Graphics.DrawString("BLACK CORPS", f, white, rc, sf);
            titleLabel.Text = "";
        };

        var div = new Panel
        {
            BackColor = Color.FromArgb(35, 138, 43, 226),
            Location  = new Point(60, 304),
            Size      = new Size(300, 1)
        };

        statusLabel = new Label
        {
            AutoSize  = false,
            Text      = "",
            Font      = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(130, 130, 175),
            BackColor = Color.Transparent,
            Location  = new Point(0, 312),
            Size      = new Size(420, 22),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var footer = new Label
        {
            AutoSize  = false,
            Text      = "v1.0.0  |  BLACK CORPS LOADER",
            Font      = new Font("Segoe UI", 7F),
            ForeColor = Color.FromArgb(35, 35, 60),
            BackColor = Color.Transparent,
            Location  = new Point(0, 378),
            Size      = new Size(420, 16),
            TextAlign = ContentAlignment.MiddleCenter
        };

        this.Controls.Add(pctLabel);
        this.Controls.Add(titleLabel);
        this.Controls.Add(div);
        this.Controls.Add(statusLabel);
        this.Controls.Add(footer);

        fillTimer = new System.Windows.Forms.Timer { Interval = 14 };
        fillTimer.Tick += (s, e) =>
        {
            if (_progress < _targetProg)
            {
                _progress       = Math.Min(_progress + 0.7f, _targetProg);
                pctLabel.Text   = $"{(int)_progress}%";
                this.Invalidate();
            }
        };
        fillTimer.Start();
    }

    private void StartSequence()
    {
        sequenceTimer = new System.Windows.Forms.Timer { Interval = 950 };
        sequenceTimer.Tick += async (s, e) =>
        {
            if (_step >= Steps.Length)
            {
                sequenceTimer.Stop();
                System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
                    this.Invoke(() => { _onComplete(); this.Close(); }));
                return;
            }
            var (msg, prog) = Steps[_step++];
            statusLabel.Text = msg;
            _targetProg      = prog;

            // When we hit "Checking for updates", run update check
            if (msg.StartsWith("Checking for updates"))
            {
                sequenceTimer.Stop();
                System.Threading.Tasks.Task.Run(async () => 
                {
                    await UpdateChecker.CheckAndApplyAsync(this, msg => this.Invoke(() => statusLabel.Text = msg));
                    this.Invoke(() => sequenceTimer.Start());
                });
            }
        };
        sequenceTimer.Start();
    }

    private void StartRain()
    {
        for (int i = 0; i < 30; i++)
            rain.Add(NewDrop(true));

        rainTimer = new System.Windows.Forms.Timer { Interval = 40 };
        rainTimer.Tick += (s, e) =>
        {
            for (int i = 0; i < rain.Count; i++)
            {
                rain[i].Y += rain[i].Speed * 2f;
                if (rain[i].Y > this.Height) rain[i] = NewDrop(false);
            }
            this.Invalidate();
        };
        rainTimer.Start();
    }

    private RainDrop NewDrop(bool randomY) => new RainDrop
    {
        X     = rnd.Next(0, 420),
        Y     = randomY ? rnd.Next(-40, 400) : -rnd.Next(5, 35),
        Len   = rnd.Next(10, 28),
        Speed = (float)(rnd.NextDouble() * 2.5 + 1.0),
        Alpha = rnd.Next(12, 60)
    };

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        rainTimer?.Dispose();
        sequenceTimer?.Dispose();
        fillTimer?.Dispose();
        base.OnFormClosed(e);
    }
}
