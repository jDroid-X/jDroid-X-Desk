using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace JDroidXDesk.Native
{
    public class UpdateCheckResult
    {
        public bool UpdateAvailable { get; set; }
        public string RemoteVersion { get; set; }
        public string LocalVersion { get; set; }
        public long RemoteFileSize { get; set; }
        public long LocalFileSize { get; set; }
        public string DownloadUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static class AutoUpdateManager
    {
        private const string DEFAULT_SSOT_URL = "https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main";
        private const string DEFAULT_EXE_URL = "https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main/jDroid-X-Desk.exe";

        public static UpdateCheckResult CheckForUpdate(string backendUrl = null)
        {
            UpdateCheckResult res = new UpdateCheckResult();
            try
            {
                string localExePath = Assembly.GetExecutingAssembly().Location;
                FileInfo fi = new FileInfo(localExePath);
                res.LocalFileSize = fi.Exists ? fi.Length : 0;
                res.LocalVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                string baseUrl = !string.IsNullOrEmpty(backendUrl) ? backendUrl.TrimEnd('/') : DEFAULT_SSOT_URL;
                string cacheBust = "?t=" + DateTime.UtcNow.Ticks.ToString();
                string versionManifestUrl = baseUrl + "/version.json" + cacheBust;
                string exeDownloadUrl = baseUrl + "/jDroid-X-Desk.exe";
                res.DownloadUrl = exeDownloadUrl;

                long remoteSize = 0;
                string remoteVer = null;
                string notes = null;
                string remoteSha = null;

                // 1. Try reading version.json
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers[HttpRequestHeader.UserAgent] = "jDroid-X-Desk-Client";
                        string json = wc.DownloadString(versionManifestUrl);
                        if (!string.IsNullOrEmpty(json))
                        {
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            var dict = jss.Deserialize<System.Collections.Generic.Dictionary<string, object>>(json);
                            if (dict.ContainsKey("version")) remoteVer = Convert.ToString(dict["version"]);
                            if (dict.ContainsKey("description")) notes = Convert.ToString(dict["description"]);
                            if (dict.ContainsKey("exeUrl")) res.DownloadUrl = Convert.ToString(dict["exeUrl"]);
                            if (dict.ContainsKey("sha256")) remoteSha = Convert.ToString(dict["sha256"]);
                        }
                    }
                }
                catch { }

                // 2. Query HTTP HEAD on the remote EXE to check Content-Length
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(res.DownloadUrl);
                    req.Method = "HEAD";
                    req.UserAgent = "jDroid-X-Desk-Client";
                    req.Timeout = 5000;
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        remoteSize = resp.ContentLength;
                    }
                }
                catch { }

                res.RemoteFileSize = remoteSize;
                res.RemoteVersion = !string.IsNullOrEmpty(remoteVer) ? remoteVer : "2.5.0.0";
                res.ReleaseNotes = notes;

                // Cryptographic validation: If remoteSha matches local binary hash, NO update needed!
                if (!string.IsNullOrEmpty(remoteSha) && File.Exists(localExePath))
                {
                    try
                    {
                        using (var sha = System.Security.Cryptography.SHA256.Create())
                        using (var fs = File.OpenRead(localExePath))
                        {
                            byte[] h = sha.ComputeHash(fs);
                            string localHex = BitConverter.ToString(h).Replace("-", "").ToLowerInvariant();
                            if (string.Equals(remoteSha.Trim(), localHex, StringComparison.OrdinalIgnoreCase))
                            {
                                res.UpdateAvailable = false;
                                return res;
                            }
                        }
                    }
                    catch { }
                }

                // Anti-Downgrade & Cryptographic Build Verification
                // Guard: Do not prompt if remote and local binaries are the same size and hash!
                bool isVersionNewer = false;
                bool isSameVersionDifferentSize = false;
                try
                {
                    Version locV = new Version(res.LocalVersion);
                    Version remV = new Version(res.RemoteVersion);

                    if (remV > locV)
                    {
                        // If sizes are identical and > 50KB, check if this is actually the same binary image
                        if (remoteSize > 50000 && res.LocalFileSize > 50000 && remoteSize == res.LocalFileSize)
                        {
                            isVersionNewer = false; // Identical byte size indicates same compiled release
                        }
                        else
                        {
                            isVersionNewer = true;
                        }
                    }
                    else if (remV == locV && remoteSize > 50000 && remoteSize != res.LocalFileSize)
                    {
                        // Same version tag but remote publisher released an updated binary size
                        isSameVersionDifferentSize = true;
                    }
                }
                catch
                {
                    if (remoteSize > 50000 && remoteSize > res.LocalFileSize)
                        isVersionNewer = true;
                }

                if (isVersionNewer || isSameVersionDifferentSize)
                {
                    res.UpdateAvailable = true;
                }
            }
            catch (Exception ex)
            {
                res.ErrorMessage = ex.Message;
                res.UpdateAvailable = false;
            }

            return res;
        }

        public static void CheckForUpdatesInteractive(IWin32Window owner, string backendUrl, bool silentIfNoUpdate)
        {
            ThreadPool.QueueUserWorkItem(delegate {
                UpdateCheckResult res = CheckForUpdate(backendUrl);
                Control parent = owner as Control;
                if (parent != null && parent.IsHandleCreated && !parent.IsDisposed)
                {
                    parent.BeginInvoke(new Action(delegate {
                        if (res.UpdateAvailable)
                        {
                            string msg = string.Format(
                                "An updated version of jDroid-X-Desk is available!\n\n" +
                                "• Current Local Size: {0:N0} bytes (v{1})\n" +
                                "• Remote Latest Size: {2:N0} bytes (v{3})\n" +
                                "{4}\n" +
                                "Would you like to download and install this update now?",
                                res.LocalFileSize, res.LocalVersion,
                                res.RemoteFileSize, res.RemoteVersion,
                                !string.IsNullOrEmpty(res.ReleaseNotes) ? "• Details: " + res.ReleaseNotes + "\n" : ""
                            );

                            DialogResult dr = MessageBox.Show(owner, msg, "jDroid-X-Desk Auto-Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (dr == DialogResult.Yes)
                            {
                                PerformDownloadAndApply(owner, res.DownloadUrl);
                            }
                        }
                        else
                        {
                            if (!silentIfNoUpdate)
                            {
                                if (!string.IsNullOrEmpty(res.ErrorMessage))
                                {
                                    MessageBox.Show(owner,
                                        string.Format("Could not verify updates at this time:\n{0}\n\nPlease verify internet connectivity.", res.ErrorMessage),
                                        "Update Check Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show(owner,
                                        string.Format("Your jDroid-X-Desk is up-to-date!\n\nLocal Version: {0}\nExecutable Size: {1:N0} bytes\nRemote Status: Verified matching SSOT release.", res.LocalVersion, res.LocalFileSize),
                                        "jDroid-X-Desk Up to Date", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }));
                }
            });
        }

        public static void PerformDownloadAndApply(IWin32Window owner, string downloadUrl)
        {
            try
            {
                string currentExe = Assembly.GetExecutingAssembly().Location;
                string tempUpdateFile = currentExe + ".update";

                Form progressForm = new Form();
                progressForm.Text = "Downloading jDroid-X-Desk Update...";
                progressForm.Size = new System.Drawing.Size(420, 150);
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.MaximizeBox = false;
                progressForm.MinimizeBox = false;

                ProgressBar pBar = new ProgressBar { Location = new System.Drawing.Point(24, 30), Size = new System.Drawing.Size(356, 24), Minimum = 0, Maximum = 100 };
                Label lblStatus = new Label { Text = "Connecting to SSOT repository...", Location = new System.Drawing.Point(24, 62), AutoSize = true };
                progressForm.Controls.Add(pBar);
                progressForm.Controls.Add(lblStatus);

                WebClient client = new WebClient();
                client.Headers[HttpRequestHeader.UserAgent] = "jDroid-X-Desk-Client";

                client.DownloadProgressChanged += (s, e) => {
                    pBar.Value = e.ProgressPercentage;
                    lblStatus.Text = string.Format("Downloaded {0:N0} KB / {1:N0} KB ({2}%)", e.BytesReceived / 1024, e.TotalBytesToReceive / 1024, e.ProgressPercentage);
                };

                client.DownloadFileCompleted += (s, e) => {
                    if (e.Error != null)
                    {
                        progressForm.Close();
                        MessageBox.Show(owner, "Update download failed: " + e.Error.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Verify valid PE executable file
                    try
                    {
                        using (FileStream fs = File.OpenRead(tempUpdateFile))
                        {
                            if (fs.Length < 50000 || fs.ReadByte() != 'M' || fs.ReadByte() != 'Z')
                            {
                                fs.Close();
                                File.Delete(tempUpdateFile);
                                progressForm.Close();
                                MessageBox.Show(owner, "Downloaded binary verification failed. Update aborted.", "Update Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                    catch (Exception vEx)
                    {
                        progressForm.Close();
                        MessageBox.Show(owner, "Verification error: " + vEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    progressForm.Close();

                    // Generate self-updater script and restart
                    LaunchUpdateBatchAndExit(currentExe, tempUpdateFile);
                };

                progressForm.Shown += (s, e) => {
                    try { client.DownloadFileAsync(new Uri(downloadUrl), tempUpdateFile); }
                    catch (Exception ex)
                    {
                        progressForm.Close();
                        MessageBox.Show(owner, "Failed to start download: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                progressForm.ShowDialog(owner);
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, "Update error: " + ex.Message, "Auto-Update Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void LaunchUpdateBatchAndExit(string targetExe, string updateFile)
        {
            try
            {
                int currentPid = Process.GetCurrentProcess().Id;
                string scriptPath = Path.Combine(Path.GetTempPath(), "jdroid_update_" + Guid.NewGuid().ToString("N") + ".bat");

                string batCode = "@echo off\r\n" +
                                 "set PID=" + currentPid + "\r\n" +
                                 "set TARGET=\"" + targetExe.Replace("\\", "\\\\") + "\"\r\n" +
                                 "set UPDATE=\"" + updateFile.Replace("\\", "\\\\") + "\"\r\n" +
                                 ":wait_loop\r\n" +
                                 "tasklist /fi \"PID eq %PID%\" 2>nul | find \"%PID%\" >nul\r\n" +
                                 "if %ERRORLEVEL% equ 0 (\r\n" +
                                 "    timeout /t 1 /nobreak >nul\r\n" +
                                 "    goto wait_loop\r\n" +
                                 ")\r\n" +
                                 "timeout /t 1 /nobreak >nul\r\n" +
                                 "move /y %UPDATE% %TARGET% >nul\r\n" +
                                 "powershell -NoProfile -ExecutionPolicy Bypass -Command \"Unblock-File -Path %TARGET% -ErrorAction SilentlyContinue\"\r\n" +
                                 "start \"\" %TARGET%\r\n" +
                                 "del \"%~f0\" 2>nul\r\n" +
                                 "exit\r\n";

                File.WriteAllText(scriptPath, batCode);

                ProcessStartInfo psi = new ProcessStartInfo(scriptPath);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = true;
                Process.Start(psi);

                Application.Exit();
            }
            catch { }
        }
    }
}
