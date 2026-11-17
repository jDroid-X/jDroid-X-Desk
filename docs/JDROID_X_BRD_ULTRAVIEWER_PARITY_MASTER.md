# jDroid-X-Desk: UltraViewer Architectural REA & Gap Closure Master Specification
**Document Reference:** `docs/JDROID_X_BRD_ULTRAVIEWER_PARITY_MASTER.md`  
**Target Application:** `jDroid-X-Desk` (`c:\Users\jiten\jAnitGravity\jDroid-X-Desk`)  
**Analyzed Binary:** `UltraViewer_setup_6.6.124_en.exe` (Inno Setup 5.5.7, 3.64 MB)  
**Operator Circuit:** JEET & jDroid-X  
**Timestamp:** 2026-10-28T15:21:00+05:30  

---

## 1. System Architectural Overview & Wire Protocol Design

UltraViewer operates across 4 core binary domains:
1. **Presentation & Coordination Engine (`RemoteControl.dll` / `UltraViewer.exe`)**:
   - .NET WinForms CLR controller managing state transitions, address books (`VContactTreeView`), chat rendering (`HtmlAgilityPack.dll`), audio (`NAudio.dll`), and network sockets (`VTCPClient` / `VUDPClient`).
2. **Native Performance Acceleration (`UVCPlus.dll`)**:
   - C++ SIMD-accelerated tile-based screen encoder. Computes tile hashes over 32×32 pixel blocks and transmits only modified blocks using dynamic TurboJPEG quality factors.
3. **Privilege & Session Broker (`UltraViewer_Service.exe`)**:
   - Windows Service (`LocalSystem`) executing in Session 0. Monitors `OnSessionChange` using `wtsapi32.dll`, launches the 64-bit helper `uv_x64.exe` inside active interactive sessions via `WTSQueryUserToken`, and navigates UAC / Secure Desktops (`Consent.exe`) and Lock Screens (`Winlogon`).
4. **Input Hook & Injection (`uvh.dll` / `uvh64.dll`)**:
   - Intercepts local hardware input to grant local operator priority (`SKIP_BECAUSE_LOCAL_USER_IS_USING`) and injects remote input via `SendInput`.

---

## 2. Granular 9-Step Operational Lifecycle

### Step 1: Client Boot & Cryptographic Identity Generation
- Hardware fingerprint generated via Motherboard UUID + Primary NIC MAC + Windows Product GUID.
- Queries registry for unattended `FixedPassword`. If not found, generates a 4-digit cryptographically random `RandomPassword`.
- Connects to local `UltraViewer_Service.exe` via Named Pipe `\\.\pipe\UltraViewerPipe`.

### Step 2: Signaling Registration & Keepalive Loop
- Establishes TLS 1.3 socket to central cluster (`https://ultraviewer.net/update/`).
- Binds random UDP port (`BindRandomPort`) for P2P candidate discovery.
- Emits ping heartbeats every 10 seconds via `VPinger`.

### Step 3: Peer Discovery & Connection Handshake
- Operator enters 8-9 digit Partner ID.
- Controller sends `IVED_CONNECT_REQUEST` packet to signaling relay.
- Signaling server locates target host; if offline, returns `CONNECTFAIL`.

### Step 4: Authentication & Granular Access Gates
- Controller transmits salted password hash.
- Target validates against active `RandomPassword` or `FixedPassword`.
- On 3 consecutive failures, triggers 60-second backoff lockout.
- If prompt option is enabled, displays modal authorization dialog to local user.

### Step 5: Multi-Path Transport Negotiation (P2P vs. Relay)
- Attempts UDP STUN hole-punching (`ConvertSocketToPeerToPeer`).
- If successful, sets `IsPeer2Peer = True` and switches directly to direct UDP.
- If NAT restricts hole-punching, falls back to encrypted transit relay (`EnumP2PType.Relay`).

### Step 6: Screen Capture & Tile-Differential Compression
- Captures active monitors via Desktop Duplication API / GDI BitBlt.
- Divides screen into 32×32 pixel matrix.
- `clsStoredFrame` compares CRC32/MD5 tile hashes against previous frame.
- Modified tiles are compressed via TurboJPEG and packed with bounding boxes `[X, Y, Width, Height]`.

### Step 7: Remote Input Injection & UAC / Secure Desktop Elevation
- Operator input translated to `WM_KEYDOWN`, `WM_KEYUP`, `WM_MOUSEMOVE`, etc.
- If local user moves mouse/keys, input is dropped (`SKIP_BECAUSE_LOCAL_USER_IS_USING`).
- When Windows enters `Consent.exe` or `Winlogon`, `UltraViewer_Service.exe` duplicates token with `WTSQueryUserToken`, calls `SetThreadDesktop`, and injects input seamlessly.

### Step 8: Auxiliary Communication (Chat, Audio, Clipboard, File Transfer)
- In-session chat rendered with HTML styling (`VChatBrowser`) and suggestion engine (`CHAT_SUGGEST`).
- Audio loopback captured with WASAPI via `NAudio.dll`.
- Bidirectional clipboard synchronized via `AddClipboardFormatListener`.
- Files transferred in 64 KB chunks with SHA-256 chunk verification.

### Step 9: Reconnection, Screen Curtain & Session Teardown
- Automatic reconnect thread `P2PTryReconnect` attempts 5 reconnect retries over 30s on packet drop.
- Optional Privacy Screen Curtain (`FullScreenLocker_Lock`) blanks out target monitor during maintenance.
- Clean session teardown via `FINISH_SUPPORT_SESSION` with optional auto-lock (`LockWorkStation`).
