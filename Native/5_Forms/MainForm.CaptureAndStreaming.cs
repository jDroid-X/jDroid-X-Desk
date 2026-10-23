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
                using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), ThemePalette.ColorBgWorkspace, ThemePalette.ColorBgCard, LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(bg, 0, 0, cw, ch);
                }

                int cardW = Math.Min(600, cw - 40);
                int cardH = 260;
                int cardX = (cw - cardW) / 2;
                int cardY = Math.Max(20, (ch - cardH) / 2);

                Rectangle r = new Rectangle(cardX, cardY, cardW, cardH);
                ThemePalette.DrawFloatingCard(g, r, ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 10);

                using (Font fTitle = new Font("Segoe UI", 14f, FontStyle.Bold))
                using (Font fSub = new Font("Segoe UI", 9.5f))
                using (Font fCode = new Font("Consolas", 9f))
                using (SolidBrush bWhite = new SolidBrush(ThemePalette.ColorTextPrimary))
                using (SolidBrush bMuted = new SolidBrush(ThemePalette.ColorTextSecondary))
                using (SolidBrush bBlue = new SolidBrush(ThemePalette.ColorPrimary))
                using (SolidBrush bGreen = new SolidBrush(ThemePalette.ColorSuccess))
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

                // 2. Local Live Screen Loopback & Mirror Shield Mode (Zero Infinite Recursion)
                if (isLiveScreenMirror || (currentHostDesktop != null && target.RemoteId == currentHostDesktop.RemoteId))
                {
                    RenderLoopbackShieldScreen(g, target, cw, ch);
                    return;
                }

                // 3. Awaiting Live Video Stream Frames (Real Buffer Status - Zero Fake Icons)
                RenderAwaitingLiveFramesScreen(g, target, cw, ch);
            }
        }

        private void RenderConnectingHeartbeatScreen(Graphics g, SimulatorDesktop target, int cw, int ch)
        {
            // Background - Deep Futuristic Slate Gradient
            using (LinearGradientBrush wallBrush = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), ThemePalette.ColorBgWorkspace, ThemePalette.ColorBgCard, LinearGradientMode.Vertical))
            {
                g.FillRectangle(wallBrush, 0, 0, cw, ch);
            }

            // Oscilloscope Grid Lines (Subtle Cyan Matrix)
            using (Pen gridPen = new Pen(ThemePalette.ColorBorderCard, 1))
            {
                for (int x = 0; x < cw; x += 40) g.DrawLine(gridPen, x, 0, x, ch);
                for (int y = 0; y < ch; y += 40) g.DrawLine(gridPen, 0, y, cw, y);
            }

            int cardW = Math.Min(700, cw - 60);
            int cardH = 360;
            int cardX = (cw - cardW) / 2;
            int cardY = Math.Max(24, (ch - cardH) / 2);

            // Center Card Container
            Rectangle rCard = new Rectangle(cardX, cardY, cardW, cardH);
            ThemePalette.DrawFloatingCard(g, rCard, ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 10);

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
            using (LinearGradientBrush wallBrush = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), ThemePalette.ColorBgWorkspace, ThemePalette.ColorBgCard, LinearGradientMode.Vertical))
            {
                g.FillRectangle(wallBrush, 0, 0, cw, ch);
            }

            int cardW = Math.Min(640, cw - 60);
            int cardH = 220;
            int cardX = (cw - cardW) / 2;
            int cardY = Math.Max(40, (ch - cardH) / 2);

            Rectangle rCard = new Rectangle(cardX, cardY, cardW, cardH);
            ThemePalette.DrawFloatingCard(g, rCard, ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 10);

            using (Font fTitle = new Font("Segoe UI", 13f, FontStyle.Bold))
            using (Font fSub = new Font("Segoe UI", 9f))
            using (Font fCode = new Font("Consolas", 9f))
            using (SolidBrush bWhite = new SolidBrush(ThemePalette.ColorTextPrimary))
            using (SolidBrush bGreen = new SolidBrush(ThemePalette.ColorSuccess))
            using (SolidBrush bCyan = new SolidBrush(ThemePalette.ColorPrimary))
            using (SolidBrush bMuted = new SolidBrush(ThemePalette.ColorTextSecondary))
            {
                g.DrawString("🟢 P2P SESSION CONNECTED & ACTIVE", fTitle, bGreen, cardX + 24, cardY + 20);
                g.DrawString("Authenticated with Remote Workstation: " + target.RemoteId + " (" + target.Name + ")", fSub, bWhite, cardX + 24, cardY + 50);
                g.DrawString("● Tunnel: AES-256 End-to-End P2P Stream Active", fCode, bCyan, cardX + 24, cardY + 80);
                g.DrawString("Awaiting video stream frames from remote host...", fSub, bMuted, cardX + 24, cardY + 110);
                g.DrawString("Bytes Received: " + (bytesTransferred / (1024 * 1024.0)).ToString("0.0") + " MB | Total Frames: " + framesReceivedCount, fCode, bWhite, cardX + 24, cardY + 135);
                g.DrawString("💡 Tip: Connect from another PC or mobile device to view remote desktop.", fSub, bGreen, cardX + 24, cardY + 170);
            }
        }

        private void RenderLoopbackShieldScreen(Graphics g, SimulatorDesktop target, int cw, int ch)
        {
            using (LinearGradientBrush wallBrush = new LinearGradientBrush(new Rectangle(0, 0, cw, ch), ThemePalette.ColorBgWorkspace, ThemePalette.ColorBgCard, LinearGradientMode.Vertical))
            {
                g.FillRectangle(wallBrush, 0, 0, cw, ch);
            }

            int cardW = Math.Min(680, cw - 60);
            int cardH = 260;
            int cardX = (cw - cardW) / 2;
            int cardY = Math.Max(40, (ch - cardH) / 2);

            Rectangle rCard = new Rectangle(cardX, cardY, cardW, cardH);
            ThemePalette.DrawFloatingCard(g, rCard, ThemePalette.ColorBgCard, ThemePalette.ColorBorderCard, ThemePalette.ColorShadow, 12);

            using (Font fTitle = new Font("Segoe UI", 13.5f, FontStyle.Bold))
            using (Font fSub = new Font("Segoe UI", 9.5f))
            using (Font fCode = new Font("Consolas", 9.5f))
            using (SolidBrush bWhite = new SolidBrush(ThemePalette.ColorTextPrimary))
            using (SolidBrush bAmber = new SolidBrush(Color.FromArgb(245, 158, 11)))
            using (SolidBrush bCyan = new SolidBrush(ThemePalette.ColorPrimary))
            using (SolidBrush bMuted = new SolidBrush(ThemePalette.ColorTextSecondary))
            {
                g.DrawString("🛡️ ANTI-RECURSION SHIELD ACTIVE", fTitle, bAmber, cardX + 24, cardY + 22);
                g.DrawString("Self-Loopback / Same-Screen Session Detected: " + (target != null ? target.RemoteId : "Local Host"), fSub, bWhite, cardX + 24, cardY + 54);
                g.DrawString("● Infinite Screen-in-Screen Mirror Loop Suppressed for Safety.", fCode, bCyan, cardX + 24, cardY + 86);
                g.DrawString("To test live remote desktop streaming without mirror recursion:", fSub, bMuted, cardX + 24, cardY + 118);
                g.DrawString("  1. Connect from a secondary machine, laptop, or mobile browser.", fSub, bMuted, cardX + 24, cardY + 140);
                g.DrawString("  2. Or open https://desk.jdroid-x.com on another monitor/device.", fSub, bMuted, cardX + 24, cardY + 162);
                g.DrawString("Keyboard/mouse relay pipelines remain active. Click '✕ Disconnect' to return.", fCode, bAmber, cardX + 24, cardY + 196);
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

    }
}
