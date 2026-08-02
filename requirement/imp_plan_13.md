# [imp_plan_13] Express Backend, PowerShell System Tray & Launch Popup

**Date**: 2026-08-02  
**Goal**: Introduce a custom Node.js Express server to handle native OS commands, a PowerShell system tray script, and an interactive frontend Launch Popup dialog.

## Summary of Changes
1. **`server.js`**: Replaced static `serve` with Express server providing `/api/startup` and `/api/shutdown` endpoints.
2. **`tray_icon.ps1`**: Created native PowerShell script displaying a System Tray icon with "Open App" and "Stop Server & Exit" menu.
3. **`index.html` & `app.js`**: Added Welcome Launch Popup modal with "Start with Windows" and "Don't show again" checkboxes.
