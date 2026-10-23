/**
 * jDroid-X-Desk Server Controller (controllers/server-controller.js)
 * Architecture: OOPS MVC Controller Layer
 * Encapsulates REST API routes, request validation, and business logic dispatchers
 */

const fs = require('fs');
const path = require('path');
const os = require('os');
const net = require('net');
const crypto = require('crypto');
const { exec, spawn } = require('child_process');
const http = require('http');
const HardwareMonitorService = require('../services/hardware-monitor-service');

// Autonomous In-Memory High-Speed Cloud Relay Hub (Desk 1 <--> Desk 2 AnyDesk Parity)
const relayFrames = new Map();     // targetId -> { b64, rawBytes, timestamp }
const relayInputs = new Map();     // targetId -> Array of queued input events
const relayDevices = new Map();    // targetId -> { id, name, ip, lastSeen, os }

class ServerController {
    static registerRoutes(app, dbContext) {
        const {
            readDB,
            writeDB,
            purgeOldLogs,
            deepMerge,
            DEFAULT_SETTINGS,
            DEFAULT_SERVER_CONFIG,
            DEFAULT_SCHEMA,
            DB_PATH
        } = dbContext;

        const ROOT_DIR = path.resolve(__dirname, '..');

        const validateBody = (requiredFields, maxLen = 255) => {
            return (req, res, next) => {
                if (!req.body || typeof req.body !== 'object') {
                    return res.status(400).json({ success: false, error: 'Request body must be a JSON object.' });
                }
                for (const field of requiredFields) {
                    const val = req.body[field];
                    if (!val || typeof val !== 'string' || val.trim().length === 0) {
                        return res.status(400).json({ success: false, error: `Field "${field}" is required and cannot be empty.` });
                    }
                    if (val.trim().length > maxLen) {
                        return res.status(400).json({ success: false, error: `Field "${field}" exceeds max length of ${maxLen} characters.` });
                    }
                }
                next();
            };
        };

        // ==========================================
        // 1. SAFE ZONES API
        // ==========================================
        app.get('/api/safe-zones', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.safeZones || [] });
        });

        app.post('/api/safe-zones', validateBody(['ip', 'name']), (req, res) => {
            const db = readDB();
            const z = { id: Date.now().toString(), ip: req.body.ip.trim(), name: req.body.name.trim(), addedAt: new Date().toISOString() };
            if (!Array.isArray(db.safeZones)) db.safeZones = [];
            db.safeZones.push(z);
            writeDB(db);
            res.json({ success: true, data: z });
        });

        app.put('/api/safe-zones/:id', validateBody(['ip', 'name']), (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.safeZones)) db.safeZones = [];
            const i = db.safeZones.findIndex(z => z.id === req.params.id);
            if (i === -1) return res.status(404).json({ success: false, error: 'Safe zone not found.' });
            db.safeZones[i] = { ...db.safeZones[i], ip: req.body.ip.trim(), name: req.body.name.trim(), updatedAt: new Date().toISOString() };
            writeDB(db);
            res.json({ success: true, data: db.safeZones[i] });
        });

        app.delete('/api/safe-zones/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.safeZones)) db.safeZones = [];
            db.safeZones = db.safeZones.filter(z => z.id !== req.params.id);
            writeDB(db);
            res.json({ success: true });
        });

        // ==========================================
        // 2. DEVICE GROUPS API
        // ==========================================
        app.get('/api/groups', (req, res) => {
            const db = readDB();
            const groups = Array.isArray(db.groups) ? db.groups : [];
            const devices = Array.isArray(db.devices) ? db.devices : [];
            const groupsWithMembers = groups.map(g => {
                const count = devices.filter(d => d.groupId === g.id).length;
                const desc = g.description || g.desc || '';
                return { ...g, desc, description: desc, members: count };
            });
            res.json({ success: true, data: groupsWithMembers });
        });

        app.post('/api/groups', validateBody(['name']), (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.groups)) db.groups = [];
            const g = { id: Date.now().toString(), name: req.body.name.trim(), description: (req.body.description || '').trim(), members: 0, addedAt: new Date().toISOString() };
            db.groups.push(g);
            writeDB(db);
            res.json({ success: true, data: g });
        });

        app.put('/api/groups/:id', validateBody(['name']), (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.groups)) db.groups = [];
            const i = db.groups.findIndex(g => g.id === req.params.id);
            if (i === -1) return res.status(404).json({ success: false, error: 'Group not found.' });
            db.groups[i] = { ...db.groups[i], name: req.body.name.trim(), description: (req.body.description || '').trim(), updatedAt: new Date().toISOString() };
            writeDB(db);
            res.json({ success: true, data: db.groups[i] });
        });

        app.delete('/api/groups/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.groups)) db.groups = [];
            db.groups = db.groups.filter(g => g.id !== req.params.id);
            if (Array.isArray(db.devices)) {
                db.devices = db.devices.map(d => d.groupId === req.params.id ? { ...d, groupId: null } : d);
            }
            writeDB(db);
            res.json({ success: true });
        });

        // ==========================================
        // 3. DEVICES FLEET API
        // ==========================================
        app.get('/api/devices', (req, res) => {
            const db = readDB();
            let devs = Array.isArray(db.devices) ? [...db.devices] : [];
            const groups = Array.isArray(db.groups) ? db.groups : [];
            if (req.query.groupId) {
                devs = devs.filter(d => d.groupId === req.query.groupId);
            }
            const populated = devs.map(d => {
                const grp = d.groupId ? groups.find(g => g.id === d.groupId) : null;
                return {
                    ...d,
                    ip: d.ip || '127.0.0.1',
                    group: grp ? grp.name : (d.group || 'Default'),
                    groupName: grp ? grp.name : (d.group || 'Default')
                };
            });
            res.json({ success: true, data: populated });
        });

        app.post('/api/devices', validateBody(['name']), (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.devices)) db.devices = [];
            const d = {
                id: Date.now().toString(),
                name: req.body.name.trim(),
                ip: req.body.ip ? req.body.ip.trim() : '127.0.0.1',
                groupId: req.body.groupId || null,
                status: req.body.status || 'STANDBY',
                accessLevel: req.body.accessLevel || 'Standard',
                addedAt: new Date().toISOString()
            };
            db.devices.push(d);
            writeDB(db);
            res.json({ success: true, data: d });
        });

        app.put('/api/devices/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.devices)) db.devices = [];
            const i = db.devices.findIndex(d => d.id === req.params.id);
            if (i === -1) return res.status(404).json({ success: false, error: 'Device not found.' });
            db.devices[i] = { ...db.devices[i], ...req.body, updatedAt: new Date().toISOString() };
            writeDB(db);
            res.json({ success: true, data: db.devices[i] });
        });

        app.delete('/api/devices/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.devices)) db.devices = [];
            db.devices = db.devices.filter(d => d.id !== req.params.id);
            writeDB(db);
            res.json({ success: true });
        });

        // ==========================================
        // 4. GUARDIANS & CONTACTS
        // ==========================================
        app.get('/api/guardians', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.guardians || [] });
        });

        app.post('/api/guardians', validateBody(['name', 'role', 'contact']), (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.guardians)) db.guardians = [];
            const g = { id: Date.now().toString(), name: req.body.name.trim(), role: req.body.role.trim(), contact: req.body.contact.trim(), status: 'Active', addedAt: new Date().toISOString() };
            db.guardians.push(g);
            writeDB(db);
            res.json({ success: true, data: g });
        });

        app.put('/api/guardians/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.guardians)) db.guardians = [];
            const i = db.guardians.findIndex(g => g.id === req.params.id);
            if (i === -1) return res.status(404).json({ success: false, error: 'Guardian not found.' });
            db.guardians[i] = { ...db.guardians[i], ...req.body, updatedAt: new Date().toISOString() };
            writeDB(db);
            res.json({ success: true, data: db.guardians[i] });
        });

        app.delete('/api/guardians/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.guardians)) db.guardians = [];
            db.guardians = db.guardians.filter(g => g.id !== req.params.id);
            writeDB(db);
            res.json({ success: true });
        });

        app.get('/api/contacts', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.contacts || [] });
        });

        app.post('/api/contacts', validateBody(['name', 'remoteId']), (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.contacts)) db.contacts = [];
            const c = { id: Date.now().toString(), name: req.body.name.trim(), remoteId: req.body.remoteId.trim(), groupId: req.body.groupId || null, addedAt: new Date().toISOString() };
            db.contacts.push(c);
            writeDB(db);
            res.json({ success: true, data: c });
        });

        app.put('/api/contacts/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.contacts)) db.contacts = [];
            const i = db.contacts.findIndex(c => c.id === req.params.id);
            if (i === -1) return res.status(404).json({ success: false, error: 'Contact not found.' });
            db.contacts[i] = { ...db.contacts[i], ...req.body, updatedAt: new Date().toISOString() };
            writeDB(db);
            res.json({ success: true, data: db.contacts[i] });
        });

        app.delete('/api/contacts/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.contacts)) db.contacts = [];
            db.contacts = db.contacts.filter(c => c.id !== req.params.id);
            writeDB(db);
            res.json({ success: true });
        });

        // ==========================================
        // 5. SYSTEM SETTINGS API
        // ==========================================
        app.get('/api/settings', (req, res) => {
            const db = readDB();
            try {
                const tunnelPath = path.join(ROOT_DIR, 'colab_tunnel.txt');
                if (fs.existsSync(tunnelPath)) {
                    const tunnelUrl = fs.readFileSync(tunnelPath, 'utf-8').trim();
                    if (tunnelUrl && tunnelUrl.includes('.trycloudflare.com')) {
                        db.settings = db.settings || {};
                        db.settings.connection = db.settings.connection || {};
                        db.settings.connection.relayServer = tunnelUrl;
                        db.settings.connection.colabRelayUrl = tunnelUrl;
                    }
                }
            } catch (_) {}
            res.json({ success: true, data: db.settings });
        });

        const saveSettingsHandler = (req, res) => {
            const db = readDB();
            db.settings = deepMerge(db.settings, req.body);
            db.settings.updatedAt = new Date().toISOString();
            if (req.body && req.body.connection && req.body.connection.relayServer) {
                try {
                    const rUrl = req.body.connection.relayServer.trim();
                    if (rUrl.includes('.trycloudflare.com')) {
                        fs.writeFileSync(path.join(ROOT_DIR, 'colab_tunnel.txt'), rUrl, 'utf-8');
                    }
                } catch (_) {}
            }
            writeDB(db);
            res.json({ success: true, data: db.settings, message: 'System Settings Saved Successfully' });
        };
        app.put('/api/settings', saveSettingsHandler);
        app.post('/api/settings', saveSettingsHandler);

        // ==========================================
        // 6. LOGS & AUDIT API
        // ==========================================
        app.get('/api/logs', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.logs || [] });
        });

        app.post('/api/logs', (req, res) => {
            const db = readDB();
            const log = {
                id: Date.now().toString(),
                level: req.body.level || req.body.status || 'INFO',
                stage: req.body.stage || 'GENERAL',
                msg: req.body.msg || req.body.details || req.body.action || '',
                timestamp: new Date().toISOString()
            };
            if (!Array.isArray(db.logs)) db.logs = [];
            db.logs.unshift(log);
            writeDB(db);
            res.json({ success: true, data: log });
        });

        app.delete('/api/logs', (req, res) => {
            const db = readDB();
            db.logs = [];
            writeDB(db);
            res.json({ success: true, message: 'Logs cleared.' });
        });

        // ==========================================
        // 7. NOTIFICATIONS API
        // ==========================================
        app.get('/api/notifications', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.notifications || [] });
        });

        app.post('/api/notifications', validateBody(['title', 'message']), (req, res) => {
            const db = readDB();
            const notif = {
                id: Date.now().toString(),
                title: req.body.title.trim(),
                message: req.body.message.trim(),
                type: req.body.type || 'info',
                read: false,
                timestamp: new Date().toISOString()
            };
            if (!Array.isArray(db.notifications)) db.notifications = [];
            db.notifications.unshift(notif);
            writeDB(db);
            res.json({ success: true, data: notif });
        });

        app.put('/api/notifications/read-all', (req, res) => {
            const db = readDB();
            if (Array.isArray(db.notifications)) {
                db.notifications = db.notifications.map(n => ({ ...n, read: true }));
            }
            writeDB(db);
            res.json({ success: true, count: (db.notifications || []).length });
        });

        app.put('/api/notifications/:id/read', (req, res) => {
            const db = readDB();
            if (Array.isArray(db.notifications)) {
                const n = db.notifications.find(item => item.id === req.params.id);
                if (n) n.read = true;
            }
            writeDB(db);
            res.json({ success: true });
        });

        app.delete('/api/notifications/:id', (req, res) => {
            const db = readDB();
            if (Array.isArray(db.notifications)) {
                db.notifications = db.notifications.filter(n => n.id !== req.params.id);
            }
            writeDB(db);
            res.json({ success: true });
        });

        // ==========================================
        // 8. SESSIONS & INVITES API
        // ==========================================
        app.get('/api/sessions', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.sessions || [] });
        });

        app.post('/api/sessions', (req, res) => {
            const targetId = req.body.targetId || req.body.remoteId;
            if (!targetId || typeof targetId !== 'string' || targetId.trim().length === 0) {
                return res.status(400).json({ success: false, error: 'Field "targetId" or "remoteId" is required.' });
            }
            const db = readDB();
            const session = {
                id: `SES-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                targetId: targetId.trim(),
                remoteId: targetId.trim(),
                targetName: (req.body.targetName || req.body.alias || 'Secondary Workstation').trim(),
                sourceId: (req.body.sourceId || 'PRIMARY-LOCAL').trim(),
                status: 'ACTIVE',
                protocol: req.body.protocol || 'Windows Remote Assistance / P2P',
                quality: req.body.quality || '1080p',
                bandwidth: '0 KB/s',
                fps: 60,
                startedAt: new Date().toISOString(),
                connectedAt: new Date().toISOString(),
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
            if (endedAt && (existing.startedAt || existing.connectedAt)) {
                const startTime = new Date(existing.startedAt || existing.connectedAt);
                durationSeconds = Math.max(1, Math.round((new Date(endedAt) - startTime) / 1000));
            }
            const mins = Math.floor(durationSeconds / 60);
            const secs = durationSeconds % 60;
            const durationStr = `${mins}m ${secs}s`;

            db.sessions[idx] = {
                ...existing,
                ...req.body,
                endedAt: endedAt || null,
                durationSeconds,
                duration: durationStr
            };
            writeDB(db);
            res.json({ success: true, data: db.sessions[idx] });
        });

        app.delete('/api/sessions/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.sessions)) db.sessions = [];
            db.sessions = db.sessions.filter(s => s.id !== req.params.id);
            writeDB(db);
            res.json({ success: true });
        });

        app.get('/api/invites', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.invites || [] });
        });

        app.post('/api/invites', validateBody(['targetId']), (req, res) => {
            const db = readDB();
            const inv = {
                id: `INV-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                targetId: req.body.targetId.trim(),
                sourceId: req.body.sourceId || 'PRIMARY-LOCAL',
                permissions: req.body.permissions || { screen: true, keyboard: true, clipboard: true },
                status: 'PENDING',
                createdAt: new Date().toISOString()
            };
            if (!Array.isArray(db.invites)) db.invites = [];
            db.invites.push(inv);
            writeDB(db);
            res.json({ success: true, data: inv });
        });

        app.get('/api/invites/pending/:targetId', (req, res) => {
            const db = readDB();
            const cleanParam = (req.params.targetId || '').replace(/\D/g, '');
            const invs = (db.invites || []).filter(i => {
                if (i.status !== 'PENDING') return false;
                if (i.targetId === req.params.targetId) return true;
                const cleanTgt = (i.targetId || '').replace(/\D/g, '');
                return (cleanParam.length > 0 && cleanTgt.length > 0 && cleanParam === cleanTgt);
            });
            res.json({ success: true, data: invs });
        });

        app.get('/api/invites/:id', (req, res) => {
            const db = readDB();
            const inv = (db.invites || []).find(i => i.id === req.params.id);
            if (!inv) return res.status(404).json({ success: false, error: 'Invitation not found.' });
            const session = (db.sessions || []).find(s => s.targetId === inv.targetId && s.sourceId === inv.sourceId && s.status === 'ACTIVE');
            res.json({ success: true, data: inv, session: session || null });
        });

        app.put('/api/invites/:id/respond', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.invites)) db.invites = [];
            const idx = db.invites.findIndex(inv => inv.id === req.params.id);
            if (idx === -1) return res.status(404).json({ success: false, error: 'Invitation not found.' });

            const status = req.body.status === 'ACCEPTED' ? 'ACCEPTED' : 'REJECTED';
            db.invites[idx].status = status;
            db.invites[idx].respondedAt = new Date().toISOString();

            let session = null;
            if (status === 'ACCEPTED') {
                if (!Array.isArray(db.sessions)) db.sessions = [];
                session = {
                    id: `SES-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                    targetId: db.invites[idx].targetId,
                    remoteId: db.invites[idx].targetId,
                    sourceId: db.invites[idx].sourceId,
                    status: 'ACTIVE',
                    protocol: 'P2P jDroid-X Relay / Colab Cloud Mesh',
                    permissions: db.invites[idx].permissions,
                    startedAt: new Date().toISOString(),
                    connectedAt: new Date().toISOString(),
                    endedAt: null,
                    durationSeconds: 0
                };
                db.sessions.push(session);
            }

            writeDB(db);
            res.json({ success: true, data: db.invites[idx], session });
        });

        // ==========================================
        // 8.5 DISCOVERED DESKS (LAN ZERO-CONFIG MESH) API
        // ==========================================
        const fallbackDiscoveredDesks = (res, db) => {
            const devices = Array.isArray(db.devices) ? db.devices : [];
            const localPort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const localDesks = devices.map(d => ({
                deskId: d.id,
                formattedDeskId: (d.id && d.id.length === 9) ? `${d.id.slice(0,3)}-${d.id.slice(3,6)}-${d.id.slice(6,9)}` : d.id,
                machineName: d.name || 'Remote Desk',
                endpoint: d.ip ? `http://${d.ip}:${localPort}` : '',
                ip: d.ip || '',
                port: localPort,
                tunnelUrl: '',
                isOnline: d.status === 'ONLINE' || d.status === 'STANDBY',
                lastSeen: new Date().toISOString()
            }));
            res.json({ success: true, data: localDesks, count: localDesks.length, source: 'database_fleet' });
        };

        app.get('/api/discovered-desks', (req, res) => {
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            
            const httpReq = http.get(`http://127.0.0.1:${nativePort}/api/discovered-desks`, { timeout: 350 }, (nRes) => {
                let data = '';
                nRes.on('data', chunk => data += chunk);
                nRes.on('end', () => {
                    try {
                        const parsed = JSON.parse(data);
                        if (parsed && Array.isArray(parsed.desks)) {
                            return res.json({ success: true, data: parsed.desks, count: parsed.desks.length, source: 'native_lan_engine' });
                        }
                    } catch (_) {}
                    fallbackDiscoveredDesks(res, db);
                });
            });

            httpReq.on('error', () => fallbackDiscoveredDesks(res, db));
            httpReq.on('timeout', () => {
                httpReq.destroy();
                fallbackDiscoveredDesks(res, db);
            });
        });

        app.post('/api/discovered-desks/probe', (req, res) => {
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const targetId = (req.body && req.body.targetId) ? req.body.targetId : '*';
            
            const postData = JSON.stringify({ targetId });
            const pReq = http.request(`http://127.0.0.1:${nativePort}/api/discovered-desks/probe`, {
                method: 'POST',
                timeout: 500,
                headers: {
                    'Content-Type': 'application/json',
                    'Content-Length': Buffer.byteLength(postData)
                }
            }, (pRes) => {
                res.json({ success: true, probed: true });
            });
            pReq.on('error', () => res.json({ success: true, probed: true, note: 'Native probe dispatched' }));
            pReq.write(postData);
            pReq.end();
        });

        // ==========================================
        // 8.5 AUTONOMOUS IN-MEMORY CLOUD RELAY & WAN P2P HUB
        // ==========================================
        // High-Speed RAW Binary Stream Push (0 Base64, 0 JSON overhead - 60 FPS)
        app.post(['/api/relay/push_bin', '/api/relay/push_raw'], (req, res) => {
            const targetId = req.query.targetId || req.headers['x-target-id'] || 'DEFAULT';
            let rawBytes = null;

            if (Buffer.isBuffer(req.body) && req.body.length > 0) {
                rawBytes = req.body;
            } else if (req.body && typeof req.body === 'object') {
                const b64 = req.body.frame || '';
                if (b64) {
                    const clean = b64.replace(/^data:image\/\w+;base64,/, '');
                    try { rawBytes = Buffer.from(clean, 'base64'); } catch (_) {}
                }
            }

            if (rawBytes && rawBytes.length > 0) {
                relayFrames.set(targetId, {
                    rawBytes,
                    b64: rawBytes.toString('base64'),
                    timestamp: Date.now()
                });
                relayDevices.set(targetId, { id: targetId, ip: req.ip, lastSeen: Date.now() });
                return res.status(200).json({ success: true, targetId, size: rawBytes.length, mode: 'BINARY_STREAM' });
            }

            res.status(400).json({ success: false, error: 'Empty or invalid binary frame payload' });
        });

        // JSON Base64 Stream Push
        app.post(['/api/relay/push_frame', '/api/relay/push'], (req, res) => {
            const targetId = (req.body && req.body.targetId) || req.query.targetId || req.headers['x-target-id'] || 'DEFAULT';
            const b64 = (req.body && req.body.frame) || '';
            let rawBytes = null;

            if (b64) {
                const clean = b64.replace(/^data:image\/\w+;base64,/, '');
                try { rawBytes = Buffer.from(clean, 'base64'); } catch (_) {}
            }

            relayFrames.set(targetId, {
                b64,
                rawBytes,
                timestamp: Date.now()
            });
            relayDevices.set(targetId, { id: targetId, ip: req.ip, lastSeen: Date.now() });

            // Forward to local native relay if active
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const postData = JSON.stringify(req.body || {});
            const pReq = http.request(`http://127.0.0.1:${nativePort}/api/relay/push_frame`, {
                method: 'POST',
                timeout: 500,
                headers: {
                    'Content-Type': 'application/json',
                    'Content-Length': Buffer.byteLength(postData)
                }
            }, () => {});
            pReq.on('error', () => {});
            pReq.write(postData);
            pReq.end();

            res.status(200).json({ success: true, targetId, timestamp: Date.now() });
        });

        // Binary Frame Stream Pull (Fastest 60 FPS viewport render)
        app.get(['/api/relay/frame_bin', '/api/relay/pull_bin/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const cached = relayFrames.get(targetId);

            if (cached && cached.rawBytes && (Date.now() - cached.timestamp < 30000)) {
                res.setHeader('Content-Type', 'image/jpeg');
                res.setHeader('Cache-Control', 'no-cache, no-store, must-revalidate');
                res.setHeader('X-Frame-Timestamp', cached.timestamp.toString());
                return res.status(200).end(cached.rawBytes);
            }

            // Fallback proxy to native port if not in cloud relay memory
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const proxyReq = http.get(`http://127.0.0.1:${nativePort}/api/relay/frame_bin?targetId=${encodeURIComponent(targetId)}`, { timeout: 1000 }, (pRes) => {
                if (pRes.statusCode === 200) {
                    res.setHeader('Content-Type', pRes.headers['content-type'] || 'image/jpeg');
                    res.setHeader('Cache-Control', 'no-cache, no-store, must-revalidate');
                    pRes.pipe(res);
                } else {
                    res.status(204).end();
                }
            });
            proxyReq.on('error', () => res.status(204).end());
            proxyReq.on('timeout', () => { proxyReq.destroy(); res.status(204).end(); });
        });

        // JSON Frame Stream Pull
        app.get(['/api/relay/frame', '/api/relay/pull_frame/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const cached = relayFrames.get(targetId);

            if (cached && cached.b64 && (Date.now() - cached.timestamp < 30000)) {
                return res.status(200).json({
                    success: true,
                    targetId,
                    frame: cached.b64.startsWith('data:') ? cached.b64 : `data:image/jpeg;base64,${cached.b64}`,
                    timestamp: cached.timestamp
                });
            }

            // Fallback proxy to native port
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const proxyReq = http.get(`http://127.0.0.1:${nativePort}/api/relay/frame?targetId=${encodeURIComponent(targetId)}`, { timeout: 1000 }, (pRes) => {
                let data = '';
                pRes.on('data', chunk => data += chunk);
                pRes.on('end', () => {
                    try {
                        res.json(JSON.parse(data));
                    } catch (_) {
                        res.json({ success: true, frame: '' });
                    }
                });
            });
            proxyReq.on('error', () => res.json({ success: true, frame: '', note: 'Awaiting host stream' }));
            proxyReq.on('timeout', () => { proxyReq.destroy(); res.json({ success: true, frame: '' }); });
        });

        // Remote Input Push (Mouse / Keyboard Events from Viewer)
        app.post(['/api/relay/push_input', '/api/relay/input'], (req, res) => {
            const targetId = (req.body && req.body.targetId) || req.query.targetId || 'DEFAULT';
            if (!relayInputs.has(targetId)) relayInputs.set(targetId, []);
            const queue = relayInputs.get(targetId);
            queue.push({ ...req.body, receivedAt: Date.now() });
            if (queue.length > 100) queue.shift();

            // Also proxy to native port if host is local
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const postData = JSON.stringify(req.body || {});
            const pReq = http.request(`http://127.0.0.1:${nativePort}/api/relay/push_input`, {
                method: 'POST',
                timeout: 500,
                headers: {
                    'Content-Type': 'application/json',
                    'Content-Length': Buffer.byteLength(postData)
                }
            }, () => {});
            pReq.on('error', () => {});
            pReq.write(postData);
            pReq.end();

            res.status(200).json({ success: true, queued: true, targetId });
        });

        // Remote Session Teardown & Disconnect Signal (Invalidates stale frame buffers)
        app.post(['/api/relay/disconnect', '/api/relay/session/close'], (req, res) => {
            const targetId = (req.body && req.body.targetId) || req.query.targetId || 'DEFAULT';
            const reason = (req.body && req.body.reason) || 'Connection closed by remote desk';

            // Invalidate cached frames to avoid frozen screen artifacts
            relayFrames.delete(targetId);
            relayBinaryFrames.delete(targetId);

            // Queue session_close event for the target desk
            if (!relayInputs.has(targetId)) relayInputs.set(targetId, []);
            relayInputs.get(targetId).push({
                type: 'session_close',
                reason: reason,
                targetId: targetId,
                receivedAt: Date.now()
            });

            // Proxy teardown signal to native relay port if local
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const postData = JSON.stringify({ targetId, reason, type: 'session_close' });
            const pReq = http.request(`http://127.0.0.1:${nativePort}/api/relay/disconnect`, {
                method: 'POST',
                timeout: 500,
                headers: {
                    'Content-Type': 'application/json',
                    'Content-Length': Buffer.byteLength(postData)
                }
            }, () => {});
            pReq.on('error', () => {});
            pReq.write(postData);
            pReq.end();

            res.status(200).json({ success: true, disconnected: true, targetId, message: 'Session closed and frame cache cleared.' });
        });

        // Remote Input Pull (Host consumes Viewer mouse/keyboard inputs)
        app.get(['/api/relay/input', '/api/relay/pull_input/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            if (relayInputs.has(targetId) && relayInputs.get(targetId).length > 0) {
                const events = relayInputs.get(targetId);
                relayInputs.set(targetId, []);
                return res.status(200).json({ success: true, targetId, events });
            }

            // Fallback proxy to native port
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const pReq = http.get(`http://127.0.0.1:${nativePort}/api/relay/input?targetId=${encodeURIComponent(targetId)}`, { timeout: 600 }, (pRes) => {
                let data = '';
                pRes.on('data', chunk => data += chunk);
                pRes.on('end', () => {
                    try { res.json(JSON.parse(data)); } catch (_) { res.json({ success: true, events: [] }); }
                });
            });
            pReq.on('error', () => res.json({ success: true, events: [] }));
        });

        // Register Active Desk Device
        app.post('/api/relay/register', (req, res) => {
            const { targetId, id, name, os } = req.body || {};
            const devId = targetId || id || (req.ip ? req.ip.replace('::ffff:', '') : 'UNKNOWN');
            relayDevices.set(devId, {
                id: devId,
                name: name || devId,
                ip: (req.headers['x-forwarded-for'] || req.ip || '').replace('::ffff:', ''),
                lastSeen: Date.now(),
                os: os || 'Windows'
            });
            res.status(200).json({ success: true, registered: true, id: devId, timestamp: new Date().toISOString() });
        });

        // Online Relay Desks List
        app.get(['/api/relay/devices', '/api/relay/discovered'], (req, res) => {
            const now = Date.now();
            const active = [];
            for (const [id, dev] of relayDevices.entries()) {
                if (now - dev.lastSeen < 60000) {
                    active.push(dev);
                }
            }
            res.status(200).json({ success: true, count: active.length, data: active });
        });

        // Relay Health & Telemetry Status
        app.get(['/api/relay/status', '/api/relay/ping'], (req, res) => {
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            res.status(200).json({
                success: true,
                status: 'ONLINE',
                engine: 'NodeRelayHub',
                hubMode: 'AUTONOMOUS_IN_MEMORY',
                activeDevices: relayDevices.size,
                cachedFrames: relayFrames.size,
                nativePort,
                timestamp: new Date().toISOString()
            });
        });

        // ==========================================
        // 9. TELEMETRY & HARDWARE ID
        // ==========================================
        app.get('/api/system-telemetry', (req, res) => {
            const db = readDB();
            const telemetry = HardwareMonitorService.getSystemTelemetry(db);
            res.json({ success: true, data: telemetry });
        });

        app.get('/api/hardware-id', (req, res) => {
            try {
                const ifaces = os.networkInterfaces();
                let mac = '00:00:00:00:00:00';
                for (const n of Object.keys(ifaces)) {
                    for (const i of ifaces[n]) {
                        if (!i.internal && i.mac !== '00:00:00:00:00:00') {
                            mac = i.mac;
                            break;
                        }
                    }
                    if (mac !== '00:00:00:00:00:00') break;
                }
                const hash = crypto.createHash('md5').update(os.hostname() + '-' + mac).digest('hex').toUpperCase();
                const devId = 'JDX-' + hash.substring(0, 4) + '-' + hash.substring(4, 8);
                res.json({ success: true, deviceId: devId, hardwareId: devId });
            } catch (err) {
                res.json({ success: true, deviceId: 'JDX-RAND-0000', hardwareId: 'JDX-RAND-0000' });
            }
        });

        // ==========================================
        // 10. REMOTE APPLICATION LAUNCHERS
        // ==========================================
        app.post('/api/remote/launch-quick-assist', (req, res) => {
            if (os.platform() !== 'win32') {
                return res.status(400).json({ success: false, error: 'Quick Assist is only available on Windows.' });
            }
            exec('start ms-quick-assist:', (err) => {
                if (err) return res.status(500).json({ success: false, error: err.message });
                res.json({ success: true, message: 'Microsoft Quick Assist launched.' });
            });
        });

        app.post('/api/remote/launch-terminal', (req, res) => {
            if (os.platform() !== 'win32') {
                return res.status(400).json({ success: false, error: 'Windows Terminal launcher is only available on Windows.' });
            }
            exec('start wt.exe || start powershell.exe', (err) => {
                if (err) return res.status(500).json({ success: false, error: err.message });
                res.json({ success: true, message: 'Windows Terminal launched.' });
            });
        });

        app.post('/api/remote/launch-msra', (req, res) => {
            if (os.platform() !== 'win32') {
                return res.status(400).json({ success: false, error: 'MSRA is only available on Windows.' });
            }
            exec('start msra.exe /getassistance', (err) => {
                if (err) return res.status(500).json({ success: false, error: err.message });
                res.json({ success: true, message: 'Microsoft Remote Assistance launched.' });
            });
        });

        // ==========================================
        // 11. DOWNLOADS API
        // ==========================================
        app.get(['/download', '/download/exe', '/download/native', '/api/download/exe', '/api/download-exe'], (req, res) => {
            const exeFile = path.join(ROOT_DIR, 'jDroid-X-Desk.exe');
            if (fs.existsSync(exeFile)) {
                res.download(exeFile, 'jDroid-X-Desk.exe');
            } else {
                res.status(404).json({ success: false, error: 'Executable not found on server.' });
            }
        });

        app.get(['/download/bat', '/download/client', '/download/share', '/api/download-client', '/api/download/client', '/api/download/share'], (req, res) => {
            const clientFile = fs.existsSync(path.join(ROOT_DIR, 'jDroid-X-Client.bat'))
                ? path.join(ROOT_DIR, 'jDroid-X-Client.bat')
                : (fs.existsSync(path.join(ROOT_DIR, 'jDroid-X-Share.bat'))
                    ? path.join(ROOT_DIR, 'jDroid-X-Share.bat')
                    : path.join(ROOT_DIR, 'jDroid-Client.bat'));
            if (fs.existsSync(clientFile)) {
                res.download(clientFile, path.basename(clientFile));
            } else {
                res.status(404).json({ success: false, error: 'Client installer not found.' });
            }
        });

        // ==========================================
        // 12. UTILITY ENDPOINTS
        // ==========================================
        app.get('/api/firebase-config', (req, res) => {
            res.json({
                success: true,
                config: {
                    apiKey: process.env.JDX_FB_API_KEY || null,
                    projectId: process.env.JDX_FB_PROJECT_ID || null,
                    databaseURL: process.env.JDX_FB_DB_URL || null
                }
            });
        });

        app.post('/api/startup', (req, res) => {
            try {
                const enable = req.body.enable;
                const appData = process.env.APPDATA || path.join(os.homedir(), 'AppData', 'Roaming');
                const startupFolder = path.join(appData, 'Microsoft', 'Windows', 'Start Menu', 'Programs', 'Startup');
                const shortcutPath = path.join(startupFolder, 'jDroid-X-Desk.lnk');
                const targetPath = path.join(ROOT_DIR, 'start_jdroid_app.vbs');
                if (enable) {
                    const vbs = `\nSet oWS = WScript.CreateObject("WScript.Shell")\nsLinkFile = "${shortcutPath.replace(/\\/g, '\\\\')}"\nSet oLink = oWS.CreateShortcut(sLinkFile)\noLink.TargetPath = "wscript.exe"\noLink.Arguments = """${targetPath.replace(/\\/g, '\\\\')}"""\noLink.WorkingDirectory = "${ROOT_DIR.replace(/\\/g, '\\\\')}"\noLink.Description = "jDroid-X-Desk Remote Desktop Console"\noLink.Save\n`;
                    const tmp = path.join(ROOT_DIR, 'temp_startup_shortcut.vbs');
                    try { fs.writeFileSync(tmp, vbs); } catch (we) { return res.status(500).json({ success: false, error: 'Failed to write VBS script. Permission denied.' }); }
                    exec('cscript //nologo "' + tmp + '"', (err) => {
                        if (fs.existsSync(tmp)) fs.unlinkSync(tmp);
                        if (err) return res.status(500).json({ success: false, error: err.message });
                        res.json({ success: true, message: 'Startup enabled' });
                    });
                } else {
                    if (fs.existsSync(shortcutPath)) fs.unlinkSync(shortcutPath);
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

        // ==========================================
        // 13. SERVER ADMIN & ENGINE MANAGEMENT
        // ==========================================
        app.get(['/admin', '/admin.html'], (req, res) => {
            const adminPath = path.join(ROOT_DIR, 'admin.html');
            if (fs.existsSync(adminPath)) {
                res.sendFile(adminPath);
            } else {
                res.redirect('/#server-admin');
            }
        });

        app.get('/api/admin/config', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.serverConfig || DEFAULT_SERVER_CONFIG });
        });

        app.put('/api/admin/config', (req, res) => {
            const db = readDB();
            if (!db.serverConfig) db.serverConfig = JSON.parse(JSON.stringify(DEFAULT_SERVER_CONFIG));
            if (req.body.processingLimits) {
                db.serverConfig.processingLimits = { ...db.serverConfig.processingLimits, ...req.body.processingLimits };
            }
            if (req.body.engines) {
                db.serverConfig.engines = { ...db.serverConfig.engines, ...req.body.engines };
            }
            if (req.body.activeEngine) {
                db.serverConfig.activeEngine = req.body.activeEngine;
            }
            writeDB(db);
            res.json({ success: true, data: db.serverConfig });
        });

        app.post('/api/admin/engine/switch', (req, res) => {
            const db = readDB();
            let targetEngine = req.body.engine || req.body.engineId;
            if (targetEngine === 'googleColab' || targetEngine === 'colab') targetEngine = 'jdroidRelay';
            if (!targetEngine || !['rustdesk', 'sunshine', 'guacamole', 'jdroidRelay'].includes(targetEngine)) {
                return res.status(400).json({ success: false, error: 'Invalid engine requested.' });
            }
            if (!db.serverConfig) db.serverConfig = JSON.parse(JSON.stringify(DEFAULT_SERVER_CONFIG));
            db.serverConfig.activeEngine = targetEngine;

            const clientIp = (req.headers['x-forwarded-for'] || req.socket.remoteAddress || '127.0.0.1').replace('::ffff:', '');
            const engineName = db.serverConfig.engines?.[targetEngine]?.name || targetEngine;
            const logEntry = {
                id: `ENG-${Date.now()}`,
                level: 'AUDIT',
                stage: 'ENGINE_SWITCH',
                method: 'POST',
                url: '/api/admin/engine/switch',
                ip: clientIp,
                statusCode: 200,
                rttMs: 1,
                msg: `[jDroid-Engine] Switched active streaming engine to: ${engineName}`,
                timestamp: new Date().toISOString()
            };
            if (!Array.isArray(db.logs)) db.logs = [];
            db.logs.push(logEntry);
            purgeOldLogs(db);
            writeDB(db);
            res.json({ success: true, activeEngine: targetEngine, engineInfo: db.serverConfig.engines?.[targetEngine] });
        });

        app.get('/api/admin/engine/probe', (req, res) => {
            const db = readDB();
            let curEngine = req.query.engine || (db.serverConfig ? db.serverConfig.activeEngine : 'sunshine');
            if (curEngine === 'googleColab' || curEngine === 'colab') curEngine = 'jdroidRelay';

            const enginePortMap = {
                sunshine: 47990,
                rustdesk: 21116,
                guacamole: 8080,
                jdroidRelay: 8088
            };

            const port = enginePortMap[curEngine] || 47990;
            const client = new net.Socket();
            let isListening = false;
            let responded = false;

            const sendResponse = () => {
                if (responded) return;
                responded = true;
                try { client.destroy(); } catch (_) {}
                res.json({
                    success: true,
                    engine: curEngine,
                    port: port,
                    isRunning: isListening,
                    statusBadge: isListening ? `RUNNING (PORT ${port})` : `PORT ${port} UNREACHABLE`,
                    scriptPath: `scripts/start-${curEngine === 'jdroidRelay' ? 'google-colab' : curEngine}.bat`
                });
            };

            client.setTimeout(450);
            client.connect(port, '127.0.0.1', () => {
                isListening = true;
                sendResponse();
            });

            client.on('error', () => { sendResponse(); });
            client.on('timeout', () => { sendResponse(); });
            client.on('close', () => { sendResponse(); });
        });

        app.post('/api/admin/engine/launch-script', (req, res) => {
            const engine = req.body.engine || req.body.engineId || 'sunshine';
            const scriptMap = {
                sunshine: 'start-sunshine.bat',
                rustdesk: 'start-rustdesk.bat',
                guacamole: 'start-guacamole.bat',
                jdroidRelay: 'start-google-colab.bat',
                googleColab: 'start-google-colab.bat'
            };
            const batFile = scriptMap[engine] || 'start-sunshine.bat';
            const scriptPath = path.join(ROOT_DIR, 'scripts', batFile);

            if (fs.existsSync(scriptPath)) {
                try {
                    const child = spawn('cmd.exe', ['/c', 'start', '""', scriptPath], {
                        cwd: ROOT_DIR,
                        detached: true,
                        stdio: 'ignore',
                        windowsHide: false
                    });
                    child.unref();
                    res.json({ success: true, script: batFile, message: `Launcher started: ${batFile}` });
                } catch (err) {
                    res.status(500).json({ success: false, error: err.message });
                }
            } else {
                res.status(404).json({ success: false, error: `Script not found: ${batFile}` });
            }
        });

        app.get('/api/admin/logs', (req, res) => {
            const db = readDB();
            let logs = Array.isArray(db.logs) ? [...db.logs] : [];
            if (req.query.level && req.query.level !== 'ALL') {
                logs = logs.filter(l => l.level === req.query.level);
            }
            if (req.query.q) {
                const query = req.query.q.toLowerCase();
                logs = logs.filter(l => (l.msg || '').toLowerCase().includes(query) || (l.stage || '').toLowerCase().includes(query) || (l.ip || '').includes(query));
            }
            const limit = parseInt(req.query.limit) || 100;
            logs = logs.reverse().slice(0, limit);
            res.json({ success: true, count: logs.length, total: (db.logs || []).length, data: logs });
        });

        app.delete('/api/admin/logs', (req, res) => {
            const db = readDB();
            const count = (db.logs || []).length;
            db.logs = [];
            writeDB(db);
            res.json({ success: true, cleared: count, message: 'Processing logs purged.' });
        });

        app.get('/api/admin/metrics', (req, res) => {
            const db = readDB();
            const metrics = HardwareMonitorService.getAdminMetrics(db, DEFAULT_SERVER_CONFIG.processingLimits);
            res.json({ success: true, data: metrics });
        });
    }
}

module.exports = ServerController;
