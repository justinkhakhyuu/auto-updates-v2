using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AotForms
{
    public static class UIHelper
    {
        // ── Premium Light Red / Pink Palette ──────────────────────────────
        public static Color AccentColor   = Color.FromArgb(232, 24, 74);    // Vivid Rose-Red
        public static Color AccentDark    = Color.FromArgb(180, 10, 50);    // Deep Crimson
        public static Color AccentLight   = Color.FromArgb(255, 182, 193);  // Soft Pink

        public static Color BgDark        = Color.FromArgb(255, 240, 245);  // Lavender Blush (form bg)
        public static Color BgMedium      = Color.FromArgb(255, 228, 235);  // Misty Rose (panel bg)
        public static Color BgPanel       = Color.FromArgb(255, 214, 224);  // Pinkish Panel (tab bar)

        // ── Text System ──────────────────────────────────────────────────
        // Single high-contrast shade for the "Proper UI" look
        public static Color TextPrimary   = Color.FromArgb(60, 12, 28);    // Deep Wine (readable on pink)
        public static Color TextSecondary = Color.FromArgb(155, 65, 90);   // Muted Rose
        public static Color TextLogo      = Color.FromArgb(232, 24, 74);    // Vivid Rose (branding)

        // Legacy aliases so existing code keeps compiling
        public static Color TextWhite = Color.FromArgb(60, 12, 28);
        public static Color TextGray  = Color.FromArgb(155, 65, 90);

        // ─────────────────────────────────────────────────────────────────
        public static void ApplyGlow(Control parent)
        {
            parent.BackColor = BgDark;

            if (parent is Form form)
            {
                form.BackColor = BgDark;
                if (form.FormBorderStyle == FormBorderStyle.None)
                {
                    form.Paint += (s, e) =>
                    {
                        using (Pen p = new Pen(AccentColor, 2))
                            e.Graphics.DrawRectangle(p, 0, 0, form.Width - 1, form.Height - 1);
                    };
                }
            }

            foreach (Control c in parent.Controls)
            {
                ApplyStyle(c);
                if (c.HasChildren) ApplyGlow(c);
            }
        }

        private static void ApplyStyle(Control c)
        {
            // ── 1. TextBoxes ─────────────────────────────────────────────
            if (c is Guna2TextBox txt)
            {
                txt.FillColor            = Color.FromArgb(255, 250, 252);
                txt.BorderColor          = Color.FromArgb(255, 182, 200);
                txt.BorderRadius         = 6;
                txt.BorderThickness      = 1;
                txt.ForeColor            = TextPrimary;
                txt.PlaceholderForeColor = TextSecondary;

                txt.FocusedState.BorderColor = AccentColor;
                txt.FocusedState.FillColor   = Color.White;
                txt.HoverState.BorderColor   = AccentLight;

                txt.ShadowDecoration.Enabled = true;
                txt.ShadowDecoration.Color   = AccentLight;
                txt.ShadowDecoration.Depth   = 6;
                txt.Animated = true;
            }
            // ── 2. Buttons ───────────────────────────────────────────────
            else if (c is Guna2Button btn)
            {
                btn.FillColor       = AccentColor;
                btn.BorderColor     = AccentDark;
                btn.BorderThickness = 0;
                btn.BorderRadius    = 6;
                btn.ForeColor       = Color.White;
                btn.Font            = new Font("Segoe UI", 9f, FontStyle.Bold);

                btn.HoverState.FillColor   = Color.FromArgb(255, 80, 120);
                btn.HoverState.ForeColor   = Color.White;
                btn.HoverState.BorderColor = Color.FromArgb(255, 80, 120);

                btn.PressedColor = AccentDark;
                btn.Animated     = true;

                btn.ShadowDecoration.Enabled = true;
                btn.ShadowDecoration.Color   = Color.FromArgb(160, 232, 24, 74);
                btn.ShadowDecoration.Depth   = 14;
            }
            // ── 3. Tab Control ───────────────────────────────────────────
            else if (c is Guna2TabControl tab)
            {
                tab.TabMenuBackColor = BgPanel;

                tab.TabButtonIdleState.FillColor = BgPanel;
                tab.TabButtonIdleState.ForeColor = TextSecondary;
                tab.TabButtonIdleState.Font      = new Font("Segoe UI", 9f, FontStyle.Regular);

                tab.TabButtonSelectedState.FillColor  = BgMedium;
                tab.TabButtonSelectedState.ForeColor  = AccentColor;
                tab.TabButtonSelectedState.Font       = new Font("Segoe UI", 9f, FontStyle.Bold);
                tab.TabButtonSelectedState.InnerColor = AccentColor;

                tab.TabButtonHoverState.FillColor = AccentLight;
                tab.TabButtonHoverState.ForeColor = AccentDark;

                foreach (TabPage page in tab.TabPages)
                    page.BackColor = BgDark;
            }
            // ── 4. Panels ────────────────────────────────────────────────
            else if (c is Guna2Panel pnl)
            {
                if (pnl.Size.Height < 50 && pnl.Dock == DockStyle.Top)
                {
                    pnl.FillColor       = AccentColor;
                    pnl.BorderColor     = AccentDark;
                    pnl.BorderThickness = 0;
                }
                else
                {
                    pnl.FillColor       = BgMedium;
                    pnl.BorderColor     = AccentLight;
                    pnl.BorderThickness = 1;
                }
            }
            // ── 5. CheckBoxes ────────────────────────────────────────────
            else if (c is Guna2CustomCheckBox chk)
            {
                chk.CheckedState.FillColor     = AccentColor;
                chk.CheckedState.BorderColor   = AccentDark;
                chk.UncheckedState.FillColor   = Color.White;
                chk.UncheckedState.BorderColor = Color.FromArgb(255, 160, 180);
                chk.CheckMarkColor = Color.White;

                chk.ShadowDecoration.Enabled = true;
                chk.ShadowDecoration.Color   = AccentLight;
                chk.ShadowDecoration.Depth   = 8;
            }
            else if (c is Label lbl)
            {
                lbl.BackColor = Color.Transparent;
                string t = (lbl.Text ?? "").Trim().ToUpper();

                // Proper UI: branding stays rose, feature labels are clean deep wine
                if (t.Contains("STREAMERX") || t.Contains("3.0") || t.Contains("INTERNAL"))
                {
                    lbl.ForeColor = TextLogo;
                    try { lbl.Font = new Font(lbl.Font.FontFamily, lbl.Font.Size, FontStyle.Bold); } catch { }
                }
                else
                {
                    lbl.ForeColor = TextPrimary;
                    // Standard consistency: no forced bold for normal items
                    if (lbl.Font.Bold && !t.Contains("FOV"))
                        try { lbl.Font = new Font(lbl.Font.FontFamily, lbl.Font.Size, FontStyle.Regular); } catch { }
                }
            }
        }
    }
}
