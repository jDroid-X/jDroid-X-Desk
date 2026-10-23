using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Reflection;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace JDroidXDesk.Native
{
    public class JDroidXIncomingSessionDialog : Form
    {
        private Action<bool, bool, bool, bool> onAccept;
        private Action onDismiss;

        public JDroidXIncomingSessionDialog(string endpoint, Action<bool, bool, bool, bool> acceptCallback, Action dismissCallback)
        {
            this.onAccept = acceptCallback;
            this.onDismiss = dismissCallback;

            this.Width = 380;
            this.Height = 250;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            bool isDark = ThemePalette.IsDarkTheme;
            Color bgCard = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(255, 255, 255);
            Color topBarBg = isDark ? Color.FromArgb(30, 58, 138) : Color.FromArgb(37, 99, 235);
            Color textPrimary = isDark ? Color.White : Color.FromArgb(15, 23, 42);
            Color textSecondary = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
            Color accentColor = isDark ? Color.FromArgb(56, 189, 248) : Color.FromArgb(37, 99, 235);
            Color borderColor = isDark ? Color.FromArgb(59, 130, 246) : Color.FromArgb(37, 99, 235);

            this.BackColor = bgCard;

            Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workArea.Right - this.Width - 24, workArea.Bottom - this.Height - 24);

            // Draggable Title Bar
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = topBarBg, Padding = new Padding(12, 6, 8, 6), Cursor = Cursors.SizeAll };
            Label lblTitle = new Label { Text = "⚡ jDroid-X SESSION REQUEST", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Left, AutoSize = true, Cursor = Cursors.SizeAll };
            
            // Drag Support so notification can be moved anywhere on screen
            Point dragOffset = Point.Empty;
            bool dragging = false;
            topBar.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    dragging = true;
                    dragOffset = new Point(e.X, e.Y);
                }
            };
            topBar.MouseMove += (s, e) => {
                if (dragging) {
                    Point currentScreenPos = PointToScreen(e.Location);
                    this.Location = new Point(currentScreenPos.X - dragOffset.X, currentScreenPos.Y - dragOffset.Y);
                }
            };
            topBar.MouseUp += (s, e) => { dragging = false; };

            lblTitle.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    dragging = true;
                    dragOffset = new Point(e.X + lblTitle.Left, e.Y + lblTitle.Top);
                }
            };
            lblTitle.MouseMove += (s, e) => {
                if (dragging) {
                    Point currentScreenPos = PointToScreen(new Point(e.X + lblTitle.Left, e.Y + lblTitle.Top));
                    this.Location = new Point(currentScreenPos.X - dragOffset.X, currentScreenPos.Y - dragOffset.Y);
                }
            };
            lblTitle.MouseUp += (s, e) => { dragging = false; };

            // Quick Dismiss Button (✕)
            Label btnClose = new Label {
                Text = "✕",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Right,
                Width = 28,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnClose.MouseEnter += (s, e) => { btnClose.ForeColor = Color.FromArgb(254, 202, 202); };
            btnClose.MouseLeave += (s, e) => { btnClose.ForeColor = Color.White; };
            btnClose.Click += (s, e) => {
                if (onDismiss != null) onDismiss();
                this.Close();
            };

            topBar.Controls.Add(lblTitle);
            topBar.Controls.Add(btnClose);

            // Content Panel
            Panel pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 10, 16, 10), BackColor = bgCard };
            Label lblEndpoint = new Label { Text = "Remote Device: " + (string.IsNullOrEmpty(endpoint) ? "Remote Operator" : endpoint), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = accentColor, Location = new Point(14, 8), AutoSize = true };
            
            CheckBox chkInput = new CheckBox { Text = "Allow Mouse & Keyboard Control", Checked = true, Location = new Point(16, 36), AutoSize = true, ForeColor = textPrimary, Font = new Font("Segoe UI", 8.2f) };
            CheckBox chkClip = new CheckBox { Text = "Allow Clipboard Synchronization", Checked = true, Location = new Point(16, 60), AutoSize = true, ForeColor = textPrimary, Font = new Font("Segoe UI", 8.2f) };
            CheckBox chkFiles = new CheckBox { Text = "Allow File Transfer", Checked = true, Location = new Point(16, 84), AutoSize = true, ForeColor = textPrimary, Font = new Font("Segoe UI", 8.2f) };
            CheckBox chkLock = new CheckBox { Text = "Lock Local Physical Input", Checked = false, Location = new Point(16, 108), AutoSize = true, ForeColor = Color.FromArgb(220, 38, 38), Font = new Font("Segoe UI", 8.2f) };

            FlowLayoutPanel btnBox = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 6, 12, 0), BackColor = bgCard };
            ModernButton btnAccept = new ModernButton { Text = "✅ Accept", Size = new Size(110, 32), NormalColor = Color.FromArgb(16, 185, 129), HoverColor = Color.FromArgb(5, 150, 105), Margin = new Padding(8, 0, 0, 0) };
            ModernButton btnDismiss = new ModernButton { Text = "❌ Dismiss", Size = new Size(110, 32), NormalColor = Color.FromArgb(220, 38, 38), HoverColor = Color.FromArgb(185, 28, 28) };

            btnAccept.Click += (s, e) => {
                if (onAccept != null) onAccept(chkInput.Checked, chkClip.Checked, chkFiles.Checked, chkLock.Checked);
                this.Close();
            };
            btnDismiss.Click += (s, e) => {
                if (onDismiss != null) onDismiss();
                this.Close();
            };

            btnBox.Controls.Add(btnAccept);
            btnBox.Controls.Add(btnDismiss);

            pnlContent.Controls.Add(lblEndpoint);
            pnlContent.Controls.Add(chkInput);
            pnlContent.Controls.Add(chkClip);
            pnlContent.Controls.Add(chkFiles);
            pnlContent.Controls.Add(chkLock);

            this.Controls.Add(pnlContent);
            this.Controls.Add(btnBox);
            this.Controls.Add(topBar);

            this.Paint += (s, e) => {
                using (Pen borderPen = new Pen(borderColor, 2))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
                }
            };
        }
    }

    // Chesterton's Fence: Backward-compatible alias for existing callers
    public class AnyDeskIncomingSessionDialog : JDroidXIncomingSessionDialog
    {
        public AnyDeskIncomingSessionDialog(string endpoint, Action<bool, bool, bool, bool> acceptCallback, Action dismissCallback)
            : base(endpoint, acceptCallback, dismissCallback)
        {
        }
    }

    // ==========================================================================
    // 2.5 DYNAMIC ENGINE SCRIPT PROVISIONER (AUTOMATIC FRESH DESK INSTALLATION)
    // ==========================================================================

}
