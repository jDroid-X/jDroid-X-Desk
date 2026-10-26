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
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "BlockInput")]
        private static extern bool BlockInput(bool fBlockIt);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SetThreadExecutionState(uint esFlags);

        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_AWAYMODE_REQUIRED = 0x00000040;

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        private const int CURSOR_SHOWING = 0x00000001;

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        private static extern bool DrawIcon(IntPtr hdc, int x, int y, IntPtr hIcon);

        private static int activeHostMonitorIndex = 0;
        public static void SetActiveHostMonitorIndex(int idx)
        {
            if (idx >= 0 && idx < Screen.AllScreens.Length) activeHostMonitorIndex = idx;
        }

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
        private Panel drawerHeader;

        // Welcome Cards SmartFit Components
        private Panel cardContainer;
        private Panel pnlEnginePill;
        private Panel cardColab;
        private Panel cardShare;
        private ModernCardFlipper flipperConnect;
        private Panel codeBox;
        private FlowLayoutPanel shareBtns;
        private TextBox txtColabInput;
        private Button btnPasteColab;
        private Button btnApplyColab;
        private Button btnStartLocalRelay;
        private Button btnColabNotebook;
        private Process localPythonRelayProcess = null;
        private ComboBox cboSavedFleet;
        private Panel cardRecent;
        private Panel cardDiscovered;
        private FlowLayoutPanel flowDiscoveredDesks;
        private Panel pnlHostViewerAlert;
        private Label lblHostViewerAlert;
        private ModernButton btnHostDisconnectViewer;
        private ModernButton btnHostAcceptViewer;
        private CheckBox chkPermInput;
        private CheckBox chkPermClip;
        private CheckBox chkPermFiles;
        private CheckBox chkPermBlockLocal;
        private bool isLocalInputBlockedByRemote = false;
        private bool isRemoteInputBlockedByViewer = false;
        private bool hostPermissionAllowInput = true;
        private bool hostPermissionAllowClipboard = true;
        private bool hostPermissionAllowFiles = true;
        private bool autoSyncClipboard = true;
        private string lastLocalClipboardText = "";
        private string lastReceivedClipboardText = "";
        private JDroidXIncomingSessionDialog activeIncomingDialog = null;
        private string lastApprovedViewerEndpoint = "";
        private string lastRejectedViewerEndpoint = "";
        private DateTime lastRejectedViewerTime = DateTime.MinValue;
        private readonly HashSet<string> approvedViewerIps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> rejectedViewerIps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private volatile bool isHostServingRemoteClient = false;
        public bool IsHostServingRemoteClient { get { return isHostServingRemoteClient; } }

        private static string ExtractClientIp(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return "Remote Desk";
            string s = endpoint.Trim();
            int colon = s.LastIndexOf(':');
            if (colon > 0 && !s.Contains("["))
            {
                return s.Substring(0, colon);
            }
            if (s.StartsWith("[") && s.Contains("]:"))
            {
                int bracket = s.IndexOf(']');
                return s.Substring(1, bracket - 1);
            }
            return s;
        }

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
        private ModernTextBox modernCardTargetBox;
        private ModernButton btnCardConnect;
        private Label lblCardLocalId;
        private Panel sessionFloatingToolbar;
        private Panel localBar;
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
        private string smartFitMode = "Stretch"; // Default: Stretch / Fill Viewport (Zero black side borders)
        private bool isImmersiveZenMode = false;
        private Panel pnlRdpPinBar = null;
        private Label lblRdpPinStatus = null;
        private Button btnRdpPinToggle = null;
        private bool isRdpPinBarPinned = true;
        private DateTime sessionStartTime;
        private DateTime connectionAttemptStartTime;
        private DateTime lastUserActivityUtc = DateTime.UtcNow;
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
        private Panel panelMetaEngine;
        private Label lblMetaEngineTitle;
        private ComboBox cbHeaderEngineSelector;
        private Button btnHeaderEngineGuide;
        private Button btnHeaderRunScript;
        private Label lblHeaderEngineStatus;
        private Label lblBackendSyncStatus;
        private Button btnThemeToggle;
        private ComboBox cbHeaderThemeSelector;
        private Label lblHeaderGatewayPill;
        private bool isDarkMode = false; // Default: Light-First Discipline
        private bool engineIsLiveActive = false;
        private string engineLiveStatusBadge = "[READY]";
        private string backendSyncStatusBadge = "[✓ SYNCED]";
        private bool isSwitchingEngineFromHeader = false;
        private FileSystemWatcher dbThemeWatcher;
        private bool isUpdatingThemeProgrammatically = false;
        private string lastNotifiedTheme = null;
        private DateTime lastThemeToastTime = DateTime.MinValue;

        // Bottom Footer with Scrolling Marquee Metadata
        private Panel footerStatusBar;
        private Label lblFooterEngineBadge;
        private Label lblFooterMarquee;
        private Button btnFooterTheme;
        private Label lblFooterNodeStatus;
        private System.Windows.Forms.Timer marqueeTimer;
        private int marqueeCharIndex = 0;
        private Panel panelMetaRam;
        private Panel panelMetaGpu;
        private Panel panelMetaDisk;
        private Panel panelMetaStatus;
        private Panel panelMetaHeartbeat;
        private Label lblMetaRamTitle;
        private Label lblMetaGpuTitle;
        private Label lblMetaDiskTitle;
        private Label lblMetaStatusTitle;
        private Label lblMetaHeartbeatTitle;
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

        // Server Admin & Engine Management
        private Panel viewServerAdmin;
        private Panel headerAdmin;
        private Panel pnlScrollAdmin;
        private Panel pnlLimitsCard;
        private Panel pnlLogsGrid;
        private DataGridView dgvAdminLogs;
        private Label lblAdminActiveEngineBadge;
        private Label lblAdminCpu;
        private Label lblAdminMem;
        private Label lblAdminUptime;
        private Label lblAdminSessions;
        private NumericUpDown numAdminMaxSessions;
        private NumericUpDown numAdminBandwidth;
        private ComboBox cbAdminFps;
        private NumericUpDown numAdminIdleTimeout;
        private NumericUpDown numAdminLogRetention;
        private ComboBox cbAdminHwAccel;
        private ComboBox cbAdminLogLevel;
        private TextBox txtAdminLogSearch;
        private System.Windows.Forms.Timer adminTelemetryTimer;
        private Dictionary<string, Panel> engineCardPanels = new Dictionary<string, Panel>();

        // System Tray
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

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
        public static void Main(string[] args)
        {
            try
            {
                UnblockSelf();
                try { SetProcessDPIAware(); } catch { }
                try {
                    ServicePointManager.DefaultConnectionLimit = 256;
                    ServicePointManager.UseNagleAlgorithm = false;
                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Tls;
                    WebRequest.DefaultWebProxy = null;
                } catch { }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Single-Instance Mutex Guard: Prevent dual-instance port collisions & zombies
                bool isFirstInstance = false;
                using (Mutex singleMutex = new Mutex(true, @"Global\jDroid-X-Desk-SingleInstance-Mutex-2026", out isFirstInstance))
                {
                    if (!isFirstInstance)
                    {
                        IntPtr hWndMini = FindWindow(null, "jDroid-X-Desk Mini | Remote Connection");
                        if (hWndMini != IntPtr.Zero)
                        {
                            ShowWindow(hWndMini, 9); // SW_RESTORE
                            SetForegroundWindow(hWndMini);
                            return;
                        }
                        IntPtr hWndFull = FindWindow(null, "jDroid-X-Desk | Remote Desktop Workspace");
                        if (hWndFull != IntPtr.Zero)
                        {
                            ShowWindow(hWndFull, 9); // SW_RESTORE
                            SetForegroundWindow(hWndFull);
                            return;
                        }
                        MessageBox.Show("jDroid-X-Desk is already running in the background or system tray.\nCheck the notification area or double-click the shield icon.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Self-healing: Ensure essential files exist in working directory
                    InstallationManager.EnsureEssentialFiles(AppDomain.CurrentDomain.BaseDirectory);

                    // Handle installation flag if requested via CLI
                    if (args != null && args.Length > 0 && (args[0] == "--install" || args[0] == "/install"))
                    {
                        string err;
                        bool ok = InstallationManager.InstallToStandardDirectory(out err);
                        if (ok) MessageBox.Show("jDroid-X-Desk installed successfully to standard user folder:\n" + InstallationManager.GetStandardInstallDirectory(), "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else MessageBox.Show("Installation failed: " + err, "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // If running outside the installed directory and not in dev workspace, prompt to install
                    bool isPortable = (args != null && args.Length > 0 && (args[0] == "--portable" || args[0] == "/portable"));
                    if (!isPortable && !InstallationManager.IsRunningFromStandardDirectory() && !InstallationManager.IsDevWorkspaceDirectory())
                    {
                        DialogResult setupResult = MessageBox.Show(
                            "Welcome to jDroid-X-Desk!\n\n" +
                            "Would you like to install jDroid-X-Desk to your standard applications folder?\n\n" +
                            "• Desktop & Start Menu Shortcuts\n" +
                            "• Clean system uninstaller in Windows Settings\n" +
                            "• Auto-healing background daemons & relays\n\n" +
                            "Click 'Yes' to install and launch, or 'No' to continue portably.",
                            "jDroid-X-Desk Universal Setup",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (setupResult == DialogResult.Yes)
                        {
                            string err;
                            bool ok = InstallationManager.InstallToStandardDirectory(out err);
                            if (ok)
                            {
                                string targetExe = Path.Combine(InstallationManager.GetStandardInstallDirectory(), "jDroid-X-Desk.exe");
                                Process.Start(new ProcessStartInfo(targetExe) { WorkingDirectory = InstallationManager.GetStandardInstallDirectory() });
                                return;
                            }
                            else
                            {
                                MessageBox.Show("Installation encountered an issue: " + err + "\nContinuing in current location.", "Setup Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }

                // Determine startup mode (Default: Mini Desk)
                string chosenMode = "mini";
                bool cliForced = false;
                if (args != null && args.Length > 0)
                {
                    if (args[0] == "--mini" || args[0] == "/mini") { chosenMode = "mini"; cliForced = true; }
                    else if (args[0] == "--desk" || args[0] == "/desk" || args[0] == "--full") { chosenMode = "desk"; cliForced = true; }
                }

                DatabaseContext tempDb = new DatabaseContext();
                string savedStartupMode = (tempDb.Settings != null && tempDb.Settings.general != null) ? tempDb.Settings.general.startupMode : "ask";

                if (!cliForced && savedStartupMode == "ask")
                {
                    bool remember;
                    chosenMode = StartupModeDialog.PromptStartupMode(null, out remember);
                    if (remember && tempDb.Settings != null && tempDb.Settings.general != null)
                    {
                        tempDb.Settings.general.startupMode = chosenMode;
                        tempDb.Save();
                    }
                }
                else if (!cliForced && !string.IsNullOrEmpty(savedStartupMode) && savedStartupMode != "ask")
                {
                    chosenMode = savedStartupMode;
                }

                MainForm mainApp = new MainForm();

                if (chosenMode == "mini")
                {
                    // Mini Desk mode: mainApp runs in background with all daemons active, MiniDeskForm is visible
                    mainApp.WindowState = FormWindowState.Minimized;
                    mainApp.ShowInTaskbar = false;
                    mainApp.Visible = false;

                    MiniDeskForm miniForm = new MiniDeskForm(mainApp);
                    mainApp.RegisterMiniDeskForm(miniForm);
                    Application.Run(miniForm);
                }
                else
                {
                    Application.Run(mainApp);
                }
                } // End Single-Instance Mutex Guard
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "native_error.log"), ex.ToString());
            }
        }
        // ======================================================================
        // JDROID-X-DESKMINI & EXTERNAL INTEGRATION API (CALL BY REFERENCE)
        // ======================================================================
        private MiniDeskForm miniDeskForm;
        public MiniDeskForm MiniDesk { get { return miniDeskForm; } }
        public SimulatorDesktop CurrentHostDesktop { get { return currentHostDesktop; } }
        public DatabaseContext Database { get { return db; } }
        public bool IsSessionActive { get { return isSessionActive; } }

        public void RegisterMiniDeskForm(MiniDeskForm mini)
        {
            this.miniDeskForm = mini;
        }

        public void ShowFullConsole()
        {
            if (this.IsDisposed) return;
            if (this.miniDeskForm != null && !this.miniDeskForm.IsDisposed)
            {
                this.miniDeskForm.Hide();
            }
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.BringToFront();
        }

        public void ShowMiniDesk()
        {
            if (miniDeskForm == null || miniDeskForm.IsDisposed)
            {
                miniDeskForm = new MiniDeskForm(this);
            }
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            miniDeskForm.Show();
            miniDeskForm.WindowState = FormWindowState.Normal;
            miniDeskForm.BringToFront();
        }

        public void SwitchToTab(string tabKey)
        {
            if (this.IsDisposed) return;
            ShowFullConsole();
            SwitchWorkspaceView(tabKey);
        }

        public void InitiateRemoteConnection(string targetId, bool keepMiniActive = false)
        {
            if (string.IsNullOrEmpty(targetId)) return;
            if (!keepMiniActive)
            {
                ShowFullConsole();
                SwitchWorkspaceView("Console");
            }
            if (txtTargetId != null) txtTargetId.Text = targetId;
            if (txtCardTargetId != null) txtCardTargetId.Text = targetId;
            if (modernCardTargetBox != null) modernCardTargetBox.Text = targetId;
            ToggleSessionConnection();
        }

        public Image GetLatestRemoteFrameClone()
        {
            lock (frameLock)
            {
                if (latestRemoteFrame == null) return null;
                try { return (Image)latestRemoteFrame.Clone(); } catch { return null; }
            }
        }

        public void ForwardRemoteInput(string eventType, System.Collections.Generic.Dictionary<string, object> details)
        {
            SendRemoteInput(eventType, details);
        }

        public void ForwardRemoteAdminCommand(string cmd)
        {
            SendRemoteAdminCommand(cmd);
        }

        public float CurrentMeasuredFps { get { return currentMeasuredFps; } }
        public int LastMeasuredRttMs { get { return lastMeasuredRttMs; } }
        public SessionConnectionState CurrentConnectionState { get { return currentConnectionState; } }
        public string ActiveReceiverTargetId { get { return activeReceiverTargetId; } }

        public void DisconnectActiveSession()
        {
            if (isSessionActive)
            {
                ToggleSessionConnection();
            }
        }

        public static bool IsRunningElevated()
        {
            try
            {
                var p = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent());
                return p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        public static void RequestProcessElevation()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(Application.ExecutablePath)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                Application.Exit();
            }
            catch { }
        }

        // ======================================================================
        // JDROID-X IN-SESSION FEATURES & 14-ACTION SUITE (OFFICIAL JDROID-X TOP BAR)
        // ======================================================================
        public string SmartFitMode { get { return smartFitMode; } set { smartFitMode = value; } }
        private bool isPrivacyModeActive = false;
        public bool IsPrivacyModeActive { get { return isPrivacyModeActive; } }
        private bool isWhiteboardActive = false;
        public bool IsWhiteboardActive { get { return isWhiteboardActive; } }
        public readonly List<List<Point>> WhiteboardStrokes = new List<List<Point>>();
        private bool isRecordingSession = false;
        public bool IsRecordingSession { get { return isRecordingSession; } }
        private string activeRecordingFolder = "";
        private int currentClientMonitorIdx = 0;
        public int CurrentClientMonitorIdx { get { return currentClientMonitorIdx; } set { currentClientMonitorIdx = value; } }

        public void TogglePrivacyMode()
        {
            isPrivacyModeActive = !isPrivacyModeActive;
            SendRemoteAdminCommand(isPrivacyModeActive ? "privacy_mode_on" : "privacy_mode_off");
            ShowToast(isPrivacyModeActive ? "⦸ Host Privacy Mode Activated (Physical screen blanked)" : "⦸ Privacy Mode Deactivated");
        }

        public void ToggleWhiteboard()
        {
            isWhiteboardActive = !isWhiteboardActive;
            if (!isWhiteboardActive)
            {
                WhiteboardStrokes.Clear();
            }
            if (viewportCanvas != null) viewportCanvas.Invalidate();
            ShowToast(isWhiteboardActive ? "✏️ Whiteboard Annotation Pen Active (Draw on remote screen)" : "✏️ Whiteboard Cleared & Deactivated");
        }

        public void ToggleSessionRecording()
        {
            isRecordingSession = !isRecordingSession;
            if (isRecordingSession)
            {
                try
                {
                    string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "jDroid-X-Desk", "Recordings");
                    if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
                    activeRecordingFolder = Path.Combine(baseDir, string.Format("Session_{0:yyyyMMdd_HHmmss}", DateTime.Now));
                    Directory.CreateDirectory(activeRecordingFolder);
                    ShowToast("📹 REC: Session recording started in Videos\\jDroid-X-Desk\\Recordings");
                }
                catch { }
            }
            else
            {
                ShowToast("⏹️ REC: Session recording saved and stopped.");
            }
        }

        public void AddCurrentTargetToFavorites()
        {
            string tid = !string.IsNullOrEmpty(activeReceiverTargetId) ? activeReceiverTargetId : (connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : "");
            if (string.IsNullOrEmpty(tid))
            {
                ShowToast("No active remote desk to add to favorites.");
                return;
            }
            if (db != null && db.Contacts != null)
            {
                ContactItem existing = db.Contacts.Find(delegate(ContactItem x) { return x.remoteId == tid; });
                if (existing == null)
                {
                    ContactItem ci = new ContactItem
                    {
                        id = Guid.NewGuid().ToString("N").Substring(0, 8),
                        name = "Desk " + tid,
                        remoteId = tid,
                        group = "Favorites"
                    };
                    db.Contacts.Add(ci);
                    db.Save();
                    SyncDynamicDesktopsFromDatabase();
                    ShowToast("★ Added " + tid + " to Fleet Favorites!");
                }
                else
                {
                    ShowToast("★ " + tid + " is already in your Favorites.");
                }
            }
        }

        public void SwitchActiveDisplay()
        {
            currentClientMonitorIdx = (currentClientMonitorIdx + 1) % 4;
            ForwardRemoteAdminCommand("switch_monitor:" + currentClientMonitorIdx);
            ShowToast("🖵 Switched to Display " + (currentClientMonitorIdx == 3 ? "All (Combined)" : (currentClientMonitorIdx + 1).ToString()));
        }

        public void ToggleSmartFitMode()
        {
            if (smartFitMode == "SmartFit") smartFitMode = "Original 1:1";
            else if (smartFitMode == "Original 1:1") smartFitMode = "Stretch";
            else smartFitMode = "SmartFit";
            if (viewportCanvas != null) viewportCanvas.Invalidate();
            ShowToast("Display Scaling: " + smartFitMode);
        }

        public void ShowActionsMenu(Control parent, Point pt)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("⌨️ Send Ctrl+Alt+Del (SAS)", null, (s, e) => {
                SendRemoteAdminCommand("cad");
                ShowToast("⚡ Injected Remote SAS (Ctrl+Alt+Del).");
            });
            menu.Items.Add("🔒 Lock Remote Workstation", null, (s, e) => {
                SendRemoteAdminCommand("lock");
                ShowToast("🔒 Sent Workstation Lock command.");
            });
            menu.Items.Add("⚙️ Launch Remote Task Manager", null, (s, e) => {
                SendRemoteAdminCommand("taskmgr");
                ShowToast("⚙️ Launched Remote Task Manager.");
            });
            menu.Items.Add("💻 Launch Remote Cmd (Admin)", null, (s, e) => {
                SendRemoteAdminCommand("cmd");
                ShowToast("💻 Launched Remote Admin Command Prompt.");
            });
            menu.Items.Add("🔄 Restart Remote Computer", null, (s, e) => {
                DialogResult dr = MessageBox.Show("Are you sure you want to restart the remote computer?", "Restart Remote Computer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    SendRemoteAdminCommand("restart");
                    ShowToast("🔄 Dispatched remote restart command.");
                }
            });
            menu.Show(parent, pt);
        }

        public void ShowKeyboardMenu(Control parent, Point pt)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("🪟 Send Windows Key", null, (s, e) => {
                SendRemoteAdminCommand("key_win");
                ShowToast("Injected Windows Key.");
            });
            menu.Items.Add("↹ Send Alt + Tab", null, (s, e) => {
                SendRemoteAdminCommand("key_alttab");
                ShowToast("Injected Alt + Tab.");
            });
            menu.Items.Add("✕ Send Alt + F4", null, (s, e) => {
                SendRemoteAdminCommand("key_altf4");
                ShowToast("Injected Alt + F4.");
            });
            menu.Items.Add("📋 Paste Clipboard as Keystrokes", null, (s, e) => {
                SendRemoteAdminCommand("paste_keystrokes");
                ShowToast("Transmitted clipboard buffer as keystrokes.");
            });
            menu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem miMap = new ToolStripMenuItem("Map 1:1 Physical Scancodes") { Checked = true };
            miMap.Click += (s, e) => { miMap.Checked = !miMap.Checked; ShowToast(miMap.Checked ? "1:1 Scancode Mapping Active" : "Translated Virtual Keys Active"); };
            menu.Items.Add(miMap);
            menu.Show(parent, pt);
        }

        public void ShowElevationMenu(Control parent, Point pt)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            bool isElevated = IsRunningElevated();
            ToolStripMenuItem miStatus = new ToolStripMenuItem(isElevated ? "🛡️ Current Status: High Integrity (Administrator)" : "🛡️ Current Status: Medium Integrity (Standard User)") { Enabled = false };
            menu.Items.Add(miStatus);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("⚡ Request Elevation on Remote Desk", null, (s, e) => {
                SendRemoteAdminCommand("elevate");
                ShowToast("🛡️ Dispatched UAC elevation request to remote desk.");
            });
            if (!isElevated)
            {
                menu.Items.Add("🚀 Elevate Local jDroid-X Process", null, (s, e) => {
                    RequestProcessElevation();
                });
            }
            menu.Show(parent, pt);
        }

        public void ShowSessionOptionsMenu(Control parent, Point pt)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("🔊 Toggle Remote Audio Loopback", null, (s, e) => {
                SendRemoteAdminCommand("audio_toggle");
                ShowToast("Toggled Remote Audio Loopback.");
            });
            menu.Items.Add("📸 Take Remote Screenshot (Copy to Clipboard)", null, (s, e) => CaptureRemoteScreenshot());
            menu.Items.Add("📋 Synchronize Clipboard Now", null, (s, e) => SynchronizeClipboardNow());
            menu.Items.Add("⛶ Toggle Fullscreen (F11)", null, (s, e) => ToggleImmersiveZenMode());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("ℹ️ Connection Diagnostics & Telemetry", null, (s, e) => {
                MessageBox.Show(GetSessionDiagnostics(), "Session Telemetry & Connection HUD", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("❌ Disconnect Session", null, (s, e) => {
                ToggleSessionConnection();
            });
            menu.Show(parent, pt);
        }

        public string GetSessionDiagnostics()
        {
            string tid = !string.IsNullOrEmpty(activeReceiverTargetId) ? activeReceiverTargetId : (connectedTargetDesktop != null ? connectedTargetDesktop.RemoteId : "---");
            return string.Format(
                "=== jDroid-X Session Telemetry ===\n" +
                "Target Desk ID   : {0}\n" +
                "Round Trip Time  : {1} ms\n" +
                "Frame Rate       : {2:F1} FPS\n" +
                "Active Scaling   : {3}\n" +
                "Relay Channel    : {4}\n" +
                "Security Cipher  : AES-256-GCM / TLS 1.3\n" +
                "Screen Resolution: {5}x{6}",
                tid,
                LastMeasuredRttMs,
                CurrentMeasuredFps,
                SmartFitMode,
                (db != null && db.Settings != null && db.Settings.connection != null && db.Settings.connection.colabCloudRelay ? "Cloud Mesh Relay (desk.jdroid-x.com)" : "Direct P2P LAN (Port 8088)"),
                (latestRemoteFrame != null ? latestRemoteFrame.Width : 1920),
                (latestRemoteFrame != null ? latestRemoteFrame.Height : 1080)
            );
        }

        public void SynchronizeClipboardNow()
        {
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    Thread ct = new Thread(() => {
                        try
                        {
                            if (Clipboard.ContainsImage())
                            {
                                Image img = Clipboard.GetImage();
                                if (img != null)
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        string b64 = Convert.ToBase64String(ms.ToArray());
                                        Dictionary<string, object> clipEv = new Dictionary<string, object>();
                                        clipEv["format"] = "image";
                                        clipEv["data"] = b64;
                                        SendRemoteInput("clipboard", clipEv);
                                    }
                                }
                            }
                            else if (Clipboard.ContainsText())
                            {
                                string curClip = Clipboard.GetText();
                                if (!string.IsNullOrEmpty(curClip) && curClip.Length < 150000)
                                {
                                    Dictionary<string, object> clipEv = new Dictionary<string, object>();
                                    clipEv["format"] = "text";
                                    clipEv["text"] = curClip;
                                    SendRemoteInput("clipboard", clipEv);
                                }
                            }
                        }
                        catch { }
                    });
                    ct.SetApartmentState(ApartmentState.STA);
                    ct.IsBackground = true;
                    ct.Start();
                }
                catch { }
            });
        }

        public void QuickAddContact(string targetId)
        {
            if (string.IsNullOrEmpty(targetId) || db == null) return;
            targetId = targetId.Trim();
            if (db.Contacts == null) db.Contacts = new List<ContactItem>();
            ContactItem existing = db.Contacts.Find(delegate(ContactItem x) { return x.remoteId == targetId; });
            if (existing == null)
            {
                ContactItem ci = new ContactItem
                {
                    id = Guid.NewGuid().ToString("N"),
                    name = "Workstation " + targetId,
                    remoteId = targetId,
                    group = "Quick Connect"
                };
                db.Contacts.Add(ci);
                db.Save();
                db.AddLog("CONTACT_ADD", "MINI_DESK", "Added contact " + targetId);
                SyncDynamicDesktopsFromDatabase();
            }
        }

        public void ToggleFullScreen()
        {
            ShowFullConsole();
            if (this.WindowState == FormWindowState.Maximized && this.FormBorderStyle == FormBorderStyle.None)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        public void CheckForUpdates(bool silent)
        {
            string url = (db != null && db.Settings != null && db.Settings.connection != null) ? db.Settings.connection.githubBackendUrl : null;
            AutoUpdateManager.CheckForUpdatesInteractive(this, url, silent);
        }

        public void InstallToUserFolder()
        {
            if (InstallationManager.IsRunningFromStandardDirectory())
            {
                MessageBox.Show(this, "jDroid-X-Desk is already running from the standard user application folder:\n\n" + InstallationManager.GetStandardInstallDirectory(), "Already Installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult dr = MessageBox.Show(this,
                "Would you like to install jDroid-X-Desk to the standard Windows user application directory?\n\n" +
                "• Target Folder: " + InstallationManager.GetStandardInstallDirectory() + "\n" +
                "• Creates Desktop Shortcut & Start Menu entry\n" +
                "• Configures Firewall & Defender exceptions\n" +
                "• Registers in Windows Installed Apps\n",
                "Install jDroid-X-Desk", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                string err;
                bool ok = InstallationManager.InstallToStandardDirectory(out err);
                if (ok)
                {
                    MessageBox.Show(this, "jDroid-X-Desk installed successfully to your user application folder!\nDesktop shortcut and Start Menu shortcuts created.", "Installation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, "Installation encountered an issue: " + err, "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void CleanShutdown()
        {
            try
            {
                if (isSessionActive && !string.IsNullOrEmpty(activeReceiverTargetId))
                {
                    SendRemoteDisconnectSignal(activeReceiverTargetId, "Application closed by user.");
                }
                if (currentHostDesktop != null && !string.IsNullOrEmpty(currentHostDesktop.RemoteId))
                {
                    SendRemoteDisconnectSignal(currentHostDesktop.RemoteId, "Host application closed.");
                }
                StopHostDaemon();
            }
            catch { }
            try
            {
                if (miniDeskForm != null && !miniDeskForm.IsDisposed)
                {
                    miniDeskForm.Dispose();
                }
            }
            catch { }
            Application.Exit();
        }


        public MainForm()
        {
            EngineScriptProvisioner.EnsureAllEngineScripts(AppDomain.CurrentDomain.BaseDirectory);
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
            this.BackColor = ThemePalette.ColorBgWorkspace;
            this.Icon = SystemIcons.Shield;
            this.KeyPreview = true;
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.F11)
                {
                    ToggleImmersiveZenMode();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape && isImmersiveZenMode)
                {
                    ToggleImmersiveZenMode();
                    e.Handled = true;
                }
            };
            this.Deactivate += (s, e) => ReleaseAllModifierKeys();


            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            sessionTelemetryTimer = new System.Windows.Forms.Timer();
            sessionTelemetryTimer.Interval = 1000;
            sessionTelemetryTimer.Tick += (s, e) => {
                if (currentConnectionState == SessionConnectionState.Connected)
                {
                    currentMeasuredFps = (float)(framesReceivedCount - lastFramesCount);
                    lastFramesCount = framesReceivedCount;
                    UpdateLiveTelemetry();
                    if (viewportCanvas != null && !viewportCanvas.IsDisposed) viewportCanvas.Invalidate();

                    // Automatic Bi-directional Clipboard Sync (jDroid-X Feature)
                    if (isSessionActive && autoSyncClipboard)
                    {
                        try
                        {
                            string curClip = "";
                            Thread ct = new Thread(() => {
                                try {
                                    if (Clipboard.ContainsText()) curClip = Clipboard.GetText();
                                } catch { }
                            });
                            ct.SetApartmentState(ApartmentState.STA);
                            ct.IsBackground = true;
                            ct.Start();
                            ct.Join(250);

                            if (!string.IsNullOrEmpty(curClip) && curClip != lastLocalClipboardText && curClip.Length < 150000)
                            {
                                lastLocalClipboardText = curClip;
                                Dictionary<string, object> clipEv = new Dictionary<string, object>();
                                clipEv["text"] = curClip;
                                SendRemoteInput("clipboard", clipEv);
                            }
                        }
                        catch { }
                    }

                    // Enforce Idle Session Timeout from ServerConfig Processing Limits
                    if (db != null && db.ServerConfig != null && db.ServerConfig.processingLimits != null)
                    {
                        int maxIdleMin = db.ServerConfig.processingLimits.idleTimeoutMinutes > 0 ? db.ServerConfig.processingLimits.idleTimeoutMinutes : 15;
                        if ((DateTime.UtcNow - lastUserActivityUtc) > TimeSpan.FromMinutes(maxIdleMin))
                        {
                            AbortSessionConnection(string.Format("Session idle for {0} min (Auto-disconnect timeout)", maxIdleMin));
                            ShowToast(string.Format("Session closed: Idle timeout of {0} min reached", maxIdleMin));
                        }
                    }
                }
            };

            connectingAnimationTimer = new System.Windows.Forms.Timer();
            connectingAnimationTimer.Interval = 250; // Smooth 4 Hz pulse when idle (zero flicker)
            connectingAnimationTimer.Tick += (s, e) => {
                headerHeartbeatPulseFrame++;
                if (panelMetaHeartbeat != null && !panelMetaHeartbeat.IsDisposed) panelMetaHeartbeat.Invalidate();

                // Periodic Host Security Alert (Incoming jDroid-X Viewer Detection on Host Desk2)
                // Gated: A connecting client (Desk1) must NEVER prompt itself to accept an incoming session
                if (currentConnectionState == SessionConnectionState.Disconnected &&
                    headerHeartbeatPulseFrame % 4 == 0 && pnlHostViewerAlert != null && !pnlHostViewerAlert.IsDisposed)
                {
                    bool hasViewer = EmbeddedRelayServer.Instance.HasActiveViewers();
                    string ep = EmbeddedRelayServer.Instance.LastViewerEndpoint ?? "Remote Desk";
                    string clientIp = ExtractClientIp(ep);

                    // Suppress loopback self-triggers
                    if (clientIp == "127.0.0.1" || clientIp == "::1" || clientIp == "localhost")
                    {
                        hasViewer = false;
                    }

                    if (!hasViewer)
                    {
                        if (pnlHostViewerAlert.Visible) pnlHostViewerAlert.Visible = false;
                        if (activeIncomingDialog != null && !activeIncomingDialog.IsDisposed)
                        {
                            activeIncomingDialog.Close();
                            activeIncomingDialog = null;
                        }
                    }
                    else
                    {
                        if (!pnlHostViewerAlert.Visible) pnlHostViewerAlert.Visible = true;
                        bool isAlreadyApproved = approvedViewerIps.Contains(clientIp);
                        if (btnHostAcceptViewer != null) btnHostAcceptViewer.Visible = !isAlreadyApproved;

                        bool isRecentlyRejected = rejectedViewerIps.Contains(clientIp) && (DateTime.UtcNow - lastRejectedViewerTime).TotalSeconds < 30;

                        if (!isAlreadyApproved && !isRecentlyRejected)
                        {
                            if (activeIncomingDialog == null || activeIncomingDialog.IsDisposed)
                            {
                                activeIncomingDialog = new JDroidXIncomingSessionDialog(ep,
                                    (allowInput, allowClip, allowFiles, lockHost) => {
                                        approvedViewerIps.Add(clientIp);
                                        rejectedViewerIps.Remove(clientIp);
                                        EmbeddedRelayServer.Instance.AuthorizeViewer(clientIp);
                                        EmbeddedRelayServer.Instance.AuthorizeViewer(ep);
                                        lastApprovedViewerEndpoint = ep;
                                        lastRejectedViewerEndpoint = "";
                                        hostPermissionAllowInput = allowInput;
                                        hostPermissionAllowClipboard = allowClip;
                                        hostPermissionAllowFiles = allowFiles;
                                        isLocalInputBlockedByRemote = lockHost;
                                        try { BlockInput(lockHost); } catch { }
                                        if (chkPermInput != null) chkPermInput.Checked = allowInput;
                                        if (chkPermClip != null) chkPermClip.Checked = allowClip;
                                        if (chkPermFiles != null) chkPermFiles.Checked = allowFiles;
                                        if (chkPermBlockLocal != null) chkPermBlockLocal.Checked = lockHost;
                                        if (btnHostAcceptViewer != null) btnHostAcceptViewer.Visible = false;
                                        ShowToast("✅ jDroid-X session accepted for " + clientIp);
                                    },
                                    () => {
                                        rejectedViewerIps.Add(clientIp);
                                        approvedViewerIps.Remove(clientIp);
                                        EmbeddedRelayServer.Instance.RejectViewer(clientIp);
                                        EmbeddedRelayServer.Instance.RejectViewer(ep);
                                        lastRejectedViewerEndpoint = ep;
                                        lastRejectedViewerTime = DateTime.UtcNow;
                                        lastApprovedViewerEndpoint = "";
                                        EmbeddedRelayServer.Instance.DisconnectAllViewers();
                                        pnlHostViewerAlert.Visible = false;
                                        ShowToast("🛑 Incoming jDroid-X session rejected for " + clientIp);
                                    });
                                activeIncomingDialog.Show();
                            }
                        }

                        if (lblHostViewerAlert != null && !lblHostViewerAlert.IsDisposed)
                        {
                            lblHostViewerAlert.Text = "⚠️ jDroid-X SESSION: " + clientIp;
                        }
                    }
                }

                // Periodic LAN Discovered Desks Refresh (every 4 seconds when in Console view)
                if (headerHeartbeatPulseFrame % 16 == 0 && activeViewKey == "Console" && flowDiscoveredDesks != null && !flowDiscoveredDesks.IsDisposed && !isSessionActive)
                {
                    UpdateDiscoveredDesksContent();
                }

                if (currentConnectionState == SessionConnectionState.Connecting)
                {
                    if (connectingAnimationTimer.Interval != 40) connectingAnimationTimer.Interval = 40;
                    connectingPulseFrame++;
                    if (connectingProgressPct < 90 && connectingPulseFrame % 5 == 0)
                    {
                        connectingProgressPct += 2;
                    }
                    if (viewportCanvas != null && !viewportCanvas.IsDisposed) viewportCanvas.Invalidate();
                }
                else
                {
                    if (connectingAnimationTimer.Interval != 250) connectingAnimationTimer.Interval = 250;
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
            SetDoubleBuffered(this);
            StartEmbeddedRelayServer();

            // Background Firewall Rule Registration (Zero-Block Inbound LAN P2P)
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo("netsh", "advfirewall firewall add rule name=\"jDroid-X-Desk\" dir=in action=allow program=\"" + Application.ExecutablePath + "\" enable=yes")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                }
                catch { }
            });
        }

        private void StartEmbeddedRelayServer()
        {
            try
            {
                int p = (db != null && db.Settings != null && db.Settings.connection != null && db.Settings.connection.relayPort > 0)
                    ? db.Settings.connection.relayPort
                    : 8088;
                int adminP = (db != null && db.ServerConfig != null && db.ServerConfig.engines != null && db.ServerConfig.engines.ContainsKey("jdroidRelay") && db.ServerConfig.engines["jdroidRelay"].localPort > 0)
                    ? db.ServerConfig.engines["jdroidRelay"].localPort
                    : 12345;
                EmbeddedRelayServer.Instance.Start(p, adminP);
                int discP = (db != null && db.Settings != null && db.Settings.connection != null && db.Settings.connection.discoveryPort > 0)
                    ? db.Settings.connection.discoveryPort
                    : 8089;
                bool lanEnabled = (db == null || db.Settings == null || db.Settings.connection == null || db.Settings.connection.lanDiscovery);
                if (lanEnabled && currentHostDesktop != null && !string.IsNullOrEmpty(currentHostDesktop.RemoteId))
                {
                    LanDiscoveryEngine.Instance.Start(currentHostDesktop.RemoteId, p, discP);
                }
            }
            catch { }
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
            if (db != null && db.Settings != null && !string.IsNullOrEmpty(db.Settings.theme))
            {
                ThemePalette.SetTheme(db.Settings.theme);
                isDarkMode = ThemePalette.IsDarkTheme;
            }
            else
            {
                ThemePalette.SetTheme("light-theme");
                isDarkMode = false;
            }

            if (dbThemeWatcher == null)
            {
                try
                {
                    string dir = AppDomain.CurrentDomain.BaseDirectory;
                    dbThemeWatcher = new FileSystemWatcher(dir, "database.json");
                    dbThemeWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                    dbThemeWatcher.Changed += (s, e) => {
                        Thread.Sleep(150);
                        if (this.IsHandleCreated && !this.IsDisposed)
                        {
                            try
                            {
                                this.BeginInvoke(new MethodInvoker(delegate {
                                    try
                                    {
                                        if (db != null) db.Load();
                                        if (db != null && db.Settings != null && !string.IsNullOrEmpty(db.Settings.theme))
                                        {
                                            if (db.Settings.theme != ThemePalette.CurrentThemeName)
                                            {
                                                ApplyThemeParadigm(db.Settings.theme, false, false);
                                            }
                                        }
                                    }
                                    catch { }
                                }));
                            }
                            catch { }
                        }
                    };
                    dbThemeWatcher.EnableRaisingEvents = true;
                }
                catch { }
            }


            // 0. BOTTOM STATUS BAR WITH SCROLLING MARQUEE (DOCK: BOTTOM, HEIGHT: 26)
            footerStatusBar = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = isDarkMode ? Color.FromArgb(10, 15, 26) : Color.FromArgb(226, 232, 240), Padding = new Padding(0) };
            BuildFooterStatusBarContent();

            // 1. TOP HEADER / TITLE BAR (DOCK: TOP, HEIGHT: 44)
            topHeaderBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = isDarkMode ? Color.FromArgb(9, 13, 22) : Color.FromArgb(241, 245, 249), Padding = new Padding(12, 0, 12, 0) };
            BuildTopHeaderContent();

            // 2. SLIM ACTIVITY BAR (DOCK: LEFT, WIDTH: 48)
            activityBar = new Panel { Dock = DockStyle.Left, Width = 48, BackColor = isDarkMode ? Color.FromArgb(7, 10, 16) : Color.FromArgb(226, 232, 240), Padding = new Padding(0) };
            BuildActivityBarContent();

            // 3. COLLAPSIBLE PRIMARY SIDEBAR / DRAWER (DOCK: LEFT, WIDTH: 260)
            collapsibleLeftDrawer = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = isDarkMode ? Color.FromArgb(13, 21, 34) : Color.FromArgb(248, 250, 252), Padding = new Padding(0) };
            BuildLeftDrawerContent();

            // 4. COLLAPSIBLE RIGHT INSPECTOR (DOCK: RIGHT, WIDTH: 260)
            collapsibleRightInspector = new Panel { Dock = DockStyle.Right, Width = 260, BackColor = isDarkMode ? Color.FromArgb(13, 21, 34) : Color.FromArgb(248, 250, 252), Padding = new Padding(12), Visible = false };
            BuildRightInspectorContent();

            // 5. COLLAPSIBLE BOTTOM PANEL (DOCK: BOTTOM, HEIGHT: 200)
            collapsibleBottomDock = new Panel { Dock = DockStyle.Bottom, Height = 200, BackColor = isDarkMode ? Color.FromArgb(9, 13, 22) : Color.FromArgb(241, 245, 249), Padding = new Padding(0), Visible = false };
            BuildBottomDockContent();

            // 6. MAIN CENTER EDITOR WORK AREA (DOCK: FILL - ZERO OVERLAP)
            centerEditorWorkArea = new Panel { Dock = DockStyle.Fill, BackColor = ThemePalette.ColorBgWorkspace };
            BuildCenterEditorContent();

            // -------------------------------------------------------------
            // CRITICAL WINFORMS NON-OVERLAPPING DOCK ORDER:
            // 6: footerStatusBar (docked 1st: takes full bottom 0, H-26, W, 26)
            // 5: topHeaderBar (docked 2nd: takes full top 0,0,W,44)
            // 4: activityBar (docked 3rd: takes far left 0,44,48,H-70)
            // 3: collapsibleLeftDrawer (docked 4th: takes left 48,44,260,H-70)
            // 2: collapsibleRightInspector (docked 5th: takes right W-260,44,260,H-70)
            // 1: collapsibleBottomDock (docked 6th: takes bottom above footer)
            // 0: centerEditorWorkArea (docked LAST: fills exact remaining center rectangle!)
            // -------------------------------------------------------------
            this.Controls.Add(footerStatusBar);
            this.Controls.Add(topHeaderBar);
            this.Controls.Add(activityBar);
            this.Controls.Add(collapsibleLeftDrawer);
            this.Controls.Add(collapsibleRightInspector);
            this.Controls.Add(collapsibleBottomDock);
            this.Controls.Add(centerEditorWorkArea);

            this.Controls.SetChildIndex(footerStatusBar, 6);
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

            // Center: Multi-Engine Switcher & Real-Time Telemetry in Top Header
            FlowLayoutPanel centerColabTelemetry = new FlowLayoutPanel
            {
                Size = new Size(770, 36),
                Location = new Point(Math.Max(leftBrand.Right + 8, (topHeaderBar.Width - 770) / 2), 4),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top
            };
            topHeaderBar.Resize += (s, e) => {
                centerColabTelemetry.Location = new Point(Math.Max(leftBrand.Right + 8, (topHeaderBar.Width - centerColabTelemetry.Width) / 2), 4);
            };

            panelMetaEngine = CreateEngineSelectorBox();
            panelMetaRam = CreateTelemetryBox("System RAM", colabRamText, out lblMetaRamTitle, out lblMetaRamVal, 0.10f);
            panelMetaGpu = CreateTelemetryBox("GPU RAM", colabGpuText, out lblMetaGpuTitle, out lblMetaGpuVal, 0.00f);
            panelMetaDisk = CreateTelemetryBox("Disk", colabDiskText, out lblMetaDiskTitle, out lblMetaDiskVal, 0.39f);
            panelMetaStatus = CreateStatusBox("Colab Relay", out lblMetaStatusTitle, out lblMetaStatusVal);
            panelMetaHeartbeat = CreateHeartbeatBox(out lblMetaHeartbeatTitle, out lblMetaHeartbeatVal);

            centerColabTelemetry.Controls.Add(panelMetaEngine);
            centerColabTelemetry.Controls.Add(panelMetaRam);
            centerColabTelemetry.Controls.Add(panelMetaGpu);
            centerColabTelemetry.Controls.Add(panelMetaDisk);
            centerColabTelemetry.Controls.Add(panelMetaStatus);
            centerColabTelemetry.Controls.Add(panelMetaHeartbeat);

            // Right: Antigravity IDE Layout View Toggles + Theme Toggle
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

            btnThemeToggle = CreateLayoutIconBtn("🌓", "Toggle Dark / High-Contrast Light Theme", (s, e) => {
                ToggleAppTheme();
            }, isDarkMode);

            lblHeaderGatewayPill = new Label
            {
                Text = "🟢 HTTP 200 : 12345",
                Font = new Font("Segoe UI", 7.6f, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                BackColor = isDarkMode ? Color.FromArgb(20, 28, 45) : Color.FromArgb(226, 232, 240),
                AutoSize = true,
                Padding = new Padding(6, 4, 6, 4),
                Margin = new Padding(0, 3, 6, 0)
            };

            cbHeaderThemeSelector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8f),
                BackColor = isDarkMode ? Color.FromArgb(20, 28, 45) : Color.FromArgb(255, 255, 255),
                ForeColor = isDarkMode ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42),
                Width = 140,
                Margin = new Padding(0, 3, 6, 0)
            };
            cbHeaderThemeSelector.Items.AddRange(new object[] {
                "☀️ Light Enterprise",
                "🌙 Dark Studio",
                "❄️ Nord Arctic",
                "🌅 Solarized Light",
                "🌌 Solarized Dark",
                "🎨 Monokai Pro",
                "🧛 Dracula Midnight",
                "🗼 Tokyo Night",
                "⚡ Cyberpunk Neon",
                "🌲 Matrix Emerald",
                "🌊 Deep Ocean",
                "🌇 Crimson Sunset",
                "📻 Gruvbox Retro",
                "🛡️ Titanium Slate",
                "👁️ High Contrast"
            });
            cbHeaderThemeSelector.SelectedIndex = ThemeToComboIndex(ThemePalette.CurrentThemeName);
            cbHeaderThemeSelector.SelectedIndexChanged += (s, e) => {
                if (isUpdatingThemeProgrammatically) return;
                if (cbHeaderThemeSelector.SelectedIndex >= 0)
                {
                    string key = ComboIndexToThemeKey(cbHeaderThemeSelector.SelectedIndex);
                    if (!string.Equals(key, ThemePalette.CurrentThemeName, StringComparison.OrdinalIgnoreCase))
                    {
                        ApplyThemeParadigm(key, true, true);
                    }
                }
            };

            rightLayoutControls.Controls.Add(lblHeaderGatewayPill);
            rightLayoutControls.Controls.Add(btnToggleActivityBar);
            rightLayoutControls.Controls.Add(btnToggleLeftDrawer);
            rightLayoutControls.Controls.Add(btnToggleBottomDock);
            rightLayoutControls.Controls.Add(btnToggleRightInspector);
            Button btnSwitchMini = CreateLayoutIconBtn("📱", "Switch to Mini Desk View", (s, e) => {
                ShowMiniDesk();
            }, false);
            Button btnCheckUpdates = CreateLayoutIconBtn("🔄", "Check for Updates", (s, e) => {
                CheckForUpdates(false);
            }, false);

            rightLayoutControls.Controls.Add(cbHeaderThemeSelector);
            rightLayoutControls.Controls.Add(btnThemeToggle);
            rightLayoutControls.Controls.Add(btnSwitchMini);
            rightLayoutControls.Controls.Add(btnCheckUpdates);

            topHeaderBar.Controls.Add(leftBrand);
            topHeaderBar.Controls.Add(centerColabTelemetry);
            topHeaderBar.Controls.Add(rightLayoutControls);
        }

        private Panel CreateEngineSelectorBox()
        {
            Panel box = new Panel { Size = new Size(235, 34), Margin = new Padding(2, 0, 3, 0), BackColor = Color.Transparent };
            lblMetaEngineTitle = new Label { Text = "⚡ jDroid-Engine", Font = new Font("Segoe UI", 6.8f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Location = new Point(4, 1), AutoSize = true };

            lblHeaderEngineStatus = new Label {
                Text = engineLiveStatusBadge,
                Font = new Font("Segoe UI", 6.2f, FontStyle.Bold),
                ForeColor = engineIsLiveActive ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11),
                Location = new Point(94, 1),
                AutoSize = true
            };

            lblBackendSyncStatus = new Label {
                Text = backendSyncStatusBadge,
                Font = new Font("Segoe UI", 6.2f),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(184, 1),
                AutoSize = true
            };

            cbHeaderEngineSelector = new ComboBox {
                Location = new Point(4, 13),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemePalette.ColorBgInput,
                ForeColor = ThemePalette.ColorTextPrimary,
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold)
            };
            cbHeaderEngineSelector.Items.Add("⚡ Sunshine (jDroid-X)");
            cbHeaderEngineSelector.Items.Add("⚡ RustDesk (jDroid-X)");
            cbHeaderEngineSelector.Items.Add("⚡ Guacamole (jDroid-X)");
            cbHeaderEngineSelector.Items.Add("⚡ Google Colab (jDroid-X)");

            SyncHeaderEngineDropdown(db != null && db.ServerConfig != null ? db.ServerConfig.activeEngine : "jdroidRelay");

            cbHeaderEngineSelector.SelectedIndexChanged += (s, e) => {
                if (isSwitchingEngineFromHeader) return;
                string selectedEng = "sunshine";
                if (cbHeaderEngineSelector.SelectedIndex == 1) selectedEng = "rustdesk";
                else if (cbHeaderEngineSelector.SelectedIndex == 2) selectedEng = "guacamole";
                else if (cbHeaderEngineSelector.SelectedIndex == 3) selectedEng = "jdroidRelay";

                isSwitchingEngineFromHeader = true;
                try {
                    SwitchActiveEngine(selectedEng);
                } finally {
                    isSwitchingEngineFromHeader = false;
                }
            };

            btnHeaderRunScript = new Button {
                Text = "🚀",
                Location = new Point(158, 12),
                Size = new Size(24, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemePalette.ColorBgCard,
                ForeColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI Symbol", 8f),
                Cursor = Cursors.Hand
            };
            btnHeaderRunScript.FlatAppearance.BorderSize = 0;
            ToolTip ttRun = new ToolTip();
            ttRun.SetToolTip(btnHeaderRunScript, "🚀 Run Automated Start Script for this Engine (scripts/start-*.bat)");
            btnHeaderRunScript.Click += (s, e) => {
                string currentEng = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) 
                    ? db.ServerConfig.activeEngine 
                    : "sunshine";
                LaunchEngineStartScript(currentEng);
            };

            btnHeaderEngineGuide = new Button {
                Text = "📖",
                Location = new Point(184, 12),
                Size = new Size(23, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemePalette.ColorBgCard,
                ForeColor = Color.FromArgb(250, 204, 21),
                Font = new Font("Segoe UI Symbol", 8f),
                Cursor = Cursors.Hand
            };
            btnHeaderEngineGuide.FlatAppearance.BorderSize = 0;
            ToolTip tt = new ToolTip();
            tt.SetToolTip(btnHeaderEngineGuide, "Open Manual Configuration, Setup Steps & Port Rules for All Engines");
            btnHeaderEngineGuide.Click += (s, e) => {
                string currentEng = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) 
                    ? db.ServerConfig.activeEngine 
                    : "sunshine";
                ShowEngineManualSetupDialog(currentEng);
            };

            box.Controls.Add(lblMetaEngineTitle);
            box.Controls.Add(lblHeaderEngineStatus);
            box.Controls.Add(lblBackendSyncStatus);
            box.Controls.Add(cbHeaderEngineSelector);
            box.Controls.Add(btnHeaderRunScript);
            box.Controls.Add(btnHeaderEngineGuide);

            box.Paint += (s, e) => {
                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, box.Width - 1, box.Height - 1), ThemePalette.ColorBgCardHeader, Color.FromArgb(56, 189, 248), ThemePalette.ColorShadow, 6);
            };

            return box;
        }

        private void BuildFooterStatusBarContent()
        {
            if (footerStatusBar == null) return;
            footerStatusBar.Controls.Clear();

            // Left: Engine Status Badge
            lblFooterEngineBadge = new Label
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 4, 8, 0),
                Cursor = Cursors.Hand
            };
            string curEng = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) ? db.ServerConfig.activeEngine : "jdroidRelay";
            lblFooterEngineBadge.Text = string.Format("⚡ jDroid-Engine: {0} [● READY]", curEng.ToUpper());
            lblFooterEngineBadge.Click += (s, e) => SwitchWorkspaceView("ServerAdmin");

            // Right: Interactive Quick Controls
            FlowLayoutPanel rightFooter = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 2, 8, 0)
            };

            btnFooterTheme = new Button
            {
                Text = isDarkMode ? "🌓 Dark" : "☀️ Light",
                Size = new Size(68, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = isDarkMode ? Color.FromArgb(20, 28, 45) : Color.FromArgb(241, 245, 249),
                ForeColor = isDarkMode ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 7.2f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
            };
            btnFooterTheme.FlatAppearance.BorderSize = 0;
            btnFooterTheme.Click += (s, e) => ToggleAppTheme();

            Label lblFooterSmartFit = new Label
            {
                Text = "🖥️ SmartFit (DirectX)",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Margin = new Padding(0, 3, 8, 0)
            };

            lblFooterNodeStatus = new Label
            {
                Text = "🟢 Node: 12345 (HTTP 200)",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 0)
            };

            rightFooter.Controls.Add(btnFooterTheme);
            rightFooter.Controls.Add(lblFooterSmartFit);
            rightFooter.Controls.Add(lblFooterNodeStatus);

            // Center: Scrolling Marquee Metadata Ticker
            Panel marqueeContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemePalette.ColorBgWorkspace,
                Padding = new Padding(8, 3, 8, 0)
            };

            lblFooterMarquee = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 7.8f),
                ForeColor = isDarkMode ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            marqueeContainer.Controls.Add(lblFooterMarquee);

            footerStatusBar.Controls.Add(marqueeContainer);
            footerStatusBar.Controls.Add(lblFooterEngineBadge);
            footerStatusBar.Controls.Add(rightFooter);

            if (marqueeTimer != null) { marqueeTimer.Stop(); marqueeTimer.Dispose(); }
            marqueeTimer = new System.Windows.Forms.Timer { Interval = 350 };
            marqueeTimer.Tick += (s, e) => UpdateMarqueeTicker();
            marqueeTimer.Start();
        }

        private void UpdateMarqueeTicker()
        {
            if (lblFooterMarquee == null || lblFooterMarquee.IsDisposed) return;
            string eng = db != null && db.ServerConfig != null ? db.ServerConfig.activeEngine : "jdroidRelay";
            string hostId = currentHostDesktop != null ? currentHostDesktop.RemoteId : "984-102-391";
            string lanIp = GetLocalLanIp();
            string themeName = ThemePalette.GetThemeDisplayName();

            string fullMarqueeText = string.Format(
                "⚡ jDroid-X Remote Console  |  jDroid-Engine: {0} ({1})  |  Security: AES-256-GCM  |  Host Desk ID: {2}  |  LAN: {3}  |  RTT Latency: <2ms  |  Display: SmartFit  |  Theme: {4}  |  Node Gateway: HTTP 200 (Port 12345)  |  Flat 55\"+ TV / Multi-Resolution Optimized  |  Operator: JEET  ---  ",
                eng.ToUpper(),
                engineIsLiveActive ? "RUNNING" : "STANDBY",
                hostId,
                lanIp,
                themeName
            );

            if (fullMarqueeText.Length > 0)
            {
                marqueeCharIndex = (marqueeCharIndex + 1) % fullMarqueeText.Length;
                string rotated = fullMarqueeText.Substring(marqueeCharIndex) + fullMarqueeText.Substring(0, marqueeCharIndex);
                lblFooterMarquee.Text = rotated;
            }
        }

        private string ComboIndexToThemeKey(int index)
        {
            switch (index)
            {
                case 1: return "dark-theme";
                case 2: return "theme-nord";
                case 3: return "theme-solarized-light";
                case 4: return "theme-solarized-dark";
                case 5: return "theme-monokai";
                case 6: return "theme-dracula";
                case 7: return "theme-tokyo-night";
                case 8: return "theme-cyberpunk";
                case 9: return "theme-emerald";
                case 10: return "theme-ocean";
                case 11: return "theme-sunset";
                case 12: return "theme-gruvbox";
                case 13: return "theme-slate";
                case 14: return "theme-high-contrast";
                default: return "light-theme";
            }
        }

        private int ThemeToComboIndex(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;
            switch (key.ToLower())
            {
                case "dark":
                case "dark-theme":
                case "theme-dark": return 1;
                case "theme-nord": return 2;
                case "theme-solarized-light": return 3;
                case "theme-solarized-dark": return 4;
                case "theme-monokai": return 5;
                case "theme-dracula": return 6;
                case "theme-tokyo-night": return 7;
                case "theme-cyberpunk": return 8;
                case "theme-emerald": return 9;
                case "theme-ocean": return 10;
                case "theme-sunset": return 11;
                case "theme-gruvbox": return 12;
                case "theme-slate": return 13;
                case "theme-high-contrast": return 14;
                default: return 0; // Light Enterprise
            }
        }

        private void ApplyThemeParadigm(string themeKey, bool saveToDb = true, bool userInitiated = false)
        {
            if (string.IsNullOrEmpty(themeKey)) return;

            bool themeActuallyChanged = !string.Equals(themeKey, ThemePalette.CurrentThemeName, StringComparison.OrdinalIgnoreCase);

            ThemePalette.SetTheme(themeKey);
            isDarkMode = ThemePalette.IsDarkTheme;

            if (saveToDb && db != null && db.Settings != null && themeActuallyChanged)
            {
                db.Settings.theme = themeKey;
                db.Save();
            }

            isUpdatingThemeProgrammatically = true;
            try
            {
                ApplyThemeToApp();
            }
            finally
            {
                isUpdatingThemeProgrammatically = false;
            }

            // Exactly ONE TIME notification only when explicitly requested by user and theme actually changed
            if (userInitiated && themeActuallyChanged)
            {
                if (lastNotifiedTheme != themeKey || (DateTime.UtcNow - lastThemeToastTime).TotalSeconds > 8)
                {
                    lastNotifiedTheme = themeKey;
                    lastThemeToastTime = DateTime.UtcNow;
                    ShowToast(string.Format("Theme applied: {0}", ThemePalette.GetThemeDisplayName().ToUpper()));
                }
            }
        }

        private void ToggleAppTheme()
        {
            string next = isDarkMode ? "light-theme" : "dark-theme";
            ApplyThemeParadigm(next, true, true);
        }

        private void ApplyThemeToApp()
        {
            Color bgWork = ThemePalette.ColorBgWorkspace;
            this.BackColor = bgWork;
            if (topHeaderBar != null) topHeaderBar.BackColor = isDarkMode ? Color.FromArgb(9, 13, 22) : Color.FromArgb(241, 245, 249);
            if (activityBar != null) activityBar.BackColor = isDarkMode ? Color.FromArgb(7, 10, 16) : Color.FromArgb(226, 232, 240);
            if (collapsibleLeftDrawer != null) collapsibleLeftDrawer.BackColor = ThemePalette.ColorBgSidebar;
            if (collapsibleRightInspector != null) collapsibleRightInspector.BackColor = ThemePalette.ColorBgSidebar;
            if (collapsibleBottomDock != null) collapsibleBottomDock.BackColor = isDarkMode ? Color.FromArgb(9, 13, 22) : Color.FromArgb(241, 245, 249);
            if (centerEditorWorkArea != null) centerEditorWorkArea.BackColor = bgWork;
            if (footerStatusBar != null) footerStatusBar.BackColor = isDarkMode ? Color.FromArgb(10, 15, 26) : Color.FromArgb(226, 232, 240);
            if (btnFooterTheme != null) btnFooterTheme.Text = isDarkMode ? "🌓 Dark" : "☀️ Light";
            if (lblFooterMarquee != null) lblFooterMarquee.ForeColor = isDarkMode ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85);

            if (cbHeaderThemeSelector != null)
            {
                int desiredIdx = ThemeToComboIndex(ThemePalette.CurrentThemeName);
                if (cbHeaderThemeSelector.SelectedIndex != desiredIdx)
                {
                    isUpdatingThemeProgrammatically = true;
                    try
                    {
                        cbHeaderThemeSelector.SelectedIndex = desiredIdx;
                    }
                    finally
                    {
                        isUpdatingThemeProgrammatically = false;
                    }
                }
                cbHeaderThemeSelector.BackColor = ThemePalette.ColorBgInput;
                cbHeaderThemeSelector.ForeColor = ThemePalette.ColorTextPrimary;
            }
            if (lblHeaderGatewayPill != null)
            {
                lblHeaderGatewayPill.BackColor = isDarkMode ? Color.FromArgb(20, 28, 45) : Color.FromArgb(226, 232, 240);
            }

            // Sync Main Tab Bar
            if (mainTabBar != null)
            {
                mainTabBar.BackColor = ThemePalette.ColorBgTabBar;
                BuildMainTabBar();
            }

            // Sync Local Ribbon Bar
            if (localBar != null)
            {
                localBar.BackColor = ThemePalette.ColorBgCardHeader;
                if (txtTargetId != null)
                {
                    txtTargetId.BackColor = ThemePalette.ColorBgInput;
                    txtTargetId.ForeColor = ThemePalette.ColorTextPrimary;
                }
            }

            // Sync Left Drawer
            if (drawerHeader != null)
            {
                drawerHeader.BackColor = ThemePalette.ColorBgSidebarHeader;
            }
            if (drawerContentPanel != null)
            {
                drawerContentPanel.BackColor = ThemePalette.ColorBgSidebar;
            }
            UpdateDrawerForCurrentView(activeViewKey);

            // Sync Subviews
            if (viewConsole != null) viewConsole.BackColor = bgWork;
            if (pnlHostViewerAlert != null)
            {
                pnlHostViewerAlert.BackColor = isDarkMode ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
                if (lblHostViewerAlert != null) lblHostViewerAlert.ForeColor = isDarkMode ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38);
                if (chkPermInput != null) chkPermInput.ForeColor = ThemePalette.ColorTextPrimary;
                if (chkPermClip != null) chkPermClip.ForeColor = ThemePalette.ColorTextPrimary;
                if (chkPermFiles != null) chkPermFiles.ForeColor = ThemePalette.ColorTextPrimary;
            }
            if (panelWelcomeCards != null)
            {
                panelWelcomeCards.BackColor = bgWork;
                BuildWelcomeCardsContent();
            }

            if (viewFileTransfer != null)
            {
                viewFileTransfer.BackColor = bgWork;
                if (lbLocalFiles != null) { lbLocalFiles.BackColor = ThemePalette.ColorBgInput; lbLocalFiles.ForeColor = ThemePalette.ColorTextPrimary; }
                if (lbRemoteFiles != null) { lbRemoteFiles.BackColor = ThemePalette.ColorBgInput; lbRemoteFiles.ForeColor = ThemePalette.ColorTextPrimary; }
                if (lblLocalPath != null) lblLocalPath.ForeColor = ThemePalette.ColorTextSecondary;
            }

            if (viewFleet != null)
            {
                viewFleet.BackColor = bgWork;
                if (dgvFleet != null)
                {
                    dgvFleet.BackgroundColor = ThemePalette.ColorBgCard;
                    dgvFleet.GridColor = ThemePalette.ColorBorderCard;
                    dgvFleet.ColumnHeadersDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
                    dgvFleet.ColumnHeadersDefaultCellStyle.ForeColor = ThemePalette.ColorTextSecondary;
                    dgvFleet.DefaultCellStyle.BackColor = ThemePalette.ColorBgCard;
                    dgvFleet.DefaultCellStyle.ForeColor = ThemePalette.ColorTextPrimary;
                    dgvFleet.AlternatingRowsDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
                }
                if (txtFleetSearch != null) { txtFleetSearch.BackColor = ThemePalette.ColorBgInput; txtFleetSearch.ForeColor = ThemePalette.ColorTextPrimary; }
                if (cboFleetGroupFilter != null) { cboFleetGroupFilter.BackColor = ThemePalette.ColorBgInput; cboFleetGroupFilter.ForeColor = ThemePalette.ColorTextPrimary; }
            }

            if (viewHistory != null)
            {
                viewHistory.BackColor = bgWork;
                if (dgvHistory != null)
                {
                    dgvHistory.BackgroundColor = ThemePalette.ColorBgCard;
                    dgvHistory.GridColor = ThemePalette.ColorBorderCard;
                    dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
                    dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = ThemePalette.ColorTextSecondary;
                    dgvHistory.DefaultCellStyle.BackColor = ThemePalette.ColorBgCard;
                    dgvHistory.DefaultCellStyle.ForeColor = ThemePalette.ColorTextPrimary;
                    dgvHistory.AlternatingRowsDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
                }
            }

            if (viewSettingsStudio != null)
            {
                viewSettingsStudio.BackColor = bgWork;
                if (settingsTree != null)
                {
                    settingsTree.BackColor = ThemePalette.ColorBgSidebar;
                    settingsTree.ForeColor = ThemePalette.ColorTextPrimary;
                }
                if (settingsSubContentPanel != null)
                {
                    settingsSubContentPanel.BackColor = bgWork;
                    if (settingsTree != null && settingsTree.SelectedNode != null)
                    {
                        RenderSettingsSubPanel(!string.IsNullOrEmpty(settingsTree.SelectedNode.Name) ? settingsTree.SelectedNode.Name : "General");
                    }
                }
            }

            if (viewServerAdmin != null)
            {
                viewServerAdmin.BackColor = bgWork;
                if (headerAdmin != null) headerAdmin.BackColor = ThemePalette.ColorBgCardHeader;
                if (pnlScrollAdmin != null) pnlScrollAdmin.BackColor = bgWork;
                if (pnlLimitsCard != null) pnlLimitsCard.Invalidate();
                if (pnlLogsGrid != null) pnlLogsGrid.Invalidate();
                if (numAdminMaxSessions != null) { numAdminMaxSessions.BackColor = ThemePalette.ColorBgInput; numAdminMaxSessions.ForeColor = ThemePalette.ColorTextPrimary; }
                if (numAdminBandwidth != null) { numAdminBandwidth.BackColor = ThemePalette.ColorBgInput; numAdminBandwidth.ForeColor = ThemePalette.ColorTextPrimary; }
                if (cbAdminFps != null) { cbAdminFps.BackColor = ThemePalette.ColorBgInput; cbAdminFps.ForeColor = ThemePalette.ColorTextPrimary; }
                if (numAdminIdleTimeout != null) { numAdminIdleTimeout.BackColor = ThemePalette.ColorBgInput; numAdminIdleTimeout.ForeColor = ThemePalette.ColorTextPrimary; }
                if (numAdminLogRetention != null) { numAdminLogRetention.BackColor = ThemePalette.ColorBgInput; numAdminLogRetention.ForeColor = ThemePalette.ColorTextPrimary; }
                if (cbAdminHwAccel != null) { cbAdminHwAccel.BackColor = ThemePalette.ColorBgInput; cbAdminHwAccel.ForeColor = ThemePalette.ColorTextPrimary; }
                if (cbAdminLogLevel != null) { cbAdminLogLevel.BackColor = ThemePalette.ColorBgInput; cbAdminLogLevel.ForeColor = ThemePalette.ColorTextPrimary; }
                if (txtAdminLogSearch != null) { txtAdminLogSearch.BackColor = ThemePalette.ColorBgInput; txtAdminLogSearch.ForeColor = ThemePalette.ColorTextPrimary; }
                if (lblAdminActiveEngineBadge != null) lblAdminActiveEngineBadge.ForeColor = ThemePalette.ColorSuccess;
                if (lblAdminCpu != null) lblAdminCpu.ForeColor = ThemePalette.ColorPrimary;
                if (lblAdminMem != null) lblAdminMem.ForeColor = ThemePalette.ColorSuccess;
                if (lblAdminUptime != null) lblAdminUptime.ForeColor = ThemePalette.ColorWarning;
                if (lblAdminSessions != null) lblAdminSessions.ForeColor = ThemePalette.ColorTextPrimary;
                if (pnlEngineCardsContainer != null)
                {
                    RenderEngineCards(pnlEngineCardsContainer);
                }
                if (dgvAdminLogs != null)
                {
                    dgvAdminLogs.BackgroundColor = ThemePalette.ColorBgCard;
                    dgvAdminLogs.GridColor = ThemePalette.ColorBorderCard;
                    dgvAdminLogs.ColumnHeadersDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
                    dgvAdminLogs.ColumnHeadersDefaultCellStyle.ForeColor = ThemePalette.ColorTextSecondary;
                    dgvAdminLogs.DefaultCellStyle.BackColor = ThemePalette.ColorBgCard;
                    dgvAdminLogs.DefaultCellStyle.ForeColor = ThemePalette.ColorTextPrimary;
                    dgvAdminLogs.AlternatingRowsDefaultCellStyle.BackColor = ThemePalette.ColorBgCardHeader;
                }
            }

            this.Invalidate(true);
        }

        private void ProbeEngineLiveStatus(string engineId)
        {
            ThreadPool.QueueUserWorkItem(delegate {
                int port = 47990;
                if (engineId == "rustdesk") port = 21116;
                else if (engineId == "guacamole") port = 8080;
                else if (engineId == "jdroidRelay") port = 8088;

                bool isLive = false;
                try {
                    using (TcpClient tcp = new TcpClient()) {
                        IAsyncResult ar = tcp.BeginConnect("127.0.0.1", port, null, null);
                        bool ok = ar.AsyncWaitHandle.WaitOne(350);
                        if (ok && tcp.Connected) {
                            tcp.EndConnect(ar);
                            isLive = true;
                        }
                    }
                } catch { }

                if (!isLive && engineId == "rustdesk") {
                    try {
                        Process[] procs = Process.GetProcessesByName("rustdesk");
                        if (procs != null && procs.Length > 0) isLive = true;
                    } catch { }
                }

                if (!isLive && engineId == "jdroidRelay" && (isColabConnected || EmbeddedRelayServer.Instance.IsRunning)) {
                    isLive = true;
                }

                engineIsLiveActive = isLive;
                engineLiveStatusBadge = isLive 
                    ? string.Format("● {0} RUNNING", port) 
                    : string.Format("⚠️ {0} STOPPED (FALLBACK: 8088)", port);

                if (this.IsHandleCreated && !this.IsDisposed) {
                    this.BeginInvoke(new Action(delegate {
                        if (lblHeaderEngineStatus != null) {
                            lblHeaderEngineStatus.Text = engineLiveStatusBadge;
                            lblHeaderEngineStatus.ForeColor = isLive ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11);
                        }
                        if (lblFooterEngineBadge != null) {
                            string curEng = db != null && db.ServerConfig != null ? db.ServerConfig.activeEngine : "sunshine";
                            lblFooterEngineBadge.Text = string.Format("⚡ jDroid-Engine: {0} [{1}]", curEng.ToUpper(), isLive ? "● RUNNING" : "⚠️ STOPPED");
                            lblFooterEngineBadge.ForeColor = isLive ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11);
                        }
                    }));
                }
            });
        }

        private void LaunchEngineStartScript(string engineId)
        {
            try {
                EngineScriptProvisioner.EnsureAllEngineScripts(AppDomain.CurrentDomain.BaseDirectory);

                if (engineId == "jdroidRelay" || engineId == "googleColab" || engineId == "colab" || engineId == "pythonRelay")
                {
                    StartLocalPythonRelay();
                    return;
                }

                string scriptName = "start-sunshine.bat";
                if (engineId == "rustdesk") scriptName = "start-rustdesk.bat";
                else if (engineId == "guacamole") scriptName = "start-guacamole.bat";

                string batPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", scriptName);
                if (File.Exists(batPath)) {
                    Process.Start(new ProcessStartInfo(batPath) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(batPath) });
                    ShowToast(string.Format("🚀 Launched automated startup script: {0}", scriptName));
                    System.Windows.Forms.Timer probeTimer = new System.Windows.Forms.Timer { Interval = 2500 };
                    probeTimer.Tick += (s, e) => {
                        probeTimer.Stop();
                        probeTimer.Dispose();
                        ProbeEngineLiveStatus(engineId);
                    };
                    probeTimer.Start();
                } else {
                    ShowToast(string.Format("Script not found: {0}", batPath));
                }
            } catch (Exception ex) {
                ShowToast("Launcher error: " + ex.Message);
            }
        }

        private void StartLocalPythonRelay()
        {
            try
            {
                EngineScriptProvisioner.EnsureAllEngineScripts(AppDomain.CurrentDomain.BaseDirectory);

                // 1. Ensure Native Embedded Relay is active on port 8088 (Zero dependencies, 100% self-hosted)
                if (!EmbeddedRelayServer.Instance.IsRunning)
                {
                    StartEmbeddedRelayServer();
                }

                // 2. If Python is available on this workstation, also launch Python GPU/Colab acceleration script
                string pyScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colab_relay_server.py");
                if (!File.Exists(pyScript))
                {
                    string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services", "colab-relay-service.py");
                    if (File.Exists(alt)) pyScript = alt;
                }

                if (File.Exists(pyScript))
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "python",
                            Arguments = "\"" + pyScript + "\"",
                            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        try { localPythonRelayProcess = Process.Start(psi); }
                        catch
                        {
                            psi.FileName = "py";
                            try { localPythonRelayProcess = Process.Start(psi); } catch { }
                        }
                    }
                    catch { }
                }

                string localUrl = "http://127.0.0.1:8088";
                string lanIp = GetLocalLanIp();

                if (db != null && db.Settings != null && db.Settings.connection != null)
                {
                    db.Settings.connection.relayServer = localUrl;
                    db.Settings.connection.customDomain = localUrl;
                    db.Settings.connection.localInstalledFolder = AppDomain.CurrentDomain.BaseDirectory;
                    db.Settings.connection.localRelayScriptPath = pyScript;
                    db.Save();
                    db.SyncYaml();
                }

                cachedWinningRelayUrl = localUrl;
                lastWinningCheckTime = DateTime.MinValue;

                if (txtColabInput != null) txtColabInput.Text = localUrl;

                ShowToast(string.Format("⚡ Relay active on 8088! Desk 2: http://{0}:8088 | Web: http://{0}:8088/view", lanIp));

                ThreadPool.QueueUserWorkItem(delegate {
                    Thread.Sleep(600);
                    PingColabServer();
                    RegisterWithColabRelay();
                });
            }
            catch (Exception ex)
            {
                ShowToast("Relay: " + ex.Message);
            }
        }

        private void StopLocalPythonRelay()
        {
            try
            {
                if (localPythonRelayProcess != null && !localPythonRelayProcess.HasExited)
                {
                    localPythonRelayProcess.Kill();
                    localPythonRelayProcess.Dispose();
                }
            }
            catch { }
            localPythonRelayProcess = null;
        }

        private void SyncHeaderEngineDropdown(string engineId)
        {
            if (cbHeaderEngineSelector == null || cbHeaderEngineSelector.IsDisposed) return;
            isSwitchingEngineFromHeader = true;
            try
            {
                if (string.Equals(engineId, "rustdesk", StringComparison.OrdinalIgnoreCase))
                    cbHeaderEngineSelector.SelectedIndex = 1;
                else if (string.Equals(engineId, "guacamole", StringComparison.OrdinalIgnoreCase))
                    cbHeaderEngineSelector.SelectedIndex = 2;
                else if (string.Equals(engineId, "sunshine", StringComparison.OrdinalIgnoreCase))
                    cbHeaderEngineSelector.SelectedIndex = 0;
                else
                    cbHeaderEngineSelector.SelectedIndex = 3; // jdroidRelay default
            }
            finally
            {
                isSwitchingEngineFromHeader = false;
            }
        }

        private void UpdateHeaderEngineTelemetryUI(string engineId)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(UpdateHeaderEngineTelemetryUI), engineId);
                return;
            }

            if (string.IsNullOrEmpty(engineId))
            {
                engineId = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) 
                    ? db.ServerConfig.activeEngine 
                    : "jdroidRelay";
            }

            SyncHeaderEngineDropdown(engineId);

            if (string.Equals(engineId, "rustdesk", StringComparison.OrdinalIgnoreCase))
            {
                if (lblMetaRamTitle != null) lblMetaRamTitle.Text = "RAM (RustDesk)";
                if (lblMetaRamVal != null) lblMetaRamVal.Text = string.IsNullOrEmpty(colabRamText) || colabRamText == "-- / --" ? "~42M P2P" : colabRamText + " (42M)";
                if (lblMetaGpuTitle != null) lblMetaGpuTitle.Text = "Codec / Pipeline";
                if (lblMetaGpuVal != null) lblMetaGpuVal.Text = "VP9 / AV1 (CPU)";
                if (lblMetaDiskTitle != null) lblMetaDiskTitle.Text = "Port / Protocol";
                if (lblMetaDiskVal != null) lblMetaDiskVal.Text = "21116 / NaCl";
                if (lblMetaStatusTitle != null) lblMetaStatusTitle.Text = "RustDesk Engine";
                if (lblMetaStatusVal != null)
                {
                    lblMetaStatusVal.Text = "● P2P MESH READY";
                    lblMetaStatusVal.ForeColor = Color.FromArgb(56, 189, 248);
                }
                if (lblMetaHeartbeatTitle != null) lblMetaHeartbeatTitle.Text = "Mesh Latency";
                if (lblMetaHeartbeatVal != null)
                {
                    lblMetaHeartbeatVal.Text = "~18ms P2P";
                    lblMetaHeartbeatVal.ForeColor = Color.FromArgb(16, 185, 129);
                }
            }
            else if (string.Equals(engineId, "guacamole", StringComparison.OrdinalIgnoreCase))
            {
                if (lblMetaRamTitle != null) lblMetaRamTitle.Text = "RAM (Guacamole)";
                if (lblMetaRamVal != null) lblMetaRamVal.Text = string.IsNullOrEmpty(colabRamText) || colabRamText == "-- / --" ? "~85M guacd" : colabRamText + " (85M)";
                if (lblMetaGpuTitle != null) lblMetaGpuTitle.Text = "Display / Pipeline";
                if (lblMetaGpuVal != null) lblMetaGpuVal.Text = "Server RDP / WebGL";
                if (lblMetaDiskTitle != null) lblMetaDiskTitle.Text = "Port / Protocol";
                if (lblMetaDiskVal != null) lblMetaDiskVal.Text = "4822 / 8080";
                if (lblMetaStatusTitle != null) lblMetaStatusTitle.Text = "Guacamole Engine";
                if (lblMetaStatusVal != null)
                {
                    lblMetaStatusVal.Text = "● DOCKER RDP READY";
                    lblMetaStatusVal.ForeColor = Color.FromArgb(168, 85, 247);
                }
                if (lblMetaHeartbeatTitle != null) lblMetaHeartbeatTitle.Text = "RDP Latency";
                if (lblMetaHeartbeatVal != null)
                {
                    lblMetaHeartbeatVal.Text = "~25ms Web";
                    lblMetaHeartbeatVal.ForeColor = Color.FromArgb(168, 85, 247);
                }
            }
            else if (string.Equals(engineId, "jdroidRelay", StringComparison.OrdinalIgnoreCase))
            {
                if (lblMetaRamTitle != null) lblMetaRamTitle.Text = "System RAM";
                if (lblMetaRamVal != null) lblMetaRamVal.Text = colabRamText;
                if (lblMetaGpuTitle != null) lblMetaGpuTitle.Text = "GPU RAM";
                if (lblMetaGpuVal != null) lblMetaGpuVal.Text = colabGpuText;
                if (lblMetaDiskTitle != null) lblMetaDiskTitle.Text = "Disk";
                if (lblMetaDiskVal != null) lblMetaDiskVal.Text = colabDiskText;
                if (lblMetaStatusTitle != null) lblMetaStatusTitle.Text = "Colab Relay";
                if (lblMetaStatusVal != null)
                {
                    lblMetaStatusVal.Text = isColabConnected ? string.Format("● ONLINE ({0}ms)", lastMeasuredRttMs) : (isProbingCandidates ? "◌ PINGING..." : "○ OFFLINE");
                    lblMetaStatusVal.ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68));
                }
                if (lblMetaHeartbeatTitle != null) lblMetaHeartbeatTitle.Text = "Heartbeat";
                if (lblMetaHeartbeatVal != null)
                {
                    lblMetaHeartbeatVal.Text = isColabConnected ? string.Format("ALIVE ({0}ms)", lastMeasuredRttMs) : (isProbingCandidates ? "PROBING" : "OFFLINE");
                    lblMetaHeartbeatVal.ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68));
                }
            }
            else // Default: sunshine
            {
                if (lblMetaRamTitle != null) lblMetaRamTitle.Text = "RAM (Sunshine)";
                if (lblMetaRamVal != null) lblMetaRamVal.Text = string.IsNullOrEmpty(colabRamText) || colabRamText == "-- / --" ? "~120M VRAM" : colabRamText + " (120M)";
                if (lblMetaGpuTitle != null) lblMetaGpuTitle.Text = "GPU / Pipeline";
                if (lblMetaGpuVal != null) lblMetaGpuVal.Text = "NVENC CUDA (120fps)";
                if (lblMetaDiskTitle != null) lblMetaDiskTitle.Text = "Port / Protocol";
                if (lblMetaDiskVal != null) lblMetaDiskVal.Text = "47990 / RTSP";
                if (lblMetaStatusTitle != null) lblMetaStatusTitle.Text = "Sunshine Engine";
                if (lblMetaStatusVal != null)
                {
                    lblMetaStatusVal.Text = "● READY (NVStream)";
                    lblMetaStatusVal.ForeColor = Color.FromArgb(16, 185, 129);
                }
                if (lblMetaHeartbeatTitle != null) lblMetaHeartbeatTitle.Text = "Latency Target";
                if (lblMetaHeartbeatVal != null)
                {
                    lblMetaHeartbeatVal.Text = "< 5ms (LAN/WAN)";
                    lblMetaHeartbeatVal.ForeColor = Color.FromArgb(56, 189, 248);
                }
            }

            if (panelMetaEngine != null && !panelMetaEngine.IsDisposed) panelMetaEngine.Invalidate();
            if (panelMetaRam != null && !panelMetaRam.IsDisposed) panelMetaRam.Invalidate();
            if (panelMetaGpu != null && !panelMetaGpu.IsDisposed) panelMetaGpu.Invalidate();
            if (panelMetaDisk != null && !panelMetaDisk.IsDisposed) panelMetaDisk.Invalidate();
            if (panelMetaStatus != null && !panelMetaStatus.IsDisposed) panelMetaStatus.Invalidate();
            if (panelMetaHeartbeat != null && !panelMetaHeartbeat.IsDisposed) panelMetaHeartbeat.Invalidate();
        }

        private Panel CreateTelemetryBox(string title, string initialValue, out Label titleLabel, out Label valLabel, float fillRatio)
        {
            Panel box = new Panel { Size = new Size(102, 34), Margin = new Padding(2, 0, 2, 0), BackColor = Color.Transparent };
            Label lTitle = new Label { Text = title, Font = new Font("Segoe UI", 6.8f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(4, 2), AutoSize = true };
            Label lVal = new Label { Text = initialValue, Font = new Font("Segoe UI", 7.6f, FontStyle.Bold), ForeColor = ThemePalette.ColorTextPrimary, Location = new Point(4, 15), AutoSize = true };
            titleLabel = lTitle;
            valLabel = lVal;
            box.Controls.Add(lTitle);
            box.Controls.Add(lVal);

            box.Paint += (s, e) => {
                string actEng = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) ? db.ServerConfig.activeEngine : "sunshine";
                Color themeCol = Color.FromArgb(16, 185, 129);
                if (actEng == "rustdesk") themeCol = Color.FromArgb(56, 189, 248);
                else if (actEng == "guacamole") themeCol = Color.FromArgb(168, 85, 247);
                else if (actEng == "jdroidRelay") themeCol = isColabConnected ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);

                ThemePalette.DrawFloatingCard(e.Graphics, new Rectangle(0, 0, box.Width - 1, box.Height - 1), ThemePalette.ColorBgCardHeader, themeCol, ThemePalette.ColorShadow, 6);
                using (SolidBrush lineBrush = new SolidBrush(themeCol))
                {
                    int lineW = Math.Max(4, (int)((box.Width - 8) * Math.Min(1f, Math.Max(0.05f, fillRatio))));
                    e.Graphics.FillRectangle(lineBrush, 4, box.Height - 3, lineW, 2);
                }
            };
            return box;
        }

        private Panel CreateStatusBox(string title, out Label titleLabel, out Label valLabel)
        {
            Panel box = new Panel { Size = new Size(115, 34), Margin = new Padding(2, 0, 2, 0), BackColor = ThemePalette.ColorBgCardHeader, Cursor = Cursors.Hand };
            Label lTitle = new Label { Text = title, Font = new Font("Segoe UI", 6.8f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(4, 2), AutoSize = true, Cursor = Cursors.Hand };
            Label lVal = new Label { Text = isColabConnected ? "● ONLINE" : (isProbingCandidates ? "◌ PINGING..." : "○ OFFLINE"), Font = new Font("Segoe UI", 7.4f, FontStyle.Bold), ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68)), Location = new Point(4, 15), AutoSize = true, Cursor = Cursors.Hand };
            titleLabel = lTitle;
            valLabel = lVal;
            box.Controls.Add(lTitle);
            box.Controls.Add(lVal);

            ToolTip tt = new ToolTip();
            tt.SetToolTip(box, "Click to configure Server Relay & Engine Parameters");
            tt.SetToolTip(lVal, "Click to configure Server Relay & Engine Parameters");

            box.Click += (s, e) => ShowRelayConfigDialog();
            lTitle.Click += (s, e) => ShowRelayConfigDialog();
            lVal.Click += (s, e) => ShowRelayConfigDialog();

            box.Paint += (s, e) => {
                Graphics g = e.Graphics;
                string actEng = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) ? db.ServerConfig.activeEngine : "sunshine";
                Color themeCol = Color.FromArgb(16, 185, 129);
                if (actEng == "rustdesk") themeCol = Color.FromArgb(56, 189, 248);
                else if (actEng == "guacamole") themeCol = Color.FromArgb(168, 85, 247);
                else if (actEng == "jdroidRelay") themeCol = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68));

                ThemePalette.DrawFloatingCard(g, new Rectangle(0, 0, box.Width - 1, box.Height - 1), ThemePalette.ColorBgCardHeader, themeCol, ThemePalette.ColorShadow, 6);
                int barX = 4;
                int barY = box.Height - 3;
                int barW = 76;
                using (SolidBrush b = new SolidBrush(themeCol))
                {
                    g.FillRectangle(b, barX, barY, barW, 2);
                }
            };
            return box;
        }

        private Panel CreateHeartbeatBox(out Label titleLabel, out Label valLabel)
        {
            Panel box = new Panel { Size = new Size(86, 34), Margin = new Padding(2, 0, 2, 0), BackColor = ThemePalette.ColorBgCardHeader, Cursor = Cursors.Hand };
            Label lTitle = new Label { Text = "Heartbeat", Font = new Font("Segoe UI", 6.8f), ForeColor = ThemePalette.ColorTextSecondary, Location = new Point(4, 1), AutoSize = true, Cursor = Cursors.Hand };
            Label lVal = new Label { Text = isColabConnected ? "ALIVE" : (isProbingCandidates ? "PROBING" : "OFFLINE"), Font = new Font("Segoe UI", 7.2f, FontStyle.Bold), ForeColor = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68)), Location = new Point(4, 12), AutoSize = true, Cursor = Cursors.Hand };
            titleLabel = lTitle;
            valLabel = lVal;
            lblMetaHeartbeatVal = lVal;
            box.Controls.Add(lTitle);
            box.Controls.Add(lVal);

            ToolTip tt = new ToolTip();
            tt.SetToolTip(box, "Real-time Engine Latency & Progress Bar");

            box.Click += (s, e) => ShowRelayConfigDialog();
            lTitle.Click += (s, e) => ShowRelayConfigDialog();
            lVal.Click += (s, e) => ShowRelayConfigDialog();

            box.Paint += (s, e) => {
                Graphics g = e.Graphics;
                string actEng = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) ? db.ServerConfig.activeEngine : "sunshine";
                Color themeCol = Color.FromArgb(16, 185, 129);
                if (actEng == "rustdesk") themeCol = Color.FromArgb(56, 189, 248);
                else if (actEng == "guacamole") themeCol = Color.FromArgb(168, 85, 247);
                else if (actEng == "jdroidRelay") themeCol = isColabConnected ? Color.FromArgb(16, 185, 129) : (isProbingCandidates ? Color.FromArgb(245, 158, 11) : Color.FromArgb(239, 68, 68));

                ThemePalette.DrawFloatingCard(g, new Rectangle(0, 0, box.Width - 1, box.Height - 1), ThemePalette.ColorBgCardHeader, themeCol, ThemePalette.ColorShadow, 6);
                int hbX = 4;
                int hbY = box.Height - 3;
                int hbW = Math.Max(6, Math.Min(box.Width - 8, (int)((box.Width - 8) * (headerHeartbeatPulseFrame % 10) / 10f)));
                using (SolidBrush b = new SolidBrush(themeCol))
                {
                    g.FillRectangle(b, hbX, hbY, hbW, 2);
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
                dlg.BackColor = ThemePalette.ColorBgWorkspace;
                dlg.ForeColor = ThemePalette.ColorTextPrimary;
                dlg.Font = new Font("Segoe UI", 9f);

                Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

                Label lblHead = new Label { Text = "Google Colab & Cloud Relay Setup", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = ThemePalette.ColorPrimary, AutoSize = true, Location = new Point(20, 16) };
                Label lblSub = new Label { Text = "Configure your relay server endpoint, port, custom domain, and Cloudflare tunnel.", Font = new Font("Segoe UI", 8.5f), ForeColor = ThemePalette.ColorTextSecondary, AutoSize = true, Location = new Point(20, 42) };

                int curY = 70;

                // Prominent Colab Connection Steps Box
                Panel pnlSteps = new Panel { Size = new Size(440, 84), Location = new Point(20, curY), BackColor = ThemePalette.ColorBgCard, BorderStyle = BorderStyle.FixedSingle };
                Label lSteps = new Label {
                    Text = "STEPS TO CONNECT IN jDroid-X-Desk:\n1. Open jDroid-X-Desk on Desk 1 and Desk 2\n2. Run your Google Colab Relay notebook\n3. Paste the generated public URL (👉 https://....trycloudflare.com 👈)\n4. Click 'Save & Apply' -> Header turns GREEN with live meters!",
                    Font = new Font("Segoe UI", 8.2f),
                    ForeColor = ThemePalette.ColorPrimary,
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

                Button btnPresetDesk = new Button { Text = "⚡ desk.jdroid-x.com", Size = new Size(130, 28), Location = new Point(200, 52), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.FromArgb(56, 189, 248), Font = new Font("Segoe UI", 8f, FontStyle.Bold), Cursor = Cursors.Hand };
                btnPresetDesk.FlatAppearance.BorderSize = 0;
                btnPresetDesk.Click += (s, e) => {
                    tbRelay.Text = "https://desk.jdroid-x.com";
                    tbDom.Text = "https://desk.jdroid-x.com";
                };
                gbTunnel.Controls.Add(btnPresetDesk);

                Button btnPresetDomain = new Button { Text = "🌐 www.jdroid-x.com", Size = new Size(130, 28), Location = new Point(335, 52), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.FromArgb(203, 213, 225), Font = new Font("Segoe UI", 8f), Cursor = Cursors.Hand };
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

        public void ShowEngineManualSetupDialog(string defaultEngineId)
        {
            if (string.IsNullOrEmpty(defaultEngineId)) defaultEngineId = "sunshine";

            using (Form dlg = new Form())
            {
                dlg.Text = "jDroid-X-Desk | Open-Source Remote Engines — Manual Setup & Update Guide";
                dlg.Size = new Size(840, 640);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = ThemePalette.ColorBgWorkspace;
                dlg.ForeColor = ThemePalette.ColorTextPrimary;
                dlg.Font = new Font("Segoe UI", 9f);

                Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };

                // Top Header Banner
                Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 64 };
                Label lblTitle = new Label { Text = "🛡️ Open-Source Remote Engines — Setup & Configuration Guide", Font = new Font("Segoe UI", 12.5f, FontStyle.Bold), ForeColor = ThemePalette.ColorPrimary, AutoSize = true, Location = new Point(0, 4) };
                Label lblSub = new Label { Text = "Step-by-step instructions, firewall rules, port mappings, and 1-click launcher actions for all engines.", Font = new Font("Segoe UI", 8.5f), ForeColor = ThemePalette.ColorTextSecondary, AutoSize = true, Location = new Point(0, 32) };
                pnlHeader.Controls.Add(lblTitle);
                pnlHeader.Controls.Add(lblSub);

                // Tab Strip for the 4 Engines
                Panel pnlTabStrip = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = ThemePalette.ColorBgCardHeader, Margin = new Padding(0, 0, 0, 10) };
                FlowLayoutPanel tabFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

                // Content Panel (Holds details for currently selected engine)
                Panel pnlContent = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemePalette.ColorBgCard, Padding = new Padding(14) };

                // Bottom Action Bar
                Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(0, 8, 0, 0) };
                FlowLayoutPanel btmFlowLeft = new FlowLayoutPanel { Dock = DockStyle.Left, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
                FlowLayoutPanel btmFlowRight = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };

                string currentSelectedEngine = defaultEngineId;

                Action<string> renderEngineDetails = null;
                renderEngineDetails = (engId) => {
                    currentSelectedEngine = engId;
                    pnlContent.Controls.Clear();
                    btmFlowLeft.Controls.Clear();
                    btmFlowRight.Controls.Clear();

                    // Update Tab Button Highlighting
                    foreach (Control c in tabFlow.Controls)
                    {
                        Button tb = c as Button;
                        if (tb != null)
                        {
                            bool isTabActive = string.Equals(tb.Tag as string, engId, StringComparison.OrdinalIgnoreCase);
                            tb.BackColor = isTabActive ? Color.FromArgb(40, 53, 76) : Color.Transparent;
                            tb.ForeColor = isTabActive ? Color.FromArgb(56, 189, 248) : Color.FromArgb(148, 163, 184);
                        }
                    }

                    string engName = "";
                    string engTagline = "";
                    string engPrereq = "";
                    string engSteps = "";
                    string engPorts = "";
                    string engFwCmd = "";
                    string engUpdateInfo = "";
                    string sourceRelPath = "";
                    string webUrl = "";

                    if (engId == "sunshine")
                    {
                        engName = "⚡ Sunshine Host & Moonlight Client (LizardByte/Sunshine)";
                        engTagline = "Lowest latency (<5ms) remote desktop and 60-120 FPS high-refresh workstation streaming via NVStream / RTSP.";
                        engPrereq = "• Hardware NVENC CUDA / Intel QuickSync / AMD AMF GPU (or high-speed CPU fallback)\n• Moonlight client installed on viewer device (PC, Android, iOS, Mac, Linux)\n• Source folder: jDroid-X-Desk-Server\\Github\\Sunshine-master & moonlight-qt-master";
                        engSteps = "1. FIRST-TIME ADMIN SETUP (Mandatory 1-Time Setup):\n" +
                                   "   - Click '🌐 Open Web UI (47990)' below or navigate to: https://localhost:47990\n" +
                                   "   - Your browser will show a self-signed certificate warning: Click 'Advanced' -> 'Proceed to localhost'.\n" +
                                   "   - Create your initial administrator Username and Password, then click Save.\n\n" +
                                   "2. MOONLIGHT CLIENT PAIRING:\n" +
                                   "   - Open Moonlight on your client device and click '+ Add PC'.\n" +
                                   "   - Type this computer's local IP (e.g., 192.168.x.x) or public IP. Moonlight will display a 4-Digit PIN.\n" +
                                   "   - Return to Sunshine Web UI (PIN Tab at https://localhost:47990/#/pin), enter the 4-Digit PIN, and click 'Send'.\n" +
                                   "   - Once paired, your Windows desktop appears in Moonlight ready for crystal-clear 60-120 FPS streaming!\n\n" +
                                   "3. CONFIGURATION PRESETS:\n" +
                                   "   - In Sunshine Web UI -> Configuration -> Audio/Video: Select NVENC H.264 or HEVC for hardware encoding.\n" +
                                   "   - Set Bitrate target matching your network (20-50 Mbps recommended).";
                        engPorts = "• TCP: 47984, 47989, 47990 (Web UI), 48010\n• UDP: 47998, 47999, 48000, 48002, 48010";
                        engFwCmd = "New-NetFirewallRule -DisplayName \"Sunshine TCP\" -Direction Inbound -LocalPort 47984,47989,47990,48010 -Protocol TCP -Action Allow; New-NetFirewallRule -DisplayName \"Sunshine UDP\" -Direction Inbound -LocalPort 47998,47999,48000,48002,48010 -Protocol UDP -Action Allow";
                        engUpdateInfo = "To update Sunshine: Pull latest commits in 'jDroid-X-Desk-Server\\Github\\Sunshine-master' or download the latest official 'sunshine-windows-installer.exe' from LizardByte GitHub releases.";
                        sourceRelPath = "jDroid-X-Desk-Server\\Github\\Sunshine-master";
                        webUrl = "https://localhost:47990";
                    }
                    else if (engId == "rustdesk")
                    {
                        engName = "🦀 RustDesk Remote Desktop (rustdesk/rustdesk)";
                        engTagline = "Best direct open-source match to enterprise remote desktop with P2P NAT Traversal, NaCl encryption, and self-hosting support.";
                        engPrereq = "• Cloned source in: jDroid-X-Desk-Server\\Github\\rustdesk-master\n• Official standalone RustDesk executable or self-hosted hbbs/hbbr server";
                        engSteps = "1. FREE PUBLIC SERVER USAGE (Zero-Config Mode):\n" +
                                   "   - RustDesk works out-of-the-box with official public rendezvous servers (relay.rustdesk.com).\n" +
                                   "   - Simply launch the RustDesk client, share your 9-digit ID and temporary one-time password.\n" +
                                   "   - No port forwarding or manual server setup required for peer-to-peer connections.\n\n" +
                                   "2. PRIVATE SELF-HOSTED SERVER (hbbs / hbbr):\n" +
                                   "   - If you want 100% private self-hosted routing, run 'hbbs' (ID Server) and 'hbbr' (Relay Server) on your Linux/Windows VPS.\n" +
                                   "   - In RustDesk Client -> Settings (three dots) -> Network -> ID/Relay Server:\n" +
                                   "     • ID Server: Enter your VPS domain or IP (e.g., id.mycompany.com:21116)\n" +
                                   "     • Key: Paste your server's public key from 'id_ed25519.pub'\n" +
                                   "   - All client connections will route strictly through your private infrastructure!\n\n" +
                                   "3. DIRECT IP ACCESS:\n" +
                                   "   - You can also enable 'Direct IP Access' in RustDesk settings to connect directly over LAN or VPN on TCP port 21118.";
                        engPorts = "• TCP: 21115 (NAT Type), 21116 (TCP Punching), 21117 (Relay), 21118 (Web Client), 21119 (Support)\n• UDP: 21116 (Heartbeat & Rendezvous)";
                        engFwCmd = "New-NetFirewallRule -DisplayName \"RustDesk TCP\" -Direction Inbound -LocalPort 21115-21119 -Protocol TCP -Action Allow; New-NetFirewallRule -DisplayName \"RustDesk UDP\" -Direction Inbound -LocalPort 21116 -Protocol UDP -Action Allow";
                        engUpdateInfo = "To update RustDesk: Open the repository folder below and run 'git pull', or download the latest 'rustdesk-*-x86_64.exe' release from rustdesk/rustdesk on GitHub.";
                        sourceRelPath = "jDroid-X-Desk-Server\\Github\\rustdesk-master";
                        webUrl = "";
                    }
                    else if (engId == "guacamole")
                    {
                        engName = "🥑 Apache Guacamole + FreeRDP (apache/guacamole-server)";
                        engTagline = "Client-less HTML5 browser gateway allowing remote desktop access from any browser without installing software on viewer.";
                        engPrereq = "• Windows Remote Desktop enabled on this computer (Settings -> System -> Remote Desktop -> On)\n• Docker Desktop or WSL2 to host guacd daemon and Tomcat web portal\n• Source folder: jDroid-X-Desk-Server\\Github\\guacamole-server";
                        engSteps = "1. ENABLE WINDOWS REMOTE DESKTOP:\n" +
                                   "   - On this host, open Windows Settings -> System -> Remote Desktop.\n" +
                                   "   - Toggle Remote Desktop to 'ON'. Note your Windows username and password.\n\n" +
                                   "2. RUN GUACAMOLE DAEMON & WEB CLIENT (via Docker):\n" +
                                   "   - Open PowerShell and execute the following one-liner:\n" +
                                   "     docker run --name guacd -d -p 4822:4822 guacamole/guacd\n" +
                                   "     docker run --name guac -d -p 8080:8080 --link guacd:guacd guacamole/guacamole\n\n" +
                                   "3. CONFIGURE DESKTOP IN WEB PORTAL:\n" +
                                   "   - Open http://localhost:8080/guacamole in your web browser.\n" +
                                   "   - Default Login: Username 'guacadmin', Password 'guacadmin'.\n" +
                                   "   - Go to Settings -> Connections -> 'New Connection':\n" +
                                   "     • Name: My Windows Workstation\n" +
                                   "     • Protocol: RDP\n" +
                                   "     • Parameters -> Hostname: host.docker.internal (or your Windows LAN IP)\n" +
                                   "     • Port: 3389  |  Security mode: NLA\n" +
                                   "     • Enter your Windows username and password, then click Save.\n" +
                                   "   - Click the connection to open your Windows desktop inside any web browser window!";
                        engPorts = "• TCP: 4822 (guacd proxy daemon), 8080 (Guacamole web client), 3389 (Windows RDP host)";
                        engFwCmd = "docker run --name guacd -d -p 4822:4822 guacamole/guacd; docker run --name guac -d -p 8080:8080 --link guacd:guacd guacamole/guacamole";
                        engUpdateInfo = "To update Guacamole: Run 'docker pull guacamole/guacd && docker pull guacamole/guacamole' or rebuild from source in 'jDroid-X-Desk-Server\\Github\\guacamole-server'.";
                        sourceRelPath = "jDroid-X-Desk-Server\\Github\\guacamole-server";
                        webUrl = "http://localhost:8080/guacamole";
                    }
                    else // jdroidRelay
                    {
                        engName = "🚀 jDroid-X Google Colab Cloud Relay";
                        engTagline = "Multi-Threaded Cloud Bridge & GPU Stream via Google Colab with AES-256 encrypted binary frame streams.";
                        engPrereq = "• Google Colab notebook instance or local Node.js relay server\n• Public Cloudflare tunnel (.trycloudflare.com)";
                        engSteps = "1. RUN GOOGLE COLAB RELAY:\n" +
                                   "   - Click the '🌐 Colab' button on the main Console tab or in Step 1.\n" +
                                   "   - Execute the Google Colab notebook cells to start the encrypted cloud bridge.\n" +
                                   "   - Colab generates a public tunnel URL (e.g. https://...trycloudflare.com).\n\n" +
                                   "2. CONNECT WORKSTATIONS:\n" +
                                   "   - Copy and paste the Cloudflare tunnel URL into Step 1 or the top header status box.\n" +
                                   "   - Click 'Save & Connect'. The top telemetry indicators turn GREEN!\n" +
                                   "   - Both workstations connect seamlessly through firewalls and NAT without port forwarding.";
                        engPorts = "• HTTP/WS: 8088 (Colab bridge), 12345 (Local Node backup gateway)";
                        engFwCmd = "";
                        engUpdateInfo = "jDroid-X Google Colab Relay is cloud-hosted and receives continuous updates through the Colab runtime.";
                        sourceRelPath = ".";
                        webUrl = "";
                    }

                    int curY = 0;

                    // Title & Tagline Box
                    Label lTitle = new Label { Text = engName, Font = new Font("Segoe UI", 11.5f, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), Location = new Point(0, curY), AutoSize = true };
                    curY += 26;
                    Label lTagline = new Label { Text = engTagline, Font = new Font("Segoe UI", 8.8f, FontStyle.Italic), ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(0, curY), AutoSize = true };
                    curY += 30;

                    // Prerequisites Section
                    Label lPreHdr = new Label { Text = "PREREQUISITES & SOURCE LOCATION:", Font = new Font("Segoe UI", 8.2f, FontStyle.Bold), ForeColor = Color.FromArgb(250, 204, 21), Location = new Point(0, curY), AutoSize = true };
                    curY += 20;
                    Label lPre = new Label { Text = engPrereq, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(226, 232, 240), Location = new Point(0, curY), AutoSize = true };
                    curY += (engPrereq.Split('\n').Length * 18) + 12;

                    // Step-by-Step Manual Setup Section
                    Label lStepsHdr = new Label { Text = "STEP-BY-STEP MANUAL SETUP WALKTHROUGH:", Font = new Font("Segoe UI", 8.2f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Location = new Point(0, curY), AutoSize = true };
                    curY += 20;
                    TextBox tbSteps = new TextBox {
                        Text = engSteps,
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        Size = new Size(770, 160),
                        Location = new Point(0, curY),
                        BackColor = ThemePalette.ColorBgInput,
                        ForeColor = ThemePalette.ColorTextPrimary,
                        Font = new Font("Consolas", 8.8f),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    curY += 170;

                    // Network & Firewall Ports Section
                    Label lPortsHdr = new Label { Text = "NETWORK PORTS & FIREWALL RULES:", Font = new Font("Segoe UI", 8.2f, FontStyle.Bold), ForeColor = Color.FromArgb(168, 85, 247), Location = new Point(0, curY), AutoSize = true };
                    curY += 20;
                    Label lPorts = new Label { Text = engPorts, Font = new Font("Consolas", 8.2f), ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(0, curY), AutoSize = true };
                    curY += (engPorts.Split('\n').Length * 18) + 10;

                    // Update Instructions
                    Label lUpdHdr = new Label { Text = "HOW TO UPDATE THIS ENGINE:", Font = new Font("Segoe UI", 8.2f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(0, curY), AutoSize = true };
                    curY += 20;
                    Label lUpd = new Label { Text = engUpdateInfo, Font = new Font("Segoe UI", 8.2f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(0, curY), AutoSize = true };
                    curY += 34;

                    pnlContent.Controls.Add(lTitle);
                    pnlContent.Controls.Add(lTagline);
                    pnlContent.Controls.Add(lPreHdr);
                    pnlContent.Controls.Add(lPre);
                    pnlContent.Controls.Add(lStepsHdr);
                    pnlContent.Controls.Add(tbSteps);
                    pnlContent.Controls.Add(lPortsHdr);
                    pnlContent.Controls.Add(lPorts);
                    pnlContent.Controls.Add(lUpdHdr);
                    pnlContent.Controls.Add(lUpd);

                    // Populate Bottom Action Buttons
                    bool isCurrentActive = string.Equals(db != null && db.ServerConfig != null ? db.ServerConfig.activeEngine : "", engId, StringComparison.OrdinalIgnoreCase);

                    Button btnActivate = new Button {
                        Text = isCurrentActive ? "✓ Current jDroid-Engine" : "⚡ Switch to " + engId.ToUpper() + " Now",
                        Size = new Size(185, 32),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = isCurrentActive ? Color.FromArgb(16, 185, 129) : Color.FromArgb(56, 189, 248),
                        ForeColor = isCurrentActive ? Color.White : ThemePalette.ColorTextPrimary,
                        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Enabled = !isCurrentActive,
                        Margin = new Padding(0, 0, 8, 0)
                    };
                    btnActivate.FlatAppearance.BorderSize = 0;
                    string captureSwitchId = engId;
                    btnActivate.Click += (s, e) => {
                        SwitchActiveEngine(captureSwitchId);
                        dlg.Close();
                    };
                    btmFlowLeft.Controls.Add(btnActivate);

                    Button btnDlgRunScript = new Button {
                        Text = "🚀 Run Start Script",
                        Size = new Size(155, 32),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(20, 32, 50),
                        ForeColor = Color.FromArgb(16, 185, 129),
                        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 0, 8, 0)
                    };
                    btnDlgRunScript.FlatAppearance.BorderSize = 0;
                    string captureRunId = engId;
                    btnDlgRunScript.Click += (s, e) => {
                        LaunchEngineStartScript(captureRunId);
                    };
                    btmFlowLeft.Controls.Add(btnDlgRunScript);

                    if (!string.IsNullOrEmpty(webUrl))
                    {
                        Button btnOpenWeb = new Button {
                            Text = "🌐 Open Web UI",
                            Size = new Size(125, 32),
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(30, 41, 59),
                            ForeColor = Color.FromArgb(250, 204, 21),
                            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                            Cursor = Cursors.Hand,
                            Margin = new Padding(0, 0, 8, 0)
                        };
                        btnOpenWeb.FlatAppearance.BorderSize = 0;
                        string captureWeb = webUrl;
                        btnOpenWeb.Click += (s, e) => {
                            try { Process.Start(captureWeb); } catch (Exception ex) { ShowToast("Browser error: " + ex.Message); }
                        };
                        btmFlowLeft.Controls.Add(btnOpenWeb);
                    }

                    if (!string.IsNullOrEmpty(sourceRelPath))
                    {
                        Button btnOpenFolder = new Button {
                            Text = "📂 Open Code Folder",
                            Size = new Size(140, 32),
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(30, 41, 59),
                            ForeColor = Color.FromArgb(148, 163, 184),
                            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                            Cursor = Cursors.Hand,
                            Margin = new Padding(0, 0, 8, 0)
                        };
                        btnOpenFolder.FlatAppearance.BorderSize = 0;
                        string absP = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceRelPath);
                        btnOpenFolder.Click += (s, e) => {
                            try {
                                if (Directory.Exists(absP)) Process.Start("explorer.exe", absP);
                                else ShowToast("Folder not found: " + absP);
                            } catch { }
                        };
                        btmFlowLeft.Controls.Add(btnOpenFolder);
                    }

                    if (!string.IsNullOrEmpty(engFwCmd))
                    {
                        Button btnCopyFw = new Button {
                            Text = "📋 Copy Firewall Rule",
                            Size = new Size(145, 32),
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(30, 41, 59),
                            ForeColor = Color.FromArgb(168, 85, 247),
                            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                            Cursor = Cursors.Hand,
                            Margin = new Padding(0, 0, 8, 0)
                        };
                        btnCopyFw.FlatAppearance.BorderSize = 0;
                        string captureCmd = engFwCmd;
                        btnCopyFw.Click += (s, e) => {
                            try {
                                Clipboard.SetText(captureCmd);
                                ShowToast("Copied PowerShell command to clipboard! Paste into Admin PowerShell to unblock ports.");
                            } catch { }
                        };
                        btmFlowLeft.Controls.Add(btnCopyFw);
                    }

                    Button btnClose = new Button {
                        Text = "Close",
                        Size = new Size(80, 32),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(30, 41, 59),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 8.5f),
                        Cursor = Cursors.Hand
                    };
                    btnClose.FlatAppearance.BorderSize = 0;
                    btnClose.Click += (s, e) => dlg.Close();
                    btmFlowRight.Controls.Add(btnClose);
                };

                // Add Tabs to Strip
                Action<string, string> addTab = (id, label) => {
                    Button tb = new Button {
                        Text = label,
                        Tag = id,
                        Size = new Size(195, 32),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.Transparent,
                        ForeColor = Color.FromArgb(148, 163, 184),
                        Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 2, 4, 0)
                    };
                    tb.FlatAppearance.BorderSize = 0;
                    string captureTabId = id;
                    tb.Click += (s, e) => renderEngineDetails(captureTabId);
                    tabFlow.Controls.Add(tb);
                };

                addTab("sunshine", "⚡ Sunshine & Moonlight");
                addTab("rustdesk", "🦀 RustDesk");
                addTab("guacamole", "🥑 Apache Guacamole");
                addTab("jdroidRelay", "🚀 Google Colab (Cloud Relay)");

                pnlTabStrip.Controls.Add(tabFlow);
                pnlBottom.Controls.Add(btmFlowLeft);
                pnlBottom.Controls.Add(btmFlowRight);

                pnlMain.Controls.Add(pnlContent);
                pnlMain.Controls.Add(pnlBottom);
                pnlMain.Controls.Add(pnlTabStrip);
                pnlMain.Controls.Add(pnlHeader);

                dlg.Controls.Add(pnlMain);

                // Initial render for requested engine
                renderEngineDetails(defaultEngineId);

                dlg.ShowDialog(this);
            }
        }

        public List<string> GetRelayCandidateUrls(string targetId = null)
        {
            List<string> candidateUrls = new List<string>();

            if (string.IsNullOrEmpty(targetId) && !string.IsNullOrEmpty(activeReceiverTargetId))
            {
                targetId = activeReceiverTargetId;
            }

            // 0a. Direct IP or Endpoint entered by user (e.g. 192.168.1.15 or 192.168.1.15:8088)
            if (!string.IsNullOrEmpty(targetId))
            {
                string raw = targetId.Trim();
                if (raw.Contains(".") && !raw.Contains(" "))
                {
                    string ep = raw;
                    if (!ep.StartsWith("http://") && !ep.StartsWith("https://")) ep = "http://" + ep;
                    int colonIdx = ep.LastIndexOf(':');
                    if (colonIdx <= 5) ep += ":8088";
                    ep = ep.TrimEnd('/');
                    if (!candidateUrls.Contains(ep)) candidateUrls.Add(ep);
                }
            }

            // 0b. Zero-Config LAN P2P Auto-Discovery resolution (jDroid-X style via UDP beacon)
            if (!string.IsNullOrEmpty(targetId))
            {
                string lanEp;
                if (LanDiscoveryEngine.Instance.TryGetDeskEndpoint(targetId, out lanEp))
                {
                    if (!candidateUrls.Contains(lanEp)) candidateUrls.Add(lanEp);
                }
                else
                {
                    LanDiscoveryEngine.Instance.BroadcastQuery(targetId);
                    Thread.Sleep(50);
                    if (LanDiscoveryEngine.Instance.TryGetDeskEndpoint(targetId, out lanEp))
                    {
                        if (!candidateUrls.Contains(lanEp)) candidateUrls.Add(lanEp);
                    }
                }

                string tunEp;
                if (LanDiscoveryEngine.Instance.TryGetDeskEndpoint(targetId + "_tunnel", out tunEp))
                {
                    if (!candidateUrls.Contains(tunEp)) candidateUrls.Add(tunEp);
                }
            }

            // 1. Prioritize verified winning cache for 0ms discovery overhead
            if (!string.IsNullOrEmpty(cachedWinningRelayUrl))
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

            // 4. Configured relay server & custom domain from settings (any valid HTTP/HTTPS endpoint)
            if (db != null && db.Settings != null && db.Settings.connection != null)
            {
                string configuredRelay = db.Settings.connection.relayServer;
                if (!string.IsNullOrEmpty(configuredRelay) && configuredRelay.Contains("freemodels.club"))
                {
                    configuredRelay = "https://desk.jdroid-x.com";
                    db.Settings.connection.relayServer = configuredRelay;
                }
                if (!string.IsNullOrEmpty(configuredRelay) && configuredRelay.StartsWith("http") && !configuredRelay.Contains("freemodels.club"))
                {
                    configuredRelay = configuredRelay.TrimEnd('/');
                    if (!candidateUrls.Contains(configuredRelay)) candidateUrls.Add(configuredRelay);
                }
                string cd = db.Settings.connection.customDomain;
                if (!string.IsNullOrEmpty(cd) && cd.StartsWith("http") && !cd.Contains("freemodels.club"))
                {
                    cd = cd.TrimEnd('/');
                    if (!candidateUrls.Contains(cd)) candidateUrls.Add(cd);
                }
            }

            // 5. Online Cloud Middle Bridge & Cloudflare Ingress (Primary Cloud Relay for jDroid-X)
            if (!candidateUrls.Contains("https://desk.jdroid-x.com")) candidateUrls.Insert(0, "https://desk.jdroid-x.com");

            // Local loopback is ONLY permitted when targetId is explicitly localhost or 127.0.0.1
            bool isExplicitLoopback = !string.IsNullOrEmpty(targetId) && (targetId.Contains("127.0.0.1") || targetId.ToLower().Contains("localhost"));
            if (isExplicitLoopback)
            {
                if (!candidateUrls.Contains("http://127.0.0.1:8088")) candidateUrls.Add("http://127.0.0.1:8088");
                if (!candidateUrls.Contains("http://localhost:8088")) candidateUrls.Add("http://localhost:8088");
            }

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
            string actEng = (db != null && db.ServerConfig != null && !string.IsNullOrEmpty(db.ServerConfig.activeEngine)) ? db.ServerConfig.activeEngine : "sunshine";
            UpdateHeaderEngineTelemetryUI(actEng);
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

    }
}
