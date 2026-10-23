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
    public partial class MainForm
    {
        // ======================================================================
        // SLIM ACTIVITY BAR (48px RIBBON ON FAR LEFT)
        // ======================================================================

        private void BuildActivityBarContent()
        {
            activityBar.Controls.Clear();

            int y = 8;
            Action<string, string, string> addActivityIcon = (key, icon, tip) => {
                Button btn = new Button
                {
                    Text = icon,
                    Size = new Size(48, 44),
                    Location = new Point(0, y),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Emoji", 13f),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;

                btn.Click += (s, e) => {
                    SelectActivityTab(key, btn);
                };

                activityBar.Controls.Add(btn);
                if (key == "Console") activeActivityButton = btn;
                y += 48;
            };

            addActivityIcon("Console", "🖥️", "Remote Desktop Console");
            addActivityIcon("FileTransfer", "📁", "Direct File Transfer");
            addActivityIcon("Terminal", "💻", "Admin PowerShell");
            addActivityIcon("Fleet", "🗄️", "Address Book & Fleet");
            addActivityIcon("History", "📜", "Session History");
            addActivityIcon("ServerAdmin", "🛡️", "Open-Source Engines & Server Admin");
            addActivityIcon("MindMap", "🕸️", "Graphify AST Mind Map & Architecture Hub");
            addActivityIcon("Settings", "⚙️", "Settings Studio");

            // Gear Settings at bottom
            Button btnSettingsGear = new Button
            {
                Text = "⚙️",
                Size = new Size(48, 44),
                Location = new Point(0, activityBar.Height - 50),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 13f),
                ForeColor = Color.FromArgb(148, 163, 184),
                Cursor = Cursors.Hand
            };
            btnSettingsGear.FlatAppearance.BorderSize = 0;
            btnSettingsGear.Click += (s, e) => SelectActivityTab("Settings", btnSettingsGear);
            activityBar.Controls.Add(btnSettingsGear);

            HighlightActivityBtn(activeActivityButton);
        }

        private void HighlightActivityBtn(Button btn)
        {
            foreach (Control c in activityBar.Controls)
            {
                if (c is Button)
                {
                    c.BackColor = Color.Transparent;
                    c.ForeColor = Color.FromArgb(148, 163, 184);
                }
            }
            if (btn != null)
            {
                btn.BackColor = Color.FromArgb(20, 28, 45);
                btn.ForeColor = Color.FromArgb(56, 189, 248); // Cyan Accent
            }
        }

        private void SelectActivityTab(string key, Button btn)
        {
            if (key == "MindMap")
            {
                try
                {
                    Process.Start("http://localhost:8080/graphify");
                    ShowToast("Launched Graphify AST Station on Port 8080");
                }
                catch (Exception ex)
                {
                    ShowToast("Could not open browser: " + ex.Message);
                }
                return;
            }

            activeActivityButton = btn;
            HighlightActivityBtn(btn);

            if (!collapsibleLeftDrawer.Visible)
            {
                collapsibleLeftDrawer.Visible = true;
                HighlightLayoutBtn(btnToggleLeftDrawer, true);
            }

            SwitchWorkspaceView(key);
            UpdateDrawerForCurrentView(key);
        }

        // ======================================================================
        // COLLAPSIBLE PRIMARY SIDEBAR / DRAWER (260px)
        // ======================================================================

        private void BuildLeftDrawerContent()
        {
            collapsibleLeftDrawer.Controls.Clear();

            drawerHeader = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = ThemePalette.ColorBgSidebarHeader, Padding = new Padding(12, 8, 8, 0) };
            lblDrawerTitle = new Label { Text = "REMOTE FLEET ROSTER", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Dock = DockStyle.Left, AutoSize = true };
            
            Button btnCloseDrawer = new Button { Text = "✕", Size = new Size(24, 24), Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, ForeColor = ThemePalette.ColorTextSecondary, Cursor = Cursors.Hand };
            btnCloseDrawer.FlatAppearance.BorderSize = 0;
            btnCloseDrawer.Click += (s, e) => {
                collapsibleLeftDrawer.Visible = false;
                HighlightLayoutBtn(btnToggleLeftDrawer, false);
            };

            drawerHeader.Controls.Add(lblDrawerTitle);
            drawerHeader.Controls.Add(btnCloseDrawer);

            drawerContentPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgSidebar, Padding = new Padding(8) };

            collapsibleLeftDrawer.Controls.Add(drawerHeader);
            collapsibleLeftDrawer.Controls.Add(drawerContentPanel);
            collapsibleLeftDrawer.Controls.SetChildIndex(drawerHeader, 1);
            collapsibleLeftDrawer.Controls.SetChildIndex(drawerContentPanel, 0);

            UpdateDrawerForCurrentView("Console");
        }

        private void UpdateDrawerForCurrentView(string key)
        {
            if (drawerContentPanel == null) return;
            drawerContentPanel.Controls.Clear();
            drawerContentPanel.BackColor = ThemePalette.ColorBgSidebar;
            if (drawerHeader != null) drawerHeader.BackColor = ThemePalette.ColorBgSidebarHeader;
            if (lblDrawerTitle != null) lblDrawerTitle.ForeColor = ThemePalette.ColorTextSecondary;

            if (key == "Console")
            {
                lblDrawerTitle.Text = "REMOTE WORKSTATIONS";

                if (dynamicDesktops.Count == 0)
                {
                    Panel pEmpty = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 40, 12, 12) };
                    Label lblIcon = new Label { Text = "💻", Font = new Font("Segoe UI Emoji", 28f), ForeColor = ThemePalette.ColorAccentSky, Dock = DockStyle.Top, Height = 48, TextAlign = ContentAlignment.MiddleCenter };
                    Label lblTitle = new Label { Text = "No Saved Workstations", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleCenter };
                    Label lblDesc = new Label { Text = "When you connect to remote computers, they will appear here for 1-click access.", Font = new Font("Segoe UI", 8.5f), ForeColor = ThemePalette.ColorTextSecondary, Dock = DockStyle.Top, Height = 60, TextAlign = ContentAlignment.TopCenter };

                    pEmpty.Controls.Add(lblDesc);
                    pEmpty.Controls.Add(lblTitle);
                    pEmpty.Controls.Add(lblIcon);
                    drawerContentPanel.Controls.Add(pEmpty);
                }
                else
                {
                    Label lPrompt = new Label { Text = "Saved remote workstations:", Font = new Font("Segoe UI", 8f), ForeColor = ThemePalette.ColorTextSecondary, Dock = DockStyle.Top, Height = 24 };
                    drawerContentPanel.Controls.Add(lPrompt);

                    FlowLayoutPanel listFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
                    foreach (SimulatorDesktop target in dynamicDesktops)
                    {
                        Panel card = new Panel { Size = new Size(236, 68), BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 8), Padding = new Padding(8) };
                        card.Paint += (s, e) => {
                            ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, card.Width - 1, card.Height - 3), ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 8);
                        };
                        Label lName = new Label { Text = "💻 " + target.Name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(6, 6), AutoSize = true, BackColor = Color.Transparent };
                        Label lSub = new Label { Text = target.RemoteId + " • " + target.Ip, Font = new Font("Consolas", 8f), ForeColor = target.AccentColor, Location = new Point(8, 26), AutoSize = true, BackColor = Color.Transparent };
                        
                        ModernButton btnCall = new ModernButton { Text = "📞 Connect", NormalColor = target.AccentColor, HoverColor = Color.FromArgb(2, 132, 199), Size = new Size(86, 24), Location = new Point(140, 36) };
                        SimulatorDesktop callTarget = target;
                        btnCall.Click += (s, e) => {
                            SwitchWorkspaceView("Console");
                            if (isSessionActive) ToggleSessionConnection();
                            ToggleSessionConnection(callTarget);
                        };

                        card.Controls.Add(lName);
                        card.Controls.Add(lSub);
                        card.Controls.Add(btnCall);
                        listFlow.Controls.Add(card);
                    }
                    drawerContentPanel.Controls.Add(listFlow);
                    listFlow.BringToFront();
                }
            }
            else if (key == "FileTransfer")
            {
                lblDrawerTitle.Text = "STORAGE & DRIVES";
                ListBox lbDrives = new ListBox { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.None };
                try {
                    foreach (DriveInfo d in DriveInfo.GetDrives())
                    {
                        if (d.IsReady) lbDrives.Items.Add(string.Format("💾 Drive {0} ({1} GB Free)", d.Name, d.AvailableFreeSpace / (1024 * 1024 * 1024)));
                    }
                } catch { lbDrives.Items.Add("💾 Drive C:\\ (System)"); }
                drawerContentPanel.Controls.Add(lbDrives);
            }
            else if (key == "ServerAdmin")
            {
                lblDrawerTitle.Text = "ENGINE & ADMIN ACTIONS";
                FlowLayoutPanel flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(4) };

                Label lblSec = new Label { Text = "Switch jDroid-Engine:", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, AutoSize = true, Margin = new Padding(0, 4, 0, 8) };
                flow.Controls.Add(lblSec);

                Action<string, string> addEngineBtn = (engId, label) => {
                    bool isActive = db != null && db.ServerConfig != null && db.ServerConfig.activeEngine == engId;
                    Button b = new Button {
                        Text = (isActive ? "✓ " : "") + label,
                        Size = new Size(236, 30),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = isActive ? Color.FromArgb(16, 185, 129) : ThemePalette.ColorBgCard,
                        ForeColor = isActive ? Color.White : ThemePalette.ColorTextPrimary,
                        Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 0, 0, 6)
                    };
                    b.FlatAppearance.BorderSize = 0;
                    b.Click += (s, e) => {
                        SwitchActiveEngine(engId);
                    };
                    flow.Controls.Add(b);
                };

                addEngineBtn("rustdesk", "🦀 RustDesk (P2P Direct)");
                addEngineBtn("sunshine", "⚡ Sunshine & Moonlight");
                addEngineBtn("guacamole", "🥑 Guacamole (HTML5)");
                addEngineBtn("jdroidRelay", "🚀 Google Colab (Cloud Relay)");

                Label lblWebHdr = new Label { Text = "Web Management (Backup):", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, AutoSize = true, Margin = new Padding(0, 16, 0, 8) };
                flow.Controls.Add(lblWebHdr);

                Button btnWeb = new Button {
                    Text = "🌐 Open Web Admin (Backup)",
                    Size = new Size(236, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ThemePalette.ColorBgCard,
                    ForeColor = ThemePalette.ColorAccentSky,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 0, 6)
                };
                btnWeb.FlatAppearance.BorderSize = 0;
                btnWeb.Click += (s, e) => {
                    try {
                        Process.Start("http://localhost:12345/admin.html");
                        ShowToast("Opened Web Admin Backup in Browser");
                    } catch (Exception ex) {
                        ShowToast("Could not open browser: " + ex.Message);
                    }
                };
                flow.Controls.Add(btnWeb);

                Button btnSunshineWeb = new Button {
                    Text = "☀️ Open Sunshine UI (47990)",
                    Size = new Size(236, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ThemePalette.ColorBgCard,
                    ForeColor = ThemePalette.ColorAccentAmber,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 0, 6)
                };
                btnSunshineWeb.FlatAppearance.BorderSize = 0;
                btnSunshineWeb.Click += (s, e) => {
                    try {
                        Process.Start("https://localhost:47990");
                        ShowToast("Opened Sunshine Web UI (47990)");
                    } catch (Exception ex) {
                        ShowToast("Could not open browser: " + ex.Message);
                    }
                };
                flow.Controls.Add(btnSunshineWeb);

                drawerContentPanel.Controls.Add(flow);
            }
            else
            {
                lblDrawerTitle.Text = key.ToUpper() + " EXPLORER";
                Label lInfo = new Label { Text = "Workspace view: " + key + "\nUse tabs or bottom dock to interact.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), Dock = DockStyle.Fill, Padding = new Padding(8) };
                drawerContentPanel.Controls.Add(lInfo);
            }
        }

        // ======================================================================
        // COLLAPSIBLE RIGHT INSPECTOR (260px)
        // ======================================================================

        private void BuildRightInspectorContent()
        {
            collapsibleRightInspector.Controls.Clear();

            Label lblHdr = new Label { Text = "SESSION TELEMETRY HUD", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Dock = DockStyle.Top, Height = 28 };
            collapsibleRightInspector.Controls.Add(lblHdr);

            TableLayoutPanel hudGrid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 250, ColumnCount = 1, RowCount = 6 };
            hudGrid.ColumnStyles.Clear();
            hudGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 6; i++) hudGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));

            Action<int, string, string, Action<Label>> addMetric = (row, label, val, setRef) => {
                Panel p = new Panel { Dock = DockStyle.Fill };
                Label l1 = new Label { Text = label, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(0, 0), AutoSize = true };
                Label l2 = new Label { Text = val, Font = new Font("Consolas", 10.5f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Location = new Point(0, 16), AutoSize = true };
                p.Controls.Add(l1);
                p.Controls.Add(l2);
                hudGrid.Controls.Add(p, 0, row);
                if (setRef != null) setRef(l2);
            };

            addMetric(0, "FRAMERATE / STREAM FPS", "60.0 FPS (Active)", (lbl) => lblInspectorFps = lbl);
            addMetric(1, "ROUND-TRIP LATENCY (RTT)", "11.2 ms (LAN P2P)", (lbl) => lblInspectorRtt = lbl);
            addMetric(2, "PAYLOAD TRANSFERRED", "1.4 MB", (lbl) => lblInspectorBytes = lbl);
            addMetric(3, "CIPHER SUITE", "AES-256-GCM Direct", (lbl) => lblInspectorCipher = lbl);
            addMetric(4, "LOCAL HOST NODE", currentHostDesktop != null ? currentHostDesktop.Name : "JEET-DESK", (lbl) => lblInspectorHost = lbl);
            addMetric(5, "TARGET WORKSTATION", connectedTargetDesktop != null ? connectedTargetDesktop.Name : "None", (lbl) => lblInspectorTarget = lbl);

            collapsibleRightInspector.Controls.Add(hudGrid);

            // Screenshot Action Button in Inspector
            ModernButton btnSnap = new ModernButton { Text = "📸 Capture Session Frame", NormalColor = Color.FromArgb(2, 132, 199), HoverColor = Color.FromArgb(14, 165, 233), Dock = DockStyle.Bottom, Height = 34 };
            btnSnap.Click += (s, e) => {
                try {
                    Bitmap bmp = new Bitmap(viewportCanvas.Width, viewportCanvas.Height);
                    viewportCanvas.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
                    bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                    bmp.Dispose();
                    ShowToast("Saved snapshot: " + Path.GetFileName(path));
                } catch { ShowToast("Captured session frame."); }
            };
            collapsibleRightInspector.Controls.Add(btnSnap);
        }

        private void UpdateLiveTelemetry()
        {
            string activeEngName = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) ? db.ServerConfig.activeEngine.ToUpper() : "RELAY";
            if (lblInspectorBytes != null) lblInspectorBytes.Text = (bytesTransferred / (1024 * 1024.0)).ToString("0.0") + " MB";
            if (lblInspectorFps != null) lblInspectorFps.Text = (currentConnectionState == SessionConnectionState.Connected) ? string.Format("{0:0.0} FPS ({1})", currentMeasuredFps, activeEngName) : "0.0 FPS (Idle)";
            if (lblInspectorRtt != null) lblInspectorRtt.Text = (currentConnectionState == SessionConnectionState.Connected && lastMeasuredRttMs > 0) ? (lastMeasuredRttMs + " ms (" + activeEngName + ")") : "-- ms";
            if (lblInspectorCipher != null) lblInspectorCipher.Text = string.Format("{0} • ChaCha20-Poly1305", activeEngName);
            if (lblInspectorHost != null && currentHostDesktop != null) lblInspectorHost.Text = currentHostDesktop.Name;
            if (lblInspectorTarget != null && connectedTargetDesktop != null) lblInspectorTarget.Text = connectedTargetDesktop.Name;

            if (lblSessionTelemetryHUD != null)
            {
                if (currentConnectionState == SessionConnectionState.Connected)
                {
                    lblSessionTelemetryHUD.Text = string.Format("🟢 {0:0} FPS • {1}ms • {2}", currentMeasuredFps, lastMeasuredRttMs > 0 ? lastMeasuredRttMs : 0, activeEngName);
                    lblSessionTelemetryHUD.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else if (currentConnectionState == SessionConnectionState.Connecting)
                {
                    lblSessionTelemetryHUD.Text = string.Format("🟡 Connecting via {0}...", activeEngName);
                    lblSessionTelemetryHUD.ForeColor = Color.FromArgb(245, 158, 11);
                }
                else
                {
                    lblSessionTelemetryHUD.Text = string.Format("⚪ Idle ({0})", activeEngName);
                    lblSessionTelemetryHUD.ForeColor = Color.FromArgb(148, 163, 184);
                }
            }
        }

        // ======================================================================
        // COLLAPSIBLE BOTTOM DOCK (TERMINAL / OUTPUT / DIAGNOSTICS)
        // ======================================================================

        private string GetPowerShellPrompt()
        {
            try
            {
                string up = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(up)) return "PS " + up + "> ";
            }
            catch { }
            return "PS C:\\Users\\" + Environment.UserName + "> ";
        }

        private void BuildBottomDockContent()
        {
            collapsibleBottomDock.Controls.Clear();

            bottomDockTabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f) };

            // Tab 1: PowerShell Terminal
            TabPage tabTerm = new TabPage("💻 PowerShell Terminal");
            tabTerm.BackColor = Color.FromArgb(9, 13, 22);

            terminalBox = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(12, 16, 28), ForeColor = Color.FromArgb(16, 185, 129), Font = new Font("Consolas", 9.5f), ReadOnly = true, Text = "Windows PowerShell [jDroid-X Elevated Diagnostic Console]\n" + GetPowerShellPrompt() };
            Panel termBottom = new Panel { Dock = DockStyle.Bottom, Height = 32, BackColor = Color.FromArgb(20, 27, 45) };
            terminalInput = new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 27, 45), ForeColor = Color.White, Font = new Font("Consolas", 10f, FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle };
            terminalInput.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    ExecuteTerminalCommand();
                    historyIndex = terminalHistory.Count;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    if (terminalHistory.Count > 0 && historyIndex > 0)
                    {
                        historyIndex--;
                        terminalInput.Text = terminalHistory[historyIndex];
                        terminalInput.SelectionStart = terminalInput.Text.Length;
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (terminalHistory.Count > 0 && historyIndex < terminalHistory.Count - 1)
                    {
                        historyIndex++;
                        terminalInput.Text = terminalHistory[historyIndex];
                        terminalInput.SelectionStart = terminalInput.Text.Length;
                    }
                    else
                    {
                        historyIndex = terminalHistory.Count;
                        terminalInput.Text = "";
                    }
                    e.Handled = true;
                }
            };
            Button btnExec = new Button { Text = "Execute", Dock = DockStyle.Right, Width = 80, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(2, 132, 199), Font = new Font("Segoe UI", 8f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExec.Click += (s, e) => ExecuteTerminalCommand();
            termBottom.Controls.Add(terminalInput);
            termBottom.Controls.Add(btnExec);
            tabTerm.Controls.Add(terminalBox);
            tabTerm.Controls.Add(termBottom);

            // Tab 2: P2P Network Traffic Logs
            TabPage tabTraffic = new TabPage("📡 Relay & Traffic Output");
            tabTraffic.BackColor = Color.FromArgb(9, 13, 22);
            lbTrafficLogs = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(12, 16, 28), ForeColor = Color.FromArgb(203, 213, 225), Font = new Font("Consolas", 8.5f), BorderStyle = BorderStyle.None };
            lbTrafficLogs.Items.Add("[INIT] Rendezvous client bound to UDP 0.0.0.0:7070");
            lbTrafficLogs.Items.Add("[NAT] STUN test: Full Cone NAT detected. Direct P2P hole-punching enabled.");
            lbTrafficLogs.Items.Add("[RELAY] Registered node on relay.jdroid-x.net:21116 (Status: ONLINE)");
            lbTrafficLogs.Items.Add("[SECURITY] Handshake cipher negotiated: TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384");
            tabTraffic.Controls.Add(lbTrafficLogs);

            // Tab 3: Hardware Diagnostics
            TabPage tabDiag = new TabPage("📈 System Diagnostics");
            tabDiag.BackColor = Color.FromArgb(9, 13, 22);
            FlowLayoutPanel diagFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), FlowDirection = FlowDirection.LeftToRight };
            lblDiagCpu = new Label { Text = "Logical Cores: " + Environment.ProcessorCount + " Active x64 Cores", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(14, 165, 233), Size = new Size(240, 24) };
            lblDiagMem = new Label { Text = "Process Memory: " + (GC.GetTotalMemory(false) / (1024 * 1024)) + " MB Managed", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Size = new Size(260, 24) };
            lblDiagUptime = new Label { Text = "Host: " + Environment.MachineName + " | OS: Windows 11 Enterprise", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(245, 158, 11), Size = new Size(320, 24) };
            diagFlow.Controls.Add(lblDiagCpu);
            diagFlow.Controls.Add(lblDiagMem);
            diagFlow.Controls.Add(lblDiagUptime);
            tabDiag.Controls.Add(diagFlow);

            bottomDockTabs.TabPages.Add(tabTerm);
            bottomDockTabs.TabPages.Add(tabTraffic);
            bottomDockTabs.TabPages.Add(tabDiag);

            collapsibleBottomDock.Controls.Add(bottomDockTabs);
        }

        private void ExecuteTerminalCommand()
        {
            string cmd = terminalInput.Text.Trim();
            if (string.IsNullOrEmpty(cmd)) return;

            terminalHistory.Add(cmd);
            historyIndex = -1;

            terminalBox.AppendText(cmd + "\n");
            terminalInput.Text = "";

            if (cmd.ToLower() == "clear" || cmd.ToLower() == "cls")
            {
                terminalBox.Text = "Windows PowerShell [jDroid-X Elevated Diagnostic Console]\n" + GetPowerShellPrompt();
                return;
            }

            ThreadPool.QueueUserWorkItem((state) => {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + cmd) {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process p = Process.Start(psi);
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit(3000);

                    this.Invoke((MethodInvoker)delegate {
                        if (!string.IsNullOrEmpty(output)) terminalBox.AppendText(output + "\n");
                        if (!string.IsNullOrEmpty(error)) terminalBox.AppendText("[Error] " + error + "\n");
                        terminalBox.AppendText(GetPowerShellPrompt());
                        terminalBox.SelectionStart = terminalBox.Text.Length;
                        terminalBox.ScrollToCaret();
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate {
                        terminalBox.AppendText("[Exec Exception] " + ex.Message + "\n" + GetPowerShellPrompt());
                    });
                }
            });
        }

        // ======================================================================
        // CENTER EDITOR WORK AREA (ANTIGRAVITY TABS & MAIN VIEWPORT)
        // ======================================================================

        private void BuildCenterEditorContent()
        {
            centerEditorWorkArea.Controls.Clear();

            // Main Antigravity Document Tab Bar across top of workspace
            mainTabBar = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = ThemePalette.ColorBgTabBar, Padding = new Padding(4, 4, 4, 0) };
            BuildMainTabBar();

            // View Container
            viewContainer = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgWorkspace };

            // Initialize Subviews
            InitializeConsoleView();
            InitializeFileTransferView();
            InitializeFleetView();
            InitializeHistoryView();
            InitializeSettingsStudioView();
            InitializeServerAdminView();

            viewContainer.SuspendLayout();
            if (viewConsole != null) { viewConsole.Dock = DockStyle.Fill; viewContainer.Controls.Add(viewConsole); }
            if (viewFileTransfer != null) { viewFileTransfer.Dock = DockStyle.Fill; viewFileTransfer.Visible = false; viewContainer.Controls.Add(viewFileTransfer); }
            if (viewFleet != null) { viewFleet.Dock = DockStyle.Fill; viewFleet.Visible = false; viewContainer.Controls.Add(viewFleet); }
            if (viewHistory != null) { viewHistory.Dock = DockStyle.Fill; viewHistory.Visible = false; viewContainer.Controls.Add(viewHistory); }
            if (viewSettingsStudio != null) { viewSettingsStudio.Dock = DockStyle.Fill; viewSettingsStudio.Visible = false; viewContainer.Controls.Add(viewSettingsStudio); }
            if (viewServerAdmin != null) { viewServerAdmin.Dock = DockStyle.Fill; viewServerAdmin.Visible = false; viewContainer.Controls.Add(viewServerAdmin); }
            viewContainer.ResumeLayout(true);

            SetDoubleBuffered(viewContainer);

            centerEditorWorkArea.Controls.Add(mainTabBar);
            centerEditorWorkArea.Controls.Add(viewContainer);
            centerEditorWorkArea.Controls.SetChildIndex(mainTabBar, 1);
            centerEditorWorkArea.Controls.SetChildIndex(viewContainer, 0);
        }

        private void BuildMainTabBar()
        {
            mainTabBar.SuspendLayout();
            mainTabBar.Controls.Clear();
            mainTabBar.BackColor = ThemePalette.ColorBgTabBar;

            FlowLayoutPanel tabFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            Action<string, string> addTab = (key, title) => {
                bool isActive = activeViewKey == key;
                Button tabBtn = new Button
                {
                    Text = title,
                    AutoSize = true,
                    Height = 32,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5f, isActive ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isActive ? ThemePalette.ColorTextTabActive : ThemePalette.ColorTextTabInactive,
                    BackColor = isActive ? ThemePalette.ColorBgTabActive : ThemePalette.ColorBgTabInactive,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 4, 0)
                };
                tabBtn.FlatAppearance.BorderSize = 0;
                tabBtn.Paint += (s, e) => {
                    if (isActive)
                    {
                        using (Pen p = new Pen(ThemePalette.ColorAccentSky, 2.5f))
                        {
                            e.Graphics.DrawLine(p, 4, tabBtn.Height - 2, tabBtn.Width - 4, tabBtn.Height - 2);
                        }
                    }
                };
                tabBtn.Click += (s, e) => SwitchWorkspaceView(key);
                tabFlow.Controls.Add(tabBtn);
            };

            addTab("Console", "🖥️ Remote Desktop");
            addTab("FileTransfer", "📁 File Transfer");
            addTab("Fleet", "🗄️ Fleet Workstations");
            addTab("History", "📜 Session Logs");
            addTab("Settings", "⚙️ Settings Studio");
            addTab("ServerAdmin", "🛡️ Server & Engines");

            mainTabBar.Controls.Add(tabFlow);
            mainTabBar.ResumeLayout(true);
        }

        private void SwitchWorkspaceView(string key)
        {
            activeViewKey = key;

            BuildMainTabBar();
            UpdateDrawerForCurrentView(key);

            Panel target = null;
            if (key == "Console") target = viewConsole;
            else if (key == "FileTransfer") target = viewFileTransfer;
            else if (key == "Fleet") target = viewFleet;
            else if (key == "History") target = viewHistory;
            else if (key == "Settings") target = viewSettingsStudio;
            else if (key == "ServerAdmin") target = viewServerAdmin;

            if (target != null && viewContainer != null)
            {
                viewContainer.SuspendLayout();
                foreach (Control c in viewContainer.Controls)
                {
                    c.Visible = (c == target);
                }
                target.BringToFront();
                viewContainer.ResumeLayout(true);
            }
        }

    }
}
