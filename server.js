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
const dbContext = require('./models/db-context');
const ServerController = require('./controllers/server-controller');

const app = express();
const PORT = process.env.PORT || 12345;

process.on('uncaughtException', (err) => { console.error('[SERVER ERROR] Uncaught Exception:', err.message); });
process.on('unhandledRejection', (reason) => { console.error('[SERVER ERROR] Unhandled Rejection:', reason); });

app.use(express.json());

// Encapsulated Static Route Mounts (OOPS MVC View & Asset Standard)
app.use(express.static(__dirname));
app.use('/views', express.static(path.join(__dirname, 'views')));
app.use('/controllers', express.static(path.join(__dirname, 'controllers')));
app.use('/models', express.static(path.join(__dirname, 'models')));
app.use('/services', express.static(path.join(__dirname, 'services')));
app.use('/scripts', express.static(path.join(__dirname, 'scripts')));
app.use('/styles', express.static(path.join(__dirname, 'styles')));

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

// Server Listener
const server = app.listen(PORT, '0.0.0.0', () => {
    console.log('[SUCCESS] jDroid-X-Desk Server running on http://localhost:' + PORT);
});
server.on('error', (err) => {
    if (err.code === 'EADDRINUSE') {
        console.log('[INFO] Port ' + PORT + ' already in use.');
    } else {
        console.error('[SERVER ERROR]', err.message);
    }
});

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
