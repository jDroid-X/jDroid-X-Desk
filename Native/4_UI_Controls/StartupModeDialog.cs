using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace JDroidXDesk.Native
{
    public class StartupModeDialog : Form
    {
        public string SelectedMode { get; private set; }
        public bool RememberChoice { get; private set; }

        private Timer countdownTimer;
        private int secondsRemaining = 3;
        private Label lblCountdown;
        private CheckBox chkRemember;
        private ModernButton btnMini;
        private ModernButton btnDesk;

        public StartupModeDialog()
        {
            this.SelectedMode = "mini"; // Default selection
            this.RememberChoice = false;

            this.Text = "jDroid-X-Desk Launcher";
            this.Size = new Size(520, 360);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemePalette.ColorBgWorkspace;
            this.ForeColor = ThemePalette.ColorTextPrimary;
            this.Font = new Font("Segoe UI", 9f);
            this.ShowIcon = false;

            Panel root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

            Label lblTitle = new Label
            {
                Text = "🚀 Launch jDroid-X-Desk",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorPrimary,
                Location = new Point(20, 16),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Choose your preferred workspace mode. Mini Desk is selected by default.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemePalette.ColorTextSecondary,
                Location = new Point(22, 46),
                AutoSize = true
            };

            // Option 1: Mini Desk (Default)
            ModernCardPanel cardMini = new ModernCardPanel
            {
                Location = new Point(20, 78),
                Size = new Size(464, 82),
                BorderColor = ThemePalette.ColorAccentSky,
                Cursor = Cursors.Hand
            };
            Label lblMiniTitle = new Label
            {
                Text = "📱 Mini Desk (Compact Quick-Connect)  [DEFAULT]",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorAccentSky,
                Location = new Point(14, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Label lblMiniDesc = new Label
            {
                Text = "Minimal floating widget with Self Desk Code, quick connect, and essential tools.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemePalette.ColorTextSecondary,
                Location = new Point(16, 34),
                Size = new Size(330, 36),
                BackColor = Color.Transparent
            };
            btnMini = new ModernButton
            {
                Text = "Launch Mini ➔",
                Size = new Size(110, 36),
                Location = new Point(340, 22),
                NormalColor = ThemePalette.ColorAccentSky,
                HoverColor = Color.FromArgb(2, 132, 199),
                TextColor = Color.White
            };
            btnMini.Click += (s, e) => SelectModeAndClose("mini");
            cardMini.Click += (s, e) => SelectModeAndClose("mini");
            lblMiniTitle.Click += (s, e) => SelectModeAndClose("mini");
            lblMiniDesc.Click += (s, e) => SelectModeAndClose("mini");

            cardMini.Controls.Add(lblMiniTitle);
            cardMini.Controls.Add(lblMiniDesc);
            cardMini.Controls.Add(btnMini);

            // Option 2: Full Desk Console
            ModernCardPanel cardDesk = new ModernCardPanel
            {
                Location = new Point(20, 168),
                Size = new Size(464, 82),
                BorderColor = ThemePalette.ColorBorderCard,
                Cursor = Cursors.Hand
            };
            Label lblDeskTitle = new Label
            {
                Text = "🖥️ Desk Console (Full Studio & Admin)",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorTextPrimary,
                Location = new Point(14, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Label lblDeskDesc = new Label
            {
                Text = "Full multi-tab workspace with remote desktop viewport, address book, logs & engines.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemePalette.ColorTextSecondary,
                Location = new Point(16, 34),
                Size = new Size(330, 36),
                BackColor = Color.Transparent
            };
            btnDesk = new ModernButton
            {
                Text = "Full Console",
                Size = new Size(110, 36),
                Location = new Point(340, 22),
                NormalColor = Color.FromArgb(51, 65, 85),
                HoverColor = Color.FromArgb(71, 85, 105),
                TextColor = Color.White
            };
            btnDesk.Click += (s, e) => SelectModeAndClose("desk");
            cardDesk.Click += (s, e) => SelectModeAndClose("desk");
            lblDeskTitle.Click += (s, e) => SelectModeAndClose("desk");
            lblDeskDesc.Click += (s, e) => SelectModeAndClose("desk");

            cardDesk.Controls.Add(lblDeskTitle);
            cardDesk.Controls.Add(lblDeskDesc);
            cardDesk.Controls.Add(btnDesk);

            // Footer controls
            chkRemember = new CheckBox
            {
                Text = "Remember this choice for next time",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemePalette.ColorTextSecondary,
                Location = new Point(22, 264),
                AutoSize = true
            };

            lblCountdown = new Label
            {
                Text = "Auto-starting Mini Desk in 3s...",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = ThemePalette.ColorAccentSky,
                Location = new Point(290, 264),
                AutoSize = true
            };

            root.Controls.Add(lblTitle);
            root.Controls.Add(lblSubtitle);
            root.Controls.Add(cardMini);
            root.Controls.Add(cardDesk);
            root.Controls.Add(chkRemember);
            root.Controls.Add(lblCountdown);

            this.Controls.Add(root);

            // 3-second auto countdown
            countdownTimer = new Timer { Interval = 1000 };
            countdownTimer.Tick += (s, e) => {
                secondsRemaining--;
                if (secondsRemaining > 0)
                {
                    lblCountdown.Text = string.Format("Auto-starting Mini Desk in {0}s...", secondsRemaining);
                }
                else
                {
                    countdownTimer.Stop();
                    SelectModeAndClose("mini");
                }
            };
            countdownTimer.Start();

            // Support Keyboard Enter/Esc
            this.KeyPreview = true;
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    SelectModeAndClose("mini");
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    SelectModeAndClose("mini");
                }
            };
        }

        private void SelectModeAndClose(string mode)
        {
            if (countdownTimer != null)
            {
                countdownTimer.Stop();
                countdownTimer.Dispose();
                countdownTimer = null;
            }
            this.SelectedMode = mode;
            this.RememberChoice = (chkRemember != null && chkRemember.Checked);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public static string PromptStartupMode(IWin32Window owner, out bool remember)
        {
            using (StartupModeDialog dlg = new StartupModeDialog())
            {
                dlg.ShowDialog(owner);
                remember = dlg.RememberChoice;
                return dlg.SelectedMode;
            }
        }
    }
}
