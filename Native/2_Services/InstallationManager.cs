using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
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

        public static bool InstallToStandardDirectory(out string error)
        {
            error = null;
            try
            {
                string targetDir = GetStandardInstallDirectory();
                string sourceDir = AppDomain.CurrentDomain.BaseDirectory;

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string targetAssets = Path.Combine(targetDir, "assets");
                if (!Directory.Exists(targetAssets))
                {
                    Directory.CreateDirectory(targetAssets);
                }

                // Copy Essential Core Executables & Configuration
                string[] filesToCopy = new string[] {
                    "jDroid-X-Desk.exe",
                    "jDroid-X-Desk.exe.config",
                    "app.manifest",
                    "jDroid-X-Enterprise.cer",
                    "version.json",
                    "colab_relay_server.py"
                };

                foreach (string f in filesToCopy)
                {
                    string src = Path.Combine(sourceDir, f);
                    string dst = Path.Combine(targetDir, f);
                    if (File.Exists(src))
                    {
                        File.Copy(src, dst, true);
                    }
                }

                // Copy database.json if not present in target to preserve user settings
                string srcDb = Path.Combine(sourceDir, "database.json");
                string dstDb = Path.Combine(targetDir, "database.json");
                if (File.Exists(srcDb) && !File.Exists(dstDb))
                {
                    File.Copy(srcDb, dstDb, false);
                }

                // Copy assets
                string srcAssets = Path.Combine(sourceDir, "assets");
                if (Directory.Exists(srcAssets))
                {
                    foreach (string file in Directory.GetFiles(srcAssets))
                    {
                        string fileName = Path.GetFileName(file);
                        File.Copy(file, Path.Combine(targetAssets, fileName), true);
                    }
                }

                // Create Desktop Shortcut
                string targetExe = Path.Combine(targetDir, "jDroid-X-Desk.exe");
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
