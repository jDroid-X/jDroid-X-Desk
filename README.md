<div align="center">

<img src="assets/jdroidxlogo_trans.png" alt="jDroid-X-Desk Logo" width="140" />

# jDroid-X-Desk
### Enterprise Remote Desktop & Machine Administration Workspace
#### Senior Dynamic OOPS MVC Architecture | AnyDesk Parity Master Standard | Cloudflare Edge Ingress

[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%207%20%7C%208%20%7C%2010%20%7C%2011%20%7C%20Server-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk)
[![Runtime: .NET 4.0 - 4.8](https://img.shields.io/badge/.NET-Modular%20Native%20C%23%20(Any%20CPU)-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk)
[![Edge Ingress: desk.jdroid-x.com](https://img.shields.io/badge/Edge%20Ingress-desk.jdroid--x.com-F38020?style=for-the-badge&logo=cloudflare&logoColor=white)](https://desk.jdroid-x.com)
[![Web Gateway: Express + WebRTC](https://img.shields.io/badge/Web%20Gateway-Port%207700%20%2F%2012345-10B981?style=for-the-badge&logo=nodedotjs&logoColor=white)](http://localhost:7700)
[![Security: AES-256 GCM + Argon2id](https://img.shields.io/badge/Security-AES--256%20GCM%20%2B%20Certificates-10B981?style=for-the-badge&logo=shield&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk)
[![Verification: 223/223 Passed](https://img.shields.io/badge/Verification-223%2F223%20Tests%20Passed%20(100%25)-10B981?style=for-the-badge)](https://github.com/jDroid-X/jDroid-X-Desk)

</div>

<div align="center">

<br/>

<img src="assets/jdroid-x-desk-presentation.png" alt="jDroid-X-Desk Workspace Presentation" width="95%" style="border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.5);" />

<br/>
<br/>

*Fig 1: jDroid-X-Desk 2-Step Workspace — Screen 1 (Connection Hub & Fleet Book) and Screen 2 (Active Viewport, Floating HUD, and 5-Permission Host Consent Modal).*

<br/>

[![Download Authorized EXE](https://img.shields.io/badge/Download-jDroid--X--Desk.exe%20(Authorized%20Build)-EF4444?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk.exe)
[![Download Universal ZIP](https://img.shields.io/badge/Download-Universal%20ZIP%20Package-3B82F6?style=for-the-badge&logo=zip&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk-Universal.zip)
[![Official Edge Gateway](https://img.shields.io/badge/Cloudflare%20Gateway-desk.jdroid--x.com-F38020?style=for-the-badge&logo=google-chrome&logoColor=white)](https://desk.jdroid-x.com)

</div>

---

## 📥 Direct Download & Universal Deployment

**jDroid-X-Desk** is an enterprise remote desktop solution architected to the **AnyDesk Master Parity Baseline Requirement (BRD)**. Built and verified as a **single universal Windows executable**, it runs on all Windows versions (Windows 7, 8, 8.1, 10, 11, and Windows Server) on AnyCPU (x86, x64, ARM64) with zero external runtime dependencies:

| Artifact | Type | Size | Direct Download Link | Specification & Authority |
|---|---|---|---|---|
| **jDroid-X-Desk.exe** | **Single Universal Windows Executable** | **387 KB** | [📥 **Download jDroid-X-Desk.exe**](https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk.exe) | **Single Canonical Executable**: Modular C# Windows Forms architecture (`Native/`), embedded manifest (DPI-aware, Win 7–11), multi-target .NET 4.0–4.8.1 runtime fallback. Direct execution from USB or network shares with zero installer required. |
| **jDroid-X-Desk-Universal.zip** | **Portable Distribution Archive** | **1.3 MB** | [📥 **Download Universal ZIP**](https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk-Universal.zip) | Standalone distribution package containing the unified executable, manifest, configs, scripts, certificates, and unblock launchers. |
| **Install-Or-Run.bat** | **1-Click SmartScreen Unblock Launcher** | **1.2 KB** | [📥 **Download Install-Or-Run.bat**](https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main/Install-Or-Run.bat) | 1-click batch launcher that removes Windows SmartScreen Mark-of-the-Web (MOTW) attribute and boots the native executable. |
| **TRUST_ENTERPRISE_CERT_AS_ADMIN.bat** | **Kernel Trust & Root Certificate Installer** | **1.8 KB** | [📥 **Download Certificate Installer**](https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main/TRUST_ENTERPRISE_CERT_AS_ADMIN.bat) | Administrator batch script that trusts the enterprise Authenticode root certificate into `Cert:\LocalMachine\Root` for Windows 11 Smart App Control (SAC) compliance. |
| **Installation Guide** | **Markdown Guide** | **3.2 KB** | [📖 **INSTALL_GUIDE_WINDOWS.md**](INSTALL_GUIDE_WINDOWS.md) | Comprehensive Windows setup, UAC elevation, firewall whitelist, and multi-tenant setup instructions. |
| **Parity Master BRD** | **Architectural Specification** | **28 KB** | [📖 **JDROID_X_BRD_ANYDESK_PARITY_MASTER.md**](docs/JDROID_X_BRD_ANYDESK_PARITY_MASTER.md) | Official 8-Phase AnyDesk Parity Lifecycle, state transitions, validation logic, and Mermaid flowcharts. |
| **Cloudflare Edge Gateway** | **Live Ingress Endpoint** | — | [🌐 **https://desk.jdroid-x.com**](https://desk.jdroid-x.com) | Central signaling directory, public presence heartbeat, and secure WebRTC tunnel broker. |

---

## 🏛️ AnyDesk Parity Master Architecture (8-Phase Lifecycle)

jDroid-X-Desk implements the complete 8-phase operational lifecycle defined in the **[AnyDesk Parity Master BRD](docs/JDROID_X_BRD_ANYDESK_PARITY_MASTER.md)**:

```
[Phase 1: Client Startup] ➔ Deterministic SHA-256 Hardware ID & Local Listener
         ↓
[Phase 2: Target Discovery] ➔ Central Signaling Server (Cloudflare Ingress Heartbeats)
         ↓
[Phase 3: NAT Traversal] ➔ P2P UDP STUN/ICE Hole-Punching (Cloud Relay Mesh Fallback)
         ↓
[Phase 4: Cryptographic Handshake] ➔ TLS 1.3 / AES-256-GCM + X.509 Trust Store Fingerprints
         ↓
[Phase 5: Authorization & Consent] ➔ 5-Checkbox Host Permission Modal & 45s Polling Timeout
         ↓
[Phase 6: Media Pipeline & DeskRT] ➔ DXGI Desktop Duplication + Tile Codec (<16ms, 60 FPS)
         ↓
[Phase 7: Active Controls & Lease] ➔ Low-Latency RTCDataChannel Input + UAC Elevation
         ↓
[Phase 8: Teardown & Convergence] ➔ Relayed Buffer Cleansing, Key Release & Clean Disconnect
```

---

## 🎯 Simplified 2-Step Interface Standard

The user experience has been streamlined into an intuitive **2-Step Interface**:

### Step 1: Connection Hub & Address Book (Screen 1)
- **This Desk Card**: Displays the persistent, hardware-grounded 9-digit Desk ID (`XXX-XXX-XXX`) with 1-click clipboard copy (equipped with automatic fallback for non-HTTPS environments).
- **Remote Desk Card**: Formatted connection input with dynamic digit masking (`___-___-___`) and an anti-mirror recursion guard preventing accidental self-connections.
- **Recent Sessions & Fleet Book**: Relational workstation cards showing status, ping, and 1-click reconnect.
- **Edge Presence Pill**: Live indicator verifying synchronization with `https://desk.jdroid-x.com`.

### Step 2: Remote Viewport & Host Consent Modal (Screen 2)
- **Instant Smooth Focus**: Auto-scrolls into the remote desktop canvas with full aspect-ratio preservation.
- **Host 5-Permission Consent Modal**: Authoritative host control with granular permission toggles:
  1. `[✔] Allow Mouse and Keyboard Control`
  2. `[✔] Allow Bi-directional Clipboard Sync`
  3. `[✔] Allow Audio Transmission`
  4. `[✔] Allow File Transfer`
  5. `[ ] Lock Local Physical Keyboard & Mouse`
- **Floating Top HUD**: Real-time display of FPS, latency (ms), bandwidth, cipher suite, and the 14 AnyDesk-parity quick actions (Ctrl+Alt+Del, UAC Elevation, Monitor Switch, In-Session Chat, Whiteboard, Session Recording).

---

## ⚡ Core Engineering Highlights

### 1. Dual Modular Architecture
- **Native C# Solution (`Native/`)**:
  - Organized strictly by OOPS MVC hierarchy:
    - `1_Core/`: Architecture constants, state machines, and configuration contracts.
    - `2_Services/`: DXGI Desktop Duplication capture service, TileCodec differential rendering, and installation managers.
    - `3_Network/`: High-performance P2P UDP engine, embedded relay server, and socket listeners.
    - `4_UI_Controls/`: Native dialogs, consent popups, and HUD widgets.
    - `5_Forms/`: `MainForm.cs` (Enterprise Suite) and `MiniDeskForm.cs` (2-Step Parity Workspace).
  - Single-command compilation via `compile_native.bat` using standard Windows built-in `csc.exe`.
- **Full-Stack Web Gateway (`server.js` + `controllers/` + `models/`)**:
  - Node.js Express server running on port `7700` (fallback `12345`).
  - Wire-speed WebRTC P2P streaming with low-latency `RTCDataChannel` remote input (<5ms latency).
  - Automatic differential frame caching (`HTTP 304 Not Modified`) minimizing bandwidth consumption.

### 2. Central Signaling Server & Cloudflare Edge Tunnel
- **Domain**: `https://desk.jdroid-x.com`
- **Port Ingress**: Port `7700`
- **Heartbeat & Discovery**:
  - Nodes dispatch automated heartbeats every 15 seconds to `/api/signaling/heartbeat`.
  - Directory daemon conducts 45-second lease sweeps to automatically prune offline peers.
  - Digit-normalized discovery via `/api/signaling/lookup/:deskId` resolves partner IPs, UDP ports, and presence instantly.

### 3. Database & Relational Schema (19 Collections)
The data layer in [`models/db-context.js`](models/db-context.js) maintains complete AnyDesk parity across 19 relational entities:
- `clients`: Hardware client identities, OS versions, and fingerprints.
- `certificates`: Cryptographic trust store and X.509 fingerprints.
- `permission_profiles`: Master access presets (`Full Control`, `View Only`).
- `sessions_active`: Real-time active connection tracking.
- `sessions_history`: Historical audit trail with connection timestamps, duration, and telemetry.
- `file_transfers`: In-flight and committed chunked file exchange ledger.
- `chat_messages`: In-session communication logs per session ID.
- `invitation_tokens`, `invites`, `sessions`, `devices`, `groups`, `safeZones`, `guardians`, `contacts`, `notifications`, `settings`, `logs`, `engines`.

---

## 🗂️ File Structure & Repository Map

```
jDroid-X-Desk/
├── Native/                             # Modular C# Native Desktop Client
│   ├── 1_Core/                         # Constants, Interfaces, & State Machines
│   ├── 2_Services/                     # DxgiCaptureService, TileCodecService, InstallationManager
│   ├── 3_Network/                      # P2PUdpEngine, EmbeddedRelayServer, Socket Bridges
│   ├── 4_UI_Controls/                  # IncomingSessionDialog, Top HUD, Floating Widgets
│   └── 5_Forms/                        # MainForm, MiniDeskForm (2-Step Standard)
├── controllers/                        # Senior OOPS MVC Controllers
│   ├── app-controller.js               # Reactive Frontend Client Controller
│   └── server-controller.js            # Express API, Signaling Hub & Relay Server
├── models/                             # Relational Data Layer
│   ├── db-context.js                   # 19 Relational Collections, Schema Validation & Seed
│   └── database.json                   # Disk Persistence Store
├── views/                              # Presentation Layer
│   └── index.html                      # 2-Step Interface & System Studio View
├── docs/                               # Specifications & Blueprints
│   ├── JDROID_X_BRD_ANYDESK_PARITY_MASTER.md # Master 8-Phase Parity Blueprint
│   ├── JDROID_X_BRD_BASELINE.md        # Architectural Baseline Specification
│   └── ANYDESK_RECONSTRUCTION_DOSSIER.md    # Reverse Engineering Analysis
├── requirement/                        # Requirements & Telemetry Change Logs
│   ├── Task_35.md                      # Task 35 Parity Completion Ledger
│   └── unified-screen-contract.txt     # Screen-Action-Affordance Contract
├── compile_native.bat                  # 1-Click C# Compiler (csc.exe -> jDroid-X-Desk.exe)
├── TRUST_ENTERPRISE_CERT_AS_ADMIN.bat  # 1-Click Root Certificate Trust Script
├── server.js                           # Express Web Gateway Entrypoint
├── app.js                              # Canonical Frontend Mirror
├── index.html                          # Root Presentation Gateway
├── test.js                             # Comprehensive 223-Test Verification Suite
├── jDroid-X-Desk.exe                   # Single Universal Windows Executable (387 KB)
└── jDroid-X-Desk-Universal.zip         # Portable Distribution Archive (1.3 MB)
```

---

## ⚡ Quick Start Guide

### 1. Run Standalone Native App (Zero Installation)
Double-click [`jDroid-X-Desk.exe`](jDroid-X-Desk.exe) or run via terminal:
```cmd
jDroid-X-Desk.exe
```
*(To unblock SmartScreen, run `Install-Or-Run.bat`. To trust certificates for Windows 11 SAC, run `TRUST_ENTERPRISE_CERT_AS_ADMIN.bat` as Administrator).*

### 2. Launch Local Web Gateway & Signaling Server
```bash
# Install dependencies
npm install

# Run automated test suite (223 assertions)
node test.js

# Launch the server (Port 7700 / Fallback 12345)
npm start
```
Open [`http://localhost:7700`](http://localhost:7700) or [`https://desk.jdroid-x.com`](https://desk.jdroid-x.com).

### 3. Recompile Native Client
```cmd
compile_native.bat
```
Compiles `Native/` sources directly with 0 warnings and 0 errors into `jDroid-X-Desk.exe`.

---

## 🧪 Automated Verification Suite

Run the full end-to-end regression and verification test suite:
```bash
node test.js
```

```text
====================================================
 jDroid-X-Desk Stage 5 Automated Verification Suite
====================================================
[1/4] Verifying Code Integrity & Static Files... [PASS]
[2/4] Verifying Database Schema (19 Collections)... [PASS]
[3/4] Verifying DOM Affordances & Element IDs...  [PASS]
[4/4] Starting Server & Testing API Endpoints...  [PASS]
  - Signaling Heartbeat & Peer Discovery:        [PASS]
  - WebRTC SDP & ICE Candidate Exchange:         [PASS]
  - DeskRT Tile Stream & HTTP 304 Caching:       [PASS]
  - Permission Profiles & Client Identities:     [PASS]
  - In-Session Chat & Certificates Store:        [PASS]
  - Active & Historical Session Separation:      [PASS]
====================================================
 VERIFICATION RESULTS: 223 PASSED, 0 FAILED
====================================================
```

---

## 📄 License, Copyright & Attribution

Designed and developed by **jDroid-X Studio**.  
**jDroid-X-Desk** is an enterprise remote desktop application of **jDroid-X Studio**.  
Architected following the Senior Dynamic OOPS-based MVC Standard.

- **License**: Released under the [MIT License](LICENSE) with Enterprise Commercial terms.
- **Copyright**: **Copyright © 2026 JEET / jDroid-X Studio. All rights reserved.**
- **Trademarks**: `jDroid-X Studio™`, `jDroid-X-Desk™`, `Dynamic OOPS MVC Architecture™`.
- **Operator Circuit**: **Operator:** JEET | **Assistant:** jDroid-X
- **Official Web Portal**: [https://www.jdroid-x.com](https://www.jdroid-x.com)
- **Edge Ingress**: [https://desk.jdroid-x.com](https://desk.jdroid-x.com)
- **Source Repository**: [https://github.com/jDroid-X/jDroid-X-Desk](https://github.com/jDroid-X/jDroid-X-Desk)
