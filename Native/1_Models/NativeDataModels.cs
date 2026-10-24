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
    // ==========================================================================
    // 1. DATA MODELS & DATABASE CONTEXT (OOPS MVC - MODEL LAYER)
    // ==========================================================================
    
    public class SimulatorDesktop
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string RemoteId { get; set; }
        public string Passcode { get; set; }
        public string Ip { get; set; }
        public string Role { get; set; }
        public string OsVersion { get; set; }
        public Color AccentColor { get; set; }
        public string[] ActiveApps { get; set; }
    }

    public static class ThemePalette
    {
        public static bool IsDarkTheme = false; // Light-First Discipline Default
        public static string CurrentThemeName = "light-theme";

        public static Color ColorBgWorkspace = Color.FromArgb(248, 250, 252);
        public static Color ColorBgCard = Color.FromArgb(255, 255, 255);
        public static Color ColorBgCardHeader = Color.FromArgb(241, 245, 249);
        public static Color ColorBgInput = Color.FromArgb(241, 245, 249);
        public static Color ColorTextPrimary = Color.FromArgb(15, 23, 42);
        public static Color ColorTextSecondary = Color.FromArgb(71, 85, 105);
        public static Color ColorBorderCard = Color.FromArgb(226, 232, 240);
        public static Color ColorAccentSky = Color.FromArgb(2, 132, 199);
        public static Color ColorAccentEmerald = Color.FromArgb(5, 150, 105);
        public static Color ColorAccentAmber = Color.FromArgb(217, 119, 6);
        public static Color ColorAccentRose = Color.FromArgb(220, 38, 38);

        // Semantic Role Aliases
        public static Color ColorPrimary { get { return ColorAccentSky; } }
        public static Color ColorSuccess { get { return ColorAccentEmerald; } }
        public static Color ColorWarning { get { return ColorAccentAmber; } }
        public static Color ColorDanger { get { return ColorAccentRose; } }

        // Dynamic 3D / Floating & Architectural Tokens
        public static Color ColorBgSidebar = Color.FromArgb(241, 245, 249);
        public static Color ColorBgSidebarHeader = Color.FromArgb(226, 232, 240);
        public static Color ColorBgTabBar = Color.FromArgb(241, 245, 249);
        public static Color ColorBgTabActive = Color.FromArgb(255, 255, 255);
        public static Color ColorBgTabInactive = Color.FromArgb(241, 245, 249);
        public static Color ColorTextTabActive = Color.FromArgb(15, 23, 42);
        public static Color TextTabInactive = Color.FromArgb(100, 116, 139);
        public static Color ColorTextTabInactive = Color.FromArgb(100, 116, 139);
        public static Color ColorBgCardElevated = Color.FromArgb(255, 255, 255);
        public static Color ColorShadow = Color.FromArgb(25, 15, 23, 42);

        public static void SetTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName)) themeName = "light-theme";
            if (themeName == "light") themeName = "light-theme";
            if (themeName == "dark") themeName = "dark-theme";
            CurrentThemeName = themeName;

            switch (themeName.ToLower())
            {
                case "dark-theme":
                case "theme-dark":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(11, 15, 25);
                    ColorBgCard = Color.FromArgb(21, 29, 48);
                    ColorBgCardHeader = Color.FromArgb(17, 24, 39);
                    ColorBgInput = Color.FromArgb(19, 28, 46);
                    ColorTextPrimary = Color.FromArgb(248, 250, 252);
                    ColorTextSecondary = Color.FromArgb(148, 163, 184);
                    ColorBorderCard = Color.FromArgb(40, 53, 76);
                    ColorAccentSky = Color.FromArgb(0, 242, 254);
                    ColorAccentEmerald = Color.FromArgb(0, 230, 118);
                    ColorAccentAmber = Color.FromArgb(255, 171, 0);
                    ColorAccentRose = Color.FromArgb(255, 51, 102);
                    break;

                case "theme-nord":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(46, 52, 64);
                    ColorBgCard = Color.FromArgb(59, 66, 82);
                    ColorBgCardHeader = Color.FromArgb(46, 52, 64);
                    ColorBgInput = Color.FromArgb(67, 76, 94);
                    ColorTextPrimary = Color.FromArgb(236, 239, 244);
                    ColorTextSecondary = Color.FromArgb(216, 222, 233);
                    ColorBorderCard = Color.FromArgb(76, 86, 106);
                    ColorAccentSky = Color.FromArgb(136, 192, 208);
                    ColorAccentEmerald = Color.FromArgb(163, 190, 140);
                    ColorAccentAmber = Color.FromArgb(235, 203, 139);
                    ColorAccentRose = Color.FromArgb(191, 97, 106);
                    break;

                case "theme-solarized-light":
                    IsDarkTheme = false;
                    ColorBgWorkspace = Color.FromArgb(253, 246, 227);
                    ColorBgCard = Color.FromArgb(238, 232, 213);
                    ColorBgCardHeader = Color.FromArgb(253, 246, 227);
                    ColorBgInput = Color.FromArgb(253, 246, 227);
                    ColorTextPrimary = Color.FromArgb(101, 123, 131);
                    ColorTextSecondary = Color.FromArgb(147, 161, 161);
                    ColorBorderCard = Color.FromArgb(211, 204, 185);
                    ColorAccentSky = Color.FromArgb(38, 139, 210);
                    ColorAccentEmerald = Color.FromArgb(133, 153, 0);
                    ColorAccentAmber = Color.FromArgb(181, 137, 0);
                    ColorAccentRose = Color.FromArgb(220, 50, 47);
                    break;

                case "theme-solarized-dark":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(0, 43, 54);
                    ColorBgCard = Color.FromArgb(7, 54, 66);
                    ColorBgCardHeader = Color.FromArgb(0, 43, 54);
                    ColorBgInput = Color.FromArgb(0, 43, 54);
                    ColorTextPrimary = Color.FromArgb(131, 148, 150);
                    ColorTextSecondary = Color.FromArgb(88, 110, 117);
                    ColorBorderCard = Color.FromArgb(20, 75, 90);
                    ColorAccentSky = Color.FromArgb(38, 139, 210);
                    ColorAccentEmerald = Color.FromArgb(133, 153, 0);
                    ColorAccentAmber = Color.FromArgb(181, 137, 0);
                    ColorAccentRose = Color.FromArgb(220, 50, 47);
                    break;

                case "theme-monokai":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(39, 40, 34);
                    ColorBgCard = Color.FromArgb(62, 61, 50);
                    ColorBgCardHeader = Color.FromArgb(39, 40, 34);
                    ColorBgInput = Color.FromArgb(46, 47, 41);
                    ColorTextPrimary = Color.FromArgb(248, 248, 242);
                    ColorTextSecondary = Color.FromArgb(117, 113, 94);
                    ColorBorderCard = Color.FromArgb(73, 72, 62);
                    ColorAccentSky = Color.FromArgb(102, 217, 239);
                    ColorAccentEmerald = Color.FromArgb(166, 226, 46);
                    ColorAccentAmber = Color.FromArgb(253, 151, 31);
                    ColorAccentRose = Color.FromArgb(249, 38, 114);
                    break;

                case "theme-dracula":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(40, 42, 54);
                    ColorBgCard = Color.FromArgb(68, 71, 90);
                    ColorBgCardHeader = Color.FromArgb(40, 42, 54);
                    ColorBgInput = Color.FromArgb(56, 58, 74);
                    ColorTextPrimary = Color.FromArgb(248, 248, 242);
                    ColorTextSecondary = Color.FromArgb(98, 114, 164);
                    ColorBorderCard = Color.FromArgb(98, 114, 164);
                    ColorAccentSky = Color.FromArgb(139, 233, 253);
                    ColorAccentEmerald = Color.FromArgb(80, 250, 123);
                    ColorAccentAmber = Color.FromArgb(241, 250, 140);
                    ColorAccentRose = Color.FromArgb(255, 85, 85);
                    break;

                case "theme-tokyo-night":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(26, 27, 38);
                    ColorBgCard = Color.FromArgb(36, 40, 59);
                    ColorBgCardHeader = Color.FromArgb(26, 27, 38);
                    ColorBgInput = Color.FromArgb(31, 35, 53);
                    ColorTextPrimary = Color.FromArgb(192, 202, 245);
                    ColorTextSecondary = Color.FromArgb(86, 95, 137);
                    ColorBorderCard = Color.FromArgb(65, 72, 104);
                    ColorAccentSky = Color.FromArgb(122, 162, 247);
                    ColorAccentEmerald = Color.FromArgb(158, 206, 106);
                    ColorAccentAmber = Color.FromArgb(224, 175, 104);
                    ColorAccentRose = Color.FromArgb(247, 118, 142);
                    break;

                case "theme-cyberpunk":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(13, 2, 33);
                    ColorBgCard = Color.FromArgb(26, 11, 46);
                    ColorBgCardHeader = Color.FromArgb(13, 2, 33);
                    ColorBgInput = Color.FromArgb(19, 6, 38);
                    ColorTextPrimary = Color.FromArgb(254, 238, 0);
                    ColorTextSecondary = Color.FromArgb(0, 240, 255);
                    ColorBorderCard = Color.FromArgb(255, 0, 85);
                    ColorAccentSky = Color.FromArgb(0, 240, 255);
                    ColorAccentEmerald = Color.FromArgb(0, 255, 159);
                    ColorAccentAmber = Color.FromArgb(254, 238, 0);
                    ColorAccentRose = Color.FromArgb(255, 0, 85);
                    break;

                case "theme-emerald":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(6, 20, 13);
                    ColorBgCard = Color.FromArgb(10, 31, 20);
                    ColorBgCardHeader = Color.FromArgb(6, 20, 13);
                    ColorBgInput = Color.FromArgb(8, 26, 17);
                    ColorTextPrimary = Color.FromArgb(209, 250, 229);
                    ColorTextSecondary = Color.FromArgb(52, 211, 153);
                    ColorBorderCard = Color.FromArgb(16, 185, 129);
                    ColorAccentSky = Color.FromArgb(52, 211, 153);
                    ColorAccentEmerald = Color.FromArgb(16, 185, 129);
                    ColorAccentAmber = Color.FromArgb(245, 158, 11);
                    ColorAccentRose = Color.FromArgb(239, 68, 68);
                    break;

                case "theme-ocean":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(8, 18, 38);
                    ColorBgCard = Color.FromArgb(15, 30, 58);
                    ColorBgCardHeader = Color.FromArgb(8, 18, 38);
                    ColorBgInput = Color.FromArgb(11, 23, 48);
                    ColorTextPrimary = Color.FromArgb(224, 242, 254);
                    ColorTextSecondary = Color.FromArgb(56, 189, 248);
                    ColorBorderCard = Color.FromArgb(14, 116, 144);
                    ColorAccentSky = Color.FromArgb(14, 165, 233);
                    ColorAccentEmerald = Color.FromArgb(16, 185, 129);
                    ColorAccentAmber = Color.FromArgb(245, 158, 11);
                    ColorAccentRose = Color.FromArgb(239, 68, 68);
                    break;

                case "theme-sunset":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(26, 12, 20);
                    ColorBgCard = Color.FromArgb(43, 19, 31);
                    ColorBgCardHeader = Color.FromArgb(26, 12, 20);
                    ColorBgInput = Color.FromArgb(34, 15, 25);
                    ColorTextPrimary = Color.FromArgb(255, 228, 230);
                    ColorTextSecondary = Color.FromArgb(251, 113, 133);
                    ColorBorderCard = Color.FromArgb(225, 29, 72);
                    ColorAccentSky = Color.FromArgb(244, 63, 94);
                    ColorAccentEmerald = Color.FromArgb(16, 185, 129);
                    ColorAccentAmber = Color.FromArgb(245, 158, 11);
                    ColorAccentRose = Color.FromArgb(225, 29, 72);
                    break;

                case "theme-gruvbox":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(40, 40, 40);
                    ColorBgCard = Color.FromArgb(60, 56, 54);
                    ColorBgCardHeader = Color.FromArgb(40, 40, 40);
                    ColorBgInput = Color.FromArgb(50, 48, 47);
                    ColorTextPrimary = Color.FromArgb(235, 219, 178);
                    ColorTextSecondary = Color.FromArgb(168, 153, 132);
                    ColorBorderCard = Color.FromArgb(102, 92, 84);
                    ColorAccentSky = Color.FromArgb(131, 165, 152);
                    ColorAccentEmerald = Color.FromArgb(184, 187, 38);
                    ColorAccentAmber = Color.FromArgb(250, 189, 47);
                    ColorAccentRose = Color.FromArgb(204, 36, 29);
                    break;

                case "theme-slate":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(15, 23, 42);
                    ColorBgCard = Color.FromArgb(30, 41, 59);
                    ColorBgCardHeader = Color.FromArgb(15, 23, 42);
                    ColorBgInput = Color.FromArgb(23, 33, 53);
                    ColorTextPrimary = Color.FromArgb(248, 250, 252);
                    ColorTextSecondary = Color.FromArgb(148, 163, 184);
                    ColorBorderCard = Color.FromArgb(51, 65, 85);
                    ColorAccentSky = Color.FromArgb(148, 163, 184);
                    ColorAccentEmerald = Color.FromArgb(16, 185, 129);
                    ColorAccentAmber = Color.FromArgb(245, 158, 11);
                    ColorAccentRose = Color.FromArgb(239, 68, 68);
                    break;

                case "theme-high-contrast":
                    IsDarkTheme = true;
                    ColorBgWorkspace = Color.FromArgb(0, 0, 0);
                    ColorBgCard = Color.FromArgb(10, 10, 10);
                    ColorBgCardHeader = Color.FromArgb(0, 0, 0);
                    ColorBgInput = Color.FromArgb(0, 0, 0);
                    ColorTextPrimary = Color.FromArgb(255, 255, 255);
                    ColorTextSecondary = Color.FromArgb(255, 255, 0);
                    ColorBorderCard = Color.FromArgb(255, 255, 255);
                    ColorAccentSky = Color.FromArgb(0, 255, 255);
                    ColorAccentEmerald = Color.FromArgb(0, 255, 0);
                    ColorAccentAmber = Color.FromArgb(255, 255, 0);
                    ColorAccentRose = Color.FromArgb(255, 0, 0);
                    break;

                default: // Light Enterprise (Light-First Default)
                    IsDarkTheme = false;
                    ColorBgWorkspace = Color.FromArgb(248, 250, 252);
                    ColorBgCard = Color.FromArgb(255, 255, 255);
                    ColorBgCardHeader = Color.FromArgb(241, 245, 249);
                    ColorBgInput = Color.FromArgb(241, 245, 249);
                    ColorTextPrimary = Color.FromArgb(15, 23, 42);
                    ColorTextSecondary = Color.FromArgb(71, 85, 105);
                    ColorBorderCard = Color.FromArgb(226, 232, 240);
                    ColorAccentSky = Color.FromArgb(2, 132, 199);
                    ColorAccentEmerald = Color.FromArgb(5, 150, 105);
                    ColorAccentAmber = Color.FromArgb(217, 119, 6);
                    ColorAccentRose = Color.FromArgb(220, 38, 38);
                    break;
            }

            if (!IsDarkTheme)
            {
                ColorBgSidebar = Color.FromArgb(241, 245, 249);
                ColorBgSidebarHeader = Color.FromArgb(226, 232, 240);
                ColorBgTabBar = Color.FromArgb(241, 245, 249);
                ColorBgTabActive = Color.FromArgb(255, 255, 255);
                ColorBgTabInactive = Color.FromArgb(241, 245, 249);
                ColorTextTabActive = ColorTextPrimary;
                ColorTextTabInactive = ColorTextSecondary;
                ColorBgCardElevated = ColorBgCard;
                ColorShadow = Color.FromArgb(25, 15, 23, 42);
            }
            else
            {
                ColorBgSidebar = Color.FromArgb(Math.Max(0, ColorBgWorkspace.R - 2), Math.Max(0, ColorBgWorkspace.G - 2), Math.Max(0, ColorBgWorkspace.B - 2));
                ColorBgSidebarHeader = Color.FromArgb(Math.Max(0, ColorBgWorkspace.R - 5), Math.Max(0, ColorBgWorkspace.G - 5), Math.Max(0, ColorBgWorkspace.B - 5));
                ColorBgTabBar = Color.FromArgb(Math.Max(0, ColorBgWorkspace.R - 4), Math.Max(0, ColorBgWorkspace.G - 4), Math.Max(0, ColorBgWorkspace.B - 4));
                ColorBgTabActive = ColorBgCard;
                ColorBgTabInactive = ColorBgTabBar;
                ColorTextTabActive = ColorTextPrimary;
                ColorTextTabInactive = ColorTextSecondary;
                ColorBgCardElevated = Color.FromArgb(Math.Min(255, ColorBgCard.R + 6), Math.Min(255, ColorBgCard.G + 6), Math.Min(255, ColorBgCard.B + 6));
                ColorShadow = Color.FromArgb(50, 0, 0, 0);
            }
        }

        public static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (rect.Width <= d || rect.Height <= d)
            {
                path.AddRectangle(rect);
                return path;
            }
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void DrawFloatingCard(Graphics g, Rectangle bounds, Color bgColor, Color borderColor, Color shadowColor, int radius)
        {
            if (bounds.Width <= 2 || bounds.Height <= 2) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (shadowColor != Color.Transparent && shadowColor.A > 0)
            {
                Rectangle sRect2 = new Rectangle(bounds.X, bounds.Y + 3, bounds.Width, bounds.Height);
                using (GraphicsPath sp2 = CreateRoundedPath(sRect2, radius))
                using (SolidBrush sb2 = new SolidBrush(Color.FromArgb(Math.Max(4, shadowColor.A / 3), shadowColor.R, shadowColor.G, shadowColor.B)))
                {
                    g.FillPath(sb2, sp2);
                }
                Rectangle sRect1 = new Rectangle(bounds.X, bounds.Y + 1, bounds.Width, bounds.Height);
                using (GraphicsPath sp1 = CreateRoundedPath(sRect1, radius))
                using (SolidBrush sb1 = new SolidBrush(shadowColor))
                {
                    g.FillPath(sb1, sp1);
                }
            }

            Rectangle cRect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            using (GraphicsPath cp = CreateRoundedPath(cRect, radius))
            {
                using (SolidBrush bb = new SolidBrush(bgColor))
                {
                    g.FillPath(bb, cp);
                }
                if (borderColor != Color.Transparent)
                {
                    using (Pen bp = new Pen(borderColor, 1.2f))
                    {
                        g.DrawPath(bp, cp);
                    }
                }
            }
        }

        public static string GetThemeDisplayName()
        {
            switch (CurrentThemeName.ToLower())
            {
                case "dark":
                case "dark-theme":
                case "theme-dark": return "Dark Studio";
                case "theme-nord": return "Nord Arctic";
                case "theme-solarized-light": return "Solarized Light";
                case "theme-solarized-dark": return "Solarized Dark";
                case "theme-monokai": return "Monokai Pro";
                case "theme-dracula": return "Dracula Midnight";
                case "theme-tokyo-night": return "Tokyo Night";
                case "theme-cyberpunk": return "Cyberpunk Neon";
                case "theme-emerald": return "Matrix Emerald";
                case "theme-ocean": return "Deep Ocean";
                case "theme-sunset": return "Crimson Sunset";
                case "theme-gruvbox": return "Gruvbox Retro";
                case "theme-slate": return "Titanium Slate";
                case "theme-high-contrast": return "High Contrast";
                default: return "Light Enterprise";
            }
        }
    }

    public class AppSettings
    {
        public string theme { get; set; }
        public string language { get; set; }
        public GeneralSettings general { get; set; }
        public ConnectionSettings connection { get; set; }
        public WakeOnLanSettings wakeOnLan { get; set; }
        public AccessSettings access { get; set; }
        public RecordingSettings recording { get; set; }
        public DisplaySettings display { get; set; }
        public AudioSettings audio { get; set; }
        public AccountSettings account { get; set; }

        public AppSettings()
        {
            theme = "dark";
            language = "autoselect";
            general = new GeneralSettings();
            connection = new ConnectionSettings();
            wakeOnLan = new WakeOnLanSettings();
            access = new AccessSettings();
            recording = new RecordingSettings();
            display = new DisplaySettings();
            audio = new AudioSettings();
            account = new AccountSettings();
        }
    }

    public class GeneralSettings
    {
        public bool hideLocalTaskbar { get; set; }
        public bool requestComment { get; set; }
        public bool openAddressBookOnStartup { get; set; }
        public bool autoUpdate { get; set; }
        public string updateChannel { get; set; }
        public string startupMode { get; set; }
        public GeneralSettings()
        {
            hideLocalTaskbar = false;
            requestComment = false;
            openAddressBookOnStartup = true;
            autoUpdate = true;
            updateChannel = "stable";
            startupMode = "ask";
        }
    }

    public class ConnectionSettings
    {
        public int localPort { get; set; }
        public int relayPort { get; set; }
        public int discoveryPort { get; set; }
        public bool lanDiscovery { get; set; }
        public string tunnelToken { get; set; }
        public bool directConnections { get; set; }
        public bool colabCloudRelay { get; set; }
        public string proxyServer { get; set; }
        public string idServer { get; set; }
        public string relayServer { get; set; }
        public string customDomain { get; set; }
        public string webRepoUrl { get; set; }
        public string colabNotebookUrl { get; set; }
        public string githubBackendUrl { get; set; }
        public string localInstalledFolder { get; set; }
        public string localRelayScriptPath { get; set; }
        public ConnectionSettings()
        {
            localPort = 7070;
            relayPort = 8088;
            discoveryPort = 8089;
            lanDiscovery = true;
            tunnelToken = "";
            directConnections = true;
            colabCloudRelay = true;
            proxyServer = "";
            idServer = "relay.jdroid-x.net:21116";
            relayServer = "https://jdroidx.freemodels.club";
            customDomain = "https://desk.jdroid-x.com";
            webRepoUrl = "https://github.com/jDroid-X/jDroid-X-Web";
            colabNotebookUrl = "https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai";
            githubBackendUrl = "https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main";
            localInstalledFolder = AppDomain.CurrentDomain.BaseDirectory;
            localRelayScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colab_relay_server.py");
        }
    }

    public class WakeOnLanSettings
    {
        public bool enabled { get; set; }
        public List<string> authorizedClients { get; set; }
        public WakeOnLanSettings()
        {
            enabled = true;
            authorizedClients = new List<string>();
        }
    }

    public class AccessSettings
    {
        public bool unattendedAccess { get; set; }
        public bool requirePasscode { get; set; }
        public string passwordHash { get; set; }
        public bool enable2FA { get; set; }
        public int accessTimeout { get; set; }
        public AccessSettings()
        {
            unattendedAccess = true;
            requirePasscode = false;
            passwordHash = "argon2id$v=19$m=65536,t=3,p=4$jdroid$hash";
            enable2FA = false;
            accessTimeout = 30;
        }
    }

    public class RecordingSettings
    {
        public bool recordIncoming { get; set; }
        public bool recordOutgoing { get; set; }
        public string recordingPath { get; set; }
        public RecordingSettings()
        {
            recordIncoming = true;
            recordOutgoing = false;
            recordingPath = "C:\\jDroid-X\\Recordings";
        }
    }

    public class DisplaySettings
    {
        public string quality { get; set; }
        public string viewMode { get; set; }
        public bool hardwareAcceleration { get; set; }
        public DisplaySettings()
        {
            quality = "balanced";
            viewMode = "smartfit";
            hardwareAcceleration = true;
        }
    }

    public class AudioSettings
    {
        public bool transmitAudio { get; set; }
        public bool outputAudio { get; set; }
        public AudioSettings()
        {
            transmitAudio = true;
            outputAudio = true;
        }
    }

    public class AccountSettings
    {
        public string username { get; set; }
        public string licenseType { get; set; }
        public string enterpriseTenant { get; set; }
        public AccountSettings()
        {
            username = "JEET";
            licenseType = "Enterprise Platinum (Unlimited) - jDroid-X Studio";
            enterpriseTenant = "jDroid-X Studio Enterprise Network";
        }
    }

    public class DeviceItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string ip { get; set; }
        public string groupId { get; set; }
        public string group { get; set; }
        public string status { get; set; }
        public string accessLevel { get; set; }
    }

    public class GroupItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
    }

    public class SessionItem
    {
        public string id { get; set; }
        public string targetId { get; set; }
        public string targetName { get; set; }
        public string type { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public int durationSeconds { get; set; }
        public string status { get; set; }
        public string sourceIp { get; set; }
    }

    public class GuardianItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string contact { get; set; }
        public string status { get; set; }
    }

    public class SafeZoneItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string ip { get; set; }
    }

    public class ContactItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string remoteId { get; set; }
        public string group { get; set; }
    }

    public class NotificationItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string type { get; set; }
        public string timestamp { get; set; }
    }

    public class LogItem
    {
        public string timestamp { get; set; }
        public string type { get; set; }
        public string action { get; set; }
        public string details { get; set; }
    }

    public class ProcessingLimitsModel
    {
        public int maxConcurrentSessions { get; set; }
        public int bandwidthLimitMbps { get; set; }
        public int maxFps { get; set; }
        public int idleTimeoutMinutes { get; set; }
        public int logRetentionDays { get; set; }
        public string hardwareAcceleration { get; set; }

        public ProcessingLimitsModel()
        {
            maxConcurrentSessions = 10;
            bandwidthLimitMbps = 50;
            maxFps = 60;
            idleTimeoutMinutes = 15;
            logRetentionDays = 7;
            hardwareAcceleration = "nvenc_cuda";
        }
    }

    public class OpenSourceEngineModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string version { get; set; }
        public bool enabled { get; set; }
        public string status { get; set; }
        public string ports { get; set; }
        public string idServer { get; set; }
        public string relayServer { get; set; }
        public string apiServer { get; set; }
        public string publicKey { get; set; }
        public int directPort { get; set; }
        public string webUiUrl { get; set; }
        public int streamPort { get; set; }
        public int controlPort { get; set; }
        public string encoder { get; set; }
        public int targetFps { get; set; }
        public int bitrateMbps { get; set; }
        public string guacdHost { get; set; }
        public int guacdPort { get; set; }
        public int clientPort { get; set; }
        public int rdpTargetPort { get; set; }
        public string rdpSecurity { get; set; }
        public int relayPort { get; set; }
        public int localPort { get; set; }
        public bool binaryStream { get; set; }
        public bool cudaActive { get; set; }
        public string manualConfigRequired { get; set; }

        public OpenSourceEngineModel()
        {
            enabled = true;
            status = "Ready";
        }
    }

    public class ServerConfigModel
    {
        public string activeEngine { get; set; }
        public ProcessingLimitsModel processingLimits { get; set; }
        public Dictionary<string, OpenSourceEngineModel> engines { get; set; }

        public ServerConfigModel()
        {
            activeEngine = "jdroidRelay";
            processingLimits = new ProcessingLimitsModel();
            engines = new Dictionary<string, OpenSourceEngineModel>();
        }

        public static ServerConfigModel CreateDefault()
        {
            ServerConfigModel cfg = new ServerConfigModel();
            cfg.activeEngine = "jdroidRelay";
            cfg.processingLimits = new ProcessingLimitsModel();
            cfg.engines = new Dictionary<string, OpenSourceEngineModel>();

            cfg.engines["rustdesk"] = new OpenSourceEngineModel
            {
                id = "rustdesk",
                name = "RustDesk (rustdesk/rustdesk)",
                type = "Direct Match P2P Engine",
                version = "v1.3.x",
                enabled = true,
                status = "Ready",
                ports = "21115 (NAT), 21116 (hbbs), 21117 (hbbr), 21118 (LAN Direct)",
                idServer = "relay.jdroid-x.net:21116",
                relayServer = "relay.jdroid-x.net:21117",
                apiServer = "http://127.0.0.1:21114",
                publicKey = "",
                directPort = 21118,
                manualConfigRequired = "Default public servers connect with zero configuration. For unlimited free bandwidth and private self-hosting, configure ID Server & Relay Server (hbbs/hbbr) with public key."
            };

            cfg.engines["sunshine"] = new OpenSourceEngineModel
            {
                id = "sunshine",
                name = "Sunshine & Moonlight (LizardByte/Sunshine)",
                type = "Lowest Latency In The World",
                version = "v0.23.x",
                enabled = true,
                status = "Active",
                ports = "47984 (HTTPS), 47989 (HTTP), 47990 (Web UI), 48010 (RTSP)",
                webUiUrl = "https://localhost:47990",
                streamPort = 47989,
                controlPort = 47984,
                encoder = "NVIDIA NVENC / DirectX Desktop Duplication (GPU VRAM sub-2ms)",
                targetFps = 60,
                bitrateMbps = 20,
                manualConfigRequired = "One-time setup: browse https://localhost:47990, create initial admin credentials, and enter Moonlight 4-digit PIN for device pairing authorization."
            };

            cfg.engines["guacamole"] = new OpenSourceEngineModel
            {
                id = "guacamole",
                name = "Apache Guacamole + FreeRDP (apache/guacamole-server)",
                type = "Clientless HTML5 Browser Gateway",
                version = "v1.5.x",
                enabled = true,
                status = "Ready",
                ports = "8080 (Client/Tomcat), 4822 (guacd), 3389 (RDP Target)",
                guacdHost = "127.0.0.1",
                guacdPort = 4822,
                clientPort = 8080,
                rdpTargetPort = 3389,
                rdpSecurity = "nla",
                manualConfigRequired = "Runs inside Docker or Linux/WSL. Configure target Windows RDP hostname/IP, port 3389, and username/password with NLA enabled."
            };

            cfg.engines["jdroidRelay"] = new OpenSourceEngineModel
            {
                id = "jdroidRelay",
                name = "jDroid-X Async Binary Relay",
                type = "Multi-Threaded Colab / Cloud Bridge",
                version = "v3.5 High-Throughput",
                enabled = true,
                status = "Online",
                ports = "8088 (Relay), 12345 (Console Web)",
                relayPort = 8088,
                localPort = 12345,
                binaryStream = true,
                cudaActive = true,
                manualConfigRequired = "Zero manual config. Runs automatically out-of-the-box with Cloudflare tunnel."
            };

            return cfg;
        }
    }

}
