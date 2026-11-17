using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace JDroidXDesk.Native
{
    public class DependencyHealthReport
    {
        public bool GatewayPort7700Active { get; set; }
        public bool LocalRelay8088Active { get; set; }
        public bool LanDiscovery8089Active { get; set; }
        public bool CloudflareTunnelActive { get; set; }
        public bool CloudflareEdgeResponding { get; set; }
        public bool NodeJsInstalled { get; set; }
        public string NodeJsPath { get; set; }
        public string ActiveRelayUrl { get; set; }
        public List<string> BrokenIntegrations { get; set; }
        public bool AllHealthy { get { return BrokenIntegrations == null || BrokenIntegrations.Count == 0; } }
        public DateTime Timestamp { get; set; }

        public DependencyHealthReport()
        {
            BrokenIntegrations = new List<string>();
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Autonomous Enterprise Dependency, Port Validation & Self-Healing Engine (OOPS MVC Service)
    /// Continuously audits Port 7700 (Cloud Gateway), Port 8088 (Local Relay), Port 8089 (Discovery),
    /// Cloudflare edge tunnel, and external runtime dependencies.
    /// Provides 0-latency auto-start and non-intrusive operator alerts.
    /// </summary>
    public class DependencySupervisor
    {
        private static DependencySupervisor _instance;
        private static readonly object _lock = new object();

        public static DependencySupervisor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null) _instance = new DependencySupervisor();
                    }
                }
                return _instance;
            }
        }

        public const int GATEWAY_PORT = 7700;
        public const int LOCAL_RELAY_PORT = 8088;
        public const int LAN_DISCOVERY_PORT = 8089;
        public const string CLOUDFLARE_CUSTOM_DOMAIN = "https://desk.jdroid-x.com";

        public DependencyHealthReport LastReport { get; private set; }
        public bool IsMonitoring { get; private set; }
        public bool AutoStartEnabled { get; set; }

        public event Action<DependencyHealthReport> OnHealthEvaluated;
        public event Action<string> OnIntegrationBroken;
        public event Action<string> OnSelfHealed;

        private Timer _monitorTimer;
        private bool _isEvaluating;

        private DependencySupervisor()
        {
            AutoStartEnabled = true;
            LastReport = new DependencyHealthReport();
        }

        public void Start(int intervalMs = 6000)
        {
            if (IsMonitoring) return;
            IsMonitoring = true;
            _monitorTimer = new Timer(state => EvaluateHealthAsync(), null, 500, intervalMs);
        }

        public void Stop()
        {
            IsMonitoring = false;
            if (_monitorTimer != null)
            {
                _monitorTimer.Dispose();
                _monitorTimer = null;
            }
        }

        public void EvaluateHealthAsync(Action<DependencyHealthReport> onDone = null)
        {
            if (_isEvaluating) return;
            ThreadPool.QueueUserWorkItem(delegate
            {
                _isEvaluating = true;
                try
                {
                    DependencyHealthReport report = RunHealthAudit();
                    LastReport = report;

                    if (onDone != null) onDone(report);
                    if (OnHealthEvaluated != null) OnHealthEvaluated(report);

                    // Self-healing check: if Port 7700 is down and autostart enabled, trigger recovery
                    if (!report.GatewayPort7700Active && AutoStartEnabled)
                    {
                        if (OnIntegrationBroken != null)
                        {
                            OnIntegrationBroken("Port 7700 (Central Ingress Gateway) is inactive. Initiating automated recovery...");
                        }
                        AutostartGateway();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[SUPERVISOR ERROR] Health audit failed: " + ex.Message);
                }
                finally
                {
                    _isEvaluating = false;
                }
            });
        }

        public DependencyHealthReport RunHealthAudit()
        {
            DependencyHealthReport r = new DependencyHealthReport();

            int gatewayPort = GATEWAY_PORT;
            int localRelayPort = LOCAL_RELAY_PORT;
            string customDomain = CLOUDFLARE_CUSTOM_DOMAIN;

            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.json");
                if (File.Exists(dbPath))
                {
                    string txt = File.ReadAllText(dbPath);
                    var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(txt);
                    if (dict != null && dict.ContainsKey("settings") && dict["settings"] is Dictionary<string, object>)
                    {
                        var s = dict["settings"] as Dictionary<string, object>;
                        if (s.ContainsKey("connection") && s["connection"] is Dictionary<string, object>)
                        {
                            var c = s["connection"] as Dictionary<string, object>;
                            if (c.ContainsKey("relayPort") && c["relayPort"] != null)
                            {
                                int p;
                                if (int.TryParse(c["relayPort"].ToString(), out p) && p > 0) localRelayPort = p;
                            }
                            if (c.ContainsKey("customDomain") && c["customDomain"] != null && !string.IsNullOrEmpty(c["customDomain"].ToString()))
                            {
                                customDomain = c["customDomain"].ToString();
                            }
                        }
                    }
                }
            }
            catch { }

            // 1. Audit Ingress Port 7700 (Node.js Gateway)
            r.GatewayPort7700Active = ProbeTcpPort("127.0.0.1", gatewayPort, 600);

            // 2. Audit Local Embedded Relay Port
            r.LocalRelay8088Active = ProbeTcpPort("127.0.0.1", localRelayPort, 500);

            // 3. Audit LAN Discovery Port 8089 (UDP)
            r.LanDiscovery8089Active = LanDiscoveryEngine.Instance != null && LanDiscoveryEngine.Instance.IsRunning;

            // 4. Audit Cloudflare Tunnel Daemon & Edge Ping
            r.CloudflareTunnelActive = IsProcessRunning("cloudflared");
            r.CloudflareEdgeResponding = ProbeHttpEndpoint(customDomain.TrimEnd('/') + "/ping", 1500);

            // 5. Audit Node.js Runtime Availability
            string nodePath;
            r.NodeJsInstalled = TryFindNodeExecutable(out nodePath);
            r.NodeJsPath = nodePath;

            // Compile Broken Integrations Ledger
            if (!r.GatewayPort7700Active)
            {
                r.BrokenIntegrations.Add(string.Format("Cloud Ingress Gateway on Port {0} is inactive.", GATEWAY_PORT));
            }
            if (!r.CloudflareTunnelActive && !r.CloudflareEdgeResponding)
            {
                r.BrokenIntegrations.Add("Cloudflare Tunnel is disconnected (Risk of Error 1033).");
            }
            if (!r.LocalRelay8088Active)
            {
                r.BrokenIntegrations.Add(string.Format("Local Embedded Relay on Port {0} is offline.", LOCAL_RELAY_PORT));
            }
            if (!r.NodeJsInstalled)
            {
                r.BrokenIntegrations.Add("Node.js runtime not found on system PATH.");
            }

            return r;
        }

        public bool AutostartGateway()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = Path.Combine(baseDir, "scripts", "start-desk-cloud-tunnel.bat");

                if (!File.Exists(scriptPath))
                {
                    scriptPath = Path.Combine(baseDir, "start_desk_tunnel.bat");
                }

                if (File.Exists(scriptPath))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = string.Format("/c \"{0}\"", scriptPath),
                        WorkingDirectory = baseDir,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    Process p = Process.Start(psi);
                    if (p != null)
                    {
                        // Wait up to 3.5 seconds for Port 7700 to bind
                        for (int i = 0; i < 7; i++)
                        {
                            Thread.Sleep(500);
                            if (ProbeTcpPort("127.0.0.1", GATEWAY_PORT, 400))
                            {
                                if (OnSelfHealed != null)
                                {
                                    OnSelfHealed(string.Format("🟢 Ingress Port {0} & Tunnel successfully restored via automated self-healing.", GATEWAY_PORT));
                                }
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    // Direct node fallback if batch script not located
                    string nodeExe;
                    if (TryFindNodeExecutable(out nodeExe))
                    {
                        string serverJs = Path.Combine(baseDir, "server.js");
                        if (File.Exists(serverJs))
                        {
                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                FileName = nodeExe,
                                Arguments = string.Format("\"{0}\"", serverJs),
                                WorkingDirectory = baseDir,
                                CreateNoWindow = true,
                                UseShellExecute = false
                            };
                            Process.Start(psi);
                            Thread.Sleep(1000);
                            return ProbeTcpPort("127.0.0.1", GATEWAY_PORT, 600);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[SUPERVISOR RECOVERY ERROR] " + ex.Message);
            }
            return false;
        }

        public static bool ProbeTcpPort(string host, int port, int timeoutMs = 600)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    IAsyncResult ar = client.BeginConnect(host, port, null, null);
                    bool success = ar.AsyncWaitHandle.WaitOne(timeoutMs);
                    if (!success) return false;
                    client.EndConnect(ar);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ProbeHttpEndpoint(string url, int timeoutMs = 1200)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Timeout = timeoutMs;
                req.ReadWriteTimeout = timeoutMs;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    return resp.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool IsProcessRunning(string processName)
        {
            try
            {
                Process[] procs = Process.GetProcessesByName(processName);
                return procs != null && procs.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryFindNodeExecutable(out string foundPath)
        {
            foundPath = null;
            try
            {
                string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
                string[] paths = pathEnv.Split(';');
                foreach (string p in paths)
                {
                    if (string.IsNullOrEmpty(p)) continue;
                    string cand = Path.Combine(p.Trim(), "node.exe");
                    if (File.Exists(cand))
                    {
                        foundPath = cand;
                        return true;
                    }
                }

                // Check common standard installation locations
                string[] standardPaths = new string[]
                {
                    @"C:\jDroid-X-AI-Tools\NodeJS\node.exe",
                    @"C:\Program Files\nodejs\node.exe",
                    @"C:\Program Files (x86)\nodejs\node.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\nodejs\node.exe")
                };

                foreach (string sp in standardPaths)
                {
                    if (File.Exists(sp))
                    {
                        foundPath = sp;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
