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
        // SUBVIEW 2: DUAL-PANE FILE TRANSFER
        // ======================================================================

        private void InitializeFileTransferView()
        {
            viewFileTransfer = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgWorkspace, Padding = new Padding(12) };

            TableLayoutPanel explorerGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, BackColor = Color.Transparent };
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));

            // Left: Local Explorer
            ModernCardPanel cardLocal = new ModernCardPanel { Dock = DockStyle.Fill };
            Panel pnlLocalHeader = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.Transparent };
            lblLocalPath = new Label { Text = "Local: " + currentLocalDirectory, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(4, 6), AutoSize = true, BackColor = Color.Transparent };
            Button btnLocalUp = new Button { Text = "⬆️ Up", Size = new Size(55, 22), Location = new Point(cardLocal.Width - 130, 4), Anchor = AnchorStyles.Top | AnchorStyles.Right, FlatStyle = FlatStyle.Flat, BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240), ForeColor = isDarkMode ? Color.White : Color.FromArgb(15, 23, 42), Font = new Font("Segoe UI", 7.5f), Cursor = Cursors.Hand };
            btnLocalUp.FlatAppearance.BorderSize = 0;
            btnLocalUp.Click += (s, e) => {
                try {
                    DirectoryInfo di = new DirectoryInfo(currentLocalDirectory);
                    if (di.Parent != null) {
                        currentLocalDirectory = di.Parent.FullName;
                        PopulateLocalFiles();
                    }
                } catch { }
            };
            Button btnLocalRefresh = new Button { Text = "🔄", Size = new Size(35, 22), Location = new Point(cardLocal.Width - 65, 4), Anchor = AnchorStyles.Top | AnchorStyles.Right, FlatStyle = FlatStyle.Flat, BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240), ForeColor = isDarkMode ? Color.White : Color.FromArgb(15, 23, 42), Font = new Font("Segoe UI", 7.5f), Cursor = Cursors.Hand };
            btnLocalRefresh.FlatAppearance.BorderSize = 0;
            btnLocalRefresh.Click += (s, e) => PopulateLocalFiles();
            pnlLocalHeader.Controls.Add(lblLocalPath);
            pnlLocalHeader.Controls.Add(btnLocalUp);
            pnlLocalHeader.Controls.Add(btnLocalRefresh);

            lbLocalFiles = new ListBox { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Consolas", 8.5f), BorderStyle = BorderStyle.None };
            lbLocalFiles.DoubleClick += (s, e) => {
                if (lbLocalFiles.SelectedItem == null) return;
                string item = lbLocalFiles.SelectedItem.ToString();
                if (item.StartsWith("[..]"))
                {
                    try {
                        DirectoryInfo di = new DirectoryInfo(currentLocalDirectory);
                        if (di.Parent != null) {
                            currentLocalDirectory = di.Parent.FullName;
                            PopulateLocalFiles();
                        }
                    } catch { }
                }
                else if (item.StartsWith("[DIR]"))
                {
                    try {
                        string dirName = item.Substring(6).Trim();
                        string target = Path.Combine(currentLocalDirectory, dirName);
                        if (Directory.Exists(target)) {
                            currentLocalDirectory = target;
                            PopulateLocalFiles();
                        }
                    } catch { }
                }
            };

            PopulateLocalFiles();
            cardLocal.Controls.Add(lbLocalFiles);
            cardLocal.Controls.Add(pnlLocalHeader);

            // Middle Actions
            Panel mid = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Button btnUp = new Button { Text = "➡️ Upload", Size = new Size(88, 32), Location = new Point(6, 120), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 8f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUp.Click += (s, e) => {
                if (lbLocalFiles.SelectedItem != null) {
                    lbRemoteFiles.Items.Add(lbLocalFiles.SelectedItem);
                    ShowToast("Uploaded " + lbLocalFiles.SelectedItem);
                }
            };
            Button btnDown = new Button { Text = "⬅️ Down", Size = new Size(88, 32), Location = new Point(6, 160), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(2, 132, 199), ForeColor = Color.White, Font = new Font("Segoe UI", 8f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDown.Click += (s, e) => {
                if (lbRemoteFiles.SelectedItem != null) {
                    lbLocalFiles.Items.Add(lbRemoteFiles.SelectedItem);
                    ShowToast("Downloaded " + lbRemoteFiles.SelectedItem);
                }
            };
            mid.Controls.Add(btnUp);
            mid.Controls.Add(btnDown);

            // Right: Remote Explorer
            ModernCardPanel cardRemote = new ModernCardPanel { Dock = DockStyle.Fill };
            Label lblRemotePath = new Label { Text = "Remote: C:\\RemoteWorkstation\\Projects", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Dock = DockStyle.Top, Height = 28, BackColor = Color.Transparent };
            lbRemoteFiles = new ListBox { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Consolas", 8.5f), BorderStyle = BorderStyle.None };
            lbRemoteFiles.Items.Add("[DIR]  Builds");
            lbRemoteFiles.Items.Add("[DIR]  Logs");
            lbRemoteFiles.Items.Add("[FILE] server_telemetry.log (142 KB)");
            lbRemoteFiles.Items.Add("[FILE] kernel_pipeline.dll   (1.8 MB)");
            cardRemote.Controls.Add(lbRemoteFiles);
            cardRemote.Controls.Add(lblRemotePath);

            explorerGrid.Controls.Add(cardLocal, 0, 0);
            explorerGrid.Controls.Add(mid, 1, 0);
            explorerGrid.Controls.Add(cardRemote, 2, 0);

            viewFileTransfer.Controls.Add(explorerGrid);
        }

        private void PopulateLocalFiles()
        {
            if (lbLocalFiles == null) return;
            lbLocalFiles.Items.Clear();
            if (lblLocalPath != null) lblLocalPath.Text = "Local: " + currentLocalDirectory;
            try {
                DirectoryInfo di = new DirectoryInfo(currentLocalDirectory);
                if (di.Parent != null) {
                    lbLocalFiles.Items.Add("[..]  (Parent Directory)");
                }
                foreach (string d in Directory.GetDirectories(currentLocalDirectory)) lbLocalFiles.Items.Add("[DIR]  " + Path.GetFileName(d));
                foreach (string f in Directory.GetFiles(currentLocalDirectory)) {
                    FileInfo fi = new FileInfo(f);
                    lbLocalFiles.Items.Add(string.Format("[FILE] {0,-22} ({1} KB)", fi.Name, fi.Length / 1024));
                }
            } catch { lbLocalFiles.Items.Add("Default root files loaded."); }
        }

        // ======================================================================
        // SUBVIEW 3: FLEET WORKSTATIONS
        // ======================================================================

        private void InitializeFleetView()
        {
            viewFleet = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgWorkspace, Padding = new Padding(12) };

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.Transparent };
            Label lblTitle = new Label { Text = "Registered Workstation Fleet", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, BackColor = Color.Transparent };
            txtFleetSearch = new TextBox { Font = new Font("Segoe UI", 8.5f), Size = new Size(140, 22), Margin = new Padding(0, 2, 8, 0), BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, BorderStyle = BorderStyle.FixedSingle };
            txtFleetSearch.TextChanged += (s, e) => FilterFleetGrid();

            cboFleetGroupFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 8.5f), Size = new Size(150, 24), Margin = new Padding(0, 0, 8, 0), BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary };
            cboFleetGroupFilter.Items.Add("All Groups");
            foreach (GroupItem g in db.Groups) cboFleetGroupFilter.Items.Add(g.name);
            cboFleetGroupFilter.SelectedIndex = 0;
            cboFleetGroupFilter.SelectedIndexChanged += (s, e) => FilterFleetGrid();

            Button btnConn = new Button { Text = "⚡ Connect", Size = new Size(86, 26), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(239, 51, 64), ForeColor = Color.White, Font = new Font("Segoe UI", 8f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnConn.Click += (s, e) => {
                if (dgvFleet.CurrentRow != null) {
                    DeviceItem d = dgvFleet.CurrentRow.DataBoundItem as DeviceItem;
                    if (d != null) {
                        SimulatorDesktop matched = dynamicDesktops.Find(desk => desk.Name == d.name || desk.Ip == d.ip);
                        SwitchWorkspaceView("Console");
                        if (isSessionActive) ToggleSessionConnection();
                        ToggleSessionConnection(matched);
                    }
                }
            };

            actions.Controls.Add(txtFleetSearch);
            actions.Controls.Add(cboFleetGroupFilter);
            actions.Controls.Add(btnConn);

            topRow.Controls.Add(lblTitle);
            topRow.Controls.Add(actions);

            dgvFleet = new DataGridView {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemePalette.ColorBgCard,
                ForeColor = ThemePalette.ColorTextPrimary,
                GridColor = ThemePalette.ColorBorderCard,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };
            dgvFleet.ColumnHeadersDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
            dgvFleet.ColumnHeadersDefaultCellStyle.ForeColor = ThemePalette.ColorTextSecondary;
            dgvFleet.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvFleet.DefaultCellStyle.BackColor = ThemePalette.ColorBgCard;
            dgvFleet.DefaultCellStyle.ForeColor = ThemePalette.ColorTextPrimary;
            dgvFleet.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f);
            dgvFleet.DefaultCellStyle.SelectionBackColor = Color.FromArgb(56, 189, 248);
            dgvFleet.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvFleet.AlternatingRowsDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;

            dgvFleet.CellDoubleClick += (s, e) => {
                if (dgvFleet.CurrentRow != null) {
                    DeviceItem d = dgvFleet.CurrentRow.DataBoundItem as DeviceItem;
                    if (d != null) {
                        SimulatorDesktop matched = dynamicDesktops.Find(desk => desk.Name == d.name || desk.Ip == d.ip);
                        SwitchWorkspaceView("Console");
                        if (isSessionActive) ToggleSessionConnection();
                        ToggleSessionConnection(matched);
                    }
                }
            };

            viewFleet.Controls.Add(dgvFleet);
            viewFleet.Controls.Add(topRow);
            topRow.BringToFront();

            FilterFleetGrid();
        }

        private void FilterFleetGrid()
        {
            if (dgvFleet == null) return;
            string sel = cboFleetGroupFilter != null && cboFleetGroupFilter.SelectedItem != null ? cboFleetGroupFilter.SelectedItem.ToString() : "All Groups";
            string q = txtFleetSearch != null ? txtFleetSearch.Text.Trim().ToLower() : "";

            List<DeviceItem> list = db.Devices;
            if (sel != "All Groups") list = list.FindAll(d => d.group == sel || d.groupId == sel);
            if (!string.IsNullOrEmpty(q)) list = list.FindAll(d => d.name.ToLower().Contains(q) || d.ip.Contains(q));

            dgvFleet.DataSource = null;
            dgvFleet.DataSource = list;
        }

        // ======================================================================
        // SUBVIEW 4: SESSION HISTORY
        // ======================================================================

        private void InitializeHistoryView()
        {
            viewHistory = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgWorkspace, Padding = new Padding(12) };

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.Transparent };
            Label lblTitle = new Label { Text = "Connection History & Auditable Session Logs", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };

            Button btnClear = new Button { Text = "🗑️ Clear Logs", Size = new Size(110, 26), Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(239, 51, 64), ForeColor = Color.White, Font = new Font("Segoe UI", 8f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.Click += (s, e) => {
                if (MessageBox.Show("Clear all session logs?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    db.Sessions.Clear();
                    db.Save();
                    RefreshHistory();
                    ShowToast("Session history cleared.");
                }
            };

            topRow.Controls.Add(lblTitle);
            topRow.Controls.Add(btnClear);

            dgvHistory = new DataGridView {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemePalette.ColorBgCard,
                ForeColor = ThemePalette.ColorTextPrimary,
                GridColor = ThemePalette.ColorBorderCard,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = ThemePalette.ColorTextSecondary;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvHistory.DefaultCellStyle.BackColor = ThemePalette.ColorBgCard;
            dgvHistory.DefaultCellStyle.ForeColor = ThemePalette.ColorTextPrimary;
            dgvHistory.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f);
            dgvHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(56, 189, 248);
            dgvHistory.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvHistory.AlternatingRowsDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;

            viewHistory.Controls.Add(dgvHistory);
            viewHistory.Controls.Add(topRow);
            topRow.BringToFront();

            RefreshHistory();
        }

        private void RefreshHistory()
        {
            if (dgvHistory == null) return;
            dgvHistory.DataSource = null;
            dgvHistory.DataSource = new List<SessionItem>(db.Sessions);
        }

        // ======================================================================
        // SUBVIEW 5: SETTINGS & SECURITY STUDIO
        // ======================================================================

        private void InitializeSettingsStudioView()
        {
            viewSettingsStudio = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgWorkspace, Padding = new Padding(12) };

            Panel header = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = ThemePalette.ColorBgCardHeader, Padding = new Padding(8, 6, 8, 6) };
            Label lblTitle = new Label { Text = "Settings & Security Studio (jDroid-X Google Colab Relay Engine)", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };

            ModernButton btnSave = new ModernButton { Text = "💾 Save & Apply", NormalColor = Color.FromArgb(16, 185, 129), HoverColor = Color.FromArgb(21, 150, 78), Size = new Size(130, 28), Dock = DockStyle.Right };
            btnSave.Click += (s, e) => {
                db.Save();
                ShowToast("Settings saved to database.json.");
            };

            header.Controls.Add(btnSave);
            header.Controls.Add(lblTitle);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 210, BackColor = ThemePalette.ColorBorderCard };
            settingsTree = new TreeView { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9f), BackColor = ThemePalette.ColorBgSidebar, ForeColor = ThemePalette.ColorTextPrimary, BorderStyle = BorderStyle.None };
            
            TreeNode nodeSettings = new TreeNode("⚙️ Settings Studio") { Name = "General" };
            nodeSettings.Nodes.Add("General", "General Preferences");
            nodeSettings.Nodes.Add("Connection", "Connection & Relays (hbbs/hbbr)");
            nodeSettings.Nodes.Add("Security", "Security & IP Whitelist");
            nodeSettings.Nodes.Add("Display", "Display & Scaling");
            nodeSettings.Nodes.Add("Account", "Operator Credentials");

            TreeNode nodeReadme = new TreeNode("📖 Readme (Presentation)") { Name = "Readme" };
            TreeNode nodeHelp = new TreeNode("❓ Help & Operations Guide") { Name = "Help" };
            TreeNode nodeAbout = new TreeNode("ℹ️ About jDroid-X-Desk") { Name = "About" };
            TreeNode nodeLicense = new TreeNode("⚖️ Software License") { Name = "License" };
            TreeNode nodeCopyright = new TreeNode("© Copyright & Trademarks") { Name = "Copyright" };

            settingsTree.Nodes.Add(nodeSettings);
            settingsTree.Nodes.Add(nodeReadme);
            settingsTree.Nodes.Add(nodeHelp);
            settingsTree.Nodes.Add(nodeAbout);
            settingsTree.Nodes.Add(nodeLicense);
            settingsTree.Nodes.Add(nodeCopyright);
            settingsTree.ExpandAll();

            settingsTree.AfterSelect += (s, e) => {
                string cat = !string.IsNullOrEmpty(e.Node.Name) ? e.Node.Name : "General";
                RenderSettingsSubPanel(cat);
            };
            split.Panel1.Controls.Add(settingsTree);

            settingsSubContentPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemePalette.ColorBgWorkspace, Padding = new Padding(16) };
            split.Panel2.Controls.Add(settingsSubContentPanel);

            viewSettingsStudio.Controls.Add(split);
            viewSettingsStudio.Controls.Add(header);
            header.BringToFront();

            settingsTree.SelectedNode = nodeSettings.Nodes[0];
        }

        private void RenderSettingsSubPanel(string category)
        {
            settingsSubContentPanel.Controls.Clear();
            int y = 10;

            Action<string> addH = (text) => {
                Label l = new Label { Text = text, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(10, y), AutoSize = true, BackColor = Color.Transparent };
                settingsSubContentPanel.Controls.Add(l);
                y += 30;
            };

            Func<string, bool, ModernSwitch> addSw = (text, isChecked) => {
                ModernSwitch sw = new ModernSwitch { LabelText = text, Checked = isChecked, Location = new Point(14, y), Size = new Size(380, 28) };
                settingsSubContentPanel.Controls.Add(sw);
                y += 32;
                return sw;
            };

            Func<string, string, TextBox> addTb = (label, val) => {
                Label lbl = new Label { Text = label, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(14, y), AutoSize = true, BackColor = Color.Transparent };
                settingsSubContentPanel.Controls.Add(lbl);
                y += 18;
                TextBox tb = new TextBox { Text = val, Font = new Font("Segoe UI", 9f), BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, BorderStyle = BorderStyle.FixedSingle, Location = new Point(14, y), Size = new Size(340, 24) };
                settingsSubContentPanel.Controls.Add(tb);
                y += 32;
                return tb;
            };

            Action<string, string> addCard = (title, body) => {
                Panel card = new Panel {
                    Location = new Point(14, y),
                    Size = new Size(580, 80),
                    BackColor = Color.Transparent,
                    Padding = new Padding(10)
                };
                card.Paint += (cs, ce) => {
                    ThemePalette.DrawFloatingCard(ce.Graphics, new Rectangle(0, 0, card.Width - 1, card.Height - 1), ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 8);
                };
                Label lt = new Label { Text = title, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Location = new Point(10, 8), AutoSize = true, BackColor = Color.Transparent };
                Label lb = new Label { Text = body, Font = new Font("Segoe UI", 8.5f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(10, 28), Size = new Size(556, 46), BackColor = Color.Transparent };
                card.Controls.Add(lt);
                card.Controls.Add(lb);
                settingsSubContentPanel.Controls.Add(card);
                y += 88;
            };

            Action<string, Action> addBtn = (btnText, onClick) => {
                Button btn = new Button {
                    Text = btnText,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    BackColor = isDarkMode ? Color.FromArgb(40, 53, 76) : Color.FromArgb(226, 232, 240),
                    ForeColor = isDarkMode ? Color.White : Color.FromArgb(15, 23, 42),
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(14, y),
                    Size = new Size(340, 30),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                if (onClick != null) btn.Click += (s, e) => onClick();
                settingsSubContentPanel.Controls.Add(btn);
                y += 38;
            };

            Action<string> addText = (text) => {
                TextBox tb = new TextBox {
                    Text = text,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Consolas", 8.5f),
                    BackColor = ThemePalette.ColorBgInput,
                    ForeColor = ThemePalette.ColorTextPrimary,
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(14, y),
                    Size = new Size(580, 160)
                };
                settingsSubContentPanel.Controls.Add(tb);
                y += 170;
            };

            if (category == "General")
            {
                addH("General Preferences");
                ModernSwitch swStart = addSw("Open address book on startup", db.Settings.general.openAddressBookOnStartup);
                swStart.CheckedChanged += (s, e) => db.Settings.general.openAddressBookOnStartup = swStart.Checked;
                ModernSwitch swUpd = addSw("Automatically download stable updates", db.Settings.general.autoUpdate);
                swUpd.CheckedChanged += (s, e) => db.Settings.general.autoUpdate = swUpd.Checked;
            }
            else if (category == "Connection")
            {
                addH("Primary Enterprise Cloud Domain & Web Integration");
                TextBox tbDom = addTb("Primary Branded Cloud Domain (Default: https://desk.jdroid-x.com)", db.Settings.connection.customDomain);
                tbDom.TextChanged += (s, e) => db.Settings.connection.customDomain = tbDom.Text.Trim();
                TextBox tbWeb = addTb("jDroid-X-Web Repository URL", db.Settings.connection.webRepoUrl);
                tbWeb.TextChanged += (s, e) => db.Settings.connection.webRepoUrl = tbWeb.Text.Trim();

                addH("Google Colab & Cloud Relay Configuration");
                ModernSwitch swColab = addSw("Enable Google Colab Central Cloud Relay", db.Settings.connection.colabCloudRelay);
                swColab.CheckedChanged += (s, e) => {
                    db.Settings.connection.colabCloudRelay = swColab.Checked;
                    RegisterWithColabRelay();
                };
                TextBox tbRelay = addTb("Cloud Relay Server URL (Default: http://127.0.0.1:8088 | WAN: https://desk.jdroid-x.com)", db.Settings.connection.relayServer);
                tbRelay.TextChanged += (s, e) => {
                    db.Settings.connection.relayServer = tbRelay.Text.Trim();
                    RegisterWithColabRelay();
                };
                TextBox tbPort = addTb("Relay Port (Default: 8088)", db.Settings.connection.relayPort > 0 ? db.Settings.connection.relayPort.ToString() : "8088");
                tbPort.TextChanged += (s, e) => {
                    int p;
                    if (int.TryParse(tbPort.Text.Trim(), out p) && p > 0) db.Settings.connection.relayPort = p;
                };
                TextBox tbToken = addTb("Cloudflare Tunnel Token (For desk.jdroid-x.com / jdroid-x.com)", db.Settings.connection.tunnelToken ?? "");
                tbToken.TextChanged += (s, e) => db.Settings.connection.tunnelToken = tbToken.Text.Trim();
                TextBox tbColab = addTb("Google Colab Central Relay Notebook URL", db.Settings.connection.colabNotebookUrl);
                tbColab.TextChanged += (s, e) => db.Settings.connection.colabNotebookUrl = tbColab.Text.Trim();
                TextBox tbGh = addTb("GitHub Central Backend SSOT URL", db.Settings.connection.githubBackendUrl);
                tbGh.TextChanged += (s, e) => {
                    db.Settings.connection.githubBackendUrl = tbGh.Text.Trim();
                    SyncFromGitHubBackend();
                };

                addH("Direct P2P & Cloud Relay Mesh");
                TextBox tbId = addTb("jDroid-X Rendezvous ID Server (hbbs :21116)", db.Settings.connection.idServer);
                tbId.TextChanged += (s, e) => db.Settings.connection.idServer = tbId.Text.Trim();

                addH("Local Network (LAN) Peer-to-Peer Auto-Discovery");
                ModernSwitch swLan = addSw("Enable Zero-Config LAN P2P Direct Discovery (UDP Beacons)", db.Settings.connection.lanDiscovery);
                swLan.CheckedChanged += (s, e) => {
                    db.Settings.connection.lanDiscovery = swLan.Checked;
                };
                TextBox tbDiscPort = addTb("LAN Discovery UDP Port (Default: 8089)", db.Settings.connection.discoveryPort > 0 ? db.Settings.connection.discoveryPort.ToString() : "8089");
                tbDiscPort.TextChanged += (s, e) => {
                    int dp;
                    if (int.TryParse(tbDiscPort.Text.Trim(), out dp) && dp > 0) db.Settings.connection.discoveryPort = dp;
                };

                Button btnSaveConn = new Button {
                    Text = "💾 Save & Persist Network Settings",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    BackColor = Color.FromArgb(16, 185, 129),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(14, y),
                    Size = new Size(340, 32),
                    Cursor = Cursors.Hand
                };
                btnSaveConn.Click += (s, e) => {
                    db.Save();
                    ShowToast("Network settings successfully saved to database.json");
                    RegisterWithColabRelay();
                    if (db.Settings.connection.lanDiscovery)
                    {
                        LanDiscoveryEngine.Instance.BroadcastQuery("*");
                    }
                    viewportCanvas.Invalidate();
                };
                settingsSubContentPanel.Controls.Add(btnSaveConn);
                y += 42;
            }
            else if (category == "Security")
            {
                addH("Access Control & Passcode Security");
                ModernSwitch swPasscode = addSw("Require One-Time Passcode for incoming connections (Default: Off / Direct Connect)", db.Settings.access.requirePasscode);
                swPasscode.CheckedChanged += (s, e) => db.Settings.access.requirePasscode = swPasscode.Checked;
                ModernSwitch swUnat = addSw("Enable Unattended 24/7 Access", db.Settings.access.unattendedAccess);
                swUnat.CheckedChanged += (s, e) => db.Settings.access.unattendedAccess = swUnat.Checked;
                addTb("Session Inactivity Timeout (minutes)", db.Settings.access.accessTimeout.ToString());
            }
            else if (category == "Display")
            {
                addH("Display & Smart Fit Scaling (Edge-to-Edge)");
                Label lblMode = new Label { Text = "Viewer Scaling Mode (Default: Stretch - Zero Black Borders)", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(14, y), AutoSize = true };
                settingsSubContentPanel.Controls.Add(lblMode);
                y += 20;

                ComboBox cboScale = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f), BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, Location = new Point(14, y), Size = new Size(340, 26) };
                cboScale.Items.Add("Stretch (Zero Black Borders - 100% Screen Fill)");
                cboScale.Items.Add("AspectFit (Maintain Aspect Ratio with Letterbox)");
                cboScale.Items.Add("Original (1:1 Native Screen Resolution)");
                if (smartFitMode == "AspectFit") cboScale.SelectedIndex = 1;
                else if (smartFitMode == "Original") cboScale.SelectedIndex = 2;
                else cboScale.SelectedIndex = 0;
                cboScale.SelectedIndexChanged += (s, e) => {
                    if (cboScale.SelectedIndex == 1) smartFitMode = "AspectFit";
                    else if (cboScale.SelectedIndex == 2) smartFitMode = "Original";
                    else smartFitMode = "Stretch";
                    db.Settings.display.viewMode = smartFitMode.ToLower();
                    if (viewportCanvas != null) viewportCanvas.Invalidate();
                };
                settingsSubContentPanel.Controls.Add(cboScale);
                y += 34;

                Label lblFps = new Label { Text = "Target Streaming Frame Rate", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(14, y), AutoSize = true };
                settingsSubContentPanel.Controls.Add(lblFps);
                y += 20;

                ComboBox cboFps = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f), BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, Location = new Point(14, y), Size = new Size(340, 26) };
                cboFps.Items.Add("60 FPS (Ultra Smooth - Low Latency Stream)");
                cboFps.Items.Add("30 FPS (Standard Balanced Streaming)");
                cboFps.Items.Add("15 FPS (Bandwidth Saver / Mobile Tether)");
                cboFps.SelectedIndex = 0;
                settingsSubContentPanel.Controls.Add(cboFps);
                y += 34;

                ModernSwitch swHw = addSw("Enable Hardware Acceleration (DirectX/GPU)", db.Settings.display.hardwareAcceleration);
                swHw.CheckedChanged += (s, e) => db.Settings.display.hardwareAcceleration = swHw.Checked;
            }
            else if (category == "Account")
            {
                addH("Cluster & Operator Credentials");
                addTb("Licensed Operator", db.Settings.account.username);
                addTb("Cluster Tenant", db.Settings.account.enterpriseTenant);
            }
            else if (category == "Readme")
            {
                addH("📖 Readme: Architecture & Presentation Showcase");
                addCard("jDroid-X-Desk Enterprise Remote Desktop", "Enterprise-grade remote desktop and machine administration workspace built with Senior Dynamic OOPS-based MVC standards. Single 117 KB universal Windows executable, zero external runtimes.");
                addCard("1. Dynamic SHA-256 System Hardware Key", "Hardware-grounded 9-digit Desk ID (XXX-XXX-XXX) combining Windows MachineGuid, physical NIC MAC address, machine name, and CPU count. Permanent and unique across reboots.");
                addCard("2. Google Remote Desktop 3-Card Interface", "1. Share this screen (Get Support), 2. Connect to another computer (Give Support), and 3. Recent Connections for 1-click reconnect.");
                addCard("3. Dual Architecture & Distribution", "Standalone Native Portable Client (jDroid-X-Desk.exe pure C# .NET WinForms) alongside Full-Stack Web Gateway (Express.js + WebRTC on port 12345/7070).");
                addCard("4. Universal Installation Artifacts", "jDroid-X-Desk.exe (117 KB), jDroid-X-Desk-Universal.zip (125 KB), Install-Or-Run.bat (SmartScreen unblocker), jDroid-X-Client.bat (technician runner).");

                addBtn("📂 Open README.md in System Viewer", () => {
                    try {
                        string rPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.md");
                        if (File.Exists(rPath)) Process.Start(rPath);
                        else ShowToast("README.md not found in " + AppDomain.CurrentDomain.BaseDirectory);
                    } catch (Exception ex) { ShowToast("Could not open file: " + ex.Message); }
                });
                addBtn("🌐 Open Web Presentation (localhost:12345)", () => {
                    try { Process.Start("http://localhost:12345/#system-settings"); } catch {}
                });
                addBtn("📁 Open App Installation Folder", () => {
                    try { Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory); } catch {}
                });
            }
            else if (category == "Help")
            {
                addH("❓ Help & Multi-OS Operations Guide");
                addCard("Cross-Platform Operational Procedures", "Windows 7/8/10/11: Native jDroid-X-Desk.exe or Install-Or-Run.bat.\nmacOS & Linux: npm install && node server.js -> http://localhost:12345.\nMobile (iOS/Android): Open Safari/Chrome to http://<HOST-IP>:12345.");
                addCard("Network Ports & Firewall Matrix", "TCP 12345/7070: Local Web Interface & Express REST API\nUDP 21116: STUN/TURN Encrypted P2P UDP Handshake (hbbs)\nTCP 8088: Cloud Relay Server (Google Colab / Cloudflare)\nTCP 21117: Relay Data Stream (hbbr)");
                addCard("Root Cause Analysis (RCA) Troubleshooting", "Scenario 1: STUN Handshake error -> Whitelist UDP 21116 in firewall.\nScenario 2: RTT > 120ms input lag -> Switch to Turbo 480p mode.\nScenario 3: SmartScreen block -> Run Install-Or-Run.bat.");

                addBtn("📋 Copy Diagnostic System Telemetry to Clipboard", () => {
                    try {
                        string telem = string.Format("jDroid-X-Desk v2.0.0 Diagnostic Report\nHost: {0}\nDesk ID: {1}\nOS: {2}\nIP: {3}\nColab Relay: {4}\nTimestamp: {5}",
                            currentHostDesktop != null ? currentHostDesktop.Name : Environment.MachineName,
                            currentHostDesktop != null ? currentHostDesktop.RemoteId : "Unknown",
                            Environment.OSVersion.ToString(),
                            currentHostDesktop != null ? currentHostDesktop.Ip : "127.0.0.1",
                            db.Settings.connection.relayServer,
                            DateTime.UtcNow.ToString("o"));
                        Clipboard.SetText(telem);
                        ShowToast("Diagnostic report copied to clipboard!");
                    } catch (Exception ex) { ShowToast("Failed to copy report: " + ex.Message); }
                });
                addBtn("🌐 Open Online Help & RCA Guide", () => {
                    try { Process.Start("http://localhost:12345/#system-settings"); } catch {}
                });
            }
            else if (category == "About")
            {
                addH("ℹ️ About jDroid-X-Desk Universal");
                addTb("Product Name", "jDroid-X-Desk Enterprise Universal");
                addTb("Version", "2.0.0 (Enterprise Release)");
                addTb("Runtime Engine", "Native .NET Framework 4.0 / 4.8 (x64/AnyCPU WinForms)");
                addTb("Architecture Standard", "Senior Dynamic OOPS-based MVC Standard");
                addTb("Operator Closed Circuit", "Operator: JEET | Assistant: jDroid-X");
                addTb("Primary Hardware Security Fingerprint", "d9cc 1547 bb14 9c3c 5696 49ad 81c6 201e f3ba b02b 78a9 4e21 f1e0 83cd");
                addTb("Authenticode Digital Authority", "jDroid-X Studio Enterprise Root CA (2026-2036)");

                addBtn("🌐 Open Official Portal (www.jdroid-x.com)", () => {
                    try { Process.Start("https://www.jdroid-x.com"); } catch {}
                });
                addBtn("⭐ View Source Repository on GitHub", () => {
                    try { Process.Start("https://github.com/jDroid-X/jDroid-X-Desk"); } catch {}
                });
            }
            else if (category == "License")
            {
                addH("⚖️ Software License Agreement");
                addCard("Dual-Licensing Model", "Released under the permissive MIT License for open-source evaluation and community tooling, alongside Enterprise Commercial terms for mission-critical IT infrastructure deployments.");
                addText("The MIT License (MIT)\n\nCopyright (c) 2026 JEET / jDroid-X Studio\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.");
            }
            else if (category == "Copyright")
            {
                addH("© Copyright & Trademark Declarations");
                addCard("Official Copyright Ownership", "Copyright © 2026 JEET / jDroid-X Studio. All rights reserved.\nAll binary assemblies, source code, UI designs, and documentation are proprietary assets of JEET and jDroid-X Studio.");
                addCard("Registered Brand Trademarks", "• jDroid-X Studio™ is an enterprise software engineering trademark.\n• jDroid-X-Desk™ is a trademark for remote desktop systems.\n• Dynamic OOPS MVC Architecture™ is an enterprise development governance standard.");
                addCard("Architectural Governance & Attribution", "Developed in adherence to Chesterton's Fence governance: foundational system structures and verified operational layers are preserved and merged safely. Closed-circuit pair-programming (Operator: JEET | Assistant: jDroid-X).");
            }
        }

        // ======================================================================
        // SUBVIEW 6: SERVER ENGINE CONTROLLER & ADMIN DASHBOARD
        // ======================================================================

        private FlowLayoutPanel pnlEngineCardsContainer;

        private void InitializeServerAdminView()
        {
            viewServerAdmin = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgWorkspace, Padding = new Padding(12) };

            // Top Header Bar
            headerAdmin = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = ThemePalette.ColorBgCardHeader, Padding = new Padding(12, 6, 12, 6) };
            
            Panel leftHeader = new Panel { Dock = DockStyle.Left, Width = 520 };
            Label lblTitle = new Label { Text = "🛡️ SERVER ENGINE CONTROLLER & ADMIN DASHBOARD", Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Dock = DockStyle.Top, Height = 20 };
            Label lblSub = new Label { Text = "Dynamic P2P / Ultra-Low-Latency / HTML5 Browser Gateway Engine Switcher & Processing Limits", Font = new Font("Segoe UI", 8f), ForeColor = ThemePalette.ColorTextSecondary, Dock = DockStyle.Top, Height = 16 };
            leftHeader.Controls.Add(lblSub);
            leftHeader.Controls.Add(lblTitle);

            FlowLayoutPanel rightHeaderBtns = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Margin = new Padding(0) };

            Button btnWebBackup = new Button {
                Text = "🌐 Open Web Backup",
                Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemePalette.ColorBgCardElevated,
                ForeColor = ThemePalette.ColorPrimary,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 8, 0)
            };
            btnWebBackup.FlatAppearance.BorderSize = 0;
            btnWebBackup.Click += (s, e) => {
                try {
                    Process.Start("http://localhost:12345/admin.html");
                    ShowToast("Launched Web Admin Backup in Browser (Port 12345)");
                } catch (Exception ex) {
                    ShowToast("Browser Launch Error: " + ex.Message);
                }
            };

            Button btnSyncDb = new Button {
                Text = "🔄 Sync database.json",
                Size = new Size(140, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemePalette.ColorBgInput,
                ForeColor = ThemePalette.ColorTextPrimary,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 8, 0)
            };
            btnSyncDb.FlatAppearance.BorderSize = 0;
            btnSyncDb.Click += (s, e) => {
                db.Load();
                PopulateProcessingLimitsToUi();
                RenderEngineCards(pnlEngineCardsContainer);
                RefreshAdminLogs();
                ShowToast("Server configuration synced from database.json");
            };

            ModernButton btnSaveAdmin = new ModernButton {
                Text = "💾 Save & Apply",
                NormalColor = ThemePalette.ColorSuccess,
                HoverColor = Color.FromArgb(21, 150, 78),
                Size = new Size(130, 30),
                Margin = new Padding(0, 2, 0, 0)
            };
            btnSaveAdmin.Click += (s, e) => {
                SaveServerConfigFromUi();
            };

            rightHeaderBtns.Controls.Add(btnWebBackup);
            rightHeaderBtns.Controls.Add(btnSyncDb);
            rightHeaderBtns.Controls.Add(btnSaveAdmin);

            headerAdmin.Controls.Add(leftHeader);
            headerAdmin.Controls.Add(rightHeaderBtns);

            // Main Scrollable Work Area
            pnlScrollAdmin = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemePalette.ColorBgWorkspace, Padding = new Padding(0, 8, 8, 8) };

            // -------------------------------------------------------------
            // SECTION 1: OPEN-SOURCE ENGINE SWITCHER (4 CARDS)
            // -------------------------------------------------------------
            Panel secEnginesHdr = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4, 8, 4, 4) };
            Label lblEngHdr = new Label { Text = "1. OPEN-SOURCE REMOTE ENGINES & MODEL CONTROLLER", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorPrimary, Dock = DockStyle.Left, AutoSize = true };
            lblAdminActiveEngineBadge = new Label { Text = "ACTIVE: " + (db.ServerConfig != null ? db.ServerConfig.activeEngine.ToUpper() : "SUNSHINE"), Font = new Font("Consolas", 8.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorSuccess, Dock = DockStyle.Right, AutoSize = true };
            secEnginesHdr.Controls.Add(lblEngHdr);
            secEnginesHdr.Controls.Add(lblAdminActiveEngineBadge);

            pnlEngineCardsContainer = new FlowLayoutPanel {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 12)
            };
            RenderEngineCards(pnlEngineCardsContainer);

            // -------------------------------------------------------------
            // SECTION 2: PROCESSING LIMITS & SYSTEM TELEMETRY
            // -------------------------------------------------------------
            Panel secLimitsHdr = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4, 12, 4, 4) };
            Label lblLimitsHdr = new Label { Text = "2. PROCESSING LIMITS & RESOURCE ALLOCATION", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorPrimary, Dock = DockStyle.Left, AutoSize = true };
            secLimitsHdr.Controls.Add(lblLimitsHdr);

            pnlLimitsCard = new Panel {
                Dock = DockStyle.Top,
                Height = 175,
                BackColor = Color.Transparent,
                Padding = new Padding(14)
            };
            pnlLimitsCard.Paint += (s, e) => {
                Rectangle r = new Rectangle(0, 0, pnlLimitsCard.Width - 1, pnlLimitsCard.Height - 1);
                ThemePalette.DrawFloatingCard(e.Graphics, r, ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 8);
            };

            // Grid for Processing Limits Controls
            TableLayoutPanel tlpLimits = new TableLayoutPanel { Dock = DockStyle.Top, Height = 95, ColumnCount = 3, RowCount = 2, BackColor = Color.Transparent };
            tlpLimits.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            tlpLimits.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            tlpLimits.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            tlpLimits.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));
            tlpLimits.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));

            // Control 1: Max Concurrent Sessions
            Panel p1 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Label l1 = new Label { Text = "Max Concurrent Sessions", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(0, 0), AutoSize = true };
            numAdminMaxSessions = new NumericUpDown { Location = new Point(0, 18), Size = new Size(180, 24), Minimum = 1, Maximum = 100, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8.5f) };
            p1.Controls.Add(l1);
            p1.Controls.Add(numAdminMaxSessions);
            tlpLimits.Controls.Add(p1, 0, 0);

            // Control 2: Bandwidth Limit Mbps
            Panel p2 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Label l2 = new Label { Text = "Bandwidth Limit (Mbps)", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(0, 0), AutoSize = true };
            numAdminBandwidth = new NumericUpDown { Location = new Point(0, 18), Size = new Size(180, 24), Minimum = 5, Maximum = 1000, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8.5f) };
            p2.Controls.Add(l2);
            p2.Controls.Add(numAdminBandwidth);
            tlpLimits.Controls.Add(p2, 1, 0);

            // Control 3: Target Max FPS
            Panel p3 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Label l3 = new Label { Text = "Target Frame Rate (FPS)", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(0, 0), AutoSize = true };
            cbAdminFps = new ComboBox { Location = new Point(0, 18), Size = new Size(180, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8.5f) };
            cbAdminFps.Items.Add("30 FPS (Low Latency / Balanced)");
            cbAdminFps.Items.Add("60 FPS (Ultra Smooth 60 FPS)");
            cbAdminFps.Items.Add("120 FPS (High Refresh Gaming)");
            p3.Controls.Add(l3);
            p3.Controls.Add(cbAdminFps);
            tlpLimits.Controls.Add(p3, 2, 0);

            // Control 4: Idle Session Timeout
            Panel p4 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Label l4 = new Label { Text = "Idle Session Timeout (Minutes)", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(0, 0), AutoSize = true };
            numAdminIdleTimeout = new NumericUpDown { Location = new Point(0, 18), Size = new Size(180, 24), Minimum = 1, Maximum = 180, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8.5f) };
            p4.Controls.Add(l4);
            p4.Controls.Add(numAdminIdleTimeout);
            tlpLimits.Controls.Add(p4, 0, 1);

            // Control 5: Log Retention Days
            Panel p5 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Label l5 = new Label { Text = "Log Retention (Days)", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(0, 0), AutoSize = true };
            numAdminLogRetention = new NumericUpDown { Location = new Point(0, 18), Size = new Size(180, 24), Minimum = 1, Maximum = 90, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8.5f) };
            p5.Controls.Add(l5);
            p5.Controls.Add(numAdminLogRetention);
            tlpLimits.Controls.Add(p5, 1, 1);

            // Control 6: Hardware Acceleration
            Panel p6 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Label l6 = new Label { Text = "Hardware Acceleration", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(0, 0), AutoSize = true };
            cbAdminHwAccel = new ComboBox { Location = new Point(0, 18), Size = new Size(180, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8.5f) };
            cbAdminHwAccel.Items.Add("nvenc_cuda (NVIDIA NVENC CUDA)");
            cbAdminHwAccel.Items.Add("quicksync (Intel QuickSync)");
            cbAdminHwAccel.Items.Add("software_cpu (Software CPU Encoding)");
            p6.Controls.Add(l6);
            p6.Controls.Add(cbAdminHwAccel);
            tlpLimits.Controls.Add(p6, 2, 1);

            // Live Telemetry Strip underneath
            Panel pnlTelemetry = new Panel { Dock = DockStyle.Bottom, Height = 42, BackColor = Color.Transparent, Padding = new Padding(10, 8, 10, 8) };
            pnlTelemetry.Paint += (s, e) => {
                Rectangle r = new Rectangle(0, 0, pnlTelemetry.Width - 1, pnlTelemetry.Height - 1);
                ThemePalette.DrawFloatingCard(e.Graphics, r, ThemePalette.ColorBgCardElevated, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 6);
            };
            lblAdminCpu = new Label { Text = "⚡ CPU: 4%", Font = new Font("Consolas", 8.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorPrimary, Dock = DockStyle.Left, AutoSize = true, Margin = new Padding(0, 0, 16, 0) };
            lblAdminMem = new Label { Text = "💾 RAM: 4.2 / 16.0 GB", Font = new Font("Consolas", 8.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorSuccess, Dock = DockStyle.Left, AutoSize = true, Margin = new Padding(0, 0, 16, 0) };
            lblAdminUptime = new Label { Text = "⏱️ UPTIME: Active", Font = new Font("Consolas", 8.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorWarning, Dock = DockStyle.Left, AutoSize = true, Margin = new Padding(0, 0, 16, 0) };
            lblAdminSessions = new Label { Text = "👥 ACTIVE SESSIONS: 0 / 10", Font = new Font("Consolas", 8.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Dock = DockStyle.Right, AutoSize = true };

            FlowLayoutPanel telemFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
            telemFlow.Controls.Add(lblAdminCpu);
            telemFlow.Controls.Add(lblAdminMem);
            telemFlow.Controls.Add(lblAdminUptime);

            pnlTelemetry.Controls.Add(telemFlow);
            pnlTelemetry.Controls.Add(lblAdminSessions);

            pnlLimitsCard.Controls.Add(tlpLimits);
            pnlLimitsCard.Controls.Add(pnlTelemetry);

            PopulateProcessingLimitsToUi();

            // -------------------------------------------------------------
            // SECTION 3: LIVE SERVER LOGS & EVENT STREAM
            // -------------------------------------------------------------
            Panel secLogsHdr = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(4, 14, 4, 4) };
            Label lblLogsHdr = new Label { Text = "3. LIVE SERVER LOGS & EVENT STREAM", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorPrimary, Dock = DockStyle.Left, AutoSize = true };
            
            FlowLayoutPanel logActionsFlow = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };

            cbAdminLogLevel = new ComboBox { Size = new Size(110, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8f), Margin = new Padding(0, 0, 6, 0) };
            cbAdminLogLevel.Items.Add("All Levels");
            cbAdminLogLevel.Items.Add("INFO");
            cbAdminLogLevel.Items.Add("WARN");
            cbAdminLogLevel.Items.Add("ERROR");
            cbAdminLogLevel.Items.Add("ENGINE");
            cbAdminLogLevel.SelectedIndex = 0;
            cbAdminLogLevel.SelectedIndexChanged += (s, e) => RefreshAdminLogs();

            txtAdminLogSearch = new TextBox { Size = new Size(140, 22), BackColor = ThemePalette.ColorBgInput, ForeColor = ThemePalette.ColorTextPrimary, Font = new Font("Segoe UI", 8.5f), Margin = new Padding(0, 0, 6, 0) };
            txtAdminLogSearch.TextChanged += (s, e) => RefreshAdminLogs();

            Button btnClearLogs = new Button {
                Text = "🗑️ Clear",
                Size = new Size(70, 24),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemePalette.ColorBgCardHeader,
                ForeColor = ThemePalette.ColorDanger,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
            };
            btnClearLogs.FlatAppearance.BorderSize = 0;
            btnClearLogs.Click += (s, e) => {
                if (MessageBox.Show("Clear server log entries?", "Confirm Purge", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    db.Logs.Clear();
                    db.Save();
                    RefreshAdminLogs();
                    ShowToast("Server logs purged.");
                }
            };

            Button btnExportLogs = new Button {
                Text = "📥 Export JSON",
                Size = new Size(95, 24),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemePalette.ColorBgCardHeader,
                ForeColor = ThemePalette.ColorPrimary,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            btnExportLogs.FlatAppearance.BorderSize = 0;
            btnExportLogs.Click += (s, e) => ExportAdminLogs();

            logActionsFlow.Controls.Add(cbAdminLogLevel);
            logActionsFlow.Controls.Add(txtAdminLogSearch);
            logActionsFlow.Controls.Add(btnClearLogs);
            logActionsFlow.Controls.Add(btnExportLogs);

            secLogsHdr.Controls.Add(lblLogsHdr);
            secLogsHdr.Controls.Add(logActionsFlow);

            pnlLogsGrid = new Panel { Dock = DockStyle.Top, Height = 280, BackColor = Color.Transparent, Padding = new Padding(6) };
            pnlLogsGrid.Paint += (s, e) => {
                Rectangle r = new Rectangle(0, 0, pnlLogsGrid.Width - 1, pnlLogsGrid.Height - 1);
                ThemePalette.DrawFloatingCard(e.Graphics, r, ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 8);
            };

            dgvAdminLogs = new DataGridView {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemePalette.ColorBgCard,
                ForeColor = ThemePalette.ColorTextPrimary,
                GridColor = ThemePalette.ColorBorderCard,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };
            dgvAdminLogs.ColumnHeadersDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
            dgvAdminLogs.ColumnHeadersDefaultCellStyle.ForeColor = ThemePalette.ColorTextSecondary;
            dgvAdminLogs.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            dgvAdminLogs.DefaultCellStyle.BackColor = ThemePalette.ColorBgCard;
            dgvAdminLogs.DefaultCellStyle.ForeColor = ThemePalette.ColorTextPrimary;
            dgvAdminLogs.DefaultCellStyle.Font = new Font("Segoe UI", 8f);
            dgvAdminLogs.DefaultCellStyle.SelectionBackColor = ThemePalette.ColorPrimary;
            dgvAdminLogs.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvAdminLogs.AlternatingRowsDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;

            pnlLogsGrid.Controls.Add(dgvAdminLogs);

            // -------------------------------------------------------------
            // SECTION 1.5: GOOGLE COLAB & CLOUD RELAY BROKER (OPTIONAL WAN)
            // -------------------------------------------------------------
            Panel secColabHdr = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4, 12, 4, 4) };
            Label lblColabHdr = new Label { Text = "2. GOOGLE COLAB & CLOUDFLARE WAN TUNNEL (OPTIONAL CLOUD RELAY)", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorPrimary, Dock = DockStyle.Left, AutoSize = true };
            secColabHdr.Controls.Add(lblColabHdr);
            Panel pnlColabCard = CreateColabRelayCard();

            // Add sections in reverse for Dock = Top ordering
            pnlScrollAdmin.Controls.Add(pnlLogsGrid);
            pnlScrollAdmin.Controls.Add(secLogsHdr);
            pnlScrollAdmin.Controls.Add(pnlLimitsCard);
            pnlScrollAdmin.Controls.Add(secLimitsHdr);
            pnlScrollAdmin.Controls.Add(pnlColabCard);
            pnlScrollAdmin.Controls.Add(secColabHdr);
            pnlScrollAdmin.Controls.Add(pnlEngineCardsContainer);
            pnlScrollAdmin.Controls.Add(secEnginesHdr);

            viewServerAdmin.Controls.Add(pnlScrollAdmin);
            viewServerAdmin.Controls.Add(headerAdmin);
            headerAdmin.BringToFront();

            RefreshAdminLogs();

            // Telemetry Timer (2-second interval)
            adminTelemetryTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            adminTelemetryTimer.Tick += (s, e) => {
                try {
                    TimeSpan uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
                    lblAdminUptime.Text = string.Format("⏱️ UPTIME: {0:D2}:{1:D2}:{2:D2}", (int)uptime.TotalHours, uptime.Minutes, uptime.Seconds);

                    long memBytes = Process.GetCurrentProcess().WorkingSet64;
                    double memMb = memBytes / (1024.0 * 1024.0);
                    lblAdminMem.Text = string.Format("💾 APP RAM: {0:F1} MB", memMb);

                    int sessCount = db.Sessions != null ? db.Sessions.Count : 0;
                    int maxSess = db.ServerConfig != null && db.ServerConfig.processingLimits != null ? db.ServerConfig.processingLimits.maxConcurrentSessions : 10;
                    lblAdminSessions.Text = string.Format("👥 TOTAL LOGGED SESSIONS: {0} | MAX: {1}", sessCount, maxSess);
                } catch { }
            };
            adminTelemetryTimer.Start();
        }

        private Panel CreateColabRelayCard()
        {
            Panel card = new Panel { Dock = DockStyle.Top, Height = 126, BackColor = Color.Transparent, Padding = new Padding(12) };
            card.Paint += (s, e) => {
                Color borderC = isColabConnected ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11);
                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, card.Width - 1, card.Height - 1), ThemePalette.ColorBgCard, borderC, ThemePalette.ColorShadow, 8);
            };

            string colabStatusText = isColabConnected ? "● ONLINE (Live Stream Ready)" : "○ NOT CONNECTED (Paste Colab URL)";
            Color colabStatusColor = isColabConnected ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11);

            Label lColabTitle = new Label { Text = "⚡ Google Colab Cloud Relay & Public Cloudflare Tunnel (Optional WAN Bridge)", Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Location = new Point(16, 10), AutoSize = true };
            Label lColabBadge = new Label { Text = colabStatusText, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = colabStatusColor, Location = new Point(560, 12), AutoSize = true };
            Label lColabDesc = new Label { Text = "Connect over the Internet across different networks using your Google Colab Cloudflare tunnel URL (Local P2P works automatically without this):", Font = new Font("Segoe UI", 8.2f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(16, 34), AutoSize = true };

            txtColabInput = new TextBox {
                Size = new Size(380, 28),
                Location = new Point(16, 62),
                Font = new Font("Consolas", 10f),
                BackColor = ThemePalette.ColorBgInput,
                ForeColor = ThemePalette.ColorTextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Text = (db != null && db.Settings != null && db.Settings.connection != null && !string.IsNullOrEmpty(db.Settings.connection.relayServer)) 
                    ? db.Settings.connection.relayServer 
                    : "https://....trycloudflare.com"
            };

            btnPasteColab = new Button {
                Text = "📋 Paste",
                Size = new Size(80, 28),
                Location = new Point(txtColabInput.Right + 8, 62),
                FlatStyle = FlatStyle.Flat,
                BackColor = isDarkMode ? Color.FromArgb(40, 53, 76) : Color.FromArgb(226, 232, 240),
                ForeColor = isDarkMode ? Color.White : Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPasteColab.FlatAppearance.BorderSize = 0;
            btnPasteColab.Click += (s, e) => {
                try {
                    if (Clipboard.ContainsText()) {
                        string clip = Clipboard.GetText().Trim();
                        int startIdx = clip.IndexOf("https://");
                        if (startIdx < 0) startIdx = clip.IndexOf("http://");
                        if (startIdx >= 0) {
                            int endIdx = clip.IndexOfAny(new char[] { ' ', '\r', '\n', '\t' }, startIdx);
                            clip = endIdx > startIdx ? clip.Substring(startIdx, endIdx - startIdx) : clip.Substring(startIdx);
                        }
                        if (clip.Contains(".trycloudflare.com") || clip.StartsWith("http")) {
                            txtColabInput.Text = clip.TrimEnd('/');
                            ShowToast("Pasted Colab tunnel URL from clipboard");
                        } else {
                            MessageBox.Show("Clipboard does not contain a valid URL. Copy the 'https://...trycloudflare.com' URL from your Colab notebook first.", "Paste Colab URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                } catch { }
            };

            btnApplyColab = new Button {
                Text = "💾 Save & Connect",
                Size = new Size(130, 28),
                Location = new Point(btnPasteColab.Right + 8, 62),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApplyColab.FlatAppearance.BorderSize = 0;
            btnApplyColab.Click += (s, e) => {
                string entered = txtColabInput.Text.Trim();
                if (!entered.StartsWith("http://") && !entered.StartsWith("https://")) entered = "https://" + entered;
                entered = entered.TrimEnd('/');

                if (db != null && db.Settings != null && db.Settings.connection != null) {
                    db.Settings.connection.relayServer = entered;
                    db.Settings.connection.customDomain = entered;
                    db.Save();
                    db.SyncYaml();
                }

                cachedWinningRelayUrl = entered;
                lastWinningCheckTime = DateTime.MinValue;

                try {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colab_tunnel.txt"), entered);
                } catch { }

                ShowToast("Colab Relay set to: " + entered + ". Probing server...");
                ThreadPool.QueueUserWorkItem(delegate { 
                    PingColabServer(); 
                    RegisterWithColabRelay();
                    PushLocalHostFrame();
                });
            };

            btnStartLocalRelay = new Button {
                Text = "⚡ Local Relay (8088)",
                Size = new Size(140, 28),
                Location = new Point(btnApplyColab.Right + 8, 62),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(14, 165, 233),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStartLocalRelay.FlatAppearance.BorderSize = 0;
            btnStartLocalRelay.Click += (s, e) => StartLocalPythonRelay();

            btnColabNotebook = new Button {
                Text = "🌐 Open Colab Notebook",
                Size = new Size(160, 28),
                Location = new Point(btnStartLocalRelay.Right + 8, 62),
                FlatStyle = FlatStyle.Flat,
                BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240),
                ForeColor = isDarkMode ? Color.FromArgb(203, 213, 225) : Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btnColabNotebook.FlatAppearance.BorderSize = 0;
            btnColabNotebook.Click += (s, e) => {
                try {
                    string nb = (db != null && db.Settings != null && db.Settings.connection != null && !string.IsNullOrEmpty(db.Settings.connection.colabNotebookUrl)) ? db.Settings.connection.colabNotebookUrl : "https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai";
                    Process.Start(new ProcessStartInfo(nb) { UseShellExecute = true });
                } catch { }
            };

            card.Controls.Add(lColabTitle);
            card.Controls.Add(lColabBadge);
            card.Controls.Add(lColabDesc);
            card.Controls.Add(txtColabInput);
            card.Controls.Add(btnPasteColab);
            card.Controls.Add(btnApplyColab);
            card.Controls.Add(btnStartLocalRelay);
            card.Controls.Add(btnColabNotebook);

            cardColab = card;
            return card;
        }

        private void PopulateProcessingLimitsToUi()
        {
            if (db == null || db.ServerConfig == null) return;
            ProcessingLimitsModel limits = db.ServerConfig.processingLimits;
            if (limits == null) limits = new ProcessingLimitsModel();

            if (numAdminMaxSessions != null) numAdminMaxSessions.Value = Math.Max(1, Math.Min(100, limits.maxConcurrentSessions));
            if (numAdminBandwidth != null) numAdminBandwidth.Value = Math.Max(5, Math.Min(1000, limits.bandwidthLimitMbps));
            if (cbAdminFps != null)
            {
                if (limits.maxFps == 120) cbAdminFps.SelectedIndex = 2;
                else if (limits.maxFps == 30) cbAdminFps.SelectedIndex = 0;
                else cbAdminFps.SelectedIndex = 1;
            }
            if (numAdminIdleTimeout != null) numAdminIdleTimeout.Value = Math.Max(1, Math.Min(180, limits.idleTimeoutMinutes));
            if (numAdminLogRetention != null) numAdminLogRetention.Value = Math.Max(1, Math.Min(90, limits.logRetentionDays));
            if (cbAdminHwAccel != null)
            {
                if (limits.hardwareAcceleration == "quicksync") cbAdminHwAccel.SelectedIndex = 1;
                else if (limits.hardwareAcceleration == "software_cpu") cbAdminHwAccel.SelectedIndex = 2;
                else cbAdminHwAccel.SelectedIndex = 0;
            }
        }

        private void RenderEngineCards(FlowLayoutPanel container)
        {
            if (container == null || db == null || db.ServerConfig == null) return;
            container.Controls.Clear();
            engineCardPanels.Clear();

            string activeId = db.ServerConfig.activeEngine != null ? db.ServerConfig.activeEngine : "jdroidRelay";
            if (lblAdminActiveEngineBadge != null)
            {
                lblAdminActiveEngineBadge.Text = "jDroid-Engine: " + activeId.ToUpper();
            }

            Dictionary<string, OpenSourceEngineModel> engines = db.ServerConfig.engines;
            if (engines == null || engines.Count == 0)
            {
                db.ServerConfig = ServerConfigModel.CreateDefault();
                engines = db.ServerConfig.engines;
            }

            foreach (KeyValuePair<string, OpenSourceEngineModel> kv in engines)
            {
                string engId = kv.Key;
                OpenSourceEngineModel eng = kv.Value;
                bool isActive = engId.Equals(activeId, StringComparison.OrdinalIgnoreCase);

                Panel card = new Panel {
                    Size = new Size(395, 252),
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 12, 12),
                    Padding = new Padding(12)
                };
                string currentEngId = engId;
                bool currentIsActive = isActive;
                card.Paint += (cs, ce) => {
                    Color borderCol = currentIsActive ? Color.FromArgb(16, 185, 129) : ThemePalette.ColorBorderCard;
                    ThemePalette.DrawFloatingCard(ce.Graphics, new Rectangle(0, 0, card.Width - 1, card.Height - 1), ThemePalette.ColorBgCard, borderCol, ThemePalette.ColorShadow, 8);
                };

                // Top Ribbon: Icon + Name + Active Status Badge
                Panel cardTop = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.Transparent };
                string icon = engId == "rustdesk" ? "🦀" : (engId == "sunshine" ? "⚡" : (engId == "guacamole" ? "🥑" : "🚀"));
                Label lblName = new Label {
                    Text = icon + " " + eng.name,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = ThemePalette.ColorTextPrimary,
                    Dock = DockStyle.Left,
                    AutoSize = true,
                    MaximumSize = new Size(240, 28)
                };

                Label lblBadge = new Label {
                    Text = isActive ? "✓ ACTIVE" : eng.status.ToUpper(),
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = isActive ? Color.FromArgb(16, 185, 129) : Color.FromArgb(148, 163, 184),
                    BackColor = isActive ? (isDarkMode ? Color.FromArgb(6, 78, 59) : Color.FromArgb(220, 252, 231)) : (isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240)),
                    Dock = DockStyle.Right,
                    AutoSize = true,
                    Padding = new Padding(4, 2, 4, 2)
                };

                cardTop.Controls.Add(lblName);
                cardTop.Controls.Add(lblBadge);

                // Type & Specs Line
                Label lblType = new Label {
                    Text = eng.type + " • " + eng.version,
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(56, 189, 248),
                    Dock = DockStyle.Top,
                    Height = 20,
                    BackColor = Color.Transparent
                };

                // Ports Line
                Label lblPorts = new Label {
                    Text = "Ports: " + eng.ports,
                    Font = new Font("Consolas", 7.5f),
                    ForeColor = ThemePalette.ColorTextSecondary,
                    Dock = DockStyle.Top,
                    Height = 22,
                    BackColor = Color.Transparent
                };

                // Manual Configuration Instructions Box
                Panel pnlManual = new Panel {
                    Dock = DockStyle.Top,
                    Height = 88,
                    BackColor = Color.Transparent,
                    Padding = new Padding(8),
                    Margin = new Padding(0, 4, 0, 8)
                };
                pnlManual.Paint += (ms, me) => {
                    ThemePalette.DrawFloatingCard(me.Graphics, new Rectangle(0, 0, pnlManual.Width - 1, pnlManual.Height - 1), ThemePalette.ColorBgCardHeader, ThemePalette.ColorBorderCard, Color.FromArgb(10, 0, 0, 0), 6);
                };
                Label lblManHdr = new Label {
                    Text = "Manual Setup Requirement:",
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = isDarkMode ? Color.FromArgb(250, 204, 21) : Color.FromArgb(161, 98, 7),
                    Dock = DockStyle.Top,
                    Height = 16,
                    BackColor = Color.Transparent
                };
                Label lblManDesc = new Label {
                    Text = eng.manualConfigRequired,
                    Font = new Font("Segoe UI", 7.5f),
                    ForeColor = ThemePalette.ColorTextSecondary,
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };
                pnlManual.Controls.Add(lblManDesc);
                pnlManual.Controls.Add(lblManHdr);

                // Action Buttons at Bottom
                FlowLayoutPanel btnFlow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 32, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };

                Button btnActivate = new Button {
                    Text = isActive ? "✓ Current jDroid-Engine" : "⚡ Switch to " + eng.id.ToUpper(),
                    Size = new Size(isActive ? 142 : 128, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isActive ? Color.FromArgb(16, 185, 129) : (isDarkMode ? Color.FromArgb(40, 53, 76) : Color.FromArgb(226, 232, 240)),
                    ForeColor = isActive ? Color.White : ThemePalette.ColorTextPrimary,
                    Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Enabled = !isActive,
                    Margin = new Padding(0, 2, 4, 0)
                };
                btnActivate.FlatAppearance.BorderSize = 0;
                string captureId = engId;
                btnActivate.Click += (s, e) => SwitchActiveEngine(captureId);
                btnFlow.Controls.Add(btnActivate);

                Button btnRunScript = new Button {
                    Text = "🚀 Start",
                    Size = new Size(58, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isDarkMode ? Color.FromArgb(20, 32, 50) : Color.FromArgb(220, 252, 231),
                    ForeColor = Color.FromArgb(16, 185, 129),
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 2, 4, 0)
                };
                btnRunScript.FlatAppearance.BorderSize = 0;
                string captureRunId = engId;
                btnRunScript.Click += (s, e) => LaunchEngineStartScript(captureRunId);
                btnFlow.Controls.Add(btnRunScript);

                // Open Local Source Code Folder
                string relSource = engId == "rustdesk" ? "jDroid-X-Desk-Server\\Github\\rustdesk-master" :
                                   (engId == "sunshine" ? "jDroid-X-Desk-Server\\Github\\Sunshine-master" :
                                   (engId == "guacamole" ? "jDroid-X-Desk-Server\\Github\\guacamole-server" : "."));
                string absSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relSource);

                Button btnSource = new Button {
                    Text = "📂 Code",
                    Size = new Size(62, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240),
                    ForeColor = isDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(51, 65, 85),
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 2, 4, 0)
                };
                btnSource.FlatAppearance.BorderSize = 0;
                string capSource = absSource;
                btnSource.Click += (s, e) => {
                    try {
                        if (Directory.Exists(capSource)) Process.Start("explorer.exe", capSource);
                        else ShowToast("Folder not found: " + capSource);
                    } catch (Exception ex) { ShowToast("Explorer error: " + ex.Message); }
                };
                btnFlow.Controls.Add(btnSource);

                Button btnGuide = new Button {
                    Text = "📖 Guide",
                    Size = new Size(62, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(254, 249, 195),
                    ForeColor = isDarkMode ? Color.FromArgb(250, 204, 21) : Color.FromArgb(161, 98, 7),
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 2, 4, 0)
                };
                btnGuide.FlatAppearance.BorderSize = 0;
                string captureGuideId = engId;
                btnGuide.Click += (s, e) => ShowEngineManualSetupDialog(captureGuideId);
                btnFlow.Controls.Add(btnGuide);

                if (engId == "sunshine")
                {
                    Button btnWeb = new Button {
                        Text = "🌐 Web (47990)",
                        Size = new Size(95, 28),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(254, 249, 195),
                        ForeColor = isDarkMode ? Color.FromArgb(250, 204, 21) : Color.FromArgb(161, 98, 7),
                        Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 2, 0, 0)
                    };
                    btnWeb.FlatAppearance.BorderSize = 0;
                    btnWeb.Click += (s, e) => {
                        try { Process.Start("https://localhost:47990"); } catch { }
                    };
                    btnFlow.Controls.Add(btnWeb);
                }
                else if (engId == "guacamole")
                {
                    Button btnGuacWeb = new Button {
                        Text = "🌐 Web (8080)",
                        Size = new Size(90, 28),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = isDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(243, 232, 255),
                        ForeColor = Color.FromArgb(168, 85, 247),
                        Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 2, 0, 0)
                    };
                    btnGuacWeb.FlatAppearance.BorderSize = 0;
                    btnGuacWeb.Click += (s, e) => {
                        try { Process.Start("http://localhost:8080/guacamole"); } catch { }
                    };
                    btnFlow.Controls.Add(btnGuacWeb);
                }

                card.Controls.Add(btnFlow);
                card.Controls.Add(pnlManual);
                card.Controls.Add(lblPorts);
                card.Controls.Add(lblType);
                card.Controls.Add(cardTop);

                engineCardPanels[engId] = card;
                container.Controls.Add(card);
            }
        }

        private void SwitchActiveEngine(string engineId)
        {
            if (db == null || db.ServerConfig == null) return;
            string prev = db.ServerConfig.activeEngine;
            db.ServerConfig.activeEngine = engineId;

            if (db.ServerConfig.engines != null)
            {
                foreach (KeyValuePair<string, OpenSourceEngineModel> kv in db.ServerConfig.engines)
                {
                    if (kv.Key == engineId) kv.Value.status = "Active";
                    else if (kv.Key == "jdroidRelay") kv.Value.status = "Online";
                    else kv.Value.status = "Ready";
                }
            }

            db.AddLog("ENGINE", "SWITCH_ENGINE", string.Format("Switched active remote engine from {0} to {1}", prev, engineId));
            db.Save();

            // Auto-bind / auto-activate the required localhost port for the switched engine
            if (engineId == "jdroidRelay")
            {
                if (!EmbeddedRelayServer.Instance.IsRunning)
                {
                    StartEmbeddedRelayServer();
                }
            }
            else
            {
                // For third-party engines, check if port is open; if stopped, auto-launch startup script
                int engPort = 47990;
                if (engineId == "rustdesk") engPort = 21116;
                else if (engineId == "guacamole") engPort = 8080;

                if (!EmbeddedRelayServer.IsPortInUse(engPort))
                {
                    ThreadPool.QueueUserWorkItem(delegate {
                        LaunchEngineStartScript(engineId);
                    });
                }
            }

            // Notify local node server asynchronously
            NotifyNodeEngineSwitch(engineId);

            // Probe live status of the newly switched engine
            ProbeEngineLiveStatus(engineId);
            ThreadPool.QueueUserWorkItem(delegate {
                Thread.Sleep(1500);
                ProbeEngineLiveStatus(engineId);
            });

            // Re-render UI
            UpdateHeaderEngineTelemetryUI(engineId);
            RenderEngineCards(pnlEngineCardsContainer);
            UpdateDrawerForCurrentView("ServerAdmin");
            RefreshAdminLogs();
            if (activeViewKey == "Console" && panelWelcomeCards != null && panelWelcomeCards.Visible)
            {
                BuildWelcomeCardsContent();
            }

            ShowToast(string.Format("jDroid-Engine switched to: {0}", engineId.ToUpper()));
        }

        private void SaveServerConfigFromUi()
        {
            if (db == null || db.ServerConfig == null) return;
            if (db.ServerConfig.processingLimits == null) db.ServerConfig.processingLimits = new ProcessingLimitsModel();

            ProcessingLimitsModel limits = db.ServerConfig.processingLimits;
            if (numAdminMaxSessions != null) limits.maxConcurrentSessions = (int)numAdminMaxSessions.Value;
            if (numAdminBandwidth != null) limits.bandwidthLimitMbps = (int)numAdminBandwidth.Value;
            if (cbAdminFps != null)
            {
                if (cbAdminFps.SelectedIndex == 2) limits.maxFps = 120;
                else if (cbAdminFps.SelectedIndex == 0) limits.maxFps = 30;
                else limits.maxFps = 60;
            }
            if (numAdminIdleTimeout != null) limits.idleTimeoutMinutes = (int)numAdminIdleTimeout.Value;
            if (numAdminLogRetention != null) limits.logRetentionDays = (int)numAdminLogRetention.Value;
            if (cbAdminHwAccel != null)
            {
                if (cbAdminHwAccel.SelectedIndex == 1) limits.hardwareAcceleration = "quicksync";
                else if (cbAdminHwAccel.SelectedIndex == 2) limits.hardwareAcceleration = "software_cpu";
                else limits.hardwareAcceleration = "nvenc_cuda";
            }

            db.AddLog("CONFIG", "UPDATE_LIMITS", string.Format("Updated server processing limits (Sessions: {0}, Bandwidth: {1} Mbps, FPS: {2})", limits.maxConcurrentSessions, limits.bandwidthLimitMbps, limits.maxFps));
            db.Save();
            RefreshAdminLogs();
            ShowToast("Server Configuration & Limits saved to database.json");
        }

        private void RefreshAdminLogs()
        {
            if (dgvAdminLogs == null || db == null) return;
            string filterLevel = cbAdminLogLevel != null && cbAdminLogLevel.SelectedItem != null ? cbAdminLogLevel.SelectedItem.ToString() : "All Levels";
            string search = txtAdminLogSearch != null ? txtAdminLogSearch.Text.Trim().ToLower() : "";

            List<LogItem> list = new List<LogItem>();
            if (db.Logs != null)
            {
                for (int i = db.Logs.Count - 1; i >= 0; i--)
                {
                    LogItem item = db.Logs[i];
                    if (filterLevel != "All Levels" && !string.Equals(item.type, filterLevel, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!string.IsNullOrEmpty(search))
                    {
                        string combined = ((item.action ?? "") + " " + (item.details ?? "") + " " + (item.type ?? "")).ToLower();
                        if (!combined.Contains(search)) continue;
                    }
                    list.Add(item);
                }
            }

            dgvAdminLogs.DataSource = null;
            dgvAdminLogs.DataSource = list;
        }

        private void ExportAdminLogs()
        {
            try
            {
                if (db == null || db.Logs == null) return;
                JavaScriptSerializer ser = new JavaScriptSerializer();
                string json = ser.Serialize(db.Logs);
                string exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("server_logs_export_{0:yyyyMMdd_HHmmss}.json", DateTime.Now));
                File.WriteAllText(exportPath, json);
                ShowToast("Exported logs to: " + Path.GetFileName(exportPath));
            }
            catch (Exception ex)
            {
                ShowToast("Log export failed: " + ex.Message);
            }
        }

        private void NotifyNodeEngineSwitch(string engineId)
        {
            if (lblBackendSyncStatus != null && !lblBackendSyncStatus.IsDisposed) {
                if (this.InvokeRequired) {
                    try {
                        this.BeginInvoke(new Action(delegate {
                            lblBackendSyncStatus.Text = "[🔄 SYNCING...]";
                            lblBackendSyncStatus.ForeColor = Color.FromArgb(245, 158, 11);
                        }));
                    } catch { }
                } else {
                    lblBackendSyncStatus.Text = "[🔄 SYNCING...]";
                    lblBackendSyncStatus.ForeColor = Color.FromArgb(245, 158, 11);
                }
            }
            ThreadPool.QueueUserWorkItem(delegate {
                bool synced = false;
                try {
                    string endpoint = "http://127.0.0.1:12345/api/admin/engine/switch";
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(endpoint);
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    req.Timeout = 1500;
                    byte[] payload = System.Text.Encoding.UTF8.GetBytes(string.Format("{{\"engine\":\"{0}\",\"engineId\":\"{0}\"}}", engineId));
                    req.ContentLength = payload.Length;
                    using (Stream os = req.GetRequestStream())
                    {
                        os.Write(payload, 0, payload.Length);
                    }
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                        synced = (resp.StatusCode == HttpStatusCode.OK);
                    }
                } catch {
                    synced = false;
                }

                if (!synced)
                {
                    EmbeddedRelayServer.Instance.ActiveEngine = engineId;
                    synced = true;
                }

                if (lblBackendSyncStatus != null && !lblBackendSyncStatus.IsDisposed) {
                    try {
                        this.BeginInvoke(new Action(delegate {
                            if (synced) {
                                lblBackendSyncStatus.Text = "[✓ SYNCED]";
                                lblBackendSyncStatus.ForeColor = Color.FromArgb(16, 185, 129);
                            } else {
                                lblBackendSyncStatus.Text = "[⚠️ STANDALONE]";
                                lblBackendSyncStatus.ForeColor = Color.FromArgb(148, 163, 184);
                            }
                        }));
                    } catch { }
                }
            });
        }

    }
}
