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
    [Test Remote Input: Key/Mouse]                  [Check HUD / Frame Counter]
              |                                             |
      +-------+-------+                             +-------+-------+
      |               |                             |               |
[Host Reacts]   [No Reaction]                  [FPS = 0]       [RTT = Timeout]
      |               |                             |               |
(Video Pipeline (Complete Drop /              (Capture or      (Transport or
 Dead / Stalled)  Socket Timeout)               Relay Stalled)  Tunnel Disconnected)
      |               |                             |               |
  Section 1       Section 4                     Section 1       Section 4
```

---

## 1. Frozen Screen & Video Pipeline Checklist

*Use when the viewport stops updating, shows a black screen, or stays stuck on a static frame.*

- [ ] **1.1 Directionality Verification**:
  - [ ] Move mouse or hit `Win` key in the frozen viewer. Does the physical host respond?
    - If **YES**: Input loop is healthy; only the video capture/relay/render pipeline is broken.
    - If **NO**: Connection is completely dead or host process is frozen.
- [ ] **1.2 Host Screen Capture Daemon Liveness**:
  - [ ] Verify `isHostDaemonRunning` / `screenStreamerThread` is active on host:
    ```powershell
    Get-Process -Name "jDroid-X-Desk" -ErrorAction SilentlyContinue | Select-Object Id, CPU, Responding
    ```
  - [ ] Check if host GDI+ `CopyFromScreen` is failing due to a minimized or virtual desktop session.
  - [ ] Check if the host screen capture is sleeping on `Thread.Sleep(500)` due to `isServingViewer == false`.
- [ ] **1.3 Host Outbound Push Route**:
  - [ ] Check if host `PushLocalHostFrame()` is sending to the correct relay URL and port:
    - Does `GetRelayCandidateUrls()` include local port `12345` (Node.js Gateway)?
    - Is it attempting WAN URL `https://desk.jdroid-x.com` while offline?
  - [ ] Verify outbound push format: Is it utilizing `/api/relay/push_bin` (RAW binary) or hanging on `/api/relay/push_frame` (JSON Base64)?
  - [ ] Confirm request timeout on host is non-blocking (<= 1200ms).
- [ ] **1.4 Relay In-Memory Buffer & Cache Retention**:
  - [ ] Check relay buffer status:
    ```powershell
    curl.exe -s http://127.0.0.1:12345/api/relay/status
    ```
  - [ ] Verify frame age: Has the frame expired due to a premature eviction threshold (e.g. `timestamp < 30000ms`)?
  - [ ] **Key Normalization**: Does the target ID match between Host and Viewer?
    - Format mismatch check: `984-102-391` vs `984102391` vs `JDX-3C16-2B17`.
    - Both exact, normalized-alphanumeric, and hardware-ID keys must resolve to the active frame buffer.
- [ ] **1.5 Viewer Frame Pull Loop (DevTools Network Tab)**:
  - [ ] Are GET requests for `/api/relay/frame_bin` actively firing?
    - **HTTP 200**: Frame received. If canvas is still frozen, inspect decode step.
    - **HTTP 204**: Relay has no frame stored for target ID (Host is not pushing).
    - **ERR_CONNECTION_REFUSED / TIMED_OUT**: Gateway is unreachable.
- [ ] **1.6 Viewer Canvas Render & Decode**:
  - [ ] Check browser console for `ImageDecodeError` or `createImageBitmap` rejection.
  - [ ] Ensure canvas dimensions (`canvas.width`, `canvas.height`) match incoming stream bitmap dimensions.
  - [ ] Check for background tab throttling: Did the viewer minimize or blur the browser tab? (`visibilitychange` handling).

---

## 2. Infinite Mirror Loop (Hall of Mirrors) Checklist

*Use when the screen recursively captures itself in an infinite visual tunnel.*

- [ ] **2.1 Self-Connection Prevention**:
  - [ ] Check if user entered their own local Host Desk ID as the target.
  - [ ] Verify self-connection blocker fires in both Native C# (`InitiateConnectToTargetId`) and Web JS (`connectBtn.click`).
  - [ ] Verify alert toast: *"Cannot connect to your own Desk ID. Connect from another workstation to avoid infinite screen recursion."*
- [ ] **2.2 Dedicated Loopback Shield HUD**:
  - [ ] Ensure local preview/mirror mode calls `RenderLoopbackShieldScreen` instead of GDI `CopyFromScreen`.
  - [ ] On web canvas, verify `renderAntiRecursionShield()` displays real telemetry without recursive pixel sampling.
- [ ] **2.3 Window Exclusion from Capture**:
  - [ ] If local screen capture is active on the same desktop, ensure viewport canvas does not capture its own display bounds.

---

## 3. Input Relay & Remote Control Checklist

*Use when screen is visible, but mouse clicks, cursor moves, or keystrokes do not affect the host.*

- [ ] **3.1 Viewer Input Event Dispatch**:
  - [ ] In browser DevTools Network, verify POST `/api/relay/push_input` or `/api/relay/input` on mouse click/keyboard press.
  - [ ] Check payload structure: Contains `{ targetId, type: "mouse"|"key", x, y, btn }`.
- [ ] **3.2 Server Queue Contract**:
  - [ ] Ensure server stores inputs in `relayInputs` map keyed by both raw ID and clean alphanumeric ID.
  - [ ] Verify GET `/api/relay/input` returns both `events: [...]` AND `inputs: [...]` for legacy/native compatibility.
- [ ] **3.3 Host Input Pull Daemon**:
  - [ ] Is host actively polling `/api/relay/pull_input/<hostId>` every 30-50ms?
  - [ ] Check native C# deserialization: Does it inspect `dict["inputs"]` OR `dict["events"]`?
- [ ] **3.4 Win32 Native Input Injection (`user32.dll`)**:
  - [ ] Are permissions enabled: `hostPermissionAllowInput == true`?
  - [ ] Is input blocked: Did remote trigger `block_input` or is `isLocalInputBlockedByRemote == true`?
  - [ ] Coordinate scaling check: Are viewer canvas coordinates properly mapped to host primary monitor resolution (`SetCursorPos(realX, realY)`)?
  - [ ] Check mouse button flags: `MOUSEEVENTF_LEFTDOWN`, `MOUSEEVENTF_LEFTUP`, `MOUSEEVENTF_RIGHTDOWN`.

---

## 4. Network, Relay & Transport Checklist

*Use when machines cannot discover each other, connection times out, or RTT is excessive.*

- [ ] **4.1 Listening Port Verification**:
  - [ ] Verify active listeners on the host/relay:
    ```powershell
    netstat -ano | findstr "12345 8088 7700 7070 21118"
    ```
    - Port `12345`: Node.js Express Gateway & Web Console.
    - Port `7700`: Gateway Relay Port.
    - Port `8088`: Colab Relay Hub.
    - Port `7070`: Native WinForms Embedded P2P Listener.
- [ ] **4.2 Zero-Config LAN UDP Auto-Discovery**:
  - [ ] Are UDP discovery beacons transmitting on port `8089`?
  - [ ] Check if subnet broadcast (`255.255.255.255`) is blocked by Wi-Fi AP Isolation or local firewall.
- [ ] **4.3 Cloudflare Tunnel / Public Relay Liveness**:
  - [ ] Check `colab_tunnel.txt` and `windows_rdp.yaml` for active tunnel URL.
  - [ ] Verify tunnel process is running:
    ```powershell
    Get-Process -Name "cloudflared" -ErrorAction SilentlyContinue
    ```
  - [ ] Probe public domain: `curl.exe -I -m 5 https://desk.jdroid-x.com` (Must return HTTP 200).
- [ ] **4.4 Firewall & Windows Defender Block**:
  - [ ] Ensure Windows Firewall rule exists for inbound connections on the assigned ports:
    ```powershell
    netsh advfirewall firewall show rule name="jDroid-X-Desk"
    ```

---

## 5. OS & Hardware Level Blocks Checklist

*Use when capture freezes without software error messages.*

- [ ] **5.1 Windows UAC / Secure Desktop Lockout**:
  - [ ] Did an Administrator prompt or Windows Credential dialog appear on host?
  - [ ] Standard user-mode GDI capture cannot read the Winlogon/Secure Desktop. The screen will pause until UAC is dismissed.
- [ ] **5.2 Display Sleep / Monitor Power-Off**:
  - [ ] Did host monitors power down due to Windows power plan?
  - [ ] GDI+ surfaces stop refreshing when displays enter D3 low-power state.
- [ ] **5.3 Display Resolution & DPI Scale Change**:
  - [ ] Did the host change resolution, attach an external monitor, or dock/undock?
  - [ ] Verify capture thread detects `SystemEvents.DisplaySettingsChanged` and reallocates bitmap buffers.
- [ ] **5.4 GPU Driver Timeout (TDR)**:
  - [ ] Check Windows Event Viewer for Event ID 4101 (GPU driver crash and recovery).
  - [ ] Restart application if DirectX/GDI surfaces lost hardware acceleration.

---

## 6. UI/UX & State Synchronization Checklist

*Use when visual controls desynchronize between Native app and Web console.*

- [ ] **6.1 Re-entrancy & Toast Spam**:
  - [ ] Are notification toasts firing multiple times (e.g. repeated "Theme applied" alerts)?
  - [ ] Verify `isUpdatingThemeProgrammatically` re-entrancy flags and debounce locks are active.
- [ ] **6.2 Connection Button State Sync**:
  - [ ] When connected, do both Header action bar and Card buttons show `✕ Disconnect` in red?
  - [ ] When disconnected, do buttons revert to `⚡ Connect` without requiring a page refresh?
- [ ] **6.3 Dual Controller Synchronization**:
  - [ ] Ensure [app.js](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/app.js) and [controllers/app-controller.js](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/controllers/app-controller.js) remain 100% identical via binary comparison:
    ```powershell
    cmd /c "fc app.js controllers\app-controller.js"
    ```

---

## 7. Diagnostic CLI Toolkit (Rapid Execution)

Run these diagnostic commands to gather immediate state across all subsystems:

| Target Domain | Diagnostic One-Liner Command |
| :--- | :--- |
| **Process Status** | `Get-Process -Name "jDroid-X-Desk", "node", "cloudflared" -ErrorAction SilentlyContinue \| Format-Table Id, ProcessName, CPU, WorkingSet64` |
| **Port Listeners** | `netstat -ano \| findstr "LISTENING" \| findstr "12345 8088 7700 7070"` |
| **Relay Health** | `curl.exe -s -m 3 http://127.0.0.1:12345/api/relay/status` |
| **Frame Buffer Probe** | `curl.exe -s -m 3 http://127.0.0.1:12345/api/relay/frame?targetId=DEFAULT` |
| **Hardware ID Probe** | `curl.exe -s -m 3 http://127.0.0.1:12345/api/hardware-id` |
| **Test Suite Run** | `node test.js` |
| **Compile Native C#** | `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /platform:anycpu /optimize+ /debug- /win32manifest:app.manifest /r:System.dll,System.Windows.Forms.dll,System.Drawing.dll,System.Web.Extensions.dll,System.Net.dll,System.Data.dll,System.Xml.dll,System.Core.dll /out:jDroid-X-Desk.exe JDroidXDeskNative.cs` |

---

## 📝 Continuous Improvement & Gap Register

*Log newly discovered failure modes, edge cases, or platform quirks here:*

| Date | Defect Description | Root Cause | Preventive Check Added |
| :--- | :--- | :--- | :--- |
| 2026-10-23 | Screen frozen on remote side mid-session | Relay frame eviction (<30s) + Target ID format mismatch (`984-102-391` vs `984102391`) + Native input key contract (`inputs` vs `events`) | Section 1.4, 3.2, 3.3 |
| 2026-10-23 | Stuck in infinite mirror loop | User connected to own Desk ID; raw `CopyFromScreen` in preview | Section 2.1, 2.2 |
| 2026-10-22 | Repeated theme applied notification toasts | Programmatic theme update triggered event handler recursively | Section 6.1 |
| *Open* | Windows UAC prompt causes black frame | Winlogon Secure Desktop boundary | Section 5.1 |
