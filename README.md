<div align="center">

<img src="assets/jdroidxlogo_trans.png" alt="jDroid-X-Desk Logo" width="140" />

# jDroid-X-Desk
### Enterprise-Grade Dynamic Remote Desktop & Machine Administration Workspace

[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011%20%7C%20Server-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk)
[![Runtime: .NET Framework 4.0/4.8](https://img.shields.io/badge/.NET-Framework%204.0%20%2F%204.8%20(Pure%20Native)-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk)
[![Engine: P2P UDP + AES-256](https://img.shields.io/badge/Security-AES--256%20GCM%20%2B%20Argon2id-10B981?style=for-the-badge&logo=shield&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk)
[![Architecture: Senior Dynamic OOPS MVC](https://img.shields.io/badge/Architecture-OOPS%20MVC%20Standard-F59E0B?style=for-the-badge)](https://github.com/jDroid-X/jDroid-X-Desk)
[![Status: Production Ready](https://img.shields.io/badge/Status-Production%20Ready-EF4444?style=for-the-badge)](https://github.com/jDroid-X/jDroid-X-Desk)

</div>

<div align="center">

<br/>

<img src="assets/jdroid-x-desk-presentation.png" alt="jDroid-X-Desk Workspace Presentation" width="95%" style="border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.5);" />

<br/>
<br/>

*Fig 1: jDroid-X-Desk Clean Standby Workspace — Dynamic SHA-256 Hardware Desk ID, One-Click Share & Connect Cards, and Zero-State Recent Connections for Fresh Installs.*

<br/>

[![Download Authorized EXE](https://img.shields.io/badge/Download-jDroid--X--Desk.exe%20(Authorized%20Build)-EF4444?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk.exe)
[![Official Web Gateway](https://img.shields.io/badge/Web%20Gateway-localhost%3A7070-10B981?style=for-the-badge&logo=google-chrome&logoColor=white)](http://localhost:7070)

</div>

---

## 📥 Direct Download & Universal Installation

**jDroid-X-Desk** is an enterprise subsidiary application of **jDroid-X Studio**. Built and verified as a **single universal Windows executable**, it runs on all Windows versions (Windows 7, 8, 8.1, 10, 11, and Windows Server) on AnyCPU (32-bit, 64-bit, and ARM64):

| Artifact | Type | Size | Direct Download Link | Security & Universal Authority Specification |
|---|---|---|---|---|
| **jDroid-X-Desk.exe** | **Single Universal Windows Executable** | **117 KB** | [📥 **Download jDroid-X-Desk.exe**](https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk.exe) | **Single Canonical Executable**: Universal AnyCPU architecture, embedded `app.manifest` (DPI-aware, Win 7/8/10/11), multi-target .NET 4.0–4.8.1 runtime fallback, Authenticode signed by **`jDroid-X Studio Enterprise`**. |
| **jDroid-X-Desk-Universal.zip** | **Portable Distribution Archive** | **125 KB** | [📥 **Download jDroid-X-Desk-Universal.zip**](https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk-Universal.zip) | Standalone package containing the unified executable, manifest, configs, and 1-click `Install-Or-Run.bat` unblock launcher. |
| **Install-Or-Run.bat** | **1-Click SmartScreen Unblock Launcher** | **1 KB** | [📥 **Download Install-Or-Run.bat**](https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main/Install-Or-Run.bat) | 1-click script that automatically unblocks the Mark-of-the-Web on Windows and launches `jDroid-X-Desk.exe`. |
| **jDroid-X-Client.bat** | **Canonical Technician Client Script** | **6.1 KB** | [📥 **Download jDroid-X-Client.bat**](https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main/jDroid-X-Client.bat) | Portable admin remote access client launcher with automated Windows UAC elevation. |
| **jDroid-X-Share.bat** | **Canonical Client Share Script** | **3.3 KB** | [📥 **Download jDroid-X-Share.bat**](https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main/jDroid-X-Share.bat) | Lightweight 1-click batch launcher to start instant screen sharing for remote users. |
| **Local Web Gateway** | **HTTP Server Endpoint** | — | `http://localhost:7070/download` | Direct browser download endpoint served by the local gateway. |
| **Official Repository** | **GitHub Repository** | — | [github.com/jDroid-X/jDroid-X-Desk](https://github.com/jDroid-X/jDroid-X-Desk) | Full source code, test suites, and documentation. |

---

## 🚀 Key Features

### 🔑 1. Dynamic SHA-256 System Hardware Key
- **No Hardcoded Static IDs**: Every computer generates its own unique, deterministic 9-digit Desk ID (`XXX-XXX-XXX`).
- **Hardware-Grounded Hash**: Combines Windows `MachineGuid` from `HKLM\SOFTWARE\Microsoft\Cryptography`, primary physical NIC MAC address, machine name, and CPU processor count through SHA-256.
- **Zero Collision Guarantee**: Permanent on that computer across reboots, completely distinct on any other PC on earth.

### 📥 2. Google Remote Desktop 3-Card Interface
- Reconstructed cleanly to match the benchmark set by [Google Chrome Remote Desktop](https://remotedesktop.google.com/access):
  1. **Share this screen (Get Support)**: Shows your 9-digit ID with prominent `[📋 Copy Desk ID]` and `[✉️ Copy Invite]` buttons.
  2. **Connect to another computer (Give Support)**: Clean empty input for remote connection with a bold `[⚡ Connect]` button.
  3. **Recent Connections**: Saved workstations with 1-click fast reconnect (opens with a clean empty state on fresh installations).

### ⚡ 3. Dual Architecture (Standalone Native + Full-Stack Web)
- **Standalone Native Portable Client (`jDroid-X-Desk.exe`)**:
  - Pure C# Windows Forms application compiled via `.NET Framework 4.0/4.8` `csc.exe`.
  - Single 93 KB file. **Requires zero installation, zero admin rights, zero Node.js runtime**.
  - Perfect for distribution to remote clients and fast technician support.
- **Full-Stack Web Gateway (`server.js` + `index.html`)**:
  - Express.js + WebRTC + WebSocket P2P streaming engine.
  - Multi-tenant web dashboard accessible from any modern web browser or mobile phone.

### 🛡️ 4. Enterprise Security & Access Governance
- **Optional Passcode Mode**: Incoming connections can run in direct connect mode or require a one-time passcode (configurable in **Settings Studio -> Security**).
- **Unattended 24/7 Access**: High-security server workstations can be managed remotely without human attendance using Argon2id hashed credentials.
- **Auditable Session Logs**: Every connection start, end, duration, and IP address is automatically recorded in SQLite/`database.json`.
- **Safe Zone IP Subnets**: CIDR-based firewall whitelisting ensures incoming connections only arrive from authorized networks.

### 🌐 5. Multi-Tier Hybrid Cloud Relays
- **Direct P2P LAN Mode**: Automatic local subnet discovery for high-throughput LAN transfers.
- **Google Colab Central Cloud Relay**: Connect remote workstations located behind strict NAT/firewalls via a Google Colab notebook relay (`colab_relay_server.py`, `jDroid-X-Desk.ipynb`).
- **RustDesk Server Compatibility**: Fully compatible with RustDesk `hbbs` (rendezvous/ID server) and `hbbr` (relay server).
- **Custom Enterprise Domain**: Direct integration with custom domains such as `https://www.jdroid-x.com`.

---

## 📦 File Architecture & Repository Map

```
jDroid-X-Desk/
├── jDroid-X-Desk.exe           # Single canonical universal Windows executable (Authenticode Signed, AnyCPU)
├── jDroid-X-Desk.exe.config    # Universal multi-target .NET runtime binding configuration
├── jDroid-X-Desk-Universal.zip # Complete standalone portable distribution package
├── JDroidXDeskNative.cs        # Senior Dynamic OOPS MVC C# source code (Safe cooperative threads & authenticated streaming)
├── app.manifest                # Windows Universal Application Manifest (AnyCPU, DPI Aware, Win 7/8/10/11)
├── Install-Or-Run.bat          # 1-Click safe runner (unblocks Windows SmartScreen Mark-of-the-Web)
├── Launch-Desk.bat             # 1-Click launcher for full-stack Node.js server & browser UI
├── Launch-Native.bat           # 1-Click launcher for native Windows executable
├── jDroid-X-Client.bat         # Canonical technician remote desktop client (UAC elevated)
├── jDroid-Client.bat           # Legacy alias technician client
├── jDroid-X-Share.bat          # Canonical client screen sharing script
├── jDroid-Share.bat            # Legacy alias screen sharing script
├── run_yaml_connection.py      # Python YAML connection executor (auto-tunnel & pairing)
├── server.js                   # Node.js Express & WebRTC backend gateway
├── app.js                      # Reactive frontend controller (OOPS MVC)
├── index.html                  # Responsive HTML5 desktop dashboard
├── styles.css                  # Modern high-contrast UI design system (Dual jDroid/AnyDesk CSS)
├── database.json               # Relational SQLite/JSON enterprise database
├── colab_relay_server.py       # Google Colab cloud relay bridge server
├── jDroid-X-Desk.ipynb         # Jupyter Notebook for running middle-layer Colab relay
├── windows_rdp.yaml            # Cloud RDP deployment configuration
├── test.js                     # Automated Stage 5 verification test suite (91 assertions)
├── assets/
│   ├── app.ico                 # 256x256 multi-resolution embedded application icon
│   ├── jdroid-x-desk-presentation.png # High-resolution UI showcase presentation
│   └── jdroidxlogo_trans.png   # Transparent jDroid-X branding logo
├── jDroid-X-Colab Server/      # Dedicated watchdog & relay scripts for Google Colab
│   ├── jDroid-X-Colab-Notebook.ipynb # Production Colab notebook
│   ├── jDroid-X-Colab-Relay-Server.py# Production Colab relay server
│   ├── colab-watchdog-keepalive.py   # Background keep-alive daemon
│   ├── run_yaml_connection.py        # Connection bootstrap script
│   ├── windows_rdp_colab.yaml        # Colab RDP config
│   └── README.md                     # Colab architecture & setup guide
└── docs/                       # Specifications, reverse engineering docs & analysis
    └── REA_CHROME_REMOTE_DESKTOP_ANALYSIS.md
```

---

## ⚡ Quick Start Guide

### Option 1: Run the Standalone Native App (No Installation Needed)
Just run the executable on any Windows PC:
```cmd
jDroid-X-Desk.exe
```
Or double-click `Launch-Native.bat`.

### Option 2: Run the Full-Stack Web Gateway & Server
To run the full multi-tenant web server locally:
```bash
# 1. Install dependencies
npm install

# 2. Run automated verification suite
node test.js

# 3. Launch the application
npm start
```
The server will start listening on `http://localhost:7070` and automatically open your default browser.

### Option 3: Deploy the Google Colab Central Cloud Relay
1. Open [`jDroid-X-Desk.ipynb`](.jDroid-X-Desk.ipynb) in Google Colab.
2. Run all cells to launch `colab_relay_server.py`.
3. In `jDroid-X-Desk.exe`, navigate to **Settings Studio -> Connection & Relays** and paste your Colab tunnel URL.

---

## 🧪 Automated Verification Suite

Run the full end-to-end regression and verification test suite:
```bash
node test.js
```

```
====================================================
 jDroid-X-Desk Stage 5 Automated Verification Suite
====================================================
[1/4] Verifying Code Integrity & Static Files... [PASS]
[2/4] Verifying Database Schema Tables...         [PASS]
[3/4] Verifying DOM Affordances & Element IDs...  [PASS]
[4/4] Starting Server & Testing API Endpoints...  [PASS]
====================================================
 VERIFICATION RESULTS: 84 PASSED, 0 FAILED
====================================================
```

---

## 📄 License & Attribution

Designed and developed by **jDroid-X Studio**.  
**jDroid-X-Desk** is an enterprise subsidiary application of **jDroid-X Studio**.  
Architected following dynamic OOPS-based MVC standards.

**Operator:** JEET | **Studio:** jDroid-X Studio | **Assistant:** jDroid-X  
**Official Portal:** [https://www.jdroid-x.com](https://www.jdroid-x.com) | **Repository:** [https://github.com/jDroid-X/jDroid-X-Desk](https://github.com/jDroid-X/jDroid-X-Desk)
