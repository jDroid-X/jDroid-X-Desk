# jDroid-X-Desk: 15-Stage Human-in-the-Loop (HITL) Master Specification & SOP
**Document Reference:** `docs/JDROID_X_15_STAGES_MASTER_HITL_SOP.md`  
**Target Architecture:** `jDroid-X-Desk` (`c:\Users\jiten\jAnitGravity\jDroid-X-Desk`)  
**Standard:** 20-Year Senior Enterprise Architect & Dynamic OOPS-based MVC (P0–P5)  
**Operator Circuit:** JEET & jDroid-X  
**Timestamp:** 2026-11-17T14:35:00+05:30  

---

## 1. Executive Directive & Architectural Purpose

In enterprise remote desktop solutions (AnyDesk, UltraViewer, TeamViewer), a product stage is not a trivial single-action checklist item. Every stage represents a complex operational subsystem requiring:
1. **Chronological Ordered Steps:** Predictable execution order from initialization to completion.
2. **Human-in-the-Loop (HITL) Perspectives:** Distinct roles for Technician (Controller), Host User (Interactive Partner), and Unattended System.
3. **Pre-Conditions & Runtime Conditions:** Environment, network, and security constraints.
4. **Granular Validations:** Exact mathematical, cryptographic, OS permission, and network checks.
5. **Notifications, Alerts & Modals:** Visible, auditory, and status feedback to all participants.
6. **Standard Operating Procedures (SOP):** Predictable recovery loops, zero false greens, and failovers.
7. **Zero-Duplication Codebase Discipline:** Clean separation of concerns via dynamic OOPS MVC call-by-reference.

---

## 2. The 15-Stage Master HITL Lifecycle Matrix

```
[STAGE 01: Hardware Identity & Keystore]
       ↓
[STAGE 02: Signaling Presence & Cloudflare Tunnel Ingress]
       ↓
[STAGE 03: Oxaam Hero UI, VIBGYOR & Dynamic Theme Engine]
       ↓
[STAGE 04: Session Proposal, Anti-Mirror Guard & Target Validation]
       ↓
[STAGE 05: Multi-Bridge Transport Negotiation & STUN/ICE Hole Punching]
       ↓
[STAGE 06: Ephemeral Cryptographic Handshake & Mutual Authentication]
       ↓
[STAGE 07: Host Authorization Gate & 5-Toggle Granular Permission Consent]
       ↓
[STAGE 08: DXGI VRAM Zero-Copy Screen Capture & 32x32 Tile Differential Codec]
       ↓
[STAGE 09: Low-Latency Remote Input Injection & Local Operator Priority]
       ↓
[STAGE 10: Windows Session 0 Privilege Elevation & UAC Traversal]
       ↓
[STAGE 11: In-Session Auxiliary Channels (Clipboard, Audio & File Transfer)]
       ↓
[STAGE 12: Privacy Screen Curtain & Session Power Management]
       ↓
[STAGE 13: 2-Step Viewport, Obsidian Canvas & In-Session Ribbon HUD]
       ↓
[STAGE 14: Network Glitch Recovery, Auto-Reconnect & Session Continuity]
       ↓
[STAGE 15: Graceful Session Teardown, Cleanup & FIFO Regulatory Audit]
```

---

## 3. Detailed Stage-Wise Breakdown (Steps, Conditions, Validations, HITL & SOP)

---

### STAGE 01: Hardware Identity, Cryptographic Keystore & Subnet Discovery
- **Scenario:** First boot or regular launch of the native executable or browser gateway.
- **HITL Perspectives:**
  - *Technician / Host:* Observes application startup, views 9-digit Desk ID, can copy or customize alias.
- **Ordered Chronological Steps:**
  1. Check single-instance mutex (`JDroidXDesk_SingleInstance_Mutex`). If already running, signal existing instance to restore window and exit.
  2. Probe Motherboard UUID, Primary NIC MAC address, and CPU hash via WMI / Win32.
  3. Generate or load the deterministic 9-digit Desk ID formatted as `XXX-XXX-XXX`.
  4. Generate or load RSA-4096 / Ed25519 device keypair and self-signed X.509 certificate in Windows Certificate Store.
  5. Generate fresh 4-digit / 6-digit ephemeral PIN for interactive access.
  6. Check registry/database for configured Unattended Access password hash.
  7. Bind UDP Multicast/Broadcast socket on `239.255.102.18:8089` to announce presence to local LAN peers.
- **Conditions:**
  - OS has crypto provider available (Microsoft Enhanced Cryptographic Provider).
  - Port 8089 available or rebinds dynamically with SO_REUSEADDR.
- **Validations:**
  - Desk ID conforms to regex `^[0-9]{3}-[0-9]{3}-[0-9]{3}$`.
  - Cryptographic private key has strict user ACLs (prevent unauthorized reads).
- **Notifications & Options:**
  - UI displays: "Your Desk: 984-102-391".
  - One-click "Copy ID" button with visual confirmation badge.
  - Option: "Set Unattended Password", "Toggle LAN Discovery".

---

### STAGE 02: Central Signaling Presence & Cloudflare Tunnel Ingress
- **Scenario:** Workstation announces availability to WAN signaling hub and establishes public edge tunnel.
- **HITL Perspectives:**
  - *User:* Sees connection status dot change from Red (Disconnected) to Yellow (Registering) to Green (Online).
- **Ordered Chronological Steps:**
  1. Read signaling hub endpoints from `database.json` (`settings.connection.relayUrl`).
  2. Dispatch initial HTTP POST to `/api/signaling/heartbeat` with Desk ID, Machine Name, OS, and public IP candidate.
  3. Supervise `cloudflared.exe` tunnel process for zero-open-port public ingress via `desk.jdroid-x.com`.
  4. Mount gateway endpoints on local port 3000 / 7700.
  5. Start 15-second timer loop emitting continuous heartbeats.
  6. Listen for incoming signaling push notifications (SSE or WebSocket).
- **Conditions:**
  - Outbound HTTPS (port 443) or WebSocket (WSS) permitted by corporate firewall.
  - Signaling server responds within 5000ms.
- **Validations:**
  - Signaling heartbeat TTL must be maintained `< 45 seconds`.
  - Server response status HTTP 200 with valid session token.
- **Notifications & Options:**
  - Top status dot: Green dot + "Connected to Signaling Hub (desk.jdroid-x.com)".
  - Option to specify custom private relay URL in Settings Studio.

---

### STAGE 03: Oxaam-Style Hero UI, VIBGYOR Palette & Dynamic Theme Engine
- **Scenario:** User interacts with Screen 1 (Hero Connection Hub).
- **HITL Perspectives:**
  - *User:* Selects themes, observes feature pointers, enters connection credentials, scrolls bottom marquee.
- **Ordered Chronological Steps:**
  1. Initialize `#hero-portal-card` with luxury 2px Solid Gold Border (`#FFD700`) and 24px rounded corners.
  2. Bind VIBGYOR top bar swatches: Violet, Indigo, Blue, Green, Yellow, Orange, Red.
  3. Attach hover and click listeners: dynamically modify CSS root variables and highlight active swatch with gold ring.
  4. Render 5 Feature Pointer Badges: 1-Click 60 FPS, 100% P2P Encrypted, UAC Admin Bypass, Zero-Install Mesh, 24x7 Instant Support.
  5. Initialize continuous smooth marquee ticker (`.portal-marquee-bar`) showing support contact, copyright, and mesh health.
  6. Enforce Light-First UI typography with high contrast readability.
- **Conditions:**
  - Responsive layout scales seamlessly from 1024×768 to 4K displays.
  - Zero DOM layout shift during font or theme loading.
- **Validations:**
  - Zero duplicate element IDs in DOM.
  - Active theme selection persisted in `localStorage` and `database.json`.
- **Notifications & Options:**
  - Interactive theme switcher dropdown with 15 paradigm styles.
  - Live preview of theme changes with instant CSS variable re-binding.

---

### STAGE 04: Session Proposal, Anti-Mirror Loopback Guard & Target Validation
- **Scenario:** Technician enters Partner ID and clicks "Connect".
- **HITL Perspectives:**
  - *Technician:* Enters target Desk ID, clicks Connect, sees progress spinner and status messages.
  - *Host User:* Receives prominent incoming session modal with Technician ID and countdown.
- **Ordered Chronological Steps:**
  1. User inputs partner ID in `#target-id-input` and optional password in `#target-pass-input`.
  2. Controller normalizes input (removes dashes, spaces, converts to 9-digit string).
  3. Anti-Mirror Loopback Guard: Verify `TargetID != LocalID`. If equal, block immediately and show modal.
  4. Query Signaling Directory (`GET /api/signaling/lookup?targetId=...`).
  5. If target is offline (Heartbeat > 45s), trigger "Remote Desk Offline" notification with Wake-on-LAN option.
  6. If target is online, construct `SESSION_PROPOSAL` packet with initiator ID, hostname, and client version.
  7. Post proposal to `/api/signaling/propose` and await host response.
- **Conditions:**
  - Valid 9-digit format entered.
  - Target Desk is currently registered in signaling directory.
- **Validations:**
  - Strict regex validation `^[0-9]{9}$`.
  - Target presence TTL verification.
- **Notifications & Options:**
  - Technician: "Connecting to Remote Workstation... Awaiting Host Authorization".
  - Host: Audio chime + Modal Dialog: "Technician [ID: XXX-XXX-XXX] requests remote control".

---

### STAGE 05: Multi-Bridge Transport Negotiation & STUN / ICE Hole Punching
- **Scenario:** System discovers and binds the fastest available network path between peers.
- **HITL Perspectives:**
  - *Technician & Host:* Automated background negotiation; UI displays selected transport path and latency badge.
- **Ordered Chronological Steps:**
  1. Check Local Subnet Cache: If host IP is on same subnet, attempt direct LAN UDP socket.
  2. STUN Profiling: Query STUN servers (RFC 5389) to determine NAT mapping behavior (Cone vs Symmetric).
  3. If Cone NAT: Perform simultaneous UDP hole punching for direct peer-to-peer data channel.
  4. If Symmetric NAT: Fall back to Google Colab GPU-Accelerated WebSocket Relay or Cloudflare Edge Tunnel.
  5. Measure Round-Trip Time (RTT) across all viable candidates.
  6. Elect the fastest valid candidate (Direct P2P preferred over Relay).
  7. Bind data transport channel and report transport type to UI.
- **Conditions:**
  - Direct UDP socket requires non-symmetric NAT or port forwarding.
  - Fallback relay requires HTTPS/WSS egress.
- **Validations:**
  - Candidate round-trip latency verified `< 1500ms`.
  - Packet integrity verification on test ping.
- **Notifications & Options:**
  - UI Badge: "⚡ Direct P2P UDP (12ms RTT)" or "🌐 Encrypted Cloud Relay (38ms RTT)".
  - Option to force specific transport mode in Settings.

---

### STAGE 06: Ephemeral Cryptographic Handshake & Mutual Authentication
- **Scenario:** Establishing end-to-end encryption before transmitting desktop media or input commands.
- **HITL Perspectives:**
  - *Technician & Host:* Observe green security lock badge with SHA-256 fingerprint; can cross-check fingerprints.
- **Ordered Chronological Steps:**
  1. Perform ephemeral ECDHE key exchange over Curve25519.
  2. Compute shared secret using Elliptic Curve Diffie-Hellman.
  3. Exchange and verify Ed25519 public keys / X.509 device certificates.
  4. Derive independent 256-bit AES-GCM session keys for video and input streams.
  5. Initialize 96-bit initialization vectors (IVs) with monotonic packet counters.
  6. Generate SHA-256 session fingerprint and render on security badge.
- **Conditions:**
  - Mutual certificate validation passes; zero untrusted intermediate proxies.
- **Validations:**
  - AES-GCM 128-bit authentication tag verified on every packet.
  - Mismatch immediately triggers socket abort and security audit alert.
- **Notifications & Options:**
  - Security Badge: "🔒 256-bit End-to-End Encrypted | Fingerprint: 8F:3A:...:B2".
  - Instant Disconnect button if fingerprint mismatch is flagged.

---

### STAGE 07: Host Authorization Gate & 5-Toggle Granular Permission Consent
- **Scenario:** Host decides whether to accept connection and what permissions to grant.
- **HITL Perspectives (Core HITL):**
  - *Host User:* Reviews request, adjusts 5 permission checkboxes, clicks Accept or Reject.
  - *Technician (Unattended):* Enters fixed unattended password; verified by Argon2id hasher.
- **Ordered Chronological Steps:**
  1. Host displays incoming authorization modal with 60-second countdown timer.
  2. Host configures the 5 Permission Toggles:
     - `[✔] Allow Mouse and Keyboard Control`
     - `[✔] Allow Bi-directional Clipboard Sync`
     - `[✔] Allow Audio Transmission`
     - `[✔] Allow File Transfer`
     - `[ ] Lock Local Physical Keyboard & Mouse`
  3. If unattended access: Technician submits password; host validates against Argon2id salted hash.
  4. If 3 consecutive incorrect passwords: Host triggers 15-minute brute-force lockout.
  5. If accepted: Host transmits `CONSENT_ACCEPTED` packet with permission bitmask.
  6. Both sides transition session state from `PROPOSING` to `CONNECTED`.
- **Conditions:**
  - Host user clicks "Accept" before 60s timeout, or valid unattended password provided.
- **Validations:**
  - Non-empty permission bitmask.
  - Brute force counter enforced strictly.
- **Notifications & Options:**
  - Host can revoke any permission dynamically during active session from floating HUD.
  - Technician sees granted permissions in session ribbon.

---

### STAGE 08: DXGI VRAM Zero-Copy Screen Capture & 32x32 Tile Differential Codec
- **Scenario:** Streaming host desktop at 60 FPS with minimal bandwidth and CPU consumption.
- **HITL Perspectives:**
  - *Technician:* Observes smooth remote desktop with zero input lag.
  - *Host:* Floating tray widget indicates active screen capture.
- **Ordered Chronological Steps:**
  1. Initialize DXGI 1.2 Desktop Duplication API to capture desktop directly from GPU VRAM. Fallback to GDI `BitBlt` if DXGI unavailable.
  2. Partition display surface into 32×32 pixel tile grid.
  3. Compute xxHash64 / MurmurHash for each tile against previously acknowledged frame buffer.
  4. Classify dirty tiles: Flat/Text -> Lossless RLE; Photographic/Motion -> TurboJPEG SIMD / DCT quantization.
  5. Pack modified bounding boxes `[x, y, w, h, data]` into differential packet with monotonic sequence ID.
  6. Transmit encrypted packet over data channel. Skip unmodified tiles (0 bytes for static areas).
- **Conditions:**
  - Handles dynamic display resolution changes, DPI changes, and multi-monitor switching.
- **Validations:**
  - Monotonic sequence numbers. If sequence gap > 3, client triggers full IDR keyframe refresh.
- **Notifications & Options:**
  - Real-time FPS and latency counter in bottom status bar (e.g. "60 FPS | 14 ms").
  - Quality selector in ribbon: Auto / High Speed / High Quality.

---

### STAGE 09: Low-Latency Remote Input Injection & Local Hardware Priority
- **Scenario:** Technician controls host mouse and keyboard; host user retains ultimate control.
- **HITL Perspectives:**
  - *Technician:* Controls mouse/keyboard as if sitting in front of the machine.
  - *Host User:* Can override technician at any time by simply moving the physical mouse.
- **Ordered Chronological Steps:**
  1. Viewer captures mouse coordinates, clicks, wheel deltas, and keyboard virtual scancodes.
  2. Normalize coordinates to host display coordinate space `[0.0 - 1.0]`.
  3. Transmit input packet with high priority (< 5ms latency).
  4. Host validates that `Allow Mouse & Keyboard` permission toggle is active.
  5. Local Operator Priority Check: If host physical mouse/key event detected in last 500ms, drop remote synthetic input (`SKIP_BECAUSE_LOCAL_USER_IS_USING`).
  6. If permitted, inject input using Win32 `SendInput` API.
- **Conditions:**
  - Host workstation interactive; UAC state elevated or handled by Session 0 service.
- **Validations:**
  - Clamped coordinates to prevent off-screen injection.
  - Release all synthetic modifier keys (Ctrl, Alt, Shift, Win) on window blur.
- **Notifications & Options:**
  - "Remote Input Active" indicator.
  - Host emergency pause button to temporarily freeze remote control.

---

### STAGE 10: Windows Session 0 Privilege Elevation & UAC / Secure Desktop Traversal
- **Scenario:** Remote software installation triggers Windows UAC prompt (`Consent.exe`) or Windows Lock Screen.
- **HITL Perspectives:**
  - *Technician:* Screen does NOT freeze; technician can view and click "Yes" on UAC prompt.
  - *Host User:* Sees administrative elevation indicator.
- **Ordered Chronological Steps:**
  1. Monitor active desktop session via `wtsapi32.dll` (`WTS_SESSION_LOCK`, `WTS_SESSION_LOGON`).
  2. Windows Service running in Session 0 (`LocalSystem` with `uiAccess=true`) detects Secure Desktop activation (`Consent.exe`).
  3. Service calls `WTSQueryUserToken` to duplicate the token of the interactive user session.
  4. Invoke `SetThreadDesktop` or spawn 64-bit helper `uv_x64.exe` inside the active console session via `tscon.exe`.
  5. Continue DXGI / GDI screen capture and `SendInput` injection on the elevated secure desktop.
  6. Seamlessly transition back to standard desktop when UAC dialog is dismissed.
- **Conditions:**
  - Windows Service installed and running as `LocalSystem`.
- **Validations:**
  - High-integrity token verification.
  - UIPI (User Interface Privilege Isolation) bypass verified.
- **Notifications & Options:**
  - Technician: "Administrative Context Active (UAC Traversal Enabled)".
  - Option to trigger administrative reboot and reconnect.

---

### STAGE 11: In-Session Auxiliary Channels (Clipboard, Audio & File Transfer)
- **Scenario:** Technician exchanges clipboard text, hears remote system sounds, or transfers files.
- **HITL Perspectives:**
  - *Technician & Host:* Seamless copy/paste; dual-pane file manager; real-time in-session chat.
- **Ordered Chronological Steps:**
  1. Clipboard Sync: Register Win32 `AddClipboardFormatListener`. On clipboard change, check permission toggle, encrypt delta, and update peer clipboard.
  2. Audio Loopback: Capture system audio via WASAPI Loopback (`NAudio.dll`). Encode to Opus 48kHz audio chunks. Transmit synchronized with video timestamps.
  3. File Transfer: Open 2-pane graphical file manager (Local <-> Remote). Slice files into 64 KB chunks, compute SHA-256 checksum per chunk, verify and write to destination.
  4. In-Session Chat: Floating chat drawer with instant text delivery and audio notification.
- **Conditions:**
  - Respective permission toggles enabled by host.
- **Validations:**
  - File SHA-256 checksum verified; partial artifacts cleaned up on cancellation.
  - Clipboard size capped at 10 MB to prevent memory overflow.
- **Notifications & Options:**
  - "File Transfer Complete: setup.exe (24.5 MB)".
  - Audio mute/unmute toggle in ribbon.

---

### STAGE 12: Privacy Screen Curtain & Session Power Management
- **Scenario:** Confidential remote administration or waking a remote workstation.
- **HITL Perspectives:**
  - *Technician:* Activates privacy curtain; continues viewing clear desktop.
  - *Host Workstation:* Physical monitor displays a blackout curtain screen; local onlookers cannot see activity.
- **Ordered Chronological Steps:**
  1. Technician toggles "Privacy Screen Curtain" in session ribbon.
  2. Host executes `FullScreenLocker_Lock`: hooks graphics output to render black overlay on physical monitors while desktop capture engine continues unaffected.
  3. Suppress local keyboard and mouse inputs on host machine.
  4. Call Win32 `SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED)` to inhibit OS sleep.
  5. Wake-on-LAN: Technician can dispatch UDP Magic Packet on Port 9 to wake sleeping machines on subnet.
- **Conditions:**
  - Host graphics driver supports display curtain hooking.
- **Validations:**
  - Screen curtain automatically releases upon session disconnect or process termination.
  - Power sleep state restored on session exit.
- **Notifications & Options:**
  - Host screen: "Remote maintenance in progress by authorized administrator".
  - Technician toggle: "Enable/Disable Privacy Curtain".

---

### STAGE 13: 2-Step Viewport, Obsidian Canvas & In-Session Ribbon HUD
- **Scenario:** Technician manages the live remote desktop session viewport.
- **HITL Perspectives:**
  - *Technician:* Interacts with full-screen remote desktop, browser tabs, and 14-tool action ribbon.
- **Ordered Chronological Steps:**
  1. Hide Screen 1 (Hero Connect Portal) completely (`display: none !important;`).
  2. Display Screen 2 (Remote Desktop Workspace) at supreme `z-index: 99999 !important;`.
  3. Render Header 2 Tab Bar: `[ ⚡ jDroid-X | 💻 984-102-391 ✕ ]`, add tab button, and fullscreen toggle.
  4. Render Header 3 Action Ribbon with 14 tools: Monitor Switcher, Ctrl+Alt+Del injection, File Manager, Chat, Clipboard, Audio, Fullscreen, Disconnect.
  5. Slide In / Out Left Sidebar Drawer (`#remote-sidebar-drawer`) with smooth 0.28s GPU transform.
  6. Display bottom status bar with live telemetry (Desk ID, 60 FPS, RTT, GPU status).
- **Conditions:**
  - Zero overlap from legacy navigation sidebars or background mockups.
- **Validations:**
  - Canvas scales correctly with window resize (Fit Screen vs 1:1 Pixel).
- **Notifications & Options:**
  - Tooltips for all ribbon actions.
  - Tab close confirmation dialog.

---

### STAGE 14: Network Glitch Recovery, Auto-Reconnect & Session Continuity
- **Scenario:** Sudden network interruption, Wi-Fi reconnection, or IP hop during an active session.
- **HITL Perspectives:**
  - *Technician & Host:* Interface displays non-intrusive "Reconnecting..." overlay; session automatically resumes without restart.
- **Ordered Chronological Steps:**
  1. Heartbeat monitor detects transport loss (`RTT > 3000ms` or TCP socket drop).
  2. Freeze viewport on last valid frame; display semi-transparent overlay: "Connection Interrupted. Reconnecting... (Attempt 1/5)".
  3. Retain active Session ID, encryption state, and sequence numbers in memory.
  4. Execute Multi-Bridge Failover: Attempt Local LAN UDP ➔ Colab Relay ➔ Cloudflare Tunnel.
  5. On socket reconnect: Transmit `SESSION_RESUME` token with last acknowledged frame sequence ID.
  6. Host verifies token, sends fresh IDR keyframe, and resumes differential tile streaming.
  7. If 5 attempts over 30 seconds fail: Gracefully teardown session and return to Screen 1.
- **Conditions:**
  - Network connectivity re-established within 30-second window.
- **Validations:**
  - Resume token cryptographic signature validated.
  - Monotonic frame sequence counters resynchronized.
- **Notifications & Options:**
  - "Connection Restored! Session Resumed", or "Session Expired: Remote Host Unreachable".
  - Manual "Retry Now" and "Cancel" buttons on reconnect overlay.

---

### STAGE 15: Graceful Session Teardown, Cleanup & FIFO Regulatory Audit
- **Scenario:** Either technician or host terminates the session.
- **HITL Perspectives:**
  - *Technician & Host:* Receive clean session summary; interface returns smoothly to Screen 1.
- **Ordered Chronological Steps:**
  1. User clicks Disconnect or closes session tab.
  2. Transmit `SESSION_TERMINATE` packet with reason code.
  3. Release all synthetic modifier keys (Ctrl, Alt, Shift, Win) to prevent stuck keys on host OS.
  4. Unlock physical input and remove Privacy Screen Curtain on host machine.
  5. Invalidate and purge ephemeral AES-256 session keys from memory.
  6. Close DXGI / GDI screen capture pipelines and dispose graphics handles.
  7. Commit session log (Session ID, Start/End Time, Duration, Bytes Transferred, Avg RTT) to `database.json`.
  8. Apply FIFO Retention: Prune logs exceeding 500 records or older than 7 days; manage maximum 3 regulatory snapshots in `backups/database/`.
  9. Restore Screen 1 (Hero Connect Portal) with all controls ready for next connection.
- **Conditions:**
  - Clean disposal of all native handles and threads; zero zombie processes.
- **Validations:**
  - Memory leak check: Memory returned to baseline.
  - Zero orphan temporary files in workspace.
- **Notifications & Options:**
  - Session Summary Dialog: "Session Duration: 18m 42s | Data: 142 MB | Avg FPS: 59.8".
  - Ready for immediate new connection.

---

## 4. Code Redundancy Elimination & Canonical Single Source of Truth (SSOT)

To eliminate code duplication without breaking existing tests or relative imports:

```
[CURRENT REDUNDANT STATE]                      [CANONICAL OOPS MVC STATE]
app.js (2,728 lines)              \           
controllers/app-controller.js (2,728) ----->  controllers/app-controller.js (Canonical SSOT)
                                               app.js (Lean Entrypoint / Call-by-Reference)

index.html (831 lines)            \           
views/index.html (831 lines)      ----->      views/index.html (Canonical SSOT)
                                               index.html (Lean Entrypoint / Template Engine)

styles.css (7,062 lines)          \           
styles/styles.css (5,742 lines)   ----->      styles/styles.css (Canonical SSOT)
                                               styles.css (Forwarding Import)

database.json                     \           
models/database.json              ----->      models/database.json (Canonical SSOT)
                                               database.json (Symlink / Mirrored State)
```

### Safety Rules for Redundancy Elimination:
1. **Zero Broken Dependencies:** Any existing tool or script calling `app.js` or `index.html` must continue to function via direct re-export or forwarding import.
2. **Synchronized Test Suite:** All 242 automated test assertions in `test.js` must verify the canonical files and their root wrappers.
3. **No Unpruned Backups:** All backups reside exclusively in `backups/database/` with a rolling limit of 3 files.
