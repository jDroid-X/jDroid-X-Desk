// ==============================================================================
// jDroid-X-Desk Native Windows Desktop Application
// Architecture: 100% Standalone WinForms C# Machine Application (Zero HTML/Browser)
// Antigravity IDE Layout: Activity Bar, Collapsible Drawer, Bottom Dock, Right Inspector
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
using System.Net.Sockets;
using System.Drawing.Imaging;

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
            theme = "dark";
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

    public class NotificationItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string type { get; set; }
        public string timestamp { get; set; }
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
        public List<NotificationItem> Notifications { get; set; }
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
                    Notifications = DeserializeList<NotificationItem>(dict, "notifications");
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
            Notifications = new List<NotificationItem> {
                new NotificationItem { id = "notif-1", title = "P2P Gateway Ready", message = "Local node listening on 7070 (AES-GCM-256)", type = "info", timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
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
                root["notifications"] = Notifications ?? new List<NotificationItem>();
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
            this.BorderColor = Color.FromArgb(30, 41, 59);
            this.CardBackground = Color.FromArgb(15, 23, 42);
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
    // 3. MAIN FORM & ANTIGRAVITY WORKSPACE LAYOUT (OOPS CONTROLLER LAYER)
    // ==========================================================================

    public class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private DatabaseContext db;
        private List<SimulatorDesktop> dynamicDesktops;
        private SimulatorDesktop currentHostDesktop;
        private SimulatorDesktop connectedTargetDesktop;

        // Antigravity Workspace Structural Containers (Zero Overlap Hierarchy)
        private Panel topHeaderBar;             // Height: 44, Dock: Top
        private Panel activityBar;              // Width: 48, Dock: Left (Slim icon ribbon)
        private Panel collapsibleLeftDrawer;    // Width: 260, Dock: Left (Primary Side Bar)
        private Panel collapsibleRightInspector;// Width: 260, Dock: Right (Secondary Side Bar)
        private Panel collapsibleBottomDock;    // Height: 200, Dock: Bottom (Terminal / Output Panel)
        private Panel centerEditorWorkArea;     // Dock: Fill (Tabs + Living Viewport)

        // Navigation Tabs & View Panels
        private Panel mainTabBar;
        private Panel viewContainer;
        private Panel viewConsole;
        private Panel viewFileTransfer;
        private Panel viewFleet;
        private Panel viewHistory;
        private Panel viewSettingsStudio;

        // Bottom Dock Controls
        private TabControl bottomDockTabs;
        private RichTextBox terminalBox;
        private TextBox terminalInput;
        private ListBox lbTrafficLogs;
        private Label lblDiagCpu;
        private Label lblDiagMem;
        private Label lblDiagUptime;
        private List<string> terminalHistory = new List<string>();
        private int historyIndex = -1;

        // Right Inspector Controls
        private Label lblInspectorFps;
        private Label lblInspectorRtt;
        private Label lblInspectorBytes;
        private Label lblInspectorCipher;
        private Label lblInspectorHost;
        private Label lblInspectorTarget;

        // Left Drawer Content Controls
        private Label lblDrawerTitle;
        private Panel drawerContentPanel;

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

        // Session State
        private bool isSessionActive = false;
        private bool isLiveScreenMirror = false;
        private string smartFitMode = "SmartFit"; // "SmartFit", "Original 1:1", "Stretch"
        private DateTime sessionStartTime;
        private System.Windows.Forms.Timer sessionTelemetryTimer;
        private long bytesTransferred = 1048576;
        private bool isStartMenuOpen = false;

        // Layout Toggle Buttons (Antigravity Image 1 & 2 controls)
        private Button btnToggleActivityBar;
        private Button btnToggleLeftDrawer;
        private Button btnToggleBottomDock;
        private Button btnToggleRightInspector;
        private TextBox txtGlobalSearch;

        // Active State
        private string activeViewKey = "Console";
        private Button activeActivityButton = null;

        // File Transfer Explorer
        private ListBox lbLocalFiles;
        private ListBox lbRemoteFiles;
        private Label lblLocalPath;
        private string currentLocalDirectory;

        // Fleet Grid
        private DataGridView dgvFleet;
        private ComboBox cboFleetGroupFilter;
        private TextBox txtFleetSearch;
        private DataGridView dgvHistory;

        // Settings Studio
        private TreeView settingsTree;
        private Panel settingsSubContentPanel;

        // System Tray & Toast
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private Panel notificationToast;
        private Label lblToastMessage;
        private System.Windows.Forms.Timer toastTimer;

        [STAThread]
        public static void Main()
        {
            try
            {
                try { SetProcessDPIAware(); } catch { }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "native_error.log"), ex.ToString());
            }
        }

        public MainForm()
        {
            db = new DatabaseContext();
            SyncDynamicDesktopsFromDatabase();

            // Intelligent station auto-detection based on host machine environment
            string myHost = Environment.MachineName.ToUpper();
            SimulatorDesktop autoDetected = dynamicDesktops.Find(d => myHost.Contains(d.Name.ToUpper()) || d.Name.ToUpper().Contains(myHost));
            if (autoDetected != null)
            {
                currentHostDesktop = autoDetected;
                connectedTargetDesktop = dynamicDesktops.Find(d => d.Key != currentHostDesktop.Key) ?? currentHostDesktop;
            }
            else
            {
                // Portable Client running on external Desk / remote PC
                string uniqueId = GenerateMachineDeskId();
                string uniquePass = GenerateMachinePasscode();
                SimulatorDesktop localNode = new SimulatorDesktop {
                    Key = "LOCAL",
                    Name = Environment.MachineName,
                    RemoteId = uniqueId,
                    Passcode = uniquePass,
                    Ip = GetLocalLanIp(),
                    Role = "Remote Workstation (Portable Client)",
                    OsVersion = Environment.OSVersion.VersionString,
                    AccentColor = Color.FromArgb(56, 189, 248),
                    ActiveApps = new[] { "Visual Studio Code", "Terminal", "Chrome" }
                };
                dynamicDesktops.Insert(0, localNode);
                currentHostDesktop = localNode;
                connectedTargetDesktop = dynamicDesktops.Count > 1 ? dynamicDesktops[1] : currentHostDesktop;
            }

            currentLocalDirectory = AppDomain.CurrentDomain.BaseDirectory;

            this.Text = "jDroid-X-Desk | Antigravity IDE Remote Desktop Workspace";
            this.Size = new Size(1400, 900);
            this.MinimumSize = new Size(1100, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42); // Antigravity Dark Navy
            this.Icon = SystemIcons.Shield;

            sessionTelemetryTimer = new System.Windows.Forms.Timer();
            sessionTelemetryTimer.Interval = 1000;
            sessionTelemetryTimer.Tick += (s, e) => {
                if (isSessionActive)
                {
                    bytesTransferred += (long)(1800000 + (new Random().Next(100000, 400000)));
                    UpdateLiveTelemetry();
                    viewportCanvas.Invalidate();
                }
            };

            BuildAntigravityLayout();
            SetupTraySupervisor();

            // Default State
            SwitchWorkspaceView("Console");
        }

        private void SyncDynamicDesktopsFromDatabase()
        {
            dynamicDesktops = new List<SimulatorDesktop>();
            Color[] colors = new Color[] {
                Color.FromArgb(16, 185, 129), // Emerald
                Color.FromArgb(2, 132, 199),  // Ocean Blue
                Color.FromArgb(245, 158, 11), // Amber
                Color.FromArgb(139, 92, 246)  // Purple
            };

            int i = 0;
            foreach (DeviceItem d in db.Devices)
            {
                Color c = colors[i % colors.Length];
                string id = "984-102-39" + (i + 1);
                string pass = "65" + (i + 1) + "-332";

                ContactItem matchedContact = db.Contacts.Find(cnt => cnt.name == d.name);
                if (matchedContact != null && !string.IsNullOrEmpty(matchedContact.remoteId))
                {
                    id = matchedContact.remoteId;
                }

                dynamicDesktops.Add(new SimulatorDesktop
                {
                    Key = d.name.Replace("-", "").ToUpper(),
                    Name = d.name,
                    RemoteId = id,
                    Passcode = pass,
                    Ip = d.ip,
                    Role = d.group + " (" + d.accessLevel + ")",
                    OsVersion = d.name.Contains("SERVER") || d.name.Contains("NODE") ? "Windows Server 2022 Datacenter" : "Windows 11 Pro Enterprise x64",
                    AccentColor = c,
                    ActiveApps = d.name.Contains("ANTIGRAVITY") ? 
                        new[] { "Google DeepMind Cognitive Engine", "Relay Bridge Server :7070", "CUDA Diagnostics" } :
                        d.name.Contains("INNOVANT") ? 
                        new[] { "Blender 4.0 Cycles GPU Render Engine", "Network Packet Capture", "Host Telemetry" } :
                        new[] { "Visual Studio 2022 - jDroid-X Native", "Antigravity Multi-Agent IDE", "PowerShell Admin" }
                });
                i++;
            }

            if (dynamicDesktops.Count == 0)
            {
                dynamicDesktops.Add(new SimulatorDesktop {
                    Key = "PRIMARY", Name = "HOST-DESK", RemoteId = "984-102-391", Passcode = "659-332", Ip = "192.168.1.44", Role = "Primary Machine", OsVersion = "Windows 11 x64", AccentColor = Color.FromArgb(16, 185, 129), ActiveApps = new[] { "Visual Studio", "Terminal" }
                });
            }
        }

        private string GenerateMachineDeskId()
        {
            try
            {
                string host = Environment.MachineName.ToUpper();
                if (host.Contains("JEET") || host.Contains("DESK1")) return "984-102-391";
                if (host.Contains("ANTIGRAVITY") || host.Contains("NODE") || host.Contains("DESK2")) return "710-224-900";
                if (host.Contains("INNOVANT") || host.Contains("STATION") || host.Contains("DESK3")) return "520-310-884";

                string mac = "";
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        mac = nic.GetPhysicalAddress().ToString();
                        if (!string.IsNullOrEmpty(mac)) break;
                    }
                }
                int hash = Math.Abs((host + mac).GetHashCode());
                string s = hash.ToString().PadLeft(9, '3');
                if (s.Length > 9) s = s.Substring(s.Length - 9);
                return s.Substring(0, 3) + "-" + s.Substring(3, 3) + "-" + s.Substring(6, 3);
            }
            catch
            {
                return "710-224-900";
            }
        }

        private string GenerateMachinePasscode()
        {
            try
            {
                int p = Math.Abs((Environment.MachineName + "PASS").GetHashCode()) % 900000 + 100000;
                string s = p.ToString();
                return s.Substring(0, 3) + "-" + s.Substring(3, 3);
            }
            catch
            {
                return "842-195";
            }
        }

        private string GetLocalLanIp()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null) return endPoint.Address.ToString();
                }
            }
            catch { }
            return "192.168.1." + (new Random().Next(10, 200));
        }

        // ======================================================================
        // ANTIGRAVITY IDE WORKSPACE ARCHITECTURE (ZERO OVERLAPPING HIERARCHY)
        // ======================================================================

        private void BuildAntigravityLayout()
        {
            this.Controls.Clear();

            // 1. TOP HEADER / TITLE BAR (DOCK: TOP, HEIGHT: 44)
            topHeaderBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(9, 13, 22), Padding = new Padding(12, 0, 12, 0) };
            BuildTopHeaderContent();

            // 2. SLIM ACTIVITY BAR (DOCK: LEFT, WIDTH: 48)
            activityBar = new Panel { Dock = DockStyle.Left, Width = 48, BackColor = Color.FromArgb(7, 10, 16), Padding = new Padding(0) };
            BuildActivityBarContent();

            // 3. COLLAPSIBLE PRIMARY SIDEBAR / DRAWER (DOCK: LEFT, WIDTH: 260)
            collapsibleLeftDrawer = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = Color.FromArgb(13, 21, 34), Padding = new Padding(0) };
            BuildLeftDrawerContent();

            // 4. COLLAPSIBLE RIGHT INSPECTOR (DOCK: RIGHT, WIDTH: 260)
            collapsibleRightInspector = new Panel { Dock = DockStyle.Right, Width = 260, BackColor = Color.FromArgb(13, 21, 34), Padding = new Padding(12), Visible = false };
            BuildRightInspectorContent();

            // 5. COLLAPSIBLE BOTTOM PANEL (DOCK: BOTTOM, HEIGHT: 200)
            collapsibleBottomDock = new Panel { Dock = DockStyle.Bottom, Height = 200, BackColor = Color.FromArgb(9, 13, 22), Padding = new Padding(0), Visible = false };
            BuildBottomDockContent();

            // 6. MAIN CENTER EDITOR WORK AREA (DOCK: FILL - ZERO OVERLAP)
            centerEditorWorkArea = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42) };
            BuildCenterEditorContent();

            // -------------------------------------------------------------
            // CRITICAL WINFORMS NON-OVERLAPPING DOCK ORDER:
            // In WinForms: Control at highest index docks FIRST (takes outer edge).
            // Control at index 0 docks LAST (fills remaining interior space).
            // 5: topHeaderBar (docked 1st: takes full top 0,0,W,44)
            // 4: activityBar (docked 2nd: takes far left 0,44,48,H-44)
            // 3: collapsibleLeftDrawer (docked 3rd: takes left 48,44,260,H-44)
            // 2: collapsibleRightInspector (docked 4th: takes right W-260,44,260,H-44)
            // 1: collapsibleBottomDock (docked 5th: takes bottom 308,H-200,W-568,200)
            // 0: centerEditorWorkArea (docked LAST: fills exact remaining center rectangle!)
            // -------------------------------------------------------------
            this.Controls.Add(topHeaderBar);
            this.Controls.Add(activityBar);
            this.Controls.Add(collapsibleLeftDrawer);
            this.Controls.Add(collapsibleRightInspector);
            this.Controls.Add(collapsibleBottomDock);
            this.Controls.Add(centerEditorWorkArea);

            this.Controls.SetChildIndex(topHeaderBar, 5);
            this.Controls.SetChildIndex(activityBar, 4);
            this.Controls.SetChildIndex(collapsibleLeftDrawer, 3);
            this.Controls.SetChildIndex(collapsibleRightInspector, 2);
            this.Controls.SetChildIndex(collapsibleBottomDock, 1);
            this.Controls.SetChildIndex(centerEditorWorkArea, 0);
        }

        private void BuildTopHeaderContent()
        {
            // Left: Logo + Breadcrumbs
            FlowLayoutPanel leftBrand = new FlowLayoutPanel { Dock = DockStyle.Left, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
            Label lblLogo = new Label { Text = "jDroid-X", Font = new Font("Segoe UI", 11.5f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), AutoSize = true, Margin = new Padding(0, 2, 4, 0) };
            Label lblSub = new Label { Text = "Antigravity Workspace", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), AutoSize = true, Margin = new Padding(0, 5, 12, 0) };
            
            // Host Node Dropdown Selector (Top Bar Pill)
            ComboBox cboHostPill = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.FromArgb(16, 185, 129), Size = new Size(185, 24), Margin = new Padding(0, 2, 0, 0) };
            foreach (SimulatorDesktop d in dynamicDesktops) cboHostPill.Items.Add("💻 " + d.Name);
            int curIdx = dynamicDesktops.IndexOf(currentHostDesktop);
            cboHostPill.SelectedIndex = curIdx >= 0 ? curIdx : 0;
            cboHostPill.SelectedIndexChanged += (s, e) => {
                if (cboHostPill.SelectedIndex >= 0 && cboHostPill.SelectedIndex < dynamicDesktops.Count)
                {
                    SwitchHostDesktop(dynamicDesktops[cboHostPill.SelectedIndex]);
                }
            };

            leftBrand.Controls.Add(lblLogo);
            leftBrand.Controls.Add(lblSub);
            leftBrand.Controls.Add(cboHostPill);

            // Center: Target Search & Quick Connect Input
            Panel centerSearchBox = new Panel { Size = new Size(340, 28), Location = new Point((topHeaderBar.Width - 340) / 2, 8), Anchor = AnchorStyles.Top };
            topHeaderBar.Resize += (s, e) => { centerSearchBox.Location = new Point((topHeaderBar.Width - centerSearchBox.Width) / 2, 8); };
            txtGlobalSearch = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9f), BackColor = Color.FromArgb(24, 32, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "710-224-900" };
            Button btnSearchGo = new Button { Text = "⚡ Connect", Dock = DockStyle.Right, Width = 80, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(239, 51, 64), ForeColor = Color.White, Font = new Font("Segoe UI", 8f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSearchGo.FlatAppearance.BorderSize = 0;
            btnSearchGo.Click += (s, e) => {
                txtTargetId.Text = txtGlobalSearch.Text.Trim();
                SwitchWorkspaceView("Console");
                ToggleSessionConnection();
            };
            centerSearchBox.Controls.Add(txtGlobalSearch);
            centerSearchBox.Controls.Add(btnSearchGo);

            // Right: THE EXACT ANTIGRAVITY LAYOUT BUTTONS (Image 1 & 2 Specification)
            FlowLayoutPanel rightLayoutControls = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 6, 0, 0) };

            btnToggleActivityBar = CreateLayoutIconBtn("◫", "Toggle Activity Bar (Far Left)", (s, e) => {
                activityBar.Visible = !activityBar.Visible;
                HighlightLayoutBtn(btnToggleActivityBar, activityBar.Visible);
            }, activityBar != null && activityBar.Visible);

            btnToggleLeftDrawer = CreateLayoutIconBtn("◨", "Toggle Primary Left Sidebar", (s, e) => {
                collapsibleLeftDrawer.Visible = !collapsibleLeftDrawer.Visible;
                HighlightLayoutBtn(btnToggleLeftDrawer, collapsibleLeftDrawer.Visible);
            }, collapsibleLeftDrawer != null && collapsibleLeftDrawer.Visible);

            btnToggleBottomDock = CreateLayoutIconBtn("⬒", "Toggle Bottom Panel (Terminal / Diagnostics)", (s, e) => {
                collapsibleBottomDock.Visible = !collapsibleBottomDock.Visible;
                HighlightLayoutBtn(btnToggleBottomDock, collapsibleBottomDock.Visible);
            }, collapsibleBottomDock != null && collapsibleBottomDock.Visible);

            btnToggleRightInspector = CreateLayoutIconBtn("◧", "Toggle Secondary Right Inspector (Session Stats)", (s, e) => {
                collapsibleRightInspector.Visible = !collapsibleRightInspector.Visible;
                HighlightLayoutBtn(btnToggleRightInspector, collapsibleRightInspector.Visible);
            }, collapsibleRightInspector != null && collapsibleRightInspector.Visible);

            Button btnSearchIcon = CreateLayoutIconBtn("🔍", "Search / Quick Open", (s, e) => {
                txtGlobalSearch.Focus();
                txtGlobalSearch.SelectAll();
            }, false);

            rightLayoutControls.Controls.Add(btnToggleActivityBar);
            rightLayoutControls.Controls.Add(btnToggleLeftDrawer);
            rightLayoutControls.Controls.Add(btnToggleBottomDock);
            rightLayoutControls.Controls.Add(btnToggleRightInspector);
            rightLayoutControls.Controls.Add(btnSearchIcon);

            topHeaderBar.Controls.Add(leftBrand);
            topHeaderBar.Controls.Add(centerSearchBox);
            topHeaderBar.Controls.Add(rightLayoutControls);
        }

        private Button CreateLayoutIconBtn(string icon, string tooltip, EventHandler onClick, bool isActive)
        {
            Button btn = new Button
            {
                Text = icon,
                Size = new Size(32, 28),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Symbol", 10.5f, FontStyle.Regular),
                ForeColor = isActive ? Color.White : Color.FromArgb(148, 163, 184),
                BackColor = isActive ? Color.FromArgb(40, 53, 76) : Color.Transparent,
                Cursor = Cursors.Hand,
                Margin = new Padding(2, 0, 2, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        private void HighlightLayoutBtn(Button btn, bool isActive)
        {
            btn.ForeColor = isActive ? Color.White : Color.FromArgb(148, 163, 184);
            btn.BackColor = isActive ? Color.FromArgb(40, 53, 76) : Color.Transparent;
        }

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

            Panel drawerHeader = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.FromArgb(10, 16, 26), Padding = new Padding(12, 8, 8, 0) };
            lblDrawerTitle = new Label { Text = "REMOTE FLEET ROSTER", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Dock = DockStyle.Left, AutoSize = true };
            
            Button btnCloseDrawer = new Button { Text = "✕", Size = new Size(24, 24), Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(148, 163, 184), Cursor = Cursors.Hand };
            btnCloseDrawer.FlatAppearance.BorderSize = 0;
            btnCloseDrawer.Click += (s, e) => {
                collapsibleLeftDrawer.Visible = false;
                HighlightLayoutBtn(btnToggleLeftDrawer, false);
            };

            drawerHeader.Controls.Add(lblDrawerTitle);
            drawerHeader.Controls.Add(btnCloseDrawer);

            drawerContentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(13, 21, 34), Padding = new Padding(8) };

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

            if (key == "Console")
            {
                lblDrawerTitle.Text = "QUICK-CALL FLEET NODES";
                Label lPrompt = new Label { Text = "Click to establish 1-click P2P stream to partner desktop:", Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, Height = 34 };
                drawerContentPanel.Controls.Add(lPrompt);

                FlowLayoutPanel listFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
                foreach (SimulatorDesktop target in dynamicDesktops)
                {
                    Panel card = new Panel { Size = new Size(236, 68), BackColor = Color.FromArgb(20, 28, 45), Margin = new Padding(0, 0, 0, 8), Padding = new Padding(8) };
                    Label lName = new Label { Text = (target.Name.Contains("JEET") ? "💻 " : target.Name.Contains("ANTIGRAVITY") ? "🤖 " : "🚀 ") + target.Name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(6, 6), AutoSize = true };
                    Label lSub = new Label { Text = target.RemoteId + " • " + target.Ip, Font = new Font("Consolas", 8f), ForeColor = target.AccentColor, Location = new Point(8, 26), AutoSize = true };
                    
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
            else if (key == "FileTransfer")
            {
                lblDrawerTitle.Text = "STORAGE & DRIVES";
                ListBox lbDrives = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 28, 45), ForeColor = Color.White, Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.None };
                try {
                    foreach (DriveInfo d in DriveInfo.GetDrives())
                    {
                        if (d.IsReady) lbDrives.Items.Add(string.Format("💾 Drive {0} ({1} GB Free)", d.Name, d.AvailableFreeSpace / (1024 * 1024 * 1024)));
                    }
                } catch { lbDrives.Items.Add("💾 Drive C:\\ (System)"); }
                drawerContentPanel.Controls.Add(lbDrives);
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
            if (lblInspectorBytes != null) lblInspectorBytes.Text = (bytesTransferred / (1024 * 1024.0)).ToString("0.0") + " MB";
            if (lblInspectorFps != null) lblInspectorFps.Text = isSessionActive ? "60.0 FPS (Active)" : "0.0 FPS (Idle)";
            if (lblInspectorRtt != null) lblInspectorRtt.Text = isSessionActive ? (10 + new Random().Next(0, 3)) + ".4 ms (P2P)" : "-- ms";
            if (lblInspectorHost != null && currentHostDesktop != null) lblInspectorHost.Text = currentHostDesktop.Name;
            if (lblInspectorTarget != null && connectedTargetDesktop != null) lblInspectorTarget.Text = connectedTargetDesktop.Name;
        }

        // ======================================================================
        // COLLAPSIBLE BOTTOM DOCK (TERMINAL / OUTPUT / DIAGNOSTICS)
        // ======================================================================

        private void BuildBottomDockContent()
        {
            collapsibleBottomDock.Controls.Clear();

            bottomDockTabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f) };

            // Tab 1: PowerShell Terminal
            TabPage tabTerm = new TabPage("💻 PowerShell Terminal");
            tabTerm.BackColor = Color.FromArgb(9, 13, 22);

            terminalBox = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(12, 16, 28), ForeColor = Color.FromArgb(16, 185, 129), Font = new Font("Consolas", 9.5f), ReadOnly = true, Text = "Windows PowerShell [jDroid-X Elevated Diagnostic Console]\nPS C:\\Users\\jiten> " };
            Panel termBottom = new Panel { Dock = DockStyle.Bottom, Height = 32, BackColor = Color.FromArgb(20, 27, 45) };
            terminalInput = new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 27, 45), ForeColor = Color.White, Font = new Font("Consolas", 10f, FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle };
            terminalInput.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) ExecuteTerminalCommand();
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
                terminalBox.Text = "Windows PowerShell [jDroid-X Elevated Diagnostic Console]\nPS C:\\Users\\jiten> ";
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
        // CENTER EDITOR WORK AREA (ANTIGRAVITY TABS & MAIN VIEWPORT)
        // ======================================================================

        private void BuildCenterEditorContent()
        {
            centerEditorWorkArea.Controls.Clear();

            // Main Antigravity Document Tab Bar across top of workspace
            mainTabBar = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(9, 13, 22), Padding = new Padding(4, 4, 4, 0) };
            BuildMainTabBar();

            // View Container
            viewContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42) };

            // Initialize Subviews
            InitializeConsoleView();
            InitializeFileTransferView();
            InitializeFleetView();
            InitializeHistoryView();
            InitializeSettingsStudioView();

            centerEditorWorkArea.Controls.Add(mainTabBar);
            centerEditorWorkArea.Controls.Add(viewContainer);
            centerEditorWorkArea.Controls.SetChildIndex(mainTabBar, 1);
            centerEditorWorkArea.Controls.SetChildIndex(viewContainer, 0);
        }

        private void BuildMainTabBar()
        {
            mainTabBar.Controls.Clear();

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
                    ForeColor = isActive ? Color.White : Color.FromArgb(148, 163, 184),
                    BackColor = isActive ? Color.FromArgb(15, 23, 42) : Color.FromArgb(9, 13, 22),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 2, 0)
                };
                tabBtn.FlatAppearance.BorderSize = 0;
                tabBtn.Click += (s, e) => SwitchWorkspaceView(key);
                tabFlow.Controls.Add(tabBtn);
            };

            addTab("Console", "🖥️ Remote Desktop");
            addTab("FileTransfer", "📁 File Transfer");
            addTab("Fleet", "🗄️ Fleet Workstations");
            addTab("History", "📜 Session Logs");
            addTab("Settings", "⚙️ Settings Studio");

            mainTabBar.Controls.Add(tabFlow);
        }

        private void SwitchWorkspaceView(string key)
        {
            activeViewKey = key;

            BuildMainTabBar();
            UpdateDrawerForCurrentView(key);

            viewContainer.Controls.Clear();
            Panel target = null;
            if (key == "Console") target = viewConsole;
            else if (key == "FileTransfer") target = viewFileTransfer;
            else if (key == "Fleet") target = viewFleet;
            else if (key == "History") target = viewHistory;
            else if (key == "Settings") target = viewSettingsStudio;

            if (target != null)
            {
                target.Dock = DockStyle.Fill;
                viewContainer.Controls.Add(target);
            }
        }

        // ======================================================================
        // SUBVIEW 1: REMOTE DESKTOP CONSOLE
        // ======================================================================

        private void InitializeConsoleView()
        {
            viewConsole = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(8) };

            // Top Floating Session Bar
            sessionFloatingToolbar = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(20, 28, 45), Padding = new Padding(6, 4, 6, 4), Visible = false };
            BuildFloatingSessionToolbar();

            // Local Node Info Ribbon (Slim, Smart Fit)
            Panel localBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(13, 21, 34), Padding = new Padding(8, 6, 8, 0) };
            lblLocalId = new Label { Text = "THIS DESK: " + (currentHostDesktop != null ? currentHostDesktop.RemoteId : "984-102-391"), Font = new Font("Consolas", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            
            lblLocalPass = new Label { Text = "Pass: " + (currentHostDesktop != null ? currentHostDesktop.Passcode : "659-332"), Font = new Font("Consolas", 9.5f), ForeColor = Color.FromArgb(16, 185, 129), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(12, 0, 0, 0) };

            Button btnCopy = new Button { Text = "📋 Copy", Size = new Size(64, 24), Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(40, 53, 76), Cursor = Cursors.Hand };
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Click += (s, e) => {
                if (currentHostDesktop != null) {
                    Clipboard.SetText(currentHostDesktop.RemoteId);
                    ShowToast("Copied address: " + currentHostDesktop.RemoteId);
                }
            };

            // Target Connect on right
            FlowLayoutPanel targetBox = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
            Label lTargetPrompt = new Label { Text = "Target:", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Margin = new Padding(0, 4, 4, 0), AutoSize = true };
            txtTargetId = new TextBox { Font = new Font("Consolas", 9.5f), Size = new Size(110, 22), Text = connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : "710-224-900" };
            txtTargetPass = new TextBox { Font = new Font("Consolas", 9.5f), Size = new Size(70, 22), UseSystemPasswordChar = true, Text = connectedTargetDesktop != null ? connectedTargetDesktop.Passcode : "884219" };
            btnConnect = new ModernButton { Text = "⚡ Connect", NormalColor = Color.FromArgb(239, 51, 64), HoverColor = Color.FromArgb(220, 38, 38), Size = new Size(86, 24) };
            btnConnect.Click += (s, e) => ToggleSessionConnection();

            targetBox.Controls.Add(lTargetPrompt);
            targetBox.Controls.Add(txtTargetId);
            targetBox.Controls.Add(txtTargetPass);
            targetBox.Controls.Add(btnConnect);

            localBar.Controls.Add(targetBox);
            localBar.Controls.Add(btnCopy);
            localBar.Controls.Add(lblLocalPass);
            localBar.Controls.Add(lblLocalId);

            // GDI+ Canvas for Remote Stream
            viewportCanvas = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 16, 26) };
            viewportCanvas.Paint += ViewportCanvas_Paint;
            viewportCanvas.MouseClick += ViewportCanvas_MouseClick;

            viewConsole.Controls.Add(sessionFloatingToolbar);
            viewConsole.Controls.Add(localBar);
            viewConsole.Controls.Add(viewportCanvas);
            viewConsole.Controls.SetChildIndex(sessionFloatingToolbar, 2);
            viewConsole.Controls.SetChildIndex(localBar, 1);
            viewConsole.Controls.SetChildIndex(viewportCanvas, 0);
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

            Button btnMirror = CreateToolbarBtn("🎥 " + (isLiveScreenMirror ? "Simulated OS" : "Live Mirror"), (s, e) => {
                isLiveScreenMirror = !isLiveScreenMirror;
                ((Button)s).Text = "🎥 " + (isLiveScreenMirror ? "Simulated OS" : "Live Mirror");
                viewportCanvas.Invalidate();
                ShowToast("Engine: " + (isLiveScreenMirror ? "Live Local Screen Mirror" : "High-Fidelity Simulated OS"));
            });

            Button btnCad = CreateToolbarBtn("⌨️ SAS", (s, e) => ShowToast("Injected Ctrl+Alt+Del."));
            Button btnTaskMgr = CreateToolbarBtn("⚙️ TaskMgr", (s, e) => { try { Process.Start("taskmgr.exe"); } catch { } });

            leftTools.Controls.Add(btnDisplay);
            leftTools.Controls.Add(btnSmartFit);
            leftTools.Controls.Add(btnMirror);
            leftTools.Controls.Add(btnCad);
            leftTools.Controls.Add(btnTaskMgr);

            FlowLayoutPanel rightHUD = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
            lblSessionTelemetryHUD = new Label { Text = "🟢 60 FPS • 11ms • P2P", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), AutoSize = true, Margin = new Padding(0, 6, 8, 0) };

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

        private void SwitchHostDesktop(SimulatorDesktop newHost)
        {
            if (isSessionActive)
            {
                MessageBox.Show("Please disconnect active session before switching local node identity.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            currentHostDesktop = newHost;
            if (lblLocalId != null) lblLocalId.Text = "THIS DESK: " + currentHostDesktop.RemoteId;
            if (lblLocalPass != null) lblLocalPass.Text = "Pass: " + currentHostDesktop.Passcode;
            UpdateDrawerForCurrentView("Console");
            ShowToast("Local Host switched to " + newHost.Name);
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
                    SimulatorDesktop matched = dynamicDesktops.Find(d => d.RemoteId == targetId || d.Name.ToUpper() == targetId.ToUpper());
                    if (matched != null) connectedTargetDesktop = matched;
                    else
                    {
                        connectedTargetDesktop = new SimulatorDesktop {
                            Key = "CUSTOM",
                            Name = "Workstation (" + targetId + ")",
                            RemoteId = targetId,
                            Passcode = txtTargetPass.Text.Trim(),
                            Ip = "192.168.1." + new Random().Next(120, 250),
                            Role = "Remote Workstation",
                            OsVersion = "Windows 11 Pro Enterprise x64",
                            AccentColor = Color.FromArgb(2, 132, 199),
                            ActiveApps = new[] { "Visual Studio Code", "File Explorer", "Command Prompt" }
                        };
                    }
                }

                isSessionActive = true;
                sessionStartTime = DateTime.Now;
                sessionTelemetryTimer.Start();
                if (sessionFloatingToolbar != null) sessionFloatingToolbar.Visible = true;

                btnConnect.Text = "Disconnect";
                btnConnect.NormalColor = Color.FromArgb(100, 116, 139);

                db.AddLog("SESSION_START", "P2P_CONNECT", "Connected to " + connectedTargetDesktop.Name);
                ShowToast("P2P Session Established with " + connectedTargetDesktop.Name);
                viewportCanvas.Invalidate();
            }
            else
            {
                isSessionActive = false;
                sessionTelemetryTimer.Stop();
                if (sessionFloatingToolbar != null) sessionFloatingToolbar.Visible = false;
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
                db.AddLog("SESSION_END", "DISCONNECT", "Session closed");
                db.Save();

                btnConnect.Text = "⚡ Connect";
                btnConnect.NormalColor = Color.FromArgb(239, 51, 64);

                ShowToast(string.Format("Session closed. Duration: {0}s", (int)duration.TotalSeconds));
                viewportCanvas.Invalidate();
            }
        }

        // ======================================================================
        // GDI+ REMOTE DESKTOP ENGINE
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
                // Standby Topology Screen
                using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), Color.FromArgb(10, 16, 26), Color.FromArgb(15, 23, 42), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(bg, 0, 0, cw, ch);
                }

                int cardW = Math.Min(600, cw - 40);
                int cardH = 260;
                int cardX = (cw - cardW) / 2;
                int cardY = Math.Max(20, (ch - cardH) / 2);

                using (SolidBrush cBg = new SolidBrush(Color.FromArgb(20, 28, 45)))
                using (Pen cBorder = new Pen(Color.FromArgb(40, 53, 76), 1))
                {
                    g.FillRectangle(cBg, cardX, cardY, cardW, cardH);
                    g.DrawRectangle(cBorder, cardX, cardY, cardW, cardH);
                }

                using (Font fTitle = new Font("Segoe UI", 14f, FontStyle.Bold))
                using (Font fSub = new Font("Segoe UI", 9.5f))
                using (Font fCode = new Font("Consolas", 9f))
                using (SolidBrush bWhite = new SolidBrush(Color.White))
                using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(148, 163, 184)))
                using (SolidBrush bBlue = new SolidBrush(Color.FromArgb(56, 189, 248)))
                using (SolidBrush bGreen = new SolidBrush(Color.FromArgb(16, 185, 129)))
                {
                    g.DrawString("🖥️ Remote Desktop Canvas Standby", fTitle, bWhite, cardX + 20, cardY + 20);
                    g.DrawString("Antigravity Workspace Engine • P2P UDP Ready", fSub, bMuted, cardX + 20, cardY + 50);

                    if (currentHostDesktop != null)
                    {
                        g.DrawString("Active Host: " + currentHostDesktop.Name + " [" + currentHostDesktop.RemoteId + "]", fCode, bBlue, cardX + 20, cardY + 85);
                        g.DrawString("IP Address : " + currentHostDesktop.Ip + ":7070 (AES-256)", fCode, bWhite, cardX + 20, cardY + 110);
                        g.DrawString("Role       : " + currentHostDesktop.Role, fCode, bMuted, cardX + 20, cardY + 135);
                    }
                    g.DrawString("Relay Node : relay.jdroid-x.net:21116 (ONLINE)", fCode, bGreen, cardX + 20, cardY + 160);

                    g.DrawString("Click any desktop node in the left drawer to connect.", fSub, bWhite, cardX + 20, cardY + 205);
                }
            }
            else
            {
                SimulatorDesktop target = connectedTargetDesktop ?? (dynamicDesktops.Count > 1 ? dynamicDesktops[1] : dynamicDesktops[0]);

                if (isLiveScreenMirror)
                {
                    try {
                        Bitmap screenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                        using (Graphics sG = Graphics.FromImage(screenCapture))
                        {
                            sG.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, screenCapture.Size);
                        }
                        DrawScaledImage(g, screenCapture, cw, ch);
                        screenCapture.Dispose();
                        return;
                    } catch { }
                }

                RenderSimulatedRemoteDesktop(g, target, cw, ch);
            }
        }

        private void RenderSimulatedRemoteDesktop(Graphics g, SimulatorDesktop target, int cw, int ch)
        {
            // Desktop Wallpaper
            using (LinearGradientBrush wallBrush = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), Color.FromArgb(17, 24, 39), Color.FromArgb(15, 23, 42), LinearGradientMode.Vertical))
            {
                g.FillRectangle(wallBrush, 0, 0, cw, ch);
            }

            // Icons
            string[] icons = new string[] { "💻 This PC", "🧠 Antigravity", "⌨️ Terminal", "🌐 Chrome", "🎨 Blender", "🗑️ Bin" };
            int iconY = 16;
            using (Font iconFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush iconBg = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
            using (SolidBrush iconTxt = new SolidBrush(Color.White))
            {
                foreach (string icon in icons)
                {
                    g.FillRectangle(iconBg, 16, iconY, 120, 26);
                    g.DrawString(icon, iconFont, iconTxt, 20, iconY + 5);
                    iconY += 34;
                }
            }

            // Primary Window - Smart Fit to Full Remaining Viewport
            int win1X = 146;
            int win1Y = 16;
            int taskH = 38;
            int win1W = Math.Max(340, cw - win1X - 20);
            int win1H = Math.Max(220, ch - taskH - 24);

            DrawRemoteWindow(g, win1X, win1Y, win1W, win1H, "🗔 " + target.ActiveApps[0] + " - [" + target.Name + "]", () => {
                using (Font bodyF = new Font("Segoe UI", 8.5f))
                using (Font codeF = new Font("Consolas", 8f))
                using (SolidBrush bWhite = new SolidBrush(Color.White))
                using (SolidBrush bGreen = new SolidBrush(Color.FromArgb(16, 185, 129)))
                using (SolidBrush bBlue = new SolidBrush(Color.FromArgb(56, 189, 248)))
                using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    g.DrawString("PID: 14208 | Thread: 48 | Priority: RealTime", codeF, bGreen, win1X + 14, win1Y + 34);
                    g.DrawString("OS: " + target.OsVersion, bodyF, bWhite, win1X + 14, win1Y + 52);
                    g.DrawString("Role: " + target.Role, bodyF, bBlue, win1X + 14, win1Y + 70);
                    g.DrawString("Stream: " + target.Ip + ":7070 (AES-GCM-256)", bodyF, bMuted, win1X + 14, win1Y + 88);

                    Rectangle actBox = new Rectangle(win1X + 14, win1Y + 110, win1W - 28, win1H - 124);
                    using (SolidBrush abBg = new SolidBrush(Color.FromArgb(10, 16, 26)))
                    using (Pen abBorder = new Pen(Color.FromArgb(40, 53, 76), 1))
                    {
                        g.FillRectangle(abBg, actBox);
                        g.DrawRectangle(abBorder, actBox);
                    }
                    g.DrawString(">> Live Remote Telemetry Stream:", codeF, bGreen, actBox.X + 8, actBox.Y + 8);
                    g.DrawString(string.Format("[{0}] Video Channel: AV1/H.264 60 FPS", DateTime.Now.ToString("HH:mm:ss")), codeF, bWhite, actBox.X + 8, actBox.Y + 26);
                    g.DrawString(string.Format("[{0}] Encryption: AES-256-GCM Handshake OK", DateTime.Now.ToString("HH:mm:ss")), codeF, bWhite, actBox.X + 8, actBox.Y + 44);
                    g.DrawString(string.Format("[{0}] Input Sync: Keyboard & Mouse (0 ms)", DateTime.Now.ToString("HH:mm:ss")), codeF, bMuted, actBox.X + 8, actBox.Y + 62);
                }
            });

            // Taskbar
            int taskY = ch - taskH;
            using (SolidBrush taskBg = new SolidBrush(Color.FromArgb(9, 13, 22)))
            {
                g.FillRectangle(taskBg, 0, taskY, cw, taskH);
            }

            using (SolidBrush startBg = new SolidBrush(isStartMenuOpen ? Color.FromArgb(2, 132, 199) : Color.FromArgb(30, 41, 59)))
            using (Font startF = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush whiteB = new SolidBrush(Color.White))
            {
                g.FillRectangle(startBg, 8, taskY + 4, 70, 30);
                g.DrawString("⊞ Start", startF, whiteB, 18, taskY + 10);
            }

            using (Font trayF = new Font("Segoe UI", 8f))
            using (SolidBrush trayTxt = new SolidBrush(Color.FromArgb(203, 213, 225)))
            {
                g.DrawString("📶 🔊 " + DateTime.Now.ToString("HH:mm:ss"), trayF, trayTxt, cw - 120, taskY + 10);
            }
        }

        private void DrawRemoteWindow(Graphics g, int x, int y, int w, int h, string title, Action renderContent)
        {
            using (SolidBrush winBg = new SolidBrush(Color.FromArgb(20, 28, 45)))
            using (SolidBrush titleBg = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (Pen borderPen = new Pen(Color.FromArgb(40, 53, 76), 1))
            using (Font titleFont = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (SolidBrush titleText = new SolidBrush(Color.White))
            {
                g.FillRectangle(winBg, x, y, w, h);
                g.FillRectangle(titleBg, x, y, w, 26);
                g.DrawRectangle(borderPen, x, y, w, h);
                g.DrawString(title, titleFont, titleText, x + 8, y + 5);

                using (SolidBrush bClose = new SolidBrush(Color.FromArgb(239, 68, 68)))
                using (SolidBrush bMin = new SolidBrush(Color.FromArgb(245, 158, 11)))
                using (SolidBrush bMax = new SolidBrush(Color.FromArgb(16, 185, 129)))
                {
                    g.FillEllipse(bClose, x + w - 18, y + 7, 10, 10);
                    g.FillEllipse(bMin, x + w - 32, y + 7, 10, 10);
                    g.FillEllipse(bMax, x + w - 46, y + 7, 10, 10);
                }

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
            if (e.Y >= ch - 38 && e.X >= 8 && e.X <= 78)
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

        // ======================================================================
        // SUBVIEW 2: DUAL-PANE FILE TRANSFER
        // ======================================================================

        private void InitializeFileTransferView()
        {
            viewFileTransfer = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(12) };

            TableLayoutPanel explorerGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
            explorerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));

            // Left: Local Explorer
            ModernCardPanel cardLocal = new ModernCardPanel { Dock = DockStyle.Fill };
            lblLocalPath = new Label { Text = "Local: " + currentLocalDirectory, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Dock = DockStyle.Top, Height = 28 };
            lbLocalFiles = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, Font = new Font("Consolas", 8.5f), BorderStyle = BorderStyle.None };
            PopulateLocalFiles();
            cardLocal.Controls.Add(lbLocalFiles);
            cardLocal.Controls.Add(lblLocalPath);

            // Middle Actions
            Panel mid = new Panel { Dock = DockStyle.Fill };
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
            Label lblRemotePath = new Label { Text = "Remote: C:\\RemoteWorkstation\\Projects", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Dock = DockStyle.Top, Height = 28 };
            lbRemoteFiles = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, Font = new Font("Consolas", 8.5f), BorderStyle = BorderStyle.None };
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
            try {
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
            viewFleet = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(12) };

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 42 };
            Label lblTitle = new Label { Text = "Registered Workstation Fleet", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
            txtFleetSearch = new TextBox { Font = new Font("Segoe UI", 8.5f), Size = new Size(140, 22), Margin = new Padding(0, 2, 8, 0) };
            txtFleetSearch.TextChanged += (s, e) => FilterFleetGrid();

            cboFleetGroupFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 8.5f), Size = new Size(150, 24), Margin = new Padding(0, 0, 8, 0) };
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

            dgvFleet = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.FromArgb(13, 21, 34), ForeColor = Color.Black, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

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
            viewHistory = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(12) };

            Panel topRow = new Panel { Dock = DockStyle.Top, Height = 42 };
            Label lblTitle = new Label { Text = "Connection History & Auditable Session Logs", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

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

            dgvHistory = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.FromArgb(13, 21, 34), ForeColor = Color.Black, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

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
            viewSettingsStudio = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(12) };

            Panel header = new Panel { Dock = DockStyle.Top, Height = 42 };
            Label lblTitle = new Label { Text = "Settings & Security Studio (RustDesk / AnyDesk Engine)", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

            ModernButton btnSave = new ModernButton { Text = "💾 Save & Apply", NormalColor = Color.FromArgb(16, 185, 129), HoverColor = Color.FromArgb(21, 150, 78), Size = new Size(130, 28), Dock = DockStyle.Right };
            btnSave.Click += (s, e) => {
                db.Save();
                ShowToast("Settings saved to database.json.");
            };

            header.Controls.Add(btnSave);
            header.Controls.Add(lblTitle);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 200 };
            settingsTree = new TreeView { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9f), BackColor = Color.FromArgb(13, 21, 34), ForeColor = Color.White, BorderStyle = BorderStyle.None };
            settingsTree.Nodes.Add("General", "General (Startup & Channel)");
            settingsTree.Nodes.Add("Connection", "Connection & Relays (hbbs/hbbr)");
            settingsTree.Nodes.Add("Security", "Security & IP Whitelist");
            settingsTree.Nodes.Add("Display", "Display & Scaling");
            settingsTree.Nodes.Add("Account", "Operator Credentials");

            settingsTree.AfterSelect += (s, e) => RenderSettingsSubPanel(e.Node.Name);
            split.Panel1.Controls.Add(settingsTree);

            settingsSubContentPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(20, 28, 45), Padding = new Padding(16) };
            split.Panel2.Controls.Add(settingsSubContentPanel);

            viewSettingsStudio.Controls.Add(split);
            viewSettingsStudio.Controls.Add(header);
            header.BringToFront();

            settingsTree.SelectedNode = settingsTree.Nodes[0];
        }

        private void RenderSettingsSubPanel(string category)
        {
            settingsSubContentPanel.Controls.Clear();
            int y = 10;

            Action<string> addH = (text) => {
                Label l = new Label { Text = text, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(10, y), AutoSize = true };
                settingsSubContentPanel.Controls.Add(l);
                y += 30;
            };

            Func<string, bool, CheckBox> addCb = (text, isChecked) => {
                CheckBox cb = new CheckBox { Text = text, Checked = isChecked, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(14, y), AutoSize = true, Cursor = Cursors.Hand };
                settingsSubContentPanel.Controls.Add(cb);
                y += 26;
                return cb;
            };

            Func<string, string, TextBox> addTb = (label, val) => {
                Label lbl = new Label { Text = label, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(14, y), AutoSize = true };
                settingsSubContentPanel.Controls.Add(lbl);
                y += 18;
                TextBox tb = new TextBox { Text = val, Font = new Font("Segoe UI", 9f), BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Location = new Point(14, y), Size = new Size(340, 24) };
                settingsSubContentPanel.Controls.Add(tb);
                y += 32;
                return tb;
            };

            if (category == "General")
            {
                addH("General Preferences");
                CheckBox cbStart = addCb("Open address book on startup", db.Settings.general.openAddressBookOnStartup);
                cbStart.CheckedChanged += (s, e) => db.Settings.general.openAddressBookOnStartup = cbStart.Checked;
                CheckBox cbUpd = addCb("Automatically download stable updates", db.Settings.general.autoUpdate);
                cbUpd.CheckedChanged += (s, e) => db.Settings.general.autoUpdate = cbUpd.Checked;
            }
            else if (category == "Connection")
            {
                addH("RustDesk Network Configuration");
                TextBox tbId = addTb("RustDesk ID Server (hbbs :21116)", db.Settings.connection.idServer);
                tbId.TextChanged += (s, e) => db.Settings.connection.idServer = tbId.Text.Trim();
                TextBox tbRelay = addTb("RustDesk Relay Server (hbbr :21117)", db.Settings.connection.relayServer);
                tbRelay.TextChanged += (s, e) => db.Settings.connection.relayServer = tbRelay.Text.Trim();
            }
            else if (category == "Security")
            {
                addH("Access Control & IP Perimeter Whitelist");
                CheckBox cbUnat = addCb("Enable Unattended 24/7 Access", db.Settings.access.unattendedAccess);
                cbUnat.CheckedChanged += (s, e) => db.Settings.access.unattendedAccess = cbUnat.Checked;
                addTb("Session Inactivity Timeout (minutes)", db.Settings.access.accessTimeout.ToString());
            }
            else if (category == "Display")
            {
                addH("Display & Smart Fit Scaling");
                addTb("Active Scaling Mode", smartFitMode);
                CheckBox cbHw = addCb("Enable Hardware Acceleration (DirectX/GPU)", db.Settings.display.hardwareAcceleration);
                cbHw.CheckedChanged += (s, e) => db.Settings.display.hardwareAcceleration = cbHw.Checked;
            }
            else if (category == "Account")
            {
                addH("Cluster & Operator Credentials");
                addTb("Licensed Operator", db.Settings.account.username);
                addTb("Cluster Tenant", db.Settings.account.enterpriseTenant);
            }
        }

        // ======================================================================
        // SYSTEM TRAY & UTILITIES
        // ======================================================================

        private void SetupTraySupervisor()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Open Antigravity RDS Workspace", (s, e) => { this.WindowState = FormWindowState.Normal; this.Show(); this.BringToFront(); });
            trayMenu.MenuItems.Add("Copy Address", (s, e) => { if (currentHostDesktop != null) Clipboard.SetText(currentHostDesktop.RemoteId); });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", (s, e) => { trayIcon.Visible = false; Application.Exit(); });

            trayIcon = new NotifyIcon {
                Text = "jDroid-X-Desk Console",
                Icon = SystemIcons.Shield,
                ContextMenu = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => { this.WindowState = FormWindowState.Normal; this.Show(); this.BringToFront(); };
        }

        private void ShowToast(string message)
        {
            if (notificationToast == null)
            {
                notificationToast = new Panel { Size = new Size(380, 42), Anchor = AnchorStyles.Bottom | AnchorStyles.Right, BackColor = Color.FromArgb(9, 13, 22), BorderStyle = BorderStyle.FixedSingle };
                lblToastMessage = new Label { Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
                notificationToast.Controls.Add(lblToastMessage);

                toastTimer = new System.Windows.Forms.Timer { Interval = 3000 };
                toastTimer.Tick += (s, e) => { toastTimer.Stop(); notificationToast.Visible = false; };
                this.Controls.Add(notificationToast);
                notificationToast.BringToFront();
            }

            lblToastMessage.Text = message;
            notificationToast.Location = new Point(this.ClientSize.Width - notificationToast.Width - 16, this.ClientSize.Height - notificationToast.Height - 16);
            notificationToast.Visible = true;
            notificationToast.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(1500, "jDroid-X-Desk", "Minimized to system tray.", ToolTipIcon.Info);
            }
            else
            {
                base.OnFormClosing(e);
            }
        }
    }
}
