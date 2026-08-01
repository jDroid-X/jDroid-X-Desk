# Windows App Installation & GitHub Sync Guide

**Application**: `jDroid-X-Desk` (Unified Remote Desktop Control Console)  
**Repository Remote**: `https://github.com/jDroid-X/jDroid-X-Desk.git`  
**Background Server Port**: `http://localhost:12345`  

---

## 1. Quick Windows 1-Click Installation (App Mode)

To install and launch `jDroid-X-Desk` as a standalone Windows Desktop Application:

1. Open the project folder: `c:\Users\jiten\jAnitGravity\jDroid-X-Desk`
2. Double-click **`install_windows_app.bat`**.
3. **What the installer does**:
   - Verifies Node.js & NPM availability.
   - Installs server package (`serve`).
   - Creates a **Desktop Shortcut** named `jDroid-X-Desk`.
   - Starts the background web server silently on `http://localhost:12345` using `start_jdroid_app.vbs`.
   - Opens Microsoft Edge / Chrome in native Standalone App Mode (`--app=http://localhost:12345`).

---

## 2. Running in Background Mode (`localhost:12345`)

- **Start Background Server & App Window**: Double-click `start_jdroid_app.vbs` or the Desktop Shortcut `jDroid-X-Desk`.
- **Stop Background Server**: Double-click `stop_jdroid_app.bat` to kill processes listening on port 12345.

---

## 3. GitHub Repository Synchronization (`jDroid-X/jDroid-X-Desk`)

To stage all code, commit changes, and push to GitHub:

### Automated Command:
```cmd
npm run sync-git
```

### Manual Command Steps:
```cmd
git init
git remote add origin https://github.com/jDroid-X/jDroid-X-Desk.git
git add .
git commit -m "v2.0 Streamlined Remote Desktop App & Continuous Traceability Engine"
git branch -M main
git push -u origin main
```
