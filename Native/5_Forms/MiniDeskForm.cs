using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;

namespace JDroidXDesk.Native
{
    public class MiniDeskForm : Form
    {
        private MainForm mainForm;
        private ToolTip toolTip;

        // --- AnyDesk Parity Top Header & Sub-Bar Controls ---
        private Panel topContainerPanel;

        // Row 1: AnyDesk Tab Header
        private Panel headerPanel;
        private Label lblAnyDeskLogo;
        private Label lblAnyDeskTitle;
        private Panel tabChip;
        private Label lblTabIcon;
        private Label lblTabTitle;
        private Button btnTabClose;
        private Button btnTabAdd;
        private Button btnHeaderProfile;
        private Button btnHeaderFullScreen;
        private ContextMenuStrip menuHamburger;

        // Row 2: AnyDesk Sub-Bar / Action Toolbar
        private Panel toolbarPanel;
        private Label lblToolbarStatusDot;
        private Label lblToolbarScreenIcon;
        private Label lblToolbarPipe;
        private Label lblToolbarDeskId;
        private FlowLayoutPanel pnlActionIcons;

        // The 14 AnyDesk Parity Action Buttons
        private Button btnPrivacy;        // 1. ⦸
        private Button btnDiagnostics;    // 2. ⌛
        private Button btnFiles;          // 3. 🗄️
        private Button btnFavorite;       // 4. ★
        private Button btnSwitchSides;    // 5. ⇄
        private Button btnDisplay;        // 6. 🖵 1
        private Button btnViewMode;       // 7. 🖥️
        private Button btnChat;           // 8. 💬
        private Button btnActions;        // 9. ⚡
        private Button btnKeyboard;       // 10. ⌨️
        private Button btnElevation;      // 11. 🛡️
        private Button btnRecord;         // 12. 📹 PRO
        private Button btnWhiteboard;     // 13. ✏️
        private Button btnMenu;           // 14. ☰

        // Pre-Connection View
        private Panel pnlPreConnect;
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

        // Connected View (Remote Screen Viewport within THIS Screen)
        private Panel pnlConnectedViewport;
        private PictureBox remoteCanvas;

        // Footer & Timers
        private Panel footerStatusBar;
        private Label lblFooterStatus;
        private Timer liveUpdateTimer;

        // Fullscreen state tracking
        private bool isFullScreenMode = false;
        private FormWindowState previousWindowState = FormWindowState.Normal;
        private FormBorderStyle previousBorderStyle = FormBorderStyle.Sizable;
        private Rectangle previousBounds = Rectangle.Empty;

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

            // Master Screen Configuration: Generous, modern workspace, Default Light Theme
            this.Text = "jDroid-X-Desk Mini | Remote Desktop Console";
            this.Size = new Size(1024, 720);
            this.MinimumSize = new Size(800, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(248, 250, 252); // Light Theme Workspace
            this.ForeColor = Color.FromArgb(15, 23, 42);
            this.Font = new Font("Segoe UI", 9f);
            this.KeyPreview = true;

            BuildMiniUi();

            // Refresh state periodically
            liveUpdateTimer = new Timer { Interval = 500 };
            liveUpdateTimer.Tick += (s, e) => UpdateLiveState();
            liveUpdateTimer.Start();

            this.FormClosing += (s, e) => {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (mainForm != null && mainForm.IsSessionActive)
                    {
                        DialogResult dr = MessageBox.Show(this, "A remote desktop connection is currently active.\nDo you want to disconnect and exit?", "Active Remote Connection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr != DialogResult.Yes)
                        {
                            e.Cancel = true;
                            return;
                        }
                        mainForm.DisconnectActiveSession();
                    }
                    // Hide to system tray
                    e.Cancel = true;
                    HideToTray("jDroid-X-Desk Mini minimized to system tray. Double-click tray icon to restore.");
                }
            };
        }

        private Button CreateAnyDeskIconBtn(string text, string tooltip, Action onClick, int width = 30)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(width, 28),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Margin = new Padding(1, 4, 1, 4),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 245, 249);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(226, 232, 240);
            toolTip.SetToolTip(btn, tooltip);
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void BuildMiniUi()
        {
            this.Controls.Clear();

            // 1. TOP DUAL-BAR CONTAINER (Row 1: AnyDesk Tab Bar, Row 2: Action Toolbar)
            topContainerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Color.White
            };

            // ROW 1: AnyDesk Tab Header
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(8, 2, 8, 2)
            };
            headerPanel.Paint += (s, e) => {
                using (Pen p = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawLine(p, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            lblAnyDeskLogo = new Label
            {
                Text = "⚡",
                Font = new Font("Segoe UI Symbol", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(14, 165, 233), // jDroid-X Electric Sky
                Location = new Point(8, 7),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblAnyDeskTitle = new Label
            {
                Text = "jDroid-X",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(28, 8),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Active Tab Chip [ 💻 1 246 039 256  ✕ ]
            tabChip = new Panel
            {
                Size = new Size(185, 28),
                Location = new Point(105, 5),
                BackColor = Color.White
            };
            tabChip.Paint += (s, e) => {
                using (Pen p = new Pen(Color.FromArgb(203, 213, 225), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, tabChip.Width - 1, tabChip.Height - 1);
                }
            };

            lblTabIcon = new Label
            {
                Text = "💻",
                Font = new Font("Segoe UI Emoji", 8.5f),
                Location = new Point(6, 6),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            string initDeskId = (mainForm != null && mainForm.CurrentHostDesktop != null) ? mainForm.CurrentHostDesktop.RemoteId : "New Session";
            lblTabTitle = new Label
            {
                Text = initDeskId,
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(26, 6),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            btnTabClose = new Button
            {
                Text = "✕",
                Size = new Size(20, 20),
                Location = new Point(tabChip.Width - 24, 4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnTabClose.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(btnTabClose, "Close / Disconnect Session");
            btnTabClose.Click += (s, e) => {
                if (mainForm != null && mainForm.IsSessionActive)
                {
                    ActionStopSession();
                }
                else
                {
                    txtRemoteDeskCode.Text = "";
                }
            };

            tabChip.Controls.Add(lblTabIcon);
            tabChip.Controls.Add(lblTabTitle);
            tabChip.Controls.Add(btnTabClose);

            btnTabAdd = new Button
            {
                Text = "+",
                Size = new Size(26, 26),
                Location = new Point(296, 6),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnTabAdd.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(btnTabAdd, "New Remote Connection");
            btnTabAdd.Click += (s, e) => {
                pnlConnectedViewport.Visible = false;
                pnlPreConnect.Visible = true;
                txtRemoteDeskCode.Focus();
            };

            menuHamburger = new ContextMenuStrip();
            menuHamburger.Items.Add("📋 Connect Detail (Full Console)", null, (s, e) => OpenFullDeskDetails());
            menuHamburger.Items.Add("🗄️ Address Book / Fleet", null, (s, e) => { mainForm.ShowFullConsole(); mainForm.SwitchToTab("Fleet"); });
            menuHamburger.Items.Add("⚙️ Settings Studio", null, (s, e) => { mainForm.ShowFullConsole(); mainForm.SwitchToTab("Settings"); });
            menuHamburger.Items.Add("🔄 Check for Updates...", null, (s, e) => mainForm.CheckForUpdates(false));
            menuHamburger.Items.Add("📦 Install to User Folder...", null, (s, e) => mainForm.InstallToUserFolder());
            menuHamburger.Items.Add(new ToolStripSeparator());
            menuHamburger.Items.Add("❌ Exit Application", null, (s, e) => Application.Exit());

            btnHeaderProfile = new Button
            {
                Text = "👤▼",
                Size = new Size(42, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(headerPanel.Width - 85, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 8.5f),
                ForeColor = Color.FromArgb(71, 85, 105),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnHeaderProfile.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(btnHeaderProfile, "Account, Console & Settings");
            btnHeaderProfile.Click += (s, e) => menuHamburger.Show(btnHeaderProfile, new Point(0, btnHeaderProfile.Height));

            btnHeaderFullScreen = new Button
            {
                Text = "⛶",
                Size = new Size(32, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(headerPanel.Width - 38, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnHeaderFullScreen.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(btnHeaderFullScreen, "Toggle Fullscreen Viewport (F11)");
            btnHeaderFullScreen.Click += (s, e) => ActionToggleFullScreen();

            headerPanel.Controls.Add(lblAnyDeskLogo);
            headerPanel.Controls.Add(lblAnyDeskTitle);
            headerPanel.Controls.Add(tabChip);
            headerPanel.Controls.Add(btnTabAdd);
            headerPanel.Controls.Add(btnHeaderProfile);
            headerPanel.Controls.Add(btnHeaderFullScreen);

            // ROW 2: AnyDesk Action Sub-Bar / Toolbar
            toolbarPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = Color.White,
                Padding = new Padding(6, 2, 6, 2)
            };
            toolbarPanel.Paint += (s, e) => {
                using (Pen p = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawLine(p, 0, toolbarPanel.Height - 1, toolbarPanel.Width, toolbarPanel.Height - 1);
                }
            };

            lblToolbarStatusDot = new Label
            {
                Text = "🟢",
                Font = new Font("Segoe UI Emoji", 9.5f),
                Location = new Point(8, 9),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblToolbarScreenIcon = new Label
            {
                Text = "🖥️",
                Font = new Font("Segoe UI Emoji", 9.5f),
                Location = new Point(28, 9),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblToolbarPipe = new Label
            {
                Text = "|",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(203, 213, 225),
                Location = new Point(48, 8),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblToolbarDeskId = new Label
            {
                Text = initDeskId,
                Font = new Font("Consolas", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(2, 132, 199),
                Location = new Point(58, 8),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            toolTip.SetToolTip(lblToolbarDeskId, "Click to copy Desk ID to clipboard");
            lblToolbarDeskId.Click += (s, e) => CopyDeskIdToClipboard();

            pnlActionIcons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Height = 32,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            // The 14 AnyDesk Parity Icons
            btnPrivacy = CreateAnyDeskIconBtn("⦸", "Privacy Mode - Blank remote physical monitor", () => {
                if (mainForm != null) mainForm.TogglePrivacyMode();
            }, 30);

            btnDiagnostics = CreateAnyDeskIconBtn("⌛", "Session Telemetry HUD (RTT, FPS, Relay Status)", () => {
                if (mainForm != null) MessageBox.Show(this, mainForm.GetSessionDiagnostics(), "Session Telemetry & HUD", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }, 30);

            btnFiles = CreateAnyDeskIconBtn("🗄️", "File Transfer - Remote File Manager", () => {
                if (mainForm != null) mainForm.OpenRemoteFileManager();
            }, 30);

            btnFavorite = CreateAnyDeskIconBtn("★", "Add to Favorites / Fleet Address Book", () => {
                if (mainForm != null) mainForm.AddCurrentTargetToFavorites();
            }, 30);

            btnSwitchSides = CreateAnyDeskIconBtn("⇄", "Switch Sides - Reverse session control direction", () => {
                if (mainForm != null) mainForm.RequestSwitchSides();
            }, 30);

            btnDisplay = CreateAnyDeskIconBtn("🖵 1", "Switch Active Display / Monitor", () => {
                if (mainForm != null) {
                    mainForm.SwitchActiveDisplay();
                    btnDisplay.Text = "🖵 " + (mainForm.CurrentClientMonitorIdx == 3 ? "All" : (mainForm.CurrentClientMonitorIdx + 1).ToString());
                }
            }, 38);

            btnViewMode = CreateAnyDeskIconBtn("🖥️", "Display Scaling (SmartFit / 1:1 / Stretch)", () => {
                if (mainForm != null) mainForm.ToggleSmartFitMode();
            }, 30);

            btnChat = CreateAnyDeskIconBtn("💬", "In-Session Chat", () => {
                if (mainForm != null) mainForm.OpenInSessionChat();
            }, 30);

            btnActions = CreateAnyDeskIconBtn("⚡", "Actions (Ctrl+Alt+Del, Lock, TaskMgr, Restart)", () => {
                if (mainForm != null) mainForm.ShowActionsMenu(btnActions, new Point(0, btnActions.Height));
            }, 30);

            btnKeyboard = CreateAnyDeskIconBtn("⌨️", "Keyboard (Send Win key, Alt+Tab, 1:1 Scancodes)", () => {
                if (mainForm != null) mainForm.ShowKeyboardMenu(btnKeyboard, new Point(0, btnKeyboard.Height));
            }, 30);

            btnElevation = CreateAnyDeskIconBtn("🛡️", "Security (Request UAC Elevation)", () => {
                if (mainForm != null) mainForm.ShowElevationMenu(btnElevation, new Point(0, btnElevation.Height));
            }, 30);

            btnRecord = CreateAnyDeskIconBtn("📹 PRO", "Session Recording", () => {
                if (mainForm != null) mainForm.ToggleSessionRecording();
            }, 54);

            btnWhiteboard = CreateAnyDeskIconBtn("✏️", "Whiteboard Annotation Tool", () => {
                if (mainForm != null) mainForm.ToggleWhiteboard();
            }, 30);

            btnMenu = CreateAnyDeskIconBtn("☰", "Session Options & Disconnect", () => {
                if (mainForm != null) mainForm.ShowSessionOptionsMenu(btnMenu, new Point(0, btnMenu.Height));
            }, 30);

            pnlActionIcons.Controls.Add(btnPrivacy);
            pnlActionIcons.Controls.Add(btnDiagnostics);
            pnlActionIcons.Controls.Add(btnFiles);
            pnlActionIcons.Controls.Add(btnFavorite);
            pnlActionIcons.Controls.Add(btnSwitchSides);
            pnlActionIcons.Controls.Add(btnDisplay);
            pnlActionIcons.Controls.Add(btnViewMode);
            pnlActionIcons.Controls.Add(btnChat);
            pnlActionIcons.Controls.Add(btnActions);
            pnlActionIcons.Controls.Add(btnKeyboard);
            pnlActionIcons.Controls.Add(btnElevation);
            pnlActionIcons.Controls.Add(btnRecord);
            pnlActionIcons.Controls.Add(btnWhiteboard);
            pnlActionIcons.Controls.Add(btnMenu);

            toolbarPanel.Controls.Add(lblToolbarStatusDot);
            toolbarPanel.Controls.Add(lblToolbarScreenIcon);
            toolbarPanel.Controls.Add(lblToolbarPipe);
            toolbarPanel.Controls.Add(lblToolbarDeskId);
            toolbarPanel.Controls.Add(pnlActionIcons);

            topContainerPanel.Controls.Add(toolbarPanel);
            topContainerPanel.Controls.Add(headerPanel);

            // 2. PRE-CONNECTION VIEW (Light Theme Cards + Icon Toolbar)
            pnlPreConnect = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(24, 20, 24, 20),
                AutoScroll = true
            };

            // CARD 1: THIS DESK CODE
            cardThisDesk = new ModernCardPanel
            {
                Dock = DockStyle.Top,
                Height = 95,
                BorderColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 16)
            };
            Label lblThisTitle = new Label
            {
                Text = "THIS DESK CODE (Share with partner to allow remote control):",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            string myId = (mainForm != null && mainForm.CurrentHostDesktop != null) ? mainForm.CurrentHostDesktop.RemoteId : "--- --- ---";
            lblMyDeskId = new Label
            {
                Text = myId,
                Font = new Font("Consolas", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(2, 132, 199), // Accent Sky
                Location = new Point(16, 32),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            toolTip.SetToolTip(lblMyDeskId, "Click to copy your Desk ID");
            lblMyDeskId.Click += (s, e) => CopyDeskIdToClipboard();

            btnCopyMyId = new ModernButton
            {
                Text = "📋 Copy Code",
                Size = new Size(110, 34),
                Location = new Point(280, 32),
                NormalColor = Color.FromArgb(241, 245, 249),
                HoverColor = Color.FromArgb(226, 232, 240),
                TextColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            toolTip.SetToolTip(btnCopyMyId, "Copy your Desk ID to clipboard");
            btnCopyMyId.Click += (s, e) => CopyDeskIdToClipboard();

            lblMyDeskStatus = new Label
            {
                Text = "🟢 Ready for Incoming Connections via Cloud Middle Bridge",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(16, 70),
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
                Height = 100,
                BorderColor = Color.FromArgb(2, 132, 199),
                BackColor = Color.White,
                Margin = new Padding(0, 16, 0, 16)
            };
            Label lblRemoteTitle = new Label
            {
                Text = "REMOTE DESK (Enter partner Desk ID to control remote machine):",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtRemoteDeskCode = new ModernTextBox
            {
                Location = new Point(16, 36),
                Size = new Size(280, 44),
                PlaceholderText = "XXX-XXX-XXX",
                AutoFormatDeskId = true
            };
            txtRemoteDeskCode.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) TriggerConnect();
            };

            btnConnect = new ModernButton
            {
                Text = "⚡ Connect",
                Size = new Size(130, 44),
                Location = new Point(310, 36),
                NormalColor = Color.FromArgb(16, 185, 129),
                HoverColor = Color.FromArgb(5, 150, 105),
                TextColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
            };
            toolTip.SetToolTip(btnConnect, "Initiate Remote Connection within this screen");
            btnConnect.Click += (s, e) => TriggerConnect();

            cardRemoteDesk.Controls.Add(lblRemoteTitle);
            cardRemoteDesk.Controls.Add(txtRemoteDeskCode);
            cardRemoteDesk.Controls.Add(btnConnect);

            // 3. MANDATORY ICON TOOLBAR
            pnlIconToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10)
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
                    Size = new Size(88, 42),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Emoji", 9.5f, FontStyle.Bold),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(15, 23, 42),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 10, 0)
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
                btn.FlatAppearance.BorderSize = 1;
                toolTip.SetToolTip(btn, tip);
                btn.Click += (s, e) => action();
                return btn;
            };

            btnIconAdd = makeIconButton("➕ Add", "Add remote Desk ID to Contacts / Address Book", () => ActionAddContact());
            btnIconStart = makeIconButton("▶️ Start", "Start / Resume Host Screen Broadcasting", () => ActionStartHosting());
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

            if (!MainForm.IsRunningElevated())
            {
                Button btnIconElevate = makeIconButton("🛡️ Admin", "Elevate to Administrator (Enables UAC & Secure Desktop control)", () => MainForm.RequestProcessElevation());
                btnIconElevate.ForeColor = Color.FromArgb(234, 88, 12);
                flowIcons.Controls.Add(btnIconElevate);
            }

            pnlIconToolbar.Controls.Add(flowIcons);

            pnlPreConnect.Controls.Add(pnlIconToolbar);
            pnlPreConnect.Controls.Add(cardRemoteDesk);
            pnlPreConnect.Controls.Add(cardThisDesk);

            // 4. CONNECTED VIEWPORT (Remote Screen displayed right within THIS screen!)
            pnlConnectedViewport = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 15, 26),
                Visible = false
            };

            // Double-Buffered Remote Viewport Canvas
            remoteCanvas = new DoubleBufferedCanvas
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 15, 26)
            };
            remoteCanvas.Paint += RemoteCanvas_Paint;
            remoteCanvas.MouseDown += RemoteCanvas_MouseDown;
            remoteCanvas.MouseUp += RemoteCanvas_MouseUp;
            remoteCanvas.MouseMove += RemoteCanvas_MouseMove;
            remoteCanvas.MouseDoubleClick += RemoteCanvas_MouseDoubleClick;
            remoteCanvas.MouseWheel += RemoteCanvas_MouseWheel;
            remoteCanvas.KeyDown += (s, e) => {
                if (mainForm != null && mainForm.IsSessionActive)
                {
                    mainForm.DispatchRemoteKey(e.KeyData);
                    e.Handled = true;
                }
            };

            pnlConnectedViewport.Controls.Add(remoteCanvas);

            // 5. FOOTER STATUS BAR
            footerStatusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(255, 255, 255),
                Padding = new Padding(12, 4, 12, 4)
            };
            footerStatusBar.Paint += (s, e) => {
                using (Pen p = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawLine(p, 0, 0, footerStatusBar.Width, 0);
                }
            };

            lblFooterStatus = new Label
            {
                Text = "🟢 Ready | Cloud Middle Bridge Connected | Zero Loopback",
                Font = new Font("Segoe UI", 8.2f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            footerStatusBar.Controls.Add(lblFooterStatus);

            // Assemble into Form
            this.Controls.Add(pnlConnectedViewport);
            this.Controls.Add(pnlPreConnect);
            this.Controls.Add(footerStatusBar);
            this.Controls.Add(topContainerPanel);
        }

        // --- Frame Arrival Notification from MainForm ---
        public void OnRemoteFrameReceived(int frameBytes)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try
            {
                this.BeginInvoke(new Action(() => {
                    if (remoteCanvas != null && !remoteCanvas.IsDisposed)
                    {
                        remoteCanvas.Invalidate();
                    }
                }));
            }
            catch { }
        }

        // --- Remote Canvas Rendering ---
        private void RemoteCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (mainForm == null) return;
            Image frame = mainForm.GetLatestRemoteFrameClone();
            if (frame == null)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(148, 163, 184)))
                using (Font f = new Font("Segoe UI", 12f, FontStyle.Bold))
                {
                    string msg = "Connecting to Remote Workstation...\nAwaiting Video Keyframe...";
                    SizeF sz = e.Graphics.MeasureString(msg, f);
                    e.Graphics.DrawString(msg, f, b, (remoteCanvas.Width - sz.Width) / 2, (remoteCanvas.Height - sz.Height) / 2);
                }
                return;
            }

            try
            {
                int cw = remoteCanvas.Width;
                int ch = remoteCanvas.Height;
                int imgW = frame.Width;
                int imgH = frame.Height;

                float ratio = Math.Min((float)cw / imgW, (float)ch / imgH);
                int targetW = (int)(imgW * ratio);
                int targetH = (int)(imgH * ratio);
                int targetX = (cw - targetW) / 2;
                int targetY = (ch - targetH) / 2;

                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.DrawImage(frame, targetX, targetY, targetW, targetH);
            }
            catch { }
            finally
            {
                frame.Dispose();
            }
        }

        // --- Input Translation & Forwarding (OOPS MVC Call-by-Reference to MainForm Background Logic) ---
        private bool MapClientToRemote(int clientX, int clientY, out int remoteX, out int remoteY)
        {
            if (mainForm != null && remoteCanvas != null)
            {
                return mainForm.MapCanvasToRemote(clientX, clientY, remoteCanvas.Width, remoteCanvas.Height, out remoteX, out remoteY);
            }
            remoteX = 0;
            remoteY = 0;
            return false;
        }

        private void RemoteCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (remoteCanvas != null) remoteCanvas.Focus();
            if (mainForm != null && remoteCanvas != null)
                mainForm.DispatchRemoteMouseDown(e.X, e.Y, e.Button, remoteCanvas.Width, remoteCanvas.Height);
        }

        private void RemoteCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (mainForm != null && remoteCanvas != null)
                mainForm.DispatchRemoteMouseUp(e.X, e.Y, e.Button, remoteCanvas.Width, remoteCanvas.Height);
        }

        private void RemoteCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mainForm != null && remoteCanvas != null)
                mainForm.DispatchRemoteMouseMove(e.X, e.Y, remoteCanvas.Width, remoteCanvas.Height);
        }

        private void RemoteCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (mainForm != null && remoteCanvas != null)
                mainForm.DispatchRemoteMouseClick(e.X, e.Y, e.Button, remoteCanvas.Width, remoteCanvas.Height);
        }

        private void RemoteCanvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (mainForm != null && remoteCanvas != null)
                mainForm.DispatchRemoteMouseDoubleClick(e.X, e.Y, e.Button, remoteCanvas.Width, remoteCanvas.Height);
        }

        private void RemoteCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (mainForm != null && remoteCanvas != null)
                mainForm.DispatchRemoteMouseWheel(e.Delta, e.X, e.Y, remoteCanvas.Width, remoteCanvas.Height);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (mainForm != null && mainForm.IsSessionActive && pnlConnectedViewport.Visible)
            {
                if (mainForm.DispatchRemoteKey(keyData))
                    return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        // --- Connection Triggers & Actions ---
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
                lblFooterStatus.Text = "🟡 Connecting to " + target + " via Cloud Middle Bridge...";
                btnConnect.Text = "Connecting...";
                btnConnect.NormalColor = Color.FromArgb(245, 158, 11);

                // Initiate connection while KEEPING MiniDeskForm active as the primary viewport
                mainForm.InitiateRemoteConnection(target, keepMiniActive: true);
            }
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
                lblMyDeskStatus.Text = "🟢 Host Broadcast Active & Streaming via Cloud Relay";
                lblMyDeskStatus.ForeColor = Color.FromArgb(16, 185, 129);
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
                    btnConnect.NormalColor = Color.FromArgb(16, 185, 129);
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
            if (!isFullScreenMode)
            {
                previousWindowState = this.WindowState;
                previousBorderStyle = this.FormBorderStyle;
                previousBounds = this.Bounds;

                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                this.Bounds = Screen.FromControl(this).Bounds;
                if (topContainerPanel != null) topContainerPanel.Visible = false;
                if (footerStatusBar != null) footerStatusBar.Visible = false;
                isFullScreenMode = true;
                if (btnHeaderFullScreen != null) btnHeaderFullScreen.Text = "🗗";
            }
            else
            {
                this.FormBorderStyle = previousBorderStyle;
                this.WindowState = previousWindowState;
                if (previousWindowState == FormWindowState.Normal && !previousBounds.IsEmpty)
                {
                    this.Bounds = previousBounds;
                }
                if (topContainerPanel != null) topContainerPanel.Visible = true;
                if (footerStatusBar != null) footerStatusBar.Visible = true;
                isFullScreenMode = false;
                if (btnHeaderFullScreen != null) btnHeaderFullScreen.Text = "⛶";
            }
        }

        public void HideToTray(string reason = null)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => HideToTray(reason)));
                return;
            }
            this.Hide();
            if (!string.IsNullOrEmpty(reason) && mainForm != null)
            {
                mainForm.ShowTrayNotification("jDroid-X-Desk Mini", reason, ToolTipIcon.Info);
            }
        }

        public void RestoreFromTray()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(RestoreFromTray));
                return;
            }
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        public void NotifyHostSessionActive(string sourceId)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => NotifyHostSessionActive(sourceId)));
                return;
            }
            // Auto-hide popup window into system tray so remote user can see and control Desk 2's entire desktop!
            HideToTray("Remote connection active with " + sourceId + ".\nWorkstation minimized to system tray for remote desktop control.");
        }

        public void NotifyHostSessionClosed()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(NotifyHostSessionClosed));
                return;
            }
            if (mainForm != null)
            {
                mainForm.ShowTrayNotification("jDroid-X-Desk Mini", "Remote session ended. Workstation released.", ToolTipIcon.Info);
            }
            RestoreFromTray();
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

                string activeDeskId = (mainForm.CurrentHostDesktop != null) ? mainForm.CurrentHostDesktop.RemoteId : "---";
                if (lblMyDeskId != null && lblMyDeskId.Text != activeDeskId)
                {
                    lblMyDeskId.Text = activeDeskId;
                }

                // Desk 2 Check: If this machine is the Host being controlled remotely and window is visible, auto-hide to tray!
                if (mainForm.IsHostServingRemoteClient && !mainForm.IsSessionActive && this.Visible)
                {
                    HideToTray("Remote connection active.\nMinimized to system tray for remote desktop control.");
                    return;
                }

                if (mainForm.IsSessionActive)
                {
                    // Seamless Switch: Display Remote Viewport within THIS Screen
                    if (!pnlConnectedViewport.Visible)
                    {
                        pnlPreConnect.Visible = false;
                        pnlConnectedViewport.Visible = true;
                    }

                    string tid = mainForm.ActiveReceiverTargetId ?? "Remote Desk";
                    if (lblToolbarStatusDot != null) lblToolbarStatusDot.Text = "🟢";
                    if (lblToolbarDeskId != null) lblToolbarDeskId.Text = tid;
                    if (lblTabTitle != null) lblTabTitle.Text = tid;
                    if (lblFooterStatus != null) lblFooterStatus.Text = string.Format("🔴 Connected to {0} | ⚡ {1:F0} FPS | {2} ms RTT | GPU Accelerated", tid, mainForm.CurrentMeasuredFps, mainForm.LastMeasuredRttMs);

                    // Dynamic State Parity Updates for the 14 AnyDesk action buttons
                    if (btnDisplay != null)
                        btnDisplay.Text = "🖵 " + (mainForm.CurrentClientMonitorIdx == 3 ? "All" : (mainForm.CurrentClientMonitorIdx + 1).ToString());

                    if (btnRecord != null)
                    {
                        btnRecord.Text = mainForm.IsRecordingSession ? "🔴 REC" : "📹 PRO";
                        btnRecord.BackColor = mainForm.IsRecordingSession ? Color.FromArgb(254, 226, 226) : Color.Transparent;
                        btnRecord.ForeColor = mainForm.IsRecordingSession ? Color.FromArgb(220, 38, 38) : Color.FromArgb(51, 65, 85);
                    }

                    if (btnWhiteboard != null)
                    {
                        btnWhiteboard.BackColor = mainForm.IsWhiteboardActive ? Color.FromArgb(224, 242, 254) : Color.Transparent;
                        btnWhiteboard.ForeColor = mainForm.IsWhiteboardActive ? Color.FromArgb(2, 132, 199) : Color.FromArgb(51, 65, 85);
                    }

                    if (btnPrivacy != null)
                    {
                        btnPrivacy.BackColor = mainForm.IsPrivacyModeActive ? Color.FromArgb(254, 243, 199) : Color.Transparent;
                        btnPrivacy.ForeColor = mainForm.IsPrivacyModeActive ? Color.FromArgb(217, 119, 6) : Color.FromArgb(51, 65, 85);
                    }
                }
                else
                {
                    // Return to Pre-Connection Cards View
                    if (!pnlPreConnect.Visible)
                    {
                        pnlConnectedViewport.Visible = false;
                        pnlPreConnect.Visible = true;
                    }

                    if (lblToolbarStatusDot != null) lblToolbarStatusDot.Text = "🟢";
                    if (lblToolbarDeskId != null) lblToolbarDeskId.Text = activeDeskId;
                    if (lblTabTitle != null) lblTabTitle.Text = activeDeskId;

                    if (btnConnect.Text == "Connecting...")
                    {
                        btnConnect.Text = "⚡ Connect";
                        btnConnect.NormalColor = Color.FromArgb(16, 185, 129);
                        lblFooterStatus.Text = "🟢 Ready | Cloud Middle Bridge Connected";
                    }
                }
            }
            catch { }
        }
    }

    // Helper Double-Buffered PictureBox to eliminate rendering flicker
    public class DoubleBufferedCanvas : PictureBox
    {
        public DoubleBufferedCanvas()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}
