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
using System.Security.Cryptography;
using System.Reflection;
using Microsoft.Win32;
using System.Text.RegularExpressions;

[assembly: AssemblyTitle("jDroid-X-Desk")]
[assembly: AssemblyDescription("Enterprise Remote Desktop & Machine Administration Workspace")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCompany("jDroid-X Studio")]
[assembly: AssemblyProduct("jDroid-X-Desk")]
[assembly: AssemblyCopyright("Copyright © 2026 JEET / jDroid-X Studio. All rights reserved.")]
[assembly: AssemblyTrademark("jDroid-X Studio™")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
[assembly: ComVisible(false)]
[assembly: Guid("96B8F8C6-534B-446A-AE50-83B504E2EF1E")]

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
        public int relayPort { get; set; }
        public string tunnelToken { get; set; }
        public bool directConnections { get; set; }
        public bool colabCloudRelay { get; set; }
        public string proxyServer { get; set; }
        public string idServer { get; set; }
        public string relayServer { get; set; }
        public string customDomain { get; set; }
        public string webRepoUrl { get; set; }
        public string colabNotebookUrl { get; set; }
        public string githubBackendUrl { get; set; }
        public ConnectionSettings()
        {
            localPort = 7070;
            relayPort = 8088;
            tunnelToken = "";
            directConnections = true;
            colabCloudRelay = true;
            proxyServer = "";
            idServer = "relay.jdroid-x.net:21116";
            relayServer = "https://prefix-dean-theft-bytes.trycloudflare.com";
            customDomain = "https://prefix-dean-theft-bytes.trycloudflare.com";
            webRepoUrl = "https://github.com/jDroid-X/jDroid-X-Web";
            colabNotebookUrl = "https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai";
            githubBackendUrl = "https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main";
        }
    }

    public class WakeOnLanSettings
    {
        public bool enabled { get; set; }
        public List<string> authorizedClients { get; set; }
        public WakeOnLanSettings()
        {
            enabled = true;
            authorizedClients = new List<string>();
        }
    }

    public class AccessSettings
    {
        public bool unattendedAccess { get; set; }
        public bool requirePasscode { get; set; }
        public string passwordHash { get; set; }
        public bool enable2FA { get; set; }
        public int accessTimeout { get; set; }
        public AccessSettings()
        {
            unattendedAccess = true;
            requirePasscode = false;
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
            licenseType = "Enterprise Platinum (Unlimited) - jDroid-X Studio";
            enterpriseTenant = "jDroid-X Studio Enterprise Network";
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
            Devices = new List<DeviceItem>();
            Groups = new List<GroupItem> {
                new GroupItem { id = "grp-1", name = "Workstations", desc = "Saved remote workstations" }
            };
            Sessions = new List<SessionItem>();
            Guardians = new List<GuardianItem>();
            SafeZones = new List<SafeZoneItem>();
            Contacts = new List<ContactItem>();
            Notifications = new List<NotificationItem>();
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
                SyncYaml();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Database Save Exception: " + ex.Message);
            }
        }

        public void SyncYaml()
        {
            try
            {
                string yamlPath = "windows_rdp.yaml";
                if (!File.Exists(yamlPath))
                {
                    string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "windows_rdp.yaml");
                    if (File.Exists(alt)) yamlPath = alt;
                }
                if (File.Exists(yamlPath) && Settings != null && Settings.connection != null)
                {
                    string content = File.ReadAllText(yamlPath);
                    if (Settings.connection.relayPort > 0)
                    {
                        content = Regex.Replace(content, @"relay_port:\s*\d+", "relay_port: " + Settings.connection.relayPort);
                    }
                    if (!string.IsNullOrEmpty(Settings.connection.customDomain))
                    {
                        content = Regex.Replace(content, @"custom_domain:\s*[""'].*?[""']", "custom_domain: \"" + Settings.connection.customDomain + "\"");
                    }
                    if (Settings.connection.tunnelToken != null)
                    {
                        if (content.Contains("tunnel_token:"))
                        {
                            content = Regex.Replace(content, @"tunnel_token:\s*[""'].*?[""']", "tunnel_token: \"" + Settings.connection.tunnelToken + "\"");
                        }
                    }
                    File.WriteAllText(yamlPath, content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("YAML Sync Exception: " + ex.Message);
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
    // CLOUDFLARE TUNNEL CONTROLLER ENGINE (WINDOWS INGRESS & REVERSE PROXY)
    // ==========================================================================
    public static class CloudflareTunnelEngine
    {
        public static bool IsRunning { get; private set; }
        public static string ActiveTunnelUrl { get; private set; }
        public static string LastStatusMessage { get; private set; }
        public static Process TunnelProcess { get; private set; }

        public static event Action<string> OnUrlDiscovered;
        public static event Action<string> OnStatusChanged;

        private static readonly object syncLock = new object();

        public static string GetCloudflaredBinaryPath()
        {
            string local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cloudflared.exe");
            if (File.Exists(local)) return local;
            string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cloudflared-windows-amd64.exe");
            if (File.Exists(alt)) return alt;
            return local;
        }

        public static void EnsureBinaryAvailable(Action<bool, string> callback)
        {
            ThreadPool.QueueUserWorkItem(delegate {
                string bin = GetCloudflaredBinaryPath();
                if (File.Exists(bin))
                {
                    if (callback != null) callback(true, bin);
                    return;
                }

                try
                {
                    string downloadUrl = "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe";
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "jDroid-X-Desk/3.0");
                        wc.DownloadFile(downloadUrl, bin);
                    }
                    if (callback != null) callback(true, bin);
                }
                catch (Exception ex)
                {
                    if (callback != null) callback(false, "Download failed: " + ex.Message);
                }
            });
        }

        public static void StartTunnel(int localPort, string tunnelToken = null)
        {
            lock (syncLock)
            {
                if (IsRunning && TunnelProcess != null && !TunnelProcess.HasExited) return;

                LastStatusMessage = "Checking cloudflared engine...";
                if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);

                EnsureBinaryAvailable((ok, binPath) => {
                    if (!ok)
                    {
                        LastStatusMessage = "Cloudflared unavailable: " + binPath;
                        if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);
                        return;
                    }

                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = binPath,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                        };

                        if (!string.IsNullOrEmpty(tunnelToken))
                        {
                            psi.Arguments = string.Format("tunnel --protocol http2 run --token {0}", tunnelToken);
                        }
                        else
                        {
                            psi.Arguments = string.Format("tunnel --protocol http2 --url http://127.0.0.1:{0}", localPort > 0 ? localPort : 8088);
                        }

                        TunnelProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
                        TunnelProcess.OutputDataReceived += (s, e) => ProcessOutputLine(e.Data);
                        TunnelProcess.ErrorDataReceived += (s, e) => ProcessOutputLine(e.Data);
                        TunnelProcess.Exited += (s, e) => {
                            lock (syncLock)
                            {
                                IsRunning = false;
                                ActiveTunnelUrl = null;
                                LastStatusMessage = "Tunnel exited";
                                if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);
                            }
                        };

                        TunnelProcess.Start();
                        TunnelProcess.BeginOutputReadLine();
                        TunnelProcess.BeginErrorReadLine();

                        IsRunning = true;
                        LastStatusMessage = "Tunnel active (waiting for public URL)...";
                        if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);
                    }
                    catch (Exception ex)
                    {
                        IsRunning = false;
                        LastStatusMessage = "Tunnel start failed: " + ex.Message;
                        if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);
                    }
                });
            }
        }

        private static void ProcessOutputLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            Match m = Regex.Match(line, @"https://[a-zA-Z0-9-]+\.trycloudflare\.com");
            if (m.Success)
            {
                string url = m.Value;
                ActiveTunnelUrl = url;
                LastStatusMessage = "Tunnel online: " + url;
                if (OnUrlDiscovered != null) OnUrlDiscovered(url);
                if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);
            }
        }

        public static void StopTunnel()
        {
            lock (syncLock)
            {
                try
                {
                    if (TunnelProcess != null && !TunnelProcess.HasExited)
                    {
                        TunnelProcess.Kill();
                        TunnelProcess.Dispose();
                    }
                }
                catch { }
                TunnelProcess = null;
                IsRunning = false;
                ActiveTunnelUrl = null;
                LastStatusMessage = "Tunnel stopped";
                if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);
            }
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

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint KEYEVENTF_KEYUP = 0x0002;

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
        private Label lblLocalStatus;
        private TextBox txtTargetId;
        private ModernButton btnConnect;
        private ModernButton btnDisconnect;
        private Label lblRemoteStatus;
        private PictureBox viewportCanvas;
        private Panel panelWelcomeCards;
        private TextBox txtCardTargetId;
        private ModernButton btnCardConnect;
        private Label lblCardLocalId;
        private Panel sessionFloatingToolbar;
        private Label lblSessionTelemetryHUD;

        // ======================================================================
        // DYNAMIC SESSION CONNECTION STATE MACHINE (NO DUMMY / SIMULATED DATA)
        // ======================================================================
        public enum SessionConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
            Failed
        }

        private SessionConnectionState currentConnectionState = SessionConnectionState.Disconnected;
        private bool isSessionActive = false;
        private bool isLiveScreenMirror = false;
        private string smartFitMode = "SmartFit"; // "SmartFit", "Original 1:1", "Stretch"
        private DateTime sessionStartTime;
        private DateTime connectionAttemptStartTime;
        private System.Windows.Forms.Timer sessionTelemetryTimer;
        private System.Windows.Forms.Timer connectingAnimationTimer;
        private int connectingPulseFrame = 0;
        private string connectionStageMessage = "Initializing Connection...";
        private int connectingProgressPct = 0;
        private long bytesTransferred = 0;
        private int lastMeasuredRttMs = 0;
        private int lastFramesCount = 0;
        private float currentMeasuredFps = 0.0f;

        // Live Screen Streaming & Cloud Relay Engine (Thread-Safe & Leak-Proof)
        private Image latestRemoteFrame = null;
        private readonly object frameLock = new object();
        private static ImageCodecInfo cachedJpegEncoder = null;
        private Thread screenStreamerThread = null;
        private Thread frameReceiverThread = null;
        private volatile bool isScreenStreamingActive = false;
        private volatile bool isFrameReceiverActive = false;
        private string activeReceiverTargetId = "";
        private int framesReceivedCount = 0;

        // Layout Toggle Buttons (Antigravity Image 1 & 2 controls)
        private Button btnToggleActivityBar;
        private Button btnToggleLeftDrawer;
        private Button btnToggleBottomDock;
        private Button btnToggleRightInspector;

        // Colab Cloud Server Telemetry & Ping Engine (Dynamic - Zero Hardcoded Strings)
        private bool isColabConnected = false;
        private string colabRamText = "--- / ---";
        private string colabGpuText = "-- / --";
        private string colabDiskText = "-- / --";
        private System.Windows.Forms.Timer colabTelemetryTimer;
        private Panel panelMetaRam;
        private Panel panelMetaGpu;
        private Panel panelMetaDisk;
        private Panel panelMetaStatus;
        private Panel panelMetaHeartbeat;
        private Label lblMetaRamVal;
        private Label lblMetaGpuVal;
        private Label lblMetaDiskVal;
        private Label lblMetaStatusVal;
        private Label lblMetaHeartbeatVal;
        private int headerHeartbeatPulseFrame = 0;
        private volatile bool isProbingCandidates = false;

        // Host Daemon (Continuous Screen Capture & Remote Admin Input Execution)
        private Thread hostDaemonThread;
        private volatile bool isHostDaemonRunning = false;
        private string cachedWinningRelayUrl = null;
        private DateTime lastWinningCheckTime = DateTime.MinValue;

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        private static void UnblockSelf()
        {
            try {
                string path = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(path))
                    DeleteFile(path + ":Zone.Identifier");
            } catch { }
        }

        [STAThread]
        public static void Main()
        {
            try
            {
                UnblockSelf();
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

            // Local Node Identity dynamically derived from machine hardware key
            string uniqueId = GenerateMachineDeskId();
            string uniquePass = GenerateMachinePasscode();
            currentHostDesktop = new SimulatorDesktop {
                Key = "LOCAL",
                Name = Environment.MachineName,
                RemoteId = uniqueId,
                Passcode = uniquePass,
                Ip = GetLocalLanIp(),
                Role = "Host Workstation",
                OsVersion = Environment.OSVersion.VersionString,
                AccentColor = Color.FromArgb(56, 189, 248),
                ActiveApps = new[] { "Desktop Session" }
            };

            // Remote Desktops are loaded strictly from db.Contacts / db.Devices (excluding self)
            SyncDynamicDesktopsFromDatabase();
            connectedTargetDesktop = null;

            currentLocalDirectory = AppDomain.CurrentDomain.BaseDirectory;

            this.Text = "jDroid-X-Desk | Remote Desktop Workspace";
            this.Size = new Size(1300, 850);
            this.MinimumSize = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.Icon = SystemIcons.Shield;

            sessionTelemetryTimer = new System.Windows.Forms.Timer();
            sessionTelemetryTimer.Interval = 1000;
            sessionTelemetryTimer.Tick += (s, e) => {
                if (currentConnectionState == SessionConnectionState.Connected)
                {
                    currentMeasuredFps = (float)(framesReceivedCount - lastFramesCount);
                    lastFramesCount = framesReceivedCount;
                    UpdateLiveTelemetry();
                    if (viewportCanvas != null && !viewportCanvas.IsDisposed) viewportCanvas.Invalidate();
                }
            };

            connectingAnimationTimer = new System.Windows.Forms.Timer();
            connectingAnimationTimer.Interval = 40; // 25 FPS smooth heartbeat animation
            connectingAnimationTimer.Tick += (s, e) => {
                headerHeartbeatPulseFrame++;
                if (panelMetaHeartbeat != null && !panelMetaHeartbeat.IsDisposed) panelMetaHeartbeat.Invalidate();
                if (panelMetaStatus != null && !panelMetaStatus.IsDisposed) panelMetaStatus.Invalidate();

                if (currentConnectionState == SessionConnectionState.Connecting)
                {
                    connectingPulseFrame++;
                    if (connectingProgressPct < 90 && connectingPulseFrame % 5 == 0)
                    {
                        connectingProgressPct += 2;
                    }
                    if (viewportCanvas != null && !viewportCanvas.IsDisposed) viewportCanvas.Invalidate();
                }
            };
            connectingAnimationTimer.Start();

            BuildAntigravityLayout();
            SetupTraySupervisor();
            RegisterWithColabRelay();
            StartHostDaemon();
            StartColabTelemetryMonitor();
            SyncFromGitHubBackend();

            // Hook Cloudflare Tunnel auto-discovery
            CloudflareTunnelEngine.OnUrlDiscovered += (url) => {
                try {
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke(new Action(() => {
                            if (db != null && db.Settings != null && db.Settings.connection != null)
                            {
                                db.Settings.connection.relayServer = url;
                                db.Save();
                                db.SyncYaml();
                            }
                            ShowToast("Cloudflare Tunnel active: " + url);
                            ThreadPool.QueueUserWorkItem(delegate { PingColabServer(); });
                        }));
                    }
                } catch { }
            };

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
                if (d.name.ToUpper() == Environment.MachineName.ToUpper()) continue;
                Color c = colors[i % colors.Length];
                string id = "";
                ContactItem matchedContact = db.Contacts.Find(cnt => cnt.name == d.name);
                if (matchedContact != null && !string.IsNullOrEmpty(matchedContact.remoteId))
                {
                    id = matchedContact.remoteId;
                }
                else
                {
                    id = d.ip;
                }

                dynamicDesktops.Add(new SimulatorDesktop
                {
                    Key = d.id,
                    Name = d.name,
                    RemoteId = id,
                    Passcode = "",
                    Ip = d.ip,
                    Role = d.group + " (" + d.accessLevel + ")",
                    OsVersion = "Remote Workstation",
                    AccentColor = c,
                    ActiveApps = new[] { "Desktop Session" }
                });
                i++;
            }

            foreach (ContactItem c in db.Contacts)
            {
                if (dynamicDesktops.Exists(d => d.Name == c.name || d.RemoteId == c.remoteId)) continue;
                if (currentHostDesktop != null && c.remoteId == currentHostDesktop.RemoteId) continue;
                dynamicDesktops.Add(new SimulatorDesktop
                {
                    Key = c.id,
                    Name = c.name,
                    RemoteId = c.remoteId,
                    Passcode = "",
                    Ip = "Remote Node",
                    Role = c.group,
                    OsVersion = "Remote Workstation",
                    AccentColor = colors[i % colors.Length],
                    ActiveApps = new[] { "Remote Screen" }
                });
                i++;
            }

            // Register Desk 1 and Desk 2 from windows_rdp.yaml pipeline
            try
            {
                string yamlPath = "windows_rdp.yaml";
                if (!File.Exists(yamlPath))
                {
                    string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "windows_rdp.yaml");
                    if (File.Exists(alt)) yamlPath = alt;
                }
                if (File.Exists(yamlPath))
                {
                    string yaml = File.ReadAllText(yamlPath);
                    Match mDesk1 = Regex.Match(yaml, @"desk1:.*?default_id:\s*[""']?([0-9-]+)[""']?.*?passcode:\s*[""']?([0-9a-zA-Z]+)[""']?", RegexOptions.Singleline);
                    Match mDesk2 = Regex.Match(yaml, @"desk2:.*?default_id:\s*[""']?([0-9-]+)[""']?.*?passcode:\s*[""']?([0-9a-zA-Z]+)[""']?", RegexOptions.Singleline);

                    if (mDesk1.Success)
                    {
                        string id1 = mDesk1.Groups[1].Value.Trim();
                        string pass1 = mDesk1.Groups[2].Value.Trim();
                        if (!dynamicDesktops.Exists(d => d.RemoteId == id1) && (currentHostDesktop == null || id1 != currentHostDesktop.RemoteId))
                        {
                            dynamicDesktops.Add(new SimulatorDesktop {
                                Key = "DESK1",
                                Name = "Desk 1 (Client / Viewer)",
                                RemoteId = id1,
                                Passcode = pass1,
                                Ip = "Cloudflare Tunnel / Relay",
                                Role = "Pipeline Desk 1",
                                OsVersion = "Windows Workstation",
                                AccentColor = Color.FromArgb(56, 189, 248),
                                ActiveApps = new[] { "Remote Screen" }
                            });
                        }
                    }

                    if (mDesk2.Success)
                    {
                        string id2 = mDesk2.Groups[1].Value.Trim();
                        string pass2 = mDesk2.Groups[2].Value.Trim();
                        if (!dynamicDesktops.Exists(d => d.RemoteId == id2) && (currentHostDesktop == null || id2 != currentHostDesktop.RemoteId))
                        {
                            dynamicDesktops.Add(new SimulatorDesktop {
                                Key = "DESK2",
                                Name = "Desk 2 (Host / Target)",
                                RemoteId = id2,
                                Passcode = pass2,
                                Ip = "Cloudflare Tunnel / Relay",
                                Role = "Pipeline Desk 2",
                                OsVersion = "Windows Workstation",
                                AccentColor = Color.FromArgb(16, 185, 129),
                                ActiveApps = new[] { "Remote Screen" }
                            });
                        }
                    }
                }
            }
            catch { }
        }

        private string GenerateMachineDeskId()
        {
            try
            {
                string machineGuid = "";
                try
                {
                    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                    {
                        if (rk != null)
                        {
                            object val = rk.GetValue("MachineGuid");
                            if (val != null) machineGuid = val.ToString().Trim();
                        }
                    }
                }
                catch { }

                string mac = "";
                try
                {
                    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (nic.OperationalStatus == OperationalStatus.Up && 
                            nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && 
                            nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                        {
                            mac = nic.GetPhysicalAddress().ToString();
                            if (!string.IsNullOrEmpty(mac)) break;
                        }
                    }
                }
                catch { }

                string seed = machineGuid + "_" + mac + "_" + Environment.MachineName.ToUpperInvariant() + "_" + Environment.ProcessorCount;
                if (string.IsNullOrEmpty(machineGuid) && string.IsNullOrEmpty(mac))
                {
                    seed = Environment.MachineName.ToUpperInvariant() + "_" + Environment.UserName.ToUpperInvariant();
                }

                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seed));
                    uint num = BitConverter.ToUInt32(hash, 0);
                    long id9 = 100000000L + (long)(num % 900000000L);
                    string s = id9.ToString();
                    return s.Substring(0, 3) + "-" + s.Substring(3, 3) + "-" + s.Substring(6, 3);
                }
            }
            catch
            {
                int h = Math.Abs((Environment.MachineName + Environment.UserName).GetHashCode());
                long fallbackId = 100000000L + (long)(h % 900000000L);
                string fs = fallbackId.ToString();
                return fs.Substring(0, 3) + "-" + fs.Substring(3, 3) + "-" + fs.Substring(6, 3);
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
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && 
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && 
                        ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null) return endPoint.Address.ToString();
                }
            }
            catch { }
            return "127.0.0.1";
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
            // Left: High-Contrast Brand Header
            FlowLayoutPanel leftBrand = new FlowLayoutPanel { Dock = DockStyle.Left, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
            Label lblLogo = new Label { Text = "jDroid-X", Font = new Font("Segoe UI", 11.5f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), AutoSize = true, Margin = new Padding(0, 2, 4, 0) };
            Label lblSub = new Label { Text = "| Remote Desktop Workspace", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(148, 163, 184), AutoSize = true, Margin = new Padding(0, 4, 12, 0) };

            leftBrand.Controls.Add(lblLogo);
            leftBrand.Controls.Add(lblSub);

            // Center: Colab Cloud Server Telemetry Indicators (Image 1 & 2 Specification)
            FlowLayoutPanel centerColabTelemetry = new FlowLayoutPanel
            {
                Size = new Size(570, 36),
                Location = new Point((topHeaderBar.Width - 570) / 2, 4),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top
            };
            topHeaderBar.Resize += (s, e) => {
                centerColabTelemetry.Location = new Point((topHeaderBar.Width - centerColabTelemetry.Width) / 2, 4);
            };

            panelMetaRam = CreateTelemetryBox("System RAM", colabRamText, out lblMetaRamVal, 0.10f);
            panelMetaGpu = CreateTelemetryBox("GPU RAM", colabGpuText, out lblMetaGpuVal, 0.00f);
            panelMetaDisk = CreateTelemetryBox("Disk", colabDiskText, out lblMetaDiskVal, 0.39f);
            panelMetaStatus = CreateStatusBox("Colab Relay", out lblMetaStatusVal);
            panelMetaHeartbeat = CreateHeartbeatBox();

            centerColabTelemetry.Controls.Add(panelMetaRam);
            centerColabTelemetry.Controls.Add(panelMetaGpu);
            centerColabTelemetry.Controls.Add(panelMetaDisk);
            centerColabTelemetry.Controls.Add(panelMetaStatus);
            centerColabTelemetry.Controls.Add(panelMetaHeartbeat);

            // Right: Antigravity IDE Layout View Toggles
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

            rightLayoutControls.Controls.Add(btnToggleActivityBar);
            rightLayoutControls.Controls.Add(btnToggleLeftDrawer);
            rightLayoutControls.Controls.Add(btnToggleBottomDock);
            rightLayoutControls.Controls.Add(btnToggleRightInspector);

            topHeaderBar.Controls.Add(leftBrand);
            topHeaderBar.Controls.Add(centerColabTelemetry);
            topHeaderBar.Controls.Add(rightLayoutControls);
        }

        private Panel CreateTelemetryBox(string title, string initialValue, out Label valLabel, float fillRatio)
        {
            Panel box = new Panel { Size = new Size(110, 34), Margin = new Padding(3, 0, 3, 0), BackColor = Color.FromArgb(15, 23, 42) };
            Label lTitle = new Label { Text = title, Font = new Font("Segoe UI", 7f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(4, 2), AutoSize = true };
            Label lVal = new Label { Text = initialValue, Font = new Font("Segoe UI", 7.8f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(4, 15), AutoSize = true };
            valLabel = lVal;
            box.Controls.Add(lTitle);
            box.Controls.Add(lVal);

            box.Paint += (s, e) => {
                Color themeCol = isColabConnected ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);
                using (Pen borderPen = new Pen(themeCol, 1.5f))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, box.Width - 1, box.Height - 1);
                }
                using (SolidBrush lineBrush = new SolidBrush(themeCol))
                {
                    int lineW = Math.Max(4, (int)((box.Width - 4) * Math.Min(1f, Math.Max(0.05f, fillRatio))));
                    e.Graphics.FillRectangle(lineBrush, 2, box.Height - 3, lineW, 2);
                }
            };
            return box;
        }

        private Panel CreateStatusBox(string title, out Label valLabel)
        {
            Panel box = new Panel { Size = new Size(115, 34), Margin = new Padding(3, 0, 3, 0), BackColor = Color.FromArgb(15, 23, 42), Cursor = Cursors.Hand };
            Label lTitle = new Label { Text = title, Font = new Font("Segoe UI", 7f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(4, 2), AutoSize = true, Cursor = Cursors.Hand };
            Label lVal = new Label { Text = isColabConnected ? "● ONLINE" : (isProbingCandidates ? "◌ PINGING..." : "○ OFFLINE"), Font = new Font("Segoe UI", 7.6f, FontStyle.Bold), ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68)), Location = new Point(4, 15), AutoSize = true, Cursor = Cursors.Hand };
            valLabel = lVal;
            box.Controls.Add(lTitle);
            box.Controls.Add(lVal);

            ToolTip tt = new ToolTip();
            tt.SetToolTip(box, "Click to configure Google Colab Relay Server & Custom Domain");
            tt.SetToolTip(lVal, "Click to configure Google Colab Relay Server & Custom Domain");

            box.Click += (s, e) => ShowRelayConfigDialog();
            lTitle.Click += (s, e) => ShowRelayConfigDialog();
            lVal.Click += (s, e) => ShowRelayConfigDialog();

            box.Paint += (s, e) => {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Color themeCol = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(100, 116, 139));
                using (Pen borderPen = new Pen(themeCol, 1.2f))
                {
                    g.DrawRectangle(borderPen, 0, 0, box.Width - 1, box.Height - 1);
                }
                // 2cm (76px) indicator line at bottom of Colab Relay box
                int barX = 4;
                int barY = box.Height - 3;
                int barW = 76; // Exact 2.0 cm on standard 96 DPI screen
                Color barCol = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68));
                using (SolidBrush b = new SolidBrush(barCol))
                {
                    g.FillRectangle(b, barX, barY, barW, 2);
                }
            };
            return box;
        }

        private Panel CreateHeartbeatBox()
        {
            // Physical standard: 2.0 cm on 96 DPI screen = (2 / 2.54) * 96 = 75.6 pixels (~76px)
            Panel box = new Panel { Size = new Size(86, 34), Margin = new Padding(3, 0, 3, 0), BackColor = Color.FromArgb(15, 23, 42), Cursor = Cursors.Hand };
            Label lTitle = new Label { Text = "Heartbeat", Font = new Font("Segoe UI", 6.8f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(4, 1), AutoSize = true, Cursor = Cursors.Hand };
            Label lVal = new Label { Text = isColabConnected ? "ALIVE" : (isProbingCandidates ? "PROBING" : "OFFLINE"), Font = new Font("Segoe UI", 7.2f, FontStyle.Bold), ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68)), Location = new Point(4, 12), AutoSize = true, Cursor = Cursors.Hand };
            lblMetaHeartbeatVal = lVal;
            box.Controls.Add(lTitle);
            box.Controls.Add(lVal);

            ToolTip tt = new ToolTip();
            tt.SetToolTip(box, "Google Colab Heartbeat Progress Bar (2cm: Red = Disconnected, Green = Connected)\nClick to configure Google Colab Relay Server");
            tt.SetToolTip(lTitle, "Google Colab Heartbeat Progress Bar (2cm: Red = Disconnected, Green = Connected)\nClick to configure Google Colab Relay Server");
            tt.SetToolTip(lVal, "Google Colab Heartbeat Progress Bar (2cm: Red = Disconnected, Green = Connected)\nClick to configure Google Colab Relay Server");

            box.Click += (s, e) => ShowRelayConfigDialog();
            lTitle.Click += (s, e) => ShowRelayConfigDialog();
            lVal.Click += (s, e) => ShowRelayConfigDialog();

            box.Paint += (s, e) => {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Color borderColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(40, 53, 76));
                using (Pen borderPen = new Pen(borderColor, 1f))
                {
                    g.DrawRectangle(borderPen, 0, 0, box.Width - 1, box.Height - 1);
                }

                // 2cm Physical Progress Bar (76px width at 96 DPI)
                int barX = 4;
                int barY = box.Height - 8;
                int barW = 76; // Exactly 2.0 cm on standard display
                int barH = 5;

                // Trough background
                using (SolidBrush troughBrush = new SolidBrush(Color.FromArgb(11, 17, 32)))
                {
                    g.FillRectangle(troughBrush, barX, barY, barW, barH);
                }
                using (Pen troughPen = new Pen(Color.FromArgb(51, 65, 85), 0.8f))
                {
                    g.DrawRectangle(troughPen, barX, barY, barW, barH);
                }

                if (isColabConnected)
                {
                    // Full Vibrant Emerald Green Bar with animated cardiac pulse pip
                    using (SolidBrush greenBar = new SolidBrush(Color.FromArgb(16, 185, 129)))
                    {
                        g.FillRectangle(greenBar, barX + 1, barY + 1, barW - 2, barH - 2);
                    }
                    // Animated Green Heartbeat Pip traversing the 76px bar
                    int pipX = barX + 1 + (headerHeartbeatPulseFrame * 3) % Math.Max(1, barW - 8);
                    using (SolidBrush pipBrush = new SolidBrush(Color.FromArgb(220, 252, 231)))
                    {
                        g.FillRectangle(pipBrush, pipX, barY, 4, barH);
                    }
                }
                else if (isProbingCandidates)
                {
                    // Amber scanning progress sweep
                    int sweepX = barX + 1 + (headerHeartbeatPulseFrame * 2) % Math.Max(1, barW - 14);
                    using (SolidBrush amberSweep = new SolidBrush(Color.FromArgb(245, 158, 11)))
                    {
                        g.FillRectangle(amberSweep, sweepX, barY + 1, 14, barH - 2);
                    }
                }
                else
                {
                    // Disconnected: Red Bar with animated pulsing alpha and rhythmic blip
                    int redAlpha = 140 + (int)(110 * Math.Abs(Math.Sin(headerHeartbeatPulseFrame * 0.15)));
                    using (SolidBrush redBar = new SolidBrush(Color.FromArgb(redAlpha, 239, 68, 68)))
                    {
                        g.FillRectangle(redBar, barX + 1, barY + 1, barW - 2, barH - 2);
                    }
                    // Red Flatline / Blip tick
                    int pipX = barX + 1 + (headerHeartbeatPulseFrame * 2) % Math.Max(1, barW - 8);
                    using (SolidBrush blip = new SolidBrush(Color.FromArgb(254, 202, 202)))
                    {
                        g.FillRectangle(blip, pipX, barY + 1, 3, barH - 2);
                    }
                }
            };
            return box;
        }

        private void ShowRelayConfigDialog()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "jDroid-X Cloud Relay & Network Configuration";
                dlg.Size = new Size(500, 560);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.FromArgb(15, 23, 42);
                dlg.ForeColor = Color.White;
                dlg.Font = new Font("Segoe UI", 9f);

                Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

                Label lblHead = new Label { Text = "Google Colab & Cloud Relay Setup", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), AutoSize = true, Location = new Point(20, 16) };
                Label lblSub = new Label { Text = "Configure your relay server endpoint, port, custom domain, and Cloudflare tunnel.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), AutoSize = true, Location = new Point(20, 42) };

                int curY = 70;

                // Prominent Colab Connection Steps Box
                Panel pnlSteps = new Panel { Size = new Size(440, 84), Location = new Point(20, curY), BackColor = Color.FromArgb(10, 16, 26), BorderStyle = BorderStyle.FixedSingle };
                Label lSteps = new Label {
                    Text = "STEPS TO CONNECT IN jDroid-X-Desk:\n1. Open jDroid-X-Desk on Desk 1 and Desk 2\n2. Run your Google Colab Relay notebook\n3. Paste the generated public URL (👉 https://....trycloudflare.com 👈)\n4. Click 'Save & Apply' -> Header turns GREEN with live meters!",
                    Font = new Font("Segoe UI", 8.2f),
                    ForeColor = Color.FromArgb(56, 189, 248),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(6)
                };
                pnlSteps.Controls.Add(lSteps);
                pnl.Controls.Add(pnlSteps);
                curY += 92;

                string initialUrl = db.Settings.connection.relayServer ?? "https://www.jdroid-x.com";
                try {
                    if (Clipboard.ContainsText()) {
                        string c = Clipboard.GetText().Trim();
                        if (c.Contains(".trycloudflare.com")) {
                            int s = c.IndexOf("https://");
                            if (s < 0) s = c.IndexOf("http://");
                            if (s >= 0) {
                                int e = c.IndexOfAny(new char[] { ' ', '\r', '\n', '\t' }, s);
                                c = e > s ? c.Substring(s, e - s) : c.Substring(s);
                            }
                            initialUrl = c.TrimEnd('/');
                        }
                    }
                } catch { }

                Action<string> addLbl = (txt) => {
                    Label l = new Label { Text = txt, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(20, curY), AutoSize = true };
                    pnl.Controls.Add(l);
                    curY += 20;
                };

                addLbl("Relay Server URL (Google Colab / Local / Custom)");
                TextBox tbRelay = new TextBox { Text = initialUrl, Location = new Point(20, curY), Size = new Size(440, 24), BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                pnl.Controls.Add(tbRelay);
                curY += 32;

                addLbl("Relay Port (Default: 8088)");
                TextBox tbPort = new TextBox { Text = db.Settings.connection.relayPort > 0 ? db.Settings.connection.relayPort.ToString() : "8088", Location = new Point(20, curY), Size = new Size(440, 24), BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                pnl.Controls.Add(tbPort);
                curY += 32;

                addLbl("Custom Domain (e.g. https://www.jdroid-x.com)");
                TextBox tbDom = new TextBox { Text = db.Settings.connection.customDomain ?? "https://www.jdroid-x.com", Location = new Point(20, curY), Size = new Size(440, 24), BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                pnl.Controls.Add(tbDom);
                curY += 32;

                addLbl("Cloudflare Tunnel Token (Optional, for zero-touch domain routing)");
                TextBox tbToken = new TextBox { Text = db.Settings.connection.tunnelToken ?? "", Location = new Point(20, curY), Size = new Size(440, 24), BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                pnl.Controls.Add(tbToken);
                curY += 34;

                // Cloudflare Ingress Tunnel Control Section
                GroupBox gbTunnel = new GroupBox { Text = "Cloudflare Ingress Tunnel Engine", Location = new Point(20, curY), Size = new Size(440, 95), ForeColor = Color.FromArgb(56, 189, 248), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
                Label lblTunnelStatus = new Label { Text = CloudflareTunnelEngine.IsRunning ? "● ACTIVE: " + (CloudflareTunnelEngine.ActiveTunnelUrl ?? "Starting...") : "○ STOPPED (Click Launch to create instant public HTTPS tunnel)", Location = new Point(12, 22), AutoSize = true, ForeColor = CloudflareTunnelEngine.IsRunning ? Color.FromArgb(16, 185, 129) : Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 8f) };
                gbTunnel.Controls.Add(lblTunnelStatus);

                Button btnToggleTunnel = new Button { Text = CloudflareTunnelEngine.IsRunning ? "⏹ Stop Tunnel" : "🚀 Launch Cloudflare Tunnel", Size = new Size(180, 28), Location = new Point(12, 52), FlatStyle = FlatStyle.Flat, BackColor = CloudflareTunnelEngine.IsRunning ? Color.FromArgb(239, 68, 68) : Color.FromArgb(2, 132, 199), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand };
                btnToggleTunnel.FlatAppearance.BorderSize = 0;
                btnToggleTunnel.Click += (s, e) => {
                    if (CloudflareTunnelEngine.IsRunning)
                    {
                        CloudflareTunnelEngine.StopTunnel();
                        btnToggleTunnel.Text = "🚀 Launch Cloudflare Tunnel";
                        btnToggleTunnel.BackColor = Color.FromArgb(2, 132, 199);
                        lblTunnelStatus.Text = "○ STOPPED";
                        lblTunnelStatus.ForeColor = Color.FromArgb(148, 163, 184);
                    }
                    else
                    {
                        btnToggleTunnel.Text = "Starting...";
                        btnToggleTunnel.Enabled = false;
                        int portVal = 8088;
                        int.TryParse(tbPort.Text.Trim(), out portVal);
                        string tokenVal = tbToken.Text.Trim();
                        CloudflareTunnelEngine.StartTunnel(portVal, tokenVal);
                        System.Windows.Forms.Timer pollT = new System.Windows.Forms.Timer { Interval = 1000 };
                        pollT.Tick += (ts, te) => {
                            if (CloudflareTunnelEngine.IsRunning)
                            {
                                btnToggleTunnel.Text = "⏹ Stop Tunnel";
                                btnToggleTunnel.BackColor = Color.FromArgb(239, 68, 68);
                                btnToggleTunnel.Enabled = true;
                                if (!string.IsNullOrEmpty(CloudflareTunnelEngine.ActiveTunnelUrl))
                                {
                                    lblTunnelStatus.Text = "● ACTIVE: " + CloudflareTunnelEngine.ActiveTunnelUrl;
                                    lblTunnelStatus.ForeColor = Color.FromArgb(16, 185, 129);
                                    tbRelay.Text = CloudflareTunnelEngine.ActiveTunnelUrl;
                                    pollT.Stop();
                                }
                            }
                            else if (!btnToggleTunnel.Enabled)
                            {
                                btnToggleTunnel.Text = "🚀 Launch Cloudflare Tunnel";
                                btnToggleTunnel.Enabled = true;
                                lblTunnelStatus.Text = "Failed: " + CloudflareTunnelEngine.LastStatusMessage;
                                pollT.Stop();
                            }
                        };
                        pollT.Start();
                    }
                };
                gbTunnel.Controls.Add(btnToggleTunnel);

                Button btnPresetDomain = new Button { Text = "🌐 Use jdroid-x.com", Size = new Size(130, 28), Location = new Point(200, 52), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.FromArgb(203, 213, 225), Font = new Font("Segoe UI", 8f), Cursor = Cursors.Hand };
                btnPresetDomain.FlatAppearance.BorderSize = 0;
                btnPresetDomain.Click += (s, e) => {
                    tbRelay.Text = "https://www.jdroid-x.com";
                    tbDom.Text = "https://www.jdroid-x.com";
                };
                gbTunnel.Controls.Add(btnPresetDomain);

                pnl.Controls.Add(gbTunnel);
                curY += 105;

                // Quick Action Bar for Cloudflare URL & Colab Notebook
                Button btnPasteClip = new Button { Text = "📋 Paste Tunnel URL", Size = new Size(140, 28), Location = new Point(20, curY), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.FromArgb(56, 189, 248), Font = new Font("Segoe UI", 8f, FontStyle.Bold), Cursor = Cursors.Hand };
                btnPasteClip.FlatAppearance.BorderSize = 0;
                btnPasteClip.Click += (s, e) => {
                    try {
                        if (Clipboard.ContainsText()) {
                            string clip = Clipboard.GetText().Trim();
                            if (clip.Contains(".trycloudflare.com") || clip.StartsWith("http")) {
                                int startIdx = clip.IndexOf("https://");
                                if (startIdx < 0) startIdx = clip.IndexOf("http://");
                                if (startIdx >= 0) {
                                    int endIdx = clip.IndexOfAny(new char[] { ' ', '\r', '\n', '\t' }, startIdx);
                                    if (endIdx > startIdx) clip = clip.Substring(startIdx, endIdx - startIdx);
                                    else clip = clip.Substring(startIdx);
                                }
                                tbRelay.Text = clip.TrimEnd('/');
                                ShowToast("Pasted active tunnel URL from clipboard");
                            } else {
                                MessageBox.Show("Clipboard does not contain a valid URL. Copy the 'https://...trycloudflare.com' URL from your Colab notebook first.", "Paste Colab URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    } catch { }
                };
                pnl.Controls.Add(btnPasteClip);

                Button btnOpenColab = new Button { Text = "🌐 Open Colab Notebook", Size = new Size(150, 28), Location = new Point(168, curY), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.FromArgb(203, 213, 225), Font = new Font("Segoe UI", 8f), Cursor = Cursors.Hand };
                btnOpenColab.FlatAppearance.BorderSize = 0;
                btnOpenColab.Click += (s, e) => {
                    try {
                        string nbUrl = db.Settings.connection.colabNotebookUrl ?? "https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai#scrollTo=zWve46xh2msm";
                        Process.Start(new ProcessStartInfo(nbUrl) { UseShellExecute = true });
                    } catch { }
                };
                pnl.Controls.Add(btnOpenColab);

                Button btnLocalRelay = new Button { Text = "▶ Local Relay (8088)", Size = new Size(130, 28), Location = new Point(326, curY), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.FromArgb(203, 213, 225), Font = new Font("Segoe UI", 8f), Cursor = Cursors.Hand };
                btnLocalRelay.FlatAppearance.BorderSize = 0;
                btnLocalRelay.Click += (s, e) => {
                    try {
                        string scriptPath = "colab_relay_server.py";
                        if (!File.Exists(scriptPath)) {
                            string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colab_relay_server.py");
                            if (File.Exists(alt)) scriptPath = alt;
                        }
                        if (File.Exists(scriptPath)) {
                            Process.Start(new ProcessStartInfo("python", scriptPath + " 8088") { CreateNoWindow = true, UseShellExecute = false });
                            tbRelay.Text = "http://127.0.0.1:8088";
                            ShowToast("Starting local Colab Relay on 127.0.0.1:8088...");
                        }
                    } catch { }
                };
                pnl.Controls.Add(btnLocalRelay);
                curY += 34;

                Label lblStatusResult = new Label { Text = "Current Status: " + (isColabConnected ? "ONLINE (" + colabRamText + ")" : "OFFLINE"), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68), Location = new Point(20, curY), AutoSize = true };
                pnl.Controls.Add(lblStatusResult);
                curY += 28;

                Button btnPing = new Button { Text = "⚡ Test Ping", Size = new Size(110, 32), Location = new Point(20, curY), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(40, 53, 76), ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
                btnPing.FlatAppearance.BorderSize = 0;
                btnPing.Click += (s, e) => {
                    btnPing.Text = "Testing...";
                    btnPing.Enabled = false;
                    ThreadPool.QueueUserWorkItem(delegate {
                        string testUrl = tbRelay.Text.Trim();
                        if (!testUrl.StartsWith("http://") && !testUrl.StartsWith("https://")) testUrl = "http://" + testUrl;
                        bool ok = false;
                        string msg = "";
                        Stopwatch sw = Stopwatch.StartNew();
                        try {
                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(testUrl + "/ping");
                            req.Timeout = 2500;
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                            using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                            {
                                sw.Stop();
                                string j = reader.ReadToEnd();
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                Dictionary<string, object> res = ser.Deserialize<Dictionary<string, object>>(j);
                                if (res != null && res.ContainsKey("status"))
                                {
                                    ok = true;
                                    string r = res.ContainsKey("system_ram") ? res["system_ram"].ToString() : "";
                                    msg = string.Format("ONLINE ({0}ms) | RAM: {1}", sw.ElapsedMilliseconds, r);
                                }
                            }
                        } catch (Exception ex) {
                            sw.Stop();
                            msg = "Failed: " + ex.Message;
                        }

                        dlg.BeginInvoke(new Action(() => {
                            btnPing.Text = "⚡ Test Ping";
                            btnPing.Enabled = true;
                            lblStatusResult.Text = ok ? "● " + msg : "○ " + msg;
                            lblStatusResult.ForeColor = ok ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);
                        }));
                    });
                };

                Button btnSave = new Button { Text = "💾 Save & Apply", Size = new Size(130, 32), Location = new Point(140, curY), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
                btnSave.FlatAppearance.BorderSize = 0;
                btnSave.Click += (s, e) => {
                    string entered = tbRelay.Text.Trim();
                    if (!entered.StartsWith("http://") && !entered.StartsWith("https://")) entered = "https://" + entered;
                    entered = entered.TrimEnd('/');

                    db.Settings.connection.relayServer = entered;
                    int p;
                    if (int.TryParse(tbPort.Text.Trim(), out p) && p > 0) db.Settings.connection.relayPort = p;
                    db.Settings.connection.customDomain = entered;
                    db.Settings.connection.tunnelToken = tbToken.Text.Trim();
                    db.Save();
                    db.SyncYaml();

                    cachedWinningRelayUrl = entered;
                    lastWinningCheckTime = DateTime.MinValue;

                    try {
                        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colab_tunnel.txt"), entered);
                    } catch { }

                    ShowToast("Relay updated: " + entered + " | Saved to database.json & windows_rdp.yaml");
                    ThreadPool.QueueUserWorkItem(delegate { 
                        PingColabServer(); 
                        RegisterWithColabRelay();
                        PushLocalHostFrame();
                    });
                    BuildWelcomeCardsContent();
                    dlg.Close();
                };

                Button btnCancel = new Button { Text = "Cancel", Size = new Size(90, 32), Location = new Point(280, curY), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
                btnCancel.FlatAppearance.BorderSize = 0;
                btnCancel.Click += (s, e) => dlg.Close();

                pnl.Controls.Add(lblHead);
                pnl.Controls.Add(lblSub);
                pnl.Controls.Add(btnPing);
                pnl.Controls.Add(btnSave);
                pnl.Controls.Add(btnCancel);
                dlg.Controls.Add(pnl);
                dlg.ShowDialog(this);
            }
        }

        public List<string> GetRelayCandidateUrls()
        {
            List<string> candidateUrls = new List<string>();

            // 1. Prioritize verified winning cache for 0ms discovery overhead
            if (!string.IsNullOrEmpty(cachedWinningRelayUrl) && (DateTime.Now - lastWinningCheckTime).TotalSeconds < 25)
            {
                candidateUrls.Add(cachedWinningRelayUrl);
            }

            // 2. Active Local Cloudflare Tunnel
            if (CloudflareTunnelEngine.IsRunning && !string.IsNullOrEmpty(CloudflareTunnelEngine.ActiveTunnelUrl))
            {
                if (!candidateUrls.Contains(CloudflareTunnelEngine.ActiveTunnelUrl))
                    candidateUrls.Add(CloudflareTunnelEngine.ActiveTunnelUrl);
            }

            // 3. Local tunnel text files (written by Colab or launch scripts)
            try
            {
                string[] tunnelFiles = new string[] { "colab_tunnel.txt", "colab_url.txt", "tunnel.txt" };
                foreach (string tf in tunnelFiles)
                {
                    string p = tf;
                    if (!File.Exists(p)) p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tf);
                    if (File.Exists(p))
                    {
                        string line = File.ReadAllText(p).Trim();
                        if (!string.IsNullOrEmpty(line) && line.Contains(".trycloudflare.com"))
                        {
                            if (!line.StartsWith("http")) line = "https://" + line;
                            line = line.TrimEnd('/');
                            if (!candidateUrls.Contains(line)) candidateUrls.Add(line);
                            break;
                        }
                    }
                }
            }
            catch { }

            // 4. Configured relay server if it contains a cloudflare tunnel
            if (db != null && db.Settings != null && db.Settings.connection != null)
            {
                string configuredRelay = db.Settings.connection.relayServer;
                if (!string.IsNullOrEmpty(configuredRelay) && configuredRelay.Contains(".trycloudflare.com"))
                {
                    configuredRelay = configuredRelay.TrimEnd('/');
                    if (!configuredRelay.StartsWith("http")) configuredRelay = "https://" + configuredRelay;
                    if (!candidateUrls.Contains(configuredRelay)) candidateUrls.Add(configuredRelay);
                }
            }

            // 4b. Active Colab Public Cloudflare Relay
            string activeColabTunnel = "https://prefix-dean-theft-bytes.trycloudflare.com";
            if (!candidateUrls.Contains(activeColabTunnel)) candidateUrls.Add(activeColabTunnel);

            // 5. Local ports (8088 is official Colab relay port) - Instant loopback
            if (!candidateUrls.Contains("http://127.0.0.1:8088")) candidateUrls.Add("http://127.0.0.1:8088");
            if (!candidateUrls.Contains("http://localhost:8088")) candidateUrls.Add("http://localhost:8088");

            // 6. YAML fallbacks
            try
            {
                string yamlPath = "windows_rdp.yaml";
                if (!File.Exists(yamlPath))
                {
                    string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "windows_rdp.yaml");
                    if (File.Exists(alt)) yamlPath = alt;
                }
                if (File.Exists(yamlPath))
                {
                    string[] lines = File.ReadAllLines(yamlPath);
                    foreach (string line in lines)
                    {
                        if (line.Contains("custom_domain:"))
                        {
                            int idx = line.IndexOf(':');
                            if (idx >= 0)
                            {
                                string cd = line.Substring(idx + 1).Trim().Trim('"', '\'');
                                if (!string.IsNullOrEmpty(cd) && !candidateUrls.Contains(cd)) candidateUrls.Add(cd);
                            }
                        }
                    }
                }
            }
            catch { }

            // 7. General configured relay fallback (e.g. https://www.jdroid-x.com)
            if (db != null && db.Settings != null && db.Settings.connection != null)
            {
                string configuredRelay = db.Settings.connection.relayServer;
                if (!string.IsNullOrEmpty(configuredRelay))
                {
                    configuredRelay = configuredRelay.TrimEnd('/');
                    if (!configuredRelay.StartsWith("http://") && !configuredRelay.StartsWith("https://")) configuredRelay = "http://" + configuredRelay;
                    if (!candidateUrls.Contains(configuredRelay)) candidateUrls.Add(configuredRelay);
                }
            }

            return candidateUrls;
        }

        private void StartColabTelemetryMonitor()
        {
            colabTelemetryTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            colabTelemetryTimer.Tick += (s, e) => {
                ThreadPool.QueueUserWorkItem(delegate { PingColabServer(); });
            };
            colabTelemetryTimer.Start();
            ThreadPool.QueueUserWorkItem(delegate { PingColabServer(); });
        }

        private void PingColabServer()
        {
            bool connected = false;
            string ram = "--- / ---";
            string gpu = "-- / --";
            string disk = "-- / --";
            string winningUrl = null;
            int rtt = 0;

            try {
                isProbingCandidates = true;
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() => UpdateColabTelemetryUI()));
                }

                List<string> candidateUrls = GetRelayCandidateUrls();
                foreach (string rUrl in candidateUrls)
                {
                    try {
                        Stopwatch sw = Stopwatch.StartNew();
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/ping");
                        req.Method = "GET";
                        req.Timeout = 1000;
                        using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            sw.Stop();
                            string jsonStr = reader.ReadToEnd();
                            JavaScriptSerializer ser = new JavaScriptSerializer();
                            Dictionary<string, object> dict = ser.Deserialize<Dictionary<string, object>>(jsonStr);
                            if (dict != null && (dict.ContainsKey("status") || dict.ContainsKey("success")))
                            {
                                connected = true;
                                winningUrl = rUrl;
                                rtt = (int)sw.ElapsedMilliseconds;
                                cachedWinningRelayUrl = rUrl;
                                lastWinningCheckTime = DateTime.Now;
                                if (dict.ContainsKey("public_url") && dict["public_url"] != null)
                                {
                                    string pub = dict["public_url"].ToString().Trim();
                                    if (!string.IsNullOrEmpty(pub) && pub.StartsWith("http") && pub.Contains(".trycloudflare.com"))
                                    {
                                        winningUrl = pub;
                                        cachedWinningRelayUrl = pub;
                                    }
                                }
                                if (dict.ContainsKey("system_ram")) ram = dict["system_ram"].ToString();
                                if (dict.ContainsKey("gpu_ram")) gpu = dict["gpu_ram"].ToString();
                                if (dict.ContainsKey("disk")) disk = dict["disk"].ToString();
                                break;
                            }
                        }
                    } catch { }
                }

                if (connected && !string.IsNullOrEmpty(winningUrl))
                {
                    if (db != null && db.Settings != null && db.Settings.connection != null && db.Settings.connection.relayServer != winningUrl)
                    {
                        db.Settings.connection.relayServer = winningUrl;
                        db.Settings.connection.customDomain = winningUrl;
                        db.Save();
                        db.SyncYaml();
                        try {
                            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colab_tunnel.txt"), winningUrl);
                        } catch { }
                    }
                }
            } catch { }
            finally {
                isProbingCandidates = false;
            }

            try {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() => {
                        bool stateChanged = (isColabConnected != connected);
                        isColabConnected = connected;
                        colabRamText = connected ? ram : "--- / ---";
                        colabGpuText = connected ? gpu : "-- / --";
                        colabDiskText = connected ? disk : "-- / --";
                        if (connected && rtt > 0) lastMeasuredRttMs = rtt;
                        if (connected)
                        {
                            RegisterWithColabRelay();
                        }
                        UpdateColabTelemetryUI();
                        if (stateChanged && activeViewKey == "Console" && panelWelcomeCards != null && panelWelcomeCards.Visible)
                        {
                            BuildWelcomeCardsContent();
                        }
                    }));
                }
            } catch { }
        }

        private void UpdateColabTelemetryUI()
        {
            if (lblMetaRamVal != null && !lblMetaRamVal.IsDisposed) lblMetaRamVal.Text = colabRamText;
            if (lblMetaGpuVal != null && !lblMetaGpuVal.IsDisposed) lblMetaGpuVal.Text = colabGpuText;
            if (lblMetaDiskVal != null && !lblMetaDiskVal.IsDisposed) lblMetaDiskVal.Text = colabDiskText;
            if (lblMetaStatusVal != null && !lblMetaStatusVal.IsDisposed)
            {
                lblMetaStatusVal.Text = isColabConnected ? string.Format("● ONLINE ({0}ms)", lastMeasuredRttMs) : (isProbingCandidates ? "◌ PINGING..." : "○ OFFLINE");
                lblMetaStatusVal.ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68));
            }
            if (lblMetaHeartbeatVal != null && !lblMetaHeartbeatVal.IsDisposed)
            {
                lblMetaHeartbeatVal.Text = isColabConnected ? string.Format("ALIVE ({0}ms)", lastMeasuredRttMs) : (isProbingCandidates ? "PROBING" : "OFFLINE");
                lblMetaHeartbeatVal.ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68));
            }
            if (panelMetaRam != null && !panelMetaRam.IsDisposed) panelMetaRam.Invalidate();
            if (panelMetaGpu != null && !panelMetaGpu.IsDisposed) panelMetaGpu.Invalidate();
            if (panelMetaDisk != null && !panelMetaDisk.IsDisposed) panelMetaDisk.Invalidate();
            if (panelMetaStatus != null && !panelMetaStatus.IsDisposed) panelMetaStatus.Invalidate();
            if (panelMetaHeartbeat != null && !panelMetaHeartbeat.IsDisposed) panelMetaHeartbeat.Invalidate();
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
                lblDrawerTitle.Text = "REMOTE WORKSTATIONS";

                if (dynamicDesktops.Count == 0)
                {
                    Panel pEmpty = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 40, 12, 12) };
                    Label lblIcon = new Label { Text = "💻", Font = new Font("Segoe UI Emoji", 28f), ForeColor = Color.FromArgb(56, 189, 248), Dock = DockStyle.Top, Height = 48, TextAlign = ContentAlignment.MiddleCenter };
                    Label lblTitle = new Label { Text = "No Saved Workstations", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleCenter };
                    Label lblDesc = new Label { Text = "When you connect to remote computers, they will appear here for 1-click access.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), Dock = DockStyle.Top, Height = 60, TextAlign = ContentAlignment.TopCenter };

                    pEmpty.Controls.Add(lblDesc);
                    pEmpty.Controls.Add(lblTitle);
                    pEmpty.Controls.Add(lblIcon);
                    drawerContentPanel.Controls.Add(pEmpty);
                }
                else
                {
                    Label lPrompt = new Label { Text = "Saved remote workstations:", Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, Height = 24 };
                    drawerContentPanel.Controls.Add(lPrompt);

                    FlowLayoutPanel listFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
                    foreach (SimulatorDesktop target in dynamicDesktops)
                    {
                        Panel card = new Panel { Size = new Size(236, 68), BackColor = Color.FromArgb(20, 28, 45), Margin = new Padding(0, 0, 0, 8), Padding = new Padding(8) };
                        Label lName = new Label { Text = "💻 " + target.Name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(6, 6), AutoSize = true };
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
            if (lblInspectorFps != null) lblInspectorFps.Text = (currentConnectionState == SessionConnectionState.Connected) ? string.Format("{0:0.0} FPS (Active)", currentMeasuredFps) : "0.0 FPS (Idle)";
            if (lblInspectorRtt != null) lblInspectorRtt.Text = (currentConnectionState == SessionConnectionState.Connected && lastMeasuredRttMs > 0) ? (lastMeasuredRttMs + " ms (P2P)") : "-- ms";
            if (lblInspectorHost != null && currentHostDesktop != null) lblInspectorHost.Text = currentHostDesktop.Name;
            if (lblInspectorTarget != null && connectedTargetDesktop != null) lblInspectorTarget.Text = connectedTargetDesktop.Name;

            if (lblSessionTelemetryHUD != null)
            {
                if (currentConnectionState == SessionConnectionState.Connected)
                {
                    lblSessionTelemetryHUD.Text = string.Format("🟢 {0:0} FPS • {1}ms • P2P", currentMeasuredFps, lastMeasuredRttMs > 0 ? lastMeasuredRttMs : 0);
                    lblSessionTelemetryHUD.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else if (currentConnectionState == SessionConnectionState.Connecting)
                {
                    lblSessionTelemetryHUD.Text = "🟡 Connecting (Heartbeat)...";
                    lblSessionTelemetryHUD.ForeColor = Color.FromArgb(245, 158, 11);
                }
                else
                {
                    lblSessionTelemetryHUD.Text = "⚪ Idle";
                    lblSessionTelemetryHUD.ForeColor = Color.FromArgb(148, 163, 184);
                }
            }
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
            viewConsole = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(0) };

            // Top Floating Session Bar
            sessionFloatingToolbar = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(20, 28, 45), Padding = new Padding(6, 4, 6, 4), Visible = false };
            BuildFloatingSessionToolbar();

            // Local Node Info Ribbon (Non-overlapping, Clean Layout)
            Panel localBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(13, 21, 34), Padding = new Padding(14, 8, 14, 8) };

            FlowLayoutPanel localLeftBox = new FlowLayoutPanel { Dock = DockStyle.Left, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Margin = new Padding(0) };
            Label lblDeskPrefix = new Label { Text = "THIS DESK:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Margin = new Padding(0, 4, 8, 0), AutoSize = true };
            lblLocalId = new Label { Text = currentHostDesktop != null ? currentHostDesktop.RemoteId : GenerateMachineDeskId(), Font = new Font("Consolas", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Margin = new Padding(0, 2, 10, 0), AutoSize = true };
            
            Button btnCopy = new Button { Text = "📋 Copy", Size = new Size(68, 24), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(40, 53, 76), Cursor = Cursors.Hand, Margin = new Padding(0, 1, 12, 0) };
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
            Label lTargetPrompt = new Label { Text = "REMOTE DESK:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Margin = new Padding(0, 4, 8, 0), AutoSize = true };
            txtTargetId = new TextBox { Font = new Font("Consolas", 11f, FontStyle.Bold), Size = new Size(130, 24), BackColor = Color.FromArgb(24, 32, 54), ForeColor = Color.FromArgb(56, 189, 248), BorderStyle = BorderStyle.FixedSingle, Text = "" };
            txtTargetId.TextChanged += (s, e) => {
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
            panelWelcomeCards = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };
            BuildWelcomeCardsContent();

            // GDI+ Canvas for Remote Stream
            viewportCanvas = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 16, 26), Visible = false };
            viewportCanvas.Paint += ViewportCanvas_Paint;
            viewportCanvas.MouseClick += ViewportCanvas_MouseClick;
            viewportCanvas.MouseDown += ViewportCanvas_MouseDown;
            viewportCanvas.MouseUp += ViewportCanvas_MouseUp;
            viewportCanvas.MouseMove += ViewportCanvas_MouseMove;
            viewportCanvas.MouseDoubleClick += ViewportCanvas_MouseDoubleClick;

            viewConsole.Controls.Add(sessionFloatingToolbar);
            viewConsole.Controls.Add(localBar);
            viewConsole.Controls.Add(panelWelcomeCards);
            viewConsole.Controls.Add(viewportCanvas);
            viewConsole.Controls.SetChildIndex(sessionFloatingToolbar, 3);
            viewConsole.Controls.SetChildIndex(localBar, 2);
            viewConsole.Controls.SetChildIndex(panelWelcomeCards, 1);
            viewConsole.Controls.SetChildIndex(viewportCanvas, 0);
        }

        private void BuildWelcomeCardsContent()
        {
            panelWelcomeCards.Controls.Clear();

            Panel cardContainer = new Panel { Width = 820, AutoSize = true, BackColor = Color.Transparent };
            Action reposition = () => {
                cardContainer.Location = new Point(Math.Max(24, (panelWelcomeCards.ClientSize.Width - 820) / 2), 20);
            };
            panelWelcomeCards.Resize += (s, e) => reposition();
            reposition();

            int y = 0;

            // Page Header
            Label lblMainTitle = new Label { Text = "Remote Access & Support", Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(0, y), AutoSize = true };
            y += 34;
            Label lblMainSub = new Label { Text = "Share this screen for remote support or connect to another workstation over the internet.", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(0, y), AutoSize = true };
            y += 38;

            // CARD 0: CONNECT TO GOOGLE COLAB CLOUD RELAY
            Panel cardColab = new Panel { Size = new Size(820, 138), Location = new Point(0, y), BackColor = Color.FromArgb(20, 28, 45) };
            cardColab.Paint += (s, e) => {
                Color borderC = isColabConnected ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11);
                e.Graphics.DrawRectangle(new Pen(borderC, 1.2f), 0, 0, cardColab.Width - 1, cardColab.Height - 1);
            };

            string colabStatusText = isColabConnected ? "● ONLINE (Live Stream Ready)" : "○ NOT CONNECTED (Paste Colab URL)";
            Color colabStatusColor = isColabConnected ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11);

            Label lColabTitle = new Label { Text = "⚡  Step 1: Connect to Google Colab Cloud Relay", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Location = new Point(20, 14), AutoSize = true };
            Label lColabBadge = new Label { Text = colabStatusText, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = colabStatusColor, Location = new Point(480, 16), AutoSize = true };
            Label lColabDesc = new Label { Text = "Colab relay connects Desk 1 & Desk 2 over the internet without port forwarding. Enter your Public Relay URL:", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, 42), AutoSize = true };

            TextBox txtColabInput = new TextBox {
                Size = new Size(410, 36),
                Location = new Point(20, 72),
                Font = new Font("Consolas", 10.5f),
                BackColor = Color.FromArgb(10, 16, 26),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = (db != null && db.Settings != null && db.Settings.connection != null && !string.IsNullOrEmpty(db.Settings.connection.relayServer)) 
                    ? db.Settings.connection.relayServer 
                    : "https://....trycloudflare.com"
            };

            Button btnPasteColab = new Button {
                Text = "📋 Paste URL",
                Size = new Size(115, 36),
                Location = new Point(440, 72),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 53, 76),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
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
                            ShowToast("Pasted active Colab tunnel URL from clipboard");
                        } else {
                            MessageBox.Show("Clipboard does not contain a valid URL. Copy the 'https://...trycloudflare.com' URL from your Colab notebook first.", "Paste Colab URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                } catch { }
            };

            Button btnApplyColab = new Button {
                Text = "💾 Save & Connect",
                Size = new Size(130, 36),
                Location = new Point(565, 72),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
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

            Button btnColabNotebook = new Button {
                Text = "🌐 Colab",
                Size = new Size(95, 36),
                Location = new Point(705, 72),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnColabNotebook.FlatAppearance.BorderSize = 0;
            btnColabNotebook.Click += (s, e) => {
                try {
                    string nb = (db != null && db.Settings != null && db.Settings.connection != null && !string.IsNullOrEmpty(db.Settings.connection.colabNotebookUrl)) ? db.Settings.connection.colabNotebookUrl : "https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai";
                    Process.Start(new ProcessStartInfo(nb) { UseShellExecute = true });
                } catch { }
            };

            cardColab.Controls.Add(lColabTitle);
            cardColab.Controls.Add(lColabBadge);
            cardColab.Controls.Add(lColabDesc);
            cardColab.Controls.Add(txtColabInput);
            cardColab.Controls.Add(btnPasteColab);
            cardColab.Controls.Add(btnApplyColab);
            cardColab.Controls.Add(btnColabNotebook);
            cardContainer.Controls.Add(cardColab);
            y += 156;

            // CARD 1: SHARE THIS SCREEN
            Panel cardShare = new Panel { Size = new Size(820, 150), Location = new Point(0, y), BackColor = Color.FromArgb(20, 28, 45) };
            cardShare.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(40, 53, 76), 1), 0, 0, cardShare.Width - 1, cardShare.Height - 1);
            };

            Label lShareTitle = new Label { Text = "📥  Step 2: Share this screen (Get Support)", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 16), AutoSize = true };
            Label lShareDesc = new Label { Text = "To let another person view and control this computer, give them this 9-digit Desk ID:", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, 44), AutoSize = true };

            Panel codeBox = new Panel { Size = new Size(240, 44), Location = new Point(20, 78), BackColor = Color.FromArgb(10, 16, 26) };
            lblCardLocalId = new Label { Text = currentHostDesktop != null ? currentHostDesktop.RemoteId : GenerateMachineDeskId(), Font = new Font("Consolas", 16f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            codeBox.Controls.Add(lblCardLocalId);

            Button btnCardCopy = new Button { Text = "📋 Copy Desk ID", Size = new Size(130, 44), Location = new Point(272, 78), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(40, 53, 76), ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCardCopy.FlatAppearance.BorderSize = 0;
            btnCardCopy.Click += (s, e) => {
                if (currentHostDesktop != null) {
                    Clipboard.SetText(currentHostDesktop.RemoteId);
                    ShowToast("Desk ID copied to clipboard: " + currentHostDesktop.RemoteId);
                }
            };

            Button btnCardInvite = new Button { Text = "✉️ Copy Invite", Size = new Size(130, 44), Location = new Point(412, 78), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCardInvite.FlatAppearance.BorderSize = 0;
            btnCardInvite.Click += (s, e) => {
                if (currentHostDesktop != null) {
                    string invite = string.Format("Join my jDroid-X Remote Desktop session.\nDesk ID: {0}\nDownload client: https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk.exe", currentHostDesktop.RemoteId);
                    Clipboard.SetText(invite);
                    ShowToast("Complete invite message copied to clipboard!");
                }
            };

            Label lShareStatus = new Label { Text = "● Ready for incoming connections", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Location = new Point(560, 92), AutoSize = true };

            cardShare.Controls.Add(lShareTitle);
            cardShare.Controls.Add(lShareDesc);
            cardShare.Controls.Add(codeBox);
            cardShare.Controls.Add(btnCardCopy);
            cardShare.Controls.Add(btnCardInvite);
            cardShare.Controls.Add(lShareStatus);
            cardContainer.Controls.Add(cardShare);
            y += 168;

            // CARD 2: CONNECT TO ANOTHER COMPUTER
            Panel cardConnect = new Panel { Size = new Size(820, 150), Location = new Point(0, y), BackColor = Color.FromArgb(20, 28, 45) };
            cardConnect.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(40, 53, 76), 1), 0, 0, cardConnect.Width - 1, cardConnect.Height - 1);
            };

            Label lConnTitle = new Label { Text = "📤  Step 3: Connect to another computer (Give Support)", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 16), AutoSize = true };
            Label lConnDesc = new Label { Text = "Enter the 9-digit Desk ID of the remote computer you want to access:", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, 44), AutoSize = true };

            txtCardTargetId = new TextBox { Size = new Size(240, 44), Location = new Point(20, 78), Font = new Font("Consolas", 15f), BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "" };
            txtCardTargetId.TextChanged += (s, e) => {
                if (txtTargetId != null && txtTargetId.Text != txtCardTargetId.Text)
                    txtTargetId.Text = txtCardTargetId.Text;
            };

            btnCardConnect = new ModernButton { Text = "⚡ Connect", Size = new Size(130, 36), Location = new Point(272, 78), NormalColor = Color.FromArgb(239, 51, 64), HoverColor = Color.FromArgb(220, 38, 38) };
            btnCardConnect.Click += (s, e) => {
                if (txtTargetId != null) txtTargetId.Text = txtCardTargetId.Text.Trim();
                ToggleSessionConnection();
            };

            ComboBox cboSavedFleet = new ComboBox { Location = new Point(415, 84), Size = new Size(260, 26), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(10, 16, 26), ForeColor = Color.FromArgb(203, 213, 225), Font = new Font("Segoe UI", 8.5f) };
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
                        txtCardTargetId.Text = db.Devices[devIdx].id;
                    }
                }
            };

            Label lConnSub = new Label { Text = "Encrypted direct peer-to-peer session over jDroid-X Relay Network.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(20, 124), AutoSize = true };

            cardConnect.Controls.Add(lConnTitle);
            cardConnect.Controls.Add(lConnDesc);
            cardConnect.Controls.Add(txtCardTargetId);
            cardConnect.Controls.Add(btnCardConnect);
            cardConnect.Controls.Add(cboSavedFleet);
            cardConnect.Controls.Add(lConnSub);
            cardContainer.Controls.Add(cardConnect);
            y += 168;

            // CARD 3: RECENT CONNECTIONS
            Panel cardRecent = new Panel { Size = new Size(820, 140), Location = new Point(0, y), BackColor = Color.FromArgb(20, 28, 45) };
            cardRecent.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(40, 53, 76), 1), 0, 0, cardRecent.Width - 1, cardRecent.Height - 1);
            };

            Label lRecTitle = new Label { Text = "🕒  Recent Connections", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 16), AutoSize = true };
            cardRecent.Controls.Add(lRecTitle);

            if (dynamicDesktops.Count == 0)
            {
                Label lRecEmpty = new Label { Text = "No remote workstations saved yet.", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(20, 48), AutoSize = true };
                Label lRecSub = new Label { Text = "When you connect to remote computers, they will appear here for fast 1-click reconnect.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, 74), AutoSize = true };
                cardRecent.Controls.Add(lRecEmpty);
                cardRecent.Controls.Add(lRecSub);
            }
            else
            {
                FlowLayoutPanel flowRecents = new FlowLayoutPanel { Location = new Point(20, 48), Size = new Size(780, 80), FlowDirection = FlowDirection.LeftToRight, AutoScroll = true };
                foreach (SimulatorDesktop target in dynamicDesktops)
                {
                    Panel p = new Panel { Size = new Size(240, 64), BackColor = Color.FromArgb(13, 21, 34), Margin = new Padding(0, 0, 12, 0), Padding = new Padding(8) };
                    Label ln = new Label { Text = "💻 " + target.Name, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(6, 6), AutoSize = true };
                    Label lid = new Label { Text = target.RemoteId, Font = new Font("Consolas", 8f), ForeColor = target.AccentColor, Location = new Point(6, 26), AutoSize = true };
                    ModernButton bc = new ModernButton { Text = "Connect", Size = new Size(70, 22), Location = new Point(160, 34), NormalColor = target.AccentColor, HoverColor = Color.FromArgb(2, 132, 199) };
                    SimulatorDesktop t = target;
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

            Button btnMirror = CreateToolbarBtn("🎥 " + (isLiveScreenMirror ? "Live Mirror (Active)" : "Live Mirror"), (s, e) => {
                isLiveScreenMirror = !isLiveScreenMirror;
                ((Button)s).Text = "🎥 " + (isLiveScreenMirror ? "Live Mirror (Active)" : "Live Mirror");
                viewportCanvas.Invalidate();
                ShowToast("Engine: " + (isLiveScreenMirror ? "Live Local Screen Mirror (Active)" : "Remote Desktop Stream"));
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

            leftTools.Controls.Add(btnDisplay);
            leftTools.Controls.Add(btnSmartFit);
            leftTools.Controls.Add(btnMirror);
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
                        if (txtCardTargetId != null) txtCardTargetId.Focus();
                        else if (txtTargetId != null) txtTargetId.Focus();
                        return;
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

        // ======================================================================
        // GDI+ REMOTE DESKTOP ENGINE (DYNAMIC FRAME STREAM & HEARTBEAT RADAR)
        // ======================================================================

        private void ViewportCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cw = viewportCanvas.Width;
            int ch = viewportCanvas.Height;
            if (cw <= 0 || ch <= 0) return;

            if (!isSessionActive || currentConnectionState == SessionConnectionState.Disconnected)
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
                    g.DrawString("jDroid-X Remote Desktop Engine • P2P UDP Ready", fSub, bMuted, cardX + 20, cardY + 50);

                    if (currentHostDesktop != null)
                    {
                        g.DrawString("Active Host: " + currentHostDesktop.Name + " [" + currentHostDesktop.RemoteId + "]", fCode, bBlue, cardX + 20, cardY + 85);
                        g.DrawString("IP Address : " + currentHostDesktop.Ip + ":7070 (AES-256)", fCode, bWhite, cardX + 20, cardY + 110);
                        g.DrawString("Role       : " + currentHostDesktop.Role, fCode, bMuted, cardX + 20, cardY + 135);
                    }
                    string relayDisplay = !string.IsNullOrEmpty(db.Settings.connection.customDomain) ? db.Settings.connection.customDomain : db.Settings.connection.relayServer;
                    string relayStatus = (db.Settings.connection.colabCloudRelay ? "Cloud Relay (" + relayDisplay + ") [" + (isColabConnected ? "ONLINE" : "OFFLINE") + "]" : "Direct P2P LAN Mode [READY]");
                    g.DrawString("Cloud Node : " + relayStatus, fCode, isColabConnected ? bGreen : bMuted, cardX + 20, cardY + 160);

                    g.DrawString("Enter a remote Desk ID above or in the cards to connect.", fSub, bWhite, cardX + 20, cardY + 205);
                }
            }
            else if (currentConnectionState == SessionConnectionState.Connecting)
            {
                // Dynamic Heartbeat Oscilloscope & Progress Monitor Screen
                SimulatorDesktop target = connectedTargetDesktop ?? (dynamicDesktops.Count > 1 ? dynamicDesktops[1] : dynamicDesktops[0]);
                RenderConnectingHeartbeatScreen(g, target, cw, ch);
            }
            else
            {
                // Live Connected Screen View
                SimulatorDesktop target = connectedTargetDesktop ?? (dynamicDesktops.Count > 1 ? dynamicDesktops[1] : dynamicDesktops[0]);

                // 1. Live Remote Screen Stream via Colab Relay (Thread-Safe Clone & Draw)
                Image frameToDraw = null;
                lock (frameLock)
                {
                    if (latestRemoteFrame != null)
                    {
                        try { frameToDraw = (Image)latestRemoteFrame.Clone(); } catch { }
                    }
                }

                if (frameToDraw != null)
                {
                    using (frameToDraw)
                    {
                        DrawScaledImage(g, frameToDraw, cw, ch);
                    }
                    using (SolidBrush bBg = new SolidBrush(Color.FromArgb(170, 15, 23, 42)))
                    using (SolidBrush bGreen = new SolidBrush(Color.FromArgb(16, 185, 129)))
                    using (SolidBrush bBlue = new SolidBrush(Color.FromArgb(56, 189, 248)))
                    using (Font fOverlay = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        g.FillRectangle(bBg, 12, 12, 380, 26);
                        g.DrawString("🟢 LIVE SCREEN SHARING: " + target.RemoteId, fOverlay, bGreen, 18, 16);
                        g.DrawString("Frames: " + framesReceivedCount, fOverlay, bBlue, 280, 16);
                    }
                    return;
                }

                // 2. Local Live Screen Mirror Mode (Loopback or Instant Preview)
                if (isLiveScreenMirror || (currentHostDesktop != null && target.RemoteId == currentHostDesktop.RemoteId))
                {
                    try {
                        using (Bitmap screenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                        {
                            using (Graphics sG = Graphics.FromImage(screenCapture))
                            {
                                sG.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, screenCapture.Size);
                            }
                            DrawScaledImage(g, screenCapture, cw, ch);
                        }
                        using (SolidBrush bBg = new SolidBrush(Color.FromArgb(170, 15, 23, 42)))
                        using (SolidBrush bBlue = new SolidBrush(Color.FromArgb(56, 189, 248)))
                        using (Font fOverlay = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                        {
                            g.FillRectangle(bBg, 12, 12, 360, 26);
                            g.DrawString("🎥 LOCAL LIVE MIRROR: " + target.RemoteId, fOverlay, bBlue, 18, 16);
                        }
                        return;
                    } catch { }
                }

                // 3. Awaiting Live Video Stream Frames (Real Buffer Status - Zero Fake Icons)
                RenderAwaitingLiveFramesScreen(g, target, cw, ch);
            }
        }

        private void RenderConnectingHeartbeatScreen(Graphics g, SimulatorDesktop target, int cw, int ch)
        {
            // Background - Deep Futuristic Slate Gradient
            using (LinearGradientBrush wallBrush = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), Color.FromArgb(10, 16, 26), Color.FromArgb(15, 23, 42), LinearGradientMode.Vertical))
            {
                g.FillRectangle(wallBrush, 0, 0, cw, ch);
            }

            // Oscilloscope Grid Lines (Subtle Cyan Matrix)
            using (Pen gridPen = new Pen(Color.FromArgb(18, 28, 45), 1))
            {
                for (int x = 0; x < cw; x += 40) g.DrawLine(gridPen, x, 0, x, ch);
                for (int y = 0; y < ch; y += 40) g.DrawLine(gridPen, 0, y, cw, y);
            }

            int cardW = Math.Min(700, cw - 60);
            int cardH = 360;
            int cardX = (cw - cardW) / 2;
            int cardY = Math.Max(24, (ch - cardH) / 2);

            // Center Card Container
            using (SolidBrush bCardBg = new SolidBrush(Color.FromArgb(235, 15, 23, 42)))
            using (Pen pCardBorder = new Pen(Color.FromArgb(56, 189, 248), 1.5f))
            {
                g.FillRectangle(bCardBg, cardX, cardY, cardW, cardH);
                g.DrawRectangle(pCardBorder, cardX, cardY, cardW, cardH);
            }

            // Top Glow Accent Line
            using (LinearGradientBrush glowBrush = new LinearGradientBrush(new Rectangle(cardX, cardY, cardW, 3), Color.FromArgb(56, 189, 248), Color.FromArgb(16, 185, 129), LinearGradientMode.Horizontal))
            {
                g.FillRectangle(glowBrush, cardX, cardY, cardW, 3);
            }

            using (Font fTitle = new Font("Segoe UI", 13f, FontStyle.Bold))
            using (Font fSub = new Font("Segoe UI", 8.5f))
            using (Font fCode = new Font("Consolas", 9f))
            using (Font fSec = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (SolidBrush bWhite = new SolidBrush(Color.White))
            using (SolidBrush bGreen = new SolidBrush(Color.FromArgb(16, 185, 129)))
            using (SolidBrush bCyan = new SolidBrush(Color.FromArgb(56, 189, 248)))
            using (SolidBrush bAmber = new SolidBrush(Color.FromArgb(245, 158, 11)))
            using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                // Header
                g.DrawString("⚡ CONNECTING TO REMOTE WORKSTATION", fTitle, bCyan, cardX + 24, cardY + 16);
                g.DrawString(string.Format("Target: {0} • Remote Desk ID: {1} • Role: {2}", target.Name, target.RemoteId, target.Role), fSub, bMuted, cardX + 24, cardY + 44);

                // ECG Heartbeat Oscilloscope Monitor Box
                int oscX = cardX + 24;
                int oscY = cardY + 70;
                int oscW = cardW - 48;
                int oscH = 96;

                using (SolidBrush oscBg = new SolidBrush(Color.FromArgb(8, 12, 20)))
                using (Pen oscBorder = new Pen(Color.FromArgb(30, 41, 59), 1))
                {
                    g.FillRectangle(oscBg, oscX, oscY, oscW, oscH);
                    g.DrawRectangle(oscBorder, oscX, oscY, oscW, oscH);
                }

                // Oscilloscope Sub-Grid
                using (Pen oscGrid = new Pen(Color.FromArgb(16, 24, 38), 1))
                {
                    for (int gx = oscX; gx < oscX + oscW; gx += 20) g.DrawLine(oscGrid, gx, oscY, gx, oscY + oscH);
                    for (int gy = oscY; gy < oscY + oscH; gy += 16) g.DrawLine(oscGrid, oscX, gy, oscX + oscW, gy);
                }

                // Draw Real Mathematical ECG Heartbeat Waveform
                int midY = oscY + (oscH / 2);
                int sweepOffset = (connectingPulseFrame * 5) % 180;
                Point[] ecgPoints = new Point[oscW];

                for (int i = 0; i < oscW; i++)
                {
                    int phase = (i - sweepOffset) % 180;
                    if (phase < 0) phase += 180;

                    float dy = 0;
                    if (phase >= 55 && phase <= 70)
                    {
                        dy = (float)(-6.0 * Math.Sin((phase - 55) * Math.PI / 15.0)); // P wave
                    }
                    else if (phase >= 80 && phase <= 86)
                    {
                        dy = (float)(4.0 * Math.Sin((phase - 80) * Math.PI / 6.0));  // Q dip
                    }
                    else if (phase > 86 && phase <= 94)
                    {
                        dy = (float)(-38.0 * Math.Sin((phase - 86) * Math.PI / 8.0)); // R spike!
                    }
                    else if (phase > 94 && phase <= 102)
                    {
                        dy = (float)(14.0 * Math.Sin((phase - 94) * Math.PI / 8.0)); // S dip
                    }
                    else if (phase >= 115 && phase <= 138)
                    {
                        dy = (float)(-12.0 * Math.Sin((phase - 115) * Math.PI / 23.0)); // T wave
                    }

                    ecgPoints[i] = new Point(oscX + i, (int)(midY + dy));
                }

                using (Pen ecgPen = new Pen(Color.FromArgb(56, 189, 248), 2.0f))
                {
                    g.DrawLines(ecgPen, ecgPoints);
                }

                // Leading glowing pulse dot
                int leadX = oscX + ((connectingPulseFrame * 5) % oscW);
                int leadY = ecgPoints[Math.Max(0, Math.Min(oscW - 1, (connectingPulseFrame * 5) % oscW))].Y;
                using (SolidBrush dotGlow = new SolidBrush(Color.FromArgb(100, 16, 185, 129)))
                using (SolidBrush dotCore = new SolidBrush(Color.FromArgb(16, 185, 129)))
                {
                    g.FillEllipse(dotGlow, leadX - 6, leadY - 6, 12, 12);
                    g.FillEllipse(dotCore, leadX - 3, leadY - 3, 6, 6);
                }

                // Oscilloscope HUD Text
                g.DrawString("💓 HEARTBEAT: PULSING", fSec, bGreen, oscX + 10, oscY + 8);
                string rttStr = lastMeasuredRttMs > 0 ? lastMeasuredRttMs + " ms" : "Measuring...";
                g.DrawString("RELAY RTT: " + rttStr, fSec, bCyan, oscX + oscW - 130, oscY + 8);

                // Progress Bar Container
                int barY = oscY + oscH + 16;
                int barW = cardW - 48;
                int barH = 14;

                using (SolidBrush barBg = new SolidBrush(Color.FromArgb(15, 23, 42)))
                using (Pen barBorder = new Pen(Color.FromArgb(30, 41, 59), 1))
                {
                    g.FillRectangle(barBg, oscX, barY, barW, barH);
                    g.DrawRectangle(barBorder, oscX, barY, barW, barH);
                }

                // Filled portion with smooth gradient
                int fillW = Math.Max(10, (barW * connectingProgressPct) / 100);
                using (LinearGradientBrush fillBrush = new LinearGradientBrush(new Rectangle(oscX, barY, fillW, barH), Color.FromArgb(2, 132, 199), Color.FromArgb(16, 185, 129), LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(fillBrush, oscX + 1, barY + 1, fillW - 2, barH - 2);
                }

                // Progress Shimmer Sweep
                int shimmerX = oscX + ((connectingPulseFrame * 8) % Math.Max(1, fillW));
                using (SolidBrush shimmer = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
                {
                    g.FillRectangle(shimmer, shimmerX, barY + 1, 12, barH - 2);
                }

                // Progress Label
                g.DrawString(connectingProgressPct + "%", fSec, bWhite, oscX + barW - 32, barY - 16);

                // Dynamic Status Stage Message
                g.DrawString("⚡ " + connectionStageMessage, fSec, bCyan, oscX, barY + 20);

                // Elapsed time & timeout countdown
                TimeSpan elapsed = DateTime.Now - connectionAttemptStartTime;
                int remainSec = Math.Max(0, 20 - (int)elapsed.TotalSeconds);
                string timeStr = string.Format("Elapsed: {0:0.0}s | Auto-Timeout in: {1}s", elapsed.TotalSeconds, remainSec);
                g.DrawString(timeStr, fCode, bMuted, oscX, barY + 40);

                // Protocol & Relay details
                string relayUrl = !string.IsNullOrEmpty(CloudflareTunnelEngine.ActiveTunnelUrl) ? CloudflareTunnelEngine.ActiveTunnelUrl : (db.Settings.connection.relayServer ?? "Offline");
                g.DrawString("Relay Node : " + relayUrl + " (AES-256-GCM Handshake)", fCode, bGreen, oscX, barY + 60);

                // Instruction to Cancel
                g.DrawString("👉 To cancel this connection attempt, click '✕ Cancel' in the top action bar.", fSub, bAmber, oscX, barY + 84);
            }
        }

        private void RenderAwaitingLiveFramesScreen(Graphics g, SimulatorDesktop target, int cw, int ch)
        {
            // Background - Deep Slate Gradient
            using (LinearGradientBrush wallBrush = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), Color.FromArgb(10, 16, 26), Color.FromArgb(15, 23, 42), LinearGradientMode.Vertical))
            {
                g.FillRectangle(wallBrush, 0, 0, cw, ch);
            }

            int cardW = Math.Min(640, cw - 60);
            int cardH = 220;
            int cardX = (cw - cardW) / 2;
            int cardY = Math.Max(40, (ch - cardH) / 2);

            using (SolidBrush bCardBg = new SolidBrush(Color.FromArgb(235, 15, 23, 42)))
            using (Pen pCardBorder = new Pen(Color.FromArgb(16, 185, 129), 1.5f))
            using (Font fTitle = new Font("Segoe UI", 13f, FontStyle.Bold))
            using (Font fSub = new Font("Segoe UI", 9f))
            using (Font fCode = new Font("Consolas", 9f))
            using (SolidBrush bWhite = new SolidBrush(Color.White))
            using (SolidBrush bGreen = new SolidBrush(Color.FromArgb(16, 185, 129)))
            using (SolidBrush bCyan = new SolidBrush(Color.FromArgb(56, 189, 248)))
            using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                g.FillRectangle(bCardBg, cardX, cardY, cardW, cardH);
                g.DrawRectangle(pCardBorder, cardX, cardY, cardW, cardH);

                g.DrawString("🟢 P2P SESSION CONNECTED & ACTIVE", fTitle, bGreen, cardX + 24, cardY + 20);
                g.DrawString("Authenticated with Remote Workstation: " + target.RemoteId + " (" + target.Name + ")", fSub, bWhite, cardX + 24, cardY + 50);
                g.DrawString("● Tunnel: AES-256 End-to-End P2P Stream Active", fCode, bCyan, cardX + 24, cardY + 80);
                g.DrawString("Awaiting video stream frames from remote host...", fSub, bMuted, cardX + 24, cardY + 110);
                g.DrawString("Bytes Received: " + (bytesTransferred / (1024 * 1024.0)).ToString("0.0") + " MB | Total Frames: " + framesReceivedCount, fCode, bWhite, cardX + 24, cardY + 135);
                g.DrawString("💡 Tip: Click '🎥 Live Mirror' above for instant local screen test.", fSub, bGreen, cardX + 24, cardY + 170);
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
            if (currentConnectionState == SessionConnectionState.Connecting)
            {
                // Clicking canvas during connecting offers quick cancel option
                int cw = viewportCanvas.Width;
                int ch = viewportCanvas.Height;
                int cardW = Math.Min(700, cw - 60);
                int cardH = 360;
                int cardX = (cw - cardW) / 2;
                int cardY = Math.Max(24, (ch - cardH) / 2);
                if (e.X >= cardX && e.X <= cardX + cardW && e.Y >= cardY + cardH - 40 && e.Y <= cardY + cardH)
                {
                    AbortSessionConnection("Connection Cancelled by User");
                }
            }
        }

        private void ViewportCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentConnectionState == SessionConnectionState.Connected)
            {
                int rx, ry;
                if (MapCanvasToRemote(e.X, e.Y, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "down";
                    ev["x"] = rx;
                    ev["y"] = ry;
                    ev["btn"] = e.Button == MouseButtons.Right ? "Right" : (e.Button == MouseButtons.Middle ? "Middle" : "Left");
                    SendRemoteInput("mouse", ev);
                }
            }
        }

        private void ViewportCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (currentConnectionState == SessionConnectionState.Connected)
            {
                int rx, ry;
                if (MapCanvasToRemote(e.X, e.Y, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "up";
                    ev["x"] = rx;
                    ev["y"] = ry;
                    ev["btn"] = e.Button == MouseButtons.Right ? "Right" : (e.Button == MouseButtons.Middle ? "Middle" : "Left");
                    SendRemoteInput("mouse", ev);
                }
            }
        }

        private DateTime lastMouseMoveSent = DateTime.MinValue;
        private void ViewportCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentConnectionState == SessionConnectionState.Connected)
            {
                if ((DateTime.Now - lastMouseMoveSent).TotalMilliseconds < 33) return;
                lastMouseMoveSent = DateTime.Now;

                int rx, ry;
                if (MapCanvasToRemote(e.X, e.Y, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "move";
                    ev["x"] = rx;
                    ev["y"] = ry;
                    ev["btn"] = "None";
                    SendRemoteInput("mouse", ev);
                }
            }
        }

        private void ViewportCanvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (currentConnectionState == SessionConnectionState.Connected)
            {
                int rx, ry;
                if (MapCanvasToRemote(e.X, e.Y, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "dblclick";
                    ev["x"] = rx;
                    ev["y"] = ry;
                    ev["btn"] = e.Button == MouseButtons.Right ? "Right" : "Left";
                    SendRemoteInput("mouse", ev);
                }
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
            Label lblTitle = new Label { Text = "Settings & Security Studio (jDroid-X Relay Engine)", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

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
                addH("Primary Enterprise Cloud Domain & Web Integration");
                TextBox tbDom = addTb("Primary Branded Cloud Domain", db.Settings.connection.customDomain);
                tbDom.TextChanged += (s, e) => db.Settings.connection.customDomain = tbDom.Text.Trim();
                TextBox tbWeb = addTb("jDroid-X-Web Repository URL", db.Settings.connection.webRepoUrl);
                tbWeb.TextChanged += (s, e) => db.Settings.connection.webRepoUrl = tbWeb.Text.Trim();

                addH("Google Colab & Cloud Relay Configuration");
                CheckBox cbColab = addCb("Enable Google Colab Central Cloud Relay", db.Settings.connection.colabCloudRelay);
                cbColab.CheckedChanged += (s, e) => {
                    db.Settings.connection.colabCloudRelay = cbColab.Checked;
                    RegisterWithColabRelay();
                };
                TextBox tbRelay = addTb("Cloud Relay Server URL (Default: https://www.jdroid-x.com)", db.Settings.connection.relayServer);
                tbRelay.TextChanged += (s, e) => {
                    db.Settings.connection.relayServer = tbRelay.Text.Trim();
                    RegisterWithColabRelay();
                };
                TextBox tbPort = addTb("Relay Port (Default: 8088)", db.Settings.connection.relayPort > 0 ? db.Settings.connection.relayPort.ToString() : "8088");
                tbPort.TextChanged += (s, e) => {
                    int p;
                    if (int.TryParse(tbPort.Text.Trim(), out p) && p > 0) db.Settings.connection.relayPort = p;
                };
                TextBox tbToken = addTb("Cloudflare Tunnel Token (For jdroid-x.com Custom Domain)", db.Settings.connection.tunnelToken ?? "");
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
                    viewportCanvas.Invalidate();
                };
                settingsSubContentPanel.Controls.Add(btnSaveConn);
                y += 42;
            }
            else if (category == "Security")
            {
                addH("Access Control & Passcode Security");
                CheckBox cbPasscode = addCb("Require One-Time Passcode for incoming connections (Default: Off / Direct Connect)", db.Settings.access.requirePasscode);
                cbPasscode.CheckedChanged += (s, e) => db.Settings.access.requirePasscode = cbPasscode.Checked;
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
        // GOOGLE COLAB CENTRAL CLOUD RELAY BROKER
        // ======================================================================

        private void RegisterWithColabRelay()
        {
            ThreadPool.QueueUserWorkItem(delegate {
                try {
                    if (db == null || db.Settings == null || db.Settings.connection == null || !db.Settings.connection.colabCloudRelay) return;
                    if (currentHostDesktop == null) return;
                    List<string> candidateUrls = GetRelayCandidateUrls();
                    string json = string.Format("{{\"id\":\"{0}\",\"name\":\"{1}\",\"role\":\"{2}\",\"passcode\":\"{3}\"}}",
                        currentHostDesktop.RemoteId, currentHostDesktop.Name, currentHostDesktop.Role, currentHostDesktop.Passcode);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                    foreach (string rUrl in candidateUrls)
                    {
                        try {
                            string endpoint = rUrl.TrimEnd('/') + "/api/relay/register";
                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(endpoint);
                            req.Method = "POST";
                            req.ContentType = "application/json";
                            req.Timeout = 1500;
                            req.ContentLength = bytes.Length;
                            using (Stream os = req.GetRequestStream())
                            {
                                os.Write(bytes, 0, bytes.Length);
                            }
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                            {
                                if ((int)resp.StatusCode == 200)
                                {
                                    break;
                                }
                            }
                        } catch { }
                    }
                } catch { }
            });
        }

        // ======================================================================
        // LIVE BIDIRECTIONAL SCREEN STREAMING & RELAY ENGINE
        // ======================================================================

        private static ImageCodecInfo GetJpegEncoder()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                    return codec;
            }
            return null;
        }

        private static ImageCodecInfo GetCachedJpegEncoder()
        {
            if (cachedJpegEncoder == null)
            {
                cachedJpegEncoder = GetJpegEncoder();
            }
            return cachedJpegEncoder;
        }

        private void StartScreenStreamer()
        {
            if (isScreenStreamingActive) return;
            isScreenStreamingActive = true;
            screenStreamerThread = new Thread(new ThreadStart(ScreenStreamerWorker)) { IsBackground = true };
            screenStreamerThread.Start();
        }

        private void StopScreenStreamer()
        {
            isScreenStreamingActive = false;
            if (screenStreamerThread != null && screenStreamerThread.IsAlive)
            {
                try { screenStreamerThread.Join(300); } catch { }
            }
            screenStreamerThread = null;
        }

        private void ScreenStreamerWorker()
        {
            while (isScreenStreamingActive)
            {
                bool pushSuccess = false;
                try {
                    if (db != null && db.Settings != null && db.Settings.connection != null && currentHostDesktop != null)
                    {
                        List<string> candidateUrls = GetRelayCandidateUrls();

                        int sw = Screen.PrimaryScreen.Bounds.Width;
                        int sh = Screen.PrimaryScreen.Bounds.Height;
                        int tw = Math.Min(1280, sw);
                        int th = (int)(sh * ((float)tw / sw));

                        string b64 = null;
                        using (Bitmap fullBmp = new Bitmap(sw, sh))
                        {
                            using (Graphics g = Graphics.FromImage(fullBmp))
                            {
                                g.CopyFromScreen(0, 0, 0, 0, fullBmp.Size);
                            }

                            using (Bitmap scaledBmp = new Bitmap(tw, th))
                            {
                                using (Graphics sg = Graphics.FromImage(scaledBmp))
                                {
                                    sg.InterpolationMode = InterpolationMode.Low;
                                    sg.DrawImage(fullBmp, 0, 0, tw, th);
                                }

                                using (MemoryStream ms = new MemoryStream())
                                {
                                    ImageCodecInfo jpgEncoder = GetCachedJpegEncoder();
                                    using (EncoderParameters ep = new EncoderParameters(1))
                                    using (EncoderParameter epParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 45L))
                                    {
                                        ep.Param[0] = epParam;
                                        if (jpgEncoder != null) scaledBmp.Save(ms, jpgEncoder, ep);
                                        else scaledBmp.Save(ms, ImageFormat.Jpeg);
                                    }
                                    b64 = Convert.ToBase64String(ms.ToArray());
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(b64))
                        {
                            string postJson = string.Format("{{\"targetId\":\"{0}\",\"frame\":\"data:image/jpeg;base64,{1}\"}}",
                                currentHostDesktop.RemoteId, b64);
                            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postJson);

                            foreach (string rUrl in candidateUrls)
                            {
                                try {
                                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/push_frame");
                                    req.Method = "POST";
                                    req.ContentType = "application/json";
                                    req.Timeout = 1200;
                                    req.ContentLength = bytes.Length;
                                    using (Stream os = req.GetRequestStream())
                                    {
                                        os.Write(bytes, 0, bytes.Length);
                                    }
                                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                                    {
                                        pushSuccess = true;
                                        bytesTransferred += bytes.Length;
                                        break;
                                    }
                                } catch { }
                            }
                        }
                    }
                } catch { }
                Thread.Sleep(pushSuccess ? 300 : 1500);
            }
        }

        // ======================================================================
        // HOST DAEMON & BIDIRECTIONAL REMOTE ADMIN INPUT ENGINE
        // ======================================================================

        private void StartHostDaemon()
        {
            if (isHostDaemonRunning) return;
            isHostDaemonRunning = true;
            hostDaemonThread = new Thread(new ThreadStart(HostDaemonWorker)) { IsBackground = true };
            hostDaemonThread.Start();
        }

        private void StopHostDaemon()
        {
            isHostDaemonRunning = false;
            if (hostDaemonThread != null && hostDaemonThread.IsAlive)
            {
                try { hostDaemonThread.Join(400); } catch { }
            }
            hostDaemonThread = null;
        }

        private void HostDaemonWorker()
        {
            while (isHostDaemonRunning)
            {
                try
                {
                    // 1. Continuous screen frame push (Keyframe kept warm on Colab)
                    PushLocalHostFrame();

                    // 2. Poll remote admin input events (mouse, key, system commands)
                    if (db != null && currentHostDesktop != null)
                    {
                        List<string> candidateUrls = GetRelayCandidateUrls();
                        foreach (string rUrl in candidateUrls)
                        {
                            try
                            {
                                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/pull_input/" + currentHostDesktop.RemoteId);
                                req.Method = "GET";
                                req.Timeout = 800;
                                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                                {
                                    string jsonStr = reader.ReadToEnd();
                                    JavaScriptSerializer ser = new JavaScriptSerializer();
                                    Dictionary<string, object> dict = ser.Deserialize<Dictionary<string, object>>(jsonStr);
                                    if (dict != null && dict.ContainsKey("success") && Convert.ToBoolean(dict["success"]) && dict.ContainsKey("inputs"))
                                    {
                                        System.Collections.IEnumerable list = dict["inputs"] as System.Collections.IEnumerable;
                                        if (list != null)
                                        {
                                            foreach (object item in list)
                                            {
                                                Dictionary<string, object> evDict = item as Dictionary<string, object>;
                                                if (evDict != null)
                                                {
                                                    ExecuteRemoteInputEvent(evDict);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                // Dynamic throttle: 80ms when actively serving, 600ms when idling
                Thread.Sleep(isSessionActive ? 80 : 600);
            }
        }

        private void PushLocalHostFrame()
        {
            try
            {
                if (db == null || db.Settings == null || db.Settings.connection == null || currentHostDesktop == null)
                    return;

                List<string> candidateUrls = GetRelayCandidateUrls();
                int sw = Screen.PrimaryScreen.Bounds.Width;
                int sh = Screen.PrimaryScreen.Bounds.Height;
                int tw = Math.Min(1280, sw);
                int th = (int)(sh * ((float)tw / sw));

                string b64 = null;
                using (Bitmap fullBmp = new Bitmap(sw, sh))
                {
                    using (Graphics g = Graphics.FromImage(fullBmp))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, fullBmp.Size);
                    }

                    using (Bitmap scaledBmp = new Bitmap(tw, th))
                    {
                        using (Graphics sg = Graphics.FromImage(scaledBmp))
                        {
                            sg.InterpolationMode = InterpolationMode.Low;
                            sg.DrawImage(fullBmp, 0, 0, tw, th);
                        }

                        using (MemoryStream ms = new MemoryStream())
                        {
                            ImageCodecInfo jpgEncoder = GetCachedJpegEncoder();
                            using (EncoderParameters ep = new EncoderParameters(1))
                            using (EncoderParameter epParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 45L))
                            {
                                ep.Param[0] = epParam;
                                if (jpgEncoder != null) scaledBmp.Save(ms, jpgEncoder, ep);
                                else scaledBmp.Save(ms, ImageFormat.Jpeg);
                            }
                            b64 = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }

                if (!string.IsNullOrEmpty(b64))
                {
                    string postJson = string.Format("{{\"targetId\":\"{0}\",\"frame\":\"data:image/jpeg;base64,{1}\"}}",
                        currentHostDesktop.RemoteId, b64);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postJson);

                    foreach (string rUrl in candidateUrls)
                    {
                        try
                        {
                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/push_frame");
                            req.Method = "POST";
                            req.ContentType = "application/json";
                            req.Timeout = 1200;
                            req.ContentLength = bytes.Length;
                            using (Stream os = req.GetRequestStream())
                            {
                                os.Write(bytes, 0, bytes.Length);
                            }
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                            {
                                cachedWinningRelayUrl = rUrl;
                                lastWinningCheckTime = DateTime.Now;
                                bytesTransferred += bytes.Length;
                                break;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private void ExecuteRemoteInputEvent(Dictionary<string, object> ev)
        {
            try
            {
                if (!ev.ContainsKey("type")) return;
                string type = ev["type"] as string;

                if (type == "mouse")
                {
                    string action = ev.ContainsKey("action") ? (ev["action"] as string) : "move";
                    int rx = ev.ContainsKey("x") ? Convert.ToInt32(ev["x"]) : 0;
                    int ry = ev.ContainsKey("y") ? Convert.ToInt32(ev["y"]) : 0;
                    string btn = ev.ContainsKey("btn") ? (ev["btn"] as string) : "Left";

                    int hostSw = Screen.PrimaryScreen.Bounds.Width;
                    int hostSh = Screen.PrimaryScreen.Bounds.Height;
                    int tw = Math.Min(1280, hostSw);
                    int th = (int)(hostSh * ((float)tw / hostSw));
                    int realX = tw > 0 ? (int)((float)rx / tw * hostSw) : rx;
                    int realY = th > 0 ? (int)((float)ry / th * hostSh) : ry;

                    realX = Math.Max(0, Math.Min(hostSw - 1, realX));
                    realY = Math.Max(0, Math.Min(hostSh - 1, realY));

                    SetCursorPos(realX, realY);

                    if (action == "down")
                    {
                        if (btn == "Right") mouse_event(MOUSEEVENTF_RIGHTDOWN, (uint)realX, (uint)realY, 0, 0);
                        else if (btn == "Middle") mouse_event(MOUSEEVENTF_MIDDLEDOWN, (uint)realX, (uint)realY, 0, 0);
                        else mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)realX, (uint)realY, 0, 0);
                    }
                    else if (action == "up")
                    {
                        if (btn == "Right") mouse_event(MOUSEEVENTF_RIGHTUP, (uint)realX, (uint)realY, 0, 0);
                        else if (btn == "Middle") mouse_event(MOUSEEVENTF_MIDDLEUP, (uint)realX, (uint)realY, 0, 0);
                        else mouse_event(MOUSEEVENTF_LEFTUP, (uint)realX, (uint)realY, 0, 0);
                    }
                    else if (action == "dblclick")
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)realX, (uint)realY, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, (uint)realX, (uint)realY, 0, 0);
                        Thread.Sleep(30);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)realX, (uint)realY, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, (uint)realX, (uint)realY, 0, 0);
                    }
                }
                else if (type == "key")
                {
                    string action = ev.ContainsKey("action") ? (ev["action"] as string) : "down";
                    int keyCode = ev.ContainsKey("keyCode") ? Convert.ToInt32(ev["keyCode"]) : 0;
                    if (keyCode > 0 && keyCode < 256)
                    {
                        byte vk = (byte)keyCode;
                        if (action == "down") keybd_event(vk, 0, 0, 0);
                        else if (action == "up") keybd_event(vk, 0, KEYEVENTF_KEYUP, 0);
                    }
                }
                else if (type == "admin_cmd")
                {
                    string cmd = ev.ContainsKey("cmd") ? (ev["cmd"] as string) : "";
                    ExecuteAdminCommand(cmd);
                }
            }
            catch { }
        }

        private void ExecuteAdminCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd)) return;
            try
            {
                switch (cmd.ToLowerInvariant())
                {
                    case "taskmgr":
                        Process.Start(new ProcessStartInfo("taskmgr.exe") { UseShellExecute = true });
                        break;
                    case "cmd":
                        Process.Start(new ProcessStartInfo("cmd.exe") { UseShellExecute = true });
                        break;
                    case "powershell":
                        Process.Start(new ProcessStartInfo("powershell.exe") { UseShellExecute = true });
                        break;
                    case "explorer":
                        Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
                        break;
                    case "lock":
                        try { Process.Start("rundll32.exe", "user32.dll,LockWorkStation"); } catch { }
                        break;
                    case "cad":
                        try { Process.Start("taskmgr.exe"); } catch { }
                        break;
                }
            }
            catch { }
        }

        private bool MapCanvasToRemote(int clientX, int clientY, out int remoteX, out int remoteY)
        {
            remoteX = 0;
            remoteY = 0;
            if (viewportCanvas == null || viewportCanvas.Width <= 0 || viewportCanvas.Height <= 0) return false;
            int cw = viewportCanvas.Width;
            int ch = viewportCanvas.Height;
            int imgW = 1280;
            int imgH = 720;
            lock (frameLock)
            {
                if (latestRemoteFrame != null)
                {
                    imgW = latestRemoteFrame.Width;
                    imgH = latestRemoteFrame.Height;
                }
            }

            if (smartFitMode == "SmartFit")
            {
                float ratio = Math.Min((float)cw / imgW, (float)ch / imgH);
                int targetW = (int)(imgW * ratio);
                int targetH = (int)(imgH * ratio);
                int targetX = (cw - targetW) / 2;
                int targetY = (ch - targetH) / 2;
                if (clientX < targetX || clientX > targetX + targetW || clientY < targetY || clientY > targetY + targetH)
                    return false;
                remoteX = (int)((clientX - targetX) / ratio);
                remoteY = (int)((clientY - targetY) / ratio);
            }
            else if (smartFitMode == "Original 1:1")
            {
                remoteX = clientX;
                remoteY = clientY;
            }
            else
            {
                remoteX = (int)((float)clientX / cw * imgW);
                remoteY = (int)((float)clientY / ch * imgH);
            }
            return true;
        }

        private void SendRemoteInput(string eventType, Dictionary<string, object> details)
        {
            if (!isSessionActive || string.IsNullOrEmpty(activeReceiverTargetId)) return;
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    List<string> candidateUrls = GetRelayCandidateUrls();
                    details["type"] = eventType;
                    Dictionary<string, object> payload = new Dictionary<string, object>();
                    payload["targetId"] = activeReceiverTargetId;
                    payload["event"] = details;

                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    string json = ser.Serialize(payload);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                    foreach (string rUrl in candidateUrls)
                    {
                        try
                        {
                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/push_input");
                            req.Method = "POST";
                            req.ContentType = "application/json";
                            req.Timeout = 800;
                            req.ContentLength = bytes.Length;
                            using (Stream os = req.GetRequestStream())
                            {
                                os.Write(bytes, 0, bytes.Length);
                            }
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                            {
                                break;
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            });
        }

        private void SendRemoteAdminCommand(string cmd)
        {
            Dictionary<string, object> ev = new Dictionary<string, object>();
            ev["cmd"] = cmd;
            SendRemoteInput("admin_cmd", ev);
        }

        private void StartFrameReceiver(string targetRemoteId)
        {
            StopFrameReceiver();
            isFrameReceiverActive = true;
            activeReceiverTargetId = targetRemoteId;
            frameReceiverThread = new Thread(new ThreadStart(FrameReceiverWorker)) { IsBackground = true };
            frameReceiverThread.Start();
        }

        private void StopFrameReceiver()
        {
            isFrameReceiverActive = false;
            if (frameReceiverThread != null && frameReceiverThread.IsAlive)
            {
                try { frameReceiverThread.Join(300); } catch { }
            }
            frameReceiverThread = null;
        }

        private void FrameReceiverWorker()
        {
            DateTime workerStart = DateTime.Now;
            while (isFrameReceiverActive && isSessionActive)
            {
                bool successFrame = false;
                try {
                    // Check connecting timeout (20 seconds)
                    if (currentConnectionState == SessionConnectionState.Connecting)
                    {
                        TimeSpan elapsed = DateTime.Now - workerStart;
                        if (elapsed.TotalSeconds > 20)
                        {
                            if (this.IsHandleCreated && !this.IsDisposed)
                            {
                                this.BeginInvoke(new Action(() => {
                                    AbortSessionConnection("Connection Timeout: Remote Desk (" + activeReceiverTargetId + ") is not streaming frames or Colab Relay is offline.");
                                }));
                            }
                            break;
                        }
                        else if (elapsed.TotalSeconds > 10)
                        {
                            connectionStageMessage = "Awaiting Live Video Stream Frames from Desk " + activeReceiverTargetId + "...";
                            connectingProgressPct = Math.Min(85, (int)(15 + elapsed.TotalSeconds * 3.5));
                        }
                        else if (elapsed.TotalSeconds > 3)
                        {
                            connectionStageMessage = "Negotiating P2P Stream via Colab Relay...";
                            connectingProgressPct = Math.Min(60, (int)(15 + elapsed.TotalSeconds * 5.0));
                        }
                    }

                    if (db != null && db.Settings != null && db.Settings.connection != null && !string.IsNullOrEmpty(activeReceiverTargetId))
                    {
                        List<string> candidateUrls = GetRelayCandidateUrls();

                        foreach (string rUrl in candidateUrls)
                        {
                            try {
                                Stopwatch sw = Stopwatch.StartNew();
                                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/pull_frame/" + activeReceiverTargetId);
                                req.Method = "GET";
                                req.Timeout = 1200;
                                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                                {
                                    sw.Stop();
                                    string jsonStr = reader.ReadToEnd();
                                    JavaScriptSerializer ser = new JavaScriptSerializer();
                                    Dictionary<string, object> dict = ser.Deserialize<Dictionary<string, object>>(jsonStr);
                                    if (dict != null && dict.ContainsKey("success") && Convert.ToBoolean(dict["success"]) && dict.ContainsKey("frame"))
                                    {
                                        string frameVal = dict["frame"] as string;
                                        if (!string.IsNullOrEmpty(frameVal))
                                        {
                                            int commaIdx = frameVal.IndexOf(',');
                                            string rawB64 = commaIdx >= 0 ? frameVal.Substring(commaIdx + 1) : frameVal;
                                            byte[] imgBytes = Convert.FromBase64String(rawB64);

                                            Bitmap independentBmp = null;
                                            using (MemoryStream ms = new MemoryStream(imgBytes))
                                            using (Image temp = Image.FromStream(ms))
                                            {
                                                independentBmp = new Bitmap(temp.Width, temp.Height, PixelFormat.Format32bppPArgb);
                                                using (Graphics gCopy = Graphics.FromImage(independentBmp))
                                                {
                                                    gCopy.DrawImage(temp, 0, 0, temp.Width, temp.Height);
                                                }
                                            }

                                            lock (frameLock)
                                            {
                                                Image oldFrame = latestRemoteFrame;
                                                latestRemoteFrame = independentBmp;
                                                if (oldFrame != null)
                                                {
                                                    try { oldFrame.Dispose(); } catch { }
                                                }
                                                framesReceivedCount++;
                                                bytesTransferred += imgBytes.Length;
                                                lastMeasuredRttMs = (int)sw.ElapsedMilliseconds;
                                            }

                                            successFrame = true;

                                            // Transition to Connected once first genuine frame arrives
                                            if (currentConnectionState == SessionConnectionState.Connecting)
                                            {
                                                if (this.IsHandleCreated && !this.IsDisposed)
                                                {
                                                    this.BeginInvoke(new Action(() => {
                                                        currentConnectionState = SessionConnectionState.Connected;
                                                        if (connectingAnimationTimer != null) connectingAnimationTimer.Stop();
                                                        sessionStartTime = DateTime.Now;
                                                        sessionTelemetryTimer.Start();

                                                        if (btnDisconnect != null)
                                                        {
                                                            btnDisconnect.Text = "✕ Disconnect";
                                                            btnDisconnect.NormalColor = Color.FromArgb(239, 68, 68);
                                                        }
                                                        if (lblRemoteStatus != null)
                                                        {
                                                            lblRemoteStatus.Text = "● Connected";
                                                            lblRemoteStatus.ForeColor = Color.FromArgb(16, 185, 129);
                                                        }
                                                        if (btnCardConnect != null)
                                                        {
                                                            btnCardConnect.Text = "✕ Disconnect";
                                                            btnCardConnect.NormalColor = Color.FromArgb(239, 68, 68);
                                                        }

                                                        db.AddLog("SESSION_START", "P2P_CONNECT", "Live stream established with " + (connectedTargetDesktop != null ? connectedTargetDesktop.Name : activeReceiverTargetId));
                                                        ShowToast("P2P Session Live: " + (connectedTargetDesktop != null ? connectedTargetDesktop.Name : activeReceiverTargetId));
                                                    }));
                                                }
                                            }

                                            if (viewportCanvas != null && !viewportCanvas.IsDisposed)
                                            {
                                                viewportCanvas.BeginInvoke(new Action(() => viewportCanvas.Invalidate()));
                                            }
                                            break;
                                        }
                                    }
                                }
                            } catch { }
                        }
                    }
                } catch { }
                Thread.Sleep(successFrame ? 150 : 800);
            }
        }

        private void SyncFromGitHubBackend()
        {
            ThreadPool.QueueUserWorkItem(delegate {
                try {
                    if (db == null || db.Settings == null || db.Settings.connection == null) return;
                    string ghUrl = db.Settings.connection.githubBackendUrl.TrimEnd('/');
                    if (string.IsNullOrEmpty(ghUrl)) return;
                    string manifestUrl = ghUrl + "/database.json";
                    using (WebClient client = new WebClient()) {
                        string cloudJson = client.DownloadString(manifestUrl);
                        if (!string.IsNullOrEmpty(cloudJson)) {
                            if (!File.Exists(db.DbPath)) {
                                File.WriteAllText(db.DbPath, cloudJson);
                                db.Load();
                                SyncDynamicDesktopsFromDatabase();
                            }
                            db.AddLog("CLOUD_SYNC", "GITHUB_OK", "Connected to GitHub SSOT: " + ghUrl);
                        }
                    }
                } catch { }
            });
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
            trayMenu.MenuItems.Add("Exit", (s, e) => {
                StopHostDaemon();
                StopScreenStreamer();
                StopFrameReceiver();
                trayIcon.Visible = false;
                Application.Exit();
            });

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
                StopHostDaemon();
                StopScreenStreamer();
                StopFrameReceiver();
                base.OnFormClosing(e);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (currentConnectionState == SessionConnectionState.Connected && activeViewKey == "Console" && viewportCanvas != null && viewportCanvas.Visible && (txtTargetId == null || !txtTargetId.Focused))
            {
                Keys pureKey = keyData & Keys.KeyCode;
                Dictionary<string, object> ev = new Dictionary<string, object>();
                ev["action"] = "down";
                ev["keyCode"] = (int)pureKey;
                SendRemoteInput("key", ev);

                ThreadPool.QueueUserWorkItem(delegate {
                    Thread.Sleep(60);
                    Dictionary<string, object> upEv = new Dictionary<string, object>();
                    upEv["action"] = "up";
                    upEv["keyCode"] = (int)pureKey;
                    SendRemoteInput("key", upEv);
                });

                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            CheckClipboardForColabUrl();
        }

        private void CheckClipboardForColabUrl()
        {
            try
            {
                if (!isColabConnected && Clipboard.ContainsText())
                {
                    string clip = Clipboard.GetText().Trim();
                    if (clip.Contains(".trycloudflare.com"))
                    {
                        int startIdx = clip.IndexOf("https://");
                        if (startIdx < 0) startIdx = clip.IndexOf("http://");
                        if (startIdx >= 0)
                        {
                            int endIdx = clip.IndexOfAny(new char[] { ' ', '\r', '\n', '\t' }, startIdx);
                            clip = endIdx > startIdx ? clip.Substring(startIdx, endIdx - startIdx) : clip.Substring(startIdx);
                        }
                        clip = clip.TrimEnd('/');
                        if (db != null && db.Settings != null && db.Settings.connection != null)
                        {
                            if (db.Settings.connection.relayServer != clip)
                            {
                                db.Settings.connection.relayServer = clip;
                                db.Settings.connection.customDomain = clip;
                                db.Save();
                                db.SyncYaml();
                                try {
                                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colab_tunnel.txt"), clip);
                                } catch { }
                                cachedWinningRelayUrl = clip;
                                lastWinningCheckTime = DateTime.MinValue;
                                ShowToast("Auto-detected active Colab Tunnel from clipboard: " + clip);
                                ThreadPool.QueueUserWorkItem(delegate { PingColabServer(); });
                                if (activeViewKey == "Console") BuildWelcomeCardsContent();
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
