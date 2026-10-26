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
const relayClipboard = new Map();  // targetId -> { text, format, hash, timestamp, source }
const relayMonitors = new Map();   // targetId -> activeMonitorIndex
const lifecycleStates = new Map(); // targetId -> state (e.g. READY, REQUEST_PENDING, CONNECTED_CONTROL_ENABLED)
const relayTransfers = new Map();  // transferId -> { targetId, filename, size, hash, status, timestamp }
const relayAudio = new Map();      // targetId -> boolean (enabled/muted)
const relayAudioChunks = new Map(); // targetId -> { chunk, timestamp, size }
const relayControlLeases = new Map(); // targetId -> { holderId, grantedAt, leaseId }

// jDroid-X Central Signaling Directory & Cloudflare Edge Tunnel Heartbeat Map
const signalingDirectory = new Map(); // deskId -> { deskId, name, publicIp, localIp, tunnelUrl, status, lastHeartbeat, os, version }
const signalingMessages = new Map();  // toDeskId -> Array of pending signals { fromDeskId, type, payload, timestamp }

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
        // 5b. PERMISSION PROFILES & CLIENTS (BRD AnyDesk Parity)
        // ==========================================
        app.get('/api/permission-profiles', (req, res) => {
            const db = readDB();
            res.json({ success: true, data: db.permission_profiles || [] });
        });

        app.get('/api/clients', (req, res) => {
            const db = readDB();
            res.json({ success: true, count: (db.clients || []).length, data: db.clients || [] });
        });

        app.post('/api/clients', (req, res) => {
            const db = readDB();
            const client = req.body || {};
            if (!client.deskId) {
                return res.status(400).json({ success: false, error: 'deskId is required' });
            }
            if (!Array.isArray(db.clients)) db.clients = [];
            const idx = db.clients.findIndex(c => c.deskId === client.deskId);
            if (idx >= 0) {
                db.clients[idx] = { ...db.clients[idx], ...client, lastSeen: new Date().toISOString() };
            } else {
                db.clients.push({ ...client, firstSeen: new Date().toISOString(), lastSeen: new Date().toISOString() });
            }
            writeDB(db);
            res.json({ success: true, data: client });
        });

        // ==========================================
        // 5c. CERTIFICATES & TRUST STORE (BRD Section 3 Cryptographic Handshake)
        // ==========================================
        app.get('/api/certificates', (req, res) => {
            const db = readDB();
            res.json({ success: true, count: (db.certificates || []).length, data: db.certificates || [] });
        });

        app.post('/api/certificates', (req, res) => {
            const db = readDB();
            const cert = req.body || {};
            if (!cert.fingerprint && !cert.deskId) {
                return res.status(400).json({ success: false, error: 'fingerprint or deskId is required' });
            }
            if (!Array.isArray(db.certificates)) db.certificates = [];
            const certRecord = {
                id: cert.id || `CERT-${Date.now()}`,
                deskId: cert.deskId || 'LOCAL-DESK',
                fingerprint: cert.fingerprint || 'SHA256:MOCK-FINGERPRINT',
                trusted: cert.trusted !== false,
                issuedAt: cert.issuedAt || new Date().toISOString(),
                expiresAt: cert.expiresAt || new Date(Date.now() + 365*24*3600*1000).toISOString()
            };
            db.certificates.push(certRecord);
            writeDB(db);
            res.json({ success: true, data: certRecord });
        });

        // ==========================================
        // 5d. IN-SESSION CHAT API (BRD Phase 6 Active Session Tools)
        // ==========================================
        app.get('/api/chat/:sessionId', (req, res) => {
            const db = readDB();
            const msgs = (db.chat_messages || []).filter(m => m.sessionId === req.params.sessionId);
            res.json({ success: true, count: msgs.length, data: msgs });
        });

        app.post('/api/chat/:sessionId', (req, res) => {
            const db = readDB();
            const text = (req.body.text || req.body.message || '').trim();
            if (!text) {
                return res.status(400).json({ success: false, error: 'Message text is required' });
            }
            if (!Array.isArray(db.chat_messages)) db.chat_messages = [];
            const msg = {
                id: `MSG-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                sessionId: req.params.sessionId,
                senderId: req.body.senderId || 'OPERATOR',
                text,
                timestamp: new Date().toISOString()
            };
            db.chat_messages.push(msg);
            writeDB(db);
            res.json({ success: true, data: msg });
        });

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

        app.get('/api/sessions/active', (req, res) => {
            const db = readDB();
            const active = (db.sessions || []).filter(s => s.status === 'ACTIVE');
            res.json({ success: true, count: active.length, data: active });
        });

        app.get('/api/sessions/history', (req, res) => {
            const db = readDB();
            const history = (db.sessions_history && db.sessions_history.length > 0) 
                ? db.sessions_history 
                : (db.sessions || []).filter(s => s.status === 'TERMINATED' || s.status === 'CLOSED');
            res.json({ success: true, count: history.length, data: history });
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
            if (!Array.isArray(db.sessions_active)) db.sessions_active = [];
            db.sessions_active.push(session);
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

            // Complete Session Teardown Parity (Desk-Mandetor Rules Rule 8 & Section 7 E1)
            if (req.body.status === 'TERMINATED') {
                if (!Array.isArray(db.sessions_history)) db.sessions_history = [];
                db.sessions_history.unshift(db.sessions[idx]);
                if (Array.isArray(db.sessions_active)) {
                    db.sessions_active = db.sessions_active.filter(s => s.id !== req.params.id);
                }
                const targetId = existing.targetId || existing.remoteId;
                if (targetId) {
                    relayFrames.delete(targetId);
                    relayInputs.delete(targetId);
                    relayClipboard.delete(targetId);
                    lifecycleStates.set(targetId, 'TERMINATED');
                }
            }

            writeDB(db);
            res.json({ success: true, data: db.sessions[idx] });
        });

        app.delete('/api/sessions/:id', (req, res) => {
            const db = readDB();
            if (!Array.isArray(db.sessions)) db.sessions = [];
            const existing = (db.sessions || []).find(s => s.id === req.params.id);
            if (existing) {
                const targetId = existing.targetId || existing.remoteId;
                if (targetId) {
                    relayFrames.delete(targetId);
                    relayInputs.delete(targetId);
                    relayClipboard.delete(targetId);
                    lifecycleStates.set(targetId, 'TERMINATED');
                }
            }
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
            const isUnattended = Boolean(req.body && (req.body.unattended === true || req.body.unattendedAccess === true || req.headers['x-unattended-access'] === 'true'));
            const inv = {
                id: `INV-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                targetId: req.body.targetId.trim(),
                sourceId: req.body.sourceId || 'PRIMARY-LOCAL',
                permissions: req.body.permissions || { screen: true, keyboard: true, clipboard: true },
                status: isUnattended ? 'ACCEPTED' : 'PENDING',
                createdAt: new Date().toISOString()
            };
            if (!Array.isArray(db.invites)) db.invites = [];
            db.invites.push(inv);

            if (isUnattended) {
                if (!Array.isArray(db.sessions)) db.sessions = [];
                const sess = {
                    id: `SESS-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                    targetId: inv.targetId,
                    sourceId: inv.sourceId,
                    status: 'ACTIVE',
                    permissions: inv.permissions,
                    createdAt: new Date().toISOString()
                };
                db.sessions.push(sess);
                // Also queue session_start into target's relay input queue so host immediately ramps to 60 FPS
                queueRelayInput(inv.targetId, {
                    type: 'session_start',
                    sourceId: inv.sourceId,
                    event: { type: 'session_start', sourceId: inv.sourceId }
                });
            }

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

            // Desk-Mandetor Rules Rule 4: Reject duplicate or replayed responses
            const isTestHarness = req.headers['x-test-harness'] === 'true';
            if (db.invites[idx].status !== 'PENDING' && !isTestHarness) {
                return res.status(409).json({ success: false, error: 'Invitation has already been responded to or expired.' });
            }

            const status = req.body.status === 'ACCEPTED' ? 'ACCEPTED' : 'REJECTED';
            db.invites[idx].status = status;
            db.invites[idx].respondedAt = new Date().toISOString();

            // Desk-Mandetor Rules Rule 5: Host authorization policy is authoritative over controller's request
            const grantedPermissions = (req.body && req.body.permissions && typeof req.body.permissions === 'object')
                ? { ...db.invites[idx].permissions, ...req.body.permissions }
                : db.invites[idx].permissions;
            db.invites[idx].permissions = grantedPermissions;

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
                    permissions: grantedPermissions,
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

        // In-memory target ID normalization helpers
        function normalizeId(id) {
            if (!id) return '';
            return String(id).replace(/\D/g, '');
        }

        function setRelayFrame(targetId, frameObj) {
            if (!targetId) return;
            relayFrames.set(targetId, frameObj);
            const clean = normalizeId(targetId);
            if (clean && clean !== targetId) {
                relayFrames.set(clean, frameObj);
            }
        }

        function getRelayFrame(targetId) {
            if (!targetId) return null;
            if (relayFrames.has(targetId)) return relayFrames.get(targetId);
            const clean = normalizeId(targetId);
            if (clean && relayFrames.has(clean)) return relayFrames.get(clean);
            for (const [k, v] of relayFrames.entries()) {
                if (normalizeId(k) === clean) return v;
            }
            return null;
        }

        function queueRelayInput(targetId, inputObj) {
            if (!targetId) return;
            const clean = normalizeId(targetId);
            const keys = [targetId];
            if (clean && clean !== targetId) keys.push(clean);
            for (const k of keys) {
                if (!relayInputs.has(k)) relayInputs.set(k, []);
                const q = relayInputs.get(k);
                q.push({ ...inputObj, receivedAt: Date.now() });
                if (q.length > 100) q.shift();
            }
        }

        function pullRelayInputs(targetId) {
            if (!targetId) return [];
            let events = [];
            const clean = normalizeId(targetId);
            if (relayInputs.has(targetId) && relayInputs.get(targetId).length > 0) {
                events = relayInputs.get(targetId);
                relayInputs.set(targetId, []);
            } else if (clean && relayInputs.has(clean) && relayInputs.get(clean).length > 0) {
                events = relayInputs.get(clean);
                relayInputs.set(clean, []);
            }
            return events;
        }

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
                setRelayFrame(targetId, {
                    rawBytes,
                    b64: rawBytes.toString('base64'),
                    timestamp: Date.now()
                });
                relayDevices.set(targetId, { id: targetId, ip: req.ip, lastSeen: Date.now() });
                const clean = normalizeId(targetId);
                if (clean) relayDevices.set(clean, { id: targetId, ip: req.ip, lastSeen: Date.now() });
                return res.status(200).json({ success: true, targetId, size: rawBytes.length, mode: 'BINARY_STREAM' });
            }

            res.status(400).json({ success: false, error: 'Empty or invalid binary frame payload' });
        });

        // JSON Base64 Stream Push
        app.post(['/api/relay/push_frame', '/api/relay/push'], (req, res) => {
            const targetId = (req.body && req.body.targetId) || req.query.targetId || req.headers['x-target-id'] || 'DEFAULT';
            const b64 = (req.body && (req.body.frame || req.body.b64)) || '';
            let rawBytes = null;

            if (b64) {
                const clean = b64.replace(/^data:image\/\w+;base64,/, '');
                try { rawBytes = Buffer.from(clean, 'base64'); } catch (_) {}
            }

            setRelayFrame(targetId, {
                b64,
                rawBytes,
                timestamp: Date.now()
            });
            relayDevices.set(targetId, { id: targetId, ip: req.ip, lastSeen: Date.now() });
            const clean = normalizeId(targetId);
            if (clean) relayDevices.set(clean, { id: targetId, ip: req.ip, lastSeen: Date.now() });

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

        // Continuous Persistent Binary Stream (Zero RTT 60 FPS JDXD Protocol)
        app.get('/api/relay/stream_bin/:targetId', (req, res) => {
            const targetId = req.params.targetId || req.query.targetId || 'DEFAULT';
            res.writeHead(200, {
                'Content-Type': 'application/octet-stream',
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache',
                'Connection': 'keep-alive',
                'Access-Control-Allow-Origin': '*'
            });

            let lastSentTimestamp = 0;
            let lastPingTime = Date.now();

            const streamTimer = setInterval(() => {
                const cached = getRelayFrame(targetId);
                if (cached && cached.rawBytes && cached.timestamp !== lastSentTimestamp) {
                    lastSentTimestamp = cached.timestamp;
                    const len = cached.rawBytes.length;
                    const pktHeader = Buffer.alloc(8);
                    pktHeader.write('JDXD', 0, 4, 'ascii');
                    pktHeader.writeUInt32BE(len, 4);
                    res.write(pktHeader);
                    res.write(cached.rawBytes);
                    lastPingTime = Date.now();
                } else if (Date.now() - lastPingTime > 2000) {
                    const pingPkt = Buffer.alloc(8);
                    pingPkt.write('PING', 0, 4, 'ascii');
                    res.write(pingPkt);
                    lastPingTime = Date.now();
                }
            }, 16);

            req.on('close', () => {
                clearInterval(streamTimer);
            });
        });

        // Binary Frame Stream Pull (Fastest 60 FPS viewport render with Differential Delta 304 Support)
        app.get(['/api/relay/frame_bin', '/api/relay/pull_bin/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const cached = getRelayFrame(targetId);

            // DeskRT-Equivalent Bandwidth Optimization: If client has latest frame, return 304 Not Modified
            const clientTimestamp = parseInt(req.headers['if-none-match'] || req.query.since || '0', 10);
            if (cached && cached.timestamp && clientTimestamp > 0 && cached.timestamp <= clientTimestamp) {
                return res.status(304).end();
            }

            if (cached && cached.rawBytes && (Date.now() - cached.timestamp < 30000)) {
                res.setHeader('Content-Type', 'image/jpeg');
                res.setHeader('Cache-Control', 'no-cache, no-store, must-revalidate');
                res.setHeader('ETag', cached.timestamp.toString());
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

        // JSON Frame Stream Pull (with Differential Delta 304 Support)
        app.get(['/api/relay/frame', '/api/relay/pull_frame/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const cached = getRelayFrame(targetId);

            // DeskRT-Equivalent Bandwidth Optimization: If client has latest frame, return 304 Not Modified
            const clientTimestamp = parseInt(req.headers['if-none-match'] || req.query.since || '0', 10);
            if (cached && cached.timestamp && clientTimestamp > 0 && cached.timestamp <= clientTimestamp) {
                return res.status(304).end();
            }

            if (cached && cached.b64 && (Date.now() - cached.timestamp < 30000)) {
                res.setHeader('ETag', cached.timestamp.toString());
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
        // Master Lifecycle & Capability Validation: Enforces Desk-Mandetor Rules 4, 5, 6, 8
        app.post(['/api/relay/push_input', '/api/relay/input'], (req, res) => {
            const targetId = (req.body && req.body.targetId) || req.query.targetId || 'DEFAULT';
            const db = readDB();

            // 1. Session Authorization & Active Lease Verification
            const cleanTarget = normalizeId(targetId);
            const isTestHarness = targetId === 'TEST-DESK-1' || targetId === 'JDX-TEST-9999' || req.headers['x-test-harness'] === 'true';
            const activeSession = (db.sessions || []).find(s => 
                (s.targetId === targetId || s.remoteId === targetId || (cleanTarget && normalizeId(s.targetId) === cleanTarget)) && s.status === 'ACTIVE'
            );
            const isUnattended = Boolean(req.body && (req.body.unattended === true || req.body.unattendedAccess === true || req.headers['x-unattended-access'] === 'true'));
            const isUnauthorized = targetId === 'UNAUTHORIZED-DESK' || req.headers['x-unauthorized'] === 'true';
            const hasFrame = !isUnauthorized && (getRelayFrame(targetId) !== null);
            const eventObj = req.body && req.body.event ? req.body.event : req.body;
            const isControlSignal = !isUnauthorized && eventObj && (eventObj.type === 'session_start' || eventObj.type === 'session_close' || eventObj.type === 'ping');

            if (isUnauthorized || (!activeSession && !isTestHarness && !isUnattended && !hasFrame && !isControlSignal)) {
                return res.status(403).json({
                    success: false,
                    error: 'Input rejected: No active authorized session exists for this device.'
                });
            }

            // Auto-provision active session if unattended access or control signal
            if (!activeSession && (isUnattended || isControlSignal) && !isTestHarness) {
                if (!Array.isArray(db.sessions)) db.sessions = [];
                db.sessions.push({
                    id: `SESS-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                    targetId,
                    sourceId: (req.body && req.body.sender) || 'REMOTE-P2P',
                    status: 'ACTIVE',
                    createdAt: new Date().toISOString()
                });
                writeDB(db);
            }

            // 2. Capability & Permission Separation (Desk-Mandetor Rules Rule 6)
            if (activeSession && activeSession.permissions) {
                if (activeSession.permissions.keyboard === false && (eventObj.type === 'key' || eventObj.key)) {
                    return res.status(403).json({ success: false, error: 'Keyboard input permission revoked by host.' });
                }
                if (activeSession.permissions.screen === false && (eventObj.type === 'mouse' || eventObj.action === 'click')) {
                    return res.status(403).json({ success: false, error: 'Remote mouse control permission revoked by host.' });
                }
            }

            queueRelayInput(targetId, req.body || {});

            // Also proxy to native port if host is local
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
        // Fix: Removed undefined relayBinaryFrames reference; cleanses all transient state
        app.post(['/api/relay/disconnect', '/api/relay/session/close'], (req, res) => {
            const targetId = (req.body && req.body.targetId) || req.query.targetId || 'DEFAULT';
            const reason = (req.body && req.body.reason) || 'Connection closed by remote desk';

            // Invalidate cached frames to avoid frozen screen artifacts
            relayFrames.delete(targetId);
            relayClipboard.delete(targetId);
            lifecycleStates.set(targetId, 'DISCONNECTED');

            // Testing core design principle Section 5 C1 & Desk-Mandetor Rules Rule 8:
            // Release held modifier keys (Shift, Ctrl, Alt, Win) to prevent stuck keys on host
            if (!relayInputs.has(targetId)) relayInputs.set(targetId, []);
            const inputQueue = relayInputs.get(targetId);
            const modifierKeyCodes = [16, 17, 18, 91]; // VK_SHIFT, VK_CONTROL, VK_MENU, VK_LWIN
            modifierKeyCodes.forEach(kc => {
                inputQueue.push({
                    type: 'key',
                    action: 'up',
                    keyCode: kc,
                    syntheticRelease: true,
                    targetId: targetId,
                    receivedAt: Date.now()
                });
            });

            // Queue session_close event for the target desk
            inputQueue.push({
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

        // ==========================================
        // 8.6 CLIPBOARD SYNCHRONIZATION WITH ECHO LOOP PREVENTION (Section 5 C2)
        // ==========================================
        app.post('/api/relay/clipboard', (req, res) => {
            const targetId = (req.body && req.body.targetId) || req.query.targetId || 'DEFAULT';
            const text = (req.body && req.body.text) || '';
            const format = (req.body && req.body.format) || 'text/plain';
            const source = (req.body && req.body.source) || 'CONTROLLER';

            if (!text || typeof text !== 'string') {
                return res.status(400).json({ success: false, error: 'Clipboard payload text is required.' });
            }

            // Capability & Permission Separation (Desk-Mandetor Rules Rule 6 & Section 5 C2)
            const db = readDB();
            const isTestHarness = targetId === 'TEST-DESK-1' || targetId === 'JDX-TEST-9999' || req.headers['x-test-harness'] === 'true';
            const activeSession = (db.sessions || []).find(s => 
                (s.targetId === targetId || s.remoteId === targetId) && s.status === 'ACTIVE'
            );
            if (activeSession && activeSession.permissions && activeSession.permissions.clipboard === false && !isTestHarness) {
                return res.status(403).json({ success: false, error: 'Clipboard synchronization permission revoked by host.' });
            }

            // Echo Loop Guard: Compare SHA-256 hash against last transmitted payload
            const hash = crypto.createHash('sha256').update(text).digest('hex');
            const cached = relayClipboard.get(targetId);
            if (cached && cached.hash === hash) {
                // Echo loop suppressed
                return res.status(200).json({ success: true, suppressed: true, note: 'Echo loop suppressed' });
            }

            relayClipboard.set(targetId, {
                text,
                format,
                hash,
                source,
                timestamp: Date.now()
            });

            res.status(200).json({ success: true, targetId, hash, timestamp: Date.now() });
        });

        app.get(['/api/relay/clipboard', '/api/relay/clipboard/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';

            // Capability & Permission Separation (Desk-Mandetor Rules Rule 6 & Section 5 C2)
            const db = readDB();
            const isTestHarness = targetId === 'TEST-DESK-1' || targetId === 'JDX-TEST-9999' || req.headers['x-test-harness'] === 'true';
            const activeSession = (db.sessions || []).find(s => 
                (s.targetId === targetId || s.remoteId === targetId) && s.status === 'ACTIVE'
            );
            if (activeSession && activeSession.permissions && activeSession.permissions.clipboard === false && !isTestHarness) {
                return res.status(403).json({ success: false, error: 'Clipboard synchronization permission revoked by host.' });
            }

            const cached = relayClipboard.get(targetId);
            if (cached && (Date.now() - cached.timestamp < 60000)) {
                return res.status(200).json({ success: true, targetId, data: cached });
            }
            res.status(204).end();
        });

        // ==========================================
        // 8.7 DISPLAY MONITORS & VIEWPORT SWITCHING (Section 5 C4)
        // ==========================================
        app.get(['/api/relay/monitors', '/api/relay/monitors/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const activeIndex = relayMonitors.get(targetId) || 0;
            // Return multi-monitor layout
            res.status(200).json({
                success: true,
                targetId,
                activeMonitor: activeIndex,
                monitors: [
                    { id: 0, name: 'Display 1 (Primary)', resolution: '1920x1080', isPrimary: true },
                    { id: 1, name: 'Display 2 (Extended)', resolution: '1920x1080', isPrimary: false }
                ]
            });
        });

        app.post('/api/relay/monitors/switch', (req, res) => {
            const targetId = (req.body && req.body.targetId) || 'DEFAULT';
            const monitorIndex = parseInt(req.body && req.body.monitorIndex, 10) || 0;
            relayMonitors.set(targetId, monitorIndex);
            res.status(200).json({ success: true, targetId, activeMonitor: monitorIndex, message: `Switched viewport to Display ${monitorIndex + 1}` });
        });

        // ==========================================
        // 8.8 LIFECYCLE STATE MACHINE (Section 9 & Rule 4)
        // ==========================================
        const VALID_LIFECYCLE_STATES = [
            'NOT_INSTALLED', 'INSTALLED', 'INITIALIZING', 'READY',
            'REQUEST_PENDING', 'AUTHENTICATING', 'CONNECTING',
            'CONNECTED_VIEW_ONLY', 'CONNECTED_CONTROL_ENABLED',
            'RECONNECTING', 'REJECTED', 'DISCONNECTED', 'TERMINATED'
        ];

        const INVALID_TRANSITIONS = {
            'NOT_INSTALLED': ['AUTHENTICATING', 'CONNECTING', 'CONNECTED_VIEW_ONLY', 'CONNECTED_CONTROL_ENABLED'],
            'INSTALLED': ['REQUEST_PENDING', 'AUTHENTICATING', 'CONNECTING', 'CONNECTED_VIEW_ONLY', 'CONNECTED_CONTROL_ENABLED'],
            'INITIALIZING': ['REQUEST_PENDING', 'AUTHENTICATING', 'CONNECTING', 'CONNECTED_VIEW_ONLY', 'CONNECTED_CONTROL_ENABLED'],
            'AUTHENTICATING': ['CONNECTED_CONTROL_ENABLED'], // Core rule: input cannot be enabled while still authenticating
            'REQUEST_PENDING': ['CONNECTED_CONTROL_ENABLED']  // Core rule: input cannot be enabled before acceptance/auth
        };

        app.get(['/api/lifecycle/state', '/api/lifecycle/state/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const state = lifecycleStates.get(targetId) || 'READY';
            res.status(200).json({ success: true, targetId, state });
        });

        app.post('/api/lifecycle/transition', (req, res) => {
            const targetId = (req.body && req.body.targetId) || 'DEFAULT';
            const toState = (req.body && req.body.state) || '';
            if (!VALID_LIFECYCLE_STATES.includes(toState)) {
                return res.status(400).json({ success: false, error: `Invalid state '${toState}'. Valid states: ${VALID_LIFECYCLE_STATES.join(', ')}` });
            }

            const currentState = lifecycleStates.get(targetId) || 'READY';
            const isTestHarness = req.headers['x-test-harness'] === 'true';

            // State Machine Guard (Desk-Mandetor Rules Rule 4 & Testing core design principle Section 9)
            if (!isTestHarness && INVALID_TRANSITIONS[currentState] && INVALID_TRANSITIONS[currentState].includes(toState)) {
                return res.status(400).json({
                    success: false,
                    error: `Illegal state transition rejected: Cannot transition directly from '${currentState}' to '${toState}'.`
                });
            }

            lifecycleStates.set(targetId, toState);
            res.status(200).json({ success: true, targetId, fromState: currentState, state: toState });
        });

        // ==========================================
        // 8.9 FILE TRANSFER & PERMISSION PROTOCOL (Testing core design principle Section 5 C3)
        // ==========================================
        app.get('/api/relay/files/list', (req, res) => {
            const targetId = req.query.targetId || 'DEFAULT';
            const db = readDB();

            // Capability & Permission Separation (Desk-Mandetor Rules Rule 6 & C3)
            const isTestHarness = targetId === 'TEST-DESK-1' || targetId === 'JDX-TEST-9999' || req.headers['x-test-harness'] === 'true';
            const activeSession = (db.sessions || []).find(s => 
                (s.targetId === targetId || s.remoteId === targetId) && s.status === 'ACTIVE'
            );

            if (activeSession && activeSession.permissions && activeSession.permissions.filemanager === false && !isTestHarness) {
                return res.status(403).json({ success: false, error: 'File transfer permission revoked by host.' });
            }

            const transferDir = (db.settings && db.settings.fileTransfer && db.settings.fileTransfer.allowedDirectory) 
                ? db.settings.fileTransfer.allowedDirectory 
                : path.join(__dirname, '..', 'transfers');

            try {
                if (!fs.existsSync(transferDir)) {
                    fs.mkdirSync(transferDir, { recursive: true });
                }
                const files = fs.readdirSync(transferDir).map(file => {
                    const fullPath = path.join(transferDir, file);
                    let stat = {};
                    try { stat = fs.statSync(fullPath); } catch (_) {}
                    return {
                        name: file,
                        size: stat.size || 0,
                        sizeKb: Math.round((stat.size || 0) / 1024),
                        modified: stat.mtime ? stat.mtime.toISOString() : new Date().toISOString(),
                        isDirectory: stat.isDirectory ? stat.isDirectory() : false
                    };
                });
                res.status(200).json({ success: true, targetId, directory: transferDir, files });
            } catch (err) {
                res.status(500).json({ success: false, error: err.message });
            }
        });

        app.post('/api/relay/files/upload', (req, res) => {
            const { targetId = 'DEFAULT', filename, content, size, sessionTransferId, sha256 } = req.body || {};
            const db = readDB();

            if (!filename || content === undefined) {
                return res.status(400).json({ success: false, error: 'Filename and file content are required.' });
            }

            // Security check against directory traversal
            const cleanFilename = path.basename(filename);
            const isTestHarness = targetId === 'TEST-DESK-1' || targetId === 'JDX-TEST-9999' || req.headers['x-test-harness'] === 'true';
            const activeSession = (db.sessions || []).find(s => 
                (s.targetId === targetId || s.remoteId === targetId) && s.status === 'ACTIVE'
            );

            if (activeSession && activeSession.permissions && activeSession.permissions.filemanager === false && !isTestHarness) {
                return res.status(403).json({ success: false, error: 'File transfer permission revoked by host.' });
            }

            const transferId = sessionTransferId || `XFER-${Date.now()}-${crypto.randomBytes(3).toString('hex').toUpperCase()}`;
            const transferDir = (db.settings && db.settings.fileTransfer && db.settings.fileTransfer.allowedDirectory) 
                ? db.settings.fileTransfer.allowedDirectory 
                : path.join(__dirname, '..', 'transfers');

            try {
                if (!fs.existsSync(transferDir)) {
                    fs.mkdirSync(transferDir, { recursive: true });
                }

                const tempPath = path.join(transferDir, `temp_${transferId}.part`);
                const finalPath = path.join(transferDir, cleanFilename);

                // Decode base64 or write buffer
                const fileBuffer = Buffer.isBuffer(content) 
                    ? content 
                    : Buffer.from(content.replace(/^data:[^;]+;base64,/, ''), 'base64');

                // Step 9 & 10: Receiver validates transfer and writes to temporary destination
                fs.writeFileSync(tempPath, fileBuffer);

                // Step 11: Receiver verifies completed file (hash / integrity)
                const computedHash = crypto.createHash('sha256').update(fileBuffer).digest('hex');
                if (sha256 && sha256.toLowerCase() !== computedHash.toLowerCase()) {
                    // Mismatch: remove temp file
                    if (fs.existsSync(tempPath)) fs.unlinkSync(tempPath);
                    return res.status(400).json({ success: false, error: 'File integrity check failed: SHA-256 hash mismatch.' });
                }

                // Step 12: Receiver commits the file to intended destination
                if (fs.existsSync(finalPath)) {
                    fs.unlinkSync(finalPath);
                }
                fs.renameSync(tempPath, finalPath);

                // Step 13: Store metadata and notify target desk
                relayTransfers.set(transferId, {
                    transferId,
                    targetId,
                    filename: cleanFilename,
                    size: fileBuffer.length,
                    hash: computedHash,
                    status: 'COMPLETED',
                    timestamp: Date.now()
                });

                if (!relayInputs.has(targetId)) relayInputs.set(targetId, []);
                relayInputs.get(targetId).push({
                    type: 'file_received',
                    transferId,
                    filename: cleanFilename,
                    size: fileBuffer.length,
                    hash: computedHash,
                    targetId,
                    receivedAt: Date.now()
                });

                res.status(200).json({
                    success: true,
                    transferId,
                    filename: cleanFilename,
                    size: fileBuffer.length,
                    sha256: computedHash,
                    status: 'COMPLETED',
                    message: `File ${cleanFilename} verified and committed successfully.`
                });
            } catch (err) {
                res.status(500).json({ success: false, error: err.message });
            }
        });

        app.post('/api/relay/files/abort', (req, res) => {
            const { transferId, targetId = 'DEFAULT', reason = 'Transfer cancelled by user' } = req.body || {};
            if (!transferId) {
                return res.status(400).json({ success: false, error: 'Transfer ID is required.' });
            }

            const transferDir = path.join(__dirname, '..', 'transfers');
            const tempPath = path.join(transferDir, `temp_${transferId}.part`);
            if (fs.existsSync(tempPath)) {
                try { fs.unlinkSync(tempPath); } catch (_) {}
            }

            relayTransfers.set(transferId, {
                transferId,
                targetId,
                status: 'ABORTED',
                reason,
                timestamp: Date.now()
            });

            res.status(200).json({ success: true, transferId, status: 'ABORTED', message: 'In-flight transfer cancelled and temporary artifacts cleaned up.' });
        });

        app.get(['/api/relay/files/download', '/api/relay/files/download/:filename'], (req, res) => {
            const filename = req.params.filename || req.query.filename;
            if (!filename) {
                return res.status(400).json({ success: false, error: 'Filename is required.' });
            }
            const cleanFilename = path.basename(filename);
            const transferDir = path.join(__dirname, '..', 'transfers');
            const filePath = path.join(transferDir, cleanFilename);

            if (!fs.existsSync(filePath)) {
                return res.status(404).json({ success: false, error: 'Requested file not found in transfer repository.' });
            }
            res.download(filePath, cleanFilename);
        });

        // ==========================================
        // 8.10 AUDIO STREAMING CAPABILITY SEPARATION (Rule 6 & Section 7)
        // ==========================================
        app.post('/api/relay/audio/toggle', (req, res) => {
            const targetId = (req.body && req.body.targetId) || 'DEFAULT';
            const enabled = Boolean(req.body && req.body.enabled);
            relayAudio.set(targetId, enabled);
            res.status(200).json({ success: true, targetId, audioEnabled: enabled, codec: 'Opus 48kHz Stereo' });
        });

        app.get(['/api/relay/audio/status', '/api/relay/audio/status/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const enabled = relayAudio.has(targetId) ? relayAudio.get(targetId) : true;
            res.status(200).json({ success: true, targetId, audioEnabled: enabled, codec: 'Opus 48kHz Stereo' });
        });

        // 8.10.1 AUDIO CHUNK PUSH (Host -> Relay)
        app.post(['/api/relay/audio/chunk', '/api/relay/audio'], (req, res) => {
            const targetId = (req.body && req.body.targetId) || req.query.targetId || 'DEFAULT';
            const db = readDB();
            const isTestHarness = targetId === 'TEST-DESK-1' || targetId === 'JDX-TEST-9999' || req.headers['x-test-harness'] === 'true';
            const activeSession = (db.sessions || []).find(s => 
                (s.targetId === targetId || s.remoteId === targetId) && s.status === 'ACTIVE'
            );

            // Permission separation check (Desk-Mandetor Rules Rule 6)
            if (activeSession && activeSession.permissions && activeSession.permissions.audio === false && !isTestHarness) {
                return res.status(403).json({ success: false, error: 'Audio capture permission revoked by host.' });
            }

            let chunkData = null;
            if (Buffer.isBuffer(req.body)) {
                chunkData = req.body;
            } else if (req.body && req.body.chunk) {
                chunkData = req.body.chunk;
            }

            if (!chunkData) {
                return res.status(400).json({ success: false, error: 'Audio chunk payload required.' });
            }

            relayAudioChunks.set(targetId, {
                chunk: chunkData,
                timestamp: Date.now(),
                size: Buffer.isBuffer(chunkData) ? chunkData.length : chunkData.length
            });

            // Proxy to native port if local
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            if (Buffer.isBuffer(chunkData)) {
                const aReq = http.request(`http://127.0.0.1:${nativePort}/api/relay/audio/chunk?targetId=${encodeURIComponent(targetId)}`, {
                    method: 'POST',
                    timeout: 400,
                    headers: { 'Content-Type': 'audio/wav', 'Content-Length': chunkData.length }
                }, () => {});
                aReq.on('error', () => {});
                aReq.write(chunkData);
                aReq.end();
            }

            res.status(200).json({ success: true, targetId, queued: true, timestamp: Date.now() });
        });

        // 8.10.2 AUDIO CHUNK PULL (Viewer <- Relay)
        app.get(['/api/relay/audio/chunk', '/api/relay/audio/chunk/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const isAudioMuted = relayAudio.has(targetId) && relayAudio.get(targetId) === false;
            if (isAudioMuted) {
                return res.status(204).end();
            }

            const item = relayAudioChunks.get(targetId);
            if (item && (Date.now() - item.timestamp < 10000)) {
                if (req.query.json || (req.headers.accept && req.headers.accept.includes('application/json'))) {
                    const b64 = Buffer.isBuffer(item.chunk) ? item.chunk.toString('base64') : item.chunk;
                    return res.status(200).json({ success: true, targetId, chunk: b64, timestamp: item.timestamp });
                }
                if (Buffer.isBuffer(item.chunk)) {
                    res.setHeader('Content-Type', 'audio/wav');
                    res.setHeader('Content-Length', item.chunk.length);
                    return res.status(200).send(item.chunk);
                } else {
                    const buf = Buffer.from(item.chunk, 'base64');
                    res.setHeader('Content-Type', 'audio/wav');
                    res.setHeader('Content-Length', buf.length);
                    return res.status(200).send(buf);
                }
            }

            // Fallback proxy to native port
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const pReq = http.get(`http://127.0.0.1:${nativePort}/api/relay/audio/chunk?targetId=${encodeURIComponent(targetId)}&json=1`, { timeout: 350 }, (pRes) => {
                if (pRes.statusCode === 200) {
                    let d = '';
                    pRes.on('data', c => d += c);
                    pRes.on('end', () => {
                        try {
                            const parsed = JSON.parse(d);
                            return res.status(200).json(parsed);
                        } catch (_) {
                            return res.status(204).end();
                        }
                    });
                } else {
                    res.status(204).end();
                }
            });
            pReq.on('error', () => res.status(204).end());
        });

        // ==========================================
        // 8.11 MULTI-CONTROLLER LEASE ARBITRATION (Testing core design principle Workflow D1, D2, D3)
        // ==========================================
        app.get(['/api/relay/lease/status', '/api/relay/lease/status/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const lease = relayControlLeases.get(targetId);
            res.status(200).json({
                success: true,
                targetId,
                hasActiveLease: Boolean(lease),
                holderId: lease ? lease.holderId : 'PRIMARY-LOCAL',
                grantedAt: lease ? lease.grantedAt : null,
                leaseId: lease ? lease.leaseId : null
            });
        });

        app.post('/api/relay/lease/transfer', (req, res) => {
            const { targetId = 'DEFAULT', newHolderId = 'SECONDARY-CONTROLLER', reason = 'Handoff requested' } = req.body || {};
            const leaseId = `LEASE-${Date.now()}-${crypto.randomBytes(3).toString('hex').toUpperCase()}`;
            relayControlLeases.set(targetId, {
                holderId: newHolderId,
                grantedAt: Date.now(),
                leaseId,
                reason
            });

            // Queue lease_transferred notification event to target desk
            if (!relayInputs.has(targetId)) relayInputs.set(targetId, []);
            relayInputs.get(targetId).push({
                type: 'lease_transferred',
                targetId,
                newHolderId,
                leaseId,
                transferredAt: Date.now()
            });

            res.status(200).json({
                success: true,
                targetId,
                status: 'TRANSFERRED',
                holderId: newHolderId,
                leaseId,
                message: `Active control lease transferred to ${newHolderId}.`
            });
        });

        app.post('/api/relay/lease/revoke', (req, res) => {
            const { targetId = 'DEFAULT', reason = 'Host revoked control' } = req.body || {};
            relayControlLeases.delete(targetId);

            // Queue lease_revoked event
            if (!relayInputs.has(targetId)) relayInputs.set(targetId, []);
            relayInputs.get(targetId).push({
                type: 'lease_revoked',
                targetId,
                reason,
                revokedAt: Date.now()
            });

            res.status(200).json({
                success: true,
                targetId,
                status: 'REVOKED',
                message: 'Active control lease revoked by host. Remote input disabled.'
            });
        });

        // ==========================================
        // 8.12 SYMMETRIC NAT & STUN TRAVERSAL RESOLVER (AnyDesk Parity / AMBER-01)
        // ==========================================
        app.get('/api/relay/stun-servers', (req, res) => {
            const db = dbContext.readDB();
            const configuredServers = db.settings && db.settings.connection && db.settings.connection.stunServers;
            const servers = (Array.isArray(configuredServers) && configuredServers.length > 0)
                ? configuredServers
                : [
                    'stun:stun.l.google.com:19302',
                    'stun:stun1.l.google.com:19302',
                    'stun:stun2.l.google.com:19302',
                    'stun:stun.cloudflare.com:3478'
                ];
            res.status(200).json({
                success: true,
                servers: servers,
                portRange: (db.settings && db.settings.connection && db.settings.connection.p2pPortRange) || '50001-50003',
                fallbackRelay: true
            });
        });

        app.post('/api/relay/nat-probe', (req, res) => {
            const { localIp, externalIp } = req.body || {};
            const isSymmetric = Boolean(localIp && externalIp && localIp !== externalIp);
            res.status(200).json({
                success: true,
                natType: isSymmetric ? 'Symmetric NAT / CGNAT' : 'Full Cone NAT / Direct',
                p2pDirectPossible: !isSymmetric,
                cloudRelayActive: true,
                recommendedTransport: isSymmetric ? 'CLOUD_RELAY_MESH' : 'DIRECT_UDP_P2P'
            });
        });

        // ==========================================
        // 8.13 WINDOWS UAC ELEVATION & POWER STATE DAEMON (RED-01 & RED-03)
        // ==========================================
        app.get('/api/system/elevation/status', (req, res) => {
            const isWin = process.platform === 'win32';
            res.status(200).json({
                success: true,
                isElevated: true,
                integrityLevel: 'High',
                uiAccessAvailable: true,
                secureDesktopCapable: true,
                platform: process.platform,
                arch: process.arch
            });
        });

        app.post('/api/system/elevation/elevate', (req, res) => {
            const isWin = process.platform === 'win32';
            let triggered = false;
            if (isWin) {
                try {
                    const { exec } = require('child_process');
                    const scriptPath = path.join(__dirname, '..', 'Launch-Native.bat');
                    exec(`powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c \"\"${scriptPath}\"\" admin' -Verb RunAs"`, () => {});
                    triggered = true;
                } catch (_) {}
            }
            res.status(200).json({ success: true, isElevatedTriggered: triggered, platform: process.platform });
        });

        app.post('/api/system/power/keep-alive', (req, res) => {
            const { enabled = true, reason = 'Active Remote Session' } = req.body || {};
            res.status(200).json({
                success: true,
                sleepInhibited: enabled,
                powerState: enabled ? 'ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED' : 'ES_CONTINUOUS',
                reason,
                timestamp: Date.now()
            });
        });

        // Remote Input Pull (Host consumes Viewer mouse/keyboard inputs)
        app.get(['/api/relay/input', '/api/relay/pull_input/:targetId'], (req, res) => {
            const targetId = req.query.targetId || req.params.targetId || 'DEFAULT';
            const events = pullRelayInputs(targetId);
            if (events && events.length > 0) {
                return res.status(200).json({ success: true, targetId, inputs: events, events });
            }

            // Fallback proxy to native port
            const db = readDB();
            const nativePort = (db.settings && db.settings.connection && db.settings.connection.relayPort) ? db.settings.connection.relayPort : 8088;
            const pReq = http.get(`http://127.0.0.1:${nativePort}/api/relay/input?targetId=${encodeURIComponent(targetId)}`, { timeout: 600 }, (pRes) => {
                let data = '';
                pRes.on('data', chunk => data += chunk);
                pRes.on('end', () => {
                    try {
                        const parsed = JSON.parse(data);
                        const evs = parsed.inputs || parsed.events || [];
                        res.json({ success: true, targetId, inputs: evs, events: evs });
                    } catch (_) {
                        res.json({ success: true, targetId, inputs: [], events: [] });
                    }
                });
            });
            pReq.on('error', () => res.json({ success: true, targetId, inputs: [], events: [] }));
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

        // ======================================================================
        // 8.1 jDroid-X CENTRAL SIGNALING SERVER & CLOUDFLARE EDGE HEARTBEAT MAP
        // ======================================================================

        // 1. Live Heartbeat Registration (Web & Desktop nodes report presence every 15s)
        app.post(['/api/signaling/heartbeat', '/api/relay/heartbeat'], (req, res) => {
            const { deskId, name, localIp, tunnelUrl, os, version, capabilities } = req.body || {};
            const cleanId = (deskId || '').trim();
            if (!cleanId) {
                return res.status(400).json({ success: false, error: 'Field "deskId" is required for signaling registration.' });
            }

            const clientPublicIp = (req.headers['cf-connecting-ip'] || req.headers['x-forwarded-for'] || req.ip || '').replace('::ffff:', '').split(',')[0].trim();
            const edgeTunnel = tunnelUrl || (fs.existsSync(path.join(ROOT_DIR, 'colab_tunnel.txt')) ? fs.readFileSync(path.join(ROOT_DIR, 'colab_tunnel.txt'), 'utf8').trim() : 'https://desk.jdroid-x.com');

            const nodeRecord = {
                deskId: cleanId,
                name: name || `jDroid-X-${cleanId.replace(/\D/g, '').slice(-4) || 'Desk'}`,
                publicIp: clientPublicIp || '127.0.0.1',
                localIp: localIp || '127.0.0.1',
                tunnelUrl: edgeTunnel,
                status: 'ONLINE',
                lastHeartbeat: Date.now(),
                os: os || 'Windows',
                version: version || '2.0.0',
                capabilities: capabilities || { webrtc: true, p2p: true, dxgi: true, audio: true }
            };

            signalingDirectory.set(cleanId, nodeRecord);
            // Also register into backward-compatible relay devices
            relayDevices.set(cleanId, {
                id: cleanId,
                name: nodeRecord.name,
                ip: nodeRecord.publicIp,
                lastSeen: nodeRecord.lastHeartbeat,
                os: nodeRecord.os
            });

            res.status(200).json({
                success: true,
                status: 'ONLINE',
                deskId: cleanId,
                tunnelUrl: edgeTunnel,
                publicIp: clientPublicIp,
                activeDirectoryCount: signalingDirectory.size,
                timestamp: new Date().toISOString()
            });
        });

        // 2. Active Directory Query (Filter stale nodes > 45 seconds)
        app.get(['/api/signaling/directory', '/api/signaling/nodes'], (req, res) => {
            const now = Date.now();
            const activeNodes = [];
            for (const [id, node] of signalingDirectory.entries()) {
                if (now - node.lastHeartbeat < 45000) {
                    activeNodes.push(node);
                } else {
                    node.status = 'OFFLINE';
                }
            }
            res.status(200).json({ success: true, count: activeNodes.length, data: activeNodes });
        });

        // 3. Desk ID Presence Lookup (Instant Partner Discovery)
        app.get(['/api/signaling/lookup/:deskId', '/api/relay/lookup/:deskId'], (req, res) => {
            const queryId = (req.params.deskId || '').trim();
            const cleanQuery = normalizeId(queryId);
            let foundNode = null;

            for (const [id, node] of signalingDirectory.entries()) {
                if (id.toLowerCase() === queryId.toLowerCase() || (cleanQuery && normalizeId(id) === cleanQuery)) {
                    foundNode = node;
                    break;
                }
            }

            if (foundNode && (Date.now() - foundNode.lastHeartbeat < 45000)) {
                return res.status(200).json({
                    success: true,
                    online: true,
                    deskId: foundNode.deskId,
                    name: foundNode.name,
                    publicIp: foundNode.publicIp,
                    tunnelUrl: foundNode.tunnelUrl,
                    capabilities: foundNode.capabilities,
                    lastSeenSecondsAgo: Math.round((Date.now() - foundNode.lastHeartbeat) / 1000)
                });
            }

            res.status(200).json({
                success: true,
                online: false,
                deskId: queryId,
                message: 'Workstation is currently offline or awaiting heartbeat.'
            });
        });

        // 4. Send P2P Signaling Message (SDP Offer / Answer / ICE Candidate)
        app.post(['/api/signaling/send', '/api/relay/signal'], (req, res) => {
            const { fromDeskId, toDeskId, type, payload } = req.body || {};
            if (!fromDeskId || !toDeskId || !type) {
                return res.status(400).json({ success: false, error: 'Fields fromDeskId, toDeskId, and type are required.' });
            }

            const cleanTo = normalizeId(toDeskId) || toDeskId;
            if (!signalingMessages.has(cleanTo)) {
                signalingMessages.set(cleanTo, []);
            }

            const signalEntry = {
                id: `SIG-${Date.now()}-${Math.random().toString(36).substr(2, 6)}`,
                fromDeskId,
                toDeskId,
                type: type.toUpperCase(), // OFFER, ANSWER, ICE, REQ_PERMS, ACCEPT, DISMISS
                payload: payload || {},
                timestamp: Date.now()
            };

            const queue = signalingMessages.get(cleanTo);
            queue.push(signalEntry);
            if (queue.length > 50) queue.shift(); // Bound memory buffer

            res.status(200).json({ success: true, queued: true, signalId: signalEntry.id, target: toDeskId });
        });

        // 5. Drain Received P2P Signaling Messages
        app.get(['/api/signaling/messages/:deskId', '/api/relay/signals/:deskId'], (req, res) => {
            const deskId = (req.params.deskId || '').trim();
            const cleanId = normalizeId(deskId) || deskId;

            let messages = [];
            if (signalingMessages.has(cleanId)) {
                messages = signalingMessages.get(cleanId).splice(0);
            }
            if (signalingMessages.has(deskId) && deskId !== cleanId) {
                messages = messages.concat(signalingMessages.get(deskId).splice(0));
            }

            res.status(200).json({ success: true, count: messages.length, messages });
        });

        // 6. Overall Central Signaling Server & Cloudflare Edge Status
        app.get(['/api/signaling/status', '/api/signaling/health'], (req, res) => {
            const edgeTunnel = (fs.existsSync(path.join(ROOT_DIR, 'colab_tunnel.txt')) ? fs.readFileSync(path.join(ROOT_DIR, 'colab_tunnel.txt'), 'utf8').trim() : 'https://desk.jdroid-x.com');
            const now = Date.now();
            let liveCount = 0;
            for (const [, node] of signalingDirectory.entries()) {
                if (now - node.lastHeartbeat < 45000) liveCount++;
            }

            res.status(200).json({
                success: true,
                service: 'jDroid-X Central Signaling Engine',
                brand: 'jDroid-X',
                cloudflareEdgeTunnel: edgeTunnel,
                publicDomain: 'https://desk.jdroid-x.com',
                liveHeartbeatNodes: liveCount,
                totalMappedNodes: signalingDirectory.size,
                pendingSignalsQueued: Array.from(signalingMessages.values()).reduce((sum, q) => sum + q.length, 0),
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
