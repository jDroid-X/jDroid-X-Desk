// ==============================================================================
// jDroid-X-Desk Native Windows Application (C# / Win32 Machine Executable)
// Architecture: Native WinForms with Embedded Browser Engine & Tray Management
// Compiler: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
// ==============================================================================

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace JDroidXDesk
{
    public class MainWindow : Form
    {
        private WebBrowser browser;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private Process nodeServerProcess;
        private string appUrl = "http://localhost:12345";

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                SetProcessDPIAware();
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }

        public MainWindow()
        {
            InitializeComponent();
            EnsureNodeServerRunning();
            SetupTrayIcon();
        }

        private void InitializeComponent()
        {
            this.Text = "jDroid-X-Desk | Unified Remote Desktop Control Console";
            this.Size = new Size(1380, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(960, 600);
            this.BackColor = Color.FromArgb(244, 246, 249);

            // Set browser emulation to IE11 / Edge standard in registry if possible
            ConfigureWebBrowserRegistry();

            browser = new WebBrowser();
            browser.Dock = DockStyle.Fill;
            browser.ScriptErrorsSuppressed = true;
            browser.IsWebBrowserContextMenuEnabled = false;
            browser.Navigate(appUrl);

            this.Controls.Add(browser);
            this.FormClosing += MainWindow_FormClosing;
        }

        private void EnsureNodeServerRunning()
        {
            bool isPortOpen = false;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(appUrl + "/api/settings");
                req.Timeout = 1000;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    if (resp.StatusCode == HttpStatusCode.OK) isPortOpen = true;
                }
            }
            catch
            {
                isPortOpen = false;
            }

            if (!isPortOpen)
            {
                try
                {
                    string scriptDir = AppDomain.CurrentDomain.BaseDirectory;
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = "/c node server.js";
                    psi.WorkingDirectory = scriptDir;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;

                    nodeServerProcess = Process.Start(psi);
                    Thread.Sleep(1500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WARN] Could not auto-launch node server: " + ex.Message);
                }
            }
        }

        private void SetupTrayIcon()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Open jDroid-X-Desk", (s, e) => { ShowWindow(); });
            trayMenu.MenuItems.Add("Copy My Desk ID", (s, e) => { CopyLocalAddress(); });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Launch Windows Quick Assist", (s, e) => { Process.Start("ms-quick-assist:"); });
            trayMenu.MenuItems.Add("Elevated Admin Terminal", (s, e) => {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                psi.Verb = "runas";
                try { Process.Start(psi); } catch { }
            });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit & Shutdown", (s, e) => { ShutdownApplication(); });

            trayIcon = new NotifyIcon();
            trayIcon.Text = "jDroid-X-Desk Remote Control Active";
            trayIcon.Icon = SystemIcons.Shield;
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => { ShowWindow(); };
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        private void CopyLocalAddress()
        {
            try
            {
                WebClient wc = new WebClient();
                string json = wc.DownloadString(appUrl + "/api/hardware-id");
                Clipboard.SetText(json);
                trayIcon.ShowBalloonTip(2000, "jDroid-X-Desk", "Desk ID copied to clipboard.", ToolTipIcon.Info);
            }
            catch
            {
                trayIcon.ShowBalloonTip(2000, "jDroid-X-Desk", "System ready.", ToolTipIcon.Info);
            }
        }

        private void ConfigureWebBrowserRegistry()
        {
            try
            {
                string appName = Process.GetCurrentProcess().ProcessName + ".exe";
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true))
                {
                    if (key != null)
                    {
                        key.SetValue(appName, 11001, Microsoft.Win32.RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Minimize to system tray instead of abrupt termination
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(1500, "jDroid-X-Desk", "Minimized to tray. Double click icon to restore.", ToolTipIcon.Info);
            }
        }

        private void ShutdownApplication()
        {
            trayIcon.Visible = false;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(appUrl + "/api/shutdown");
                req.Method = "POST";
                req.Timeout = 1000;
                req.GetResponse();
            }
            catch { }

            if (nodeServerProcess != null && !nodeServerProcess.HasExited)
            {
                try { nodeServerProcess.Kill(); } catch { }
            }

            Application.Exit();
            Environment.Exit(0);
        }
    }
}
