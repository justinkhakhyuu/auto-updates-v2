using ShayanUI;

namespace BlackCorps.App;

partial class MainMenu
{
    private System.ComponentModel.IContainer components = null;

    private ShayanBorderless  shayanBorderless;
    private ShayanHeader      shayanHeader;
    private ShayanTabSelector tabSelector;

    private Panel aimPanel;
    private Panel espPanel;
    private Panel miscPanel;
    private Panel keybindsPanel;
    private Panel settingsPanel;
    private Panel[] tabPanels;

    private System.Windows.Forms.Timer rainTimer;
    private List<RainDrop> rain = new();
    private Random rnd = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        rainTimer?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.shayanBorderless = new ShayanBorderless();
        this.shayanHeader     = new ShayanHeader();
        this.tabSelector      = new ShayanTabSelector();
        this.aimPanel         = new Panel();
        this.espPanel         = new Panel();
        this.miscPanel        = new Panel();
        this.keybindsPanel    = new Panel();
        this.settingsPanel    = new Panel();
        this.rainTimer        = new System.Windows.Forms.Timer();

        this.SuspendLayout();

        // ShayanBorderless
        this.shayanBorderless.TargetForm          = this;
        this.shayanBorderless.BorderRadius        = 16;
        this.shayanBorderless.EnableBlur          = true;
        this.shayanBorderless.DisableTransparency = false;
        this.shayanBorderless.Transparency        = 0.98;
        this.shayanBorderless.BlurColor           = Color.FromArgb(8, 6, 22);
        this.shayanBorderless.BlurOpacity         = 240;

        // ShayanHeader
        this.shayanHeader.Dock  = DockStyle.Top;
        this.shayanHeader.Size  = new Size(960, 42);
        this.shayanHeader.Title = "BLACK CORPS";

        // ShayanTabSelector — horizontal tabs
        this.tabSelector.Tabs            = new[] { "AIM", "ESP", "MISC", "KEYBINDS", "SETTINGS" };
        this.tabSelector.SelectedIndex   = 0;
        this.tabSelector.AutoAlignPanels = false;
        this.tabSelector.Dock            = DockStyle.Top;
        this.tabSelector.Height          = 48;
        this.tabSelector.BackColor       = Color.FromArgb(11, 9, 28);
        this.tabSelector.ForeColor       = Color.FromArgb(160, 160, 200);
        this.tabSelector.Font            = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.tabSelector.SelectedIndexChanged += (s, e) =>
        {
            for (int i = 0; i < tabPanels.Length; i++)
                tabPanels[i].Visible = (i == tabSelector.SelectedIndex);
        };

        // Content panels
        tabPanels = new[] { aimPanel, espPanel, miscPanel, keybindsPanel, settingsPanel };
        string[] names = { "AIM", "ESP", "MISC", "KEYBINDS", "SETTINGS" };
        for (int i = 0; i < tabPanels.Length; i++)
        {
            var p = tabPanels[i];
            p.BackColor = Color.Transparent;
            p.Dock      = DockStyle.Fill;
            p.AutoScroll = true;
            p.VerticalScroll.Enabled = true;
            p.VerticalScroll.Visible = true;
            p.HorizontalScroll.Enabled = false;
            p.HorizontalScroll.Visible = false;
            p.Visible   = (i == 0);
            string tabName = names[i];
            p.Controls.Add(new Label
            {
                AutoSize  = false, Text = tabName,
                Font      = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent,
                Location  = new Point(40, 30), Size = new Size(600, 48)
            });
            p.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(45, 138, 43, 226),
                Location  = new Point(40, 80), Size = new Size(880, 1)
            });
        }
        BuildEspTab();
        BuildAimTab();
        BuildSettingsTab();

        // Rain drawn via OnPaintBackground — no overlay panel needed
        this.Paint += (s, e) => PaintRain(e.Graphics);

        // Form
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode       = AutoScaleMode.Font;
        this.BackColor           = Color.FromArgb(8, 6, 22);
        this.ClientSize          = new Size(960, 580);
        this.FormBorderStyle     = FormBorderStyle.None;
        this.StartPosition       = FormStartPosition.CenterScreen;
        this.Text                = "Black Corps";

        // Add panels (reverse z-order — rain on top)
        foreach (var p in tabPanels) this.Controls.Add(p);
        this.Controls.Add(this.tabSelector);
        this.Controls.Add(this.shayanHeader);

        this.ResumeLayout(false);

        InitRain();
        ShowTab(0);
    }

    private void ShowTab(int index)
    {
        for (int i = 0; i < tabPanels.Length; i++)
            tabPanels[i].Visible = (i == index);
    }

    private void InitRain()
    {
        for (int i = 0; i < 35; i++)
            rain.Add(NewDrop(true));

        rainTimer.Interval = 40;
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

    private RainDrop NewDrop(bool randomY)
    {
        return new RainDrop
        {
            X     = rnd.Next(0, 960),
            Y     = randomY ? rnd.Next(-40, 580) : -rnd.Next(5, 40),
            Len   = rnd.Next(10, 28),
            Speed = (float)(rnd.NextDouble() * 2.8 + 1.0),
            Alpha = rnd.Next(15, 70)
        };
    }

    private void PaintRain(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        foreach (var d in rain)
        {
            using var pen = new Pen(Color.FromArgb(d.Alpha, 100, 30, 180), 1f);
            g.DrawLine(pen, d.X, d.Y, d.X, d.Y + d.Len);
        }
    }

    private void BuildEspTab()
    {
        var p = espPanel;
        int col1 = 40, col2 = 500, rowH = 38, y = 100;

        // ── Enable Hook ──────────────────────────────────────────
        var enableHook = MakeToggle("Enable Hook", col1, y);
        enableHook.CheckedChanged += (s,e) =>
        {
            if (enableHook.Checked)
            {
                System.Threading.Tasks.Task.Run(() => InjectionHelper.Inject());
            }
        };
        p.Controls.Add(enableHook);
        p.Controls.Add(MakeDimLabel("Enables the ESP injection hook", col1 + 230, y + 5));
        y += rowH + 4;
        p.Controls.Add(MakeDivider(col1, y, 840));
        y += 14;

        // ── PLAYER section label ──────────────────────────────────
        p.Controls.Add(MakeSectionLabel("PLAYER", col1, y));
        y += 22;

        // ── ESP Box + conditional box type ────────────────────────
        var espBox = MakeToggle("ESP Box", col1, y);
        var boxTypeLbl = MakeSubLabel("Box Type", 500, y + 7);
        var boxTypeDD  = MakeCombo(new[]{"2D","3D","Corner"}, 620, y);
        boxTypeLbl.Visible = false;
        boxTypeDD.Visible  = false;
        boxTypeDD.SelectedIndexChanged += (s,e) =>
            ShayanNotificationManager.Show("ESP Box", "Box type: " + boxTypeDD.SelectedItem, NotificationType.Info, 1800);
        espBox.CheckedChanged += (s,e) =>
        {
            boxTypeLbl.Visible = espBox.Checked;
            boxTypeDD.Visible  = espBox.Checked;
        };
        WireConfigPipe(espBox, "ESPBox");
        p.Controls.Add(espBox); p.Controls.Add(boxTypeLbl); p.Controls.Add(boxTypeDD);
        y += rowH + 2;

        // ── Auto Refresh ────────────────────────────────────────────
        var autoRefresh = MakeToggle("Auto Refresh", col2, y);
        WireToggleNotify(autoRefresh, "Auto Refresh");
        WireConfigPipe(autoRefresh, "UpdateEntities");
        p.Controls.Add(autoRefresh);
        y += rowH + 2;

        // ── ESP Skeleton + glow slider ────────────────────────────
        var espSkel     = MakeToggle("ESP Skeleton", col1, y);
        var glowLbl     = MakeSubLabel("Glow Intensity", 500, y + 7);
        var glowSlider  = MakeSlider(620, y, 60);
        glowLbl.Visible    = false;
        glowSlider.Visible = false;
        espSkel.CheckedChanged += (s,e) =>
        {
            glowLbl.Visible    = espSkel.Checked;
            glowSlider.Visible = espSkel.Checked;
        };
        WireConfigPipe(espSkel, "ESPSkeleton");
        p.Controls.Add(espSkel); p.Controls.Add(glowLbl); p.Controls.Add(glowSlider);
        y += rowH + 2;

        // ── ESP Name ──────────────────────────────────────────────
        var espName = MakeToggle("ESP Name", col1, y);
        WireConfigPipe(espName, "ESPName");
        p.Controls.Add(espName);
        y += rowH + 2;

        // ── ESP Box (swapped) → ESP Line ──────────────────────────
        var espLine      = MakeToggle("ESP Line", col1, y);
        var linePosLbl   = MakeSubLabel("Line Position", 500, y + 7);
        var linePosDD    = MakeCombo(new[]{"Top","Bottom","Middle"}, 620, y);
        linePosLbl.Visible = false;
        linePosDD.Visible  = false;
        linePosDD.SelectedIndexChanged += (s,e) =>
            ShayanNotificationManager.Show("ESP Line", "Line position: " + linePosDD.SelectedItem, NotificationType.Info, 1800);
        espLine.CheckedChanged += (s,e) =>
        {
            linePosLbl.Visible = espLine.Checked;
            linePosDD.Visible  = espLine.Checked;
        };
        WireConfigPipe(espLine, "ESPLine");
        p.Controls.Add(espLine); p.Controls.Add(linePosLbl); p.Controls.Add(linePosDD);
        y += rowH + 2;

        // ── ESP Health Bar ────────────────────────────────────────
        var espHp = MakeToggle("ESP Health Bar", col1, y);
        WireConfigPipe(espHp, "ESPHealth");
        p.Controls.Add(espHp);
        y += rowH + 2;

        // ── ESP Health Text ────────────────────────────────────────
        var espHpText = MakeToggle("ESP Health Text", col2, y);
        WireConfigPipe(espHpText, "ESPHealthText");
        p.Controls.Add(espHpText);
        y += rowH + 4;

        p.Controls.Add(MakeDivider(col1, y, 840));
        y += 14;
        p.Controls.Add(MakeSectionLabel("WEAPON", col1, y));
        y += 22;

        // ── ESP Weapon Name ────────────────────────────────────────
        var espWpnName = MakeToggle("ESP Weapon Name", col1, y);
        WireConfigPipe(espWpnName, "ESPWeaponName");
        p.Controls.Add(espWpnName);
        y += rowH + 2;

        // ── ESP Ammo ──────────────────────────────────────────────
        var espAmmo = MakeToggle("ESP Ammo Count", col1, y);
        WireConfigPipe(espAmmo, "espweapon");
        p.Controls.Add(espAmmo);
        y += rowH + 2;

        // ── ESP Distance + render slider ──────────────────────────
        var espDist      = MakeToggle("ESP Distance", col2, y);
        var distLbl      = MakeSubLabel("Render Distance", 700, y + 7);
        var distSlider   = MakeSlider(820, y, 75);
        distLbl.Visible    = false;
        distSlider.Visible = false;
        espDist.CheckedChanged += (s,e) =>
        {
            distLbl.Visible    = espDist.Checked;
            distSlider.Visible = espDist.Checked;
        };
        WireConfigPipe(espDist, "ESPDistance");
        p.Controls.Add(espDist); p.Controls.Add(distLbl); p.Controls.Add(distSlider);
        y += rowH + 4;

        p.Controls.Add(MakeDivider(col1, y, 840));
        y += 14;
        p.Controls.Add(MakeSectionLabel("BOX STYLES", col1, y));
        y += 22;

        // ── ESP Fill Box ───────────────────────────────────────────
        var espFillBox = MakeToggle("ESP Fill Box", col1, y);
        WireConfigPipe(espFillBox, "ESPFillBox");
        p.Controls.Add(espFillBox);
        y += rowH + 2;

        // ── ESP Corner Box (Box2) ─────────────────────────────────
        var espCornerBox = MakeToggle("ESP Corner Box", col2, y);
        WireConfigPipe(espCornerBox, "ESPBox2");
        p.Controls.Add(espCornerBox);
        y += rowH + 4;

        p.Controls.Add(MakeDivider(col1, y, 840));
        y += 14;
        p.Controls.Add(MakeSectionLabel("EXTRA", col1, y));
        y += 22;

        // ── ESP Shuriken ───────────────────────────────────────────
        var espShuriken = MakeToggle("ESP Shuriken", col1, y);
        WireConfigPipe(espShuriken, "ESPShuriken");
        p.Controls.Add(espShuriken);
        y += rowH + 2;

        // ── ESP Info Box ───────────────────────────────────────────
        var espInfoBox = MakeToggle("ESP Info Box", col2, y);
        WireConfigPipe(espInfoBox, "ESPInformation");
        p.Controls.Add(espInfoBox);
        y += rowH + 2;

        // ── Enemy Count ────────────────────────────────────────────
        var enemyCount = MakeToggle("Enemy Count", col1, y);
        WireConfigPipe(enemyCount, "EnemyCount");
        p.Controls.Add(enemyCount);
        y += rowH + 2;

        // ── ESP Fix ────────────────────────────────────────────────
        var espFix = MakeToggle("ESP Fix", col2, y);
        WireConfigPipe(espFix, "FixEsp");
        p.Controls.Add(espFix);
        y += rowH + 2;
    }

    private void WireToggleNotify(ShayanCheckBox cb, string name)
    {
        cb.CheckedChanged += (s, e) =>
        {
            if (_suppressNotify) return;
            if (MuteNotifications) return;
            bool chk = cb.Checked;
            ShayanNotificationManager.Show(name,
                chk ? name + " enabled" : name + " disabled",
                chk ? NotificationType.Info : NotificationType.Info, 1800);
        };
    }

    private Label MakeSubLabel(string text, int x, int y) => new Label
    {
        AutoSize  = false, Text = text,
        Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
        ForeColor = Color.FromArgb(138, 43, 226), BackColor = Color.Transparent,
        Location  = new Point(x, y), Size = new Size(120, 18)
    };

    private ShayanComboBox MakeCombo(string[] items, int x, int y)
    {
        var cb = new ShayanComboBox
        {
            Location      = new Point(x, y),
            Size          = new Size(130, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor     = Color.FromArgb(8, 6, 22),
            ForeColor     = Color.White,
            Font          = new Font("Segoe UI", 9F),
            BorderRadius  = 6
        };
        cb.Items.AddRange(items);
        cb.SelectedIndex = 0;
        return cb;
    }

    private ShayanSlider MakeSlider(int x, int y, int val) => new ShayanSlider
    {
        Location    = new Point(x, y),
        Size        = new Size(140, 28),
        Minimum     = 0,
        Maximum     = 100,
        Value       = val,
        AccentColor = Color.FromArgb(138, 43, 226),
        BackColor   = Color.FromArgb(8, 6, 22)
    };

    private void BuildAimTab()
    {
        var p = aimPanel;
        int x = 40, y = 100, rowH = 38;

        // ── Left column ───────────────────────────────────────────

        // Aimbot Collider
        var aimbotCollider = MakeToggle("Aimbot Collider", x, y);
        aimbotCollider.CheckedChanged += async (s,e) =>
        {
            if (aimbotCollider.Checked)
            {
                MainMenu.AimbotColliderInstance = new AimbotAI();
                bool success = await MainMenu.AimbotColliderInstance.InitAimbot();
                if (success)
                {
                    if (!MainMenu.MuteNotifications)
                        ShayanNotificationManager.Show("Aimbot Collider", "Applied Successfully!", NotificationType.Info, 1800);
                }
                else
                {
                    aimbotCollider.Checked = false;
                    MainMenu.AimbotColliderInstance = null;
                    if (!MainMenu.MuteNotifications)
                        ShayanNotificationManager.Show("Aimbot Collider", "Game Not Found - Start HD-Player!", NotificationType.Info, 1800);
                }
            }
            else
            {
                MainMenu.AimbotColliderInstance?.Stop();
                MainMenu.AimbotColliderInstance = null;
            }
        };
        p.Controls.Add(aimbotCollider);
        y += rowH;

        // Aimbot External
        var aimbotExternal = MakeToggle("Aimbot External", x, y);
        aimbotExternal.CheckedChanged += async (s,e) =>
        {
            if (aimbotExternal.Checked)
            {
                await ExternalAimbot.EnableAimbotExternal();
            }
            else
            {
                ExternalAimbot.DisableAimbotExternal();
            }
        };
        p.Controls.Add(aimbotExternal);
        y += rowH + 24; // gap

        // Divider
        p.Controls.Add(MakeDivider(x, y, 400));
        y += 14;

        // AWM-Y Switch
        var awmySwitch = MakeToggle("AWM-Y Switch", x, y);
        awmySwitch.CheckedChanged += async (s, e) =>
        {
            if (awmySwitch.Checked)
            {
                bool wasMuted = MuteNotifications;
                MuteNotifications = true;
                awmySwitch.Checked = false;
                MuteNotifications = wasMuted;
                
                var startTime = DateTime.Now;
                if (!MainMenu.MuteNotifications)
                    ShayanNotificationManager.Show("AWM-Y Switch", "Applying...", NotificationType.Info, 1800);
                
                bool initialized = await AWMYSwitch.Initialize();
                if (initialized)
                {
                    bool enabled = AWMYSwitch.Enable();
                    if (enabled)
                    {
                        var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
                        if (!MainMenu.MuteNotifications)
                            ShayanNotificationManager.Show("AWM-Y Switch", $"Applied Successfully! ({timeTaken}s)", NotificationType.Info, 1800);
                        awmySwitch.Checked = true;
                    }
                    else
                    {
                        var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
                        if (!MainMenu.MuteNotifications)
                            ShayanNotificationManager.Show("AWM-Y Switch", $"Failed to Apply! ({timeTaken}s)", NotificationType.Info, 3000);
                    }
                }
                else
                {
                    var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
                    if (!MainMenu.MuteNotifications)
                        ShayanNotificationManager.Show("AWM-Y Switch", $"Failed to Load! ({timeTaken}s)", NotificationType.Info, 3000);
                }
            }
            else
            {
                if (!MainMenu.MuteNotifications)
                    ShayanNotificationManager.Show("AWM-Y Switch", "Cleared", NotificationType.Info, 1800);
                AWMYSwitch.Disable();
            }
        };
        p.Controls.Add(awmySwitch);
        y += rowH;

        // Sniper Scope
        var sniperScopeLoad = MakeToggle("Sniper Scope Load", x, y);
        sniperScopeLoad.CheckedChanged += async (s,e) =>
        {
            if (sniperScopeLoad.Checked)
            {
                bool wasMuted = MuteNotifications;
                MuteNotifications = true;
                sniperScopeLoad.Checked = false;
                MuteNotifications = wasMuted;
                
                var startTime = DateTime.Now;
                ShayanNotificationManager.Show("Sniper Scope", "Loading...", NotificationType.Info, 1800);
                
                bool initialized = await SniperScope.Initialize();
                if (initialized)
                {
                    var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
                    ShayanNotificationManager.Show("Sniper Scope", $"Loaded Successfully! ({timeTaken}s)", NotificationType.Info, 1800);
                    sniperScopeLoad.Checked = true;
                }
                else
                {
                    var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
                    ShayanNotificationManager.Show("Sniper Scope", $"Failed to Load! ({timeTaken}s)", NotificationType.Info, 3000);
                }
            }
            else
            {
                ShayanNotificationManager.Show("Sniper Scope", "Cleared", NotificationType.Info, 1800);
                SniperScope.Clear();
            }
        };
        p.Controls.Add(sniperScopeLoad);
        y += rowH;

        var sniperScopeMacro = MakeToggle("Sniper Scope Macro", x, y);
        var sniperScopeKey = MakeKeybindBox(x + 240, y);
        RegisterMacroKeybind(sniperScopeKey, sniperScopeMacro, "Sniper Scope Macro");
        p.Controls.Add(sniperScopeMacro);
        p.Controls.Add(sniperScopeKey);
        y += rowH + 24; // gap

        // Divider
        p.Controls.Add(MakeDivider(x, y, 400));
        y += 14;

        // Female Fix External
        var recoilFix = MakeToggle("Female Fix External", x, y);
        recoilFix.CheckedChanged += async (s, e) =>
        {
            if (recoilFix.Checked)
            {
                bool wasMuted = MuteNotifications;
                MuteNotifications = true;
                recoilFix.Checked = false;
                MuteNotifications = wasMuted;
                
                var startTime = DateTime.Now;
                if (!MainMenu.MuteNotifications)
                    ShayanNotificationManager.Show("Female Fix", "Applying...", NotificationType.Info, 1800);
                
                bool initialized = await FemaleFix.Initialize();
                if (initialized)
                {
                    bool enabled = FemaleFix.Enable();
                    if (enabled)
                    {
                        var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
                        if (!MainMenu.MuteNotifications)
                            ShayanNotificationManager.Show("Female Fix", $"Applied Successfully! ({timeTaken}s)", NotificationType.Info, 1800);
                        recoilFix.Checked = true;
                    }
                    else
                    {
                        var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
                        if (!MainMenu.MuteNotifications)
                            ShayanNotificationManager.Show("Female Fix", $"Failed to Apply! ({timeTaken}s)", NotificationType.Info, 3000);
                    }
                }
                else
                {
                    var timeTaken = (DateTime.Now - startTime).TotalSeconds.ToString("F2");
                    if (!MainMenu.MuteNotifications)
                        ShayanNotificationManager.Show("Female Fix", $"Failed to Load! ({timeTaken}s)", NotificationType.Info, 3000);
                }
            }
            else
            {
                if (!MainMenu.MuteNotifications)
                    ShayanNotificationManager.Show("Female Fix", "Cleared", NotificationType.Info, 1800);
                FemaleFix.Disable();
            }
        };
        p.Controls.Add(recoilFix);
        y += rowH;

        // Aimbot Body
        var aimbotBody = MakeToggle("Aimbot Body", x, y);
        WireToggleNotify(aimbotBody, "Aimbot Body");
        p.Controls.Add(aimbotBody);

        // ── Right column ──────────────────────────────────────────
        int rx = 500;

        var aimbotAI = MakeToggle("Aimbot AI", rx, 100);
        WireToggleNotify(aimbotAI, "Aimbot AI");
        WireConfigPipe(aimbotAI, "AimbotVisible");
        p.Controls.Add(aimbotAI);

        // ── Fake Leg Menu — aligned with AWM-Y Switch ─────────────
        // AWM-Y Switch sits at: y=100 + rowH(38) + rowH+24(62) + 14 = 214
        int ry = 214;

        p.Controls.Add(MakeSectionLabel("FAKE LAG MENU", rx, ry));
        ry += 22;
        p.Controls.Add(new Panel
        {
            BackColor = Color.FromArgb(40, 138, 43, 226),
            Location  = new Point(rx, ry), Size = new Size(400, 1)
        });
        ry += 12;

        // Aim Lag — checkbox + keybind (keybind toggles checkbox)
        var aimLag    = MakeToggle("Aim Lag",   rx, ry);
        var aimLagKey = MakeKeybindBox(rx + 240, ry);
        RegisterKeybind(aimLagKey, aimLag, "Aim Lag");
        aimLag.CheckedChanged += (s,e) =>
        {
            if (aimLag.Checked)
            {
                bool wasMuted = MuteNotifications;
                MuteNotifications = true;
                aimLag.Checked = false;
                MuteNotifications = wasMuted;
                AimLag.Start();
                aimLag.Checked = true;
            }
            else
            {
                AimLag.Stop();
            }
        };
        p.Controls.Add(aimLag); p.Controls.Add(aimLagKey);
        ry += rowH + 6;

        // Fake Lag — checkbox + keybind
        var fakeLag    = MakeToggle("Fake Lag",  rx, ry);
        var fakeLagKey = MakeKeybindBox(rx + 240, ry);
        RegisterKeybind(fakeLagKey, fakeLag, "Fake Lag");
        fakeLag.CheckedChanged += (s,e) =>
        {
            if (fakeLag.Checked)
            {
                bool wasMuted = MuteNotifications;
                MuteNotifications = true;
                fakeLag.Checked = false;
                MuteNotifications = wasMuted;
                FakeLag.Start();
                fakeLag.Checked = true;
            }
            else
            {
                FakeLag.Stop();
            }
        };
        p.Controls.Add(fakeLag);
        p.Controls.Add(fakeLagKey);
        ry += rowH + 6;

        // Ghost Lag — checkbox + keybind
        var ghostLag    = MakeToggle("Ghost Lag", rx, ry);
        var ghostLagKey = MakeKeybindBox(rx + 240, ry);
        RegisterKeybind(ghostLagKey, ghostLag, "Ghost Lag");
        ghostLag.CheckedChanged += (s,e) =>
        {
            if (ghostLag.Checked)
            {
                bool wasMuted = MuteNotifications;
                MuteNotifications = true;
                ghostLag.Checked = false;
                MuteNotifications = wasMuted;
                GhostLag.Start();
                ghostLag.Checked = true;
            }
            else
            {
                GhostLag.Stop();
            }
        };
        p.Controls.Add(ghostLag); p.Controls.Add(ghostLagKey);
    }

    private TextBox MakeKeybindBox(int x, int y) => new TextBox
    {
        Location    = new Point(x, y + 2),
        Size        = new Size(72, 24),
        Text        = "None",
        ReadOnly    = true,
        BackColor   = Color.FromArgb(18, 14, 42),
        ForeColor   = Color.FromArgb(138, 43, 226),
        Font        = new Font("Segoe UI", 8F, FontStyle.Bold),
        BorderStyle = BorderStyle.FixedSingle,
        TextAlign   = HorizontalAlignment.Center,
        Cursor      = Cursors.Hand,
        Tag         = (object)false
    };

    private void BuildSettingsTab()
    {
        var p   = settingsPanel;
        int lx  = 40;   // left column
        int rx  = 600;  // right column (far right)
        int y   = 100;
        int rowH = 38;

        // ── LEFT: Stream Mode ────────────────────────────────────
        var streamMode = MakeToggle("Stream Mode", lx, y);
        WireToggleNotify(streamMode, "Stream Mode");
        WireConfigPipe(streamMode, "StreamerMode");
        streamMode.CheckedChanged += (s,e) =>
        {
            if (streamMode.Checked)
            {
                this.ShowInTaskbar = false;
                SetWindowDisplayAffinity(this.Handle, 17U); // Hide from screen share
            }
            else
            {
                this.ShowInTaskbar = true;
                SetWindowDisplayAffinity(this.Handle, 0U); // Show normally
            }
        };
        p.Controls.Add(streamMode);
        y += rowH + 2;

        // ── LEFT: Mute Notifications ───────────────────────────────
        var muteNotif = MakeToggle("Mute Notifications", lx, y);
        muteNotif.CheckedChanged += (s,e) => MuteNotifications = muteNotif.Checked;
        p.Controls.Add(muteNotif);
        y += rowH + 2;

        // ── LEFT: Clear Binds ──────────────────────────────────────
        var clearBinds = MakeToggle("Clear Binds", lx, y);
        clearBinds.CheckedChanged += (s,e) =>
        {
            if (clearBinds.Checked)
            {
                _keybinds.Clear();
                _lastToggle.Clear();
                _mouseKeybinds.Clear();
                _keyPressed.Clear();
                _mouseKeyPressed.Clear();
                ShayanNotificationManager.Show("Keybinds", "All Keybinds Cleared", NotificationType.Info, 2000);
                clearBinds.Checked = false;
            }
        };
        p.Controls.Add(clearBinds);
        y += rowH + 2;

        // ── LEFT: Fix Fake Lag (WinDivert) ─────────────────────────
        var fixFakeLag = MakeToggle("Fix Fake Lag", lx, y);
        fixFakeLag.CheckedChanged += (s,e) =>
        {
            if (fixFakeLag.Checked)
            {
                bool wasMuted = MuteNotifications;
                MuteNotifications = true;
                fixFakeLag.Checked = false;
                MuteNotifications = wasMuted;

                if (!WinDivertHelper.CheckWinDivertFiles())
                {
                    bool success = WinDivertHelper.CopyWinDivertFiles();
                    if (success)
                    {
                        ShayanNotificationManager.Show("Fix Fake Lag", "WinDivert files installed successfully", NotificationType.Success, 2000);
                    }
                    else
                    {
                        ShayanNotificationManager.Show("Fix Fake Lag", "Failed to install WinDivert files", NotificationType.Info, 3000);
                    }
                }
                else
                {
                    ShayanNotificationManager.Show("Fix Fake Lag", "WinDivert files already present", NotificationType.Info, 2000);
                }
            }
        };
        p.Controls.Add(fixFakeLag);
        y += rowH + 2;

        // ── RIGHT: UI Accent Color swatch + label ─────────────────
        p.Controls.Add(new Label
        {
            AutoSize  = false, Text = "UI Accent Color",
            Font      = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(190, 190, 220), BackColor = Color.Transparent,
            Location  = new Point(rx, y + 6), Size = new Size(130, 22)
        });
        var colorSwatch = new Panel
        {
            Location  = new Point(rx + 140, y + 4),
            Size      = new Size(28, 28),
            BackColor = Color.FromArgb(138, 43, 226),
            Cursor    = Cursors.Hand
        };
        colorSwatch.Click += (s, e) =>
        {
            using var dlg = new ColorDialog { Color = colorSwatch.BackColor, FullOpen = true };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                colorSwatch.BackColor = dlg.Color;
                if (!MainMenu.MuteNotifications)
                    ShayanNotificationManager.Show("UI Color", "Accent color updated", NotificationType.Info, 1800);
            }
        };
        p.Controls.Add(colorSwatch);

        y += rowH * 2; // two blocks down

        // ── LEFT: Close Menu ──────────────────────────────────────
        var closeMenu = MakeToggle("Close Menu", lx, y);
        WireToggleNotify(closeMenu, "Close Menu");
        closeMenu.CheckedChanged += (s, e) =>
        {
            if (closeMenu.Checked) Application.Exit();
        };
        p.Controls.Add(closeMenu);

        // ── RIGHT: Open Emulator (same Y) ─────────────────────────
        var openEmu = MakeToggle("Open Emulator", rx, y);
        WireToggleNotify(openEmu, "Open Emulator");
        p.Controls.Add(openEmu);

        y += rowH;

        // ── LEFT: Download Required Emulator ──────────────────────
        var dlEmu = MakeToggle("Download Required Emulator", lx, y);
        WireToggleNotify(dlEmu, "Download Required Emulator");
        p.Controls.Add(dlEmu);

        // ── RIGHT: Download Required Free Fire ────────────────────
        var dlFF = MakeToggle("Download Required Free Fire", rx, y);
        WireToggleNotify(dlFF, "Download Required Free Fire");
        p.Controls.Add(dlFF);

        y += rowH;

        // ── LEFT: Check for Updates ───────────────────────────────
        var checkUpdates = MakeToggle("Check for Updates", lx, y);
        WireToggleNotify(checkUpdates, "Check for Updates");
        p.Controls.Add(checkUpdates);

        // ── RIGHT: Validate License ───────────────────────────────
        var validateLic = MakeToggle("Validate License", rx, y);
        WireToggleNotify(validateLic, "Validate License");
        p.Controls.Add(validateLic);
    }

    // ── ESP tab helpers ──────────────────────────────────────────

    private ShayanCheckBox MakeToggle(string text, int x, int y) => new ShayanCheckBox
    {
        Text         = text,
        AutoSize     = false,
        Checked      = false,
        Size         = new Size(220, 28),
        Location     = new Point(x, y),
        Font         = new Font("Segoe UI", 9F),
        ForeColor    = Color.FromArgb(190, 190, 220),
        BackColor    = Color.Transparent,
        CheckedColor = Color.FromArgb(138, 43, 226)
    };

    private Label MakeSectionLabel(string text, int x, int y) => new Label
    {
        AutoSize  = false, Text = text,
        Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
        ForeColor = Color.FromArgb(138, 43, 226), BackColor = Color.Transparent,
        Location  = new Point(x, y), Size = new Size(200, 18)
    };

    private Label MakeDimLabel(string text, int x, int y) => new Label
    {
        AutoSize  = false, Text = text,
        Font      = new Font("Segoe UI", 8F),
        ForeColor = Color.FromArgb(70, 70, 100), BackColor = Color.Transparent,
        Location  = new Point(x, y), Size = new Size(300, 18)
    };

    private Panel MakeDivider(int x, int y, int width) => new Panel
    {
        BackColor = Color.FromArgb(22, 138, 43, 226),
        Location  = new Point(x, y), Size = new Size(width, 1)
    };
}
