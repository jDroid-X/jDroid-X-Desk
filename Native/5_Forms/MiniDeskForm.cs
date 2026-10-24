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

        // --- UI Panels ---
        private Panel headerPanel;
        private Label lblHeaderTitle;
        private Label lblStatusDot;
        private Button btnHamburger;
        private ContextMenuStrip menuHamburger;
        private Button btnHeaderFullScreen;

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
        private Panel pnlSessionControls;
        private Label lblSessionTarget;
        private Label lblSessionTelemetry;
        private Button btnSessionDisconnect;
        private Button btnSessionFullScreen;
        private Button btnSessionCad;
        private Button btnSessionClipboard;
        private Button btnSessionDetails;
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

        private void BuildMiniUi()
        {
            this.Controls.Clear();

            // 1. TOP HEADER PANEL
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(255, 255, 255),
                Padding = new Padding(8, 4, 8, 4)
            };
            headerPanel.Paint += (s, e) => {
                using (Pen p = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawLine(p, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            // Top-left Menu [☰] Button
            btnHamburger = new Button
            {
                Text = "☰",
                Size = new Size(38, 36),
                Location = new Point(6, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnHamburger.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(btnHamburger, "Options & Connect Details Menu");

            // Drop-down Context Menu
            menuHamburger = new ContextMenuStrip();
            menuHamburger.Items.Add("📋 Connect Detail (Full Console)", null, (s, e) => OpenFullDeskDetails());
            menuHamburger.Items.Add("🗄️ Address Book / Fleet", null, (s, e) => { mainForm.ShowFullConsole(); mainForm.SwitchToTab("Fleet"); });
            menuHamburger.Items.Add("⚙️ Settings Studio", null, (s, e) => { mainForm.ShowFullConsole(); mainForm.SwitchToTab("Settings"); });
            menuHamburger.Items.Add("🔄 Check for Updates...", null, (s, e) => mainForm.CheckForUpdates(false));
            menuHamburger.Items.Add("📦 Install to User Folder...", null, (s, e) => mainForm.InstallToUserFolder());
            menuHamburger.Items.Add(new ToolStripSeparator());
            menuHamburger.Items.Add("❌ Exit Application", null, (s, e) => Application.Exit());

            btnHamburger.Click += (s, e) => {
                menuHamburger.Show(btnHamburger, new Point(0, btnHamburger.Height));
            };

            lblStatusDot = new Label
            {
                Text = "🟢",
                Font = new Font("Segoe UI Emoji", 10f),
                Location = new Point(50, 13),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblHeaderTitle = new Label
            {
                Text = "jDroid-X-Desk Mini",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(74, 11),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            btnHeaderFullScreen = new Button
            {
                Text = "⛶ Full Screen",
                Size = new Size(110, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 140, 7),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = Color.FromArgb(241, 245, 249),
                Cursor = Cursors.Hand
            };
            btnHeaderFullScreen.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            toolTip.SetToolTip(btnHeaderFullScreen, "Toggle Fullscreen Viewport");
            btnHeaderFullScreen.Click += (s, e) => ActionToggleFullScreen();

            headerPanel.Controls.Add(btnHamburger);
            headerPanel.Controls.Add(lblStatusDot);
            headerPanel.Controls.Add(lblHeaderTitle);
            headerPanel.Controls.Add(btnHeaderFullScreen);

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

            // Top Session Floating Control Bar
            pnlSessionControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(24, 32, 47),
                Padding = new Padding(8, 4, 8, 4)
            };

            lblSessionTarget = new Label
            {
                Text = "Connected: ---",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                Location = new Point(12, 12),
                AutoSize = true
            };

            lblSessionTelemetry = new Label
            {
                Text = "⚡ 60 FPS | 12 ms RTT | GPU Accelerated",
                Font = new Font("Consolas", 9f),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(220, 13),
                AutoSize = true
            };

            Func<string, string, Action, Color, Button> makeBarButton = (text, tip, action, bg) => {
                Button btn = new Button
                {
                    Text = text,
                    Size = new Size(100, 32),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Emoji", 8.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = bg,
                    Cursor = Cursors.Hand,
                    Dock = DockStyle.Right,
                    Margin = new Padding(4, 0, 4, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                toolTip.SetToolTip(btn, tip);
                btn.Click += (s, e) => action();
                return btn;
            };

            btnSessionDisconnect = makeBarButton("⏹️ Disconnect", "Disconnect and End Remote Session", () => ActionStopSession(), Color.FromArgb(239, 68, 68));
            btnSessionFullScreen = makeBarButton("⛶ Full Screen", "Toggle Borderless Full Screen Viewport", () => ActionToggleFullScreen(), Color.FromArgb(51, 65, 85));
            btnSessionCad = makeBarButton("⌨️ Ctrl+Alt+Del", "Send Ctrl+Alt+Del Administrative Command", () => {
                if (mainForm != null) mainForm.ForwardRemoteAdminCommand("cad");
            }, Color.FromArgb(71, 85, 105));
            int currentClientMonitorIdx = 0;
            Button btnSessionMonitor = null;
            btnSessionMonitor = makeBarButton("🖥️ Display", "Switch Active Remote Monitor / Display", () => {
                if (mainForm != null)
                {
                    currentClientMonitorIdx = (currentClientMonitorIdx + 1) % 4;
                    mainForm.ForwardRemoteAdminCommand("switch_monitor:" + currentClientMonitorIdx);
                    btnSessionMonitor.Text = "🖥️ Mon " + (currentClientMonitorIdx + 1);
                }
            }, Color.FromArgb(71, 85, 105));

            btnSessionClipboard = makeBarButton("📋 Clipboard", "Synchronize Clipboard (Text & Screenshots) with Remote Desk", () => {
                if (mainForm != null)
                {
                    mainForm.SynchronizeClipboardNow();
                    ModernNotificationToast.ShowToast("📋 Clipboard Synced", "Text & Image sync triggered to remote desk.", "info");
                }
            }, Color.FromArgb(71, 85, 105));
            btnSessionDetails = makeBarButton("ℹ️ Details", "Switch to Full Console", () => OpenFullDeskDetails(), Color.FromArgb(71, 85, 105));

            pnlSessionControls.Controls.Add(lblSessionTarget);
            pnlSessionControls.Controls.Add(lblSessionTelemetry);
            pnlSessionControls.Controls.Add(btnSessionDetails);
            pnlSessionControls.Controls.Add(btnSessionMonitor);
            pnlSessionControls.Controls.Add(btnSessionClipboard);
            pnlSessionControls.Controls.Add(btnSessionCad);
            pnlSessionControls.Controls.Add(btnSessionFullScreen);
            pnlSessionControls.Controls.Add(btnSessionDisconnect);

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
            pnlConnectedViewport.Controls.Add(pnlSessionControls);

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
            this.Controls.Add(headerPanel);
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
                headerPanel.Visible = false;
                footerStatusBar.Visible = false;
                isFullScreenMode = true;
                btnSessionFullScreen.Text = "🗗 Restore";
            }
            else
            {
                this.FormBorderStyle = previousBorderStyle;
                this.WindowState = previousWindowState;
                if (previousWindowState == FormWindowState.Normal && !previousBounds.IsEmpty)
                {
                    this.Bounds = previousBounds;
                }
                headerPanel.Visible = true;
                footerStatusBar.Visible = true;
                isFullScreenMode = false;
                btnSessionFullScreen.Text = "⛶ Full Screen";
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

                if (mainForm.CurrentHostDesktop != null && lblMyDeskId.Text != mainForm.CurrentHostDesktop.RemoteId)
                {
                    lblMyDeskId.Text = mainForm.CurrentHostDesktop.RemoteId;
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

                    lblStatusDot.Text = "🔴";
                    string tid = mainForm.ActiveReceiverTargetId ?? "Remote Desk";
                    lblSessionTarget.Text = "Connected: " + tid;
                    lblSessionTelemetry.Text = string.Format("⚡ {0:F0} FPS | {1} ms RTT | GPU Accelerated", mainForm.CurrentMeasuredFps, mainForm.LastMeasuredRttMs);
                    lblFooterStatus.Text = "🔴 Connected to " + tid + " | Active Stream | 60 FPS";
                }
                else
                {
                    // Return to Pre-Connection Cards View
                    if (!pnlPreConnect.Visible)
                    {
                        pnlConnectedViewport.Visible = false;
                        pnlPreConnect.Visible = true;
                    }

                    lblStatusDot.Text = "🟢";
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
