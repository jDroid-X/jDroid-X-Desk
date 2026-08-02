# [imp_plan_18] Automated Node.js Provisioning & PowerShell Installer

**Date**: 2026-08-02  
**Goal**: Address terminal crashes on fresh laptops lacking Node.js by replacing the `PAUSE` command in the Windows installer with an automatic PowerShell download and installation launcher.

## Summary of Changes
1. **Windows Installer (`install_windows_app.bat`)**:
   - Replaced basic crash-prone checking with dynamic PowerShell script execution.
   - Removed the simple `PAUSE` command when Node.js is missing.
2. **PowerShell Setup Script Integration**:
   - Programmed `Invoke-WebRequest` to download the official Node.js MSI package directly from Node.js servers.
   - Configured silent installer execution with auto-restart path variables.
   - Instructed the user gracefully through the setup process.

## Verification Plan
- Tested installer run on environment lacking Node.js; verified download trigger and installer prompt.
