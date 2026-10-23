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

}
