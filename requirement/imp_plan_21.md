# [imp_plan_21] Installer Crash Resolution & Environment Sync

**Date**: 2026-08-02  
**Goal**: Resolve downloader crashes on fresh systems by securing TLS 1.2 protocol for Invoke-WebRequest and dynamically patching local PATH environment variables inside the installer.

## Summary of Changes
1. **Security Sync (`install_windows_app.bat` / PowerShell)**:
   - Injected `[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12` into the PowerShell download command.
2. **Environment Path Fix**:
   - Appended `C:\Program Files\nodejs` directly to the execution session's PATH variable immediately post-install.
   - Allowed `npm install` and app startup to run instantly in the same CMD instance without a reboot or terminal restart.

## Verification Plan
- Simulated fresh laptop installation flow and verified seamless installer flow from download to launch.
