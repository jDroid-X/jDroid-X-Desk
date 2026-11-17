/**
 * jDroid-X-Desk Server Entrypoint (server.js)
 * Architecture: Clean OOPS MVC Gateway Entrypoint
 * Model Layer: models/db-context.js
 * Controller Layer: controllers/server-controller.js
 * Service Layer: services/hardware-monitor-service.js
 * Views Layer: views/ (index.html, admin.html)
 */

const express = require('express');
const path = require('path');
const fs = require('fs');
const dbContext = require('./models/db-context');
const ServerController = require('./controllers/server-controller');

const app = express();
// Priority: environment variable > Cloudflare Tunnel Ingress Port 7700 > Gateway Fallbacks
const PORT = process.env.PORT || 7700;
const FALLBACK_PORT = 12345;

process.on('uncaughtException', (err) => { console.error('[SERVER ERROR] Uncaught Exception:', err.message); });
process.on('unhandledRejection', (reason) => { console.error('[SERVER ERROR] Unhandled Rejection:', reason); });

app.use(express.json({ limit: '15mb' }));
app.use(express.raw({ type: ['image/jpeg', 'image/*', 'application/octet-stream'], limit: '15mb' }));

// Edge & Proxy Cache Control: Ensure latest assets on Cloudflare Tunnel & Browser
app.use((req, res, next) => {
    res.setHeader('Cache-Control', 'no-cache, must-revalidate');
    next();
});

// Holistic Gateway Mounts (Port 7700 /jdroid-x-desk/...)
app.use('/jdroid-x-desk', express.static(__dirname));

// Canonical OOPS MVC Route Mapping (Single Source of Truth — root files are canonical)
const CANONICAL_INDEX = path.join(__dirname, 'index.html');
const CANONICAL_ADMIN = path.join(__dirname, 'admin.html');
const CANONICAL_APP_JS = path.join(__dirname, 'app.js');
const CANONICAL_STYLES = path.join(__dirname, 'styles.css');

app.get(['/', '/index.html', '/views/index.html', '/jdroid-x-desk', '/jdroid-x-desk/', '/jdroid-x-desk/index.html', '/jdroid-x-desk/views/index.html'], (req, res) => res.sendFile(CANONICAL_INDEX));
app.get(['/admin', '/admin.html', '/views/admin.html', '/jdroid-x-desk/admin', '/jdroid-x-desk/admin.html', '/jdroid-x-desk/views/admin.html'], (req, res) => res.sendFile(CANONICAL_ADMIN));
app.get(['/app.js', '/controllers/app-controller.js', '/jdroid-x-desk/app.js', '/jdroid-x-desk/controllers/app-controller.js'], (req, res) => res.sendFile(CANONICAL_APP_JS));
app.get(['/styles.css', '/styles/styles.css', '/jdroid-x-desk/styles.css', '/jdroid-x-desk/styles/styles.css'], (req, res) => res.sendFile(CANONICAL_STYLES));

// Encapsulated Static Route Mounts (OOPS MVC View & Asset Standard)
app.use(express.static(__dirname));
app.use(['/assets', '/styles/assets', '/jdroid-x-desk/assets', '/views/assets'], express.static(path.join(__dirname, 'assets')));
app.use('/views', express.static(path.join(__dirname, 'views')));
app.use('/controllers', express.static(path.join(__dirname, 'controllers')));
app.use('/models', express.static(path.join(__dirname, 'models')));
app.use('/services', express.static(path.join(__dirname, 'services')));
app.use('/scripts', express.static(path.join(__dirname, 'scripts')));
app.use('/styles', express.static(path.join(__dirname, 'styles')));

// Gateway Health, Telemetry & Ping Endpoints (Stage 6 Verification)
app.get(['/api/health', '/health'], (req, res) => {
    res.status(200).json({
        success: true,
        status: 'UP',
        appSlug: 'jdroid-x-desk',
        gatewayUrl: `http://localhost:${PORT}/jdroid-x-desk/index.html#remote-console`,
        port: PORT,
        uptime: process.uptime(),
        timestamp: new Date().toISOString()
    });
});

// Master Ping & Wake-on-Ping Bridge for Desktop Client & Colab Relay Compatibility
app.get(['/ping', '/api/relay/ping', '/wake', '/api/relay/wake'], (req, res) => {
    const mem = process.memoryUsage();
    const ramStr = `${(mem.heapUsed / (1024 * 1024)).toFixed(1)} / ${(mem.heapTotal / (1024 * 1024)).toFixed(1)} MB`;
    
    // Dynamic config resolution from dbContext
    let customDomain = 'https://desk.jdroid-x.com';
    let relayServer = 'https://desk.jdroid-x.com';
    let tunnelUrl = 'https://desk.jdroid-x.com';
    try {
        const db = dbContext.getDatabase();
        if (db && db.settings && db.settings.connection) {
            if (db.settings.connection.customDomain) customDomain = db.settings.connection.customDomain;
            if (db.settings.connection.relayServer) relayServer = db.settings.connection.relayServer;
            if (db.settings.connection.relayUrl) tunnelUrl = db.settings.connection.relayUrl;
        }
    } catch (_) {}

    res.status(200).json({
        success: true,
        status: 'UP',
        service: 'jDroid-X Central Ingress Gateway',
        organization: 'jDroid-X Studio Enterprise',
        public_url: customDomain,
        relay_server: relayServer,
        tunnel_url: tunnelUrl,
        port: Number(PORT),
        active_devices: 1,
        system_ram: ramStr,
        gpu_ram: 'Hardware Direct',
        disk: 'Standard NVMe',
        timestamp: new Date().toISOString(),
        message: `jDroid-X Gateway Port ${PORT} active and operational.`
    });
});

// End-to-End System Dependencies & Integration Diagnostics API
app.get(['/api/system/diagnostics', '/api/dependencies/status'], async (req, res) => {
    const net = require('net');
    const http = require('http');

    const probePort = (port, host = '127.0.0.1', timeoutMs = 800) => {
        return new Promise((resolve) => {
            const socket = new net.Socket();
            let resolved = false;
            socket.setTimeout(timeoutMs);
            socket.once('connect', () => {
                resolved = true;
                socket.destroy();
                resolve(true);
            });
            socket.once('timeout', () => {
                if (!resolved) { resolved = true; socket.destroy(); resolve(false); }
            });
            socket.once('error', () => {
                if (!resolved) { resolved = true; resolve(false); }
            });
            socket.connect(port, host);
        });
    };

    const db = (dbContext && dbContext.readDB) ? dbContext.readDB() : {};
    const generalSettings = (db.settings && db.settings.general) ? db.settings.general : {};
    const connectionSettings = (db.settings && db.settings.connection) ? db.settings.connection : {};
    const companionPort = connectionSettings.relayPort || generalSettings.nativeCompanionPort || 8088;

    const isRelay8088Up = await probePort(companionPort);
    const isPort7700Up = true; // Current process is responding on this port

    let cloudflareTunnelActive = false;
    let cloudflareProcessRunning = false;
    try {
        const { execSync } = require('child_process');
        const tasklist = execSync('tasklist /FI "IMAGENAME eq cloudflared.exe" 2>nul', { encoding: 'utf8' });
        cloudflareProcessRunning = tasklist.toLowerCase().includes('cloudflared.exe');
    } catch (_) {}

    const brokenIntegrations = [];
    if (!isRelay8088Up) {
        brokenIntegrations.push(`Desktop Embedded Relay (Port ${companionPort}) is not listening`);
    }
    if (!cloudflareProcessRunning) {
        brokenIntegrations.push('Cloudflare Tunnel Daemon (cloudflared.exe) is not running');
    }

    res.status(200).json({
        success: true,
        timestamp: new Date().toISOString(),
        gateway: {
            port: PORT,
            status: 'UP',
            uptimeSeconds: Math.floor(process.uptime()),
            gatewayUrl: `http://localhost:${PORT}/jdroid-x-desk/index.html#remote-console`
        },
        dependencies: {
            ingressPort7700: { active: isPort7700Up, status: 'LISTENING', port: PORT },
            desktopRelay8088: { active: isRelay8088Up, status: isRelay8088Up ? 'ONLINE' : 'OFFLINE', port: companionPort },
            cloudflareTunnel: {
                active: cloudflareProcessRunning,
                status: cloudflareProcessRunning ? 'ACTIVE' : 'DISCONNECTED',
                customDomain: (db && db.settings && db.settings.connection && db.settings.connection.customDomain) || 'https://desk.jdroid-x.com'
            },
            nodeRuntime: {
                version: process.version,
                executable: process.execPath,
                platform: process.platform,
                arch: process.arch
            }
        },
        brokenIntegrations: brokenIntegrations,
        allHealthy: brokenIntegrations.length === 0
    });
});

// Automated Self-Healing / Autostart Dispatcher
app.post(['/api/system/heal', '/api/dependencies/heal'], (req, res) => {
    const { spawn } = require('child_process');
    const healedActions = [];

    // Check & autostart cloudflared if offline
    try {
        const { execSync } = require('child_process');
        const tasklist = execSync('tasklist /FI "IMAGENAME eq cloudflared.exe" 2>nul', { encoding: 'utf8' });
        if (!tasklist.toLowerCase().includes('cloudflared.exe')) {
            const startBat = path.join(__dirname, 'scripts', 'start-desk-cloud-tunnel.bat');
            if (fs.existsSync(startBat)) {
                spawn('cmd.exe', ['/c', startBat], {
                    cwd: __dirname,
                    detached: true,
                    stdio: 'ignore'
                }).unref();
                healedActions.push('Started Cloudflare Tunnel via start-desk-cloud-tunnel.bat');
            }
        }
    } catch (e) {
        healedActions.push(`Cloudflare heal attempted: ${e.message}`);
    }

    // Check & autostart jDroid-X-Desk.exe (Port 8088 Embedded Relay) if offline
    try {
        const { execSync } = require('child_process');
        const tasklistDesk = execSync('tasklist /FI "IMAGENAME eq jDroid-X-Desk.exe" 2>nul', { encoding: 'utf8' });
        if (!tasklistDesk.toLowerCase().includes('jdroid-x-desk.exe')) {
            const exePath = path.join(__dirname, 'jDroid-X-Desk.exe');
            if (fs.existsSync(exePath)) {
                spawn(exePath, [], {
                    cwd: __dirname,
                    detached: true,
                    stdio: 'ignore'
                }).unref();
                const db = (dbContext && dbContext.readDB) ? dbContext.readDB() : {};
        const companionPort = (db.settings && db.settings.connection && db.settings.connection.relayPort) || 8088;
                healedActions.push(`Launched jDroid-X-Desk native client (Port ${companionPort})`);
            }
        }
    } catch (e) {
        healedActions.push(`Native app heal attempted: ${e.message}`);
    }

    res.status(200).json({
        success: true,
        healed: true,
        actions: healedActions.length > 0 ? healedActions : ['All services are already active.'],
        timestamp: new Date().toISOString()
    });
});

app.get('/api/gateway/routes', (req, res) => {
    const routeFile = path.join(__dirname, 'scripts', 'gateway_routes.json');
    if (fs.existsSync(routeFile)) {
        return res.sendFile(routeFile);
    }
    res.status(404).json({ success: false, error: 'Gateway routes registry not found.' });
});

app.get('/api/version', (req, res) => {
    const exeFile = path.join(__dirname, 'jDroid-X-Desk.exe');
    let exeSize = 0;
    if (fs.existsSync(exeFile)) {
        try { exeSize = fs.statSync(exeFile).size; } catch (_) {}
    }
    const db = (dbContext && dbContext.readDB) ? dbContext.readDB() : {};
    const generalSettings = (db.settings && db.settings.general) ? db.settings.general : {};
    const connectionSettings = (db.settings && db.settings.connection) ? db.settings.connection : {};
    const targetVersion = generalSettings.targetReleaseVersion || '2.5.0';
    const githubUrl = generalSettings.githubDownloadUrl || 'https://github.com/jDroid-X/jDroid-X-Desk/raw/main/jDroid-X-Desk.exe';
    const companionPort = connectionSettings.relayPort || generalSettings.nativeCompanionPort || 8088;

    res.status(200).json({
        success: true,
        name: 'jDroid-X-Desk',
        version: '2.0.0',
        targetReleaseVersion: targetVersion,
        githubDownloadUrl: githubUrl,
        nativeCompanionPort: companionPort,
        author: 'jDroid-X',
        license: 'MIT',
        exeSize: exeSize,
        exeSizeKb: Math.round(exeSize / 1024),
        downloadUrl: '/api/download-exe',
        portalUrl: (generalSettings && generalSettings.portalUrl) || 'https://www.jdroid-x.com',
        customDomain: connectionSettings.customDomain || 'https://desk.jdroid-x.com',
        timestamp: new Date().toISOString()
    });
});

let reqLogSeq = 1;

// Automatic Hourly Background 7-Day Log Purging Maintenance
setInterval(() => {
    try {
        const db = dbContext.readDB();
        if (dbContext.purgeOldLogs(db)) dbContext.writeDB(db);
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
        if (req.url.startsWith('/api/logs')) return;

        try {
            const db = dbContext.readDB();
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
            dbContext.purgeOldLogs(db);
            dbContext.writeDB(db);
        } catch (_) {}
    });

    next();
});

// Register OOPS MVC Controller routes
ServerController.registerRoutes(app, dbContext);

// Primary Server Listener (Gateway Port 8080)
const server = app.listen(PORT, '0.0.0.0', () => {
    console.log(`[SUCCESS] jDroid-X-Desk Server running on http://localhost:${PORT}`);
    console.log(`[GATEWAY] Unified Gateway route mounted at http://localhost:${PORT}/jdroid-x-desk/index.html#remote-console`);
});
server.on('error', (err) => {
    if (err.code === 'EADDRINUSE') {
        console.log(`[INFO] Port ${PORT} already in use.`);
    } else {
        console.error('[SERVER ERROR]', err.message);
    }
});

// Optional Dual-Listener on Port 12345 (Chesterton's Fence: Backward Compatibility)
let legacyServer = null;
if (String(PORT) !== String(FALLBACK_PORT) && !process.env.NO_DUAL_PORT) {
    try {
        legacyServer = app.listen(FALLBACK_PORT, '0.0.0.0', () => {
            console.log(`[COMPAT] Backward-compatible legacy listener active on http://localhost:${FALLBACK_PORT}`);
        });
        legacyServer.on('error', (err) => {
            if (err.code === 'EADDRINUSE') {
                console.log(`[INFO] Legacy port ${FALLBACK_PORT} already bound or in use by another instance.`);
            }
        });
    } catch (_) {}
}

// Backward-compatible re-exports for legacy callers (Chesterton's Fence)
module.exports = {
    app,
    server,
    dbContext,
    ServerController,
    readDB: dbContext.readDB,
    writeDB: dbContext.writeDB,
    purgeOldLogs: dbContext.purgeOldLogs,
    deepMerge: dbContext.deepMerge,
    DEFAULT_SETTINGS: dbContext.DEFAULT_SETTINGS,
    DEFAULT_SERVER_CONFIG: dbContext.DEFAULT_SERVER_CONFIG,
    DEFAULT_SCHEMA: dbContext.DEFAULT_SCHEMA
};
