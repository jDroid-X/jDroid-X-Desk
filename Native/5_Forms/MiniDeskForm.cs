using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace JDroidXDesk.Native
{
    public class MiniDeskForm : Form
    {
        private MainForm mainForm;
        private ToolTip toolTip;

        // UI Controls
        private Panel headerPanel;
        private Label lblHeaderTitle;
        private Label lblStatusDot;
        private Button btnHamburger;
        private ContextMenuStrip menuHamburger;
        private Button btnMaxToFull;

        private ModernCardPanel cardThisDesk;
        private Label lblMyDeskId;
        private ModernButton btnCopyMyId;
        private Label lblMyDeskStatus;

        private ModernCardPanel cardRemoteDesk;
        private ModernTextBox txtRemoteDeskCode;
        private ModernButton btnConnect;

        private Panel pnlIconToolbar;
        private Button btnIconAdd;
        private Button btnIconStart;
        private Button btnIconStop;
        private Button btnIconDetails;
        private Button btnIconFullScreen;
        private Button btnIconClose;

        private Panel footerStatusBar;
        private Label lblFooterStatus;
        private Timer refreshTimer;

        public MiniDeskForm(MainForm main)
        {
            this.mainForm = main;
            this.toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 300,
                ReshowDelay = 200,
                ShowAlways = true
            };

            this.Text = "jDroid-X-Desk Mini | Remote Connection";
            this.Size = new Size(500, 360);
            this.MinimumSize = new Size(460, 340);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = ThemePalette.ColorBgWorkspace;
            this.ForeColor = ThemePalette.ColorTextPrimary;
            this.Font = new Font("Segoe UI", 9f);

            BuildMiniUi();

            // Hook state refresh timer
            refreshTimer = new Timer { Interval = 1000 };
            refreshTimer.Tick += (s, e) => UpdateLiveState();
            refreshTimer.Start();

            this.FormClosing += (s, e) => {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    // If session active, prompt
                    if (mainForm != null && mainForm.IsSessionActive)
                    {
                        DialogResult dr = MessageBox.Show(this, "A remote connection is currently active. Do you want to disconnect and close?", "Active Session", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr != DialogResult.Yes)
                        {
                            e.Cancel = true;
                            return;
                        }
                        mainForm.DisconnectActiveSession();
                    }
                    // Hide to background tray instead of killing daemon
                    e.Cancel = true;
                    this.Hide();
                }
            };
        }

        private void BuildMiniUi()
        {
            this.Controls.Clear();

            // 1. TOP HEADER BAR (With Left Hamburger Menu Button)
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = ThemePalette.ColorBgSidebarHeader,
                Padding = new Padding(8, 4, 8, 4)
            };

            // Top-left Menu [☰] Button
            btnHamburger = new Button
            {
                Text = "☰",
                Size = new Size(36, 34),
                Location = new Point(6, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorTextPrimary,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnHamburger.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(btnHamburger, "Options & Connect Details Menu");

            // Build Drop-down Context Menu
            menuHamburger = new ContextMenuStrip();
            menuHamburger.Items.Add("📋 Connect Detail (Full Desk Console)", null, (s, e) => OpenFullDeskDetails());
            menuHamburger.Items.Add("🗄️ Address Book / Fleet", null, (s, e) => { mainForm.ShowFullConsole(); mainForm.SwitchToTab("Fleet"); });
            menuHamburger.Items.Add("⚙️ Settings Studio", null, (s, e) => { mainForm.ShowFullConsole(); mainForm.SwitchToTab("Settings"); });
            menuHamburger.Items.Add("🔄 Check for Updates...", null, (s, e) => mainForm.CheckForUpdates(false));
            menuHamburger.Items.Add("📦 Install to User Folder...", null, (s, e) => mainForm.InstallToUserFolder());
            menuHamburger.Items.Add(new ToolStripSeparator());
            menuHamburger.Items.Add("❌ Exit Application", null, (s, e) => this.Close());

            btnHamburger.Click += (s, e) => {
                menuHamburger.Show(btnHamburger, new Point(0, btnHamburger.Height));
            };

            lblStatusDot = new Label
            {
                Text = "🟢",
                Font = new Font("Segoe UI Emoji", 10f),
                Location = new Point(48, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblHeaderTitle = new Label
            {
                Text = "jDroid-X-Desk Mini",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorTextPrimary,
                Location = new Point(72, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            btnMaxToFull = new Button
            {
                Text = "🗖 Details",
                Size = new Size(74, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 110, 8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorTextSecondary,
                Cursor = Cursors.Hand
            };
            btnMaxToFull.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(btnMaxToFull, "Open Full Desk Console (Connect Detail)");
            btnMaxToFull.Click += (s, e) => OpenFullDeskDetails();

            headerPanel.Controls.Add(btnHamburger);
            headerPanel.Controls.Add(lblStatusDot);
            headerPanel.Controls.Add(lblHeaderTitle);
            headerPanel.Controls.Add(btnMaxToFull);

            // 2. BOTTOM STATUS BAR
            footerStatusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = ThemePalette.ColorBgSidebarHeader,
                Padding = new Padding(10, 5, 10, 5)
            };
            lblFooterStatus = new Label
            {
                Text = "🟢 Ready | Local Port: 8088 | Direct & Relay Active",
                Font = new Font("Segoe UI", 8.2f),
                ForeColor = ThemePalette.ColorTextSecondary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            footerStatusBar.Controls.Add(lblFooterStatus);

            // 3. CENTER CONTENT
            Panel pnlCenter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 10, 14, 6),
                AutoScroll = true
            };

            // CARD 1: THIS DESK
            cardThisDesk = new ModernCardPanel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BorderColor = ThemePalette.ColorBorderCard,
                Margin = new Padding(0, 0, 0, 10)
            };
            Label lblThisTitle = new Label
            {
                Text = "THIS DESK CODE (Share to allow remote control):",
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorTextSecondary,
                Location = new Point(12, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            string myId = (mainForm != null && mainForm.CurrentHostDesktop != null) ? mainForm.CurrentHostDesktop.RemoteId : "--- --- ---";
            lblMyDeskId = new Label
            {
                Text = myId,
                Font = new Font("Consolas", 15f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorAccentSky,
                Location = new Point(12, 30),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            toolTip.SetToolTip(lblMyDeskId, "Click to copy your Desk ID");
            lblMyDeskId.Click += (s, e) => CopyDeskIdToClipboard();

            btnCopyMyId = new ModernButton
            {
                Text = "📋 Copy",
                Size = new Size(74, 30),
                Location = new Point(240, 28),
                NormalColor = Color.FromArgb(51, 65, 85),
                HoverColor = Color.FromArgb(71, 85, 105),
                TextColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            toolTip.SetToolTip(btnCopyMyId, "Copy your Desk ID to clipboard");
            btnCopyMyId.Click += (s, e) => CopyDeskIdToClipboard();

            lblMyDeskStatus = new Label
            {
                Text = "🟢 Active & Ready for Incoming Connections",
                Font = new Font("Segoe UI", 7.8f),
                ForeColor = ThemePalette.ColorAccentEmerald,
                Location = new Point(12, 60),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            cardThisDesk.Controls.Add(lblThisTitle);
            cardThisDesk.Controls.Add(lblMyDeskId);
            cardThisDesk.Controls.Add(btnCopyMyId);
            cardThisDesk.Controls.Add(lblMyDeskStatus);

            // CARD 2: REMOTE DESK
            cardRemoteDesk = new ModernCardPanel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BorderColor = ThemePalette.ColorAccentSky,
                Margin = new Padding(0, 8, 0, 10)
            };
            Label lblRemoteTitle = new Label
            {
                Text = "REMOTE DESK (Enter partner Desk ID to connect):",
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                ForeColor = ThemePalette.ColorTextSecondary,
                Location = new Point(12, 8),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtRemoteDeskCode = new ModernTextBox
            {
                Location = new Point(12, 28),
                Size = new Size(240, 42),
                PlaceholderText = "XXX-XXX-XXX",
                AutoFormatDeskId = true
            };
            txtRemoteDeskCode.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) TriggerConnect();
            };

            btnConnect = new ModernButton
            {
                Text = "⚡ Connect",
                Size = new Size(116, 42),
                Location = new Point(262, 28),
                NormalColor = ThemePalette.ColorAccentEmerald,
                HoverColor = Color.FromArgb(5, 150, 105),
                TextColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            toolTip.SetToolTip(btnConnect, "Initiate Remote Desktop Connection");
            btnConnect.Click += (s, e) => TriggerConnect();

            cardRemoteDesk.Controls.Add(lblRemoteTitle);
            cardRemoteDesk.Controls.Add(txtRemoteDeskCode);
            cardRemoteDesk.Controls.Add(btnConnect);

            // 4. MANDATORY ICON TOOLBAR (Add, Start, Stop, Details, Full Screen, Close)
            pnlIconToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.Transparent,
                Padding = new Padding(4, 6, 4, 6)
            };

            FlowLayoutPanel flowIcons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            Func<string, string, Action, Button> makeIconButton = (iconText, tip, action) => {
                Button btn = new Button
                {
                    Text = iconText,
                    Size = new Size(68, 40),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Emoji", 11f),
                    BackColor = ThemePalette.ColorBgCard,
                    ForeColor = ThemePalette.ColorTextPrimary,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(4, 0, 4, 0)
                };
                btn.FlatAppearance.BorderColor = ThemePalette.ColorBorderCard;
                btn.FlatAppearance.BorderSize = 1;
                toolTip.SetToolTip(btn, tip);
                btn.Click += (s, e) => action();
                return btn;
            };

            // Mandatory Options in Icon format:
            btnIconAdd = makeIconButton("➕ Add", "Add remote Desk ID to Contacts / Address Book", () => ActionAddContact());
            btnIconStart = makeIconButton("▶️ Start", "Start / Resume Host Screen Broadcasting Daemon", () => ActionStartHosting());
            btnIconStop = makeIconButton("⏹️ Stop", "Stop Active Session or Disconnect Remote Link", () => ActionStopSession());
            btnIconDetails = makeIconButton("ℹ️ Details", "Connect Detail: Open Full Detailed Workspace", () => OpenFullDeskDetails());
            btnIconFullScreen = makeIconButton("⛶ Full", "Toggle Full Screen Remote Viewport", () => ActionToggleFullScreen());
            btnIconClose = makeIconButton("✕ Close", "Close or Minimize to System Tray", () => this.Close());

            flowIcons.Controls.Add(btnIconAdd);
            flowIcons.Controls.Add(btnIconStart);
            flowIcons.Controls.Add(btnIconStop);
            flowIcons.Controls.Add(btnIconDetails);
            flowIcons.Controls.Add(btnIconFullScreen);
            flowIcons.Controls.Add(btnIconClose);
            pnlIconToolbar.Controls.Add(flowIcons);

            // Assemble into Center
            pnlCenter.Controls.Add(pnlIconToolbar);
            pnlCenter.Controls.Add(cardRemoteDesk);
            pnlCenter.Controls.Add(cardThisDesk);

            this.Controls.Add(pnlCenter);
            this.Controls.Add(footerStatusBar);
            this.Controls.Add(headerPanel);
        }

        private void CopyDeskIdToClipboard()
        {
            try
            {
                string id = lblMyDeskId.Text.Replace(" ", "").Trim();
                if (!string.IsNullOrEmpty(id) && id != "---")
                {
                    Clipboard.SetText(id);
                    MessageBox.Show(this, "Desk ID copied to clipboard: " + id, "Desk ID Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch { }
        }

        private void TriggerConnect()
        {
            string target = txtRemoteDeskCode.Text.Trim();
            if (string.IsNullOrEmpty(target))
            {
                MessageBox.Show(this, "Please enter a valid remote Desk ID or IP address to connect.", "Enter Remote Desk Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRemoteDeskCode.Focus();
                return;
            }

            if (mainForm != null)
            {
                lblFooterStatus.Text = "🟡 Connecting to " + target + "...";
                btnConnect.Text = "Connecting...";
                btnConnect.NormalColor = Color.FromArgb(245, 158, 11);

                mainForm.InitiateRemoteConnection(target);
            }
        }

        private void ActionAddContact()
        {
            string target = txtRemoteDeskCode.Text.Trim();
            if (string.IsNullOrEmpty(target))
            {
                MessageBox.Show(this, "Please enter a Desk ID above to add to contacts.", "Add Contact", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (mainForm != null)
            {
                mainForm.QuickAddContact(target);
                MessageBox.Show(this, "Desk ID " + target + " has been added to your Address Book / Contacts.", "Contact Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ActionStartHosting()
        {
            if (mainForm != null)
            {
                mainForm.StartHostDaemon();
                lblMyDeskStatus.Text = "🟢 Host Broadcast Active & Streaming";
                lblMyDeskStatus.ForeColor = ThemePalette.ColorAccentEmerald;
                MessageBox.Show(this, "Host screening daemon is active and listening for authorized connections.", "Host Streaming Active", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ActionStopSession()
        {
            if (mainForm != null)
            {
                if (mainForm.IsSessionActive)
                {
                    mainForm.DisconnectActiveSession();
                    lblFooterStatus.Text = "🟢 Session Disconnected | Ready";
                    btnConnect.Text = "⚡ Connect";
                    btnConnect.NormalColor = ThemePalette.ColorAccentEmerald;
                    MessageBox.Show(this, "Active remote session has been disconnected.", "Session Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, "No active session is currently connected.", "Idle State", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ActionToggleFullScreen()
        {
            if (mainForm != null)
            {
                mainForm.ToggleFullScreen();
            }
        }

        public void OpenFullDeskDetails()
        {
            if (mainForm != null)
            {
                this.Hide();
                mainForm.ShowFullConsole();
            }
        }

        public void UpdateLiveState()
        {
            try
            {
                if (mainForm == null) return;

                if (mainForm.CurrentHostDesktop != null && lblMyDeskId.Text != mainForm.CurrentHostDesktop.RemoteId)
                {
                    lblMyDeskId.Text = mainForm.CurrentHostDesktop.RemoteId;
                }

                if (mainForm.IsSessionActive)
                {
                    lblStatusDot.Text = "🔴";
                    btnConnect.Text = "Disconnect";
                    btnConnect.NormalColor = ThemePalette.ColorAccentRose;
                    lblFooterStatus.Text = "🔴 Connected | Active Stream | 60 FPS";
                }
                else
                {
                    lblStatusDot.Text = "🟢";
                    if (btnConnect.Text == "Disconnect")
                    {
                        btnConnect.Text = "⚡ Connect";
                        btnConnect.NormalColor = ThemePalette.ColorAccentEmerald;
                        lblFooterStatus.Text = "🟢 Ready | Local Port: 8088 | Direct & Relay Active";
                    }
                }
            }
            catch { }
        }
    }
}
