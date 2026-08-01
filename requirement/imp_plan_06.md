# [imp_plan_06] Windows Installer Scripting, Background Server (Port 12345) & GitHub Sync

**Date**: 2026-08-01  
**Goal**: Create Windows 1-click installer scripts (`install_windows_app.bat`), silent background service launcher (`start_jdroid_app.vbs` on `http://localhost:12345`), standalone browser app launcher (`--app`), and sync with `https://github.com/jDroid-X/jDroid-X-Desk`.

## Summary of Proposed Changes
1. **`start_jdroid_app.vbs`**: Runs `npx serve . -p 12345` silently in the background without a DOS popup, then opens Edge in `--app=http://localhost:12345` mode.
2. **`install_windows_app.bat`**: 1-Click installer verifying Node.js, installing dependencies, creating Desktop Shortcut `jDroid-X-Desk`, and starting app.
3. **`stop_jdroid_app.bat`**: Stops background server processes listening on port 12345.
4. **`INSTALL_GUIDE_WINDOWS.md`**: Step-by-step installation & GitHub sync manual.
5. **Git Repository Setup**: Initialized Git repository in `c:\Users\jiten\jAnitGravity\jDroid-X-Desk`, set remote origin to `https://github.com/jDroid-X/jDroid-X-Desk.git`, and committed root commit `5bbcb41`.

## Verification Plan
- Verify scripts and commit log status.
