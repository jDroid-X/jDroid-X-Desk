// ==============================================================================
// jDroid-X-Desk Native Windows Application (C# / Win32 Machine Executable)
// Architecture: Native Win32 Tray Supervisor & Chromeless Modern Desktop App
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
    public class Program : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private Process nodeServerProcess;
        private Process appProcess;
        private static string appUrl = "http://localhost:12345";

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        public static void Main(string[] args)
        {
            try { SetProcessDPIAware(); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }

        public Program()
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;

            EnsureNodeServerRunning();
            SetupTrayIcon();
            LaunchAppWindow();
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
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
            catch { isPortOpen = false; }

            if (!isPortOpen)
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = "/c node server.js";
                    psi.WorkingDirectory = baseDir;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;

                    nodeServerProcess = Process.Start(psi);
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WARN] Could not auto-launch node server: " + ex.Message);
                }
            }
        }

        private void LaunchAppWindow()
        {
            try
            {
                string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                if (!File.Exists(edgePath))
                {
                    edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
                }

                string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                string userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "jDroidXDeskApp");

                string targetExe = null;
                if (File.Exists(edgePath)) targetExe = edgePath;
                else if (File.Exists(chromePath)) targetExe = chromePath;

                if (targetExe != null)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = targetExe;
                    psi.Arguments = string.Format("--app=\"{0}\" --window-size=1380,850 --user-data-dir=\"{1}\"", appUrl, userDataDir);
                    appProcess = Process.Start(psi);
                }
                else
                {
                    Process.Start(appUrl);
                }
            }
            catch
            {
                try { Process.Start(appUrl); } catch { }
            }
        }

        private void SetupTrayIcon()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Open jDroid-X-Desk Console", (s, e) => { LaunchAppWindow(); });
            trayMenu.MenuItems.Add("Copy My Desk ID", (s, e) => { CopyLocalAddress(); });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Launch Windows Quick Assist", (s, e) => {
                try { Process.Start("ms-quick-assist:"); } catch { }
            });
            trayMenu.MenuItems.Add("Launch Remote Assistance (MSRA)", (s, e) => {
                try { Process.Start("msra.exe"); } catch { }
            });
            trayMenu.MenuItems.Add("Elevated Admin Terminal", (s, e) => {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                psi.Verb = "runas";
                try { Process.Start(psi); } catch { }
            });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit & Shutdown", (s, e) => { ShutdownApplication(); });

            trayIcon = new NotifyIcon();
            trayIcon.Text = "jDroid-X-Desk (Running)";
            trayIcon.Icon = SystemIcons.Shield;
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => { LaunchAppWindow(); };
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

        private void ShutdownApplication()
        {
            if (trayIcon != null) trayIcon.Visible = false;
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

            if (appProcess != null && !appProcess.HasExited)
            {
                try { appProcess.Kill(); } catch { }
            }

            Application.Exit();
            Environment.Exit(0);
        }
    }
}
