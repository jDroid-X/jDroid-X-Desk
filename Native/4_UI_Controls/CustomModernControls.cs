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
    public class ModernButton : Button
    {
        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }
        public Color TextColor { get; set; }
        public int BorderRadius { get; set; }

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.NormalColor = Color.FromArgb(239, 51, 64);
            this.HoverColor = Color.FromArgb(200, 30, 45);
            this.TextColor = Color.White;
            this.BackColor = NormalColor;
            this.ForeColor = TextColor;
            this.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.Cursor = Cursors.Hand;
            this.BorderRadius = 6;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.BackColor = HoverColor;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.BackColor = NormalColor;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = ThemePalette.CreateRoundedPath(rect, BorderRadius))
            {
                using (SolidBrush b = new SolidBrush(this.BackColor))
                {
                    g.FillPath(b, path);
                }
            }
            TextRenderer.DrawText(g, this.Text, this.Font, rect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    public class ModernCardPanel : Panel
    {
        public Color BorderColor { get; set; }
        public Color CardBackground { get; set; }

        public ModernCardPanel()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.BorderColor = Color.Empty;
            this.CardBackground = Color.Empty;
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color bg = (CardBackground != Color.Empty && CardBackground != Color.Transparent) ? CardBackground : ThemePalette.ColorBgCard;
            Color border = (BorderColor != Color.Empty && BorderColor != Color.Transparent) ? BorderColor : ThemePalette.ColorBorderCard;
            ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, this.Width - 1, this.Height - 3), bg, border, ThemePalette.ColorShadow, 8);
        }
    }

    public class ModernTextBox : UserControl
    {
        private TextBox innerBox;
        private Label lblClear;
        private Label lblPlaceholder;
        public Color BorderColor { get; set; }
        public Color FocusBorderColor { get; set; }
        public bool AutoFormatDeskId { get; set; }

        public event EventHandler TextChangedCustom;

        public override string Text
        {
            get { return innerBox != null ? innerBox.Text : ""; }
            set { if (innerBox != null) innerBox.Text = value; }
        }

        public string PlaceholderText
        {
            get { return lblPlaceholder != null ? lblPlaceholder.Text : ""; }
            set { if (lblPlaceholder != null) { lblPlaceholder.Text = value; UpdatePlaceholder(); } }
        }

        public Font InputFont
        {
            get { return innerBox != null ? innerBox.Font : this.Font; }
            set { if (innerBox != null) innerBox.Font = value; }
        }

        public ModernTextBox()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Size = new Size(260, 42);
            this.BackColor = Color.Transparent;
            this.BorderColor = ThemePalette.ColorBorderCard;
            this.FocusBorderColor = ThemePalette.ColorAccentSky;
            this.AutoFormatDeskId = true;

            innerBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = ThemePalette.ColorBgInput,
                ForeColor = ThemePalette.ColorTextPrimary,
                Font = new Font("Consolas", 14f, FontStyle.Bold),
                Location = new Point(12, 11),
                Width = this.Width - 44
            };

            lblPlaceholder = new Label
            {
                Text = "XXX-XXX-XXX",
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Consolas", 13f),
                BackColor = Color.Transparent,
                Location = new Point(12, 10),
                AutoSize = true,
                Cursor = Cursors.IBeam
            };
            lblPlaceholder.Click += (s, e) => innerBox.Focus();

            lblClear = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.Transparent,
                Size = new Size(20, 20),
                Location = new Point(this.Width - 28, 11),
                Cursor = Cursors.Hand,
                Visible = false
            };
            lblClear.MouseEnter += (s, e) => lblClear.ForeColor = Color.FromArgb(239, 68, 68);
            lblClear.MouseLeave += (s, e) => lblClear.ForeColor = Color.FromArgb(148, 163, 184);
            lblClear.Click += (s, e) => { innerBox.Text = ""; innerBox.Focus(); };

            innerBox.GotFocus += (s, e) => { this.Invalidate(); UpdatePlaceholder(); };
            innerBox.LostFocus += (s, e) => { this.Invalidate(); UpdatePlaceholder(); };

            bool isFormatting = false;
            innerBox.TextChanged += (s, e) => {
                UpdatePlaceholder();
                lblClear.Visible = !string.IsNullOrEmpty(innerBox.Text);
                if (AutoFormatDeskId && !isFormatting)
                {
                    isFormatting = true;
                    string digits = Regex.Replace(innerBox.Text, @"[^\d]", "");
                    if (digits.Length > 9) digits = digits.Substring(0, 9);
                    string formatted = digits;
                    if (digits.Length > 6)
                        formatted = string.Format("{0}-{1}-{2}", digits.Substring(0, 3), digits.Substring(3, 3), digits.Substring(6));
                    else if (digits.Length > 3)
                        formatted = string.Format("{0}-{1}", digits.Substring(0, 3), digits.Substring(3));
                    if (formatted != innerBox.Text)
                    {
                        innerBox.Text = formatted;
                        innerBox.SelectionStart = innerBox.Text.Length;
                    }
                    isFormatting = false;
                }
                this.Invalidate();
                if (TextChangedCustom != null) TextChangedCustom(this, EventArgs.Empty);
            };

            this.Controls.Add(lblClear);
            this.Controls.Add(lblPlaceholder);
            this.Controls.Add(innerBox);

            this.Resize += (s, e) => {
                innerBox.Width = this.Width - 44;
                lblClear.Location = new Point(this.Width - 28, 11);
            };
        }

        public void ApplyTheme()
        {
            if (innerBox != null)
            {
                innerBox.BackColor = ThemePalette.ColorBgInput;
                innerBox.ForeColor = ThemePalette.ColorTextPrimary;
            }
            this.BorderColor = ThemePalette.ColorBorderCard;
            this.FocusBorderColor = ThemePalette.ColorAccentSky;
            this.Invalidate();
        }

        private void UpdatePlaceholder()
        {
            if (lblPlaceholder != null)
                lblPlaceholder.Visible = string.IsNullOrEmpty(innerBox.Text) && !innerBox.Focused;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color curBorder = innerBox.Focused ? (FocusBorderColor != Color.Empty ? FocusBorderColor : ThemePalette.ColorAccentSky) : (BorderColor != Color.Empty ? BorderColor : ThemePalette.ColorBorderCard);
            string digits = Regex.Replace(innerBox.Text, @"[^\d]", "");
            if (digits.Length == 9) curBorder = Color.FromArgb(16, 185, 129);

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = ThemePalette.CreateRoundedPath(rect, 6))
            {
                using (SolidBrush b = new SolidBrush(innerBox.BackColor))
                {
                    g.FillPath(b, path);
                }
                using (Pen pen = new Pen(curBorder, 1.4f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }
    }

    public class ModernSwitch : Control
    {
        private bool isChecked = false;
        public bool Checked
        {
            get { return isChecked; }
            set { if (isChecked != value) { isChecked = value; this.Invalidate(); if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty); } }
        }

        public string LabelText { get; set; }
        public Color OnColor { get; set; }
        public Color OffColor { get; set; }

        public event EventHandler CheckedChanged;

        public ModernSwitch()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Size = new Size(240, 28);
            this.Cursor = Cursors.Hand;
            this.OnColor = Color.FromArgb(16, 185, 129);
            this.OffColor = Color.FromArgb(51, 65, 85);
            this.LabelText = "Switch Option";
            this.Font = new Font("Segoe UI", 8.8f);
            this.ForeColor = Color.FromArgb(203, 213, 225);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.Checked = !this.Checked;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int pillW = 44;
            int pillH = 22;
            int pillY = (this.Height - pillH) / 2;

            Color trackColor = isChecked ? OnColor : OffColor;
            using (SolidBrush trackBrush = new SolidBrush(trackColor))
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(0, pillY, pillH, pillH, 90, 180);
                    path.AddArc(pillW - pillH, pillY, pillH, pillH, 270, 180);
                    path.CloseFigure();
                    g.FillPath(trackBrush, path);
                }
            }

            int knobDiameter = pillH - 4;
            int knobX = isChecked ? (pillW - knobDiameter - 2) : 2;
            int knobY = pillY + 2;

            using (SolidBrush knobBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(knobBrush, knobX, knobY, knobDiameter, knobDiameter);
            }

            if (!string.IsNullOrEmpty(LabelText))
            {
                using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
                {
                    g.DrawString(LabelText, this.Font, textBrush, pillW + 10, (this.Height - this.Font.Height) / 2);
                }
            }
        }
    }

    public class ModernSegmentedToggle : Control
    {
        private string[] options = new string[] { "Option A", "Option B" };
        private int selectedIndex = 0;

        public string[] Options
        {
            get { return options; }
            set { options = value ?? new string[0]; this.Invalidate(); }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value && value >= 0 && value < options.Length)
                {
                    selectedIndex = value;
                    this.Invalidate();
                    if (SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler SelectedIndexChanged;

        public ModernSegmentedToggle()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Size = new Size(240, 32);
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (options.Length == 0) return;
            int segmentW = this.Width / options.Length;
            int clicked = e.X / segmentW;
            if (clicked >= 0 && clicked < options.Length)
            {
                this.SelectedIndex = clicked;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(10, 16, 26)))
            using (Pen borderPen = new Pen(Color.FromArgb(40, 53, 76), 1))
            {
                g.FillRectangle(bg, 0, 0, this.Width - 1, this.Height - 1);
                g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
            }

            if (options.Length == 0) return;
            int segmentW = this.Width / options.Length;

            int activeX = selectedIndex * segmentW;
            Rectangle activeRect = new Rectangle(activeX + 2, 2, segmentW - 4, this.Height - 4);
            using (SolidBrush activeBrush = new SolidBrush(Color.FromArgb(2, 132, 199)))
            {
                g.FillRectangle(activeBrush, activeRect);
            }

            for (int i = 0; i < options.Length; i++)
            {
                Color textCol = (i == selectedIndex) ? Color.White : Color.FromArgb(148, 163, 184);
                using (SolidBrush textBrush = new SolidBrush(textCol))
                {
                    SizeF sz = g.MeasureString(options[i], this.Font);
                    float tx = (i * segmentW) + (segmentW - sz.Width) / 2f;
                    float ty = (this.Height - sz.Height) / 2f;
                    g.DrawString(options[i], this.Font, textBrush, tx, ty);
                }
            }
        }
    }

    public class ModernCardFlipper : Panel
    {
        private Panel frontPanel;
        private Panel backPanel;
        private ModernButton btnFlip;
        private bool isFlipped = false;

        public Panel FrontView { get { return frontPanel; } }
        public Panel BackView { get { return backPanel; } }
        public bool IsFlipped { get { return isFlipped; } }

        public event EventHandler Flipped;

        public ModernCardFlipper()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Size = new Size(820, 165);
            this.BackColor = Color.Transparent;

            frontPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            backPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            btnFlip = new ModernButton
            {
                Text = "⮌ Telemetry / Keys",
                Size = new Size(140, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = ThemePalette.IsDarkTheme ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240),
                HoverColor = ThemePalette.IsDarkTheme ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225),
                TextColor = ThemePalette.ColorAccentSky,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold)
            };
            btnFlip.Location = new Point(this.Width - 156, 12);
            btnFlip.Click += (s, e) => Flip();

            this.Controls.Add(btnFlip);
            this.Controls.Add(frontPanel);
            this.Controls.Add(backPanel);
            btnFlip.BringToFront();

            this.Paint += (s, e) => {
                Color borderC = isFlipped ? ThemePalette.ColorAccentSky : ThemePalette.ColorBorderCard;
                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, this.Width - 1, this.Height - 3), ThemePalette.ColorBgCard, borderC, ThemePalette.ColorShadow, 10);
            };

            this.Resize += (s, e) => {
                btnFlip.Location = new Point(this.Width - 156, 12);
            };
        }

        public void Flip()
        {
            isFlipped = !isFlipped;
            frontPanel.Visible = !isFlipped;
            backPanel.Visible = isFlipped;
            btnFlip.Text = isFlipped ? "⮌ Return to Connect" : "⮌ Telemetry / Keys";
            this.Invalidate();
            if (Flipped != null) Flipped(this, EventArgs.Empty);
        }
    }

    public class ModernListBoxItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Status { get; set; }
        public Color StatusColor { get; set; }
        public object Tag { get; set; }
    }

    public class ModernListBox : UserControl
    {
        private ListBox innerList;
        private List<ModernListBoxItem> items = new List<ModernListBoxItem>();

        public List<ModernListBoxItem> CustomItems { get { return items; } }
        public ModernListBoxItem SelectedItem { get { return innerList.SelectedIndex >= 0 && innerList.SelectedIndex < items.Count ? items[innerList.SelectedIndex] : null; } }

        public event EventHandler SelectedIndexChanged;

        public ModernListBox()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.BackColor = Color.FromArgb(10, 16, 26);

            innerList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 16, 26),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawVariable,
                ItemHeight = 44
            };

            innerList.MeasureItem += (s, e) => e.ItemHeight = 44;
            innerList.DrawItem += InnerList_DrawItem;
            innerList.SelectedIndexChanged += (s, e) => {
                if (SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty);
            };

            this.Controls.Add(innerList);
        }

        public void RefreshList()
        {
            innerList.Items.Clear();
            foreach (var it in items) innerList.Items.Add(it);
        }

        private void InnerList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= items.Count) return;
            ModernListBoxItem it = items[e.Index];
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isSelected ? ThemePalette.ColorPrimary : (e.Index % 2 == 0 ? ThemePalette.ColorBgCard : ThemePalette.ColorBgCardElevated);

            using (SolidBrush b = new SolidBrush(bg))
            {
                g.FillRectangle(b, e.Bounds);
            }

            g.DrawString("🖥️", new Font("Segoe UI Emoji", 11f), Brushes.White, e.Bounds.X + 8, e.Bounds.Y + 12);

            using (SolidBrush tb = new SolidBrush(isSelected ? Color.White : ThemePalette.ColorTextPrimary))
            {
                g.DrawString(it.Title ?? "", new Font("Segoe UI", 9f, FontStyle.Bold), tb, e.Bounds.X + 36, e.Bounds.Y + 6);
            }

            using (SolidBrush sb = new SolidBrush(isSelected ? Color.FromArgb(226, 232, 240) : ThemePalette.ColorTextSecondary))
            {
                g.DrawString(it.Subtitle ?? "", new Font("Segoe UI", 7.8f), sb, e.Bounds.X + 36, e.Bounds.Y + 24);
            }

            if (!string.IsNullOrEmpty(it.Status))
            {
                Color pillC = it.StatusColor != Color.Empty ? it.StatusColor : (it.Status == "Online" ? Color.FromArgb(16, 185, 129) : Color.FromArgb(100, 116, 139));
                int pillW = 60;
                int pillH = 18;
                int pillX = e.Bounds.Right - pillW - 10;
                int pillY = e.Bounds.Y + 13;

                using (SolidBrush pb = new SolidBrush(Color.FromArgb(40, pillC.R, pillC.G, pillC.B)))
                using (Pen pp = new Pen(pillC, 1f))
                {
                    g.FillRectangle(pb, pillX, pillY, pillW, pillH);
                    g.DrawRectangle(pp, pillX, pillY, pillW, pillH);
                }
                using (SolidBrush pt = new SolidBrush(pillC))
                {
                    SizeF sz = g.MeasureString(it.Status, new Font("Segoe UI", 7.2f, FontStyle.Bold));
                    g.DrawString(it.Status, new Font("Segoe UI", 7.2f, FontStyle.Bold), pt, pillX + (pillW - sz.Width) / 2f, pillY + (pillH - sz.Height) / 2f);
                }
            }
        }
    }

    public class ModernMultiOptionDialog : Form
    {
        public int SelectedOptionIndex { get; private set; }
        public string SelectedOptionText { get; private set; }

        public ModernMultiOptionDialog(string title, string message, string[] options)
        {
            this.SelectedOptionIndex = -1;
            this.SelectedOptionText = "";

            this.Text = title;
            this.Size = new Size(540, 240);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemePalette.ColorBgWorkspace;
            this.ForeColor = ThemePalette.ColorTextPrimary;
            this.Font = new Font("Segoe UI", 9f);
            this.ShowInTaskbar = false;

            Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

            Label lblHead = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorPrimary,
                Location = new Point(24, 18),
                AutoSize = true
            };

            Label lblMsg = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 9.2f),
                ForeColor = ThemePalette.ColorTextSecondary,
                Location = new Point(24, 52),
                Size = new Size(476, 75)
            };

            FlowLayoutPanel btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 16, 10)
            };

            for (int i = 0; i < options.Length; i++)
            {
                int idx = i;
                string opt = options[i];
                ModernButton btn = new ModernButton
                {
                    Text = opt,
                    Size = new Size(Math.Max(120, opt.Length * 9 + 24), 36),
                    Margin = new Padding(8, 0, 0, 0),
                    NormalColor = (idx == 0) ? Color.FromArgb(16, 185, 129) : Color.FromArgb(30, 41, 59),
                    HoverColor = (idx == 0) ? Color.FromArgb(5, 150, 105) : Color.FromArgb(51, 65, 85),
                    TextColor = Color.White
                };
                btn.Click += (s, e) => {
                    SelectedOptionIndex = idx;
                    SelectedOptionText = opt;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                btnFlow.Controls.Add(btn);
            }

            pnl.Controls.Add(lblHead);
            pnl.Controls.Add(lblMsg);
            this.Controls.Add(pnl);
            this.Controls.Add(btnFlow);
        }

        public static int Show(IWin32Window owner, string title, string message, string[] options)
        {
            using (ModernMultiOptionDialog dlg = new ModernMultiOptionDialog(title, message, options))
            {
                dlg.ShowDialog(owner);
                return dlg.SelectedOptionIndex;
            }
        }
    }

    public class ModernNotificationToast : Form
    {
        private static ModernNotificationToast currentToast = null;
        private System.Windows.Forms.Timer animTimer;
        private System.Windows.Forms.Timer closeTimer;
        private int targetY;
        private int currentY;
        private bool isSlidingOut = false;
        private string toastTitle;
        private string toastMessage;
        private Color accentColor;

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return cp;
            }
        }

        public ModernNotificationToast(string title, string message, string type = "info")
        {
            this.toastTitle = title;
            this.toastMessage = message;
            this.accentColor = (type == "success") ? Color.FromArgb(16, 185, 129) :
                               (type == "error") ? Color.FromArgb(239, 68, 68) :
                               (type == "warning") ? Color.FromArgb(245, 158, 11) : Color.FromArgb(56, 189, 248);

            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = ThemePalette.ColorBgCard;

            Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
            int x = workArea.Right - this.Width - 16;
            targetY = workArea.Bottom - this.Height - 16;
            currentY = workArea.Bottom + 10;
            this.Location = new Point(x, currentY);

            this.Paint += (s, e) => {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                ThemePalette.DrawFloatingCard(g, r, ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 8);

                using (SolidBrush ab = new SolidBrush(accentColor))
                {
                    g.FillRectangle(ab, 0, 4, 4, this.Height - 8);
                }

                using (SolidBrush tb = new SolidBrush(ThemePalette.ColorTextPrimary))
                {
                    g.DrawString(toastTitle, new Font("Segoe UI", 9.5f, FontStyle.Bold), tb, 14, 10);
                }

                using (SolidBrush mb = new SolidBrush(ThemePalette.ColorTextSecondary))
                {
                    g.DrawString(toastMessage, new Font("Segoe UI", 8.2f), mb, 14, 34);
                }
            };

            animTimer = new System.Windows.Forms.Timer { Interval = 15 };
            animTimer.Tick += (s, e) => {
                if (!isSlidingOut)
                {
                    currentY -= Math.Max(4, (currentY - targetY) / 4);
                    if (currentY <= targetY)
                    {
                        currentY = targetY;
                        animTimer.Stop();
                        closeTimer.Start();
                    }
                }
                else
                {
                    currentY += Math.Max(4, (workArea.Bottom + 10 - currentY) / 4);
                    if (currentY >= workArea.Bottom)
                    {
                        animTimer.Stop();
                        this.Close();
                    }
                }
                this.Location = new Point(x, currentY);
            };

            closeTimer = new System.Windows.Forms.Timer { Interval = 3500 };
            closeTimer.Tick += (s, e) => {
                closeTimer.Stop();
                isSlidingOut = true;
                animTimer.Start();
            };
        }

        public static void ShowToast(string title, string message, string type = "info")
        {
            try
            {
                if (currentToast != null && !currentToast.IsDisposed)
                {
                    currentToast.Close();
                    currentToast.Dispose();
                }
                currentToast = new ModernNotificationToast(title, message, type);
                currentToast.Show();
                currentToast.animTimer.Start();
            }
            catch { }
        }
    }

    // ==========================================================================
    // JDROID-X INCOMING SESSION APPROVAL DIALOG (FLOATING SYSTEM POPUP)
    // ==========================================================================

}
