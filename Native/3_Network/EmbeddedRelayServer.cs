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
    public class EmbeddedRelayServer
    {
        private static readonly EmbeddedRelayServer _instance = new EmbeddedRelayServer();
        public static EmbeddedRelayServer Instance { get { return _instance; } }

        private TcpListener _listener;
        private Thread _listenThread;
        private TcpListener _adminListener;
        private Thread _adminListenThread;
        private volatile bool _isRunning;
        private int _port = 8088;
        private int _adminPort = 12345;
        private string _activeEngine = "jdroidRelay";
        private readonly object _syncLock = new object();
        private readonly Dictionary<string, byte[]> _binaryFrames = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, string> _base64Frames = new Dictionary<string, string>();
        private readonly Dictionary<string, byte[]> _audioChunks = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, Dictionary<string, object>> _registeredDevices = new Dictionary<string, Dictionary<string, object>>();
        private readonly Dictionary<string, List<Dictionary<string, object>>> _inputQueues = new Dictionary<string, List<Dictionary<string, object>>>();
        private readonly DateTime _startTime = DateTime.UtcNow;
        private DateTime _lastViewerActivityTime = DateTime.MinValue;
        private string _lastViewerEndpoint = "";

        public bool IsRunning { get { return _isRunning; } }
        public int Port { get { return _port; } }
        public int AdminPort { get { return _adminPort; } }
        public string ActiveEngine { get { return _activeEngine; } set { _activeEngine = value; } }
        public string LastViewerEndpoint { get { return _lastViewerEndpoint; } }

        private readonly HashSet<string> _approvedViewers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _rejectedViewers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool HasActiveViewers()
        {
            return (DateTime.UtcNow - _lastViewerActivityTime).TotalSeconds < 4;
        }

        public bool HasApprovedViewers()
        {
            lock (_syncLock)
            {
                return _approvedViewers.Count > 0;
            }
        }

        public void AuthorizeViewer(string endpointOrIp)
        {
            if (string.IsNullOrEmpty(endpointOrIp)) return;
            string clean = ExtractCleanIp(endpointOrIp);
            lock (_syncLock)
            {
                _approvedViewers.Add(clean);
                _approvedViewers.Add(endpointOrIp.Trim());
                _rejectedViewers.Remove(clean);
                _rejectedViewers.Remove(endpointOrIp.Trim());
            }
        }

        public void RejectViewer(string endpointOrIp)
        {
            if (string.IsNullOrEmpty(endpointOrIp)) return;
            string clean = ExtractCleanIp(endpointOrIp);
            lock (_syncLock)
            {
                _rejectedViewers.Add(clean);
                _rejectedViewers.Add(endpointOrIp.Trim());
                _approvedViewers.Remove(clean);
                _approvedViewers.Remove(endpointOrIp.Trim());
                _lastViewerActivityTime = DateTime.MinValue;
            }
        }

        public bool IsViewerAuthorized(string endpointOrIp, string sourceId = null)
        {
            lock (_syncLock)
            {
                if (!string.IsNullOrEmpty(sourceId))
                {
                    string cleanSrc = ExtractCleanIp(sourceId);
                    if (_rejectedViewers.Contains(cleanSrc) || _rejectedViewers.Contains(sourceId.Trim())) return false;
                    if (_approvedViewers.Contains(cleanSrc) || _approvedViewers.Contains(sourceId.Trim())) return true;
                }
                if (!string.IsNullOrEmpty(endpointOrIp))
                {
                    string clean = ExtractCleanIp(endpointOrIp);
                    if (_rejectedViewers.Contains(clean) || _rejectedViewers.Contains(endpointOrIp.Trim())) return false;
                    if (_approvedViewers.Contains(clean) || _approvedViewers.Contains(endpointOrIp.Trim())) return true;
                }
                return false;
            }
        }

        public static string ExtractCleanIp(string ep)
        {
            if (string.IsNullOrEmpty(ep)) return "";
            string ip = ep.Trim();
            if (ip.StartsWith("["))
            {
                int closeB = ip.IndexOf(']');
                if (closeB > 0) return ip.Substring(1, closeB - 1);
            }
            int colon = ip.IndexOf(':');
            if (colon > 0) return ip.Substring(0, colon);
            return ip;
        }

        public void DisconnectAllViewers()
        {
            lock (_syncLock)
            {
                _lastViewerActivityTime = DateTime.MinValue;
                _lastViewerEndpoint = "";
                _approvedViewers.Clear();
            }
        }

        public void RecordViewerActivity()
        {
            _lastViewerActivityTime = DateTime.UtcNow;
        }

        public static bool IsPortInUse(int port)
        {
            try
            {
                IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] listeners = ipProps.GetActiveTcpListeners();
                for (int i = 0; i < listeners.Length; i++)
                {
                    if (listeners[i].Port == port) return true;
                }
            }
            catch { }
            return false;
        }

        public void Start(int port, int adminPort = 12345)
        {
            _port = port > 0 ? port : 8088;
            _adminPort = adminPort > 0 ? adminPort : 12345;

            // 1. Primary Relay Listener (Port 8088)
            if (_listener == null && !IsPortInUse(_port))
            {
                try
                {
                    _listener = new TcpListener(IPAddress.Any, _port);
                    _listener.Start();
                    _isRunning = true;
                    _listenThread = new Thread(new ThreadStart(ListenLoop))
                    {
                        IsBackground = true,
                        Name = "jDroid-X-NativeRelay"
                    };
                    _listenThread.Start();
                }
                catch
                {
                    _isRunning = false;
                }
            }
            else if (IsPortInUse(_port))
            {
                _isRunning = true;
            }

            // 2. Secondary Admin & Engine Switch Listener (Port 12345)
            if (_adminListener == null && !IsPortInUse(_adminPort))
            {
                try
                {
                    _adminListener = new TcpListener(IPAddress.Any, _adminPort);
                    _adminListener.Start();
                    _adminListenThread = new Thread(new ThreadStart(AdminListenLoop))
                    {
                        IsBackground = true,
                        Name = "jDroid-X-NativeAdmin"
                    };
                    _adminListenThread.Start();
                }
                catch { }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            if (_listener != null)
            {
                try { _listener.Stop(); } catch { }
                _listener = null;
            }
            if (_adminListener != null)
            {
                try { _adminListener.Stop(); } catch { }
                _adminListener = null;
            }
        }

        private void AdminListenLoop()
        {
            while (_adminListener != null)
            {
                try
                {
                    TcpClient client = _adminListener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), client);
                }
                catch
                {
                    break;
                }
            }
        }

        public void UpdateFrame(string targetId, byte[] rawBytes, string b64)
        {
            if (string.IsNullOrEmpty(targetId)) return;
            lock (_syncLock)
            {
                if (rawBytes != null && rawBytes.Length > 0)
                {
                    _binaryFrames[targetId] = rawBytes;
                }
                if (!string.IsNullOrEmpty(b64))
                {
                    _base64Frames[targetId] = b64;
                }
            }
        }

        public void UpdateAudioChunk(string targetId, byte[] wavBytes)
        {
            if (string.IsNullOrEmpty(targetId) || wavBytes == null || wavBytes.Length == 0) return;
            lock (_syncLock)
            {
                _audioChunks[targetId] = wavBytes;
            }
        }

        public byte[] GetAudioChunk(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return null;
            lock (_syncLock)
            {
                if (_audioChunks.ContainsKey(targetId))
                    return _audioChunks[targetId];
            }
            return null;
        }

        public List<Dictionary<string, object>> GetPendingInputEvents(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return null;
            lock (_syncLock)
            {
                if (_inputQueues.ContainsKey(targetId) && _inputQueues[targetId].Count > 0)
                {
                    List<Dictionary<string, object>> copy = new List<Dictionary<string, object>>(_inputQueues[targetId]);
                    _inputQueues[targetId].Clear();
                    return copy;
                }
            }
            return null;
        }

        private void ListenLoop()
        {
            while (_isRunning && _listener != null)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), client);
                }
                catch
                {
                    if (!_isRunning) break;
                }
            }
        }

        private void ProcessClient(object state)
        {
            TcpClient client = state as TcpClient;
            if (client == null) return;

            try
            {
                client.ReceiveTimeout = 4000;
                client.SendTimeout = 4000;

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[8192];
                    int totalRead = 0;
                    MemoryStream msHeader = new MemoryStream();
                    int headerEndIndex = -1;

                    while (totalRead < 32768)
                    {
                        int read = stream.Read(buffer, 0, buffer.Length);
                        if (read <= 0) break;
                        msHeader.Write(buffer, 0, read);
                        totalRead += read;

                        byte[] currentBytes = msHeader.ToArray();
                        for (int i = 0; i < currentBytes.Length - 3; i++)
                        {
                            if (currentBytes[i] == '\r' && currentBytes[i + 1] == '\n' && currentBytes[i + 2] == '\r' && currentBytes[i + 3] == '\n')
                            {
                                headerEndIndex = i;
                                break;
                            }
                        }
                        if (headerEndIndex >= 0) break;
                    }

                    if (headerEndIndex < 0) return;

                    byte[] allHeaderBytes = msHeader.ToArray();
                    string headerText = System.Text.Encoding.ASCII.GetString(allHeaderBytes, 0, headerEndIndex);
                    string[] lines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);
                    if (lines.Length == 0) return;

                    string[] requestLine = lines[0].Split(' ');
                    if (requestLine.Length < 2) return;

                    string method = requestLine[0].ToUpperInvariant();
                    string fullUrl = requestLine[1];
                    string path = fullUrl.Split('?')[0];
                    bool isRealViewerSession = path.StartsWith("/api/relay/pull_bin") || 
                                               path.StartsWith("/api/relay/pull_frame") || 
                                               path.StartsWith("/api/relay/frame_bin") ||
                                               path.StartsWith("/api/relay/frame") ||
                                               path.StartsWith("/view") ||
                                               (path.StartsWith("/api/relay/push_input") && method == "POST");
                    if (isRealViewerSession)
                    {
                        _lastViewerActivityTime = DateTime.UtcNow;
                        try
                        {
                            if (client.Client != null && client.Client.RemoteEndPoint != null)
                            {
                                _lastViewerEndpoint = client.Client.RemoteEndPoint.ToString();
                            }
                        }
                        catch { }
                    }

                    Dictionary<string, string> queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (fullUrl.Contains("?"))
                    {
                        string qStr = fullUrl.Substring(fullUrl.IndexOf('?') + 1);
                        string[] pairs = qStr.Split('&');
                        for (int i = 0; i < pairs.Length; i++)
                        {
                            string[] kv = pairs[i].Split(new[] { '=' }, 2);
                            if (kv.Length == 2) queryParams[kv[0]] = Uri.UnescapeDataString(kv[1]);
                            else if (kv.Length == 1) queryParams[kv[0]] = "";
                        }
                    }

                    Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        int colon = lines[i].IndexOf(':');
                        if (colon > 0)
                        {
                            string k = lines[i].Substring(0, colon).Trim();
                            string v = lines[i].Substring(colon + 1).Trim();
                            headers[k] = v;
                        }
                    }

                    if (method == "OPTIONS")
                    {
                        SendSimpleResponse(stream, 200, "text/plain", "");
                        return;
                    }

                    int contentLength = 0;
                    if (headers.ContainsKey("Content-Length"))
                    {
                        int.TryParse(headers["Content-Length"], out contentLength);
                    }

                    byte[] bodyBytes = new byte[contentLength];
                    int bodyStartInHeaderBuffer = headerEndIndex + 4;
                    int alreadyReadBodyBytes = allHeaderBytes.Length - bodyStartInHeaderBuffer;
                    if (alreadyReadBodyBytes > 0 && contentLength > 0)
                    {
                        int copyLen = Math.Min(alreadyReadBodyBytes, contentLength);
                        Array.Copy(allHeaderBytes, bodyStartInHeaderBuffer, bodyBytes, 0, copyLen);
                        int remaining = contentLength - copyLen;
                        int offset = copyLen;
                        while (remaining > 0)
                        {
                            int r = stream.Read(bodyBytes, offset, remaining);
                            if (r <= 0) break;
                            offset += r;
                            remaining -= r;
                        }
                    }
                    else if (contentLength > 0)
                    {
                        int remaining = contentLength;
                        int offset = 0;
                        while (remaining > 0)
                        {
                            int r = stream.Read(bodyBytes, offset, remaining);
                            if (r <= 0) break;
                            offset += r;
                            remaining -= r;
                        }
                    }

                    JavaScriptSerializer serializer = new JavaScriptSerializer();

                    string clientIp = "";
                    try
                    {
                        if (client.Client != null && client.Client.RemoteEndPoint != null)
                        {
                            clientIp = ExtractCleanIp(client.Client.RemoteEndPoint.ToString());
                        }
                    }
                    catch { }
                    if (headers.ContainsKey("X-Forwarded-For"))
                    {
                        string xff = headers["X-Forwarded-For"].Split(',')[0].Trim();
                        if (!string.IsNullOrEmpty(xff)) clientIp = ExtractCleanIp(xff);
                    }
                    string sourceDeskId = headers.ContainsKey("X-Source-Id") ? headers["X-Source-Id"].Trim() : (queryParams.ContainsKey("sourceId") ? queryParams["sourceId"].Trim() : "");

                    if (path == "/" || path == "/ping" || path == "/api/relay/ping" || path == "/wake" || path == "/api/relay/status" || path == "/status" || path == "/api/settings" || path == "/api/admin/metrics")
                    {
                        int devCount = 0;
                        int frameCount = 0;
                        lock (_syncLock)
                        {
                            devCount = _registeredDevices.Count;
                            frameCount = _binaryFrames.Count;
                        }

                        Dictionary<string, object> status = new Dictionary<string, object>();
                        status["success"] = true;
                        status["status"] = "AWAKE";
                        status["service"] = "jDroid-X Native C# Embedded Relay Server";
                        status["engine"] = "NativeNetEngine";
                        status["port"] = _port;
                        status["active_devices"] = devCount;
                        status["frames_buffered"] = frameCount;
                        status["uptime_sec"] = (int)(DateTime.UtcNow - _startTime).TotalSeconds;
                        status["timestamp"] = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                        SendJsonResponse(stream, 200, serializer.Serialize(status));
                        return;
                    }

                    if (path == "/api/admin/engine/switch")
                    {
                        string bodyStr = System.Text.Encoding.UTF8.GetString(bodyBytes);
                        Dictionary<string, object> pDict = serializer.Deserialize<Dictionary<string, object>>(bodyStr);
                        string swEng = "jdroidRelay";
                        if (pDict != null)
                        {
                            if (pDict.ContainsKey("engineId")) swEng = Convert.ToString(pDict["engineId"]);
                            else if (pDict.ContainsKey("engine")) swEng = Convert.ToString(pDict["engine"]);
                        }
                        else if (queryParams.ContainsKey("engine"))
                        {
                            swEng = queryParams["engine"];
                        }
                        _activeEngine = swEng;
                        Dictionary<string, object> resp = new Dictionary<string, object>();
                        resp["success"] = true;
                        resp["engine"] = swEng;
                        resp["message"] = "Engine switched to " + swEng + " via Native Embedded Gateway";
                        SendJsonResponse(stream, 200, serializer.Serialize(resp));
                        return;
                    }

                    if (path == "/api/admin/config")
                    {
                        Dictionary<string, object> cfg = new Dictionary<string, object>();
                        cfg["success"] = true;
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        data["activeEngine"] = _activeEngine;
                        data["hardwareAcceleration"] = "native_gdi_direct";
                        Dictionary<string, object> limits = new Dictionary<string, object>();
                        limits["maxConcurrentSessions"] = 10;
                        limits["maxFps"] = 60;
                        limits["idleTimeoutMinutes"] = 15;
                        data["processingLimits"] = limits;
                        cfg["data"] = data;

                        SendJsonResponse(stream, 200, serializer.Serialize(cfg));
                        return;
                    }

                    if (path == "/api/relay/devices" && method == "GET")
                    {
                        List<Dictionary<string, object>> devs = new List<Dictionary<string, object>>();
                        lock (_syncLock)
                        {
                            devs.AddRange(_registeredDevices.Values);
                        }
                        Dictionary<string, object> resp = new Dictionary<string, object>();
                        resp["success"] = true;
                        resp["devices"] = devs;
                        SendJsonResponse(stream, 200, serializer.Serialize(resp));
                        return;
                    }

                    if (path == "/api/discovered-desks" || path == "/api/relay/discovered")
                    {
                        List<DiscoveredDeskInfo> dList = LanDiscoveryEngine.Instance.GetDiscoveredDesks();
                        Dictionary<string, object> dResp = new Dictionary<string, object>();
                        dResp["success"] = true;
                        dResp["desks"] = dList;
                        dResp["count"] = dList.Count;
                        SendJsonResponse(stream, 200, serializer.Serialize(dResp));
                        return;
                    }

                    if (path == "/api/discovered-desks/probe")
                    {
                        LanDiscoveryEngine.Instance.BroadcastQuery("*");
                        SendJsonResponse(stream, 200, "{\"success\":true,\"probed\":true}");
                        return;
                    }

                    if (path == "/api/relay/register" && method == "POST")
                    {
                        string bodyStr = System.Text.Encoding.UTF8.GetString(bodyBytes);
                        Dictionary<string, object> devDict = serializer.Deserialize<Dictionary<string, object>>(bodyStr);
                        if (devDict != null)
                        {
                            string devId = devDict.ContainsKey("id") ? Convert.ToString(devDict["id"]) : "";
                            if (!string.IsNullOrEmpty(devId))
                            {
                                lock (_syncLock)
                                {
                                    _registeredDevices[devId] = devDict;
                                }
                            }
                        }
                        SendJsonResponse(stream, 200, "{\"success\":true,\"registered\":true}");
                        return;
                    }

                    if (path == "/api/relay/push_bin" && method == "POST")
                    {
                        string tid = queryParams.ContainsKey("targetId") ? queryParams["targetId"] : "";
                        if (string.IsNullOrEmpty(tid) && headers.ContainsKey("X-Target-Id")) tid = headers["X-Target-Id"];
                        if (!string.IsNullOrEmpty(tid) && bodyBytes.Length > 0)
                        {
                            lock (_syncLock)
                            {
                                _binaryFrames[tid] = bodyBytes;
                            }
                        }
                        SendJsonResponse(stream, 200, string.Format("{{\"success\":true,\"bytes\":{0}}}", bodyBytes.Length));
                        return;
                    }

                    if ((path == "/api/relay/push_frame" || path == "/api/relay/push") && method == "POST")
                    {
                        string bodyStr = System.Text.Encoding.UTF8.GetString(bodyBytes);
                        Dictionary<string, object> payload = serializer.Deserialize<Dictionary<string, object>>(bodyStr);
                        if (payload != null)
                        {
                            string target = payload.ContainsKey("targetId") ? Convert.ToString(payload["targetId"]) : "";
                            string frame = payload.ContainsKey("frame") ? Convert.ToString(payload["frame"]) : "";
                            if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(frame))
                            {
                                lock (_syncLock)
                                {
                                    _base64Frames[target] = frame;
                                    try
                                    {
                                        string pureB64 = frame;
                                        if (pureB64.Contains(",")) pureB64 = pureB64.Substring(pureB64.IndexOf(',') + 1);
                                        _binaryFrames[target] = Convert.FromBase64String(pureB64);
                                    }
                                    catch { }
                                }
                            }
                        }
                        SendJsonResponse(stream, 200, "{\"success\":true}");
                        return;
                    }

                    if (path == "/api/relay/authorize" && method == "POST")
                    {
                        string bodyStr = System.Text.Encoding.UTF8.GetString(bodyBytes);
                        Dictionary<string, object> payload = serializer.Deserialize<Dictionary<string, object>>(bodyStr);
                        string vTarget = payload != null && payload.ContainsKey("viewer") ? Convert.ToString(payload["viewer"]) : "";
                        string action = payload != null && payload.ContainsKey("action") ? Convert.ToString(payload["action"]) : "accept";
                        if (!string.IsNullOrEmpty(vTarget))
                        {
                            if (action.ToLower() == "accept") AuthorizeViewer(vTarget);
                            else RejectViewer(vTarget);
                        }
                        SendJsonResponse(stream, 200, "{\"success\":true,\"action\":\"" + action + "\"}");
                        return;
                    }

                    if (path == "/api/relay/frame_bin" || path.StartsWith("/api/relay/pull_bin/"))
                    {
                        if (!IsViewerAuthorized(clientIp, sourceDeskId))
                        {
                            SendJsonResponse(stream, 403, "{\"success\":false,\"status\":\"AWAITING_AUTHORIZATION\",\"message\":\"Remote desk has not accepted the connection request yet.\"}");
                            return;
                        }
                        string tid = queryParams.ContainsKey("targetId") ? queryParams["targetId"] : "";
                        if (string.IsNullOrEmpty(tid) && path.StartsWith("/api/relay/pull_bin/"))
                        {
                            tid = path.Substring("/api/relay/pull_bin/".Length).Trim('/');
                        }
                        byte[] fData = null;
                        lock (_syncLock)
                        {
                            if (!string.IsNullOrEmpty(tid) && _binaryFrames.ContainsKey(tid)) fData = _binaryFrames[tid];
                            else if (_binaryFrames.Count > 0)
                            {
                                foreach (var kvp in _binaryFrames) { fData = kvp.Value; break; }
                            }
                        }
                        if (fData != null && fData.Length > 0)
                        {
                            SendBinaryResponse(stream, 200, "image/jpeg", fData);
                        }
                        else
                        {
                            SendSimpleResponse(stream, 204, "text/plain", "");
                        }
                        return;
                    }

                    if (path == "/api/relay/frame" || path.StartsWith("/api/relay/pull_frame/"))
                    {
                        if (!IsViewerAuthorized(clientIp, sourceDeskId))
                        {
                            SendJsonResponse(stream, 403, "{\"success\":false,\"status\":\"AWAITING_AUTHORIZATION\",\"message\":\"Remote desk has not accepted the connection request yet.\"}");
                            return;
                        }
                        string tid = queryParams.ContainsKey("targetId") ? queryParams["targetId"] : "";
                        if (string.IsNullOrEmpty(tid) && path.StartsWith("/api/relay/pull_frame/"))
                        {
                            tid = path.Substring("/api/relay/pull_frame/".Length).Trim('/');
                        }
                        string b64 = "";
                        lock (_syncLock)
                        {
                            if (!string.IsNullOrEmpty(tid) && _base64Frames.ContainsKey(tid)) b64 = _base64Frames[tid];
                            else if (_base64Frames.Count > 0)
                            {
                                foreach (var kvp in _base64Frames) { b64 = kvp.Value; break; }
                            }
                        }
                        Dictionary<string, object> resp = new Dictionary<string, object>();
                        resp["success"] = true;
                        resp["frame"] = b64;
                        SendJsonResponse(stream, 200, serializer.Serialize(resp));
                        return;
                    }

                    if (path.StartsWith("/api/relay/stream_bin/"))
                    {
                        if (!IsViewerAuthorized(clientIp, sourceDeskId))
                        {
                            SendJsonResponse(stream, 403, "{\"success\":false,\"status\":\"AWAITING_AUTHORIZATION\",\"message\":\"Remote desk has not accepted the connection request yet.\"}");
                            return;
                        }
                        string tid = path.Substring("/api/relay/stream_bin/".Length).Trim('/');
                        client.ReceiveTimeout = 0;
                        client.SendTimeout = 0;

                        string streamHeaders = "HTTP/1.1 200 OK\r\n" +
                                               "Content-Type: application/octet-stream\r\n" +
                                               "Cache-Control: no-cache, no-store, must-revalidate\r\n" +
                                               "Pragma: no-cache\r\n" +
                                               "Connection: keep-alive\r\n" +
                                               "Access-Control-Allow-Origin: *\r\n\r\n";
                        byte[] shBytes = System.Text.Encoding.ASCII.GetBytes(streamHeaders);
                        stream.Write(shBytes, 0, shBytes.Length);
                        stream.Flush();

                        byte[] lastSentFrame = null;
                        Stopwatch pingSw = Stopwatch.StartNew();

                        while (_isRunning && client.Connected)
                        {
                            byte[] currentFrame = null;
                            lock (_syncLock)
                            {
                                if (!string.IsNullOrEmpty(tid) && _binaryFrames.ContainsKey(tid)) currentFrame = _binaryFrames[tid];
                                else if (_binaryFrames.Count > 0)
                                {
                                    foreach (var kvp in _binaryFrames) { currentFrame = kvp.Value; break; }
                                }
                            }

                            if (currentFrame != null && (lastSentFrame == null || !ReferenceEquals(currentFrame, lastSentFrame)))
                            {
                                lastSentFrame = currentFrame;
                                byte[] pktHeader = new byte[8];
                                pktHeader[0] = (byte)'J';
                                pktHeader[1] = (byte)'D';
                                pktHeader[2] = (byte)'X';
                                pktHeader[3] = (byte)'D';
                                int len = currentFrame.Length;
                                pktHeader[4] = (byte)((len >> 24) & 0xFF);
                                pktHeader[5] = (byte)((len >> 16) & 0xFF);
                                pktHeader[6] = (byte)((len >> 8) & 0xFF);
                                pktHeader[7] = (byte)(len & 0xFF);

                                stream.Write(pktHeader, 0, 8);
                                stream.Write(currentFrame, 0, currentFrame.Length);
                                stream.Flush();
                                pingSw.Restart();
                            }
                            else if (pingSw.ElapsedMilliseconds > 2000)
                            {
                                byte[] pingPkt = new byte[8];
                                pingPkt[0] = (byte)'P';
                                pingPkt[1] = (byte)'I';
                                pingPkt[2] = (byte)'N';
                                pingPkt[3] = (byte)'G';
                                stream.Write(pingPkt, 0, 8);
                                stream.Flush();
                                pingSw.Restart();
                            }

                            Thread.Sleep(16);
                        }
                        return;
                    }

                    if ((path == "/api/relay/push_input" || path == "/api/relay/input") && method == "POST")
                    {
                        if (!IsViewerAuthorized(clientIp, sourceDeskId))
                        {
                            SendJsonResponse(stream, 403, "{\"success\":false,\"status\":\"INPUT_BLOCKED_UNAUTHORIZED\",\"message\":\"Remote desk has not accepted the connection request yet.\"}");
                            return;
                        }
                        string bodyStr = System.Text.Encoding.UTF8.GetString(bodyBytes);
                        Dictionary<string, object> payload = serializer.Deserialize<Dictionary<string, object>>(bodyStr);
                        if (payload != null)
                        {
                            string target = payload.ContainsKey("targetId") ? Convert.ToString(payload["targetId"]) : "";
                            if (!string.IsNullOrEmpty(target))
                            {
                                Dictionary<string, object> eventToQueue = payload;
                                if (payload.ContainsKey("event") && payload["event"] is Dictionary<string, object>)
                                {
                                    eventToQueue = payload["event"] as Dictionary<string, object>;
                                    if (!eventToQueue.ContainsKey("targetId")) eventToQueue["targetId"] = target;
                                }
                                lock (_syncLock)
                                {
                                    if (!_inputQueues.ContainsKey(target)) _inputQueues[target] = new List<Dictionary<string, object>>();
                                    _inputQueues[target].Add(eventToQueue);
                                }
                            }
                        }
                        SendJsonResponse(stream, 200, "{\"success\":true}");
                        return;
                    }

                    if (path == "/api/relay/input" || path.StartsWith("/api/relay/pull_input/"))
                    {
                        string tid = queryParams.ContainsKey("targetId") ? queryParams["targetId"] : "";
                        if (string.IsNullOrEmpty(tid) && path.StartsWith("/api/relay/pull_input/"))
                        {
                            tid = path.Substring("/api/relay/pull_input/".Length).Trim('/');
                        }
                        List<Dictionary<string, object>> events = new List<Dictionary<string, object>>();
                        lock (_syncLock)
                        {
                            if (!string.IsNullOrEmpty(tid) && _inputQueues.ContainsKey(tid))
                            {
                                events.AddRange(_inputQueues[tid]);
                                _inputQueues[tid].Clear();
                            }
                        }
                        Dictionary<string, object> resp = new Dictionary<string, object>();
                        resp["success"] = true;
                        resp["inputs"] = events;
                        resp["events"] = events;
                        SendJsonResponse(stream, 200, serializer.Serialize(resp));
                        return;
                    }

                    if ((path == "/api/relay/disconnect" || path == "/api/relay/close_session") && method == "POST")
                    {
                        string bodyStr = System.Text.Encoding.UTF8.GetString(bodyBytes);
                        Dictionary<string, object> payload = serializer.Deserialize<Dictionary<string, object>>(bodyStr);
                        string target = payload != null && payload.ContainsKey("targetId") ? Convert.ToString(payload["targetId"]) : "";
                        string reason = payload != null && payload.ContainsKey("reason") ? Convert.ToString(payload["reason"]) : "Remote desk closed connection";
                        if (!string.IsNullOrEmpty(target))
                        {
                            lock (_syncLock)
                            {
                                _binaryFrames.Remove(target);
                                _base64Frames.Remove(target);
                                if (!_inputQueues.ContainsKey(target)) _inputQueues[target] = new List<Dictionary<string, object>>();
                                Dictionary<string, object> closeEv = new Dictionary<string, object>();
                                closeEv["type"] = "session_close";
                                closeEv["reason"] = reason;
                                closeEv["targetId"] = target;
                                _inputQueues[target].Add(closeEv);
                            }
                        }
                        SendJsonResponse(stream, 200, "{\"success\":true,\"message\":\"Session closed and frames flushed\"}");
                        return;
                    }

                    // Audio Loopback Chunk Push & Pull Endpoints
                    if ((path == "/api/relay/audio/chunk" || path == "/api/relay/audio") && method == "POST")
                    {
                        string tid = queryParams.ContainsKey("targetId") ? queryParams["targetId"] : "DEFAULT";
                        byte[] audioData = null;
                        if (headers.ContainsKey("content-type") && headers["content-type"].Contains("application/json"))
                        {
                            try {
                                string bStr = System.Text.Encoding.UTF8.GetString(bodyBytes);
                                Dictionary<string, object> aPayload = serializer.Deserialize<Dictionary<string, object>>(bStr);
                                if (aPayload != null)
                                {
                                    if (aPayload.ContainsKey("targetId")) tid = Convert.ToString(aPayload["targetId"]);
                                    if (aPayload.ContainsKey("chunk")) audioData = Convert.FromBase64String(Convert.ToString(aPayload["chunk"]));
                                }
                            } catch { }
                        }
                        else
                        {
                            audioData = bodyBytes;
                        }

                        if (audioData != null && audioData.Length > 0)
                        {
                            UpdateAudioChunk(tid, audioData);
                            SendJsonResponse(stream, 200, string.Format("{{\"success\":true,\"targetId\":\"{0}\",\"bytes\":{1}}}", tid, audioData.Length));
                        }
                        else
                        {
                            SendJsonResponse(stream, 400, "{\"success\":false,\"error\":\"No audio payload provided\"}");
                        }
                        return;
                    }

                    if (path == "/api/relay/audio/chunk" || path.StartsWith("/api/relay/audio/chunk/") || path == "/api/relay/audio")
                    {
                        string tid = queryParams.ContainsKey("targetId") ? queryParams["targetId"] : "";
                        if (string.IsNullOrEmpty(tid) && path.StartsWith("/api/relay/audio/chunk/"))
                        {
                            tid = path.Substring("/api/relay/audio/chunk/".Length).Trim('/');
                        }
                        if (string.IsNullOrEmpty(tid)) tid = "DEFAULT";

                        byte[] chunk = GetAudioChunk(tid);
                        if (chunk != null && chunk.Length > 0)
                        {
                            if (queryParams.ContainsKey("json") || (headers.ContainsKey("accept") && headers["accept"].Contains("application/json")))
                            {
                                string cJson = string.Format("{{\"success\":true,\"targetId\":\"{0}\",\"chunk\":\"{1}\",\"size\":{2}}}", tid, Convert.ToBase64String(chunk), chunk.Length);
                                SendJsonResponse(stream, 200, cJson);
                            }
                            else
                            {
                                string rawResp = "HTTP/1.1 200 OK\r\n" +
                                                 "Content-Type: audio/wav\r\n" +
                                                 "Content-Length: " + chunk.Length + "\r\n" +
                                                 "Access-Control-Allow-Origin: *\r\n" +
                                                 "Cache-Control: no-cache\r\n\r\n";
                                byte[] rawHead = System.Text.Encoding.ASCII.GetBytes(rawResp);
                                stream.Write(rawHead, 0, rawHead.Length);
                                stream.Write(chunk, 0, chunk.Length);
                                stream.Flush();
                            }
                        }
                        else
                        {
                            SendJsonResponse(stream, 204, "");
                        }
                        return;
                    }

                    // Multi-Monitor Enumeration & Switching Endpoints
                    if (path == "/api/relay/monitors" && method == "GET")
                    {
                        List<Dictionary<string, object>> mons = new List<Dictionary<string, object>>();
                        Screen[] allScreens = Screen.AllScreens;
                        for (int mi = 0; mi < allScreens.Length; mi++)
                        {
                            Dictionary<string, object> mInfo = new Dictionary<string, object>();
                            mInfo["id"] = mi;
                            mInfo["index"] = mi;
                            mInfo["name"] = allScreens[mi].DeviceName;
                            mInfo["isPrimary"] = allScreens[mi].Primary;
                            mInfo["width"] = allScreens[mi].Bounds.Width;
                            mInfo["height"] = allScreens[mi].Bounds.Height;
                            mInfo["left"] = allScreens[mi].Bounds.Left;
                            mInfo["top"] = allScreens[mi].Bounds.Top;
                            mInfo["resolution"] = allScreens[mi].Bounds.Width + "x" + allScreens[mi].Bounds.Height;
                            mons.Add(mInfo);
                        }
                        Dictionary<string, object> mResp = new Dictionary<string, object>();
                        mResp["success"] = true;
                        mResp["monitors"] = mons;
                        SendJsonResponse(stream, 200, serializer.Serialize(mResp));
                        return;
                    }

                    if (path == "/api/relay/monitors/switch" && method == "POST")
                    {
                        int targetIdx = 0;
                        try {
                            string bStr = System.Text.Encoding.UTF8.GetString(bodyBytes);
                            Dictionary<string, object> mDict = serializer.Deserialize<Dictionary<string, object>>(bStr);
                            if (mDict != null && mDict.ContainsKey("monitorIndex"))
                            {
                                targetIdx = Convert.ToInt32(mDict["monitorIndex"]);
                            }
                        } catch { }
                        MainForm.SetActiveHostMonitorIndex(targetIdx);
                        SendJsonResponse(stream, 200, string.Format("{{\"success\":true,\"activeMonitor\":{0}}}", targetIdx));
                        return;
                    }

                    // UAC Elevation Status & Elevation Trigger
                    if (path == "/api/system/elevation/status" && method == "GET")
                    {
                        bool isElevated = false;
                        try {
                            System.Security.Principal.WindowsPrincipal p = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent());
                            isElevated = p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                        } catch { }

                        Dictionary<string, object> elResp = new Dictionary<string, object>();
                        elResp["success"] = true;
                        elResp["isElevated"] = isElevated;
                        elResp["integrityLevel"] = isElevated ? "High" : "Medium";
                        elResp["uiAccessAvailable"] = isElevated;
                        elResp["secureDesktopCapable"] = isElevated;
                        elResp["platform"] = "win32";
                        SendJsonResponse(stream, 200, serializer.Serialize(elResp));
                        return;
                    }

                    if (path == "/api/system/elevation/elevate" && method == "POST")
                    {
                        bool ok = false;
                        try {
                            string exe = Application.ExecutablePath;
                            ProcessStartInfo psi = new ProcessStartInfo(exe);
                            psi.UseShellExecute = true;
                            psi.Verb = "runas";
                            Process.Start(psi);
                            ok = true;
                        } catch { }

                        SendJsonResponse(stream, 200, string.Format("{{\"success\":{0},\"elevateTriggered\":{1}}}", ok ? "true" : "false", ok ? "true" : "false"));
                        return;
                    }

                    if (path == "/api/version" || path == "/api/app-info")
                    {
                        string localExe = Application.ExecutablePath;
                        long sz = 0;
                        try { if (File.Exists(localExe)) sz = new FileInfo(localExe).Length; } catch { }
                        Dictionary<string, object> vInfo = new Dictionary<string, object>();
                        vInfo["success"] = true;
                        vInfo["name"] = "jDroid-X-Desk";
                        vInfo["version"] = "2.5.0";
                        vInfo["exeSize"] = sz;
                        vInfo["exeSizeKb"] = sz / 1024;
                        vInfo["downloadUrl"] = "/api/download-exe";
                        vInfo["timestamp"] = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                        SendJsonResponse(stream, 200, serializer.Serialize(vInfo));
                        return;
                    }

                    if (path == "/api/download-exe" || path == "/download/exe" || path == "/download-exe")
                    {
                        string localExe = Application.ExecutablePath;
                        if (File.Exists(localExe))
                        {
                            byte[] exeBytes = null;
                            using (FileStream fs = new FileStream(localExe, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                exeBytes = new byte[fs.Length];
                                fs.Read(exeBytes, 0, exeBytes.Length);
                            }
                            string dlHeaders = "HTTP/1.1 200 OK\r\n" +
                                               "Content-Type: application/octet-stream\r\n" +
                                               "Content-Disposition: attachment; filename=\"jDroid-X-Desk.exe\"\r\n" +
                                               "Content-Length: " + exeBytes.Length + "\r\n" +
                                               "Cache-Control: no-cache, no-store\r\n" +
                                               "Connection: close\r\n" +
                                               "Access-Control-Allow-Origin: *\r\n\r\n";
                            byte[] dh = System.Text.Encoding.ASCII.GetBytes(dlHeaders);
                            stream.Write(dh, 0, dh.Length);
                            stream.Write(exeBytes, 0, exeBytes.Length);
                            stream.Flush();
                            return;
                        }
                        SendSimpleResponse(stream, 404, "application/json", "{\"success\":false,\"error\":\"Executable not found\"}");
                        return;
                    }

                    if (path == "/view" || path.StartsWith("/view/") || path == "/web")
                    {
                        string tid = queryParams.ContainsKey("targetId") ? queryParams["targetId"] : "";
                        if (string.IsNullOrEmpty(tid) && path.StartsWith("/view/"))
                        {
                            tid = path.Substring("/view/".Length).Trim('/');
                        }
                        string html = GetHtml5ViewerSource(tid);
                        SendHtmlResponse(stream, html);
                        return;
                    }

                    SendJsonResponse(stream, 200, "{\"success\":true,\"message\":\"jDroid-X Native Relay ready\"}");
                }
            }
            catch
            {
            }
            finally
            {
                try { client.Close(); } catch { }
            }
        }

        private static void SendJsonResponse(Stream stream, int statusCode, string json)
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            string headers = string.Format("HTTP/1.1 {0} OK\r\n" +
                                           "Content-Type: application/json; charset=utf-8\r\n" +
                                           "Content-Length: {1}\r\n" +
                                           "Access-Control-Allow-Origin: *\r\n" +
                                           "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
                                           "Access-Control-Allow-Headers: *\r\n" +
                                           "Connection: close\r\n\r\n", statusCode, body.Length);
            byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(headers);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(body, 0, body.Length);
            stream.Flush();
        }

        private static void SendBinaryResponse(Stream stream, int statusCode, string contentType, byte[] data)
        {
            string headers = string.Format("HTTP/1.1 {0} OK\r\n" +
                                           "Content-Type: {1}\r\n" +
                                           "Content-Length: {2}\r\n" +
                                           "Access-Control-Allow-Origin: *\r\n" +
                                           "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
                                           "Access-Control-Allow-Headers: *\r\n" +
                                           "Connection: close\r\n\r\n", statusCode, contentType, data.Length);
            byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(headers);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        private static void SendSimpleResponse(Stream stream, int statusCode, string contentType, string text)
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(text);
            string headers = string.Format("HTTP/1.1 {0} OK\r\n" +
                                           "Content-Type: {1}\r\n" +
                                           "Content-Length: {2}\r\n" +
                                           "Access-Control-Allow-Origin: *\r\n" +
                                           "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
                                           "Access-Control-Allow-Headers: *\r\n" +
                                           "Connection: close\r\n\r\n", statusCode, contentType, body.Length);
            byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(headers);
            stream.Write(headerBytes, 0, headerBytes.Length);
            if (body.Length > 0) stream.Write(body, 0, body.Length);
            stream.Flush();
        }

        private static void SendHtmlResponse(Stream stream, string html)
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(html);
            string headers = string.Format("HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/html; charset=utf-8\r\n" +
                                           "Content-Length: {0}\r\n" +
                                           "Access-Control-Allow-Origin: *\r\n" +
                                           "Connection: close\r\n\r\n", body.Length);
            byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(headers);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(body, 0, body.Length);
            stream.Flush();
        }

        private static string GetHtml5ViewerSource(string targetId)
        {
            return "<!DOCTYPE html>\r\n" +
                   "<html lang=\"en\">\r\n" +
                   "<head>\r\n" +
                   "<meta charset=\"UTF-8\">\r\n" +
                   "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
                   "<title>jDroid-X-Desk | Live Remote Desktop Web Viewer</title>\r\n" +
                   "<style>\r\n" +
                   "* { box-sizing: border-box; margin: 0; padding: 0; }\r\n" +
                   "body { background: #070b12; color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; overflow: hidden; height: 100vh; display: flex; flex-direction: column; }\r\n" +
                   "#hud { height: 44px; background: rgba(15, 23, 42, 0.95); border-bottom: 1px solid rgba(56, 189, 248, 0.2); display: flex; align-items: center; justify-content: space-between; padding: 0 16px; font-size: 13px; z-index: 100; backdrop-filter: blur(12px); }\r\n" +
                   ".badge { display: inline-flex; align-items: center; gap: 6px; padding: 4px 10px; border-radius: 9999px; background: rgba(16, 185, 129, 0.15); border: 1px solid rgba(16, 185, 129, 0.4); color: #34d399; font-weight: 600; font-size: 11px; }\r\n" +
                   ".pulse { width: 8px; height: 8px; border-radius: 50%; background: #10b981; box-shadow: 0 0 10px #10b981; animation: pulse 1.5s infinite; }\r\n" +
                   "@keyframes pulse { 0% { opacity: 0.4; } 50% { opacity: 1; } 100% { opacity: 0.4; } }\r\n" +
                   ".hud-title { font-weight: 700; color: #38bdf8; letter-spacing: 0.5px; }\r\n" +
                   ".btn { background: #1e293b; color: #cbd5e1; border: 1px solid #334155; padding: 5px 12px; border-radius: 6px; cursor: pointer; font-size: 12px; transition: all 0.15s ease; }\r\n" +
                   ".btn:hover { background: #334155; color: #fff; border-color: #38bdf8; }\r\n" +
                   "#viewport-container { flex: 1; position: relative; display: flex; align-items: center; justify-content: center; background: #030712; }\r\n" +
                   "#screen { max-width: 100%; max-height: 100%; object-fit: contain; cursor: crosshair; }\r\n" +
                   "#stats { font-family: monospace; font-size: 11px; color: #94a3b8; }\r\n" +
                   "</style>\r\n" +
                   "</head>\r\n" +
                   "<body>\r\n" +
                   "<div id=\"hud\">\r\n" +
                   "  <div style=\"display:flex; align-items:center; gap:12px;\">\r\n" +
                   "    <div class=\"pulse\"></div>\r\n" +
                   "    <span class=\"hud-title\">jDroid-X Live Web View</span>\r\n" +
                   "    <span class=\"badge\">PORT 8088 NATIVE</span>\r\n" +
                   "    <span id=\"stats\">Target: " + (string.IsNullOrEmpty(targetId) ? "AUTO" : targetId) + " | FPS: <span id=\"fps\">0</span> | Latency: <span id=\"lat\">0</span>ms</span>\r\n" +
                   "  </div>\r\n" +
                   "  <div style=\"display:flex; align-items:center; gap:8px;\">\r\n" +
                   "    <button class=\"btn\" onclick=\"toggleFullscreen()\">⛶ Fullscreen</button>\r\n" +
                   "  </div>\r\n" +
                   "</div>\r\n" +
                   "<div id=\"viewport-container\">\r\n" +
                   "  <img id=\"screen\" src=\"\" alt=\"Connecting to remote desk...\">\r\n" +
                   "</div>\r\n" +
                   "<script>\r\n" +
                   "var targetId = '" + (targetId ?? "") + "';\r\n" +
                   "var img = document.getElementById('screen');\r\n" +
                   "var fpsSpan = document.getElementById('fps');\r\n" +
                   "var latSpan = document.getElementById('lat');\r\n" +
                   "var lastFrameTime = performance.now();\r\n" +
                   "var frames = 0;\r\n" +
                   "var isFetching = false;\r\n" +
                   "function fetchFrame() {\r\n" +
                   "  if (isFetching) return;\r\n" +
                   "  isFetching = true;\r\n" +
                   "  var start = performance.now();\r\n" +
                   "  var url = '/api/relay/frame_bin?targetId=' + encodeURIComponent(targetId) + '&t=' + Date.now();\r\n" +
                   "  fetch(url)\r\n" +
                   "    .then(function(res) { if (res.status === 200) return res.blob(); return null; })\r\n" +
                   "    .then(function(blob) {\r\n" +
                   "      if (blob) {\r\n" +
                   "        var oldSrc = img.src;\r\n" +
                   "        img.src = URL.createObjectURL(blob);\r\n" +
                   "        if (oldSrc.indexOf('blob:') === 0) URL.revokeObjectURL(oldSrc);\r\n" +
                   "        frames++;\r\n" +
                   "        var rtt = Math.round(performance.now() - start);\r\n" +
                   "        latSpan.textContent = rtt;\r\n" +
                   "      }\r\n" +
                   "      isFetching = false;\r\n" +
                   "    })\r\n" +
                   "    .catch(function() { isFetching = false; });\r\n" +
                   "}\r\n" +
                   "setInterval(fetchFrame, 33);\r\n" +
                   "setInterval(function() {\r\n" +
                   "  var now = performance.now();\r\n" +
                   "  var elapsed = (now - lastFrameTime) / 1000;\r\n" +
                   "  var fps = Math.round(frames / elapsed);\r\n" +
                   "  fpsSpan.textContent = fps;\r\n" +
                   "  frames = 0;\r\n" +
                   "  lastFrameTime = now;\r\n" +
                   "}, 1000);\r\n" +
                   "function sendInput(action, ev) {\r\n" +
                   "  var rect = img.getBoundingClientRect();\r\n" +
                   "  if (!rect.width || !rect.height) return;\r\n" +
                   "  var scaleX = 1920 / rect.width;\r\n" +
                   "  var scaleY = 1080 / rect.height;\r\n" +
                   "  var payload = {\r\n" +
                   "    targetId: targetId,\r\n" +
                   "    type: 'mouse',\r\n" +
                   "    action: action,\r\n" +
                   "    x: Math.round((ev.clientX - rect.left) * scaleX),\r\n" +
                   "    y: Math.round((ev.clientY - rect.top) * scaleY),\r\n" +
                   "    button: ev.button === 2 ? 'right' : 'left'\r\n" +
                   "  };\r\n" +
                   "  fetch('/api/relay/push_input', {\r\n" +
                   "    method: 'POST',\r\n" +
                   "    headers: { 'Content-Type': 'application/json' },\r\n" +
                   "    body: JSON.stringify(payload)\r\n" +
                   "  }).catch(function() {});\r\n" +
                   "}\r\n" +
                   "img.addEventListener('mousedown', function(e) { sendInput('down', e); e.preventDefault(); });\r\n" +
                   "img.addEventListener('mouseup', function(e) { sendInput('up', e); e.preventDefault(); });\r\n" +
                   "img.addEventListener('contextmenu', function(e) { e.preventDefault(); });\r\n" +
                   "function toggleFullscreen() {\r\n" +
                   "  if (!document.fullscreenElement) document.documentElement.requestFullscreen();\r\n" +
                   "  else document.exitFullscreen();\r\n" +
                   "}\r\n" +
                   "</script>\r\n" +
                   "</body>\r\n" +
                   "</html>";
        }
    }

    // ==========================================================================
    // 2.6 NATIVE P2P LAN DISCOVERY ENGINE (ZERO-CONFIG ANYDESK-STYLE UDP BEACON)
    // ==========================================================================

}
