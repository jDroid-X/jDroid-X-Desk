// ==============================================================================
// [Variables]            : IsRunning, ActiveTunnelUrl, LastStatusMessage, TunnelProcess, syncLock, _isNamedTunnel, _configuredCustomDomain
// [Constants]            : DEFAULT_CUSTOM_DOMAIN ("https://desk.jdroid-x.com")
// [Dependencies]         : System, System.IO, System.Diagnostics, System.Text.RegularExpressions, System.Web.Script.Serialization
// [Integration Points]   : MainForm.cs, EmbeddedRelayServer.cs, colab_tunnel.txt, LanDiscoveryEngine.cs
// [Hardcode to Valuebase]: Dynamic domain lookup from database.json; automatic colab_tunnel.txt synchronization
// [Optimized Performance]: Zero-allocation regex match; async process line streaming; safe process termination
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

namespace JDroidXDesk.Native
{
    public static class CloudflareTunnelEngine
    {
        public static bool IsRunning { get; private set; }
        public static string ActiveTunnelUrl { get; private set; }
        public static string LastStatusMessage { get; private set; }
        public static Process TunnelProcess { get; private set; }

        public static event Action<string> OnUrlDiscovered;
        public static event Action<string> OnStatusChanged;

        private static readonly object syncLock = new object();
        private static bool _isNamedTunnel = false;
        private static string _configuredCustomDomain = "https://desk.jdroid-x.com";

        public static void SetConfiguredDomain(string domain)
        {
            if (!string.IsNullOrEmpty(domain)) _configuredCustomDomain = domain.Trim();
        }

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

                // Cloudflare is detached from the jDroid-X-Desk executable. Check system PATH or download as external tool.
                try
                {
                    string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
                    foreach (string p in pathEnv.Split(';'))
                    {
                        string candidate = Path.Combine(p.Trim(), "cloudflared.exe");
                        if (File.Exists(candidate))
                        {
                            if (callback != null) callback(true, candidate);
                            return;
                        }
                    }
                }
                catch { }

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

                _isNamedTunnel = !string.IsNullOrEmpty(tunnelToken);

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

            // 1. Quick Tunnel Auto-Discovery (https://*.trycloudflare.com)
            Match m = Regex.Match(line, @"https://[a-zA-Z0-9-]+\.trycloudflare\.com");
            if (m.Success)
            {
                string url = m.Value;
                ActiveTunnelUrl = url;
                LastStatusMessage = "Tunnel online: " + url;
                PersistTunnelUrl(url);
                if (OnUrlDiscovered != null) OnUrlDiscovered(url);
                if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);
                return;
            }

            // 2. Named Tunnel Connection Activation (Token-based custom domain, e.g. desk.jdroid-x.com)
            if (_isNamedTunnel && (line.Contains("Registered tunnel connection") || (line.Contains("Connection") && line.Contains("registered"))))
            {
                if (string.IsNullOrEmpty(ActiveTunnelUrl))
                {
                    string url = ResolveConfiguredDomain();
                    ActiveTunnelUrl = url;
                    LastStatusMessage = "Cloudflare Edge Tunnel connected: " + url;
                    PersistTunnelUrl(url);
                    if (OnUrlDiscovered != null) OnUrlDiscovered(url);
                    if (OnStatusChanged != null) OnStatusChanged(LastStatusMessage);
                }
            }
        }

        private static string ResolveConfiguredDomain()
        {
            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.json");
                if (File.Exists(dbPath))
                {
                    string json = File.ReadAllText(dbPath);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(json);
                    if (dict != null && dict.ContainsKey("settings"))
                    {
                        Dictionary<string, object> s = dict["settings"] as Dictionary<string, object>;
                        if (s != null && s.ContainsKey("connection"))
                        {
                            Dictionary<string, object> c = s["connection"] as Dictionary<string, object>;
                            if (c != null)
                            {
                                if (c.ContainsKey("customDomain") && !string.IsNullOrEmpty(Convert.ToString(c["customDomain"])))
                                    return Convert.ToString(c["customDomain"]).Trim();
                                if (c.ContainsKey("relayServer") && !string.IsNullOrEmpty(Convert.ToString(c["relayServer"])))
                                    return Convert.ToString(c["relayServer"]).Trim();
                            }
                        }
                    }
                }
            }
            catch { }
            return _configuredCustomDomain ?? "https://desk.jdroid-x.com";
        }

        private static void PersistTunnelUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            try
            {
                string tunnelFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colab_tunnel.txt");
                File.WriteAllText(tunnelFilePath, url);
            }
            catch { }
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
}
