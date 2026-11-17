# Windows App Installation & User Guide (jDroid-X-Desk & Mini Desk)

**Application**: `jDroid-X-Desk` & `jDroid-X-DeskMini`  
**Architecture**: Native C# / .NET Multi-Mode Standalone Client & Universal Node Gateway  
**Default Standard Installation**: `%LOCALAPPDATA%\Programs\jDroid-X-Desk`  
**Repository Remote**: `https://github.com/jDroid-X/jDroid-X-Desk.git`  
**Local Gateway Port**: `http://localhost:12345` (Web Admin) | `8088` (P2P Stream) | `12345` (Native Relay)  

---

## 1. Standard Windows User Installation (`%LOCALAPPDATA%\Programs\jDroid-X-Desk`)

`jDroid-X-Desk` installs cleanly into the standard Windows user application directory—the identical per-user standard employed by VS Code, Google Chrome, Discord, and RustDesk. No administrator privileges or system root modifications are required.

### 1-Click Installer
1. Double-click **`install_windows_app.bat`** (or execute `jDroid-X-Desk.exe --install`).
2. **Installation Actions Performed**:
   - Creates `%LOCALAPPDATA%\Programs\jDroid-X-Desk` and `%LOCALAPPDATA%\Programs\jDroid-X-Desk\assets`.
   - Copies `jDroid-X-Desk.exe`, `jDroid-X-Desk.exe.config`, `app.manifest`, `version.json`, `colab_relay_server.py`, and certificates.
   - Registers Authenticode certificate in user `TrustedPublisher` store.
   - Unblocks Windows Mark-of-the-Web (MOTW) / SmartScreen flags.
   - Creates a **Desktop Shortcut** (`jDroid-X-Desk.lnk`) with high-resolution icon.
   - Creates a **Start Menu Shortcut** (`Programs\jDroid-X-Desk.lnk`).
   - Registers uninstall entry in Windows Registry `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk` and generates `uninstall.bat`.

---

## 2. Dual-Mode Launcher: Mini Desk vs Full Desk

When launching `jDroid-X-Desk.exe`:
- **Startup Mode Selector**: A sleek dialog appears on startup defaulting to **Mini Desk** with a 3-second auto-launch countdown.
- **Default Selected**: `📱 Mini Desk` (Instant connect widget).
- **Secondary Option**: `🖥️ Desk Console` (Full multi-tab workstation studio).
- **Remember Preference**: Checkbox persists selection in `database.json` (`settings.general.startupMode = "mini" | "desk" | "ask"`).
- **CLI Flags**:
  - `jDroid-X-Desk.exe --mini` : Launches Mini Desk directly.
  - `jDroid-X-Desk.exe --desk` : Launches Full Desk Console directly.
  - `jDroid-X-Desk.exe --install` : Installs application to standard user folder.

---

## 3. jDroid-X-DeskMini (`MiniDeskForm`) Features

The Mini Desk provides minimal screen footprint with 100% backend capability running in background:
- **Top-Left Menu (`[☰]`)**:
  - `📋 Connect Detail`: Switches immediately to the full detailed Console (`MainForm`).
  - `🗄️ Address Book / Fleet`: Opens partner contacts roster.
  - `⚙️ Settings Studio`: Opens the 12-panel configuration console.
  - `🔄 Check for Updates...`: Triggers instant SSOT auto-update verification.
  - `📦 Install to User Folder`: Migrates portable binary into standard user directory.
- **This Desk Card**:
  - Displays your formatted 9-digit Desk ID (e.g. `729-928-576`).
  - `[📋 Copy]` button with tooltip to quickly share with remote partner.
  - Live indicator: `🟢 Active & Ready for Incoming Connections`.
- **Remote Desk Card**:
  - Auto-formatting `ModernTextBox` with `XXX-XXX-XXX` hyphenation.
  - `[⚡ Connect]` button with real-time status feedback.
- **Mandatory Icon Action Ribbon (with Rich ToolTips)**:
  - `➕ Add`: Adds remote Desk ID to contacts and address book.
  - `▶️ Start`: Starts/resumes background host screen broadcast.
  - `⏹️ Stop`: Disconnects active remote session.
  - `ℹ️ Details`: Connect Detail button opening full workspace.
  - `⛶ Full Screen`: Toggles full-screen display mode.
  - `✕ Close`: Minimizes or closes application cleanly.
- **Bottom Status Bar**:
  - Real-time connection telemetry (FPS, local port, direct & relay connectivity).

---

## 4. Seamless Switching Between Mini Desk and Full Desk

- **Mini Desk ➔ Full Desk**: Click `[☰] -> Connect Detail` or the `[ℹ️ Details]` icon or `[🗖 Details]` button in top-right.
- **Full Desk ➔ Mini Desk**: Click the `[📱]` Mini Desk icon button in the top right header controls.
- **Background Persistence**: Underlying background daemons (`EmbeddedRelayServer`, `LanDiscoveryEngine`, `CloudflareTunnelEngine`, `WasapiAudioCaptureService`) continue running uninterrupted without reconnecting or dropping sessions.

---

## 5. Automated Updates (Size & Version SSOT Verification)

`AutoUpdateManager` keeps `jDroid-X-Desk` synchronized with the latest GitHub releases:
- Compares local file size and version against remote `version.json` and HTTP headers (`Content-Length`).
- If a newer version or file size difference is detected:
  - Prompts user with details (Current vs Remote version and size).
  - Downloads binary in background to `jDroid-X-Desk.exe.update`.
  - Verifies valid PE header (`MZ`).
  - Launches self-updating process, cleanly replaces executable, unblocks file, and restarts application.
- Can be triggered manually via `[🔄 Check for Updates]` or automatically via `settings.general.autoUpdate`.

---

## 6. Verification and Testing

Execute the test suite at any time:
```cmd
test.bat
```
Build modular native solution:
```cmd
compile_native.bat
```
