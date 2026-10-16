# Windows App Installation & GitHub Sync Guide

**Application**: `jDroid-X-Desk` (Unified Remote Desktop Control Console)  
**Repository Remote**: `https://github.com/jDroid-X/jDroid-X-Desk.git`  
**Background Server Port**: `http://localhost:12345`  

---

## 1. Quick Windows 1-Click Installation (Native App Mode)

To install and launch `jDroid-X-Desk` as a standalone Windows Desktop Application (matching AnyDesk window behavior without browser tabs or address bars):

1. Open the project folder: `c:\Users\jiten\jAnitGravity\jDroid-X-Desk`
2. Double-click **`install_windows_app.bat`** (or **`start_jdroid_app.vbs`**).
3. **What happens**:
   - Verifies Node.js & NPM availability.
   - Creates a **Desktop Shortcut** named `jDroid-X-Desk`.
   - Starts the background Node.js server silently on `http://localhost:12345`.
   - Opens Microsoft Edge / Chrome in native Standalone Container Mode (`--app=http://localhost:12345 --window-size=1280,820`).

---

## 2. Remote Client Sharing & Administrative Access

- **Client Executable**: Share **`jDroid-Client.bat`** (or click `[Download Client (.bat)]` in the UI).
- **Features**:
  - Automatically self-elevates to Windows Administrator via UAC.
  - Generates a deterministic 9-digit Desk ID and 4-digit security PIN.
  - Provides 1-click Quick Assist, MS Remote Assistance, and Admin Remote Shell triggers.

---

## 3. AnyDesk Session Invitation Engine

- Click the red **`[Invite]`** button next to your 9-digit Desk ID.
- Enter target Desk ID / Alias and select a permission profile (Default, Screen Sharing, Full Access, Unattended).
- Customize the 14 granular permission toggles (Audio, Clipboard, File Manager, Whiteboard, CAD, Restart, UAC).
- Invitations feature a strict 2-minute time-to-live (TTL) and real-time pop-up notifications on the recipient's machine.

---

## 4. AnyDesk Settings Studio

Navigate to **Settings** (`#system-settings`) in the sidebar to access the 12-panel settings console:
- **Application**: General (Language, Theme, Notifications, Paths), Connection (Port 7070, HTTP/SOCKS5 Proxy, NTLM), Wake-On-LAN, Screen Recording.
- **Security**: Access (Interactive modes, 2FA, ACL whitelist), Permissions (Profiles & 14 toggles), Privacy (Screen framing highlight border).
- **Session**: Display (Quality, visual cursor helpers, Direct3D acceleration), Audio, Recording, Remote Printer redirection.
- **Account & Cloud**: License status, Security Fingerprint, and Google Colab / Firebase signaling bridge.

---

## 5. Google Colab High-Memory Relay Hub

Use Google Colab as a free, 12+ GB RAM centralized relay unit to bridge Desk 1 and Desk 2 across NAT/firewalls:
1. Open the Colab notebook: `https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai`
2. Run the cloud relay script with Cloudflare Tunnel (`cloudflared`).
3. Copy the output public tunnel URL (e.g. `https://xxxx.trycloudflare.com`) and paste it into **Settings -> Cloud & Signaling -> Centralized Relay Server URL**.

---

## 6. GitHub Repository Synchronization (`jDroid-X/jDroid-X-Desk`)

```cmd
git add .
git commit -m "feat: complete AnyDesk parity with session invites, settings studio, and Colab relay"
git push -u origin main
```

