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
    public static class EngineScriptProvisioner
    {
        public static void EnsureAllEngineScripts(string baseDir)
        {
            try
            {
                if (string.IsNullOrEmpty(baseDir)) baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string scriptsDir = Path.Combine(baseDir, "scripts");
                if (!Directory.Exists(scriptsDir)) Directory.CreateDirectory(scriptsDir);

                // 1. launch-native.bat
                string launchNativeBat = Path.Combine(scriptsDir, "launch-native.bat");
                if (!File.Exists(launchNativeBat))
                {
                    File.WriteAllText(launchNativeBat, "@echo off\r\ncd /d \"%~dp0..\"\r\nstart \"\" \"jDroid-X-Desk.exe\"\r\n");
                }

                // 2. start-google-colab.bat
                string startColabBat = Path.Combine(scriptsDir, "start-google-colab.bat");
                if (!File.Exists(startColabBat))
                {
                    File.WriteAllText(startColabBat, GetStartGoogleColabBatSource());
                }

                // 3. start-sunshine.bat
                string startSunshineBat = Path.Combine(scriptsDir, "start-sunshine.bat");
                if (!File.Exists(startSunshineBat))
                {
                    File.WriteAllText(startSunshineBat, GetStartSunshineBatSource());
                }

                // 4. start-rustdesk.bat
                string startRustdeskBat = Path.Combine(scriptsDir, "start-rustdesk.bat");
                if (!File.Exists(startRustdeskBat))
                {
                    File.WriteAllText(startRustdeskBat, GetStartRustdeskBatSource());
                }

                // 5. start-guacamole.bat
                string startGuacBat = Path.Combine(scriptsDir, "start-guacamole.bat");
                if (!File.Exists(startGuacBat))
                {
                    File.WriteAllText(startGuacBat, GetStartGuacamoleBatSource());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EngineScriptProvisioner: " + ex.Message);
            }
        }

        public static string GetStartGoogleColabBatSource()
        {
            return "@echo off\r\nstart https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai\r\nexit /b 0\r\n";
        }

        public static string GetStartSunshineBatSource()
        {
            return "@echo off\r\nif exist \"%ProgramFiles%\\LizardByte\\Sunshine\\sunshine.exe\" (\r\n    start \"\" \"%ProgramFiles%\\LizardByte\\Sunshine\\sunshine.exe\"\r\n) else (\r\n    start https://localhost:47990\r\n)\r\nexit /b 0\r\n";
        }

        public static string GetStartRustdeskBatSource()
        {
            return "@echo off\r\nif exist \"%ProgramFiles%\\RustDesk\\rustdesk.exe\" (\r\n    start \"\" \"%ProgramFiles%\\RustDesk\\rustdesk.exe\"\r\n) else (\r\n    start https://rustdesk.com\r\n)\r\nexit /b 0\r\n";
        }

        public static string GetStartGuacamoleBatSource()
        {
            return "@echo off\r\nstart http://localhost:8080/guacamole\r\nexit /b 0\r\n";
        }
    }

    // ==========================================================================
    // 2.5 NATIVE EMBEDDED C# RELAY SERVER (ZERO-DEPENDENCY 0.0.0.0:8088 LISTENER)
    // ==========================================================================

}
