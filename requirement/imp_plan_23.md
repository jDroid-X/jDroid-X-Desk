# [imp_plan_23] Hardware-Based Device ID Generation & Tray Decoupling

**Date**: 2026-08-02  
**Goal**: Remove hardcoded identifier strings and generate a unique hardware fingerprint using hostname and MAC Address hashing, while making the tray icon visible by spawning it in the active user desktop session.

## Summary of Changes
1. **Hardware ID Backend API (`server.js`)**:
   - Replaced static ID `984-102-391` with a dynamic API route (`/api/hardware-id`).
   - Retrieved local MAC addresses and hostname, ran SHA-256 hash, and formatted it into a structured `JDX-XXXX-XXXX` token.
2. **Dynamic UI Update (`app.js` & `index.html`)**:
   - Configured app to fetch dynamic ID on launch and update the Host Address elements.
3. **Tray Icon Decoupling (`Launch-Desk.bat` & `install_windows_app.bat` & `server.js`)**:
   - Decoupled `tray_icon.ps1` from the background hidden Node server process.
   - Spowned `tray_icon.ps1` directly in the active CMD script window so it runs within the user's interactive desktop window session, ensuring visibility of the system tray icon.

## Verification Plan
- Inspected active device ID on launch; verified it follows the hash-generated `JDX-` format.
- Checked Windows system tray area for the active jDroid-X Desk icon.
