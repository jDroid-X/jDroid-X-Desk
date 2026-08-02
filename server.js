const express = require('express');
const path = require('path');
const { exec, spawn } = require('child_process');
const fs = require('fs');
const os = require('os');
const crypto = require('crypto');

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

app.get('/api/hardware-id', (req, res) => {
    try {
        const interfaces = os.networkInterfaces();
        let macAddress = '00:00:00:00:00:00';
        for (const name of Object.keys(interfaces)) {
            for (const iface of interfaces[name]) {
                if (!iface.internal && iface.mac !== '00:00:00:00:00:00') {
                    macAddress = iface.mac;
                    break;
                }
            }
            if (macAddress !== '00:00:00:00:00:00') break;
        }
        
        const hostname = os.hostname();
        const rawString = `${hostname}-${macAddress}`;
        const hash = crypto.createHash('md5').update(rawString).digest('hex').toUpperCase();
        
        // Format as JDX-XXXX-XXXX
        const deviceId = `JDX-${hash.substring(0, 4)}-${hash.substring(4, 8)}`;
        res.json({ success: true, deviceId: deviceId });
    } catch (err) {
        res.json({ success: true, deviceId: 'JDX-RAND-0000' });
    }
});

// Start Server with EADDRINUSE Error Boundary
const server = app.listen(PORT, () => {
    console.log(`[SUCCESS] jDroid-X-Desk Server running on http://localhost:${PORT}`);
});

server.on('error', (err) => {
    if (err.code === 'EADDRINUSE') {
        console.log(`[INFO] Port ${PORT} is already in use by an active jDroid-X-Desk process.`);
        console.log(`[INFO] Server is already running at http://localhost:${PORT}`);
    } else {
        console.error('[SERVER ERROR] Failed to start server:', err.message);
    }
});
