const express = require('express');
const path = require('path');
const { exec, spawn } = require('child_process');
const fs = require('fs');

const app = express();
const PORT = 12345;

// Prevent Process Crashes on Uncaught Exceptions
process.on('uncaughtException', (err) => {
    console.error('[SERVER ERROR] Uncaught Exception:', err.message);
});

process.on('unhandledRejection', (reason, promise) => {
    console.error('[SERVER ERROR] Unhandled Rejection:', reason);
});

app.use(express.json());
app.use(express.static(__dirname));

app.post('/api/startup', (req, res) => {
    try {
        const enable = req.body.enable;
        const startupFolder = path.join(process.env.APPDATA, 'Microsoft', 'Windows', 'Start Menu', 'Programs', 'Startup');
        const shortcutPath = path.join(startupFolder, 'jDroid-X-Desk.lnk');
        const targetPath = path.join(__dirname, 'start_jdroid_app.vbs');

        if (enable) {
            const vbsScript = `
                Set oWS = WScript.CreateObject("WScript.Shell")
                sLinkFile = "${shortcutPath.replace(/\\/g, '\\\\')}"
                Set oLink = oWS.CreateShortcut(sLinkFile)
                oLink.TargetPath = "wscript.exe"
                oLink.Arguments = """${targetPath.replace(/\\/g, '\\\\')}"""
                oLink.WorkingDirectory = "${__dirname.replace(/\\/g, '\\\\')}"
                oLink.Description = "jDroid-X-Desk Remote Desktop Console"
                oLink.Save
            `;
            const tempVbs = path.join(__dirname, 'temp_startup_shortcut.vbs');
            fs.writeFileSync(tempVbs, vbsScript);
            exec(`cscript //nologo "${tempVbs}"`, (err) => {
                if (fs.existsSync(tempVbs)) fs.unlinkSync(tempVbs);
                if (err) return res.status(500).json({ success: false, error: err.message });
                res.json({ success: true, message: 'Startup enabled' });
            });
        } else {
            if (fs.existsSync(shortcutPath)) {
                fs.unlinkSync(shortcutPath);
            }
            res.json({ success: true, message: 'Startup disabled' });
        }
    } catch (err) {
        res.status(500).json({ success: false, error: err.message });
    }
});

app.post('/api/shutdown', (req, res) => {
    res.json({ success: true, message: 'Server shutting down' });
    setTimeout(() => process.exit(0), 1000);
});

// Function to safely launch System Tray Icon
function launchSystemTray() {
    const psScript = path.join(__dirname, 'tray_icon.ps1');
    if (!fs.existsSync(psScript)) return;

    // Check if tray_icon.ps1 is already running to prevent duplicates
    exec('wmic process where "name=\'powershell.exe\'" get commandline', (err, stdout) => {
        if (!err && stdout && stdout.includes('tray_icon.ps1')) {
            console.log('[TRAY] System Tray Icon is already active.');
            return;
        }

        const trayProcess = spawn('powershell.exe', [
            '-ExecutionPolicy', 'Bypass',
            '-WindowStyle', 'Hidden',
            '-File', psScript
        ], {
            detached: true,
            stdio: 'ignore'
        });
        trayProcess.unref();
        console.log('[TRAY] System Tray Icon spawned successfully.');
    });
}

// Start Server with EADDRINUSE Error Boundary
const server = app.listen(PORT, () => {
    console.log(`[SUCCESS] jDroid-X-Desk Server running on http://localhost:${PORT}`);
    launchSystemTray();
});

server.on('error', (err) => {
    if (err.code === 'EADDRINUSE') {
        console.log(`[INFO] Port ${PORT} is already in use by an active jDroid-X-Desk process.`);
        console.log(`[INFO] Server is already running at http://localhost:${PORT}`);
        launchSystemTray();
    } else {
        console.error('[SERVER ERROR] Failed to start server:', err.message);
    }
});
