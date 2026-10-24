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
        // SUBVIEW 1: REMOTE DESKTOP CONSOLE
        // ======================================================================

        private void InitializeConsoleView()
        {
            viewConsole = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgWorkspace, Padding = new Padding(0) };

            // Top Floating Session Bar
            sessionFloatingToolbar = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(20, 28, 45), Padding = new Padding(6, 4, 6, 4), Visible = false };
            BuildFloatingSessionToolbar();

            // jDroid-X Host Session Security Ribbon (Interactive Permission Controls & Approval)
            bool isDarkRibbon = ThemePalette.IsDarkTheme;
            pnlHostViewerAlert = new Panel {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = isDarkRibbon ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249),
                Padding = new Padding(10, 5, 10, 5),
                Visible = false
            };
            lblHostViewerAlert = new Label {
                Text = "⚠️ jDroid-X SESSION: Remote Desk",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = isDarkRibbon ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38),
                Dock = DockStyle.Left,
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            };

            FlowLayoutPanel pnlPerms = new FlowLayoutPanel {
                Dock = DockStyle.Left,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(12, 2, 0, 0)
            };

            chkPermInput = new CheckBox { Text = "🎮 Input", Checked = true, AutoSize = true, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 7.8f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(4, 4, 6, 0) };
            chkPermInput.CheckedChanged += (s, e) => { hostPermissionAllowInput = chkPermInput.Checked; ShowToast(hostPermissionAllowInput ? "Allowed remote control" : "Blocked remote control"); };

            chkPermClip = new CheckBox { Text = "📋 Clipboard", Checked = true, AutoSize = true, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 7.8f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(4, 4, 6, 0) };
            chkPermClip.CheckedChanged += (s, e) => { hostPermissionAllowClipboard = chkPermClip.Checked; ShowToast(hostPermissionAllowClipboard ? "Allowed clipboard sync" : "Blocked clipboard sync"); };

            chkPermFiles = new CheckBox { Text = "📁 Files", Checked = true, AutoSize = true, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 7.8f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(4, 4, 6, 0) };
            chkPermFiles.CheckedChanged += (s, e) => { hostPermissionAllowFiles = chkPermFiles.Checked; ShowToast(hostPermissionAllowFiles ? "Allowed file transfer" : "Blocked file transfer"); };

            chkPermBlockLocal = new CheckBox { Text = "🚫 Lock Host", Checked = false, AutoSize = true, ForeColor = Color.FromArgb(220, 38, 38), Font = new Font("Segoe UI", 7.8f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(4, 4, 6, 0) };
            chkPermBlockLocal.CheckedChanged += (s, e) => {
                isLocalInputBlockedByRemote = chkPermBlockLocal.Checked;
                try { BlockInput(isLocalInputBlockedByRemote); } catch { }
                ShowToast(isLocalInputBlockedByRemote ? "Local host input locked" : "Local host input unlocked");
            };

            pnlPerms.Controls.Add(chkPermInput);
            pnlPerms.Controls.Add(chkPermClip);
            pnlPerms.Controls.Add(chkPermFiles);
            pnlPerms.Controls.Add(chkPermBlockLocal);

            FlowLayoutPanel pnlAlertActions = new FlowLayoutPanel {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };

            btnHostAcceptViewer = new ModernButton {
                Text = "✅ Accept",
                Size = new Size(82, 26),
                NormalColor = Color.FromArgb(16, 185, 129),
                HoverColor = Color.FromArgb(5, 150, 105),
                Margin = new Padding(0, 0, 6, 0)
            };
            btnHostAcceptViewer.Click += (s, e) => {
                btnHostAcceptViewer.Visible = false;
                string ep = EmbeddedRelayServer.Instance.LastViewerEndpoint ?? "Remote Desk";
                string clientIp = ExtractClientIp(ep);
                if (!string.IsNullOrEmpty(clientIp)) {
                    approvedViewerIps.Add(clientIp);
                    rejectedViewerIps.Remove(clientIp);
                    EmbeddedRelayServer.Instance.AuthorizeViewer(clientIp);
                    EmbeddedRelayServer.Instance.AuthorizeViewer(ep);
                }
                hostPermissionAllowInput = true;
                lastApprovedViewerEndpoint = ep;
                lastRejectedViewerEndpoint = "";
                ShowToast("Incoming jDroid-X session accepted. Remote control granted.");
            };

            btnHostDisconnectViewer = new ModernButton {
                Text = "🛑 Disconnect",
                Size = new Size(96, 26),
                Dock = DockStyle.None,
                NormalColor = Color.FromArgb(220, 38, 38),
                HoverColor = Color.FromArgb(185, 28, 28)
            };
            btnHostDisconnectViewer.Click += (s, e) => {
                string ep = EmbeddedRelayServer.Instance.LastViewerEndpoint ?? "Remote Desk";
                string clientIp = ExtractClientIp(ep);
                if (!string.IsNullOrEmpty(clientIp)) {
                    rejectedViewerIps.Add(clientIp);
                    approvedViewerIps.Remove(clientIp);
                    EmbeddedRelayServer.Instance.RejectViewer(clientIp);
                    EmbeddedRelayServer.Instance.RejectViewer(ep);
                }
                lastRejectedViewerEndpoint = ep;
                lastRejectedViewerTime = DateTime.UtcNow;
                lastApprovedViewerEndpoint = "";
                EmbeddedRelayServer.Instance.DisconnectAllViewers();
                pnlHostViewerAlert.Visible = false;
                if (isLocalInputBlockedByRemote) {
                    isLocalInputBlockedByRemote = false;
                    try { BlockInput(false); } catch { }
                    if (chkPermBlockLocal != null) chkPermBlockLocal.Checked = false;
                }
                ShowToast("jDroid-X session disconnected. Access revoked.");
            };

            pnlAlertActions.Controls.Add(btnHostAcceptViewer);
            pnlAlertActions.Controls.Add(btnHostDisconnectViewer);

            pnlHostViewerAlert.Controls.Add(pnlAlertActions);
            pnlHostViewerAlert.Controls.Add(pnlPerms);
            pnlHostViewerAlert.Controls.Add(lblHostViewerAlert);

            // Local Node Info Ribbon (Non-overlapping, Clean Layout)
            localBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = ThemePalette.ColorBgCardHeader, Padding = new Padding(14, 8, 14, 8) };

            FlowLayoutPanel localLeftBox = new FlowLayoutPanel { Dock = DockStyle.Left, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Margin = new Padding(0) };
            Label lblDeskPrefix = new Label { Text = "THIS DESK:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Margin = new Padding(0, 4, 8, 0), AutoSize = true };
            lblLocalId = new Label { Text = currentHostDesktop != null ? currentHostDesktop.RemoteId : GenerateMachineDeskId(), Font = new Font("Consolas", 12f, FontStyle.Bold), ForeColor = ThemePalette.ColorAccentSky, Margin = new Padding(0, 2, 10, 0), AutoSize = true };
            
            Button btnCopy = new Button { Text = "📋 Copy", Size = new Size(68, 24), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, BackColor = ThemePalette.ColorBgInput, Cursor = Cursors.Hand, Margin = new Padding(0, 1, 12, 0) };
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Click += (s, e) => {
                if (currentHostDesktop != null) {
                    Clipboard.SetText(currentHostDesktop.RemoteId);
                    ShowToast("Copied Desk ID: " + currentHostDesktop.RemoteId);
                }
            };

            lblLocalStatus = new Label { Text = "● Ready", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Margin = new Padding(0, 4, 0, 0), AutoSize = true };

            localLeftBox.Controls.Add(lblDeskPrefix);
            localLeftBox.Controls.Add(lblLocalId);
            localLeftBox.Controls.Add(btnCopy);
            localLeftBox.Controls.Add(lblLocalStatus);

            // Target Connect on right (Symmetric format matching left side)
            FlowLayoutPanel targetBox = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Margin = new Padding(0) };
            Label lTargetPrompt = new Label { Text = "REMOTE DESK:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Margin = new Padding(0, 4, 8, 0), AutoSize = true };
            txtTargetId = new TextBox { Font = new Font("Consolas", 11f, FontStyle.Bold), Size = new Size(130, 24), BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, BorderStyle = BorderStyle.FixedSingle, Text = "" };
            bool isFormattingTxtTarget = false;
            txtTargetId.TextChanged += (s, e) => {
                if (isFormattingTxtTarget) return;
                string raw = txtTargetId.Text.Trim();
                bool isDirect = raw.Contains(".") || raw.Contains(":") || raw.ToLowerInvariant().StartsWith("http");
                string digits = Regex.Replace(raw, @"\D", "");
                if (!isDirect && digits.Length == 9 && !raw.Contains("-"))
                {
                    try
                    {
                        isFormattingTxtTarget = true;
                        txtTargetId.Text = string.Format("{0}-{1}-{2}", digits.Substring(0, 3), digits.Substring(3, 3), digits.Substring(6, 3));
                        txtTargetId.SelectionStart = txtTargetId.Text.Length;
                    }
                    finally { isFormattingTxtTarget = false; }
                }

                if (modernCardTargetBox != null && modernCardTargetBox.Text != txtTargetId.Text)
                    modernCardTargetBox.Text = txtTargetId.Text;
                if (txtCardTargetId != null && txtCardTargetId.Text != txtTargetId.Text)
                    txtCardTargetId.Text = txtTargetId.Text;
            };

            // Connect button (Green)
            btnConnect = new ModernButton { Text = "⚡ Connect", NormalColor = Color.FromArgb(16, 185, 129), HoverColor = Color.FromArgb(5, 150, 105), Size = new Size(88, 24), Margin = new Padding(6, 0, 0, 0), Cursor = Cursors.Hand };
            btnConnect.Click += (s, e) => {
                if (isSessionActive) {
                    ShowToast("Session already active with " + (connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : "remote workstation"));
                    return;
                }
                if (string.IsNullOrEmpty(txtTargetId.Text.Trim())) {
                    ShowToast("Please enter a 9-digit Remote Desk ID to connect.");
                    txtTargetId.Focus();
                    return;
                }
                ToggleSessionConnection();
            };

            // Disconnect button (Red - Visible only when session is active)
            btnDisconnect = new ModernButton { Text = "✕ Disconnect", NormalColor = Color.FromArgb(239, 68, 68), HoverColor = Color.FromArgb(220, 38, 38), Size = new Size(96, 24), Margin = new Padding(6, 0, 0, 0), Cursor = Cursors.Hand, Visible = false, Enabled = false };
            btnDisconnect.Click += (s, e) => {
                if (!isSessionActive) {
                    ShowToast("No active session to disconnect.");
                    return;
                }
                ToggleSessionConnection();
            };

            // Status indicator matching left side
            lblRemoteStatus = new Label { Text = "● Ready", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Margin = new Padding(8, 4, 0, 0), AutoSize = true };

            targetBox.Controls.Add(lTargetPrompt);
            targetBox.Controls.Add(txtTargetId);
            targetBox.Controls.Add(btnConnect);
            targetBox.Controls.Add(btnDisconnect);
            targetBox.Controls.Add(lblRemoteStatus);

            localBar.Controls.Add(localLeftBox);
            localBar.Controls.Add(targetBox);

            // Google Chrome Remote Desktop Style Page (Standby / Fresh Install)
            panelWelcomeCards = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemePalette.ColorBgWorkspace };
            BuildWelcomeCardsContent();

            // GDI+ Canvas for Remote Stream
            viewportCanvas = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 16, 26), Visible = false };
            viewportCanvas.Paint += ViewportCanvas_Paint;
            viewportCanvas.MouseClick += ViewportCanvas_MouseClick;
            viewportCanvas.MouseDown += ViewportCanvas_MouseDown;
            viewportCanvas.MouseUp += ViewportCanvas_MouseUp;
            viewportCanvas.MouseMove += ViewportCanvas_MouseMove;
            viewportCanvas.MouseDoubleClick += ViewportCanvas_MouseDoubleClick;
            viewportCanvas.MouseWheel += ViewportCanvas_MouseWheel;

            viewConsole.Controls.Add(sessionFloatingToolbar);
            viewConsole.Controls.Add(pnlHostViewerAlert);
            viewConsole.Controls.Add(localBar);
            viewConsole.Controls.Add(panelWelcomeCards);
            viewConsole.Controls.Add(viewportCanvas);
            viewConsole.Controls.SetChildIndex(pnlHostViewerAlert, 4);
            viewConsole.Controls.SetChildIndex(sessionFloatingToolbar, 3);
            viewConsole.Controls.SetChildIndex(localBar, 2);
            viewConsole.Controls.SetChildIndex(panelWelcomeCards, 1);
            viewConsole.Controls.SetChildIndex(viewportCanvas, 0);
        }

        private bool isRelayingOutCards = false;

        private void PanelWelcomeCards_Resize(object sender, EventArgs e)
        {
            RelayoutWelcomeCards();
        }

        private void RelayoutWelcomeCards()
        {
            if (isRelayingOutCards || panelWelcomeCards == null || cardContainer == null) return;
            try
            {
                isRelayingOutCards = true;
                cardContainer.SuspendLayout();
                int curW = Math.Max(500, panelWelcomeCards.ClientSize.Width);
                int dynW = Math.Max(640, Math.Min(1400, curW - 48));
                cardContainer.Width = dynW;
                cardContainer.Location = new Point(Math.Max(16, (curW - dynW) / 2), 16);

                // Re-arrange Engine Pill
                if (pnlEnginePill != null) pnlEnginePill.Width = dynW;

                // Calculate responsive columns for Step 1 and Step 2
                bool isTwoCol = dynW >= 760;
                int colW = isTwoCol ? (dynW - 16) / 2 : dynW;
                int colH = 176;

                int step2Y = (pnlEnginePill != null ? pnlEnginePill.Bottom : 0) + 12;

                if (cardShare != null)
                {
                    cardShare.Location = new Point(0, step2Y);
                    cardShare.Size = new Size(colW, colH);
                    if (codeBox != null) codeBox.Width = Math.Min(220, colW - 28);
                    if (shareBtns != null) shareBtns.Width = colW - 28;
                }

                if (flipperConnect != null)
                {
                    int step3Y = isTwoCol ? step2Y : (cardShare != null ? cardShare.Bottom + 14 : step2Y + colH + 14);
                    int step3X = isTwoCol ? colW + 16 : 0;
                    flipperConnect.Location = new Point(step3X, step3Y);
                    flipperConnect.Size = new Size(colW, colH);

                    if (modernCardTargetBox != null)
                    {
                        int targetInputW = Math.Max(120, Math.Min(240, colW - 120));
                        modernCardTargetBox.Size = new Size(targetInputW, 38);
                        if (btnCardConnect != null)
                        {
                            btnCardConnect.Location = new Point(modernCardTargetBox.Right + 8, 58);
                        }
                    }
                    if (cboSavedFleet != null)
                    {
                        cboSavedFleet.Width = colW - 28;
                    }
                }

                // Re-arrange Card Discovered
                int afterStep2Y = Math.Max(cardShare != null ? cardShare.Bottom : 0, flipperConnect != null ? flipperConnect.Bottom : 0) + 14;
                if (cardDiscovered != null)
                {
                    cardDiscovered.Location = new Point(0, afterStep2Y);
                    cardDiscovered.Width = dynW;
                    if (flowDiscoveredDesks != null) flowDiscoveredDesks.Width = dynW - 28;
                }

                // Re-arrange Card Recent
                if (cardRecent != null)
                {
                    int recentY = (cardDiscovered != null ? cardDiscovered.Bottom : afterStep2Y) + 14;
                    cardRecent.Location = new Point(0, recentY);
                    cardRecent.Width = dynW;
                }
            }
            finally
            {
                if (cardContainer != null) cardContainer.ResumeLayout(true);
                isRelayingOutCards = false;
            }
        }

        private void BuildWelcomeCardsContent()
        {
            if (panelWelcomeCards == null) return;
            panelWelcomeCards.Controls.Clear();
            panelWelcomeCards.BackColor = ThemePalette.ColorBgWorkspace;

            int clientW = Math.Max(600, panelWelcomeCards.ClientSize.Width);
            int containerW = Math.Max(720, Math.Min(1400, clientW - 48));
            bool isTwoCol = containerW >= 760;
            int colW = isTwoCol ? (containerW - 16) / 2 : containerW;

            cardContainer = new Panel { Width = containerW, AutoSize = true, BackColor = ThemePalette.ColorBgWorkspace };
            panelWelcomeCards.Resize -= PanelWelcomeCards_Resize;
            panelWelcomeCards.Resize += PanelWelcomeCards_Resize;

            int y = 0;

            // Page Header
            Label lblMainTitle = new Label { Text = "Remote Access & Support", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(0, y), AutoSize = true };
            y += 30;
            Label lblMainSub = new Label { Text = "Professional peer-to-peer screen sharing and remote administration across jDroid-X Mesh.", Font = new Font("Segoe UI", 9f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(0, y), AutoSize = true };
            y += 32;

            cardContainer.Controls.Add(lblMainTitle);
            cardContainer.Controls.Add(lblMainSub);

            // Active jDroid-Engine Quick Switch Banner (Floating Pill)
            string curEngKey = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) ? db.ServerConfig.activeEngine : "sunshine";
            string curEngName = (db != null && db.ServerConfig != null && db.ServerConfig.engines != null && db.ServerConfig.engines.ContainsKey(curEngKey))
                ? db.ServerConfig.engines[curEngKey].name
                : "jDroid-X Sunshine Ultra-Low Latency";

            pnlEnginePill = new Panel {
                Size = new Size(containerW, 38),
                Location = new Point(0, y),
                BackColor = Color.Transparent
            };
            pnlEnginePill.Paint += (s, e) => {
                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, pnlEnginePill.Width - 1, pnlEnginePill.Height - 1), ThemePalette.ColorBgCardHeader, Color.FromArgb(56, 189, 248), ThemePalette.ColorShadow, 8);
            };

            Label lEngPillText = new Label {
                Text = string.Format("⚡ jDroid-Engine: {0} [{1}]", curEngName, engineIsLiveActive ? "● RUNNING" : "⚠️ STANDBY"),
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(12, 10),
                AutoSize = true
            };

            FlowLayoutPanel pillBtns = new FlowLayoutPanel {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 5, 8, 0)
            };

            Button btnPillRun = new Button {
                Text = "🚀 Run Script",
                Size = new Size(95, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = isDarkMode ? Color.FromArgb(20, 32, 50) : Color.FromArgb(220, 252, 231),
                ForeColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 7.6f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
            };
            btnPillRun.FlatAppearance.BorderSize = 0;
            string runEngKey = curEngKey;
            btnPillRun.Click += (s, e) => LaunchEngineStartScript(runEngKey);

            Button btnPillAdmin = new Button {
                Text = "🛡️ Engines",
                Size = new Size(85, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = isDarkMode ? Color.FromArgb(40, 53, 76) : Color.FromArgb(226, 232, 240),
                ForeColor = isDarkMode ? Color.White : Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 7.6f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
            };
            btnPillAdmin.FlatAppearance.BorderSize = 0;
            btnPillAdmin.Click += (s, e) => SwitchWorkspaceView("ServerAdmin");

            Button btnPillGuide = new Button {
                Text = "📖 Guide",
                Size = new Size(75, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(254, 249, 195),
                ForeColor = isDarkMode ? Color.FromArgb(250, 204, 21) : Color.FromArgb(161, 98, 7),
                Font = new Font("Segoe UI", 7.6f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0)
            };
            btnPillGuide.FlatAppearance.BorderSize = 0;
            string pillEngKey = curEngKey;
            btnPillGuide.Click += (s, e) => ShowEngineManualSetupDialog(pillEngKey);

            pillBtns.Controls.Add(btnPillRun);
            pillBtns.Controls.Add(btnPillAdmin);
            pillBtns.Controls.Add(btnPillGuide);

            pnlEnginePill.Controls.Add(lEngPillText);
            pnlEnginePill.Controls.Add(pillBtns);
            cardContainer.Controls.Add(pnlEnginePill);
            y += 46;

            // ==================================================================
            // RESPONSIVE 2-COLUMN GRID: STEP 1 & STEP 2 SIDE-BY-SIDE
            // ==================================================================
            int colH = 176;

            // COLUMN 1: SHARE SCREEN (GET SUPPORT) - 3D Floating Card
            cardShare = new Panel {
                Size = new Size(colW, colH),
                Location = new Point(0, y),
                BackColor = Color.Transparent
            };
            cardShare.Paint += (s, e) => {
                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, cardShare.Width - 1, cardShare.Height - 1), ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 10);
            };

            Label lShareTitle = new Label { Text = "📥  Step 1: Share Screen (This Desk)", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(14, 12), AutoSize = true };
            Label lShareDesc = new Label { Text = "Give your 9-digit Desk ID to allow incoming control:", Font = new Font("Segoe UI", 8.2f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(14, 34), AutoSize = true };

            codeBox = new Panel { Size = new Size(Math.Min(220, colW - 28), 38), Location = new Point(14, 58), BackColor = Color.Transparent };
            codeBox.Paint += (s, e) => {
                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, codeBox.Width - 1, codeBox.Height - 1), ThemePalette.ColorBgInput, ThemePalette.ColorBorderCard, Color.FromArgb(12, 0, 0, 0), 6);
            };
            lblCardLocalId = new Label { Text = currentHostDesktop != null ? currentHostDesktop.RemoteId : GenerateMachineDeskId(), Font = new Font("Consolas", 14f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            codeBox.Controls.Add(lblCardLocalId);

            shareBtns = new FlowLayoutPanel {
                Location = new Point(14, 104),
                Size = new Size(colW - 28, 36),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            Button btnCardCopy = new Button { Text = "📋 Copy ID", Size = new Size(88, 28), FlatStyle = FlatStyle.Flat, BackColor = isDarkMode ? Color.FromArgb(40, 53, 76) : Color.FromArgb(226, 232, 240), ForeColor = isDarkMode ? Color.White : Color.FromArgb(15, 23, 42), Font = new Font("Segoe UI", 7.8f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) };
            btnCardCopy.FlatAppearance.BorderSize = 0;
            btnCardCopy.Click += (s, e) => {
                if (currentHostDesktop != null) {
                    Clipboard.SetText(currentHostDesktop.RemoteId);
                    ShowToast("Desk ID copied to clipboard: " + currentHostDesktop.RemoteId);
                }
            };

            Button btnCardInvite = new Button { Text = "✉️ Invite", Size = new Size(78, 28), FlatStyle = FlatStyle.Flat, BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240), ForeColor = isDarkMode ? Color.White : Color.FromArgb(15, 23, 42), Font = new Font("Segoe UI", 7.8f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) };
            btnCardInvite.FlatAppearance.BorderSize = 0;
            btnCardInvite.Click += (s, e) => {
                if (currentHostDesktop != null) {
                    string invite = string.Format("Join my jDroid-X Remote Desktop session.\nDesk ID: {0}\nDownload client: https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk-Universal.zip", currentHostDesktop.RemoteId);
                    Clipboard.SetText(invite);
                    ShowToast("Complete invite message copied to clipboard!");
                }
            };

            Label lShareStatus = new Label { Text = "● Ready", Font = new Font("Segoe UI", 7.6f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), AutoSize = true, Margin = new Padding(4, 6, 0, 0) };

            shareBtns.Controls.Add(btnCardCopy);
            shareBtns.Controls.Add(btnCardInvite);
            shareBtns.Controls.Add(lShareStatus);

            cardShare.Controls.Add(lShareTitle);
            cardShare.Controls.Add(lShareDesc);
            cardShare.Controls.Add(codeBox);
            cardShare.Controls.Add(shareBtns);
            cardContainer.Controls.Add(cardShare);

            // COLUMN 2: CONNECT TO ANOTHER WORKSTATION (GIVE SUPPORT) - 3D Floating Flipper
            flipperConnect = new ModernCardFlipper {
                Size = new Size(colW, colH),
                Location = isTwoCol ? new Point(colW + 16, y) : new Point(0, y + colH + 14)
            };

            // FRONT VIEW: Connection Actions
            Label lConnTitle = new Label { Text = "📤  Step 2: Connect (Remote Desk)", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(14, 12), AutoSize = true };
            Label lConnDesc = new Label { Text = "Enter remote 9-digit Desk ID or select fleet:", Font = new Font("Segoe UI", 8.2f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(14, 34), AutoSize = true };

            int targetInputW = Math.Max(120, Math.Min(220, colW - 130));
            modernCardTargetBox = new ModernTextBox { Location = new Point(14, 58), Size = new Size(targetInputW, 38), AutoFormatDeskId = true };
            if (txtCardTargetId == null) txtCardTargetId = new TextBox();
            modernCardTargetBox.TextChangedCustom += (s, e) => {
                txtCardTargetId.Text = modernCardTargetBox.Text;
                if (txtTargetId != null && txtTargetId.Text != modernCardTargetBox.Text)
                    txtTargetId.Text = modernCardTargetBox.Text;
            };

            btnCardConnect = new ModernButton { Text = "⚡ Connect", Size = new Size(88, 38), Location = new Point(modernCardTargetBox.Right + 8, 58), NormalColor = Color.FromArgb(239, 51, 64), HoverColor = Color.FromArgb(220, 38, 38) };
            btnCardConnect.Click += (s, e) => {
                if (txtTargetId != null) txtTargetId.Text = modernCardTargetBox.Text.Trim();
                ToggleSessionConnection();
            };

            cboSavedFleet = new ComboBox { Location = new Point(14, 106), Size = new Size(colW - 28, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8f) };
            cboSavedFleet.Items.Add("-- Quick Select Fleet Workstation --");
            if (db != null && db.Devices != null)
            {
                foreach (var d in db.Devices)
                {
                    cboSavedFleet.Items.Add(string.Format("{0} ({1}) [{2}]", d.name, d.id, d.group ?? "Default"));
                }
            }
            cboSavedFleet.SelectedIndex = 0;
            cboSavedFleet.SelectedIndexChanged += (s, e) => {
                if (cboSavedFleet.SelectedIndex > 0 && db != null && db.Devices != null)
                {
                    int devIdx = cboSavedFleet.SelectedIndex - 1;
                    if (devIdx >= 0 && devIdx < db.Devices.Count)
                    {
                        modernCardTargetBox.Text = db.Devices[devIdx].id;
                    }
                }
            };

            flipperConnect.FrontView.Controls.Add(lConnTitle);
            flipperConnect.FrontView.Controls.Add(lConnDesc);
            flipperConnect.FrontView.Controls.Add(modernCardTargetBox);
            flipperConnect.FrontView.Controls.Add(btnCardConnect);
            flipperConnect.FrontView.Controls.Add(cboSavedFleet);

            // BACK VIEW: Telemetry & Cipher
            Label lBackTitle = new Label { Text = "🛡️ Telemetry & Security Cipher", Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Location = new Point(14, 12), AutoSize = true };
            ModernSwitch swHwAccel = new ModernSwitch { LabelText = "GDI+ Blit", Checked = true, Location = new Point(14, 42), Size = new Size(110, 22) };
            ModernSwitch swDirectLan = new ModernSwitch { LabelText = "LAN Fallback", Checked = db.Settings.connection.directConnections, Location = new Point(130, 42), Size = new Size(120, 22) };
            swDirectLan.CheckedChanged += (s, e) => {
                db.Settings.connection.directConnections = swDirectLan.Checked;
                db.Save();
                ShowToast("LAN fallback: " + (swDirectLan.Checked ? "ON" : "OFF"));
            };

            FlowLayoutPanel flowCipher = new FlowLayoutPanel { Location = new Point(14, 72), Size = new Size(colW - 28, 54), FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            Action<string, string> addTelemTag = (k, v) => {
                Panel tp = new Panel { Size = new Size(82, 42), Margin = new Padding(0, 0, 4, 0), BackColor = ThemePalette.ColorBgInput, BorderStyle = BorderStyle.FixedSingle };
                Label tk = new Label { Text = k, Font = new Font("Segoe UI", 6.5f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(3, 1), AutoSize = true };
                Label tv = new Label { Text = v, Font = new Font("Segoe UI", 7.2f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Location = new Point(3, 16), AutoSize = true };
                tp.Controls.Add(tk);
                tp.Controls.Add(tv);
                flowCipher.Controls.Add(tp);
            };

            addTelemTag("Cipher", "AES-256");
            addTelemTag("Relay Port", db.Settings.connection.relayPort > 0 ? db.Settings.connection.relayPort.ToString() : "8088");
            addTelemTag("LAN IP", GetLocalLanIp());
            addTelemTag("FPS", "60 FPS");

            flipperConnect.BackView.Controls.Add(lBackTitle);
            flipperConnect.BackView.Controls.Add(swHwAccel);
            flipperConnect.BackView.Controls.Add(swDirectLan);
            flipperConnect.BackView.Controls.Add(flowCipher);

            cardContainer.Controls.Add(flipperConnect);

            y += isTwoCol ? colH + 16 : (colH * 2) + 28;

            // CARD 2.5: DISCOVERED ON LOCAL NETWORK (Nearby Desks) - 3D Floating Card
            cardDiscovered = new Panel {
                Size = new Size(containerW, 134),
                Location = new Point(0, y),
                BackColor = Color.Transparent
            };
            cardDiscovered.Paint += (s, e) => {
                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, cardDiscovered.Width - 1, cardDiscovered.Height - 1), ThemePalette.ColorBgCard, Color.FromArgb(16, 185, 129), ThemePalette.ColorShadow, 10);
            };

            Label lDiscTitle = new Label { Text = "📡  Discovered on Local Network (Nearby Desks)", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(14, 10), AutoSize = true };
            cardDiscovered.Controls.Add(lDiscTitle);

            ModernButton btnDiscScan = new ModernButton {
                Text = "🔄 Scan LAN",
                Size = new Size(88, 22),
                Location = new Point(containerW - 106, 10),
                NormalColor = Color.FromArgb(40, 53, 76),
                HoverColor = Color.FromArgb(16, 185, 129)
            };
            btnDiscScan.Click += (s, e) => {
                LanDiscoveryEngine.Instance.BroadcastQuery("*");
                ShowToast("Probing local network (UDP 8089)...");
                UpdateDiscoveredDesksContent();
            };
            cardDiscovered.Controls.Add(btnDiscScan);

            flowDiscoveredDesks = new FlowLayoutPanel {
                Location = new Point(14, 38),
                Size = new Size(containerW - 28, 86),
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            cardDiscovered.Controls.Add(flowDiscoveredDesks);
            UpdateDiscoveredDesksContent();

            cardContainer.Controls.Add(cardDiscovered);
            y += 144;

            // CARD 3: RECENT CONNECTIONS (3D Floating Card)
            cardRecent = new Panel { Size = new Size(containerW, 124), Location = new Point(0, y), BackColor = Color.Transparent };
            cardRecent.Paint += (s, e) => {
                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, cardRecent.Width - 1, cardRecent.Height - 1), ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 10);
            };

            Label lRecTitle = new Label { Text = "🕒  Recent Workstations", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(14, 10), AutoSize = true };
            cardRecent.Controls.Add(lRecTitle);

            if (dynamicDesktops.Count == 0)
            {
                Label lRecEmpty = new Label { Text = "No remote workstations saved yet.", Font = new Font("Segoe UI", 8.8f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(14, 38), AutoSize = true };
                Label lRecSub = new Label { Text = "When you connect to remote computers, they will appear here for fast 1-click reconnect.", Font = new Font("Segoe UI", 8f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(14, 62), AutoSize = true };
                cardRecent.Controls.Add(lRecEmpty);
                cardRecent.Controls.Add(lRecSub);
            }
            else
            {
                FlowLayoutPanel flowRecents = new FlowLayoutPanel { Location = new Point(14, 36), Size = new Size(containerW - 28, 76), FlowDirection = FlowDirection.LeftToRight, AutoScroll = true, BackColor = Color.Transparent };
                foreach (SimulatorDesktop target in dynamicDesktops)
                {
                    Panel p = new Panel { Size = new Size(210, 58), BackColor = Color.Transparent, Margin = new Padding(0, 0, 10, 0), Padding = new Padding(6) };
                    SimulatorDesktop t = target;
                    p.Paint += (ps, pe) => {
                        ThemePalette.DrawFloatingCard(pe.Graphics, new Rectangle(0, 0, p.Width - 1, p.Height - 1), ThemePalette.ColorBgCardHeader, t.AccentColor, ThemePalette.ColorShadow, 6);
                    };
                    Label ln = new Label { Text = "💻 " + target.Name, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(6, 6), AutoSize = true, BackColor = Color.Transparent };
                    Label lid = new Label { Text = target.RemoteId, Font = new Font("Consolas", 7.5f), ForeColor = target.AccentColor, Location = new Point(6, 26), AutoSize = true, BackColor = Color.Transparent };
                    ModernButton bc = new ModernButton { Text = "Connect", Size = new Size(62, 22), Location = new Point(140, 28), NormalColor = target.AccentColor, HoverColor = Color.FromArgb(2, 132, 199) };
                    bc.Click += (s, e) => ToggleSessionConnection(t);
                    p.Controls.Add(ln);
                    p.Controls.Add(lid);
                    p.Controls.Add(bc);
                    flowRecents.Controls.Add(p);
                }
                cardRecent.Controls.Add(flowRecents);
            }

            cardContainer.Controls.Add(cardRecent);
            panelWelcomeCards.Controls.Add(cardContainer);
            RelayoutWelcomeCards();
        }

        private void UpdateDiscoveredDesksContent()
        {
            if (flowDiscoveredDesks == null || flowDiscoveredDesks.IsDisposed) return;
            try
            {
                flowDiscoveredDesks.SuspendLayout();
                flowDiscoveredDesks.Controls.Clear();

                List<DiscoveredDeskInfo> desks = LanDiscoveryEngine.Instance.GetDiscoveredDesks();
                if (desks == null || desks.Count == 0)
                {
                    Label lDiscEmpty = new Label {
                        Text = "📡 Scanning local Wi-Fi / LAN (UDP 8089)... Other jDroid-X Desks will appear automatically here.",
                        Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                        ForeColor = ThemePalette.ColorTextSecondary,
                        Location = new Point(4, 12),
                        AutoSize = true
                    };
                    flowDiscoveredDesks.Controls.Add(lDiscEmpty);
                }
                else
                {
                    foreach (DiscoveredDeskInfo d in desks)
                    {
                        DiscoveredDeskInfo deskItem = d;
                        Panel p = new Panel {
                            Size = new Size(230, 72),
                            BackColor = Color.Transparent,
                            Margin = new Padding(0, 0, 10, 0),
                            Padding = new Padding(6)
                        };
                        p.Paint += (ps, pe) => {
                            ThemePalette.DrawFloatingCard(pe.Graphics, new Rectangle(0, 0, p.Width - 1, p.Height - 1), ThemePalette.ColorBgCardHeader, Color.FromArgb(16, 185, 129), ThemePalette.ColorShadow, 6);
                        };
                        Label ln = new Label {
                            Text = "💻 " + (string.IsNullOrEmpty(deskItem.machineName) ? "Remote Desk" : deskItem.machineName),
                            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                            ForeColor = ThemePalette.ColorTextPrimary,
                            Location = new Point(6, 6),
                            AutoSize = true,
                            BackColor = Color.Transparent
                        };
                        Label lid = new Label {
                            Text = deskItem.formattedDeskId,
                            Font = new Font("Consolas", 7.8f, FontStyle.Bold),
                            ForeColor = Color.FromArgb(56, 189, 248),
                            Location = new Point(6, 26),
                            AutoSize = true,
                            BackColor = Color.Transparent
                        };
                        Label lep = new Label {
                            Text = (deskItem.ip ?? "") + ":" + deskItem.port + " ● LAN",
                            Font = new Font("Segoe UI", 6.8f),
                            ForeColor = Color.FromArgb(16, 185, 129),
                            Location = new Point(6, 46),
                            AutoSize = true,
                            BackColor = Color.Transparent
                        };
                        ModernButton bc = new ModernButton {
                            Text = "⚡ Connect",
                            Size = new Size(72, 26),
                            Location = new Point(148, 36),
                            NormalColor = Color.FromArgb(16, 185, 129),
                            HoverColor = Color.FromArgb(5, 150, 105)
                        };
                        bc.Click += (s, e) => {
                            if (txtTargetId != null) txtTargetId.Text = deskItem.deskId;
                            if (modernCardTargetBox != null) modernCardTargetBox.Text = deskItem.deskId;
                            ToggleSessionConnection();
                        };

                        p.Controls.Add(ln);
                        p.Controls.Add(lid);
                        p.Controls.Add(lep);
                        p.Controls.Add(bc);
                        flowDiscoveredDesks.Controls.Add(p);
                    }
                }
            }
            finally
            {
                flowDiscoveredDesks.ResumeLayout(true);
            }
        }

        private void BuildFloatingSessionToolbar()
        {
            sessionFloatingToolbar.Controls.Clear();

            FlowLayoutPanel leftTools = new FlowLayoutPanel { Dock = DockStyle.Left, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };

            Button btnDisplay = CreateToolbarBtn("🖥️ Virtual 1080p", (s, e) => ShowToast("Active Display: Virtual Monitor 1"));
            
            Button btnSmartFit = CreateToolbarBtn("📐 " + smartFitMode, (s, e) => {
                if (smartFitMode == "SmartFit") smartFitMode = "Original 1:1";
                else if (smartFitMode == "Original 1:1") smartFitMode = "Stretch";
                else smartFitMode = "SmartFit";
                ((Button)s).Text = "📐 " + smartFitMode;
                viewportCanvas.Invalidate();
                ShowToast("Scaling Mode: " + smartFitMode);
            });

            Button btnMirror = CreateToolbarBtn("🛡️ " + (isLiveScreenMirror ? "Loopback Shield (ON)" : "Anti-Recursion Shield"), (s, e) => {
                isLiveScreenMirror = !isLiveScreenMirror;
                ((Button)s).Text = "🛡️ " + (isLiveScreenMirror ? "Loopback Shield (ON)" : "Anti-Recursion Shield");
                viewportCanvas.Invalidate();
                ShowToast(isLiveScreenMirror ? "Loopback Shield Active: Infinite mirror recursion prevented." : "Direct Remote Stream Mode Active.");
            });

            Button btnCad = CreateToolbarBtn("⌨️ SAS", (s, e) => {
                SendRemoteAdminCommand("cad");
                ShowToast("Injected Remote SAS (Ctrl+Alt+Del).");
            });
            Button btnTaskMgr = CreateToolbarBtn("⚙️ TaskMgr", (s, e) => {
                SendRemoteAdminCommand("taskmgr");
                ShowToast("Launched Remote Task Manager (Admin).");
            });
            Button btnCmd = CreateToolbarBtn("💻 Cmd", (s, e) => {
                SendRemoteAdminCommand("cmd");
                ShowToast("Launched Remote Admin Command Prompt.");
            });
            Button btnExplorer = CreateToolbarBtn("📁 Explorer", (s, e) => {
                SendRemoteAdminCommand("explorer");
                ShowToast("Opened Remote Windows Explorer.");
            });
            Button btnLock = CreateToolbarBtn("🔒 Lock", (s, e) => {
                SendRemoteAdminCommand("lock");
                ShowToast("Locked Remote Workstation.");
            });
            Button btnImmersive = CreateToolbarBtn("⛶ Immersive (F11)", (s, e) => ToggleImmersiveZenMode());

            // AnyDesk Features
            Button btnSyncClip = CreateToolbarBtn("📋 Sync Clip", (s, e) => SyncLocalClipboardToRemote());
            Button btnScreenshot = CreateToolbarBtn("📸 Screenshot", (s, e) => CaptureRemoteScreenshot());
            Button btnBlockInput = CreateToolbarBtn("🚫 Block Input", (s, e) => ToggleBlockRemoteUserInput((Button)s));
            Button btnFiles = CreateToolbarBtn("📁 Files", (s, e) => OpenRemoteFileManager());
            Button btnSwitchSides = CreateToolbarBtn("⚡ Switch Sides", (s, e) => RequestSwitchSides());
            Button btnChat = CreateToolbarBtn("💬 Chat", (s, e) => OpenInSessionChat());

            leftTools.Controls.Add(btnDisplay);
            leftTools.Controls.Add(btnSmartFit);
            leftTools.Controls.Add(btnImmersive);
            leftTools.Controls.Add(btnMirror);
            leftTools.Controls.Add(btnSyncClip);
            leftTools.Controls.Add(btnScreenshot);
            leftTools.Controls.Add(btnBlockInput);
            leftTools.Controls.Add(btnFiles);
            leftTools.Controls.Add(btnSwitchSides);
            leftTools.Controls.Add(btnChat);
            leftTools.Controls.Add(btnCad);
            leftTools.Controls.Add(btnTaskMgr);
            leftTools.Controls.Add(btnCmd);
            leftTools.Controls.Add(btnExplorer);
            leftTools.Controls.Add(btnLock);

            FlowLayoutPanel rightHUD = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
            lblSessionTelemetryHUD = new Label { Text = "⏳ Awaiting Stream...", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), AutoSize = true, Margin = new Padding(0, 6, 8, 0) };

            ModernButton btnDisconnect = new ModernButton { Text = "❌ Close", NormalColor = Color.FromArgb(239, 51, 64), HoverColor = Color.FromArgb(220, 38, 38), Size = new Size(74, 24) };
            btnDisconnect.Click += (s, e) => {
                if (isSessionActive) ToggleSessionConnection();
                else ShowToast("No active session.");
            };

            rightHUD.Controls.Add(lblSessionTelemetryHUD);
            rightHUD.Controls.Add(btnDisconnect);

            sessionFloatingToolbar.Controls.Add(rightHUD);
            sessionFloatingToolbar.Controls.Add(leftTools);
        }

        private Button CreateToolbarBtn(string text, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 24,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 6, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        // ======================================================================
        // ANYDESK COMPONENT HELPERS (SCREENSHOT, CLIPBOARD, INPUT LOCK, SWITCH SIDES)
        // ======================================================================

        private void CaptureRemoteScreenshot()
        {
            try
            {
                Bitmap snap = null;
                lock (frameLock)
                {
                    if (latestRemoteFrame != null)
                    {
                        snap = new Bitmap(latestRemoteFrame);
                    }
                }

                if (snap == null && viewportCanvas != null && viewportCanvas.Width > 0 && viewportCanvas.Height > 0)
                {
                    snap = new Bitmap(viewportCanvas.Width, viewportCanvas.Height);
                    viewportCanvas.DrawToBitmap(snap, new Rectangle(0, 0, snap.Width, snap.Height));
                }

                if (snap != null)
                {
                    string picDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "jDroid-X-Desk", "Screenshots");
                    if (!Directory.Exists(picDir)) Directory.CreateDirectory(picDir);

                    string targetTag = !string.IsNullOrEmpty(activeReceiverTargetId) ? activeReceiverTargetId : (connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : "Remote");
                    string fileName = string.Format("Desk_{0}_{1:yyyyMMdd_HHmmss}.png", targetTag, DateTime.Now);
                    string fullPath = Path.Combine(picDir, fileName);

                    snap.Save(fullPath, ImageFormat.Png);

                    // Copy snapshot to Windows clipboard
                    Thread t = new Thread(() => {
                        try { Clipboard.SetImage(snap); } catch { }
                    });
                    t.SetApartmentState(ApartmentState.STA);
                    t.IsBackground = true;
                    t.Start();

                    ShowToast("📸 Screenshot saved: " + fileName + " & copied to clipboard!");
                }
                else
                {
                    ShowToast("⚠️ No active remote frame to capture.");
                }
            }
            catch (Exception ex)
            {
                ShowToast("Screenshot error: " + ex.Message);
            }
        }

        private void SyncLocalClipboardToRemote()
        {
            try
            {
                string clipText = "";
                Thread t = new Thread(() => {
                    try {
                        if (Clipboard.ContainsText()) clipText = Clipboard.GetText();
                    } catch { }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.IsBackground = true;
                t.Start();
                t.Join(350);

                if (!string.IsNullOrEmpty(clipText))
                {
                    lastLocalClipboardText = clipText;
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["text"] = clipText;
                    SendRemoteInput("clipboard", ev);
                    ShowToast("📋 Synchronized clipboard (" + clipText.Length + " chars) to remote desk.");
                }
                else
                {
                    ShowToast("📋 Local clipboard is empty or non-text.");
                }
            }
            catch (Exception ex)
            {
                ShowToast("Clipboard sync error: " + ex.Message);
            }
        }

        private void ToggleBlockRemoteUserInput(Button btn)
        {
            isRemoteInputBlockedByViewer = !isRemoteInputBlockedByViewer;
            SendRemoteAdminCommand("block_input");
            if (btn != null)
            {
                btn.Text = isRemoteInputBlockedByViewer ? "🔓 Unblock Input" : "🚫 Block Input";
                btn.BackColor = isRemoteInputBlockedByViewer ? Color.FromArgb(220, 38, 38) : Color.FromArgb(30, 41, 59);
            }
            ShowToast(isRemoteInputBlockedByViewer ? "🚫 Remote user input disabled." : "🔓 Remote user input restored.");
        }

        private void OpenRemoteFileManager()
        {
            string targetId = !string.IsNullOrEmpty(activeReceiverTargetId) ? activeReceiverTargetId : (connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : "");
            SwitchWorkspaceView("FileTransfer");
            ShowToast("📁 File Transfer Workspace activated for Desk: " + targetId);
        }

        private void RequestSwitchSides()
        {
            string localDeskId = currentHostDesktop != null ? currentHostDesktop.RemoteId : "";
            Dictionary<string, object> ev = new Dictionary<string, object>();
            ev["cmd"] = "switch_sides";
            ev["viewerDeskId"] = localDeskId;
            SendRemoteInput("admin_cmd", ev);
            ShowToast("⚡ Sent switch sides request to remote desk.");
        }

        private void HandleSwitchSidesRequested(string viewerDeskId)
        {
            ShowToast("⚡ Remote desk requested side switch. Initiating connection back to " + viewerDeskId + "...");
            if (txtTargetId != null && !string.IsNullOrEmpty(viewerDeskId))
            {
                txtTargetId.Text = viewerDeskId;
            }
            if (modernCardTargetBox != null && !string.IsNullOrEmpty(viewerDeskId))
            {
                modernCardTargetBox.Text = viewerDeskId;
            }
            ThreadPool.QueueUserWorkItem(delegate {
                Thread.Sleep(600);
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() => {
                        if (!isSessionActive) ToggleSessionConnection();
                    }));
                }
            });
        }

        private void OpenInSessionChat()
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 440;
                prompt.Height = 165;
                prompt.Text = "💬 jDroid-X In-Session Chat";
                prompt.StartPosition = FormStartPosition.CenterParent;
                bool isDarkChat = ThemePalette.IsDarkTheme;
                prompt.BackColor = isDarkChat ? Color.FromArgb(15, 23, 42) : Color.FromArgb(255, 255, 255);
                prompt.ForeColor = isDarkChat ? Color.White : Color.FromArgb(15, 23, 42);

                Label textLabel = new Label() { Left = 16, Top = 14, Text = "Message to Remote Operator:", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                TextBox textBox = new TextBox() { Left = 16, Top = 40, Width = 390, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f) };
                Button confirmation = new Button() { Text = "Send", Left = 246, Width = 76, Top = 80, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
                confirmation.FlatAppearance.BorderSize = 0;
                Button cancel = new Button() { Text = "Cancel", Left = 330, Width = 76, Top = 80, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(51, 65, 85), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f) };
                cancel.FlatAppearance.BorderSize = 0;

                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                if (prompt.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(textBox.Text))
                {
                    string msg = textBox.Text.Trim();
                    string myId = currentHostDesktop != null ? currentHostDesktop.RemoteId : "Operator";
                    Dictionary<string, object> chatEv = new Dictionary<string, object>();
                    chatEv["sender"] = myId;
                    chatEv["text"] = msg;
                    SendRemoteInput("chat", chatEv);
                    ShowToast("💬 Sent: " + msg);
                }
            }
        }

        private void ReleaseAllModifierKeys()
        {
            try {
                // INP-05: Release stuck Ctrl, Alt, Shift, and Windows keys on focus loss or disconnect
                keybd_event(0x11, 0, KEYEVENTF_KEYUP, 0); // VK_CONTROL
                keybd_event(0x12, 0, KEYEVENTF_KEYUP, 0); // VK_MENU (Alt)
                keybd_event(0x10, 0, KEYEVENTF_KEYUP, 0); // VK_SHIFT
                keybd_event(0x5B, 0, KEYEVENTF_KEYUP, 0); // VK_LWIN
                keybd_event(0x5C, 0, KEYEVENTF_KEYUP, 0); // VK_RWIN
            } catch { }
        }

        private void ToggleImmersiveZenMode()
        {
            isImmersiveZenMode = !isImmersiveZenMode;
            if (topHeaderBar != null) topHeaderBar.Visible = !isImmersiveZenMode;
            if (activityBar != null) activityBar.Visible = !isImmersiveZenMode;
            if (mainTabBar != null) mainTabBar.Visible = !isImmersiveZenMode;
            if (localBar != null) localBar.Visible = !isImmersiveZenMode;
            if (sessionFloatingToolbar != null) sessionFloatingToolbar.Visible = !isImmersiveZenMode;
            if (collapsibleLeftDrawer != null) collapsibleLeftDrawer.Visible = !isImmersiveZenMode;
            if (collapsibleRightInspector != null) collapsibleRightInspector.Visible = !isImmersiveZenMode;
            if (collapsibleBottomDock != null) collapsibleBottomDock.Visible = !isImmersiveZenMode;

            if (isImmersiveZenMode)
            {
                smartFitMode = "Stretch"; // Edge-to-edge canvas with zero wasted pillarbox borders
                if (pnlRdpPinBar == null) BuildRdpFloatingPinBar();
                if (pnlRdpPinBar != null)
                {
                    pnlRdpPinBar.Visible = true;
                    pnlRdpPinBar.BringToFront();
                    UpdateRdpPinBarStatus();
                }
                ShowToast("Entered Immersive Fullscreen Mode (Press F11 or Esc to exit)");
            }
            else
            {
                if (pnlRdpPinBar != null) pnlRdpPinBar.Visible = false;
                if (currentConnectionState == SessionConnectionState.Connected)
                {
                    if (sessionFloatingToolbar != null) sessionFloatingToolbar.Visible = true;
                    if (localBar != null) localBar.Visible = true;
                }
                ShowToast("Exited Immersive Mode");
            }
            this.PerformLayout();
            if (viewportCanvas != null && !viewportCanvas.IsDisposed) viewportCanvas.Invalidate();
        }

        private void BuildRdpFloatingPinBar()
        {
            if (pnlRdpPinBar != null) return;
            pnlRdpPinBar = new Panel
            {
                Size = new Size(580, 36),
                BackColor = Color.Transparent,
                Padding = new Padding(6, 4, 6, 4),
                Visible = false
            };
            pnlRdpPinBar.Location = new Point(Math.Max(10, (this.ClientSize.Width - pnlRdpPinBar.Width) / 2), 0);
            this.Resize += (s, e) => {
                if (pnlRdpPinBar != null)
                    pnlRdpPinBar.Location = new Point(Math.Max(10, (this.ClientSize.Width - pnlRdpPinBar.Width) / 2), 0);
            };

            pnlRdpPinBar.Paint += (s, e) => {
                Rectangle r = new Rectangle(0, 0, pnlRdpPinBar.Width - 1, pnlRdpPinBar.Height - 1);
                ThemePalette.DrawFloatingCard(e.Graphics, r, ThemePalette.ColorBgCardElevated, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 8);
            };

            FlowLayoutPanel pinFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };

            btnRdpPinToggle = CreateToolbarBtn(isRdpPinBarPinned ? "📌 Pinned" : "📍 Auto-Hide", (s, e) => {
                isRdpPinBarPinned = !isRdpPinBarPinned;
                btnRdpPinToggle.Text = isRdpPinBarPinned ? "📌 Pinned" : "📍 Auto-Hide";
                ShowToast(isRdpPinBarPinned ? "Pin Bar Locked at Top" : "Pin Bar will auto-hide");
            });

            lblRdpPinStatus = new Label
            {
                Text = "🟢 Live Remote Desktop",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                AutoSize = true,
                Margin = new Padding(8, 6, 8, 0)
            };

            Button btnToggleFit = CreateToolbarBtn("📐 " + smartFitMode, (s, e) => {
                if (smartFitMode == "Stretch") smartFitMode = "SmartFit";
                else if (smartFitMode == "SmartFit") smartFitMode = "Original 1:1";
                else smartFitMode = "Stretch";
                ((Button)s).Text = "📐 " + smartFitMode;
                if (viewportCanvas != null) viewportCanvas.Invalidate();
                ShowToast("Scaling Mode: " + smartFitMode);
            });

            Button btnExitImmersive = CreateToolbarBtn("⛶ Exit Zen", (s, e) => ToggleImmersiveZenMode());

            Button btnPinDisconnect = CreateToolbarBtn("✕ Disconnect", (s, e) => {
                if (isImmersiveZenMode) ToggleImmersiveZenMode();
                if (isSessionActive) ToggleSessionConnection();
            });
            btnPinDisconnect.BackColor = Color.FromArgb(239, 68, 68);

            pinFlow.Controls.Add(btnRdpPinToggle);
            pinFlow.Controls.Add(lblRdpPinStatus);
            pinFlow.Controls.Add(btnToggleFit);
            pinFlow.Controls.Add(btnExitImmersive);
            pinFlow.Controls.Add(btnPinDisconnect);

            pnlRdpPinBar.Controls.Add(pinFlow);
            this.Controls.Add(pnlRdpPinBar);
            this.Controls.SetChildIndex(pnlRdpPinBar, 0);
        }

        private void UpdateRdpPinBarStatus()
        {
            if (lblRdpPinStatus != null && !lblRdpPinStatus.IsDisposed)
            {
                string rId = connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : activeReceiverTargetId;
                lblRdpPinStatus.Text = string.Format("🟢 {0} | {1:0.0} FPS | {2}ms", rId, currentMeasuredFps, lastMeasuredRttMs);
            }
        }


        private void SwitchHostDesktop(SimulatorDesktop newHost)
        {
            if (isSessionActive)
            {
                MessageBox.Show("Please disconnect active session before switching local node identity.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            currentHostDesktop = newHost;
            if (lblLocalId != null) lblLocalId.Text = currentHostDesktop.RemoteId;
            if (lblCardLocalId != null) lblCardLocalId.Text = currentHostDesktop.RemoteId;
            UpdateDrawerForCurrentView("Console");
            BuildWelcomeCardsContent();
            RegisterWithColabRelay();
            ShowToast("Local Host switched to " + newHost.Name);
        }

        private string FindFuzzyMatch(string enteredDigits)
        {
            if (string.IsNullOrEmpty(enteredDigits) || enteredDigits.Length < 6) return null;

            List<string> candidateIds = new List<string>();
            if (dynamicDesktops != null)
            {
                foreach (var d in dynamicDesktops)
                {
                    if (!string.IsNullOrEmpty(d.RemoteId))
                        candidateIds.Add(Regex.Replace(d.RemoteId, @"[^\d]", ""));
                }
            }
            if (db != null && db.Contacts != null)
            {
                foreach (var c in db.Contacts)
                {
                    if (!string.IsNullOrEmpty(c.id))
                        candidateIds.Add(Regex.Replace(c.id, @"[^\d]", ""));
                }
            }
            if (db != null && db.Devices != null)
            {
                foreach (var dev in db.Devices)
                {
                    if (!string.IsNullOrEmpty(dev.id))
                        candidateIds.Add(Regex.Replace(dev.id, @"[^\d]", ""));
                }
            }

            foreach (var cand in candidateIds)
            {
                if (cand == enteredDigits) continue;
                if (cand.Length == enteredDigits.Length)
                {
                    int diff = 0;
                    for (int i = 0; i < cand.Length; i++)
                    {
                        if (cand[i] != enteredDigits[i]) diff++;
                    }
                    if (diff <= 3)
                    {
                        if (cand.Length == 9)
                            return string.Format("{0}-{1}-{2}", cand.Substring(0, 3), cand.Substring(3, 3), cand.Substring(6, 3));
                        return cand;
                    }
                }
            }
            return null;
        }

        private void ToggleSessionConnection(SimulatorDesktop directTarget = null)
        {
            if (currentConnectionState == SessionConnectionState.Disconnected || currentConnectionState == SessionConnectionState.Failed)
            {
                string targetId = "";
                if (directTarget != null)
                {
                    connectedTargetDesktop = directTarget;
                    targetId = directTarget.RemoteId;
                    txtTargetId.Text = directTarget.RemoteId;
                    if (txtCardTargetId != null) txtCardTargetId.Text = directTarget.RemoteId;
                    if (modernCardTargetBox != null) modernCardTargetBox.Text = directTarget.RemoteId;
                }
                else
                {
                    targetId = txtTargetId.Text.Trim();
                    if (string.IsNullOrEmpty(targetId) && txtCardTargetId != null)
                    {
                        targetId = txtCardTargetId.Text.Trim();
                        txtTargetId.Text = targetId;
                    }

                    if (string.IsNullOrEmpty(targetId))
                    {
                        MessageBox.Show("Please enter a valid 9-digit Remote Desk ID to connect.", "Enter Remote Desk ID", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (modernCardTargetBox != null) modernCardTargetBox.Focus();
                        else if (txtCardTargetId != null) txtCardTargetId.Focus();
                        else if (txtTargetId != null) txtTargetId.Focus();
                        return;
                    }

                    bool isDirectEndpoint = targetId.Contains(".");
                    if (!isDirectEndpoint)
                    {
                        // Auto-normalize Desk ID (e.g. "729 928 576" or "729928576" -> "729-928-576")
                        string digitsOnly = System.Text.RegularExpressions.Regex.Replace(targetId, @"[^\d]", "");
                        if (digitsOnly.Length == 9)
                        {
                            targetId = string.Format("{0}-{1}-{2}", digitsOnly.Substring(0, 3), digitsOnly.Substring(3, 3), digitsOnly.Substring(6, 3));
                            txtTargetId.Text = targetId;
                            if (txtCardTargetId != null) txtCardTargetId.Text = targetId;
                            if (modernCardTargetBox != null) modernCardTargetBox.Text = targetId;
                        }

                        // Intelligent Fuzzy Typo & Candidate Detection
                        string fuzzyCandidate = FindFuzzyMatch(digitsOnly);
                        if (!string.IsNullOrEmpty(fuzzyCandidate) && fuzzyCandidate != targetId)
                        {
                            int opt = ModernMultiOptionDialog.Show(this, "Target Device Typo Detected",
                                string.Format("Target device '{0}' appears to be a typo.\n\nActive registered workstation '{1}' was found.\nWould you like to auto-correct?", targetId, fuzzyCandidate),
                                new string[] { "⚡ Auto-Correct & Connect", "Connect As-Is", "Cancel" });

                            if (opt == 0)
                            {
                                targetId = fuzzyCandidate;
                                txtTargetId.Text = targetId;
                                if (txtCardTargetId != null) txtCardTargetId.Text = targetId;
                                if (modernCardTargetBox != null) modernCardTargetBox.Text = targetId;
                            }
                            else if (opt == 2 || opt == -1)
                            {
                                return; // User canceled
                            }
                        }
                    }

                    // Anti-Mirror Recursion: Prohibit self-connections to local host Desk ID
                    if (currentHostDesktop != null && !string.IsNullOrEmpty(currentHostDesktop.RemoteId))
                    {
                        if (string.Equals(targetId, currentHostDesktop.RemoteId, StringComparison.OrdinalIgnoreCase))
                        {
                            ShowToast("Self-Connection Blocked", "Cannot connect to your own Desk ID. Connect from another workstation to avoid infinite screen recursion.", "warning");
                            return;
                        }
                    }

                    SimulatorDesktop matched = dynamicDesktops.Find(d => d.RemoteId == targetId || d.Name.ToUpper() == targetId.ToUpper());
                    if (matched != null) connectedTargetDesktop = matched;
                    else
                    {
                        connectedTargetDesktop = new SimulatorDesktop {
                            Key = "CUSTOM",
                            Name = "Workstation (" + targetId + ")",
                            RemoteId = targetId,
                            Passcode = "",
                            Ip = "Remote Node",
                            Role = "Remote Workstation",
                            OsVersion = "Remote Workstation",
                            AccentColor = Color.FromArgb(2, 132, 199),
                            ActiveApps = new[] { "Desktop Session" }
                        };
                    }
                }

                // Transition to CONNECTING state with Animated Heartbeat Progress Bar
                currentConnectionState = SessionConnectionState.Connecting;
                isSessionActive = true;
                connectionAttemptStartTime = DateTime.Now;
                connectingPulseFrame = 0;
                connectingProgressPct = 12;
                connectionStageMessage = "Testing Colab Relay / Cloudflare Tunnel...";
                bytesTransferred = 0;
                framesReceivedCount = 0;
                lastFramesCount = 0;
                currentMeasuredFps = 0.0f;

                if (sessionFloatingToolbar != null) sessionFloatingToolbar.Visible = true;
                if (panelWelcomeCards != null) panelWelcomeCards.Visible = false;
                if (viewportCanvas != null) viewportCanvas.Visible = true;

                // Symmetric ribbon bar buttons in connecting mode
                btnConnect.Visible = false;
                btnConnect.Enabled = false;
                if (btnDisconnect != null)
                {
                    btnDisconnect.Visible = true;
                    btnDisconnect.Text = "✕ Cancel";
                    btnDisconnect.NormalColor = Color.FromArgb(217, 119, 6); // Amber
                    btnDisconnect.HoverColor = Color.FromArgb(180, 83, 9);
                    btnDisconnect.Enabled = true;
                }
                if (lblRemoteStatus != null)
                {
                    lblRemoteStatus.Text = "● Connecting (Heartbeat)...";
                    lblRemoteStatus.ForeColor = Color.FromArgb(245, 158, 11); // Amber
                }
                if (btnCardConnect != null)
                {
                    btnCardConnect.Text = "✕ Cancel";
                    btnCardConnect.NormalColor = Color.FromArgb(217, 119, 6);
                }

                if (connectingAnimationTimer != null) connectingAnimationTimer.Start();

                // Start pulling live remote screen frames via Colab Relay
                StartFrameReceiver(connectedTargetDesktop.RemoteId);
                StartScreenStreamer();

                // Background Automatic Version & Executable Size Synchronization
                CheckAndSyncAppVersion(connectedTargetDesktop.RemoteId);

                db.AddLog("SESSION_ATTEMPT", "P2P_CONNECT", "Initiated connection to " + connectedTargetDesktop.Name + " [" + connectedTargetDesktop.RemoteId + "]");
                viewportCanvas.Invalidate();
            }
            else
            {
                // Disconnect or Cancel active connection
                AbortSessionConnection("User Disconnected");
            }
        }

        private void AbortSessionConnection(string reason = null)
        {
            bool wasConnected = (currentConnectionState == SessionConnectionState.Connected);
            string prevTarget = activeReceiverTargetId ?? (connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : null);
            if (!string.IsNullOrEmpty(prevTarget))
            {
                SendRemoteDisconnectSignal(prevTarget, reason ?? "User Disconnected");
            }

            currentConnectionState = SessionConnectionState.Disconnected;
            isSessionActive = false;
            if (connectingAnimationTimer != null) connectingAnimationTimer.Stop();
            if (sessionTelemetryTimer != null) sessionTelemetryTimer.Stop();
            StopFrameReceiver();
            StopScreenStreamer();

            lock (frameLock)
            {
                if (latestRemoteFrame != null)
                {
                    try { latestRemoteFrame.Dispose(); } catch { }
                    latestRemoteFrame = null;
                }
            }

            if (sessionFloatingToolbar != null) sessionFloatingToolbar.Visible = false;
            if (viewportCanvas != null) viewportCanvas.Visible = false;
            if (panelWelcomeCards != null) panelWelcomeCards.Visible = true;

            TimeSpan duration = DateTime.Now - sessionStartTime;

            if (wasConnected)
            {
                db.Sessions.Add(new SessionItem {
                    id = "ses-" + (db.Sessions.Count + 1),
                    targetId = connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : "Unknown",
                    targetName = connectedTargetDesktop != null ? connectedTargetDesktop.Name : "Remote",
                    type = "Direct P2P",
                    startTime = sessionStartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    durationSeconds = (int)duration.TotalSeconds,
                    status = "Completed",
                    sourceIp = currentHostDesktop != null ? currentHostDesktop.Ip : "127.0.0.1"
                });
                db.AddLog("SESSION_END", "DISCONNECT", "Session closed (" + (int)duration.TotalSeconds + "s)");
                db.Save();
            }

            // Reset symmetric ribbon bar buttons
            btnConnect.Visible = true;
            btnConnect.NormalColor = Color.FromArgb(16, 185, 129);
            btnConnect.Enabled = true;
            if (btnDisconnect != null)
            {
                btnDisconnect.Visible = false;
                btnDisconnect.Text = "✕ Disconnect";
                btnDisconnect.NormalColor = Color.FromArgb(239, 68, 68);
                btnDisconnect.Enabled = false;
            }
            if (lblRemoteStatus != null)
            {
                lblRemoteStatus.Text = "● Ready";
                lblRemoteStatus.ForeColor = Color.FromArgb(148, 163, 184);
            }
            if (btnCardConnect != null)
            {
                btnCardConnect.Text = "⚡ Connect";
                btnCardConnect.NormalColor = Color.FromArgb(16, 185, 129);
            }

            if (!string.IsNullOrEmpty(reason) && reason != "User Disconnected")
            {
                ShowToast(reason);
            }
            else if (wasConnected)
            {
                ShowToast(string.Format("Session closed. Duration: {0}s", (int)duration.TotalSeconds));
            }
        }

    }
}
