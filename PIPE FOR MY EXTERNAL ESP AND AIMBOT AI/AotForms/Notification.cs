using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AotForms
{
    public partial class Notification : Form
    {
        private System.Windows.Forms.Timer timeoutTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer();

        public Notification(string message, Color neonColor)
        {
            InitializeComponent();

            // Text and Neon Color Setup
            lblMsg.Text = message;
            lblMsg.ForeColor = neonColor;

            // Transparency and Background
            this.BackColor = Color.FromArgb(15, 15, 15); // Dark Background
            this.Opacity = 0; // Start invisible for fade-in effect

            // Optional: Set panel border to neon color if you have a guna2Panel1
            guna2Panel1.BorderColor = neonColor;

            // Position it at the bottom right of the screen
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width - 10,
                                     Screen.PrimaryScreen.WorkingArea.Height - this.Height - 10);

            // 1. Smooth Fade-In Logic
            fadeTimer.Interval = 15;
            fadeTimer.Tick += (s, e) =>
            {
                if (this.Opacity < 0.85) // 0.85 makes it "Little Transparent"
                    this.Opacity += 0.05;
                else
                    fadeTimer.Stop();
            };
            fadeTimer.Start();

            // 2. Auto-Close Logic
            timeoutTimer.Interval = 3000; // Show for 3 seconds
            timeoutTimer.Tick += (s, e) => { this.Close(); };
            timeoutTimer.Start();
        }

        // Static method to show it easily from anywhere
        public static void Show(string message, Color color)
        {
            // We create a new instance so multiple notifications don't crash
            Notification notif = new Notification(message, color);
            notif.Show();
        }

        // KEEP THIS: The Designer needs the empty constructor to load the preview
        public Notification()
        {
            InitializeComponent();
        }

        private void Notification_Load(object sender, EventArgs e)
        {
            // Do not remove this method to avoid designer errors
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {
            // Do not remove this method to avoid designer errors
        }
    }
}