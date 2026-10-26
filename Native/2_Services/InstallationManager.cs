using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Win32;

namespace JDroidXDesk.Native
{
    public static class InstallationManager
    {
        public static string GetStandardInstallDirectory()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "Programs", "jDroid-X-Desk");
        }

        public static bool IsRunningFromStandardDirectory()
        {
            try
            {
                string currentExe = Assembly.GetExecutingAssembly().Location;
                string standardDir = GetStandardInstallDirectory();
                return currentExe.StartsWith(standardDir, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDevWorkspaceDirectory()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                return File.Exists(Path.Combine(baseDir, "compile_native.bat")) ||
                       File.Exists(Path.Combine(baseDir, "Native", "5_Forms", "MainForm.cs"));
            }
            catch
            {
                return false;
            }
        }

        public static bool ExtractEmbeddedResource(string resourceIdentifier, string targetFilePath, bool overwrite = true)
        {
            try
            {
                if (File.Exists(targetFilePath) && !overwrite) return true;

                string dir = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                Assembly asm = Assembly.GetExecutingAssembly();
                string[] allNames = asm.GetManifestResourceNames();
                string match = null;

                foreach (string n in allNames)
                {
                    if (n.Equals(resourceIdentifier, StringComparison.OrdinalIgnoreCase) ||
                        n.EndsWith(resourceIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        match = n;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(match)) return false;

                using (Stream s = asm.GetManifestResourceStream(match))
                {
                    if (s == null) return false;
                    using (FileStream fs = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
                    {
                        s.CopyTo(fs);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void EnsureEssentialFiles(string targetDir)
        {
            try
            {
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                string targetAssets = Path.Combine(targetDir, "assets");
                string targetScripts = Path.Combine(targetDir, "scripts");
                if (!Directory.Exists(targetAssets)) Directory.CreateDirectory(targetAssets);
                if (!Directory.Exists(targetScripts)) Directory.CreateDirectory(targetScripts);

                Dictionary<string, string> map = new Dictionary<string, string>
                {
                    { Path.Combine(targetDir, "database.json"), "database.json" },
                    { Path.Combine(targetDir, "version.json"), "version.json" },
                    { Path.Combine(targetDir, "colab_relay_server.py"), "colab_relay_server.py" },
                    { Path.Combine(targetDir, "jDroid-X-Enterprise.cer"), "jDroid_X_Enterprise.cer" },
                    { Path.Combine(targetDir, "jDroid-X-Desk.exe.config"), "jDroid_X_Desk.exe.config" },
                    { Path.Combine(targetDir, "app.manifest"), "app.manifest" },
                    { Path.Combine(targetAssets, "app.ico"), "assets.app.ico" },
                    { Path.Combine(targetAssets, "jdroidxlogo_trans.png"), "assets.jdroidxlogo_trans.png" },
                    { Path.Combine(targetAssets, "jdroid-x-desk-presentation.png"), "assets.jdroid_x_desk_presentation.png" },
                    { Path.Combine(targetScripts, "configure-smart-app-control.bat"), "configure_smart_app_control.bat" },
                    { Path.Combine(targetScripts, "trust-root-cert.bat"), "trust_root_cert.bat" },
                    { Path.Combine(targetDir, "TRUST_ENTERPRISE_CERT_AS_ADMIN.bat"), "TRUST_ENTERPRISE_CERT_AS_ADMIN.bat" }
                };

                foreach (KeyValuePair<string, string> kvp in map)
                {
                    if (!File.Exists(kvp.Key))
                    {
                        ExtractEmbeddedResource(kvp.Value, kvp.Key, false);
                    }
                }
            }
            catch { }
        }

        public static bool InstallToStandardDirectory(out string error)
        {
            error = null;
            try
            {
                string targetDir = GetStandardInstallDirectory();
                string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
                string currentExe = Assembly.GetExecutingAssembly().Location;

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string targetAssets = Path.Combine(targetDir, "assets");
                if (!Directory.Exists(targetAssets)) Directory.CreateDirectory(targetAssets);
                string targetScripts = Path.Combine(targetDir, "scripts");
                if (!Directory.Exists(targetScripts)) Directory.CreateDirectory(targetScripts);

                // Terminate any old running instances in target directory to prevent file locking
                try
                {
                    Process[] existing = Process.GetProcessesByName("jDroid-X-Desk");
                    int myPid = Process.GetCurrentProcess().Id;
                    foreach (Process p in existing)
                    {
                        if (p.Id != myPid)
                        {
                            try
                            {
                                string pPath = p.MainModule != null ? p.MainModule.FileName : null;
                                if (!string.IsNullOrEmpty(pPath) && pPath.StartsWith(targetDir, StringComparison.OrdinalIgnoreCase))
                                {
                                    p.Kill();
                                    p.WaitForExit(2000);
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                // 1. Copy Primary Executable
                string targetExe = Path.Combine(targetDir, "jDroid-X-Desk.exe");
                if (!string.Equals(currentExe, targetExe, StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(currentExe, targetExe, true);
                }

                // 2. Deploy all core files from source if present, else extract from embedded resources
                Dictionary<string, string> filesToDeploy = new Dictionary<string, string>
                {
                    { "database.json", "database.json" },
                    { "version.json", "version.json" },
                    { "colab_relay_server.py", "colab_relay_server.py" },
                    { "jDroid-X-Enterprise.cer", "jDroid_X_Enterprise.cer" },
                    { "jDroid-X-Desk.exe.config", "jDroid_X_Desk.exe.config" },
                    { "app.manifest", "app.manifest" },
                    { Path.Combine("assets", "app.ico"), "assets.app.ico" },
                    { Path.Combine("assets", "jdroidxlogo_trans.png"), "assets.jdroidxlogo_trans.png" },
                    { Path.Combine("assets", "jdroid-x-desk-presentation.png"), "assets.jdroid_x_desk_presentation.png" },
                    { Path.Combine("scripts", "configure-smart-app-control.bat"), "configure_smart_app_control.bat" },
                    { Path.Combine("scripts", "trust-root-cert.bat"), "trust_root_cert.bat" },
                    { "TRUST_ENTERPRISE_CERT_AS_ADMIN.bat", "TRUST_ENTERPRISE_CERT_AS_ADMIN.bat" }
                };

                foreach (KeyValuePair<string, string> item in filesToDeploy)
                {
                    string srcPath = Path.Combine(sourceDir, item.Key);
                    string dstPath = Path.Combine(targetDir, item.Key);

                    // Preserve existing database.json if already populated with user data
                    if (item.Key == "database.json" && File.Exists(dstPath))
                    {
                        continue;
                    }

                    if (File.Exists(srcPath) && !string.Equals(srcPath, dstPath, StringComparison.OrdinalIgnoreCase))
                    {
                        string pDir = Path.GetDirectoryName(dstPath);
                        if (!Directory.Exists(pDir)) Directory.CreateDirectory(pDir);
                        File.Copy(srcPath, dstPath, true);
                    }
                    else if (!File.Exists(dstPath))
                    {
                        ExtractEmbeddedResource(item.Value, dstPath, true);
                    }
                }

                // Create Desktop Shortcut
                CreateWindowsShortcut(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "jDroid-X-Desk.lnk"),
                    targetExe,
                    targetDir,
                    "jDroid-X-Desk Enterprise Remote Desktop Console"
                );

                // Create Start Menu Shortcut
                string startMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
                if (Directory.Exists(startMenuPrograms))
                {
                    CreateWindowsShortcut(
                        Path.Combine(startMenuPrograms, "jDroid-X-Desk.lnk"),
                        targetExe,
                        targetDir,
                        "jDroid-X-Desk Enterprise Remote Desktop Console"
                    );
                }

                // Register in Windows Control Panel (Add/Remove Programs)
                RegisterInWindowsUninstall(targetDir, targetExe);

                // Create clean Uninstaller script
                CreateUninstallerScript(targetDir);

                // Unblock Mark-of-the-Web (MOTW) on installed folder
                UnblockDirectory(targetDir);

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void CreateWindowsShortcut(string shortcutPath, string targetPath, string workingDir, string description)
        {
            try
            {
                string vbsScript = Path.Combine(Path.GetTempPath(), "MakeShortcut_" + Guid.NewGuid().ToString("N") + ".vbs");
                string iconLocation = Path.Combine(workingDir, "assets", "app.ico");
                bool hasIcon = File.Exists(iconLocation);

                string vbsCode = "Set oWS = WScript.CreateObject(\"WScript.Shell\")\r\n" +
                                 "sLinkFile = \"" + shortcutPath.Replace("\\", "\\\\") + "\"\r\n" +
                                 "Set oLink = oWS.CreateShortcut(sLinkFile)\r\n" +
                                 "oLink.TargetPath = \"" + targetPath.Replace("\\", "\\\\") + "\"\r\n" +
                                 "oLink.WorkingDirectory = \"" + workingDir.Replace("\\", "\\\\") + "\"\r\n" +
                                 "oLink.Description = \"" + description + "\"\r\n" +
                                 (hasIcon ? "oLink.IconLocation = \"" + iconLocation.Replace("\\", "\\\\") + ",0\"\r\n" : "") +
                                 "oLink.Save\r\n";

                File.WriteAllText(vbsScript, vbsCode);
                ProcessStartInfo psi = new ProcessStartInfo("cscript.exe", "//nologo \"" + vbsScript + "\"");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process p = Process.Start(psi);
                p.WaitForExit(3000);
                try { File.Delete(vbsScript); } catch { }
            }
            catch { }
        }

        private static void RegisterInWindowsUninstall(string installDir, string exePath)
        {
            try
            {
                string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk";
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayName", "jDroid-X-Desk Remote Workstation");
                        key.SetValue("DisplayVersion", "2.5.0");
                        key.SetValue("Publisher", "jDroid-X Studio");
                        key.SetValue("DisplayIcon", exePath + ",0");
                        key.SetValue("InstallLocation", installDir);
                        key.SetValue("UninstallString", "\"" + Path.Combine(installDir, "uninstall.bat") + "\"");
                        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        private static void CreateUninstallerScript(string installDir)
        {
            try
            {
                string uninstallerPath = Path.Combine(installDir, "uninstall.bat");
                string script = "@echo off\r\n" +
                                "title Uninstalling jDroid-X-Desk...\r\n" +
                                "taskkill /f /im jDroid-X-Desk.exe >nul 2>&1\r\n" +
                                "timeout /t 1 /nobreak >nul\r\n" +
                                "del /f /q \"%USERPROFILE%\\Desktop\\jDroid-X-Desk.lnk\" >nul 2>&1\r\n" +
                                "del /f /q \"%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\jDroid-X-Desk.lnk\" >nul 2>&1\r\n" +
                                "reg delete \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\jDroid-X-Desk\" /f >nul 2>&1\r\n" +
                                "echo jDroid-X-Desk has been removed.\r\n" +
                                "pause\r\n" +
                                "(goto) 2>nul & rmdir /s /q \"" + installDir + "\"\r\n";
                File.WriteAllText(uninstallerPath, script);
            }
            catch { }
        }

        private static void UnblockDirectory(string dir)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe",
                    "-NoProfile -ExecutionPolicy Bypass -Command \"Get-ChildItem -Path '" + dir + "' -Recurse | Unblock-File -ErrorAction SilentlyContinue\"");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process p = Process.Start(psi);
                p.WaitForExit(3000);
            }
            catch { }
        }
    }
}
