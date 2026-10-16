const express = require('express');
const path = require('path');
const { exec } = require('child_process');
const fs = require('fs');
const os = require('os');
const crypto = require('crypto');

const app = express();
const PORT = process.env.PORT || 12345;
const DB_PATH = path.join(__dirname, 'database.json');

process.on('uncaughtException', (err) => { console.error('[SERVER ERROR] Uncaught Exception:', err.message); });
process.on('unhandledRejection', (reason) => { console.error('[SERVER ERROR] Unhandled Rejection:', reason); });

app.use(express.json());
app.use(express.static(__dirname));

const DEFAULT_SETTINGS = {
    pingThresholdMs: 100,
    downloadThresholdMBs: 1.0,
    toastDurationMs: 4000,
    defaultAccessLevel: 'Standard',
    theme: 'light',
    networkBaseline: '4G/5G Auto',
    autoStartup: false,
    jarvisTelemetry: true
};

const DEFAULT_SCHEMA = {
    safeZones: [],
    groups: [],
    sessions: [],
    devices: [],
    guardians: [],
    contacts: [],
    notifications: [],
    settings: DEFAULT_SETTINGS,
    logs: []
};

const RETENTION_MS = 7 * 24 * 60 * 60 * 1000; // 7 Days in Milliseconds
let reqLogSeq = 1000;

function purgeOldLogs(db) {
    if (!Array.isArray(db.logs)) return false;
    const cutoff = Date.now() - RETENTION_MS;
    const initialLen = db.logs.length;
    db.logs = db.logs.filter(l => {
        const t = new Date(l.timestamp).getTime();
        return !isNaN(t) && t >= cutoff;
    });
    return db.logs.length < initialLen;
}

function writeDB(data) {
    const tmpPath = DB_PATH + '.tmp_' + Date.now();
    try {
        fs.writeFileSync(tmpPath, JSON.stringify(data, null, 2), 'utf8');
        try {
            fs.renameSync(tmpPath, DB_PATH);
        } catch (renameErr) {
            if (renameErr.code === 'EPERM' || renameErr.code === 'EBUSY') {
                fs.copyFileSync(tmpPath, DB_PATH);
                try { fs.unlinkSync(tmpPath); } catch (_) {}
            } else {
                throw renameErr;
            }
        }
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
        for (const key of Object.keys(DEFAULT_SCHEMA)) {
            if (!parsed[key]) {
                parsed[key] = Array.isArray(DEFAULT_SCHEMA[key]) ? [] : { ...DEFAULT_SCHEMA[key] };
                dirty = true;
            }
        }
        if (purgeOldLogs(parsed)) dirty = true;
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

// Automatic Hourly Background 7-Day Log Purging Maintenance
setInterval(() => {
    try {
        const db = readDB();
        if (purgeOldLogs(db)) writeDB(db);
    } catch (_) {}
}, 3600000); // 1 Hour

// IN/OUT Request Logger Middleware for API Endpoints
app.use((req, res, next) => {
    // Exclude static web asset files to keep log audit focused on API traffic
    if (req.url.match(/\.(html|css|js|png|jpg|ico|svg|vbs|bat|sh)$/i)) {
        return next();
    }

    const start = Date.now();
    const clientIp = (req.headers['x-forwarded-for'] || req.socket.remoteAddress || '127.0.0.1').replace('::ffff:', '');

    res.on('finish', () => {
        const durationMs = Date.now() - start;
        // Don't recursively log calls to /api/logs GET/POST to avoid infinite logging loops
        if (req.url.startsWith('/api/logs')) return;

        try {
            const db = readDB();
            const logEntry = {
                id: `REQ-${Date.now()}-${reqLogSeq++}`,
                level: res.statusCode >= 400 ? (res.statusCode >= 500 ? 'ERROR' : 'WARN') : 'INFO',
                stage: 'HTTP_TRAFFIC',
                method: req.method,
                url: req.url,
                ip: clientIp,
                statusCode: res.statusCode,
                rttMs: durationMs,
                msg: `[IN/OUT] ${req.method} ${req.url} -> HTTP ${res.statusCode} (${durationMs}ms) from ${clientIp}`,
                timestamp: new Date().toISOString()
            };

            db.logs.push(logEntry);
            purgeOldLogs(db);
            writeDB(db);
        } catch (_) {}
    });

    next();
});

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

// ==========================================
// 1. SAFE ZONES API
// ==========================================
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

// ==========================================
// 2. DEVICE GROUPS API (Relational calculation)
// ==========================================
app.get('/api/groups', (req, res) => {
    const db = readDB();
    // Dynamically populate member count from devices table relationship
    const groupsWithMembers = db.groups.map(g => {
        const count = db.devices.filter(d => d.groupId === g.id).length;
        return { ...g, members: count };
    });
    res.json({ success: true, data: groupsWithMembers });
});
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
app.delete('/api/groups/:id', (req, res) => {
    const db = readDB();
    db.groups = db.groups.filter(g => g.id !== req.params.id);
    // Unassign devices from deleted group
    db.devices = db.devices.map(d => d.groupId === req.params.id ? { ...d, groupId: null } : d);
    writeDB(db); res.json({ success: true });
});

// ==========================================
// 3. DEVICES FLEET API
// ==========================================
app.get('/api/devices', (req, res) => { const db = readDB(); res.json({ success: true, data: db.devices }); });
app.post('/api/devices', validateBody(['name', 'accessLevel']), (req, res) => {
    const db = readDB();
    if (!/^[a-zA-Z0-9 _.-]+$/.test(req.body.name)) return res.status(400).json({ success: false, error: 'Device Name contains invalid characters.' });
    const d = {
        id: Date.now().toString(),
        name: req.body.name.trim(),
        accessLevel: req.body.accessLevel.trim(),
        groupId: req.body.groupId || null,
        status: req.body.status || 'Online',
        addedAt: new Date().toISOString()
    };
    db.devices.push(d); writeDB(db); res.json({ success: true, data: d });
});
app.put('/api/devices/:id', validateBody(['name', 'accessLevel']), (req, res) => {
    const db = readDB(); const i = db.devices.findIndex(d => d.id === req.params.id);
    if (i === -1) return res.status(404).json({ success: false, error: 'Device not found.' });
    if (!/^[a-zA-Z0-9 _.-]+$/.test(req.body.name)) return res.status(400).json({ success: false, error: 'Device Name contains invalid characters.' });
    db.devices[i] = {
        ...db.devices[i],
        name: req.body.name.trim(),
        accessLevel: req.body.accessLevel.trim(),
        groupId: req.body.groupId !== undefined ? req.body.groupId : db.devices[i].groupId,
        status: req.body.status || db.devices[i].status,
        updatedAt: new Date().toISOString()
    };
    writeDB(db); res.json({ success: true, data: db.devices[i] });
});
app.delete('/api/devices/:id', (req, res) => { const db = readDB(); db.devices = db.devices.filter(d => d.id !== req.params.id); writeDB(db); res.json({ success: true }); });

// ==========================================
// 4. GUARDIANS / FAMILY API
// ==========================================
app.get('/api/guardians', (req, res) => { const db = readDB(); res.json({ success: true, data: db.guardians }); });
app.post('/api/guardians', validateBody(['name', 'role', 'contact']), (req, res) => {
    const db = readDB();
    const g = {
        id: Date.now().toString(),
        name: req.body.name.trim(),
        role: req.body.role.trim(),
        contact: req.body.contact.trim(),
        status: req.body.status || 'Active',
        addedAt: new Date().toISOString()
    };
    db.guardians.push(g); writeDB(db); res.json({ success: true, data: g });
});
app.put('/api/guardians/:id', validateBody(['name', 'role', 'contact']), (req, res) => {
    const db = readDB(); const i = db.guardians.findIndex(g => g.id === req.params.id);
    if (i === -1) return res.status(404).json({ success: false, error: 'Guardian not found.' });
    db.guardians[i] = { ...db.guardians[i], name: req.body.name.trim(), role: req.body.role.trim(), contact: req.body.contact.trim(), status: req.body.status || 'Active', updatedAt: new Date().toISOString() };
    writeDB(db); res.json({ success: true, data: db.guardians[i] });
});
app.delete('/api/guardians/:id', (req, res) => { const db = readDB(); db.guardians = db.guardians.filter(g => g.id !== req.params.id); writeDB(db); res.json({ success: true }); });

// ==========================================
// 5. REMOTE CONTACTS API
// ==========================================
app.get('/api/contacts', (req, res) => { const db = readDB(); res.json({ success: true, data: db.contacts }); });
app.post('/api/contacts', validateBody(['name', 'remoteId']), (req, res) => {
    const db = readDB();
    const c = {
        id: Date.now().toString(),
        name: req.body.name.trim(),
        remoteId: req.body.remoteId.trim(),
        group: req.body.group || 'General',
        addedAt: new Date().toISOString()
    };
    db.contacts.push(c); writeDB(db); res.json({ success: true, data: c });
});
app.put('/api/contacts/:id', validateBody(['name', 'remoteId']), (req, res) => {
    const db = readDB(); const i = db.contacts.findIndex(c => c.id === req.params.id);
    if (i === -1) return res.status(404).json({ success: false, error: 'Contact not found.' });
    db.contacts[i] = { ...db.contacts[i], name: req.body.name.trim(), remoteId: req.body.remoteId.trim(), group: req.body.group || 'General', updatedAt: new Date().toISOString() };
    writeDB(db); res.json({ success: true, data: db.contacts[i] });
});
app.delete('/api/contacts/:id', (req, res) => { const db = readDB(); db.contacts = db.contacts.filter(c => c.id !== req.params.id); writeDB(db); res.json({ success: true }); });

// ==========================================
// 6. DYNAMIC SYSTEM SETTINGS API (No hardcoding)
// ==========================================
app.get('/api/settings', (req, res) => { const db = readDB(); res.json({ success: true, data: db.settings }); });
app.put('/api/settings', (req, res) => {
    const db = readDB();
    db.settings = { ...db.settings, ...req.body, updatedAt: new Date().toISOString() };
    writeDB(db);
    res.json({ success: true, data: db.settings, message: 'System Settings Saved Successfully' });
});

// ==========================================
// 7. SYSTEM LOGS & TELEMETRY AUDIT API
// ==========================================
app.get('/api/logs', (req, res) => { const db = readDB(); res.json({ success: true, data: db.logs }); });
app.post('/api/logs', (req, res) => {
    const db = readDB();
    const log = { id: Date.now().toString(), level: req.body.level || 'INFO', stage: req.body.stage || 'GENERAL', msg: req.body.msg || '', timestamp: new Date().toISOString() };
    db.logs.push(log);
    if (db.logs.length > 500) db.logs.shift(); // Keep last 500 logs
    writeDB(db);
    res.json({ success: true, data: log });
});
app.delete('/api/logs', (req, res) => { const db = readDB(); db.logs = []; writeDB(db); res.json({ success: true, message: 'Logs cleared.' }); });

// ==========================================
// 8. NOTIFICATIONS CENTER API
// ==========================================
app.get('/api/notifications', (req, res) => { const db = readDB(); res.json({ success: true, data: db.notifications }); });
app.post('/api/notifications', validateBody(['title', 'message']), (req, res) => {
    const db = readDB();
    const n = { id: Date.now().toString(), title: req.body.title.trim(), message: req.body.message.trim(), type: req.body.type || 'info', read: false, createdAt: new Date().toISOString() };
    db.notifications.push(n); writeDB(db); res.json({ success: true, data: n });
});
app.put('/api/notifications/:id/read', (req, res) => {
    const db = readDB(); const i = db.notifications.findIndex(n => n.id === req.params.id);
    if (i !== -1) { db.notifications[i].read = true; writeDB(db); }
    res.json({ success: true });
});
app.delete('/api/notifications/:id', (req, res) => { const db = readDB(); db.notifications = db.notifications.filter(n => n.id !== req.params.id); writeDB(db); res.json({ success: true }); });

// ==========================================
// 9. REMOTE DESKTOP SESSIONS API (Synced with DB)
// ==========================================
app.get('/api/sessions', (req, res) => {
    const db = readDB();
    res.json({ success: true, data: db.sessions || [] });
});

app.post('/api/sessions', validateBody(['targetId']), (req, res) => {
    const db = readDB();
    const session = {
        id: `SES-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
        targetId: req.body.targetId.trim(),
        targetName: (req.body.targetName || 'Secondary Workstation').trim(),
        sourceId: (req.body.sourceId || 'PRIMARY-LOCAL').trim(),
        status: 'ACTIVE',
        protocol: req.body.protocol || 'Windows Remote Assistance / P2P',
        quality: req.body.quality || '1080p',
        startedAt: new Date().toISOString(),
        endedAt: null,
        durationSeconds: 0
    };
    if (!Array.isArray(db.sessions)) db.sessions = [];
    db.sessions.push(session);
    writeDB(db);
    res.json({ success: true, data: session });
});

app.put('/api/sessions/:id', (req, res) => {
    const db = readDB();
    if (!Array.isArray(db.sessions)) db.sessions = [];
    const idx = db.sessions.findIndex(s => s.id === req.params.id);
    if (idx === -1) return res.status(404).json({ success: false, error: 'Session not found.' });

    const existing = db.sessions[idx];
    const endedAt = req.body.endedAt || (req.body.status === 'TERMINATED' ? new Date().toISOString() : existing.endedAt);
    let durationSeconds = existing.durationSeconds;
    if (endedAt && existing.startedAt) {
        durationSeconds = Math.max(1, Math.round((new Date(endedAt) - new Date(existing.startedAt)) / 1000));
    }

    db.sessions[idx] = {
        ...existing,
        ...req.body,
        endedAt: endedAt,
        durationSeconds: req.body.durationSeconds || durationSeconds,
        updatedAt: new Date().toISOString()
    };
    writeDB(db);
    res.json({ success: true, data: db.sessions[idx] });
});

app.delete('/api/sessions/:id', (req, res) => {
    const db = readDB();
    if (!Array.isArray(db.sessions)) db.sessions = [];
    db.sessions = db.sessions.filter(s => s.id !== req.params.id);
    writeDB(db);
    res.json({ success: true, message: 'Session record deleted.' });
});

// ==========================================
// 10. REAL SYSTEM TELEMETRY & OS METRICS
// ==========================================
app.get('/api/system-telemetry', (req, res) => {
    const db = readDB();
    const ifaces = os.networkInterfaces();
    const netList = [];
    for (const name of Object.keys(ifaces)) {
        for (const iface of ifaces[name]) {
            if (!iface.internal) {
                netList.push({ name, family: iface.family, address: iface.address });
            }
        }
    }

    const telemetry = {
        hostname: os.hostname(),
        platform: os.platform(),
        arch: os.arch(),
        uptimeSeconds: os.uptime(),
        totalMemoryMB: Math.round(os.totalmem() / 1024 / 1024),
        freeMemoryMB: Math.round(os.freemem() / 1024 / 1024),
        memoryUsagePercent: Math.round(((os.totalmem() - os.freemem()) / os.totalmem()) * 100),
        cpuCount: os.cpus().length,
        cpuModel: os.cpus()[0]?.model || 'Standard CPU',
        interfaces: netList,
        activeSessions: (db.sessions || []).filter(s => s.status === 'ACTIVE').length,
        totalDevices: (db.devices || []).length
    };
    res.json({ success: true, data: telemetry });
});

// ==========================================
// 11. NATIVE WINDOWS REMOTE SHARING API (Zero Cost)
// ==========================================
app.post('/api/remote/launch-quick-assist', (req, res) => {
    if (os.platform() !== 'win32') {
        return res.status(400).json({ success: false, error: 'Quick Assist is only available on Windows.' });
    }
    exec('start ms-quick-assist:', (err) => {
        if (err) {
            console.error('[NATIVE REMOTE] Failed to launch Quick Assist:', err.message);
            return res.status(500).json({ success: false, error: 'Failed to launch Quick Assist: ' + err.message });
        }
        res.json({ success: true, message: 'Windows Quick Assist launched successfully.' });
    });
});

app.post('/api/remote/launch-msra', (req, res) => {
    if (os.platform() !== 'win32') {
        return res.status(400).json({ success: false, error: 'MSRA is only available on Windows.' });
    }
    exec('start msra.exe', (err) => {
        if (err) {
            console.error('[NATIVE REMOTE] Failed to launch MSRA:', err.message);
            return res.status(500).json({ success: false, error: 'Failed to launch MSRA: ' + err.message });
        }
        res.json({ success: true, message: 'Microsoft Remote Assistance launched successfully.' });
    });
});

// ==========================================
// 9. UTILITY ENDPOINTS
// ==========================================
app.get('/api/firebase-config', (req, res) => {
    res.json({ success: true, config: { apiKey: process.env.JDX_FB_API_KEY || null, projectId: process.env.JDX_FB_PROJECT_ID || null, databaseURL: process.env.JDX_FB_DB_URL || null } });
});

app.post('/api/startup', (req, res) => {
    try {
        const enable = req.body.enable;
        const appData = process.env.APPDATA || path.join(os.homedir(), 'AppData', 'Roaming');
        const startupFolder = path.join(appData, 'Microsoft', 'Windows', 'Start Menu', 'Programs', 'Startup');
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

const server = app.listen(PORT, '0.0.0.0', () => { console.log('[SUCCESS] jDroid-X-Desk Server running on http://localhost:' + PORT); });
server.on('error', (err) => { if (err.code === 'EADDRINUSE') { console.log('[INFO] Port ' + PORT + ' already in use.'); } else { console.error('[SERVER ERROR]', err.message); } });

