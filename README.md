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

---

## 🌟 Overview

**jDroid-X-Desk** is a lightning-fast, ultra-portable, enterprise-ready remote desktop control and fleet management suite. Engineered following the industry standard of **Google Chrome Remote Desktop**, **RustDesk**, and **AnyDesk**, it combines a **zero-dependency native standalone Windows executable (`jDroid-X-Desk.exe`, 93 KB)** with a rich **full-stack WebRTC/Node.js web administration gateway**.

Whether you need 1-click remote assistance for non-technical users or a central multi-node cloud relay infrastructure across remote clusters, **jDroid-X-Desk** delivers deterministic performance, hardware-grounded security, and frictionless usability.

---

## 📥 Direct Download Paths

Get the latest updated client and tools directly:

| Artifact | Type | Size | Direct Download Link | Description |
|---|---|---|---|---|
| **jDroid-X-Desk.exe** | **Standalone Windows Native Executable** | **93 KB** | [📥 **Download jDroid-X-Desk.exe**](https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk.exe) | **Single-file portable client**. Zero installer, zero dependencies. Runs on Windows 10, 11 & Server with dynamic hardware key. |
| **jDroid-Share.bat** | **Portable Client Share Script** | **3.3 KB** | [📥 **Download jDroid-Share.bat**](https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main/jDroid-Share.bat) | Lightweight 1-click batch launcher to start instant screen sharing for remote users. |
| **jDroid-Client.bat** | **Technician Client Script** | **6.1 KB** | [📥 **Download jDroid-Client.bat**](https://raw.githubusercontent.com/jDroid-X/jDroid-X-Desk/main/jDroid-Client.bat) | Portable admin remote access client launcher. |
| **Local Web Gateway** | **HTTP Server Endpoint** | — | `http://localhost:7070/download` | Direct browser download endpoint served by the local gateway. |
| **Official Repository** | **GitHub Repository** | — | [github.com/jDroid-X/jDroid-X-Desk](https://github.com/jDroid-X/jDroid-X-Desk) | Full source code, test suites, and documentation. |

---

## 📸 Visual Showcase & Presentation

### 1. Main Remote Desktop Workspace & Google Remote Desktop 3-Card Experience
The primary interface provides a clean, Google Remote Desktop-inspired workflow:
- **Share this screen (Get Support)**: Instant unique 9-digit Desk ID with 1-click clipboard copy and invite generation.
- **Connect to another computer (Give Support)**: Clean, non-overlapping target ID input for direct P2P sessions.
- **Recent Connections**: Saved partner workstations with 1-click instant reconnect.

<div align="center">
  <img src="screenshot/Screenshot 2026-10-16 191644.png" alt="Main Remote Desktop Workspace" width="900" />
</div>

---

### 2. Live High-Fidelity Remote Desktop Viewport & SAS Diagnostics
Ultra-low latency screen mirroring (60 FPS, direct UDP peer-to-peer or encrypted cloud relay) with real-time HUD telemetry, SAS (Ctrl+Alt+Del) injection, and smart display fitting.

<div align="center">
  <img src="screenshot/Screenshot 2026-10-16 191912.png" alt="Remote Desktop Viewport" width="440" />
  <img src="screenshot/Screenshot 2026-10-16 192022.png" alt="Remote Desktop Session In Progress" width="440" />
</div>

---

### 3. Enterprise Connection Relays & Multi-Cloud Infrastructure
Configure custom relay domains (`https://www.jdroid-x.com`), direct RustDesk `hbbs`/`hbbr` ID servers, Google Colab cloud brokers, or public secure tunnels.

<div align="center">
  <img src="screenshot/connection.png" alt="Connection & Relays Configuration" width="900" />
</div>

---

### 4. Comprehensive Settings & Security Studio
Fine-grained permission controls, unattended 24/7 access toggles, optional passcode verification, and session timeouts.

<div align="center">
  <img src="screenshot/access1.png" alt="Access Control & Security Settings" width="440" />
  <img src="screenshot/permission1.png" alt="Access Permissions & Guardian Whitelist" width="440" />
</div>

---

### 5. Display Scaling, Wake-On-LAN & Session Recording
Hardware acceleration, multi-monitor topology, Wake-On-LAN (WOL) magic packet broadcasting, and auditable session screen recording.

<div align="center">
  <img src="screenshot/display2.png" alt="Display & Scaling Preferences" width="290" />
  <img src="screenshot/wake-on-lan.png" alt="Wake On LAN Configuration" width="290" />
  <img src="screenshot/screen recording.png" alt="Session Screen Recording" width="290" />
</div>

---

### 6. Architecture & System Diagnostics
Pure standalone native compilation details, zero-overhead memory footprint (<65 MB RAM), and auditable cryptographic verification.

<div align="center">
  <img src="screenshot/About.png" alt="About jDroid-X-Desk & Architecture" width="900" />
</div>

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
├── jDroid-X-Desk.exe           # Single standalone portable native client (93 KB, ZERO dependencies)
├── JDroidXDeskNative.cs        # Senior Dynamic OOPS MVC C# source code
├── Launch-Desk.bat             # 1-Click launcher for full-stack Node.js server & browser UI
├── Launch-Native.bat           # 1-Click launcher for native Windows executable
├── jDroid-Share.bat            # Lightweight script for client screen sharing
├── jDroid-Client.bat           # Portable remote desktop technician client
├── server.js                   # Node.js Express & WebRTC backend gateway
├── app.js                      # Reactive frontend controller (OOPS MVC)
├── index.html                  # Responsive HTML5 desktop dashboard
├── styles.css                  # Modern high-contrast UI design system
├── database.json               # Relational SQLite/JSON database context
├── colab_relay_server.py       # Google Colab cloud relay bridge server
├── jDroid-X-Desk.ipynb         # Jupyter Notebook for running middle-layer Colab relay
├── windows_rdp.yaml            # Cloud RDP deployment configuration
├── test.js                     # Automated Stage 5 verification test suite (55 tests)
├── assets/
│   └── jdroidxlogo_trans.png   # Transparent jDroid-X branding logo
├── screenshot/                 # Complete gallery of 21 verified screenshots
│   ├── Screenshot 2026-10-16 191644.png
│   ├── connection.png
│   ├── access1.png
│   └── ...
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
 VERIFICATION RESULTS: 55 PASSED, 0 FAILED
====================================================
```

---

## 📄 License & Attribution

Designed and developed for **jDroid-X Ecosystem**.  
Architected following dynamic OOPS-based MVC standards.

**Operator:** JEET | **Assistant:** jDroid-X  
**Official Portal:** [https://www.jdroid-x.com](https://www.jdroid-x.com) | **Repository:** [https://github.com/jDroid-X/jDroid-X-Desk](https://github.com/jDroid-X/jDroid-X-Desk)
