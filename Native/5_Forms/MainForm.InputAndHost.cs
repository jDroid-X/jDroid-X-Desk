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
    public partial class MainForm
    {
        // ======================================================================
        // GOOGLE COLAB CENTRAL CLOUD RELAY & jDroid-X SIGNALING BROKER
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

                    if (!P2PUdpEngine.Instance.IsRunning)
                    {
                        P2PUdpEngine.Instance.Start(0);
                    }
                    int p2pPort = P2PUdpEngine.Instance.LocalPort;
                    string p2pEp = P2PUdpEngine.Instance.PublicReflexiveEndPoint != null ? P2PUdpEngine.Instance.PublicReflexiveEndPoint.ToString() : "";

                    string hbJson = string.Format("{{\"deskId\":\"{0}\",\"name\":\"{1}\",\"os\":\"Windows\",\"version\":\"2.0.0\",\"tunnelUrl\":\"https://desk.jdroid-x.com\",\"p2pPort\":{2},\"publicEp\":\"{3}\"}}",
                        currentHostDesktop.RemoteId, currentHostDesktop.Name, p2pPort, p2pEp);
                    byte[] hbBytes = System.Text.Encoding.UTF8.GetBytes(hbJson);

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
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) { }

                            // Register presence with jDroid-X Central Signaling & Heartbeat Engine
                            string hbEndpoint = rUrl.TrimEnd('/') + "/api/signaling/heartbeat";
                            HttpWebRequest hbReq = (HttpWebRequest)WebRequest.Create(hbEndpoint);
                            hbReq.Method = "POST";
                            hbReq.ContentType = "application/json";
                            hbReq.Timeout = 1500;
                            hbReq.ContentLength = hbBytes.Length;
                            using (Stream os = hbReq.GetRequestStream())
                            {
                                os.Write(hbBytes, 0, hbBytes.Length);
                            }
                            using (HttpWebResponse hbResp = (HttpWebResponse)hbReq.GetResponse())
                            {
                                if ((int)hbResp.StatusCode == 200) break;
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
            try { SetThreadExecutionState(ES_CONTINUOUS); } catch { }
            try { WasapiAudioCaptureService.Instance.Stop(); } catch { }
            if (screenStreamerThread != null && screenStreamerThread.IsAlive)
            {
                try { screenStreamerThread.Join(300); } catch { }
            }
            screenStreamerThread = null;
        }

        private void ScreenStreamerWorker()
        {
            try { SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED | ES_AWAYMODE_REQUIRED); } catch { }
            try { WasapiAudioCaptureService.Instance.Start(); } catch { }
            ulong lastFrameHash = 0;
            int staticFrameCount = 0;
            byte[] lastWavChunk = null;
            while (isScreenStreamingActive)
            {
                bool pushSuccess = false;
                try {
                    if (db != null && db.Settings != null && db.Settings.connection != null && currentHostDesktop != null)
                    {
                        List<string> candidateUrls = GetRelayCandidateUrls();

                        // Multi-Monitor resolution & bounds targeting
                        Screen targetScreen = (activeHostMonitorIndex >= 0 && activeHostMonitorIndex < Screen.AllScreens.Length) ? Screen.AllScreens[activeHostMonitorIndex] : Screen.PrimaryScreen;
                        Rectangle screenRect = targetScreen.Bounds;
                        int sw = screenRect.Width;
                        int sh = screenRect.Height;
                        // 4K Downscale Clamp: preserve aspect ratio while bounding memory & CPU usage
                        int tw = Math.Min(1920, sw);
                        int th = (int)(sh * ((float)tw / sw));

                        // Audio Loopback Chunk Sync
                        try {
                            byte[] curWav = WasapiAudioCaptureService.Instance.GetLatestWavChunk();
                            if (curWav != null && curWav != lastWavChunk)
                            {
                                lastWavChunk = curWav;
                                EmbeddedRelayServer.Instance.UpdateAudioChunk(currentHostDesktop.RemoteId, curWav);
                                byte[] localWav = curWav;
                                string aTargetId = currentHostDesktop.RemoteId;
                                ThreadPool.QueueUserWorkItem(delegate {
                                    foreach (string rUrl in candidateUrls)
                                    {
                                        try {
                                            HttpWebRequest aReq = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/audio/chunk?targetId=" + aTargetId);
                                            aReq.Method = "POST";
                                            aReq.ContentType = "audio/wav";
                                            aReq.Timeout = 800;
                                            aReq.ContentLength = localWav.Length;
                                            using (Stream aOs = aReq.GetRequestStream()) { aOs.Write(localWav, 0, localWav.Length); }
                                            using (HttpWebResponse aResp = (HttpWebResponse)aReq.GetResponse()) { break; }
                                        } catch { }
                                    }
                                });
                            }
                        } catch { }

                        string b64 = null;
                        using (Bitmap fullBmp = DxgiCaptureService.Instance.CaptureScreen(screenRect))
                        {
                            using (Graphics g = Graphics.FromImage(fullBmp))
                            {
                                // Real OS Hardware Cursor Blitting
                                try {
                                    CURSORINFO pci;
                                    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                                    if (GetCursorInfo(out pci) && pci.flags == CURSOR_SHOWING)
                                    {
                                        int cursorX = pci.ptScreenPos.x - screenRect.Left;
                                        int cursorY = pci.ptScreenPos.y - screenRect.Top;
                                        if (cursorX >= 0 && cursorX < sw && cursorY >= 0 && cursorY < sh)
                                        {
                                            IntPtr hdc = g.GetHdc();
                                            try {
                                                DrawIcon(hdc, cursorX, cursorY, pci.hCursor);
                                            } finally {
                                                g.ReleaseHdc(hdc);
                                            }
                                        }
                                    }
                                } catch { }
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
                                    long jpgQuality = 45L;
                                    if (db != null && db.ServerConfig != null && db.ServerConfig.processingLimits != null)
                                    {
                                        int bw = db.ServerConfig.processingLimits.bandwidthLimitMbps;
                                        if (bw >= 50) jpgQuality = 65L;
                                        else if (bw >= 25) jpgQuality = 50L;
                                        else if (bw < 15) jpgQuality = 35L;
                                    }
                                    ImageCodecInfo jpgEncoder = GetCachedJpegEncoder();
                                    using (EncoderParameters ep = new EncoderParameters(1))
                                    using (EncoderParameter epParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpgQuality))
                                    {
                                        ep.Param[0] = epParam;
                                        if (jpgEncoder != null) scaledBmp.Save(ms, jpgEncoder, ep);
                                        else scaledBmp.Save(ms, ImageFormat.Jpeg);
                                    }
                                    b64 = Convert.ToBase64String(ms.ToArray());
                                    byte[] rawJpgBytes = ms.ToArray();

                                    // Differential Tile Delta Optimization (32x32 DeskRT Dirty-Rect Codec)
                                    TileFrameDiffResult tileDiff = TileCodecService.Instance.EncodeDifferentialFrame(scaledBmp, false);
                                    if (tileDiff != null && tileDiff.DirtyCount == 0 && staticFrameCount < 18)
                                    {
                                        staticFrameCount++;
                                    }

                                    // P2P UDP Direct Peer Broadcast (Zero-Proxy UDP Pipeline)
                                    if (P2PUdpEngine.Instance.IsRunning && rawJpgBytes != null && rawJpgBytes.Length > 0)
                                    {
                                        P2PUdpEngine.Instance.SendPacket(rawJpgBytes);
                                    }

                                    ulong frameHash = 14695981039346656037UL;
                                    int step = Math.Max(1, rawJpgBytes.Length / 512);
                                    for (int i = 0; i < rawJpgBytes.Length; i += step)
                                    {
                                        frameHash ^= rawJpgBytes[i];
                                        frameHash *= 1099511628211UL;
                                    }

                                    if (frameHash == lastFrameHash && staticFrameCount < 18)
                                    {
                                        staticFrameCount++;
                                        Thread.Sleep(25);
                                        continue;
                                    }
                                    lastFrameHash = frameHash;
                                    staticFrameCount = 0;

                                    foreach (string rUrl in candidateUrls)
                                    {
                                        // 1. Try High-Speed RAW Binary Stream Push (0 Base64, 0 JSON overhead - 60 FPS)
                                        try {
                                            HttpWebRequest reqBin = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/push_bin?targetId=" + currentHostDesktop.RemoteId);
                                            reqBin.Method = "POST";
                                            reqBin.ContentType = "image/jpeg";
                                            reqBin.Headers["X-Target-Id"] = currentHostDesktop.RemoteId;
                                            reqBin.Timeout = 1200;
                                            reqBin.Proxy = null;
                                            reqBin.KeepAlive = true;
                                            reqBin.ServicePoint.UseNagleAlgorithm = false;
                                            reqBin.ServicePoint.Expect100Continue = false;
                                            reqBin.ContentLength = rawJpgBytes.Length;
                                            using (Stream os = reqBin.GetRequestStream())
                                            {
                                                os.Write(rawJpgBytes, 0, rawJpgBytes.Length);
                                            }
                                            using (HttpWebResponse resp = (HttpWebResponse)reqBin.GetResponse())
                                            {
                                                pushSuccess = true;
                                                bytesTransferred += rawJpgBytes.Length;
                                                break;
                                            }
                                        } catch { }

                                        // 2. Fallback to JSON Base64 push
                                        if (!pushSuccess && !string.IsNullOrEmpty(b64))
                                        {
                                            try {
                                                string postJson = string.Format("{{\"targetId\":\"{0}\",\"frame\":\"data:image/jpeg;base64,{1}\"}}",
                                                    currentHostDesktop.RemoteId, b64);
                                                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postJson);
                                                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/push_frame");
                                                req.Method = "POST";
                                                req.ContentType = "application/json";
                                                req.Timeout = 1500;
                                                req.Proxy = null;
                                                req.KeepAlive = true;
                                                req.ServicePoint.UseNagleAlgorithm = false;
                                                req.ServicePoint.Expect100Continue = false;
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
                            }
                        }
                    }
                } catch { }
                int targetFps = (db != null && db.ServerConfig != null && db.ServerConfig.processingLimits != null && db.ServerConfig.processingLimits.maxFps > 0) ? db.ServerConfig.processingLimits.maxFps : 60;
                int frameDelayMs = Math.Max(8, 1000 / targetFps);
                Thread.Sleep(pushSuccess ? frameDelayMs : 400);
            }
        }


        // ======================================================================
        // HOST DAEMON & BIDIRECTIONAL REMOTE ADMIN INPUT ENGINE
        // ======================================================================

        public void StartHostDaemon()
        {
            if (isHostDaemonRunning) return;
            isHostDaemonRunning = true;
            hostDaemonThread = new Thread(new ThreadStart(HostDaemonWorker)) { IsBackground = true };
            hostDaemonThread.Start();
        }

        public void StopHostDaemon()
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

                    // 1.5 Process pending in-memory input events from EmbeddedRelayServer
                    if (currentHostDesktop != null)
                    {
                        List<Dictionary<string, object>> localInputs = EmbeddedRelayServer.Instance.GetPendingInputEvents(currentHostDesktop.RemoteId);
                        if (localInputs != null && localInputs.Count > 0)
                        {
                            for (int li = 0; li < localInputs.Count; li++)
                            {
                                ExecuteRemoteInputEvent(localInputs[li]);
                            }
                        }
                    }

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
                                    if (dict != null && dict.ContainsKey("success") && Convert.ToBoolean(dict["success"]))
                                    {
                                        object rawList = dict.ContainsKey("inputs") ? dict["inputs"] : (dict.ContainsKey("events") ? dict["events"] : null);
                                        System.Collections.IEnumerable list = rawList as System.Collections.IEnumerable;
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

                // Dynamic throttle: 16ms (60 FPS) when actively serving viewer, 500ms when idling
                bool isServingViewer = isSessionActive || isHostServingRemoteClient || EmbeddedRelayServer.Instance.HasActiveViewers();
                Thread.Sleep(isServingViewer ? 16 : 500);
            }
        }

        private void PushLocalHostFrame()
        {
            try
            {
                if (db == null || db.Settings == null || db.Settings.connection == null || currentHostDesktop == null)
                    return;

                List<string> candidateUrls = GetRelayCandidateUrls();
                Screen targetScreen = (activeHostMonitorIndex >= 0 && activeHostMonitorIndex < Screen.AllScreens.Length) ? Screen.AllScreens[activeHostMonitorIndex] : Screen.PrimaryScreen;
                Rectangle screenRect = targetScreen.Bounds;
                int sw = screenRect.Width;
                int sh = screenRect.Height;
                int tw = Math.Min(1920, sw);
                int th = (int)(sh * ((float)tw / sw));

                // Audio chunk update
                try {
                    byte[] curWav = WasapiAudioCaptureService.Instance.GetLatestWavChunk();
                    if (curWav != null && curWav.Length > 0)
                    {
                        EmbeddedRelayServer.Instance.UpdateAudioChunk(currentHostDesktop.RemoteId, curWav);
                    }
                } catch { }

                string b64 = null;
                byte[] rawBytes = null;
                using (Bitmap fullBmp = new Bitmap(sw, sh))
                {
                    using (Graphics g = Graphics.FromImage(fullBmp))
                    {
                        g.CopyFromScreen(screenRect.Left, screenRect.Top, 0, 0, screenRect.Size);

                        // Real OS Hardware Cursor Blitting
                        try {
                            CURSORINFO pci;
                            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                            if (GetCursorInfo(out pci) && pci.flags == CURSOR_SHOWING)
                            {
                                int cursorX = pci.ptScreenPos.x - screenRect.Left;
                                int cursorY = pci.ptScreenPos.y - screenRect.Top;
                                if (cursorX >= 0 && cursorX < sw && cursorY >= 0 && cursorY < sh)
                                {
                                    IntPtr hdc = g.GetHdc();
                                    try {
                                        DrawIcon(hdc, cursorX, cursorY, pci.hCursor);
                                    } finally {
                                        g.ReleaseHdc(hdc);
                                    }
                                }
                            }
                        } catch { }
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
                            rawBytes = ms.ToArray();
                            b64 = Convert.ToBase64String(rawBytes);
                            EmbeddedRelayServer.Instance.UpdateFrame(currentHostDesktop.RemoteId, rawBytes, b64);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(b64))
                {
                    string targetId = currentHostDesktop.RemoteId;
                    byte[] localRawBytes = rawBytes;
                    ThreadPool.QueueUserWorkItem(delegate {
                        try
                        {
                            foreach (string rUrl in candidateUrls)
                            {
                                // 1. Fast binary push first
                                try
                                {
                                    HttpWebRequest reqBin = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/push_bin?targetId=" + targetId);
                                    reqBin.Method = "POST";
                                    reqBin.ContentType = "image/jpeg";
                                    reqBin.Headers["X-Target-Id"] = targetId;
                                    reqBin.Timeout = 1000;
                                    reqBin.Proxy = null;
                                    reqBin.ContentLength = localRawBytes.Length;
                                    using (Stream os = reqBin.GetRequestStream())
                                    {
                                        os.Write(localRawBytes, 0, localRawBytes.Length);
                                    }
                                    using (HttpWebResponse resp = (HttpWebResponse)reqBin.GetResponse())
                                    {
                                        cachedWinningRelayUrl = rUrl;
                                        lastWinningCheckTime = DateTime.Now;
                                        bytesTransferred += localRawBytes.Length;
                                        break;
                                    }
                                }
                                catch { }

                                // 2. Fallback to json base64 push
                                try
                                {
                                    string postJson = string.Format("{{\"targetId\":\"{0}\",\"frame\":\"data:image/jpeg;base64,{1}\"}}", targetId, b64);
                                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postJson);
                                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/push_frame");
                                    req.Method = "POST";
                                    req.ContentType = "application/json";
                                    req.Timeout = 1000;
                                    req.Proxy = null;
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
                        catch { }
                    });
                }
            }
            catch { }
        }

        private void ExecuteRemoteInputEvent(Dictionary<string, object> ev)
        {
            try
            {
                if (ev == null) return;
                // Auto-unwrap if payload is wrapped inside "event" property
                if (!ev.ContainsKey("type") && ev.ContainsKey("event") && ev["event"] is Dictionary<string, object>)
                {
                    ev = ev["event"] as Dictionary<string, object>;
                }
                if (!ev.ContainsKey("type")) return;
                string type = ev["type"] as string;

                if (type == "session_start")
                {
                    isHostServingRemoteClient = true;
                    string src = ev.ContainsKey("sourceId") ? (ev["sourceId"] as string) : "Remote Desk";
                    if (miniDeskForm != null && !miniDeskForm.IsDisposed && !isSessionActive)
                    {
                        miniDeskForm.NotifyHostSessionActive(src);
                    }
                    return;
                }
                else if (type == "session_close")
                {
                    isHostServingRemoteClient = false;
                    if (miniDeskForm != null && !miniDeskForm.IsDisposed && !isSessionActive)
                    {
                        miniDeskForm.NotifyHostSessionClosed();
                    }
                    return;
                }

                if (type == "mouse" || type == "wheel")
                {
                    if (!hostPermissionAllowInput) return; // jDroid-X Permission Gate
                    bool hasApprovedViewer = (approvedViewerIps.Count > 0) ||
                                             EmbeddedRelayServer.Instance.HasApprovedViewers() ||
                                             isHostServingRemoteClient ||
                                             (db != null && db.Settings != null && db.Settings.access != null && db.Settings.access.unattendedAccess);
                    if (!hasApprovedViewer) return;

                    // Ensure host knows it is actively serving client
                    if (!isHostServingRemoteClient)
                    {
                        isHostServingRemoteClient = true;
                        if (miniDeskForm != null && !miniDeskForm.IsDisposed && !isSessionActive)
                        {
                            miniDeskForm.NotifyHostSessionActive("Remote Desk");
                        }
                    }

                    string action = ev.ContainsKey("action") ? (ev["action"] as string) : (ev.ContainsKey("act") ? (ev["act"] as string) : (type == "wheel" ? "wheel" : "move"));
                    int rx = ev.ContainsKey("x") ? Convert.ToInt32(ev["x"]) : 0;
                    int ry = ev.ContainsKey("y") ? Convert.ToInt32(ev["y"]) : 0;
                    string btn = ev.ContainsKey("btn") ? (ev["btn"] as string) : "Left";

                    Screen targetScreen = (activeHostMonitorIndex >= 0 && activeHostMonitorIndex < Screen.AllScreens.Length) ? Screen.AllScreens[activeHostMonitorIndex] : Screen.PrimaryScreen;
                    Rectangle sRect = targetScreen.Bounds;
                    int hostSw = sRect.Width;
                    int hostSh = sRect.Height;
                    int tw = Math.Min(1920, hostSw);
                    int th = (int)(hostSh * ((float)tw / hostSw));
                    int realX = tw > 0 ? (int)((float)rx / tw * hostSw) : rx;
                    int realY = th > 0 ? (int)((float)ry / th * hostSh) : ry;

                    realX = sRect.Left + Math.Max(0, Math.Min(hostSw - 1, realX));
                    realY = sRect.Top + Math.Max(0, Math.Min(hostSh - 1, realY));

                    SetCursorPos(realX, realY);

                    if (string.Equals(action, "down", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(btn, "Right", StringComparison.OrdinalIgnoreCase)) mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        else if (string.Equals(btn, "Middle", StringComparison.OrdinalIgnoreCase)) mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                        else mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    }
                    else if (string.Equals(action, "up", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(btn, "Right", StringComparison.OrdinalIgnoreCase)) mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        else if (string.Equals(btn, "Middle", StringComparison.OrdinalIgnoreCase)) mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                        else mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    }
                    else if (string.Equals(action, "click", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(btn, "Right", StringComparison.OrdinalIgnoreCase))
                        {
                            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        }
                        else
                        {
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        }
                    }
                    else if (string.Equals(action, "dblclick", StringComparison.OrdinalIgnoreCase))
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        Thread.Sleep(30);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    }
                    else if (string.Equals(action, "wheel", StringComparison.OrdinalIgnoreCase))
                    {
                        int delta = ev.ContainsKey("delta") ? Convert.ToInt32(ev["delta"]) : (ev.ContainsKey("deltaY") ? -Convert.ToInt32(ev["deltaY"]) : 120);
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, 0);
                    }
                }
                else if (type == "key")
                {
                    if (!hostPermissionAllowInput) return; // jDroid-X Permission Gate
                    bool hasApprovedViewer = (approvedViewerIps.Count > 0) ||
                                             EmbeddedRelayServer.Instance.HasApprovedViewers() ||
                                             isHostServingRemoteClient ||
                                             (db != null && db.Settings != null && db.Settings.access != null && db.Settings.access.unattendedAccess);
                    if (!hasApprovedViewer) return;

                    string action = ev.ContainsKey("action") ? (ev["action"] as string) : (ev.ContainsKey("act") ? (ev["act"] as string) : "down");
                    int keyCode = ev.ContainsKey("keyCode") ? Convert.ToInt32(ev["keyCode"]) : (ev.ContainsKey("key") ? Convert.ToInt32(ev["key"]) : 0);
                    if (keyCode > 0 && keyCode < 256)
                    {
                        byte vk = (byte)keyCode;
                        if (string.Equals(action, "down", StringComparison.OrdinalIgnoreCase)) keybd_event(vk, 0, 0, 0);
                        else if (string.Equals(action, "up", StringComparison.OrdinalIgnoreCase)) keybd_event(vk, 0, KEYEVENTF_KEYUP, 0);
                    }
                }
                else if (type == "clipboard")
                {
                    if (hostPermissionAllowClipboard)
                    {
                        string format = ev.ContainsKey("format") ? (ev["format"] as string) : "text";
                        if (format == "image" && ev.ContainsKey("data"))
                        {
                            string b64 = ev["data"] as string;
                            if (!string.IsNullOrEmpty(b64))
                            {
                                Thread t = new Thread(() => {
                                    try {
                                        byte[] imgBytes = Convert.FromBase64String(b64);
                                        using (MemoryStream ms = new MemoryStream(imgBytes))
                                        using (Image img = Image.FromStream(ms))
                                        {
                                            Clipboard.SetImage(new Bitmap(img));
                                        }
                                        ShowToast("📋 Remote screenshot/image copied to host clipboard.");
                                    } catch { }
                                });
                                t.SetApartmentState(ApartmentState.STA);
                                t.IsBackground = true;
                                t.Start();
                            }
                        }
                        else
                        {
                            string text = ev.ContainsKey("text") ? (ev["text"] as string) : "";
                            if (!string.IsNullOrEmpty(text) && text != lastReceivedClipboardText)
                            {
                                lastReceivedClipboardText = text;
                                lastLocalClipboardText = text;
                                Thread t = new Thread(() => {
                                    try { Clipboard.SetText(text); } catch { }
                                });
                                t.SetApartmentState(ApartmentState.STA);
                                t.IsBackground = true;
                                t.Start();
                                ShowToast("📋 Remote clipboard received and synced to host.");
                            }
                        }
                    }
                }
                else if (type == "chat")
                {
                    string sender = ev.ContainsKey("sender") ? (ev["sender"] as string) : "Remote Desk";
                    string text = ev.ContainsKey("text") ? (ev["text"] as string) : "";
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (this.IsHandleCreated && !this.IsDisposed)
                        {
                            this.BeginInvoke(new Action(() => {
                                ModernNotificationToast.ShowToast("💬 Message from " + sender, text, "info");
                                ShowToast("💬 [" + sender + "]: " + text);
                            }));
                        }
                    }
                }
                else if (type == "admin_cmd")
                {
                    string cmd = ev.ContainsKey("cmd") ? (ev["cmd"] as string) : "";
                    if (cmd.StartsWith("switch_monitor"))
                    {
                        int mIdx = 0;
                        if (cmd.Contains(":")) int.TryParse(cmd.Substring(cmd.IndexOf(':') + 1), out mIdx);
                        else if (cmd.Contains("_")) int.TryParse(cmd.Substring(cmd.LastIndexOf('_') + 1), out mIdx);
                        SetActiveHostMonitorIndex(mIdx);
                        ShowToast("🖥️ Switched to Display " + (mIdx + 1));
                    }
                    else if (cmd == "switch_sides")
                    {
                        string viewerDeskId = ev.ContainsKey("viewerDeskId") ? (ev["viewerDeskId"] as string) : "";
                        HandleSwitchSidesRequested(viewerDeskId);
                    }
                    else
                    {
                        ExecuteAdminCommand(cmd);
                    }
                }
                else if (type == "session_close" || type == "disconnect")
                {
                    string closeReason = ev.ContainsKey("reason") ? Convert.ToString(ev["reason"]) : "Remote desk closed the session.";
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke(new Action(() => {
                            if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
                            {
                                AbortSessionConnection("Connection Closed: " + closeReason);
                                ShowToast("Connection Closed", closeReason, "warning");
                            }
                            else
                            {
                                EmbeddedRelayServer.Instance.DisconnectAllViewers();
                                if (lblRemoteStatus != null)
                                {
                                    lblRemoteStatus.Text = "● Ready";
                                    lblRemoteStatus.ForeColor = Color.FromArgb(148, 163, 184);
                                }
                                ShowToast("Viewer Disconnected", "Remote desk ended session: " + closeReason, "info");
                            }
                        }));
                    }
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
                        if (hostPermissionAllowFiles)
                        {
                            Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
                        }
                        break;
                    case "lock":
                        try { Process.Start("rundll32.exe", "user32.dll,LockWorkStation"); } catch { }
                        break;
                    case "cad":
                        try { Process.Start("taskmgr.exe"); } catch { }
                        break;
                    case "block_input":
                        isLocalInputBlockedByRemote = !isLocalInputBlockedByRemote;
                        try { BlockInput(isLocalInputBlockedByRemote); } catch { }
                        if (chkPermBlockLocal != null && this.IsHandleCreated && !this.IsDisposed)
                        {
                            this.BeginInvoke(new Action(() => {
                                chkPermBlockLocal.Checked = isLocalInputBlockedByRemote;
                            }));
                        }
                        ShowToast(isLocalInputBlockedByRemote ? "🚫 Local host input locked." : "🔓 Local host input unlocked.");
                        break;
                }
            }
            catch { }
        }

        public bool MapCanvasToRemote(int clientX, int clientY, int cw, int ch, out int remoteX, out int remoteY)
        {
            remoteX = 0;
            remoteY = 0;
            if (cw <= 0 || ch <= 0) return false;
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

        public bool MapCanvasToRemote(int clientX, int clientY, out int remoteX, out int remoteY)
        {
            int cw = (viewportCanvas != null) ? viewportCanvas.Width : 0;
            int ch = (viewportCanvas != null) ? viewportCanvas.Height : 0;
            return MapCanvasToRemote(clientX, clientY, cw, ch, out remoteX, out remoteY);
        }

        private void SendRemoteInput(string eventType, Dictionary<string, object> details)
        {
            lastUserActivityUtc = DateTime.UtcNow;
            if (!isSessionActive || string.IsNullOrEmpty(activeReceiverTargetId)) return;
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    List<string> candidateUrls = GetRelayCandidateUrls(activeReceiverTargetId);
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

        public void DispatchRemoteMouseDown(int clientX, int clientY, MouseButtons btn, int cw = -1, int ch = -1)
        {
            if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
            {
                int w = (cw > 0) ? cw : (viewportCanvas != null ? viewportCanvas.Width : 0);
                int h = (ch > 0) ? ch : (viewportCanvas != null ? viewportCanvas.Height : 0);
                int rx, ry;
                if (MapCanvasToRemote(clientX, clientY, w, h, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "down";
                    ev["x"] = rx;
                    ev["y"] = ry;
                    ev["btn"] = btn == MouseButtons.Right ? "Right" : (btn == MouseButtons.Middle ? "Middle" : "Left");
                    SendRemoteInput("mouse", ev);
                }
            }
        }

        public void DispatchRemoteMouseUp(int clientX, int clientY, MouseButtons btn, int cw = -1, int ch = -1)
        {
            if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
            {
                int w = (cw > 0) ? cw : (viewportCanvas != null ? viewportCanvas.Width : 0);
                int h = (ch > 0) ? ch : (viewportCanvas != null ? viewportCanvas.Height : 0);
                int rx, ry;
                if (MapCanvasToRemote(clientX, clientY, w, h, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "up";
                    ev["x"] = rx;
                    ev["y"] = ry;
                    ev["btn"] = btn == MouseButtons.Right ? "Right" : (btn == MouseButtons.Middle ? "Middle" : "Left");
                    SendRemoteInput("mouse", ev);
                }
            }
        }

        public void DispatchRemoteMouseMove(int clientX, int clientY, int cw = -1, int ch = -1)
        {
            if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
            {
                if ((DateTime.Now - lastMouseMoveSent).TotalMilliseconds < 33) return;
                lastMouseMoveSent = DateTime.Now;

                int w = (cw > 0) ? cw : (viewportCanvas != null ? viewportCanvas.Width : 0);
                int h = (ch > 0) ? ch : (viewportCanvas != null ? viewportCanvas.Height : 0);
                int rx, ry;
                if (MapCanvasToRemote(clientX, clientY, w, h, out rx, out ry))
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

        public void DispatchRemoteMouseClick(int clientX, int clientY, MouseButtons btn, int cw = -1, int ch = -1)
        {
            if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
            {
                int w = (cw > 0) ? cw : (viewportCanvas != null ? viewportCanvas.Width : 0);
                int h = (ch > 0) ? ch : (viewportCanvas != null ? viewportCanvas.Height : 0);
                int rx, ry;
                if (MapCanvasToRemote(clientX, clientY, w, h, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "click";
                    ev["x"] = rx;
                    ev["y"] = ry;
                    ev["btn"] = btn == MouseButtons.Right ? "Right" : (btn == MouseButtons.Middle ? "Middle" : "Left");
                    SendRemoteInput("mouse", ev);
                }
            }
        }

        public void DispatchRemoteMouseDoubleClick(int clientX, int clientY, MouseButtons btn, int cw = -1, int ch = -1)
        {
            if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
            {
                int w = (cw > 0) ? cw : (viewportCanvas != null ? viewportCanvas.Width : 0);
                int h = (ch > 0) ? ch : (viewportCanvas != null ? viewportCanvas.Height : 0);
                int rx, ry;
                if (MapCanvasToRemote(clientX, clientY, w, h, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "dblclick";
                    ev["x"] = rx;
                    ev["y"] = ry;
                    ev["btn"] = btn == MouseButtons.Right ? "Right" : "Left";
                    SendRemoteInput("mouse", ev);
                }
            }
        }

        public void DispatchRemoteMouseWheel(int delta, int clientX, int clientY, int cw = -1, int ch = -1)
        {
            if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
            {
                int w = (cw > 0) ? cw : (viewportCanvas != null ? viewportCanvas.Width : 0);
                int h = (ch > 0) ? ch : (viewportCanvas != null ? viewportCanvas.Height : 0);
                int rx, ry;
                if (MapCanvasToRemote(clientX, clientY, w, h, out rx, out ry))
                {
                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["action"] = "wheel";
                    ev["delta"] = delta;
                    ev["x"] = rx;
                    ev["y"] = ry;
                    SendRemoteInput("wheel", ev);
                }
            }
        }

        public bool DispatchRemoteKey(Keys keyData)
        {
            if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
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
            return false;
        }

        private void SendRemoteAdminCommand(string cmd)
        {
            Dictionary<string, object> ev = new Dictionary<string, object>();
            ev["cmd"] = cmd;
            SendRemoteInput("admin_cmd", ev);
        }

        private void SendRemoteDisconnectSignal(string targetId, string reason = "User Disconnected")
        {
            if (string.IsNullOrEmpty(targetId)) return;
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    List<string> candidateUrls = GetRelayCandidateUrls(targetId);
                    Dictionary<string, object> payload = new Dictionary<string, object>();
                    payload["targetId"] = targetId;
                    payload["reason"] = reason;
                    payload["sender"] = currentHostDesktop != null ? currentHostDesktop.RemoteId : "Remote Desk";

                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["type"] = "session_close";
                    ev["reason"] = reason;
                    ev["sender"] = payload["sender"];
                    payload["event"] = ev;

                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    string json = ser.Serialize(payload);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                    foreach (string rUrl in candidateUrls)
                    {
                        try
                        {
                            // 1. Send session_close input event
                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/push_input");
                            req.Method = "POST";
                            req.ContentType = "application/json";
                            req.Timeout = 800;
                            req.ContentLength = bytes.Length;
                            using (Stream os = req.GetRequestStream())
                            {
                                os.Write(bytes, 0, bytes.Length);
                            }
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) { }

                            // 2. Clear relay buffers
                            HttpWebRequest reqDisc = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/disconnect");
                            reqDisc.Method = "POST";
                            reqDisc.ContentType = "application/json";
                            reqDisc.Timeout = 800;
                            reqDisc.ContentLength = bytes.Length;
                            using (Stream os = reqDisc.GetRequestStream())
                            {
                                os.Write(bytes, 0, bytes.Length);
                            }
                            using (HttpWebResponse respDisc = (HttpWebResponse)reqDisc.GetResponse()) { }
                        }
                        catch { }
                    }
                }
                catch { }
            });
        }

        private void CheckAndSyncAppVersion(string targetDeskId)
        {
            if (string.IsNullOrEmpty(targetDeskId)) return;
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    string localExePath = Application.ExecutablePath;
                    if (!File.Exists(localExePath)) return;
                    long localExeSize = new FileInfo(localExePath).Length;
                    long localExeSizeKb = localExeSize / 1024;

                    List<string> candidateUrls = GetRelayCandidateUrls(targetDeskId);
                    foreach (string rUrl in candidateUrls)
                    {
                        try
                        {
                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/version");
                            req.Method = "GET";
                            req.Timeout = 2500;
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                string json = sr.ReadToEnd();
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                Dictionary<string, object> dict = ser.Deserialize<Dictionary<string, object>>(json);
                                if (dict != null && dict.ContainsKey("exeSize"))
                                {
                                    long remoteExeSize = Convert.ToInt64(dict["exeSize"]);
                                    long remoteExeSizeKb = remoteExeSize / 1024;

                                    // If remote desk is running a larger/newer application (diff >= 1024 bytes)
                                    if (remoteExeSize > localExeSize && (remoteExeSize - localExeSize) >= 1024)
                                    {
                                        if (this.IsHandleCreated && !this.IsDisposed)
                                        {
                                            this.BeginInvoke(new Action(() => {
                                                ShowToast("Updating Application", string.Format("Remote Desk is on a newer build ({0} KB vs {1} KB). Downloading update in background...", remoteExeSizeKb, localExeSizeKb), "info");
                                            }));
                                        }

                                        string updateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jDroid-X-Desk.exe.update");
                                        string dlUrl = rUrl + "/api/download-exe";
                                        using (WebClient wc = new WebClient())
                                        {
                                            wc.DownloadFile(dlUrl, updateFilePath);
                                        }

                                        if (File.Exists(updateFilePath))
                                        {
                                            FileInfo fi = new FileInfo(updateFilePath);
                                            if (fi.Length == remoteExeSize)
                                            {
                                                byte[] mz = new byte[2];
                                                using (FileStream fs = new FileStream(updateFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                                {
                                                    fs.Read(mz, 0, 2);
                                                }

                                                if (mz[0] == 0x4D && mz[1] == 0x5A) // 'MZ' PE header verification
                                                {
                                                    if (this.IsHandleCreated && !this.IsDisposed)
                                                    {
                                                        this.BeginInvoke(new Action(() => {
                                                            PromptRestartForUpdate(updateFilePath, remoteExeSizeKb, localExeSizeKb);
                                                        }));
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            });
        }

        private void PromptRestartForUpdate(string updateFilePath, long remoteKb, long localKb)
        {
            int choice = ModernMultiOptionDialog.Show(
                this,
                "jDroid-X-Desk Auto-Update Ready",
                string.Format("Both Desks Synchronization:\n\nRemote Desk is running a newer synchronized build ({0} KB vs local {1} KB).\nThe update package has been downloaded in background.\n\nRestart now to synchronize features and functions with Remote Desk?", remoteKb, localKb),
                new string[] { "⚡ Restart & Sync Now", "Later" }
            );

            if (choice == 0)
            {
                ApplyUpdateAndRestart(updateFilePath);
            }
        }

        private void ApplyUpdateAndRestart(string updateFilePath)
        {
            try
            {
                string currentExe = Application.ExecutablePath;
                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string batPath = Path.Combine(currentDir, "apply_update.bat");

                string batScript = "@echo off\r\n" +
                                   "title jDroid-X-Desk Auto-Updater\r\n" +
                                   "echo Waiting for application to exit...\r\n" +
                                   "timeout /t 1 /nobreak >nul\r\n" +
                                   ":retry\r\n" +
                                   "copy /y \"" + updateFilePath + "\" \"" + currentExe + "\" >nul 2>&1\r\n" +
                                   "if errorlevel 1 (\r\n" +
                                   "    timeout /t 1 /nobreak >nul\r\n" +
                                   "    goto retry\r\n" +
                                   ")\r\n" +
                                   "del \"" + updateFilePath + "\" >nul 2>&1\r\n" +
                                   "echo Launching updated jDroid-X-Desk...\r\n" +
                                   "start \"\" \"" + currentExe + "\"\r\n" +
                                   "del \"%~f0\" >nul 2>&1\r\n" +
                                   "exit\r\n";

                File.WriteAllText(batPath, batScript);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = batPath,
                    WorkingDirectory = currentDir,
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                Process.Start(psi);

                isSessionActive = false;
                AbortSessionConnection("Application restarting for update");
                Application.Exit();
            }
            catch (Exception ex)
            {
                ShowToast("Update Error", "Failed to apply update: " + ex.Message, "error");
            }
        }

        private void SendRemoteConnectSignal(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return;
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    List<string> candidateUrls = GetRelayCandidateUrls(targetId);
                    Dictionary<string, object> payload = new Dictionary<string, object>();
                    payload["targetId"] = targetId;
                    payload["sender"] = currentHostDesktop != null ? currentHostDesktop.RemoteId : "Remote Desk";

                    Dictionary<string, object> ev = new Dictionary<string, object>();
                    ev["type"] = "session_start";
                    ev["sourceId"] = payload["sender"];
                    payload["event"] = ev;

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
                            req.Timeout = 1000;
                            req.ContentLength = bytes.Length;
                            using (Stream os = req.GetRequestStream())
                            {
                                os.Write(bytes, 0, bytes.Length);
                            }
                            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) { break; }
                        }
                        catch { }
                    }
                }
                catch { }
            });
        }

        private void StartFrameReceiver(string targetRemoteId)
        {
            StopFrameReceiver();
            isFrameReceiverActive = true;
            activeReceiverTargetId = targetRemoteId;
            SendRemoteConnectSignal(targetRemoteId);
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

        private static int ReadExactBytes(Stream stream, byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = stream.Read(buffer, offset + totalRead, count - totalRead);
                if (read <= 0) break;
                totalRead += read;
            }
            return totalRead;
        }

        private void ProcessAndRenderFrame(byte[] imgBytes, int rttMs)
        {
            if (imgBytes == null || imgBytes.Length == 0) return;
            try
            {
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
                    lastMeasuredRttMs = Math.Max(1, rttMs);
                }

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

                if (miniDeskForm != null && !miniDeskForm.IsDisposed && miniDeskForm.Visible)
                {
                    miniDeskForm.OnRemoteFrameReceived(imgBytes.Length);
                }
            }
            catch { }
        }

        private void FrameReceiverWorker()
        {
            DateTime workerStart = DateTime.Now;
            DateTime lastFrameReceivedUtc = DateTime.UtcNow;
            while (isFrameReceiverActive && isSessionActive)
            {
                bool successFrame = false;
                try {
                    // Check connecting timeout (45 seconds)
                    if (currentConnectionState == SessionConnectionState.Connecting)
                    {
                        TimeSpan elapsed = DateTime.Now - workerStart;
                        if (elapsed.TotalSeconds > 45)
                        {
                            if (this.IsHandleCreated && !this.IsDisposed)
                            {
                                this.BeginInvoke(new Action(() => {
                                    AbortSessionConnection("Connection Timeout: Remote Desk (" + activeReceiverTargetId + ") is not streaming frames or Colab Relay is offline.");
                                }));
                            }
                            break;
                        }
                        else if (elapsed.TotalSeconds > 15)
                        {
                            connectionStageMessage = "Awaiting Live Video Stream Frames from Desk " + activeReceiverTargetId + "...";
                            connectingProgressPct = Math.Min(90, (int)(20 + elapsed.TotalSeconds * 2.0));
                        }
                        else if (elapsed.TotalSeconds > 3)
                        {
                            connectionStageMessage = "Negotiating P2P Stream via Colab Relay...";
                            connectingProgressPct = Math.Min(65, (int)(15 + elapsed.TotalSeconds * 4.0));
                        }
                    }

                    if (db != null && db.Settings != null && db.Settings.connection != null && !string.IsNullOrEmpty(activeReceiverTargetId))
                    {
                        List<string> candidateUrls = GetRelayCandidateUrls(activeReceiverTargetId);

                        foreach (string rUrl in candidateUrls)
                        {
                            try {
                                // 0. Try High-Speed Persistent Asynchronous Binary Stream (Zero RTT / 60 FPS Engine)
                                try {
                                    HttpWebRequest reqStream = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/stream_bin/" + activeReceiverTargetId);
                                    reqStream.Method = "GET";
                                    reqStream.Proxy = null;
                                    reqStream.Timeout = 6000;
                                    reqStream.ReadWriteTimeout = 6000;
                                    if (currentHostDesktop != null && !string.IsNullOrEmpty(currentHostDesktop.RemoteId))
                                    {
                                        reqStream.Headers["X-Source-Id"] = currentHostDesktop.RemoteId;
                                    }
                                    using (HttpWebResponse respStream = (HttpWebResponse)reqStream.GetResponse())
                                    {
                                        if (respStream.StatusCode == HttpStatusCode.OK && respStream.ContentType != null && respStream.ContentType.Contains("octet-stream"))
                                        {
                                            cachedWinningRelayUrl = rUrl;
                                            using (Stream ns = respStream.GetResponseStream())
                                            {
                                                byte[] headerBuf = new byte[8];
                                                while (isFrameReceiverActive && isSessionActive)
                                                {
                                                    Stopwatch swStream = Stopwatch.StartNew();
                                                    int hRead = ReadExactBytes(ns, headerBuf, 0, 8);
                                                    if (hRead < 8)
                                                    {
                                                        if (currentConnectionState == SessionConnectionState.Connected && this.IsHandleCreated && !this.IsDisposed)
                                                        {
                                                            this.BeginInvoke(new Action(() => {
                                                                AbortSessionConnection("Connection Closed by Remote Desk (Stream Terminated).");
                                                                ShowToast("Connection Closed", "Remote Desk disconnected or closed the application.", "warning");
                                                            }));
                                                        }
                                                        break; // Stream ended or remote host closed
                                                    }

                                                    // Keepalive heartbeat ping
                                                    if (headerBuf[0] == 'P' && headerBuf[1] == 'I' && headerBuf[2] == 'N' && headerBuf[3] == 'G')
                                                    {
                                                        lastFrameReceivedUtc = DateTime.UtcNow;
                                                        continue;
                                                    }

                                                    if (headerBuf[0] == 'J' && headerBuf[1] == 'D' && headerBuf[2] == 'X' && headerBuf[3] == 'D')
                                                    {
                                                        int fLen = (headerBuf[4] << 24) | (headerBuf[5] << 16) | (headerBuf[6] << 8) | headerBuf[7];
                                                        if (fLen <= 0 || fLen > 10 * 1024 * 1024) break;

                                                        byte[] fBytes = new byte[fLen];
                                                        int bRead = ReadExactBytes(ns, fBytes, 0, fLen);
                                                        if (bRead < fLen) break;

                                                        swStream.Stop();
                                                        ProcessAndRenderFrame(fBytes, (int)swStream.ElapsedMilliseconds);
                                                        successFrame = true;
                                                    }
                                                    else
                                                    {
                                                        break; // Out of sync, reconnect
                                                    }
                                                }
                                                if (successFrame) break;
                                            }
                                        }
                                    }
                                }
                                catch (WebException wex)
                                {
                                    HttpWebResponse fResp = wex.Response as HttpWebResponse;
                                    if (fResp != null && fResp.StatusCode == HttpStatusCode.Forbidden)
                                    {
                                        if (currentConnectionState == SessionConnectionState.Connecting)
                                        {
                                            connectionStageMessage = "Waiting for Remote Desk (" + activeReceiverTargetId + ") to Accept Connection...";
                                            if (lblRemoteStatus != null && this.IsHandleCreated && !this.IsDisposed)
                                            {
                                                this.BeginInvoke(new Action(() => {
                                                    if (lblRemoteStatus != null)
                                                    {
                                                        lblRemoteStatus.Text = "● Awaiting Acceptance...";
                                                        lblRemoteStatus.ForeColor = Color.FromArgb(245, 158, 11);
                                                    }
                                                }));
                                            }
                                        }
                                    }
                                }
                                catch { }

                                if (successFrame) break;

                                Stopwatch sw = Stopwatch.StartNew();
                                byte[] imgBytes = null;

                                // 1. Try High-Speed RAW Binary Stream Pull (0 Base64, 0 JSON overhead - 60 FPS)
                                try {
                                    HttpWebRequest reqBin = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/pull_bin/" + activeReceiverTargetId);
                                    reqBin.Method = "GET";
                                    reqBin.Timeout = 1200;
                                    reqBin.Proxy = null;
                                    reqBin.KeepAlive = true;
                                    reqBin.ServicePoint.UseNagleAlgorithm = false;
                                    if (currentHostDesktop != null && !string.IsNullOrEmpty(currentHostDesktop.RemoteId))
                                    {
                                        reqBin.Headers["X-Source-Id"] = currentHostDesktop.RemoteId;
                                    }
                                    using (HttpWebResponse respBin = (HttpWebResponse)reqBin.GetResponse())
                                    {
                                        if (respBin.StatusCode == HttpStatusCode.OK && respBin.ContentType != null && respBin.ContentType.StartsWith("image/"))
                                        {
                                            using (Stream s = respBin.GetResponseStream())
                                            using (MemoryStream ms = new MemoryStream())
                                            {
                                                s.CopyTo(ms);
                                                byte[] b = ms.ToArray();
                                                if (b != null && b.Length > 100) imgBytes = b;
                                            }
                                        }
                                    }
                                }
                                catch (WebException wex)
                                {
                                    HttpWebResponse fResp = wex.Response as HttpWebResponse;
                                    if (fResp != null && fResp.StatusCode == HttpStatusCode.Forbidden)
                                    {
                                        if (currentConnectionState == SessionConnectionState.Connecting)
                                        {
                                            connectionStageMessage = "Waiting for Remote Desk (" + activeReceiverTargetId + ") to Accept Connection...";
                                            if (lblRemoteStatus != null && this.IsHandleCreated && !this.IsDisposed)
                                            {
                                                this.BeginInvoke(new Action(() => {
                                                    if (lblRemoteStatus != null)
                                                    {
                                                        lblRemoteStatus.Text = "● Awaiting Acceptance...";
                                                        lblRemoteStatus.ForeColor = Color.FromArgb(245, 158, 11);
                                                    }
                                                }));
                                            }
                                        }
                                    }
                                }
                                catch { }

                                // 2. Fallback to JSON Base64 pull
                                if (imgBytes == null)
                                {
                                    try {
                                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rUrl + "/api/relay/pull_frame/" + activeReceiverTargetId);
                                        req.Method = "GET";
                                        req.Timeout = 1800;
                                        req.Proxy = null;
                                        req.KeepAlive = true;
                                        req.ServicePoint.UseNagleAlgorithm = false;
                                        if (currentHostDesktop != null && !string.IsNullOrEmpty(currentHostDesktop.RemoteId))
                                        {
                                            req.Headers["X-Source-Id"] = currentHostDesktop.RemoteId;
                                        }
                                        using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                                        {
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
                                                    imgBytes = Convert.FromBase64String(rawB64);
                                                }
                                            }
                                        }
                                    } catch { }
                                }

                                if (imgBytes != null && imgBytes.Length > 0)
                                {
                                    sw.Stop();
                                    cachedWinningRelayUrl = rUrl;
                                    ProcessAndRenderFrame(imgBytes, (int)sw.ElapsedMilliseconds);
                                    successFrame = true;
                                    break;
                                }
                            } catch { }
                        }
                    }
                } catch { }

                // Anti-Freeze Radar: Detect sudden disconnection or termination of remote host
                if (isSessionActive || currentConnectionState == SessionConnectionState.Connected)
                {
                    if (successFrame)
                    {
                        lastFrameReceivedUtc = DateTime.UtcNow;
                    }
                    else if ((DateTime.UtcNow - lastFrameReceivedUtc).TotalSeconds > 5.0)
                    {
                        if (this.IsHandleCreated && !this.IsDisposed)
                        {
                            this.BeginInvoke(new Action(() => {
                                AbortSessionConnection("Connection Lost: Remote Desk is no longer transmitting frames or closed the application.");
                                ShowToast("Connection Closed", "Remote Desk stopped transmitting frames or closed the application.", "warning");
                            }));
                        }
                        break;
                    }
                }

                Thread.Sleep(successFrame ? 4 : 200);
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
            trayMenu.MenuItems.Add("📱 Open Mini Desk", (s, e) => {
                if (miniDeskForm != null) miniDeskForm.RestoreFromTray();
                else ShowMiniDesk();
            });
            trayMenu.MenuItems.Add("🖥️ Open Full Desk Console", (s, e) => { ShowFullConsole(); });
            trayMenu.MenuItems.Add("📋 Copy My Desk ID", (s, e) => { if (currentHostDesktop != null) Clipboard.SetText(currentHostDesktop.RemoteId); });
            trayMenu.MenuItems.Add("🛑 Disconnect Remote Session", (s, e) => {
                if (isSessionActive) DisconnectActiveSession();
                if (isHostServingRemoteClient)
                {
                    isHostServingRemoteClient = false;
                    if (miniDeskForm != null) miniDeskForm.RestoreFromTray();
                }
            });
            trayMenu.MenuItems.Add("🔄 Check for Updates...", (s, e) => { CheckForUpdates(false); });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit Application", (s, e) => {
                CleanShutdown();
            });

            trayIcon = new NotifyIcon {
                Text = "jDroid-X-Desk Remote Workstation",
                Icon = SystemIcons.Shield,
                ContextMenu = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => {
                if (miniDeskForm != null)
                {
                    miniDeskForm.RestoreFromTray();
                }
                else if (this.Visible && this.WindowState != FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                }
                else
                {
                    ShowMiniDesk();
                }
            };
        }

        public void ShowTrayNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            try
            {
                if (trayIcon != null)
                {
                    trayIcon.ShowBalloonTip(2500, title, message, icon);
                }
            }
            catch { }
        }

        private void ShowToast(string message)
        {
            ShowToast("jDroid-X-Desk", message, "info");
        }

        private void ShowToast(string title, string message, string type = "info")
        {
            ModernNotificationToast.ShowToast(title, message, type);
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
                if (isSessionActive && !string.IsNullOrEmpty(activeReceiverTargetId))
                {
                    SendRemoteDisconnectSignal(activeReceiverTargetId, "Application closed by user.");
                }
                if (currentHostDesktop != null && !string.IsNullOrEmpty(currentHostDesktop.RemoteId))
                {
                    SendRemoteDisconnectSignal(currentHostDesktop.RemoteId, "Host application closed.");
                }
                StopHostDaemon();
                StopScreenStreamer();
                StopFrameReceiver();
                StopLocalPythonRelay();
                EmbeddedRelayServer.Instance.Stop();
                LanDiscoveryEngine.Instance.Stop();
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

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED (Turn on offscreen compositing for all child controls)
                return cp;
            }
        }

        public static void SetDoubleBuffered(Control c)
        {
            if (c == null) return;
            try
            {
                PropertyInfo pi = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
                if (pi != null) pi.SetValue(c, true, null);
            }
            catch { }
            foreach (Control child in c.Controls)
            {
                SetDoubleBuffered(child);
            }
        }
    }
}
