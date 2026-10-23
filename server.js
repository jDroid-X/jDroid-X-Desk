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

// Canonical OOPS MVC Route Mapping (Single Source of Truth)
app.get(['/views/index.html', '/jdroid-x-desk/views/index.html'], (req, res) => res.sendFile(path.join(__dirname, 'index.html')));
app.get(['/views/admin.html', '/jdroid-x-desk/views/admin.html'], (req, res) => res.sendFile(path.join(__dirname, 'admin.html')));
app.get(['/controllers/app-controller.js', '/jdroid-x-desk/controllers/app-controller.js'], (req, res) => res.sendFile(path.join(__dirname, 'app.js')));
app.get(['/styles/styles.css', '/jdroid-x-desk/styles/styles.css'], (req, res) => res.sendFile(path.join(__dirname, 'styles.css')));

// Encapsulated Static Route Mounts (OOPS MVC View & Asset Standard)
app.use(express.static(__dirname));
app.use('/views', express.static(path.join(__dirname, 'views')));
app.use('/controllers', express.static(path.join(__dirname, 'controllers')));
app.use('/models', express.static(path.join(__dirname, 'models')));
app.use('/services', express.static(path.join(__dirname, 'services')));
app.use('/scripts', express.static(path.join(__dirname, 'scripts')));
app.use('/styles', express.static(path.join(__dirname, 'styles')));

// Gateway Health & Routes APIs (Stage 6 Verification)
app.get('/api/health', (req, res) => {
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
    res.status(200).json({
        success: true,
        name: 'jDroid-X-Desk',
        version: '2.0.0',
        author: 'jDroid-X',
        license: 'MIT',
        exeSize: exeSize,
        exeSizeKb: Math.round(exeSize / 1024),
        downloadUrl: '/api/download-exe',
        portalUrl: 'https://www.jdroid-x.com',
        customDomain: 'https://desk.jdroid-x.com',
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
