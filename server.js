const express = require('express');
const path = require('path');
const { exec } = require('child_process');
const fs = require('fs');
const os = require('os');
const crypto = require('crypto');

const app = express();
const PORT = 12345;
const DB_PATH = path.join(__dirname, 'database.json');

process.on('uncaughtException', (err) => { console.error('[SERVER ERROR] Uncaught Exception:', err.message); });
process.on('unhandledRejection', (reason) => { console.error('[SERVER ERROR] Unhandled Rejection:', reason); });

app.use(express.json());
app.use(express.static(__dirname));

const DEFAULT_SCHEMA = { safeZones: [], groups: [], sessions: [], devices: [] };

function writeDB(data) {
    const tmpPath = DB_PATH + '.tmp_' + Date.now();
    try {
        fs.writeFileSync(tmpPath, JSON.stringify(data, null, 2), 'utf8');
        fs.renameSync(tmpPath, DB_PATH);
    } catch (e) {
        if (fs.existsSync(tmpPath)) { try { fs.unlinkSync(tmpPath); } catch (_) {} }
        throw e;
    }
}

function readDB() {
    if (!fs.existsSync(DB_PATH)) fs.writeFileSync(DB_PATH, JSON.stringify(DEFAULT_SCHEMA, null, 2));
    try {
        const raw = fs.readFileSync(DB_PATH, 'utf8');
        const parsed = JSON.parse(raw);
        let dirty = false;
        for (const key of Object.keys(DEFAULT_SCHEMA)) { if (!parsed[key]) { parsed[key] = []; dirty = true; } }
        if (dirty) writeDB(parsed);
        return parsed;
    } catch (e) {
        console.error('[DATABASE CORRUPTION] Resetting.', e.message);
        const bk = DB_PATH + '.backup_' + Date.now();
        if (fs.existsSync(DB_PATH)) fs.renameSync(DB_PATH, bk);
        fs.writeFileSync(DB_PATH, JSON.stringify(DEFAULT_SCHEMA, null, 2));
        return { ...DEFAULT_SCHEMA };
    }
}

function validateBody(requiredFields, maxLen = 255) {
    return (req, res, next) => {
        for (const field of requiredFields) {
            const val = req.body[field];
            if (!val || typeof val !== 'string' || val.trim().length === 0)
                return res.status(400).json({ success: false, error: 'Field "' + field + '" is required and cannot be empty.' });
            if (val.trim().length > maxLen)
                return res.status(400).json({ success: false, error: 'Field "' + field + '" exceeds max length of ' + maxLen + ' characters.' });
        }
        next();
    };
}

app.get('/api/safe-zones', (req, res) => { const db = readDB(); res.json({ success: true, data: db.safeZones }); });
app.post('/api/safe-zones', validateBody(['ip', 'name']), (req, res) => {
    const db = readDB();
    const z = { id: Date.now().toString(), ip: req.body.ip.trim(), name: req.body.name.trim(), addedAt: new Date().toISOString() };
    db.safeZones.push(z); writeDB(db); res.json({ success: true, data: z });
});
app.put('/api/safe-zones/:id', validateBody(['ip', 'name']), (req, res) => {
    const db = readDB(); const i = db.safeZones.findIndex(z => z.id === req.params.id);
    if (i === -1) return res.status(404).json({ success: false, error: 'Safe zone not found.' });
    db.safeZones[i] = { ...db.safeZones[i], ip: req.body.ip.trim(), name: req.body.name.trim(), updatedAt: new Date().toISOString() };
    writeDB(db); res.json({ success: true, data: db.safeZones[i] });
});
app.delete('/api/safe-zones/:id', (req, res) => { const db = readDB(); db.safeZones = db.safeZones.filter(z => z.id !== req.params.id); writeDB(db); res.json({ success: true }); });

app.get('/api/groups', (req, res) => { const db = readDB(); res.json({ success: true, data: db.groups }); });
app.post('/api/groups', validateBody(['name']), (req, res) => {
    const db = readDB();
    const g = { id: Date.now().toString(), name: req.body.name.trim(), description: (req.body.description || '').trim(), members: 0, addedAt: new Date().toISOString() };
    db.groups.push(g); writeDB(db); res.json({ success: true, data: g });
});
app.put('/api/groups/:id', validateBody(['name']), (req, res) => {
    const db = readDB(); const i = db.groups.findIndex(g => g.id === req.params.id);
    if (i === -1) return res.status(404).json({ success: false, error: 'Group not found.' });
    db.groups[i] = { ...db.groups[i], name: req.body.name.trim(), description: (req.body.description || '').trim(), updatedAt: new Date().toISOString() };
    writeDB(db); res.json({ success: true, data: db.groups[i] });
});
app.delete('/api/groups/:id', (req, res) => { const db = readDB(); db.groups = db.groups.filter(g => g.id !== req.params.id); writeDB(db); res.json({ success: true }); });

app.get('/api/firebase-config', (req, res) => {
    res.json({ success: true, config: { apiKey: process.env.JDX_FB_API_KEY || null, projectId: process.env.JDX_FB_PROJECT_ID || null, databaseURL: process.env.JDX_FB_DB_URL || null } });
});

app.post('/api/startup', (req, res) => {
    try {
        const enable = req.body.enable;
        const startupFolder = path.join(process.env.APPDATA, 'Microsoft', 'Windows', 'Start Menu', 'Programs', 'Startup');
        const shortcutPath = path.join(startupFolder, 'jDroid-X-Desk.lnk');
        const targetPath = path.join(__dirname, 'start_jdroid_app.vbs');
        if (enable) {
            const vbs = '\n                Set oWS = WScript.CreateObject("WScript.Shell")\n                sLinkFile = "' + shortcutPath.replace(/\\/g, '\\\\') + '"\n                Set oLink = oWS.CreateShortcut(sLinkFile)\n                oLink.TargetPath = "wscript.exe"\n                oLink.Arguments = """' + targetPath.replace(/\\/g, '\\\\') + '"""\n                oLink.WorkingDirectory = "' + __dirname.replace(/\\/g, '\\\\') + '"\n                oLink.Description = "jDroid-X-Desk Remote Desktop Console"\n                oLink.Save\n            ';
            const tmp = path.join(__dirname, 'temp_startup_shortcut.vbs');
            try { fs.writeFileSync(tmp, vbs); } catch (we) { return res.status(500).json({ success: false, error: 'Failed to write VBS script. Permission denied.' }); }
            exec('cscript //nologo "' + tmp + '"', (err) => { if (fs.existsSync(tmp)) fs.unlinkSync(tmp); if (err) return res.status(500).json({ success: false, error: err.message }); res.json({ success: true, message: 'Startup enabled' }); });
        } else {
            if (fs.existsSync(shortcutPath)) fs.unlinkSync(shortcutPath);
            res.json({ success: true, message: 'Startup disabled' });
        }
    } catch (err) { res.status(500).json({ success: false, error: err.message }); }
});

app.post('/api/shutdown', (req, res) => { res.json({ success: true, message: 'Server shutting down' }); setTimeout(() => process.exit(0), 1000); });

app.get('/api/hardware-id', (req, res) => {
    try {
        const ifaces = os.networkInterfaces(); let mac = '00:00:00:00:00:00';
        for (const n of Object.keys(ifaces)) { for (const i of ifaces[n]) { if (!i.internal && i.mac !== '00:00:00:00:00:00') { mac = i.mac; break; } } if (mac !== '00:00:00:00:00:00') break; }
        const hash = crypto.createHash('md5').update(os.hostname() + '-' + mac).digest('hex').toUpperCase();
        res.json({ success: true, deviceId: 'JDX-' + hash.substring(0,4) + '-' + hash.substring(4,8) });
    } catch (err) { res.json({ success: true, deviceId: 'JDX-RAND-0000' }); }
});

const server = app.listen(PORT, () => { console.log('[SUCCESS] jDroid-X-Desk Server running on http://localhost:' + PORT); });
server.on('error', (err) => { if (err.code === 'EADDRINUSE') { console.log('[INFO] Port ' + PORT + ' already in use.'); } else { console.error('[SERVER ERROR]', err.message); } });
