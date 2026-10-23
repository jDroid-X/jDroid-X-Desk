# jDroid-X-Desk: Master Operational & RCA Issue Checklist

> **Purpose**: A living, comprehensive Root Cause Analysis (RCA) checklist and diagnostic standard for resolving operational bugs, stream freezes, connection drops, and input failures across **jDroid-X-Desk** (Native C# WinForms Client, Node.js Gateway, and Web HTML5 Viewport).

---

## ⚡ 30-Second Quick-Triage Flowchart

When an active session fails or freezes, perform this quick triage first:

```
                      [Active Session Freezes / Fails]
                                     |
              +----------------------+----------------------+
              |                                             |
    [0. Dual-Desk Liveness Check]               [Check HUD / Frame Counter]
              |                                             |
      +-------+-------+                             +-------+-------+
      |               |                             |               |
[Both Desks Online] [One Desk Offline]         [FPS = 0]       [RTT = Timeout]
      |               |                             |               |
  (Test Remote Input) (Desk B Slept /           (Capture or      (Transport or
      |                App Exited)                Relay Stalled)  Tunnel Disconnected)
      |               |                             |               |
  Section 1       Section 0                     Section 2       Section 5
```

---

## 0. Dual-Desk Liveness & Presence Checklist (Are Both Desks Active?)

*MANDATORY STEP 0: Before analyzing stream pipelines, verify that both workstations are actively running the application and registered with the relay network.*

- [ ] **0.1 Desk A (Viewer / Controller) Liveness**:
  - [ ] Is the app open and visible on Desk A?
  - [ ] If Web Console: Is the browser tab active, or did Chrome/Edge suspend it due to tab discarding/battery saver?
  - [ ] If Native App: Is `jDroid-X-Desk.exe` running in Task Manager (`Get-Process -Name "jDroid-X-Desk"`)?
- [ ] **0.2 Desk B (Host / Remote Machine) Liveness**:
  - [ ] Is the app actually open and running on Desk B?
  - [ ] Check if the app crashed silently on Desk B or exited on unhandled exception.
  - [ ] Check if Desk B minimized to the **System Tray**:
    - When minimized, is the background Host Daemon (`StartHostDaemon()`) still active, or did the process suspend?
  - [ ] Check relay device registry: Has Desk B checked in within the last 15 seconds?
    ```powershell
    curl.exe -s http://127.0.0.1:12345/api/relay/devices
    # Or WAN:
    curl.exe -s https://desk.jdroid-x.com/api/relay/devices
    ```
    - Verify `data[]` contains Desk B's Desk ID and `lastSeen` timestamp is fresh (< 15000ms ago).
- [ ] **0.3 LAN Zero-Config Discovery Probe**:
  - [ ] If both machines are on the same Wi-Fi / LAN, probe LAN discovery table:
    ```powershell
    curl.exe -s http://127.0.0.1:12345/api/discovered-desks
    ```
    - Does Desk B appear in the discovered LAN desks list with its local IP (e.g. `192.168.1.X`)?
- [ ] **0.4 Power State, Sleep & Laptop Lid Check**:
  - [ ] Did Desk B go into **Sleep**, **Hibernate**, or **Modern Standby**?
  - [ ] If Desk B is a laptop: Did someone **close the laptop lid**?
    - On Windows, closing the lid disables display output and halts CPU threads unless set to *"When I close the lid: Do nothing"*.
- [ ] **0.5 Windows User Account & Session Lockout**:
  - [ ] Did Desk B lock Windows (`Win + L`) or switch user?
  - [ ] Did a screensaver activate on Desk B?
  - [ ] Did a remote RDP session disconnect on Desk B, leaving Windows in Session 0 / headless state?

---

## 1. Handshake, Consent & Security Authorization Checklist

*Use when Desk A attempts to connect to Desk B, but the session hangs indefinitely on "Connecting..." or stays frozen on the initial frame.*

- [ ] **1.1 Interactive Consent Prompt on Desk B**:
  - [ ] Does Desk B require user confirmation before allowing remote control?
  - [ ] Look at Desk B's physical screen: Is an **"Incoming Connection Request: Allow / Reject"** dialog waiting for someone to click **"Allow"**?
  - [ ] If unattended access is enabled, verify unattended password matches.
- [ ] **1.2 Passcode / Credential Expiration**:
  - [ ] Did the One-Time Passcode regenerate on Desk B?
  - [ ] Verify passcode entered in `#target-pass-input` matches Desk B's current active passcode.
- [ ] **1.3 Ghost / Zombie Session Disconnect**:
  - [ ] Did Desk B click "Disconnect" or close the session, while Desk A was not notified?
  - [ ] If the disconnect packet was dropped, Desk A will remain frozen on the last frame received.

---

## 2. Frozen Screen & Video Pipeline Checklist

*Use when the viewport stops updating, shows a black screen, or stays stuck on a static frame.*

- [ ] **2.1 Directionality Verification**:
  - [ ] Move mouse or hit `Win` key in the frozen viewer. Does the physical host respond?
    - If **YES**: Input loop is healthy; only the video capture/relay/render pipeline is broken.
    - If **NO**: Connection is completely dead or host process is frozen.
- [ ] **2.2 Host Screen Capture Daemon Liveness**:
  - [ ] Verify `isHostDaemonRunning` / `screenStreamerThread` is active on host:
    ```powershell
    Get-Process -Name "jDroid-X-Desk" -ErrorAction SilentlyContinue | Select-Object Id, CPU, Responding
    ```
  - [ ] Check if host GDI+ `CopyFromScreen` is failing due to a minimized or virtual desktop session.
  - [ ] Check if the host screen capture is sleeping on `Thread.Sleep(500)` due to `isServingViewer == false`.
- [ ] **2.3 Host Outbound Push Route**:
  - [ ] Check if host `PushLocalHostFrame()` is sending to the correct relay URL and port:
    - Does `GetRelayCandidateUrls()` include local port `12345` (Node.js Gateway)?
    - Is it attempting WAN URL `https://desk.jdroid-x.com` while offline?
  - [ ] Verify outbound push format: Is it utilizing `/api/relay/push_bin` (RAW binary) or hanging on `/api/relay/push_frame` (JSON Base64)?
  - [ ] Confirm request timeout on host is non-blocking (<= 1200ms).
- [ ] **2.4 Relay In-Memory Buffer & Cache Retention**:
  - [ ] Check relay buffer status:
    ```powershell
    curl.exe -s http://127.0.0.1:12345/api/relay/status
    ```
  - [ ] Verify frame age: Has the frame expired due to a premature eviction threshold (e.g. `timestamp < 30000ms`)?
  - [ ] **Key Normalization**: Does the target ID match between Host and Viewer?
    - Format mismatch check: `984-102-391` vs `984102391` vs `JDX-3C16-2B17`.
    - Both exact, normalized-alphanumeric, and hardware-ID keys must resolve to the active frame buffer.
- [ ] **2.5 Viewer Frame Pull Loop (DevTools Network Tab)**:
  - [ ] Are GET requests for `/api/relay/frame_bin` actively firing?
    - **HTTP 200**: Frame received. If canvas is still frozen, inspect decode step.
    - **HTTP 204**: Relay has no frame stored for target ID (Host is not pushing).
    - **ERR_CONNECTION_REFUSED / TIMED_OUT**: Gateway is unreachable.
- [ ] **2.6 Viewer Canvas Render & Decode**:
  - [ ] Check browser console for `ImageDecodeError` or `createImageBitmap` rejection.
  - [ ] Ensure canvas dimensions (`canvas.width`, `canvas.height`) match incoming stream bitmap dimensions.
  - [ ] Check for background tab throttling: Did the viewer minimize or blur the browser tab? (`visibilitychange` handling).

---

## 3. Infinite Mirror Loop (Hall of Mirrors) Checklist

*Use when the screen recursively captures itself in an infinite visual tunnel.*

- [ ] **3.1 Self-Connection Prevention**:
  - [ ] Check if user entered their own local Host Desk ID as the target.
  - [ ] Verify self-connection blocker fires in both Native C# (`InitiateConnectToTargetId`) and Web JS (`connectBtn.click`).
  - [ ] Verify alert toast: *"Cannot connect to your own Desk ID. Connect from another workstation to avoid infinite screen recursion."*
- [ ] **3.2 Dedicated Loopback Shield HUD**:
  - [ ] Ensure local preview/mirror mode calls `RenderLoopbackShieldScreen` instead of GDI `CopyFromScreen`.
  - [ ] On web canvas, verify `renderAntiRecursionShield()` displays real telemetry without recursive pixel sampling.
- [ ] **3.3 Window Exclusion from Capture**:
  - [ ] If local screen capture is active on the same desktop, ensure viewport canvas does not capture its own display bounds.

---

## 4. Input Relay & Remote Control Checklist

*Use when screen is visible, but mouse clicks, cursor moves, or keystrokes do not affect the host.*

- [ ] **4.1 Viewer Input Event Dispatch**:
  - [ ] In browser DevTools Network, verify POST `/api/relay/push_input` or `/api/relay/input` on mouse click/keyboard press.
  - [ ] Check payload structure: Contains `{ targetId, type: "mouse"|"key", x, y, btn }`.
- [ ] **4.2 Server Queue Contract**:
  - [ ] Ensure server stores inputs in `relayInputs` map keyed by both raw ID and clean alphanumeric ID.
  - [ ] Verify GET `/api/relay/input` returns both `events: [...]` AND `inputs: [...]` for legacy/native compatibility.
- [ ] **4.3 Host Input Pull Daemon**:
  - [ ] Is host actively polling `/api/relay/pull_input/<hostId>` every 30-50ms?
  - [ ] Check native C# deserialization: Does it inspect `dict["inputs"]` OR `dict["events"]`?
- [ ] **4.4 Win32 Native Input Injection (`user32.dll`)**:
  - [ ] Are permissions enabled: `hostPermissionAllowInput == true`?
  - [ ] Is input blocked: Did remote trigger `block_input` or is `isLocalInputBlockedByRemote == true`?
  - [ ] Coordinate scaling check: Are viewer canvas coordinates properly mapped to host primary monitor resolution (`SetCursorPos(realX, realY)`)?
  - [ ] Check mouse button flags: `MOUSEEVENTF_LEFTDOWN`, `MOUSEEVENTF_LEFTUP`, `MOUSEEVENTF_RIGHTDOWN`.

---

## 5. Network, Relay & Active Engine Checklist

*Use when machines cannot discover each other, connection times out, or RTT is excessive.*

- [ ] **5.1 Listening Port Verification**:
  - [ ] Verify active listeners on the host/relay:
    ```powershell
    netstat -ano | findstr "12345 8088 7700 7070 21118"
    ```
    - Port `12345`: Node.js Express Gateway & Web Console.
    - Port `7700`: Gateway Relay Port.
    - Port `8088`: Colab Relay Hub.
    - Port `7070`: Native WinForms Embedded P2P Listener.
- [ ] **5.2 Active Streaming Engine Mode**:
  - [ ] Check configured engine in `database.json`:
    ```powershell
    curl.exe -s http://127.0.0.1:12345/api/admin/config
    ```
    - If `activeEngine` is set to `guacd`, `moonlight`, or `colab`, verify that the external engine daemon is actually running on its assigned port.
    - If the external engine is down, switch active engine back to `NodeRelayHub` (in-memory zero-dependency engine).
- [ ] **5.3 Cloudflare Tunnel / Public Relay Liveness**:
  - [ ] Check `colab_tunnel.txt` and `windows_rdp.yaml` for active tunnel URL.
  - [ ] Verify tunnel process is running:
    ```powershell
    Get-Process -Name "cloudflared" -ErrorAction SilentlyContinue
    ```
  - [ ] Probe public domain: `curl.exe -I -m 5 https://desk.jdroid-x.com` (Must return HTTP 200).
- [ ] **5.4 Firewall & Windows Defender Block**:
  - [ ] Ensure Windows Firewall rule exists for inbound connections on the assigned ports:
    ```powershell
    netsh advfirewall firewall show rule name="jDroid-X-Desk"
    ```

---

## 6. OS, Display & Headless Hardware Quirks Checklist

*Use when capture freezes without software error messages.*

- [ ] **6.1 Windows UAC / Secure Desktop Lockout**:
  - [ ] Did an Administrator prompt or Windows Credential dialog appear on host?
  - [ ] Standard user-mode GDI capture cannot read the Winlogon/Secure Desktop. The screen will pause until UAC is dismissed.
- [ ] **6.2 Multi-Monitor & Wrong Screen Capture**:
  - [ ] Does Desk B have multiple monitors attached?
  - [ ] Did the capture engine select `Screen.AllScreens[1]` (Monitor 2) which is empty or powered off?
  - [ ] Verify capture targets `Screen.PrimaryScreen` or the active focused monitor.
- [ ] **6.3 Headless Machine (No Physical Monitor Attached)**:
  - [ ] Is Desk B a headless server / minipc with no monitor plugged into the HDMI/DP port?
  - [ ] Windows disables display rendering on headless GPUs (0x0 desktop bounds).
  - [ ] Solution: Use an HDMI Dummy Plug / Virtual Display Driver.
- [ ] **6.4 Display Resolution & DPI Scale Change**:
  - [ ] Did the host change resolution, attach an external monitor, or dock/undock?
  - [ ] Verify capture thread detects `SystemEvents.DisplaySettingsChanged` and reallocates bitmap buffers.
- [ ] **6.5 GPU Driver Timeout (TDR)**:
  - [ ] Check Windows Event Viewer for Event ID 4101 (GPU driver crash and recovery).
  - [ ] Restart application if DirectX/GDI surfaces lost hardware acceleration.

---

## 7. UI/UX & State Synchronization Checklist

*Use when visual controls desynchronize between Native app and Web console.*

- [ ] **7.1 Re-entrancy & Toast Spam**:
  - [ ] Are notification toasts firing multiple times (e.g. repeated "Theme applied" alerts)?
  - [ ] Verify `isUpdatingThemeProgrammatically` re-entrancy flags and debounce locks are active.
- [ ] **7.2 Connection Button State Sync**:
  - [ ] When connected, do both Header action bar and Card buttons show `✕ Disconnect` in red?
  - [ ] When disconnected, do buttons revert to `⚡ Connect` without requiring a page refresh?
- [ ] **7.3 Dual Controller Synchronization**:
  - [ ] Ensure [app.js](file:///c:/Users/jiten\jAnitGravity\jDroid-X-Desk\app.js) and [controllers/app-controller.js](file:///c:/Users/jiten\jAnitGravity\jDroid-X-Desk\controllers\app-controller.js) remain 100% identical via binary comparison:
    ```powershell
    cmd /c "fc app.js controllers\app-controller.js"
    ```

---

## 8. Diagnostic CLI Toolkit (Rapid Execution)

Run these diagnostic commands to gather immediate state across all subsystems:

| Target Domain | Diagnostic One-Liner Command |
| :--- | :--- |
| **Both Desks Online?** | `curl.exe -s http://127.0.0.1:12345/api/relay/devices` |
| **LAN Desks Discovered?** | `curl.exe -s http://127.0.0.1:12345/api/discovered-desks` |
| **Process Status** | `Get-Process -Name "jDroid-X-Desk", "node", "cloudflared" -ErrorAction SilentlyContinue \| Format-Table Id, ProcessName, CPU, WorkingSet64` |
| **Port Listeners** | `netstat -ano \| findstr "LISTENING" \| findstr "12345 8088 7700 7070"` |
| **Relay Health** | `curl.exe -s -m 3 http://127.0.0.1:12345/api/relay/status` |
| **Frame Buffer Probe** | `curl.exe -s -m 3 http://127.0.0.1:12345/api/relay/frame?targetId=DEFAULT` |
| **Active Engine Probe** | `curl.exe -s -m 3 http://127.0.0.1:12345/api/admin/config` |
| **Test Suite Run** | `node test.js` |
| **Compile Native C#** | `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /platform:anycpu /optimize+ /debug- /win32manifest:app.manifest /r:System.dll,System.Windows.Forms.dll,System.Drawing.dll,System.Web.Extensions.dll,System.Net.dll,System.Data.dll,System.Xml.dll,System.Core.dll /out:jDroid-X-Desk.exe JDroidXDeskNative.cs` |

---

## 📝 Continuous Improvement & Gap Register

*Log newly discovered failure modes, edge cases, or platform quirks here:*

| Date | Defect Description | Root Cause | Preventive Check Added |
| :--- | :--- | :--- | :--- |
| 2026-10-23 | Screen frozen on remote side mid-session | Desk B was not confirmed active / Relay frame eviction (<30s) / Target ID format mismatch / Native input key contract (`inputs` vs `events`) | Section 0.1, 0.2, 2.4, 4.2 |
| 2026-10-23 | Repeated incoming session dialogs (never dismisses) | Ephemeral TCP client port in `ep` changed on every HTTP request, failing `ep == lastApprovedViewerEndpoint` | Section 1.1, 1.2 |
| 2026-10-23 | Input works but screen is frozen | Synchronous HTTP POST to external relays (`timeout=3500ms`) blocked host GDI capture thread to 0.2 FPS | Section 2.3 |
| 2026-10-23 | Stuck in infinite mirror loop | User connected to own Desk ID; raw `CopyFromScreen` in preview | Section 3.1, 3.2 |
| 2026-10-22 | Repeated theme applied notification toasts | Programmatic theme update triggered event handler recursively | Section 7.1 |
| *Open* | Windows UAC prompt causes black frame | Winlogon Secure Desktop boundary | Section 6.1 |
| *Open* | Laptop lid closed stops capture | Windows OS display power management | Section 0.4 |
| *Open* | Headless PC has 0x0 resolution | GPU disables display pipe without HDMI load | Section 6.3 |
