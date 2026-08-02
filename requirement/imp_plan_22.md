# [imp_plan_22] Cross-Platform Unix Installation & Launch Scripts

**Date**: 2026-08-02  
**Goal**: Build native Unix installation and startup compatibility for macOS and Linux operating systems with proper process management and desktop shortcut bindings.

## Summary of Changes
1. **OS Detection & Package Managers (`install_unix_app.sh`)**:
   - Built a shell script supporting macOS (`Darwin`) and Linux.
   - Programmed package manager detection (`brew`, `apt-get`, `dnf`) to auto-install Node.js.
2. **Launch & Terminate Utilities**:
   - `Launch-Desk.sh`: Starts the backend Node server using `nohup` in the background and launches MS Edge or default browser.
   - `stop_jdroid_app.sh`: Automatically finds and terminates the active process bound to port 12345.
3. **Desktop Integrations**:
   - Generated native `.command` launch shortcuts for macOS.
   - Generated `.desktop` integration files for Linux XDG-compliant environments.
   - Set proper Unix file executable permissions (`chmod +x`).

## Verification Plan
- Tested scripts on a Unix environment (macOS/Linux shell) to verify package lookup and desktop configuration generator paths.
