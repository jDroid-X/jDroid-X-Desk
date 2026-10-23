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
    public class DiscoveredDeskInfo
    {
        public string deskId { get; set; }
        public string formattedDeskId { get; set; }
        public string endpoint { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public string machineName { get; set; }
        public string tunnelUrl { get; set; }
        public DateTime lastSeen { get; set; }
        public bool isOnline { get { return (DateTime.UtcNow - lastSeen).TotalSeconds < 15; } }
    }

    public class LanDiscoveryEngine
    {
        private static readonly LanDiscoveryEngine _instance = new LanDiscoveryEngine();
        public static LanDiscoveryEngine Instance { get { return _instance; } }

        private UdpClient _udpListener;
        private UdpClient _udpBroadcastSender;
        private Thread _broadcastThread;
        private Thread _listenThread;
        private volatile bool _isRunning = false;
        private int _discoveryPort = 8089;
        private string _localDeskId = "";
        private int _localHttpPort = 8088;

        private readonly Dictionary<string, DiscoveredDeskInfo> _discoveredDesks = new Dictionary<string, DiscoveredDeskInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _discoveredEndpoints = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new object();

        public bool IsRunning { get { return _isRunning; } }

        public void Start(string localDeskId, int localHttpPort = 8088, int discoveryPort = 8089)
        {
            if (_isRunning) return;
            _localDeskId = localDeskId ?? "";
            _localHttpPort = localHttpPort > 0 ? localHttpPort : 8088;
            _discoveryPort = discoveryPort > 0 ? discoveryPort : 8089;
            _isRunning = true;

            try
            {
                _udpListener = new UdpClient();
                _udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpListener.Client.Bind(new IPEndPoint(IPAddress.Any, _discoveryPort));
                _udpListener.EnableBroadcast = true;

                _listenThread = new Thread(new ThreadStart(ListenLoop))
                {
                    IsBackground = true,
                    Name = "jDroid-X-LanDiscovery-Listen"
                };
                _listenThread.Start();
            }
            catch { }

            try
            {
                _udpBroadcastSender = new UdpClient();
                _udpBroadcastSender.EnableBroadcast = true;

                _broadcastThread = new Thread(new ThreadStart(BroadcastLoop))
                {
                    IsBackground = true,
                    Name = "jDroid-X-LanDiscovery-Broadcast"
                };
                _broadcastThread.Start();
            }
            catch { }
        }

        public void Stop()
        {
            _isRunning = false;
            try { if (_udpListener != null) _udpListener.Close(); } catch { }
            try { if (_udpBroadcastSender != null) _udpBroadcastSender.Close(); } catch { }
        }

        public void RegisterDiscoveredDesk(string deskId, string endpoint, string machineName = "", string tunnelUrl = "")
        {
            if (string.IsNullOrEmpty(deskId) || string.IsNullOrEmpty(endpoint)) return;
            string cleanId = deskId.Trim().Replace(" ", "").Replace("-", "");
            string formattedId = (cleanId.Length == 9) ? string.Format("{0}-{1}-{2}", cleanId.Substring(0, 3), cleanId.Substring(3, 3), cleanId.Substring(6, 3)) : deskId.Trim();

            string ip = "";
            int port = 8088;
            try
            {
                Uri u = new Uri(endpoint);
                ip = u.Host;
                port = u.Port;
            }
            catch { }

            lock (_lock)
            {
                _discoveredEndpoints[deskId] = endpoint;
                _discoveredEndpoints[cleanId] = endpoint;
                _discoveredEndpoints[formattedId] = endpoint;

                DiscoveredDeskInfo info;
                if (!_discoveredDesks.TryGetValue(cleanId, out info))
                {
                    info = new DiscoveredDeskInfo();
                    _discoveredDesks[cleanId] = info;
                }
                info.deskId = cleanId;
                info.formattedDeskId = formattedId;
                info.endpoint = endpoint;
                info.ip = ip;
                info.port = port;
                if (!string.IsNullOrEmpty(machineName)) info.machineName = machineName;
                else if (string.IsNullOrEmpty(info.machineName)) info.machineName = "Remote Desk " + formattedId;
                if (!string.IsNullOrEmpty(tunnelUrl)) info.tunnelUrl = tunnelUrl;
                info.lastSeen = DateTime.UtcNow;
            }
        }

        public List<DiscoveredDeskInfo> GetDiscoveredDesks()
        {
            List<DiscoveredDeskInfo> list = new List<DiscoveredDeskInfo>();
            DateTime cutoff = DateTime.UtcNow.AddSeconds(-45);
            lock (_lock)
            {
                List<string> expiredKeys = new List<string>();
                foreach (var kvp in _discoveredDesks)
                {
                    if (kvp.Value.lastSeen < cutoff)
                    {
                        expiredKeys.Add(kvp.Key);
                    }
                    else
                    {
                        list.Add(new DiscoveredDeskInfo
                        {
                            deskId = kvp.Value.deskId,
                            formattedDeskId = kvp.Value.formattedDeskId,
                            endpoint = kvp.Value.endpoint,
                            ip = kvp.Value.ip,
                            port = kvp.Value.port,
                            machineName = kvp.Value.machineName,
                            tunnelUrl = kvp.Value.tunnelUrl,
                            lastSeen = kvp.Value.lastSeen
                        });
                    }
                }
                foreach (string k in expiredKeys)
                {
                    _discoveredDesks.Remove(k);
                }
            }
            return list;
        }

        public bool TryGetDeskEndpoint(string deskId, out string endpoint)
        {
            endpoint = null;
            if (string.IsNullOrEmpty(deskId)) return false;
            string cleanId = deskId.Trim().Replace(" ", "").Replace("-", "");
            lock (_lock)
            {
                if (_discoveredEndpoints.TryGetValue(deskId, out endpoint) ||
                    _discoveredEndpoints.TryGetValue(cleanId, out endpoint))
                {
                    return true;
                }
                DiscoveredDeskInfo info;
                if (_discoveredDesks.TryGetValue(cleanId, out info))
                {
                    endpoint = !string.IsNullOrEmpty(info.endpoint) ? info.endpoint : info.tunnelUrl;
                    return !string.IsNullOrEmpty(endpoint);
                }
            }
            return false;
        }

        public void BroadcastQuery(string targetDeskId = "*")
        {
            try
            {
                if (_udpBroadcastSender != null && !string.IsNullOrEmpty(targetDeskId))
                {
                    string msg = "JDX_QUERY|" + targetDeskId;
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
                    IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, _discoveryPort);
                    _udpBroadcastSender.Send(data, data.Length, ep);
                }
            }
            catch { }
        }

        private void BroadcastLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_localDeskId) && _udpBroadcastSender != null)
                    {
                        string localIp = GetPrimaryLocalIp();
                        string activeTun = (CloudflareTunnelEngine.IsRunning && !string.IsNullOrEmpty(CloudflareTunnelEngine.ActiveTunnelUrl)) ? CloudflareTunnelEngine.ActiveTunnelUrl : "";
                        string beaconMsg = string.Format("JDX_BEACON|{0}|{1}|{2}|{3}|{4}",
                            _localDeskId, localIp, _localHttpPort, Environment.MachineName, activeTun);
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(beaconMsg);
                        IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, _discoveryPort);
                        _udpBroadcastSender.Send(data, data.Length, ep);
                    }
                }
                catch { }

                Thread.Sleep(2000);
            }
        }

        private void ListenLoop()
        {
            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            while (_isRunning)
            {
                try
                {
                    if (_udpListener == null) break;
                    byte[] bytes = _udpListener.Receive(ref remoteEp);
                    if (bytes != null && bytes.Length > 0)
                    {
                        string msg = System.Text.Encoding.UTF8.GetString(bytes);
                        if (msg.StartsWith("JDX_BEACON|") || msg.StartsWith("JDX_REPLY|"))
                        {
                            string[] parts = msg.Split('|');
                            if (parts.Length >= 4)
                            {
                                string senderDeskId = parts[1];
                                string senderIp = parts[2];
                                string senderPort = parts[3];
                                string machineName = (parts.Length >= 5) ? parts[4] : "";
                                string senderTun = (parts.Length >= 6) ? parts[5] : "";

                                if (!string.IsNullOrEmpty(_localDeskId) && senderDeskId.Replace("-", "") == _localDeskId.Replace("-", ""))
                                    continue;

                                if (senderIp == "127.0.0.1" || senderIp == "localhost" || string.IsNullOrEmpty(senderIp))
                                {
                                    if (remoteEp.Address != null) senderIp = remoteEp.Address.ToString();
                                }

                                string endpoint = string.Format("http://{0}:{1}", senderIp, senderPort);
                                RegisterDiscoveredDesk(senderDeskId, endpoint, machineName, senderTun);
                                if (!string.IsNullOrEmpty(senderTun))
                                {
                                    RegisterDiscoveredDesk(senderDeskId + "_tunnel", senderTun, machineName, senderTun);
                                }
                            }
                        }
                        else if (msg.StartsWith("JDX_QUERY|"))
                        {
                            string[] parts = msg.Split('|');
                            if (parts.Length >= 2)
                            {
                                string queryDeskId = parts[1].Trim().Replace("-", "");
                                string myCleanId = _localDeskId.Trim().Replace("-", "");
                                if (!string.IsNullOrEmpty(myCleanId) && (queryDeskId == myCleanId || queryDeskId == "*"))
                                {
                                    string localIp = GetPrimaryLocalIp();
                                    string activeTun = (CloudflareTunnelEngine.IsRunning && !string.IsNullOrEmpty(CloudflareTunnelEngine.ActiveTunnelUrl)) ? CloudflareTunnelEngine.ActiveTunnelUrl : "";
                                    string replyMsg = string.Format("JDX_REPLY|{0}|{1}|{2}|{3}|{4}",
                                        _localDeskId, localIp, _localHttpPort, Environment.MachineName, activeTun);
                                    byte[] replyData = System.Text.Encoding.UTF8.GetBytes(replyMsg);
                                    if (_udpBroadcastSender != null)
                                    {
                                        _udpBroadcastSender.Send(replyData, replyData.Length, new IPEndPoint(remoteEp.Address, _discoveryPort));
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    if (!_isRunning) break;
                    Thread.Sleep(100);
                }
            }
        }

        public static string GetPrimaryLocalIp()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    {
                        string desc = (ni.Description + " " + ni.Name).ToLowerInvariant();
                        if (desc.Contains("virtual") || desc.Contains("vmware") || desc.Contains("vbox") || desc.Contains("hyper-v") || desc.Contains("wsl") || desc.Contains("tailscale"))
                            continue;

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
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }
    }

    // ==========================================================================
    // 3. MAIN FORM & ANTIGRAVITY WORKSPACE LAYOUT (OOPS CONTROLLER LAYER)
    // ==========================================================================

}
