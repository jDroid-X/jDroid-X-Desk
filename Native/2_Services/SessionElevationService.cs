// ==============================================================================
// [Variables]            : _instance, _isElevatedCache, _uiAccessActive, _cachedSessionId
// [Constants]            : MSGFLT_ADD, WM_COMMAND, WM_COPYDATA, WM_USER, DESKTOP_*, UOI_NAME, SECURITY_IMPERSONATION
// [Dependencies]         : System, System.Diagnostics, System.Security.Principal, System.Runtime.InteropServices, System.Windows.Forms
// [Integration Points]   : MainForm.InputAndHost.cs, MainForm.cs, DxgiCaptureService.cs, EmbeddedRelayServer.cs, InstallationManager.cs
// [Hardcode to Valuebase]: Dynamic session lookup via WTSGetActiveConsoleSessionId; dynamic desktop handle resolution
// [Optimized Performance]: Cached token check; zero-overhead native P/Invoke; leak-free desktop handle teardown
// ==============================================================================

using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JDroidXDesk.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    /// <summary>
    /// Senior Dynamic OOPS Session Elevation & UAC Traversal Service.
    /// Provides UltraViewer/AnyDesk parity for administrative elevation,
    /// UIPI bypass, and Session 0 secure desktop traversal.
    /// </summary>
    public sealed class SessionElevationService
    {
        private static readonly SessionElevationService _instance = new SessionElevationService();
        public static SessionElevationService Instance { get { return _instance; } }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ChangeWindowMessageFilter(uint message, uint dwFlag);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, uint action, IntPtr pChangeFilterStruct);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetThreadDesktop(uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, [Out] byte[] pvInfo, uint nLength, out uint lpnLengthNeeded);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSQueryUserToken(uint SessionId, out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            IntPtr lpTokenAttributes,
            int ImpersonationLevel,
            int TokenType,
            out IntPtr phNewToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        private const uint MSGFLT_ADD = 1;
        private const uint WM_COMMAND = 0x0111;
        private const uint WM_COPYDATA = 0x004A;
        private const uint WM_USER = 0x0400;

        private const uint DESKTOP_READOBJECTS = 0x0001;
        private const uint DESKTOP_CREATEWINDOW = 0x0002;
        private const uint DESKTOP_CREATEMENU = 0x0004;
        private const uint DESKTOP_HOOKCONTROL = 0x0008;
        private const uint DESKTOP_JOURNALRECORD = 0x0010;
        private const uint DESKTOP_JOURNALPLAYBACK = 0x0020;
        private const uint DESKTOP_ENUMERATE = 0x0040;
        private const uint DESKTOP_WRITEOBJECTS = 0x0080;
        private const uint DESKTOP_SWITCHDESKTOP = 0x0100;
        private const int UOI_NAME = 2;

        private const uint MAXIMUM_ALLOWED = 0x02000000;
        private const int SecurityImpersonation = 2;
        private const int TokenTypePrimary = 1;

        private bool? _isElevatedCache = null;

        private SessionElevationService()
        {
        }

        /// <summary>
        /// Checks whether the current process is running with elevated Administrator privileges.
        /// </summary>
        public bool IsElevated()
        {
            if (_isElevatedCache.HasValue) return _isElevatedCache.Value;
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    _isElevatedCache = principal.IsInRole(WindowsBuiltInRole.Administrator);
                    return _isElevatedCache.Value;
                }
            }
            catch
            {
                _isElevatedCache = false;
                return false;
            }
        }

        /// <summary>
        /// Retrieves the current active console session ID (User Desktop or Winlogon).
        /// </summary>
        public uint GetActiveSessionId()
        {
            try
            {
                return WTSGetActiveConsoleSessionId();
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Relaunches the current application with full Administrator privileges via Windows UAC.
        /// </summary>
        public bool RelaunchElevated(string additionalArgs = null)
        {
            if (IsElevated()) return true;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Application.ExecutablePath;
                psi.Verb = "runas";
                psi.Arguments = additionalArgs ?? "--elevated";
                psi.UseShellExecute = true;

                Process.Start(psi);
                Application.Exit();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[ELEVATION] Relaunch elevated canceled or failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Configures User Interface Privilege Isolation (UIPI) bypass filters
        /// allowing synthesized inputs to interact with elevated target windows.
        /// </summary>
        public void ConfigureUIPIBypass()
        {
            try
            {
                ChangeWindowMessageFilter(WM_COPYDATA, MSGFLT_ADD);
                ChangeWindowMessageFilter(WM_COMMAND, MSGFLT_ADD);
                ChangeWindowMessageFilter(WM_USER, MSGFLT_ADD);
            }
            catch { }
        }

        /// <summary>
        /// Detects whether the current active desktop is the Secure Desktop (Winlogon / UAC prompt Consent.exe).
        /// </summary>
        public bool IsSecureDesktopActive()
        {
            IntPtr hDesk = IntPtr.Zero;
            try
            {
                hDesk = OpenInputDesktop(0, false, DESKTOP_READOBJECTS);
                if (hDesk == IntPtr.Zero)
                {
                    int err = Marshal.GetLastWin32Error();
                    return err == 5; // ERROR_ACCESS_DENIED typically signifies Secure Desktop prompt
                }

                uint needed = 0;
                byte[] nameBuf = new byte[256];
                if (GetUserObjectInformation(hDesk, UOI_NAME, nameBuf, (uint)nameBuf.Length, out needed) && needed > 0)
                {
                    string deskName = System.Text.Encoding.Unicode.GetString(nameBuf, 0, (int)needed).TrimEnd('\0');
                    return string.Equals(deskName, "Winlogon", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(deskName, "Screen-saver", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
            }
            finally
            {
                if (hDesk != IntPtr.Zero)
                {
                    try { CloseDesktop(hDesk); } catch { }
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to attach the calling thread to the currently active input desktop
        /// (enabling seamless screen capture and synthetic input injection across UAC prompts).
        /// </summary>
        public bool TryAttachToInputDesktop(out IntPtr hInputDesktop)
        {
            hInputDesktop = IntPtr.Zero;
            try
            {
                IntPtr hDesk = OpenInputDesktop(0, false, DESKTOP_READOBJECTS | DESKTOP_SWITCHDESKTOP | DESKTOP_WRITEOBJECTS);
                if (hDesk != IntPtr.Zero)
                {
                    if (SetThreadDesktop(hDesk))
                    {
                        hInputDesktop = hDesk;
                        return true;
                    }
                    CloseDesktop(hDesk);
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Detaches from the input desktop and restores default desktop handle.
        /// </summary>
        public void RestoreDefaultDesktop(IntPtr hDesktop)
        {
            if (hDesktop != IntPtr.Zero)
            {
                try
                {
                    CloseDesktop(hDesktop);
                }
                catch { }
            }
        }

        /// <summary>
        /// Launches an interactive target process into the active user session from Session 0 (SYSTEM Service).
        /// </summary>
        public bool LaunchProcessInActiveSession(string applicationPath, string arguments = null)
        {
            IntPtr userToken = IntPtr.Zero;
            IntPtr duplicatedToken = IntPtr.Zero;
            try
            {
                uint activeSessionId = GetActiveSessionId();
                if (!WTSQueryUserToken(activeSessionId, out userToken))
                {
                    return false;
                }

                if (!DuplicateTokenEx(userToken, MAXIMUM_ALLOWED, IntPtr.Zero, SecurityImpersonation, TokenTypePrimary, out duplicatedToken))
                {
                    return false;
                }

                STARTUPINFO si = new STARTUPINFO();
                si.cb = Marshal.SizeOf(typeof(STARTUPINFO));
                si.lpDesktop = @"winsta0\default";

                PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
                string cmdLine = string.IsNullOrEmpty(arguments) 
                    ? string.Format("\"{0}\"", applicationPath)
                    : string.Format("\"{0}\" {1}", applicationPath, arguments);

                bool created = CreateProcessAsUser(
                    duplicatedToken,
                    null,
                    cmdLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    0,
                    IntPtr.Zero,
                    null,
                    ref si,
                    out pi);

                if (created)
                {
                    if (pi.hProcess != IntPtr.Zero) CloseHandle(pi.hProcess);
                    if (pi.hThread != IntPtr.Zero) CloseHandle(pi.hThread);
                }
                return created;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[ELEVATION] Session 0 dispatch failed: " + ex.Message);
                return false;
            }
            finally
            {
                if (userToken != IntPtr.Zero) CloseHandle(userToken);
                if (duplicatedToken != IntPtr.Zero) CloseHandle(duplicatedToken);
            }
        }
    }
}

