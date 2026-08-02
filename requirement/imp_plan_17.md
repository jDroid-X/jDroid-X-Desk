# [imp_plan_17] Terminal Crash Remediation & Server Error Boundaries

**Date**: 2026-08-02  
**Goal**: Identify and resolve root cause of frequent terminal crashes when launching `install_windows_app.bat` or `start_jdroid_app.vbs`.

## Summary of Changes
1. **Root Cause Identified**:
   - Multiple lingering background instances of `node.exe` and `powershell.exe` were occupying Port `12345`.
   - Express threw unhandled `EADDRINUSE` exception, causing `node server.js` to crash immediately and terminate the CMD window.
   - PowerShell `tray_icon.ps1` lacked error handling for duplicate tray instances.
2. **Server Fault Tolerance (`server.js`)**:
   - Added `process.on('uncaughtException')` and `process.on('unhandledRejection')` guards.
   - Added `EADDRINUSE` error handler to `app.listen()`. If Port 12345 is already in use, the server logs status and continues execution safely without crashing.
   - Added `WMIC` process check to prevent spawning duplicate `tray_icon.ps1` instances.
3. **PowerShell Fault Tolerance (`tray_icon.ps1`)**:
   - Wrapped script in `try { ... } catch { ... }` blocks with fallback system icons and web launch handlers.
4. **Installer Error Handling (`install_windows_app.bat`)**:
   - Fixed unescaped ampersand (`&`) in `ECHO` statement that caused CMD to misinterpret `NPM` as a command and crash.
   - Replaced `timeout /t 10` with `ping 127.0.0.1 -n 11 > nul` to prevent `ERROR: Input redirection is not supported` crashes in non-interactive terminal environments.
   - Added error level checking for `npm install`.

## Verification Plan
- Tested `node server.js` and confirmed log output: `[SUCCESS] jDroid-X-Desk Server running on http://localhost:12345`.
