// ==============================================================================
// jDroid-X-Desk Native Windows Desktop Application (Next-Gen RDS Console)
// Architecture: 100% Standalone WinForms C# Machine Application (Zero HTML/Browser)
// Specification: RustDesk / FreeRDP / AnyDesk Unified Remote Desktop Console
// Standard: Senior Dynamic OOPS-based MVC Architecture (20-Year Industry Standard)
// Target Runtime: .NET Framework 4.0 / 4.8 (x64) via C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
// Operator: JEET | Assistant: jDroid-X
// ==============================================================================

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

namespace JDroidXDesk.Native
{
    // ==========================================================================
    // 1. DATA MODELS & DATABASE CONTEXT (OOPS MVC - MODEL LAYER)
    // ==========================================================================
    
    public class SimulatorDesktop
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string RemoteId { get; set; }
        public string Passcode { get; set; }
        public string Ip { get; set; }
        public string Role { get; set; }
        public string OsVersion { get; set; }
        public Color AccentColor { get; set; }
        public string[] ActiveApps { get; set; }
    }

    public class AppSettings
    {
        public string theme { get; set; }
        public string language { get; set; }
        public GeneralSettings general { get; set; }
        public ConnectionSettings connection { get; set; }
        public WakeOnLanSettings wakeOnLan { get; set; }
        public AccessSettings access { get; set; }
        public RecordingSettings recording { get; set; }
        public DisplaySettings display { get; set; }
        public AudioSettings audio { get; set; }
        public AccountSettings account { get; set; }

        public AppSettings()
        {
            theme = "light";
            language = "autoselect";
            general = new GeneralSettings();
            connection = new ConnectionSettings();
            wakeOnLan = new WakeOnLanSettings();
            access = new AccessSettings();
            recording = new RecordingSettings();
            display = new DisplaySettings();
            audio = new AudioSettings();
            account = new AccountSettings();
        }
    }

    public class GeneralSettings
    {
        public bool hideLocalTaskbar { get; set; }
        public bool requestComment { get; set; }
        public bool openAddressBookOnStartup { get; set; }
        public bool autoUpdate { get; set; }
        public string updateChannel { get; set; }
        public GeneralSettings()
        {
            hideLocalTaskbar = false;
            requestComment = false;
            openAddressBookOnStartup = true;
            autoUpdate = true;
            updateChannel = "stable";
        }
    }

    public class ConnectionSettings
    {
        public int localPort { get; set; }
        public bool directConnections { get; set; }
        public bool colabCloudRelay { get; set; }
        public string proxyServer { get; set; }
        public string idServer { get; set; }
        public string relayServer { get; set; }
        public ConnectionSettings()
        {
            localPort = 7070;
            directConnections = true;
            colabCloudRelay = true;
            proxyServer = "";
            idServer = "relay.jdroid-x.net:21116";
            relayServer = "relay.jdroid-x.net:21117";
        }
    }

    public class WakeOnLanSettings
    {
        public bool enabled { get; set; }
        public List<string> authorizedClients { get; set; }
        public WakeOnLanSettings()
        {
            enabled = true;
            authorizedClients = new List<string> { "984-102-391", "710-224-900" };
        }
    }

    public class AccessSettings
    {
        public bool unattendedAccess { get; set; }
        public string passwordHash { get; set; }
        public bool enable2FA { get; set; }
        public int accessTimeout { get; set; }
        public AccessSettings()
        {
            unattendedAccess = true;
            passwordHash = "argon2id$v=19$m=65536,t=3,p=4$jdroid$hash";
            enable2FA = false;
            accessTimeout = 30;
        }
    }

    public class RecordingSettings
    {
        public bool recordIncoming { get; set; }
        public bool recordOutgoing { get; set; }
        public string recordingPath { get; set; }
        public RecordingSettings()
        {
            recordIncoming = true;
            recordOutgoing = false;
            recordingPath = "C:\\jDroid-X\\Recordings";
        }
    }

    public class DisplaySettings
    {
        public string quality { get; set; }
        public string viewMode { get; set; }
        public bool hardwareAcceleration { get; set; }
        public DisplaySettings()
        {
            quality = "balanced";
            viewMode = "smartfit";
            hardwareAcceleration = true;
        }
    }

    public class AudioSettings
    {
        public bool transmitAudio { get; set; }
        public bool outputAudio { get; set; }
        public AudioSettings()
        {
            transmitAudio = true;
            outputAudio = true;
        }
    }

    public class AccountSettings
    {
        public string username { get; set; }
        public string licenseType { get; set; }
        public string enterpriseTenant { get; set; }
        public AccountSettings()
        {
            username = "JEET";
            licenseType = "Enterprise Platinum (Unlimited)";
            enterpriseTenant = "JEET-Antigravity-Innovant-Cluster";
        }
    }

    public class DeviceItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string ip { get; set; }
        public string groupId { get; set; }
        public string group { get; set; }
        public string status { get; set; }
        public string accessLevel { get; set; }
    }

    public class GroupItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
    }

    public class SessionItem
    {
        public string id { get; set; }
        public string targetId { get; set; }
        public string targetName { get; set; }
        public string type { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public int durationSeconds { get; set; }
        public string status { get; set; }
        public string sourceIp { get; set; }
    }

    public class GuardianItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string contact { get; set; }
        public string status { get; set; }
    }

    public class SafeZoneItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string ip { get; set; }
    }

    public class ContactItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string remoteId { get; set; }
        public string group { get; set; }
    }

    public class LogItem
    {
        public string timestamp { get; set; }
        public string type { get; set; }
        public string action { get; set; }
        public string details { get; set; }
    }

    public class DatabaseContext
    {
        public string DbPath { get; private set; }
        public AppSettings Settings { get; set; }
        public List<DeviceItem> Devices { get; set; }
        public List<GroupItem> Groups { get; set; }
        public List<SessionItem> Sessions { get; set; }
        public List<GuardianItem> Guardians { get; set; }
        public List<SafeZoneItem> SafeZones { get; set; }
        public List<ContactItem> Contacts { get; set; }
        public List<LogItem> Logs { get; set; }

        private JavaScriptSerializer serializer;

        public DatabaseContext()
        {
            serializer = new JavaScriptSerializer();
            DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.json");
            Load();
        }

        public void Load()
        {
            if (File.Exists(DbPath))
            {
                try
                {
                    string json = File.ReadAllText(DbPath);
                    Dictionary<string, object> dict = serializer.Deserialize<Dictionary<string, object>>(json);

                    if (dict.ContainsKey("settings"))
                    {
                        string sJson = serializer.Serialize(dict["settings"]);
                        Settings = serializer.Deserialize<AppSettings>(sJson);
                    }
                    else Settings = new AppSettings();

                    Devices = DeserializeList<DeviceItem>(dict, "devices");
                    Groups = DeserializeList<GroupItem>(dict, "groups");
                    Sessions = DeserializeList<SessionItem>(dict, "sessions");
                    Guardians = DeserializeList<GuardianItem>(dict, "guardians");
                    SafeZones = DeserializeList<SafeZoneItem>(dict, "safeZones");
                    Contacts = DeserializeList<ContactItem>(dict, "contacts");
                    Logs = DeserializeList<LogItem>(dict, "logs");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Database Load Exception: " + ex.Message);
                }
            }

            InitDefaults();
        }

        private List<T> DeserializeList<T>(Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                string s = serializer.Serialize(dict[key]);
                return serializer.Deserialize<List<T>>(s) ?? new List<T>();
            }
            return new List<T>();
        }

        private void InitDefaults()
        {
            Settings = new AppSettings();
            Devices = new List<DeviceItem> {
                new DeviceItem { id = "DEV-1", name = "JEET-DESK", ip = "192.168.1.44", groupId = "grp-1", group = "Lead Architects", status = "ONLINE", accessLevel = "Administrator" },
                new DeviceItem { id = "DEV-2", name = "ANTIGRAVITY-NODE", ip = "192.168.1.88", groupId = "grp-2", group = "AI Clusters", status = "ONLINE", accessLevel = "Cluster Controller" },
                new DeviceItem { id = "DEV-3", name = "INNOVANT-STATION", ip = "192.168.1.105", groupId = "grp-3", group = "Render Stations", status = "ONLINE", accessLevel = "GPU Render Node" }
            };
            Groups = new List<GroupItem> {
                new GroupItem { id = "grp-1", name = "Lead Architects", desc = "Primary management and command workstations" },
                new GroupItem { id = "grp-2", name = "AI Clusters", desc = "Autonomous agentic reasoning nodes" },
                new GroupItem { id = "grp-3", name = "Render Stations", desc = "High-throughput GPU render fleet" }
            };
            Sessions = new List<SessionItem>();
            Guardians = new List<GuardianItem> {
                new GuardianItem { id = "grd-1", name = "JEET (Lead)", role = "Supreme Commander & Architect", contact = "jeet@enterprise.local", status = "Active" },
                new GuardianItem { id = "grd-2", name = "Antigravity Supervisor", role = "Autonomous Multi-Agent Manager", contact = "ai@enterprise.local", status = "Active" }
            };
            SafeZones = new List<SafeZoneItem> {
                new SafeZoneItem { id = "sz-1", name = "LAN Safe Subnet", ip = "192.168.1.0/24" },
                new SafeZoneItem { id = "sz-2", name = "Cloud Gateway Relay", ip = "10.8.0.0/16" }
            };
            Contacts = new List<ContactItem> {
                new ContactItem { id = "cnt-1", name = "JEET-DESK", remoteId = "984-102-391", group = "Core Nodes" },
                new ContactItem { id = "cnt-2", name = "ANTIGRAVITY-NODE", remoteId = "710-224-900", group = "AI Nodes" },
                new ContactItem { id = "cnt-3", name = "INNOVANT-STATION", remoteId = "520-310-884", group = "GPU Fleet" }
            };
            Logs = new List<LogItem>();
            Save();
        }

        public void Save()
        {
            try
            {
                Dictionary<string, object> root = new Dictionary<string, object>();
                root["settings"] = Settings;
                root["devices"] = Devices;
                root["groups"] = Groups;
                root["sessions"] = Sessions;
                root["guardians"] = Guardians;
                root["safeZones"] = SafeZones;
                root["contacts"] = Contacts;
                root["logs"] = Logs;

                string json = serializer.Serialize(root);
                File.WriteAllText(DbPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Database Save Exception: " + ex.Message);
            }
        }

        public void AddLog(string type, string action, string details)
        {
            Logs.Insert(0, new LogItem {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                type = type,
                action = action,
                details = details
            });
            if (Logs.Count > 200) Logs.RemoveAt(Logs.Count - 1);
            Save();
        }
    }

    // ==========================================================================
    // 2. MODERN REUSABLE UI CONTROLS & RENDERERS (VIEW LAYER)
    // ==========================================================================

    public class ModernButton : Button
    {
        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }
        public Color TextColor { get; set; }

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
    }

    public class ModernCardPanel : Panel
    {
        public Color BorderColor { get; set; }
        public Color CardBackground { get; set; }

        public ModernCardPanel()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.BorderColor = Color.FromArgb(226, 232, 240);
            this.CardBackground = Color.White;
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (SolidBrush bgBrush = new SolidBrush(CardBackground))
            {
                g.FillRectangle(bgBrush, rect);
            }
            using (Pen borderPen = new Pen(BorderColor, 1))
            {
                g.DrawRectangle(borderPen, rect);
            }
        }
    }

    // ==========================================================================
    // 3. MAIN FORM & ORCHESTRATION (CONTROLLER & VIEW LAYER)
    // ==========================================================================

    public class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private DatabaseContext db;
        private List<SimulatorDesktop> simulatorDesktops;
        private SimulatorDesktop currentHostDesktop;
        private SimulatorDesktop connectedTargetDesktop;

        // UI Core
        private Panel topHeaderPanel;
        private Label lblActiveViewTitle;
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Button currentNavButton;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        // Navigation Subviews
        private Panel viewConsole;
        private Panel viewFileTransfer;
        private Panel viewTerminal;
        private Panel viewFleet;
        private Panel viewHistory;
        private Panel viewGuardians;
        private Panel viewSafeZones;
        private Panel viewHealth;
        private Panel viewSettingsStudio;

        // Session State & Telemetry
        private bool isSessionActive = false;
        private bool isLiveScreenMirror = false;
        private string smartFitMode = "SmartFit"; // "SmartFit", "Original", "Stretch"
        private string qualityMode = "60 FPS (Fast)";
        private DateTime sessionStartTime;
        private System.Windows.Forms.Timer sessionTelemetryTimer;
        private long bytesTransferred = 1048576;
        private bool isStartMenuOpen = false;

        // Console Controls
        private Label lblLocalId;
        private Label lblLocalPass;
        private Label lblLocalIp;
        private Label lblLocalStatus;
        private TextBox txtTargetId;
        private TextBox txtTargetPass;
        private ModernButton btnConnect;
        private PictureBox viewportCanvas;
        private Panel sessionFloatingToolbar;
        private Label lblSessionTelemetryHUD;
        private FlowLayoutPanel quickCallPanel;
        private FlowLayoutPanel hostSelectorPanel;
        private Label lblSessionStatusBottom;

        // File Transfer Controls
        private ListBox lbLocalFiles;
        private ListBox lbRemoteFiles;
        private Label lblLocalPath;
        private Label lblRemotePath;

        // Terminal Controls
        private RichTextBox terminalBox;
        private TextBox terminalInput;

        // Grids & Settings
        private DataGridView dgvFleet;
        private ComboBox cboFleetGroupFilter;
        private DataGridView dgvHistory;
        private DataGridView dgvGuardians;
        private TreeView settingsTree;
        private Panel settingsSubContentPanel;

        // Toast Notification
        private Panel notificationToast;
        private Label lblToastMessage;
        private System.Windows.Forms.Timer toastTimer;

        [STAThread]
        public static void Main()
        {
            try { SetProcessDPIAware(); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public MainForm()
        {
            db = new DatabaseContext();

            // 3 Standard Machine Nodes (JEET, ANTIGRAVITY, INNOVANT)
            simulatorDesktops = new List<SimulatorDesktop>
            {
                new SimulatorDesktop
                {
                    Key = "JEET",
                    Name = "JEET-DESK",
                    RemoteId = "984-102-391",
                    Passcode = "659-332",
                    Ip = "192.168.1.44",
                    Role = "Primary Workstation (Lead Architect)",
                    OsVersion = "Windows 11 Pro Enterprise x64",
                    AccentColor = Color.FromArgb(16, 185, 129), // Emerald
                    ActiveApps = new[] { "Visual Studio 2022 - jDroid-X Native", "Antigravity Multi-Agent IDE", "Windows Terminal (PowerShell)" }
                },
                new SimulatorDesktop
                {
                    Key = "ANTIGRAVITY",
                    Name = "ANTIGRAVITY-NODE",
                    RemoteId = "710-224-900",
                    Passcode = "884-219",
                    Ip = "192.168.1.88",
                    Role = "Autonomous AI Cluster & Gateway",
                    OsVersion = "Windows Server 2022 Datacenter",
                    AccentColor = Color.FromArgb(2, 132, 199), // Ocean Blue
                    ActiveApps = new[] { "Google DeepMind Cognitive Engine", "Relay Bridge Server :7070", "CUDA AI Diagnostics" }
                },
                new SimulatorDesktop
                {
                    Key = "INNOVANT",
                    Name = "INNOVANT-STATION",
                    RemoteId = "520-310-884",
                    Passcode = "441-902",
                    Ip = "192.168.1.105",
                    Role = "Field Fleet & GPU Render Station",
                    OsVersion = "Windows 10 Enterprise (Build 19045)",
                    AccentColor = Color.FromArgb(245, 158, 11), // Amber
                    ActiveApps = new[] { "Blender 4.0 Cycles GPU Render Engine", "Network Packet Capture & Security", "Host Telemetry Service" }
                }
            };

            currentHostDesktop = simulatorDesktops[0]; // Acting as JEET-DESK
            connectedTargetDesktop = simulatorDesktops[1]; // Default Target: ANTIGRAVITY-NODE

            this.Text = "jDroid-X-Desk | Professional Windows Remote Control Console (RustDesk / FreeRDP Standard)";
            this.Size = new Size(1380, 880);
            this.MinimumSize = new Size(1080, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Icon = SystemIcons.Shield;

            // Session timer for real-time FPS & RTT
            sessionTelemetryTimer = new System.Windows.Forms.Timer();
            sessionTelemetryTimer.Interval = 1000;
            sessionTelemetryTimer.Tick += (s, e) => {
                if (isSessionActive)
                {
                    bytesTransferred += (long)(1800000 + (new Random().Next(100000, 400000)));
                    UpdateSessionHUD();
                    viewportCanvas.Invalidate();
                }
            };

            InitializeLayout();
            SetupTraySupervisor();

            // Default View
            SwitchView("Remote Desktop Console", viewConsole);
        }

        private void InitializeLayout()
        {
            // 1. TOP HEADER (BRAND & OPERATOR STATUS)
            topHeaderPanel = new Panel();
            topHeaderPanel.Dock = DockStyle.Top;
            topHeaderPanel.Height = 54;
            topHeaderPanel.BackColor = Color.FromArgb(15, 23, 42); // Deep Navy Slate
            topHeaderPanel.Padding = new Padding(16, 0, 16, 0);

            Label lblLogo = new Label();
            lblLogo.Text = "jDroid-X-Desk";
            lblLogo.ForeColor = Color.White;
            lblLogo.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            lblLogo.AutoSize = true;
            lblLogo.Location = new Point(16, 14);

            Label lblEdition = new Label();
            lblEdition.Text = "RDS NATIVE";
            lblEdition.ForeColor = Color.FromArgb(14, 165, 233);
            lblEdition.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblEdition.BackColor = Color.FromArgb(30, 41, 59);
            lblEdition.Padding = new Padding(4, 2, 4, 2);
            lblEdition.AutoSize = true;
            lblEdition.Location = new Point(148, 17);

            Label lblOperator = new Label();
            lblOperator.Text = "OPERATOR: JEET (Lead Architect • L5)";
            lblOperator.ForeColor = Color.FromArgb(16, 185, 129); // Emerald
            lblOperator.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblOperator.AutoSize = true;
            lblOperator.Location = new Point(245, 18);

            lblActiveViewTitle = new Label();
            lblActiveViewTitle.Text = "Remote Desktop Console";
            lblActiveViewTitle.ForeColor = Color.FromArgb(148, 163, 184);
            lblActiveViewTitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            lblActiveViewTitle.AutoSize = false;
            lblActiveViewTitle.Width = 360;
            lblActiveViewTitle.Dock = DockStyle.Right;
            lblActiveViewTitle.TextAlign = ContentAlignment.MiddleRight;

            topHeaderPanel.Controls.Add(lblActiveViewTitle);
            topHeaderPanel.Controls.Add(lblOperator);
            topHeaderPanel.Controls.Add(lblEdition);
            topHeaderPanel.Controls.Add(lblLogo);
            this.Controls.Add(topHeaderPanel);

            // 2. LEFT SIDEBAR NAVIGATION
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 240;
            sidebarPanel.BackColor = Color.FromArgb(30, 41, 59); // Slate Dark
            sidebarPanel.Padding = new Padding(8);

            BuildSidebarNavigation();
            this.Controls.Add(sidebarPanel);

            // 3. MAIN CONTENT CONTAINER
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(241, 245, 249);
            this.Controls.Add(contentPanel);

            // 4. INITIALIZE ALL CORE SUBVIEWS
            InitializeConsoleView();
            InitializeFileTransferView();
            InitializeTerminalView();
            InitializeFleetView();
            InitializeHistoryView();
            InitializeGuardiansView();
            InitializeSafeZonesView();
            InitializeHealthView();
            InitializeSettingsStudioView();
        }

        private void BuildSidebarNavigation()
        {
            string[] items = new string[] {
                "Remote Desktop Console",
                "Direct File Transfer",
                "Admin PowerShell",
                "Fleet Workstations",
                "Session History & Logs",
                "Guardians Roster",
                "Safe Zones (Perimeter)",
                "Hardware Diagnostics",
                "Settings Studio"
            };

            int y = 16;
            foreach (string item in items)
            {
                Button btn = new Button();
                btn.Text = "  " + item;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.ForeColor = Color.FromArgb(203, 213, 225);
                btn.BackColor = Color.Transparent;
                btn.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
                btn.Size = new Size(224, 40);
                btn.Location = new Point(8, y);
                btn.Cursor = Cursors.Hand;

                string target = item;
                btn.Click += (s, e) => {
                    HighlightNav(btn);
                    if (target == "Remote Desktop Console") SwitchView(target, viewConsole);
                    else if (target == "Direct File Transfer") SwitchView(target, viewFileTransfer);
                    else if (target == "Admin PowerShell") SwitchView(target, viewTerminal);
                    else if (target == "Fleet Workstations") SwitchView(target, viewFleet);
                    else if (target == "Session History & Logs") SwitchView(target, viewHistory);
                    else if (target == "Guardians Roster") SwitchView(target, viewGuardians);
                    else if (target == "Safe Zones (Perimeter)") SwitchView(target, viewSafeZones);
                    else if (target == "Hardware Diagnostics") SwitchView(target, viewHealth);
                    else if (target == "Settings Studio") SwitchView(target, viewSettingsStudio);
                };

                sidebarPanel.Controls.Add(btn);
                if (item == "Remote Desktop Console") currentNavButton = btn;
                y += 46;
            }

            HighlightNav(currentNavButton);
        }

        private void HighlightNav(Button btn)
        {
            foreach (Control c in sidebarPanel.Controls)
            {
                if (c is Button)
                {
                    c.BackColor = Color.Transparent;
                    c.ForeColor = Color.FromArgb(203, 213, 225);
                }
            }
            if (btn != null)
            {
                btn.BackColor = Color.FromArgb(239, 51, 64); // Crimson Highlight
                btn.ForeColor = Color.White;
            }
        }

        private void SwitchView(string title, Panel targetView)
        {
            lblActiveViewTitle.Text = title;
            contentPanel.Controls.Clear();
            if (targetView != null)
            {
                targetView.Dock = DockStyle.Fill;
                contentPanel.Controls.Add(targetView);
            }
        }

        // ======================================================================
        // VIEW 1: REMOTE DESKTOP CONSOLE (RUSTDESK / ANYDESK HIGH-FIDELITY ENGINE)
        // ======================================================================

        private void InitializeConsoleView()
        {
            viewConsole = new Panel();
            viewConsole.Dock = DockStyle.Fill;
            viewConsole.Padding = new Padding(12);

            // 1. TOP CARDS (LOCAL MACHINE & REMOTE TARGET)
            TableLayoutPanel topCards = new TableLayoutPanel();
            topCards.Dock = DockStyle.Top;
            topCards.Height = 168;
            topCards.ColumnCount = 2;
            topCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            topCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            topCards.Padding = new Padding(0, 0, 0, 8);

            // -------------------------------------------------------------
            // CARD 1: THIS MACHINE (LOCAL HOST DETAILS & SWITCHER)
            // -------------------------------------------------------------
            ModernCardPanel cardThisMachine = new ModernCardPanel();
            cardThisMachine.Dock = DockStyle.Fill;
            cardThisMachine.Margin = new Padding(0, 0, 6, 0);

            TableLayoutPanel leftGrid = new TableLayoutPanel();
            leftGrid.Dock = DockStyle.Fill;
            leftGrid.RowCount = 4;
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f)); // Header + Switcher
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f)); // ID + Copy
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f)); // Passcode + Eye
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f)); // IP & Status

            // Row 0: Switcher Prompt & Segmented Buttons
            Panel hostHeader = new Panel { Dock = DockStyle.Fill };
            Label lblHostHdr = new Label { Text = "THIS DESK:", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Location = new Point(0, 5) };
            hostSelectorPanel = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
            hostHeader.Controls.Add(lblHostHdr);
            hostHeader.Controls.Add(hostSelectorPanel);

            // Row 1: Big Bold 9-Digit ID Display & Copy Button
            Panel idRow = new Panel { Dock = DockStyle.Fill };
            lblLocalId = new Label { Text = "984-102-391", Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true, Location = new Point(0, 2) };
            ModernButton btnCopyId = new ModernButton { Text = "📋 Copy ID", NormalColor = Color.FromArgb(71, 85, 105), HoverColor = Color.FromArgb(100, 116, 139), Size = new Size(95, 30), Location = new Point(230, 8) };
            btnCopyId.Click += (s, e) => {
                if (currentHostDesktop != null) {
                    Clipboard.SetText(currentHostDesktop.RemoteId);
                    ShowToast("Copied " + currentHostDesktop.RemoteId + " to clipboard");
                }
            };
            idRow.Controls.Add(lblLocalId);
            idRow.Controls.Add(btnCopyId);

            // Row 2: Passcode with Refresh
            Panel passRow = new Panel { Dock = DockStyle.Fill };
            lblLocalPass = new Label { Text = "One-Time Password: " + currentHostDesktop.Passcode, Font = new Font("Consolas", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), AutoSize = true, Location = new Point(0, 4) };
            Button btnNewPass = new Button { Text = "🔄", Size = new Size(26, 22), Location = new Point(230, 2), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnNewPass.FlatAppearance.BorderSize = 0;
            btnNewPass.Click += (s, e) => {
                currentHostDesktop.Passcode = new Random().Next(100, 999) + "-" + new Random().Next(100, 999);
                lblLocalPass.Text = "One-Time Password: " + currentHostDesktop.Passcode;
                ShowToast("Generated fresh security passcode.");
            };
            passRow.Controls.Add(lblLocalPass);
            passRow.Controls.Add(btnNewPass);

            // Row 3: Status & Network
            Panel ipRow = new Panel { Dock = DockStyle.Fill };
            lblLocalIp = new Label { Text = "IP: 192.168.1.44 | Port: 7070", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true, Location = new Point(0, 4) };
            lblLocalStatus = new Label { Text = "🟢 Ready (Rendezvous Online)", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), AutoSize = true, Location = new Point(190, 4) };
            ipRow.Controls.Add(lblLocalIp);
            ipRow.Controls.Add(lblLocalStatus);

            leftGrid.Controls.Add(hostHeader, 0, 0);
            leftGrid.Controls.Add(idRow, 0, 1);
            leftGrid.Controls.Add(passRow, 0, 2);
            leftGrid.Controls.Add(ipRow, 0, 3);
            cardThisMachine.Controls.Add(leftGrid);

            // -------------------------------------------------------------
            // CARD 2: CONTROL REMOTE DESKTOP (INPUT & 1-CLICK CALL BAR)
            // -------------------------------------------------------------
            ModernCardPanel cardRemote = new ModernCardPanel();
            cardRemote.Dock = DockStyle.Fill;
            cardRemote.Margin = new Padding(6, 0, 0, 0);

            TableLayoutPanel rightGrid = new TableLayoutPanel();
            rightGrid.Dock = DockStyle.Fill;
            rightGrid.RowCount = 4;
            rightGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f)); // Header
            rightGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f)); // Quick Call 1-Click Buttons
            rightGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f)); // Target ID + Pass + Connect
            rightGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f)); // Secondary features

            Label lblRemotePrompt = new Label { Text = "CONTROL REMOTE DESKTOP (RUSTDESK / P2P DIRECT):", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Dock = DockStyle.Fill };

            quickCallPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            // Manual Connection Row
            FlowLayoutPanel manualConnectRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            Panel targetIdBox = new Panel { Size = new Size(180, 34), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Margin = new Padding(0, 2, 8, 0) };
            txtTargetId = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Consolas", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(6, 6), Size = new Size(168, 22), Text = "710-224-900" };
            targetIdBox.Controls.Add(txtTargetId);

            Panel targetPassBox = new Panel { Size = new Size(110, 34), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Margin = new Padding(0, 2, 8, 0) };
            txtTargetPass = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Consolas", 11f), Location = new Point(6, 6), Size = new Size(98, 22), UseSystemPasswordChar = true, Text = "884219" };
            targetPassBox.Controls.Add(txtTargetPass);

            btnConnect = new ModernButton { Text = "⚡ Connect", NormalColor = Color.FromArgb(239, 51, 64), HoverColor = Color.FromArgb(220, 38, 38), Size = new Size(110, 34), Margin = new Padding(0, 2, 8, 0) };
            btnConnect.Click += (s, e) => ToggleSessionConnection();

            Button btnFileTransfer = new Button { Text = "📁 File Transfer", Size = new Size(120, 34), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(2, 132, 199), BackColor = Color.FromArgb(240, 249, 255), Cursor = Cursors.Hand, Margin = new Padding(0, 2, 0, 0) };
            btnFileTransfer.FlatAppearance.BorderColor = Color.FromArgb(186, 230, 253);
            btnFileTransfer.Click += (s, e) => SwitchView("Direct File Transfer", viewFileTransfer);

            manualConnectRow.Controls.Add(targetIdBox);
            manualConnectRow.Controls.Add(targetPassBox);
            manualConnectRow.Controls.Add(btnConnect);
            manualConnectRow.Controls.Add(btnFileTransfer);

            // Row 3: Security & Quick Assist
            FlowLayoutPanel subActions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            Label lblEnc = new Label { Text = "🔒 P2P End-to-End: ChaCha20-Poly1305 / AES-256", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true, Margin = new Padding(0, 4, 12, 0) };
            Button btnMsQuick = new Button { Text = "Launch Windows Quick Assist", Size = new Size(180, 24), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(71, 85, 105), BackColor = Color.FromArgb(241, 245, 249), Cursor = Cursors.Hand };
            btnMsQuick.FlatAppearance.BorderSize = 0;
            btnMsQuick.Click += (s, e) => { try { Process.Start("ms-quick-assist:"); } catch { } };

            subActions.Controls.Add(lblEnc);
            subActions.Controls.Add(btnMsQuick);

            rightGrid.Controls.Add(lblRemotePrompt, 0, 0);
            rightGrid.Controls.Add(quickCallPanel, 0, 1);
            rightGrid.Controls.Add(manualConnectRow, 0, 2);
            rightGrid.Controls.Add(subActions, 0, 3);
            cardRemote.Controls.Add(rightGrid);

            topCards.Controls.Add(cardThisMachine, 0, 0);
            topCards.Controls.Add(cardRemote, 1, 0);
            viewConsole.Controls.Add(topCards);

            // Rebuild host selector and quick call buttons
            RebuildSimulatorControls();

            // 2. REMOTE DESKTOP SCREEN VIEWPORT CONTAINER
            Panel canvasContainer = new Panel();
            canvasContainer.Dock = DockStyle.Fill;
            canvasContainer.BackColor = Color.FromArgb(15, 23, 42); // Navy background

            // Floating Session Bar across the top of the canvas
            sessionFloatingToolbar = new Panel();
            sessionFloatingToolbar.Dock = DockStyle.Top;
            sessionFloatingToolbar.Height = 40;
            sessionFloatingToolbar.BackColor = Color.FromArgb(30, 41, 59);
            sessionFloatingToolbar.Padding = new Padding(8, 4, 8, 4);
            sessionFloatingToolbar.Visible = true; // Always visible as toolbar

            BuildFloatingSessionToolbar();
            canvasContainer.Controls.Add(sessionFloatingToolbar);

            // GDI+ Canvas for high-fidelity remote desktop stream
            viewportCanvas = new PictureBox();
            viewportCanvas.Dock = DockStyle.Fill;
            viewportCanvas.BackColor = Color.FromArgb(15, 23, 42);
            viewportCanvas.Paint += ViewportCanvas_Paint;
            viewportCanvas.MouseClick += ViewportCanvas_MouseClick;
            viewportCanvas.MouseMove += ViewportCanvas_MouseMove;

            canvasContainer.Controls.Add(viewportCanvas);
            viewConsole.Controls.Add(canvasContainer);

            // 3. BOTTOM STATUS STRIP
            Panel bottomStrip = new Panel();
            bottomStrip.Dock = DockStyle.Bottom;
            bottomStrip.Height = 28;
            bottomStrip.BackColor = Color.FromArgb(226, 232, 240);
            bottomStrip.Padding = new Padding(8, 5, 8, 0);

            lblSessionStatusBottom = new Label();
            lblSessionStatusBottom.Text = "Status: Idle (Standby) | Rendezvous: relay.jdroid-x.net:21116 (Active) | NAT Type: Full Cone P2P Ready";
            lblSessionStatusBottom.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblSessionStatusBottom.ForeColor = Color.FromArgb(71, 85, 105);
            lblSessionStatusBottom.AutoSize = true;

            bottomStrip.Controls.Add(lblSessionStatusBottom);
            viewConsole.Controls.Add(bottomStrip);
        }

        private void BuildFloatingSessionToolbar()
        {
            sessionFloatingToolbar.Controls.Clear();

            FlowLayoutPanel leftTools = new FlowLayoutPanel();
            leftTools.Dock = DockStyle.Left;
            leftTools.FlowDirection = FlowDirection.LeftToRight;
            leftTools.WrapContents = false;
            leftTools.AutoSize = true;

            // Monitor Switcher
            Button btnDisplay = CreateToolbarBtn("🖥️ Display 1 (1080p)", (s, e) => ShowToast("Switched to Virtual Display 1 (1920x1080)"));
            
            // Smart Fit Mode Toggle
            Button btnSmartFit = CreateToolbarBtn("📐 " + smartFitMode, (s, e) => {
                if (smartFitMode == "SmartFit") smartFitMode = "Original 1:1";
                else if (smartFitMode == "Original 1:1") smartFitMode = "Stretch";
                else smartFitMode = "SmartFit";
                ((Button)s).Text = "📐 " + smartFitMode;
                viewportCanvas.Invalidate();
                ShowToast("View Scale Mode: " + smartFitMode);
            });

            // Live Screen Mirror Toggle (Actual Local Display vs Simulated Remote OS)
            Button btnMirror = CreateToolbarBtn("🎥 " + (isLiveScreenMirror ? "Simulated OS" : "Live Mirror"), (s, e) => {
                isLiveScreenMirror = !isLiveScreenMirror;
                ((Button)s).Text = "🎥 " + (isLiveScreenMirror ? "Simulated OS" : "Live Mirror");
                viewportCanvas.Invalidate();
                ShowToast("Display Engine: " + (isLiveScreenMirror ? "Live Local Display Mirror (60 FPS)" : "Simulated Remote OS Desktop"));
            });

            // Action Triggers
            Button btnCad = CreateToolbarBtn("⌨️ Ctrl+Alt+Del", (s, e) => ShowToast("Sent Secure Attention Sequence (Ctrl+Alt+Del) to remote node."));
            Button btnTaskMgr = CreateToolbarBtn("⚙️ Task Manager", (s, e) => {
                try { Process.Start("taskmgr.exe"); } catch { }
            });
            Button btnShell = CreateToolbarBtn("💻 PowerShell", (s, e) => SwitchView("Admin PowerShell", viewTerminal));
            Button btnFiles = CreateToolbarBtn("📁 Files", (s, e) => SwitchView("Direct File Transfer", viewFileTransfer));

            leftTools.Controls.Add(btnDisplay);
            leftTools.Controls.Add(btnSmartFit);
            leftTools.Controls.Add(btnMirror);
            leftTools.Controls.Add(btnCad);
            leftTools.Controls.Add(btnTaskMgr);
            leftTools.Controls.Add(btnShell);
            leftTools.Controls.Add(btnFiles);

            // Right HUD & Disconnect
            FlowLayoutPanel rightHUD = new FlowLayoutPanel();
            rightHUD.Dock = DockStyle.Right;
            rightHUD.FlowDirection = FlowDirection.LeftToRight;
            rightHUD.WrapContents = false;
            rightHUD.AutoSize = true;

            lblSessionTelemetryHUD = new Label();
            lblSessionTelemetryHUD.Text = "🟢 60.0 FPS | 11ms RTT | 1.8 Mbps | Direct P2P | AES-GCM-256";
            lblSessionTelemetryHUD.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblSessionTelemetryHUD.ForeColor = Color.FromArgb(16, 185, 129);
            lblSessionTelemetryHUD.AutoSize = true;
            lblSessionTelemetryHUD.Margin = new Padding(0, 8, 12, 0);

            ModernButton btnDisconnect = new ModernButton { Text = "❌ Disconnect", NormalColor = Color.FromArgb(239, 51, 64), HoverColor = Color.FromArgb(220, 38, 38), Size = new Size(110, 28) };
            btnDisconnect.Click += (s, e) => {
                if (isSessionActive) ToggleSessionConnection();
                else ShowToast("No active remote desktop session to disconnect.");
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
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 6, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        private void UpdateSessionHUD()
        {
            if (lblSessionTelemetryHUD != null && isSessionActive)
            {
                TimeSpan span = DateTime.Now - sessionStartTime;
                string mb = (bytesTransferred / (1024 * 1024.0)).ToString("0.0");
                lblSessionTelemetryHUD.Text = string.Format("🟢 Connected: {0:D2}:{1:D2} | 60.0 FPS | 11ms RTT | {2} MB | Direct P2P", (int)span.TotalMinutes, span.Seconds, mb);
            }
        }

        private void RebuildSimulatorControls()
        {
            if (hostSelectorPanel == null || currentHostDesktop == null) return;

            // 1. Update Host Switcher Buttons
            hostSelectorPanel.Controls.Clear();
            foreach (SimulatorDesktop desk in simulatorDesktops)
            {
                bool isCurrent = desk.Key == currentHostDesktop.Key;
                Button btn = new Button
                {
                    Text = (desk.Key == "JEET" ? "💻 " : desk.Key == "ANTIGRAVITY" ? "🤖 " : "🚀 ") + desk.Name,
                    Size = new Size(160, 26),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8f, isCurrent ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isCurrent ? Color.White : Color.FromArgb(30, 41, 59),
                    BackColor = isCurrent ? desk.AccentColor : Color.FromArgb(241, 245, 249),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 4, 0)
                };
                btn.FlatAppearance.BorderSize = isCurrent ? 0 : 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
                SimulatorDesktop targetDesk = desk;
                btn.Click += (s, e) => SwitchHostDesktop(targetDesk);
                hostSelectorPanel.Controls.Add(btn);
            }

            // 2. Update Local Card Details
            if (lblLocalId != null) lblLocalId.Text = currentHostDesktop.RemoteId;
            if (lblLocalPass != null) lblLocalPass.Text = "One-Time Password: " + currentHostDesktop.Passcode;
            if (lblLocalIp != null) lblLocalIp.Text = "IP: " + currentHostDesktop.Ip + " | Port: 7070";

            // 3. Update Quick Call Buttons (1-Click Connecting to the OTHER 2 Desktops!)
            if (quickCallPanel != null)
            {
                quickCallPanel.Controls.Clear();
                foreach (SimulatorDesktop target in simulatorDesktops)
                {
                    if (target.Key != currentHostDesktop.Key)
                    {
                        Button btnCall = new Button
                        {
                            Text = "📞 Call " + target.Name + " (" + target.RemoteId + ")",
                            Size = new Size(220, 32),
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                            ForeColor = Color.White,
                            BackColor = target.AccentColor,
                            Cursor = Cursors.Hand,
                            Margin = new Padding(0, 2, 8, 0)
                        };
                        btnCall.FlatAppearance.BorderSize = 0;
                        SimulatorDesktop callTarget = target;
                        btnCall.Click += (s, e) => {
                            if (isSessionActive) ToggleSessionConnection();
                            ToggleSessionConnection(callTarget);
                        };
                        quickCallPanel.Controls.Add(btnCall);
                    }
                }
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
            RebuildSimulatorControls();
            ShowToast("Local Host switched to " + newHost.Name + " (" + newHost.RemoteId + ")");
        }

        private void ToggleSessionConnection(SimulatorDesktop directTarget = null)
        {
            if (!isSessionActive)
            {
                if (directTarget != null)
                {
                    connectedTargetDesktop = directTarget;
                    txtTargetId.Text = directTarget.RemoteId;
                    txtTargetPass.Text = directTarget.Passcode;
                }
                else
                {
                    string targetId = txtTargetId.Text.Trim();
                    SimulatorDesktop matched = simulatorDesktops.Find(d => d.RemoteId == targetId || d.Key == targetId.ToUpper());
                    if (matched != null) connectedTargetDesktop = matched;
                    else
                    {
                        connectedTargetDesktop = new SimulatorDesktop {
                            Key = "CUSTOM",
                            Name = "Remote Workstation (" + targetId + ")",
                            RemoteId = targetId,
                            Passcode = txtTargetPass.Text.Trim(),
                            Ip = "192.168.1." + new Random().Next(120, 250),
                            Role = "External Remote Enterprise Workstation",
                            OsVersion = "Windows 11 Pro Enterprise x64",
                            AccentColor = Color.FromArgb(2, 132, 199),
                            ActiveApps = new[] { "Visual Studio Code - Workspace", "File Explorer", "Command Prompt" }
                        };
                    }
                }

                isSessionActive = true;
                sessionStartTime = DateTime.Now;
                sessionTelemetryTimer.Start();

                btnConnect.Text = "Disconnect";
                btnConnect.NormalColor = Color.FromArgb(100, 116, 139);
                lblSessionStatusBottom.Text = string.Format("CONNECTED: {0} [{1}] | IP: {2} | Mode: Direct P2P (60 FPS) | Encryption: AES-256-GCM", connectedTargetDesktop.Name, connectedTargetDesktop.RemoteId, connectedTargetDesktop.Ip);
                lblSessionStatusBottom.ForeColor = Color.FromArgb(16, 185, 129);

                db.AddLog("SESSION_START", "P2P_CONNECT", "Connected to " + connectedTargetDesktop.Name + " (" + connectedTargetDesktop.RemoteId + ")");
                ShowToast("P2P Direct Session Established with " + connectedTargetDesktop.Name);
                viewportCanvas.Invalidate();
            }
            else
            {
                isSessionActive = false;
                sessionTelemetryTimer.Stop();
                TimeSpan duration = DateTime.Now - sessionStartTime;

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
                db.AddLog("SESSION_END", "DISCONNECT", "Session closed with " + (connectedTargetDesktop != null ? connectedTargetDesktop.Name : "Remote"));
                db.Save();
                RefreshHistoryView();

                btnConnect.Text = "⚡ Connect";
                btnConnect.NormalColor = Color.FromArgb(239, 51, 64);
                lblSessionStatusBottom.Text = "Status: Idle (Standby) | Rendezvous: relay.jdroid-x.net:21116 (Active) | NAT Type: Full Cone P2P Ready";
                lblSessionStatusBottom.ForeColor = Color.FromArgb(71, 85, 105);

                ShowToast(string.Format("Session closed. Duration: {0}s", (int)duration.TotalSeconds));
                viewportCanvas.Invalidate();
            }
        }

        // ======================================================================
        // GDI+ HIGH-FIDELITY REMOTE DESKTOP SCREEN RENDERING ENGINE
        // ======================================================================

        private void ViewportCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cw = viewportCanvas.Width;
            int ch = viewportCanvas.Height;
            if (cw <= 0 || ch <= 0) return;

            if (!isSessionActive)
            {
                // -------------------------------------------------------------
                // STANDBY STATE: SOPHISTICATED TOPOLOGY & SYSTEM READY DASHBOARD
                // -------------------------------------------------------------
                using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), Color.FromArgb(15, 23, 42), Color.FromArgb(30, 41, 59), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(bg, 0, 0, cw, ch);
                }

                // Grid lines pattern
                using (Pen gridPen = new Pen(Color.FromArgb(20, 255, 255, 255), 1))
                {
                    for (int x = 0; x < cw; x += 40) g.DrawLine(gridPen, x, 0, x, ch);
                    for (int y = 0; y < ch; y += 40) g.DrawLine(gridPen, 0, y, cw, y);
                }

                // Hero Standby Card
                int cardW = Math.Min(700, cw - 60);
                int cardH = 340;
                int cardX = (cw - cardW) / 2;
                int cardY = Math.Max(20, (ch - cardH) / 2 - 20);

                using (SolidBrush cBg = new SolidBrush(Color.FromArgb(24, 32, 54)))
                using (Pen cBorder = new Pen(Color.FromArgb(51, 65, 85), 1))
                {
                    g.FillRectangle(cBg, cardX, cardY, cardW, cardH);
                    g.DrawRectangle(cBorder, cardX, cardY, cardW, cardH);
                }

                using (Font fTitle = new Font("Segoe UI", 16f, FontStyle.Bold))
                using (Font fSub = new Font("Segoe UI", 10f, FontStyle.Regular))
                using (Font fCode = new Font("Consolas", 9.5f, FontStyle.Bold))
                using (SolidBrush bWhite = new SolidBrush(Color.White))
                using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(148, 163, 184)))
                using (SolidBrush bEmerald = new SolidBrush(Color.FromArgb(16, 185, 129)))
                using (SolidBrush bBlue = new SolidBrush(Color.FromArgb(14, 165, 233)))
                {
                    g.DrawString("🖥️ Remote Desktop Engine Standby", fTitle, bWhite, cardX + 24, cardY + 24);
                    g.DrawString("Direct P2P Encrypted Session Engine (RustDesk & FreeRDP Compatible)", fSub, bMuted, cardX + 24, cardY + 58);

                    // Node Info Block
                    g.DrawString(string.Format("Active Local Host : {0} [{1}]", currentHostDesktop.Name, currentHostDesktop.RemoteId), fCode, bBlue, cardX + 24, cardY + 100);
                    g.DrawString(string.Format("Network Address   : {0}:7070 (AES-GCM-256)", currentHostDesktop.Ip), fCode, bWhite, cardX + 24, cardY + 125);
                    g.DrawString(string.Format("Operating Role    : {0}", currentHostDesktop.Role), fCode, bMuted, cardX + 24, cardY + 150);
                    g.DrawString("Rendezvous Relay  : relay.jdroid-x.net:21116 (Status: ONLINE • 11ms RTT)", fCode, bEmerald, cardX + 24, cardY + 175);

                    // Quick Action Hint
                    using (Pen sep = new Pen(Color.FromArgb(51, 65, 85), 1))
                    {
                        g.DrawLine(sep, cardX + 24, cardY + 215, cardX + cardW - 24, cardY + 215);
                    }

                    g.DrawString("READY TO CONNECT: Click any '📞 Call' button above or enter partner 9-digit ID to launch live screen stream.", fSub, bWhite, cardX + 24, cardY + 235);
                    g.DrawString("Available Dry Run Nodes: [💻 JEET-DESK: 984-102-391]  [🤖 ANTIGRAVITY: 710-224-900]  [🚀 INNOVANT: 520-310-884]", fCode, bMuted, cardX + 24, cardY + 265);
                }
            }
            else
            {
                // -------------------------------------------------------------
                // ACTIVE STATE: FULL LIVING WINDOWS 11 / SERVER DESKTOP SCREEN
                // -------------------------------------------------------------
                SimulatorDesktop target = connectedTargetDesktop ?? simulatorDesktops[1];

                if (isLiveScreenMirror)
                {
                    // Real Screen Capture from Primary Monitor
                    try
                    {
                        Bitmap screenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                        using (Graphics sG = Graphics.FromImage(screenCapture))
                        {
                            sG.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, screenCapture.Size);
                        }

                        DrawScaledImage(g, screenCapture, cw, ch);
                        screenCapture.Dispose();

                        // Watermark
                        using (SolidBrush tagBg = new SolidBrush(Color.FromArgb(180, 15, 23, 42)))
                        using (Font tagFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                        using (SolidBrush tagText = new SolidBrush(Color.FromArgb(16, 185, 129)))
                        {
                            g.FillRectangle(tagBg, 12, ch - 64, 340, 24);
                            g.DrawString("🔴 LIVE SCREEN MIRROR • 60 FPS • Real Display", tagFont, tagText, 18, ch - 60);
                        }
                        return;
                    }
                    catch { }
                }

                // Render High-Fidelity Simulated Windows Desktop
                RenderSimulatedRemoteDesktop(g, target, cw, ch);
            }
        }

        private void RenderSimulatedRemoteDesktop(Graphics g, SimulatorDesktop target, int cw, int ch)
        {
            // 1. Desktop Wallpaper (Rich Dark Slate or Azure Bloom Gradient)
            using (LinearGradientBrush wallBrush = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), Color.FromArgb(17, 24, 39), Color.FromArgb(15, 23, 42), LinearGradientMode.Vertical))
            {
                g.FillRectangle(wallBrush, 0, 0, cw, ch);
            }

            // Tech Grid Accent
            using (Pen dotPen = new Pen(Color.FromArgb(15, 255, 255, 255), 1))
            {
                for (int x = 20; x < cw; x += 32)
                {
                    for (int y = 20; y < ch - 48; y += 32)
                    {
                        g.FillRectangle(Brushes.White, x, y, 1, 1);
                    }
                }
            }

            // 2. Desktop Icons on the left
            string[] icons = new string[] {
                "💻 This PC",
                "🧠 Antigravity IDE",
                "⌨️ Windows Terminal",
                "🌐 Google Chrome",
                "🎨 Blender 4.0 Cycles",
                "🗄️ Database Hub",
                "🗑️ Recycle Bin"
            };

            int iconY = 16;
            using (Font iconFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush iconBg = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
            using (SolidBrush iconTxt = new SolidBrush(Color.White))
            {
                foreach (string icon in icons)
                {
                    g.FillRectangle(iconBg, 16, iconY, 130, 28);
                    g.DrawString(icon, iconFont, iconTxt, 22, iconY + 6);
                    iconY += 38;
                }
            }

            // 3. Remote Window 1: Primary Task (Dynamic per Target)
            int win1X = Math.Min(170, cw / 4);
            int win1Y = 24;
            int win1W = Math.Max(380, (cw - win1X - 30) * 3 / 5);
            int win1H = Math.Max(260, ch - 120);

            DrawRemoteWindow(g, win1X, win1Y, win1W, win1H, "🗔 " + target.ActiveApps[0] + " - [Remote Host: " + target.Name + "]", () => {
                using (Font bodyF = new Font("Segoe UI", 9f, FontStyle.Regular))
                using (Font codeF = new Font("Consolas", 8.5f, FontStyle.Regular))
                using (SolidBrush bWhite = new SolidBrush(Color.White))
                using (SolidBrush bGreen = new SolidBrush(Color.FromArgb(16, 185, 129)))
                using (SolidBrush bBlue = new SolidBrush(Color.FromArgb(14, 165, 233)))
                using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    g.DrawString("Process PID: 14208 | Thread Count: 48 | Priority: RealTime", codeF, bGreen, win1X + 16, win1Y + 38);
                    g.DrawString("Operating System: " + target.OsVersion, bodyF, bWhite, win1X + 16, win1Y + 58);
                    g.DrawString("Assigned Role   : " + target.Role, bodyF, bBlue, win1X + 16, win1Y + 78);
                    g.DrawString("Direct Network  : " + target.Ip + ":7070 (Active TLS Stream)", bodyF, bMuted, win1X + 16, win1Y + 98);

                    // Simulated Real-Time Activity Box
                    Rectangle actBox = new Rectangle(win1X + 16, win1Y + 124, win1W - 32, win1H - 140);
                    using (SolidBrush abBg = new SolidBrush(Color.FromArgb(15, 23, 42)))
                    using (Pen abBorder = new Pen(Color.FromArgb(51, 65, 85), 1))
                    {
                        g.FillRectangle(abBg, actBox);
                        g.DrawRectangle(abBorder, actBox);
                    }

                    g.DrawString(">> Live Remote Telemetry Output Stream:", codeF, bGreen, actBox.X + 8, actBox.Y + 8);
                    g.DrawString(string.Format("[{0}] Direct P2P Video Channel: H.264 / AV1 60 FPS", DateTime.Now.ToString("HH:mm:ss")), codeF, bWhite, actBox.X + 8, actBox.Y + 28);
                    g.DrawString(string.Format("[{0}] End-to-End Encryption: AES-256-GCM Handshake Validated", DateTime.Now.ToString("HH:mm:ss")), codeF, bWhite, actBox.X + 8, actBox.Y + 46);
                    g.DrawString(string.Format("[{0}] Remote Input: Keyboard & Mouse synchronized (0 ms lag)", DateTime.Now.ToString("HH:mm:ss")), codeF, bMuted, actBox.X + 8, actBox.Y + 64);
                    g.DrawString(string.Format("[{0}] GPU Engine: Hardware Acceleration Enabled (DirectX 12)", DateTime.Now.ToString("HH:mm:ss")), codeF, bBlue, actBox.X + 8, actBox.Y + 82);
                    if (actBox.Height > 140)
                    {
                        g.DrawString(string.Format("[{0}] Host Workstation Service: jDroid-X Service v2.0 Healthy", DateTime.Now.ToString("HH:mm:ss")), codeF, bGreen, actBox.X + 8, actBox.Y + 100);
                    }
                }
            });

            // 4. Remote Window 2: Diagnostics / Second App (Docked Right if wide enough)
            if (cw >= 1150)
            {
                int win2X = win1X + win1W + 12;
                int win2W = cw - win2X - 16;
                int win2Y = 24;
                int win2H = win1H;

                DrawRemoteWindow(g, win2X, win2Y, win2W, win2H, "📊 Remote Node Hardware & GPU Telemetry", () => {
                    using (Font codeF = new Font("Consolas", 8.5f, FontStyle.Regular))
                    using (SolidBrush bWhite = new SolidBrush(Color.White))
                    using (SolidBrush bGreen = new SolidBrush(Color.FromArgb(16, 185, 129)))
                    using (SolidBrush bAmber = new SolidBrush(Color.FromArgb(245, 158, 11)))
                    using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    {
                        g.DrawString("Remote CPU Load: 14.2% [16 Cores]", codeF, bGreen, win2X + 12, win2Y + 38);
                        g.DrawString("Memory Footprint: 8.4 GB / 64 GB", codeF, bWhite, win2X + 12, win2Y + 58);
                        g.DrawString("GPU Memory (VRAM): 4.1 GB / 24 GB", codeF, bAmber, win2X + 12, win2Y + 78);
                        g.DrawString("Round-Trip Time: 11.2 ms (LAN P2P)", codeF, bGreen, win2X + 12, win2Y + 98);

                        // Visual CPU Bar
                        Rectangle barRect = new Rectangle(win2X + 12, win2Y + 125, win2W - 24, 18);
                        using (SolidBrush bbg = new SolidBrush(Color.FromArgb(15, 23, 42)))
                        using (SolidBrush bfill = new SolidBrush(Color.FromArgb(16, 185, 129)))
                        {
                            g.FillRectangle(bbg, barRect);
                            g.FillRectangle(bfill, barRect.X, barRect.Y, (barRect.Width * 35) / 100, barRect.Height);
                        }

                        g.DrawString("Node Active Apps: " + target.ActiveApps[1], codeF, bMuted, win2X + 12, win2Y + 155);
                        if (target.ActiveApps.Length > 2)
                        {
                            g.DrawString("Background Worker: " + target.ActiveApps[2], codeF, bMuted, win2X + 12, win2Y + 175);
                        }
                    }
                });
            }

            // 5. Windows Taskbar at bottom of remote screen
            int taskH = 44;
            int taskY = ch - taskH;
            using (SolidBrush taskBg = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                g.FillRectangle(taskBg, 0, taskY, cw, taskH);
            }
            using (Pen taskSep = new Pen(Color.FromArgb(51, 65, 85), 1))
            {
                g.DrawLine(taskSep, 0, taskY, cw, taskY);
            }

            // Start Button
            using (SolidBrush startBg = new SolidBrush(isStartMenuOpen ? Color.FromArgb(2, 132, 199) : Color.FromArgb(30, 41, 59)))
            using (Font startF = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (SolidBrush whiteB = new SolidBrush(Color.White))
            {
                g.FillRectangle(startBg, 8, taskY + 5, 80, 34);
                g.DrawString("⊞ Start", startF, whiteB, 20, taskY + 12);
            }

            // Taskbar Pinned Icons
            string[] taskApps = new string[] { "🧠 Antigravity", "⌨️ Terminal", "🌐 Edge", "📁 Explorer", "⚙️ TaskMgr" };
            int appX = 96;
            using (Font appF = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (SolidBrush appBg = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (SolidBrush appTxt = new SolidBrush(Color.White))
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(14, 165, 233)))
            {
                foreach (string a in taskApps)
                {
                    g.FillRectangle(appBg, appX, taskY + 6, 95, 32);
                    g.DrawString(a, appF, appTxt, appX + 6, taskY + 14);
                    // Blue active underline
                    g.FillRectangle(glow, appX + 10, taskY + 36, 75, 2);
                    appX += 102;
                }
            }

            // System Tray (Right)
            using (Font trayF = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (SolidBrush trayTxt = new SolidBrush(Color.FromArgb(203, 213, 225)))
            {
                string timeStr = DateTime.Now.ToString("HH:mm:ss");
                string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                g.DrawString("📶 🔊 🔋 " + timeStr + "\n" + dateStr, trayF, trayTxt, cw - 150, taskY + 6);
            }

            // 6. Interactive Start Menu Popup (if open)
            if (isStartMenuOpen)
            {
                int smW = 280;
                int smH = 260;
                int smX = 8;
                int smY = taskY - smH - 4;

                using (SolidBrush smBg = new SolidBrush(Color.FromArgb(30, 41, 59)))
                using (Pen smBorder = new Pen(Color.FromArgb(71, 85, 105), 1))
                using (Font smTitle = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                using (Font smItem = new Font("Segoe UI", 9f))
                using (SolidBrush bWhite = new SolidBrush(Color.White))
                using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    g.FillRectangle(smBg, smX, smY, smW, smH);
                    g.DrawRectangle(smBorder, smX, smY, smW, smH);

                    g.DrawString("Pinned Applications", smTitle, bWhite, smX + 14, smY + 12);
                    string[] menuItems = new string[] {
                        "🧠 Antigravity AI Orchestrator",
                        "⌨️ Windows PowerShell (Admin)",
                        "⚙️ Windows Settings & Control Panel",
                        "📊 Process Task Manager",
                        "📁 File Explorer (C:\\Remote)",
                        "🔒 Lock Computer / Sign out",
                        "🔄 Restart Remote Node"
                    };

                    int miY = smY + 40;
                    foreach (string mi in menuItems)
                    {
                        g.DrawString(mi, smItem, bMuted, smX + 14, miY);
                        miY += 28;
                    }
                }
            }
        }

        private void DrawRemoteWindow(Graphics g, int x, int y, int w, int h, string title, Action renderContent)
        {
            using (SolidBrush winBg = new SolidBrush(Color.FromArgb(24, 32, 54)))
            using (SolidBrush titleBg = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (Pen borderPen = new Pen(Color.FromArgb(51, 65, 85), 1))
            using (Font titleFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush titleText = new SolidBrush(Color.White))
            {
                // Window Frame
                g.FillRectangle(winBg, x, y, w, h);
                g.FillRectangle(titleBg, x, y, w, 28);
                g.DrawRectangle(borderPen, x, y, w, h);

                // Title
                g.DrawString(title, titleFont, titleText, x + 8, y + 6);

                // Window Controls
                using (SolidBrush bClose = new SolidBrush(Color.FromArgb(239, 68, 68)))
                using (SolidBrush bMin = new SolidBrush(Color.FromArgb(245, 158, 11)))
                using (SolidBrush bMax = new SolidBrush(Color.FromArgb(16, 185, 129)))
                {
                    g.FillEllipse(bClose, x + w - 20, y + 8, 11, 11);
                    g.FillEllipse(bMin, x + w - 36, y + 8, 11, 11);
                    g.FillEllipse(bMax, x + w - 52, y + 8, 11, 11);
                }

                // Inner content
                if (renderContent != null) renderContent();
            }
        }

        private void DrawScaledImage(Graphics g, Image img, int cw, int ch)
        {
            if (smartFitMode == "SmartFit")
            {
                float ratio = Math.Min((float)cw / img.Width, (float)ch / img.Height);
                int targetW = (int)(img.Width * ratio);
                int targetH = (int)(img.Height * ratio);
                int targetX = (cw - targetW) / 2;
                int targetY = (ch - targetH) / 2;
                g.DrawImage(img, targetX, targetY, targetW, targetH);
            }
            else if (smartFitMode == "Original 1:1")
            {
                g.DrawImage(img, 0, 0, img.Width, img.Height);
            }
            else
            {
                g.DrawImage(img, 0, 0, cw, ch);
            }
        }

        private void ViewportCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isSessionActive) return;

            int ch = viewportCanvas.Height;
            // Check if clicked the Start button in bottom taskbar
            if (e.Y >= ch - 44 && e.X >= 8 && e.X <= 88)
            {
                isStartMenuOpen = !isStartMenuOpen;
                viewportCanvas.Invalidate();
            }
            else if (isStartMenuOpen)
            {
                isStartMenuOpen = false;
                viewportCanvas.Invalidate();
            }
        }

        private void ViewportCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Interactive mouse tracking
        }

        // ======================================================================
        // VIEW 2: DUAL-PANE DIRECT FILE TRANSFER EXPLORER
        // ======================================================================

        private void InitializeFileTransferView()
        {
            viewFileTransfer = new Panel();
            viewFileTransfer.Dock = DockStyle.Fill;
            viewFileTransfer.Padding = new Padding(16);

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 48 };
            Label lblTitle = new Label { Text = "Direct P2P File Transfer (RustDesk / AnyDesk Virtual Channel)", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            topRow.Controls.Add(lblTitle);

            TableLayoutPanel explorerGrid = new TableLayoutPanel();
            explorerGrid.Dock = DockStyle.Fill;
            explorerGrid.ColumnCount = 3;
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            // Left Pane: Local Machine
            ModernCardPanel cardLocalFiles = new ModernCardPanel { Dock = DockStyle.Fill };
            Panel lTop = new Panel { Dock = DockStyle.Top, Height = 42 };
            lblLocalPath = new Label { Text = "Local Workstation (C:\\Users\\jiten\\jAnitGravity\\jDroid-X-Desk)", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(4, 4), AutoSize = true };
            lTop.Controls.Add(lblLocalPath);
            lbLocalFiles = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9f), BorderStyle = BorderStyle.FixedSingle };
            PopulateLocalFiles();
            cardLocalFiles.Controls.Add(lbLocalFiles);
            cardLocalFiles.Controls.Add(lTop);

            // Middle: Transfer Action Buttons
            Panel midActions = new Panel { Dock = DockStyle.Fill };
            Button btnUpload = new Button { Text = "➡️ Upload", Size = new Size(95, 34), Location = new Point(8, 120), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpload.Click += (s, e) => {
                if (lbLocalFiles.SelectedItem != null)
                {
                    string sel = lbLocalFiles.SelectedItem.ToString();
                    lbRemoteFiles.Items.Add(sel);
                    ShowToast("Uploaded " + sel + " to remote workstation.");
                }
            };
            Button btnDownload = new Button { Text = "⬅️ Download", Size = new Size(95, 34), Location = new Point(8, 165), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(2, 132, 199), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDownload.Click += (s, e) => {
                if (lbRemoteFiles.SelectedItem != null)
                {
                    string sel = lbRemoteFiles.SelectedItem.ToString();
                    lbLocalFiles.Items.Add(sel);
                    ShowToast("Downloaded " + sel + " to local machine.");
                }
            };
            Button btnRefresh = new Button { Text = "🔄 Refresh", Size = new Size(95, 30), Location = new Point(8, 210), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnRefresh.Click += (s, e) => { PopulateLocalFiles(); ShowToast("Synchronized directory listings."); };

            midActions.Controls.Add(btnUpload);
            midActions.Controls.Add(btnDownload);
            midActions.Controls.Add(btnRefresh);

            // Right Pane: Remote Target Machine
            ModernCardPanel cardRemoteFiles = new ModernCardPanel { Dock = DockStyle.Fill };
            Panel rTop = new Panel { Dock = DockStyle.Top, Height = 42 };
            lblRemotePath = new Label { Text = "Remote Target Files (C:\\RemoteWorkstation\\Projects)", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(4, 4), AutoSize = true };
            rTop.Controls.Add(lblRemotePath);
            lbRemoteFiles = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9f), BorderStyle = BorderStyle.FixedSingle };
            lbRemoteFiles.Items.Add("[DIR]  C:\\RemoteWorkstation\\Builds");
            lbRemoteFiles.Items.Add("[DIR]  C:\\RemoteWorkstation\\Logs");
            lbRemoteFiles.Items.Add("[FILE] server_telemetry.log    (142 KB)");
            lbRemoteFiles.Items.Add("[FILE] cuda_kernel_pipeline.dll (1.8 MB)");
            lbRemoteFiles.Items.Add("[FILE] deepmind_cognitive.weights (84 MB)");
            cardRemoteFiles.Controls.Add(lbRemoteFiles);
            cardRemoteFiles.Controls.Add(rTop);

            explorerGrid.Controls.Add(cardLocalFiles, 0, 0);
            explorerGrid.Controls.Add(midActions, 1, 0);
            explorerGrid.Controls.Add(cardRemoteFiles, 2, 0);

            viewFileTransfer.Controls.Add(explorerGrid);
            viewFileTransfer.Controls.Add(topRow);
            topRow.BringToFront();
        }

        private void PopulateLocalFiles()
        {
            if (lbLocalFiles == null) return;
            lbLocalFiles.Items.Clear();
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    lbLocalFiles.Items.Add("[DIR]  " + Path.GetFileName(d));
                }
                foreach (string f in Directory.GetFiles(dir))
                {
                    FileInfo fi = new FileInfo(f);
                    lbLocalFiles.Items.Add(string.Format("[FILE] {0,-26} ({1} KB)", fi.Name, fi.Length / 1024));
                }
            }
            catch { lbLocalFiles.Items.Add("Default root files loaded."); }
        }

        // ======================================================================
        // VIEW 3: ADMIN POWERSHELL / COMMAND LINE VIEW
        // ======================================================================

        private void InitializeTerminalView()
        {
            viewTerminal = new Panel();
            viewTerminal.Dock = DockStyle.Fill;
            viewTerminal.Padding = new Padding(16);

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 44 };
            Label lblTitle = new Label { Text = "Elevated Remote Administrative Shell (Windows PowerShell)", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            topRow.Controls.Add(lblTitle);

            terminalBox = new RichTextBox();
            terminalBox.Dock = DockStyle.Fill;
            terminalBox.BackColor = Color.FromArgb(12, 16, 28);
            terminalBox.ForeColor = Color.FromArgb(16, 185, 129);
            terminalBox.Font = new Font("Consolas", 10f, FontStyle.Regular);
            terminalBox.ReadOnly = true;
            terminalBox.Text = "Windows PowerShell [Elevated Remote Terminal Session]\nType diagnostic commands: ping, ipconfig, systeminfo, hostname, tasklist, clear\n\nPS C:\\Users\\jiten> ";

            Panel cmdBottom = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(20, 27, 45) };
            terminalInput = new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 27, 45), ForeColor = Color.White, Font = new Font("Consolas", 10.5f, FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle };
            terminalInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ExecuteTerminalCommand(); };

            Button btnExec = new Button { Text = "Execute", Dock = DockStyle.Right, Width = 90, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(2, 132, 199), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExec.Click += (s, e) => ExecuteTerminalCommand();

            cmdBottom.Controls.Add(terminalInput);
            cmdBottom.Controls.Add(btnExec);

            viewTerminal.Controls.Add(terminalBox);
            viewTerminal.Controls.Add(cmdBottom);
            viewTerminal.Controls.Add(topRow);
            topRow.BringToFront();
        }

        private void ExecuteTerminalCommand()
        {
            string cmd = terminalInput.Text.Trim();
            if (string.IsNullOrEmpty(cmd)) return;

            terminalBox.AppendText(cmd + "\n");
            terminalInput.Text = "";

            if (cmd.ToLower() == "clear" || cmd.ToLower() == "cls")
            {
                terminalBox.Text = "Windows PowerShell [Elevated Remote Diagnostic Console]\n\nPS C:\\Users\\jiten> ";
                return;
            }

            ThreadPool.QueueUserWorkItem((state) => {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + cmd);
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;

                    Process p = Process.Start(psi);
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit(3000);

                    this.Invoke((MethodInvoker)delegate {
                        if (!string.IsNullOrEmpty(output)) terminalBox.AppendText(output + "\n");
                        if (!string.IsNullOrEmpty(error)) terminalBox.AppendText("[Error] " + error + "\n");
                        terminalBox.AppendText("PS C:\\Users\\jiten> ");
                        terminalBox.SelectionStart = terminalBox.Text.Length;
                        terminalBox.ScrollToCaret();
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate {
                        terminalBox.AppendText("[Exec Exception] " + ex.Message + "\nPS C:\\Users\\jiten> ");
                    });
                }
            });
        }

        // ======================================================================
        // VIEW 4: FLEET WORKSTATIONS
        // ======================================================================

        private void InitializeFleetView()
        {
            viewFleet = new Panel();
            viewFleet.Dock = DockStyle.Fill;
            viewFleet.Padding = new Padding(16);

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 50 };
            Label lblTitle = new Label { Text = "Registered Device Fleet & Connected Workstations", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

            FlowLayoutPanel actionsFlow = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 8, 0, 8) };

            Label lblGroup = new Label { Text = "Group By:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Margin = new Padding(0, 6, 4, 0), AutoSize = true };
            cboFleetGroupFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f), Size = new Size(180, 28), Margin = new Padding(0, 2, 10, 0) };
            PopulateFleetGroupDropdown();
            cboFleetGroupFilter.SelectedIndexChanged += (s, e) => FilterFleetGrid();

            Button btnAddDev = new Button { Text = "➕ Register Workstation", Size = new Size(160, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0) };
            btnAddDev.Click += (s, e) => {
                string name = PromptInputDialog("Register Workstation", "Enter workstation hostname / name:", "Core-Node-04");
                string ip = PromptInputDialog("Network IP Address", "Enter static or DHCP IP:", "192.168.1.188");
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(ip)) {
                    string selGroup = cboFleetGroupFilter.SelectedIndex > 0 ? cboFleetGroupFilter.SelectedItem.ToString() : "Lead Architects";
                    GroupItem matchedGrp = db.Groups.Find(g => g.name == selGroup);
                    string gId = matchedGrp != null ? matchedGrp.id : "grp-1";
                    db.Devices.Add(new DeviceItem { id = "DEV-" + (db.Devices.Count + 1), name = name, ip = ip, groupId = gId, group = selGroup, status = "ONLINE", accessLevel = "Standard" });
                    db.Save();
                    FilterFleetGrid();
                    ShowToast("Workstation registered: " + name + " in group " + selGroup);
                }
            };

            Button btnDeleteDev = new Button { Text = "🗑️ Delete", Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(239, 51, 64), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDeleteDev.Click += (s, e) => {
                if (dgvFleet.CurrentRow != null) {
                    DeviceItem item = dgvFleet.CurrentRow.DataBoundItem as DeviceItem;
                    if (item != null && MessageBox.Show("Remove workstation " + item.name + " from fleet?", "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                        db.Devices.Remove(item);
                        db.Save();
                        FilterFleetGrid();
                        ShowToast("Workstation removed from fleet.");
                    }
                }
            };

            actionsFlow.Controls.Add(lblGroup);
            actionsFlow.Controls.Add(cboFleetGroupFilter);
            actionsFlow.Controls.Add(btnAddDev);
            actionsFlow.Controls.Add(btnDeleteDev);

            topRow.Controls.Add(lblTitle);
            topRow.Controls.Add(actionsFlow);

            dgvFleet = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

            viewFleet.Controls.Add(topRow);
            viewFleet.Controls.Add(dgvFleet);
            topRow.BringToFront();

            FilterFleetGrid();
        }

        private void PopulateFleetGroupDropdown()
        {
            if (cboFleetGroupFilter == null) return;
            cboFleetGroupFilter.Items.Clear();
            cboFleetGroupFilter.Items.Add("All Groups (Complete Fleet)");
            foreach (GroupItem g in db.Groups) cboFleetGroupFilter.Items.Add(g.name);
            cboFleetGroupFilter.SelectedIndex = 0;
        }

        private void FilterFleetGrid()
        {
            if (dgvFleet == null) return;
            string selectedGroup = cboFleetGroupFilter.SelectedItem != null ? cboFleetGroupFilter.SelectedItem.ToString() : "All Groups (Complete Fleet)";
            if (selectedGroup == "All Groups (Complete Fleet)")
            {
                dgvFleet.DataSource = null;
                dgvFleet.DataSource = new List<DeviceItem>(db.Devices);
            }
            else
            {
                List<DeviceItem> filtered = db.Devices.FindAll(d => d.group == selectedGroup || d.groupId == selectedGroup);
                dgvFleet.DataSource = null;
                dgvFleet.DataSource = filtered;
            }
        }

        // ======================================================================
        // VIEW 5: SESSION HISTORY & AUDIT LOGS
        // ======================================================================

        private void InitializeHistoryView()
        {
            viewHistory = new Panel();
            viewHistory.Dock = DockStyle.Fill;
            viewHistory.Padding = new Padding(16);

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 50 };
            Label lblTitle = new Label { Text = "Connection History & Auditable Session Logs", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

            FlowLayoutPanel actionsFlow = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 8, 0, 8) };

            Button btnRefresh = new Button { Text = "🔄 Refresh History", Size = new Size(140, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(2, 132, 199), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0) };
            btnRefresh.Click += (s, e) => { RefreshHistoryView(); ShowToast("History synchronized with database.json"); };

            Button btnClearHist = new Button { Text = "🗑️ Clear History", Size = new Size(130, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(239, 51, 64), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClearHist.Click += (s, e) => {
                if (MessageBox.Show("Are you sure you want to clear session logs?", "Clear History", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    db.Sessions.Clear();
                    db.Save();
                    RefreshHistoryView();
                    ShowToast("Session history cleared.");
                }
            };

            actionsFlow.Controls.Add(btnRefresh);
            actionsFlow.Controls.Add(btnClearHist);
            topRow.Controls.Add(lblTitle);
            topRow.Controls.Add(actionsFlow);

            dgvHistory = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

            viewHistory.Controls.Add(topRow);
            viewHistory.Controls.Add(dgvHistory);
            topRow.BringToFront();

            RefreshHistoryView();
        }

        private void RefreshHistoryView()
        {
            if (dgvHistory == null) return;
            dgvHistory.DataSource = null;
            dgvHistory.DataSource = new List<SessionItem>(db.Sessions);
        }

        // ======================================================================
        // VIEW 6: GUARDIANS ROSTER
        // ======================================================================

        private void InitializeGuardiansView()
        {
            viewGuardians = new Panel();
            viewGuardians.Dock = DockStyle.Fill;
            viewGuardians.Padding = new Padding(16);

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 50 };
            Label lblTitle = new Label { Text = "Security Guardians & Authorized Operators", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

            FlowLayoutPanel actionsFlow = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 8, 0, 8) };

            Button btnAddGrd = new Button { Text = "🛡️ Add Guardian", Size = new Size(150, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0) };
            btnAddGrd.Click += (s, e) => {
                string name = PromptInputDialog("Add Guardian", "Enter Guardian Name:", "Security Officer");
                string role = PromptInputDialog("Guardian Role", "Enter Guardian Role:", "Field Support Engineer");
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(role)) {
                    db.Guardians.Add(new GuardianItem { id = "grd-" + (db.Guardians.Count + 1), name = name, role = role, contact = "operator@enterprise.local", status = "Active" });
                    db.Save();
                    RefreshGuardiansView();
                    ShowToast("Guardian " + name + " added.");
                }
            };

            Button btnToggleStatus = new Button { Text = "⚡ Toggle Status", Size = new Size(130, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(2, 132, 199), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnToggleStatus.Click += (s, e) => {
                if (dgvGuardians.CurrentRow != null) {
                    GuardianItem g = dgvGuardians.CurrentRow.DataBoundItem as GuardianItem;
                    if (g != null) {
                        g.status = g.status == "Active" ? "Standby" : "Active";
                        db.Save();
                        RefreshGuardiansView();
                        ShowToast("Guardian status updated: " + g.name + " -> " + g.status);
                    }
                }
            };

            actionsFlow.Controls.Add(btnAddGrd);
            actionsFlow.Controls.Add(btnToggleStatus);
            topRow.Controls.Add(lblTitle);
            topRow.Controls.Add(actionsFlow);

            dgvGuardians = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

            viewGuardians.Controls.Add(topRow);
            viewGuardians.Controls.Add(dgvGuardians);
            topRow.BringToFront();

            RefreshGuardiansView();
        }

        private void RefreshGuardiansView()
        {
            if (dgvGuardians == null) return;
            dgvGuardians.DataSource = null;
            dgvGuardians.DataSource = new List<GuardianItem>(db.Guardians);
        }

        // ======================================================================
        // VIEW 7: SAFE ZONES & IP WHITELISTS
        // ======================================================================

        private void InitializeSafeZonesView()
        {
            viewSafeZones = new Panel();
            viewSafeZones.Dock = DockStyle.Fill;
            viewSafeZones.Padding = new Padding(16);

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 50 };
            Label lblTitle = new Label { Text = "Safe Zones & IP Subnet Whitelist Perimeter", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

            FlowLayoutPanel actionsFlow = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 8, 0, 8) };

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, DataSource = db.SafeZones };

            Button btnAddZone = new Button { Text = "🔒 Add Safe Zone", Size = new Size(150, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0) };
            btnAddZone.Click += (s, e) => {
                string name = PromptInputDialog("Add Safe Zone", "Enter zone name:", "Branch Office Subnet");
                string ip = PromptInputDialog("Subnet / CIDR", "Enter IPv4 or CIDR:", "192.168.10.0/24");
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(ip)) {
                    db.SafeZones.Add(new SafeZoneItem { id = "SZ-" + (db.SafeZones.Count + 1), name = name, ip = ip });
                    db.Save();
                    dgv.DataSource = null;
                    dgv.DataSource = db.SafeZones;
                }
            };

            Button btnEmergencyKill = new Button { Text = "🚨 Emergency Kill Switch", ForeColor = Color.White, BackColor = Color.FromArgb(239, 51, 64), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand, Size = new Size(180, 30) };
            btnEmergencyKill.Click += (s, e) => {
                if (MessageBox.Show("EMERGENCY ACTION: Sever all active remote desktop sessions immediately?", "Emergency Kill Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                    isSessionActive = false;
                    btnConnect.Text = "⚡ Connect";
                    btnConnect.NormalColor = Color.FromArgb(239, 51, 64);
                    lblSessionStatusBottom.Text = "EMERGENCY ISOLATION ENGAGED: All remote connections terminated.";
                    lblSessionStatusBottom.ForeColor = Color.Red;
                    db.AddLog("SECURITY_AUDIT", "KILL_SWITCH", "Operator JEET triggered emergency session isolation");
                    viewportCanvas.Invalidate();
                }
            };

            actionsFlow.Controls.Add(btnAddZone);
            actionsFlow.Controls.Add(btnEmergencyKill);
            topRow.Controls.Add(lblTitle);
            topRow.Controls.Add(actionsFlow);

            viewSafeZones.Controls.Add(topRow);
            viewSafeZones.Controls.Add(dgv);
            topRow.BringToFront();
        }

        // ======================================================================
        // VIEW 8: HARDWARE TELEMETRY & SYSTEM DIAGNOSTICS
        // ======================================================================

        private void InitializeHealthView()
        {
            viewHealth = new Panel();
            viewHealth.Dock = DockStyle.Fill;
            viewHealth.Padding = new Padding(16);

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 44 };
            Label lblTitle = new Label { Text = "Host Hardware Diagnostics & Real OS Telemetry", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            topRow.Controls.Add(lblTitle);

            TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            // CPU Card
            ModernCardPanel cardCpu = new ModernCardPanel { Dock = DockStyle.Fill };
            cardCpu.Controls.Add(new Label { Text = "CPU Core Allocation", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(14, 12), AutoSize = true });
            cardCpu.Controls.Add(new Label { Text = Environment.ProcessorCount + " Active Logical Cores", Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = Color.FromArgb(2, 132, 199), Location = new Point(14, 45), AutoSize = true });
            cardCpu.Controls.Add(new Label { Text = "Architecture: x64 | Windows Host Engine", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(14, 90), AutoSize = true });

            // Memory Card
            ModernCardPanel cardMem = new ModernCardPanel { Dock = DockStyle.Fill };
            long ramBytes = GC.GetTotalMemory(false);
            cardMem.Controls.Add(new Label { Text = "Managed Working Memory", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(14, 12), AutoSize = true });
            cardMem.Controls.Add(new Label { Text = (ramBytes / (1024 * 1024)) + " MB Allocated", Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Location = new Point(14, 45), AutoSize = true });
            cardMem.Controls.Add(new Label { Text = "Real-time process memory footprint (zero leak)", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(14, 90), AutoSize = true });

            // Uptime Card
            ModernCardPanel cardUptime = new ModernCardPanel { Dock = DockStyle.Fill };
            int uptimeSeconds = Environment.TickCount / 1000;
            TimeSpan uptime = TimeSpan.FromSeconds(uptimeSeconds);
            cardUptime.Controls.Add(new Label { Text = "System Uptime", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(14, 12), AutoSize = true });
            cardUptime.Controls.Add(new Label { Text = string.Format("{0}h {1}m {2}s", (int)uptime.TotalHours, uptime.Minutes, uptime.Seconds), Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = Color.FromArgb(245, 158, 11), Location = new Point(14, 45), AutoSize = true });
            cardUptime.Controls.Add(new Label { Text = "Node Hostname: " + Environment.MachineName, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(14, 90), AutoSize = true });

            // NICs Card
            ModernCardPanel cardNic = new ModernCardPanel { Dock = DockStyle.Fill };
            cardNic.Controls.Add(new Label { Text = "Active Network Adapters (NICs)", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(14, 12), AutoSize = true });
            ListBox lbNics = new ListBox { Location = new Point(14, 40), Size = new Size(380, 100), BorderStyle = BorderStyle.None, Font = new Font("Consolas", 8.5f) };
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        lbNics.Items.Add(string.Format("{0} ({1}) - {2} Mbps", nic.Name, nic.NetworkInterfaceType, nic.Speed / 1000000));
                    }
                }
            } catch { lbNics.Items.Add("Default IPv4 Adapter Active"); }
            cardNic.Controls.Add(lbNics);

            grid.Controls.Add(cardCpu, 0, 0);
            grid.Controls.Add(cardMem, 1, 0);
            grid.Controls.Add(cardUptime, 0, 1);
            grid.Controls.Add(cardNic, 1, 1);

            viewHealth.Controls.Add(topRow);
            viewHealth.Controls.Add(grid);
            topRow.BringToFront();
        }

        // ======================================================================
        // VIEW 9: ANYDESK & RUSTDESK SETTINGS STUDIO
        // ======================================================================

        private void InitializeSettingsStudioView()
        {
            viewSettingsStudio = new Panel();
            viewSettingsStudio.Dock = DockStyle.Fill;
            viewSettingsStudio.Padding = new Padding(16);

            Panel studioHeader = new Panel { Dock = DockStyle.Top, Height = 50 };
            Label lblStudio = new Label { Text = "AnyDesk & RustDesk Settings Studio", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft, AutoSize = true };

            ModernButton btnSaveSettings = new ModernButton { Text = "💾 Save & Apply Settings", NormalColor = Color.FromArgb(16, 185, 129), HoverColor = Color.FromArgb(21, 150, 78), Size = new Size(190, 36), Dock = DockStyle.Right };
            btnSaveSettings.Click += (s, e) => SaveSettingsToDb();

            studioHeader.Controls.Add(btnSaveSettings);
            studioHeader.Controls.Add(lblStudio);
            viewSettingsStudio.Controls.Add(studioHeader);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 260 };
            settingsTree = new TreeView { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle };

            settingsTree.Nodes.Add("General", "General (Paths, Prompts, Startup)");
            settingsTree.Nodes.Add("Connection", "Connection & RustDesk Relay Server");
            settingsTree.Nodes.Add("Access", "Access Control & Passwords (ACL)");
            settingsTree.Nodes.Add("Display", "Display & Hardware Acceleration");
            settingsTree.Nodes.Add("Audio", "Audio Output & Transmission");
            settingsTree.Nodes.Add("Account", "Licensing & Cluster Credentials");

            settingsTree.AfterSelect += (s, e) => RenderSettingsSubPanel(e.Node.Name);
            split.Panel1.Controls.Add(settingsTree);

            settingsSubContentPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, Padding = new Padding(20), BorderStyle = BorderStyle.FixedSingle };
            split.Panel2.Controls.Add(settingsSubContentPanel);

            viewSettingsStudio.Controls.Add(split);
            settingsTree.SelectedNode = settingsTree.Nodes[0];
        }

        private void RenderSettingsSubPanel(string category)
        {
            settingsSubContentPanel.Controls.Clear();
            int y = 14;

            Action<string> addHeading = (text) => {
                Label l = new Label { Text = text, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(14, y), AutoSize = true };
                settingsSubContentPanel.Controls.Add(l);
                y += 34;
            };

            Func<string, bool, CheckBox> addCheck = (text, isChecked) => {
                CheckBox cb = new CheckBox { Text = text, Checked = isChecked, Font = new Font("Segoe UI", 9.5f), Location = new Point(20, y), AutoSize = true, Cursor = Cursors.Hand };
                settingsSubContentPanel.Controls.Add(cb);
                y += 30;
                return cb;
            };

            Func<string, string, TextBox> addText = (label, val) => {
                Label lbl = new Label { Text = label, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(20, y), AutoSize = true };
                settingsSubContentPanel.Controls.Add(lbl);
                y += 22;
                TextBox tb = new TextBox { Text = val, Font = new Font("Segoe UI", 9.5f), Location = new Point(20, y), Size = new Size(380, 26) };
                settingsSubContentPanel.Controls.Add(tb);
                y += 36;
                return tb;
            };

            if (category == "General")
            {
                addHeading("General Application Settings");
                CheckBox cbStartup = addCheck("Open address book on application startup", db.Settings.general.openAddressBookOnStartup);
                cbStartup.CheckedChanged += (s, e) => db.Settings.general.openAddressBookOnStartup = cbStartup.Checked;
                CheckBox cbAutoUpdate = addCheck("Automatically download and install stable updates", db.Settings.general.autoUpdate);
                cbAutoUpdate.CheckedChanged += (s, e) => db.Settings.general.autoUpdate = cbAutoUpdate.Checked;
                addText("Application Language", "English (United States)");
            }
            else if (category == "Connection")
            {
                addHeading("Network & RustDesk Server Configuration");
                TextBox tbIdServer = addText("RustDesk ID Server (hbbs - Port 21116)", db.Settings.connection.idServer);
                tbIdServer.TextChanged += (s, e) => db.Settings.connection.idServer = tbIdServer.Text.Trim();
                TextBox tbRelay = addText("RustDesk Relay Server (hbbr - Port 21117)", db.Settings.connection.relayServer);
                tbRelay.TextChanged += (s, e) => db.Settings.connection.relayServer = tbRelay.Text.Trim();
                CheckBox cbDirect = addCheck("Enable Direct P2P Connections over LAN / UPnP", db.Settings.connection.directConnections);
                cbDirect.CheckedChanged += (s, e) => db.Settings.connection.directConnections = cbDirect.Checked;
            }
            else if (category == "Access")
            {
                addHeading("Access Control & Security Passwords");
                CheckBox cbUnattended = addCheck("Enable Unattended Access", db.Settings.access.unattendedAccess);
                cbUnattended.CheckedChanged += (s, e) => db.Settings.access.unattendedAccess = cbUnattended.Checked;
                TextBox tbTimeout = addText("Session Inactivity Timeout (minutes)", db.Settings.access.accessTimeout.ToString());
                tbTimeout.TextChanged += (s, e) => {
                    int val;
                    if (int.TryParse(tbTimeout.Text, out val)) db.Settings.access.accessTimeout = val;
                };
            }
            else if (category == "Display")
            {
                addHeading("Display & Quality Calibration");
                addText("Active View Scaling Mode", smartFitMode);
                addText("Streaming Quality Profile", qualityMode);
                CheckBox cbHw = addCheck("Enable DirectX / GPU Hardware Acceleration", db.Settings.display.hardwareAcceleration);
                cbHw.CheckedChanged += (s, e) => db.Settings.display.hardwareAcceleration = cbHw.Checked;
            }
            else if (category == "Audio")
            {
                addHeading("Audio Transmission");
                CheckBox cbTx = addCheck("Transmit remote system audio", db.Settings.audio.transmitAudio);
                cbTx.CheckedChanged += (s, e) => db.Settings.audio.transmitAudio = cbTx.Checked;
            }
            else if (category == "Account")
            {
                addHeading("Licensing & Operator Roster");
                addText("Licensed Operator", db.Settings.account.username);
                addText("License Tier", db.Settings.account.licenseType);
                addText("Enterprise Cluster Tenant", db.Settings.account.enterpriseTenant);
            }
        }

        private void SaveSettingsToDb()
        {
            db.Save();
            db.AddLog("SETTINGS_UPDATE", "APPLY", "Operator JEET applied settings studio changes");
            ShowToast("Settings successfully saved and applied to database.json");
        }

        // ======================================================================
        // SYSTEM TRAY, TOAST NOTIFICATION & DIALOGS
        // ======================================================================

        private void SetupTraySupervisor()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Open jDroid-X Console", (s, e) => { this.WindowState = FormWindowState.Normal; this.Show(); this.BringToFront(); });
            trayMenu.MenuItems.Add("Copy System Address", (s, e) => { Clipboard.SetText(currentHostDesktop.RemoteId); });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Launch Windows Quick Assist", (s, e) => { try { Process.Start("ms-quick-assist:"); } catch { } });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit Application", (s, e) => {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon = new NotifyIcon();
            trayIcon.Text = "jDroid-X-Desk Native Console";
            trayIcon.Icon = SystemIcons.Shield;
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => { this.WindowState = FormWindowState.Normal; this.Show(); this.BringToFront(); };
        }

        private void ShowToast(string message, Color? bg = null)
        {
            if (notificationToast == null)
            {
                notificationToast = new Panel();
                notificationToast.Size = new Size(440, 48);
                notificationToast.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                notificationToast.BackColor = bg ?? Color.FromArgb(15, 23, 42);
                notificationToast.BorderStyle = BorderStyle.FixedSingle;

                lblToastMessage = new Label();
                lblToastMessage.Dock = DockStyle.Fill;
                lblToastMessage.ForeColor = Color.White;
                lblToastMessage.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                lblToastMessage.TextAlign = ContentAlignment.MiddleCenter;
                notificationToast.Controls.Add(lblToastMessage);

                toastTimer = new System.Windows.Forms.Timer();
                toastTimer.Interval = 3200;
                toastTimer.Tick += (s, e) => {
                    toastTimer.Stop();
                    notificationToast.Visible = false;
                };

                this.Controls.Add(notificationToast);
                notificationToast.BringToFront();
            }

            notificationToast.BackColor = bg ?? Color.FromArgb(15, 23, 42);
            lblToastMessage.Text = message;
            notificationToast.Location = new Point(this.ClientSize.Width - notificationToast.Width - 24, this.ClientSize.Height - notificationToast.Height - 36);
            notificationToast.Visible = true;
            notificationToast.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        private string PromptInputDialog(string title, string prompt, string defaultValue)
        {
            Form form = new Form();
            form.Text = title;
            form.Size = new Size(420, 170);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;

            Label lbl = new Label { Text = prompt, Location = new Point(16, 16), Size = new Size(370, 20) };
            TextBox txt = new TextBox { Text = defaultValue, Location = new Point(16, 42), Size = new Size(370, 24) };
            Button btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(220, 85), Size = new Size(80, 28) };
            Button btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(306, 85), Size = new Size(80, 28) };

            form.Controls.Add(lbl);
            form.Controls.Add(txt);
            form.Controls.Add(btnOk);
            form.Controls.Add(btnCancel);
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() == DialogResult.OK)
            {
                return txt.Text.Trim();
            }
            return null;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(1500, "jDroid-X-Desk", "Console minimized to System Tray. Running in background.", ToolTipIcon.Info);
            }
            else
            {
                base.OnFormClosing(e);
            }
        }
    }
}
