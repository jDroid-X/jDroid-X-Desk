/**
 * jDroid-X-Desk OOPS MVC Data Context (models/db-context.js)
 * Master Database Adapter, Schema Validator, Log Purging & JSON Store Interface
 */

const fs = require('fs');
const path = require('path');

const ROOT_DIR = path.resolve(__dirname, '..');
const DB_PATH = path.join(ROOT_DIR, 'database.json');
const MODELS_DB_PATH = path.join(__dirname, 'database.json');

const defaultUserProfile = process.env.USERPROFILE || 'C:\\Users\\Public';
const defaultAppData = process.env.APPDATA || path.join(defaultUserProfile, 'AppData', 'Roaming');

const DEFAULT_SETTINGS = {
    pingThresholdMs: 100,
    downloadThresholdMBs: 1.0,
    toastDurationMs: 4000,
    defaultAccessLevel: 'Standard',
    theme: 'light',
    networkBaseline: '4G/5G Auto',
    autoStartup: false,
    jarvisTelemetry: true,
    language: 'autoselect',
    general: {
        hideLocalTaskbar: false,
        requestComment: false,
        openAddressBookOnStartup: false,
        showAcceptWindowOnChat: false,
        showPopupsForInvites: true,
        soundOnIncoming: false,
        transmitHotkeys: true,
        screenshotPath: path.join(defaultUserProfile, 'Pictures', 'jDroid-X'),
        chatLogPath: path.join(defaultAppData, 'jDroid-X', 'chat'),
        onlineStatusVisibility: true,
        allowDataCollection: false
    },
    connection: {
        allowDirectConnections: true,
        directConnections: true,
        localPort: 7070,
        relayPort: 8088,
        relayServer: 'http://127.0.0.1:8088',
        customDomain: 'https://desk.jdroid-x.com',
        portalUrl: 'https://www.jdroid-x.com',
        tunnelPort: 7700,
        discoveryPort: 8089,
        lanDiscovery: true,
        proxyMode: 'detect',
        proxyType: 'HTTP(S)',
        proxyAddress: '',
        proxyPort: '',
        proxyAuthMode: 'ntlm',
        proxyUsername: '',
        proxyPassword: ''
    },
    wakeOnLan: {
        mode: 'enabled',
        authorizedClients: []
    },
    screenRecording: {
        enabled: true,
        recordingPath: path.join(defaultUserProfile, 'Videos', 'jDroid-X', 'recordings'),
        audioInput: 'default',
        micInput: 'default'
    },
    access: {
        interactiveAccess: 'open_only',
        unattendedPassword: '',
        allowSaveLoginTokens: true,
        enable2FA: false,
        aclEnabled: false,
        aclAddresses: [],
        localDiscovery: true,
        excludeFromDiscovery: false,
        autoDisconnectMinutes: 10
    },
    permissions: {
        globalPrint: true,
        globalSyncClipboard: true,
        globalSyncClipboardFiles: true,
        globalLockOnEnd: false,
        activeProfile: 'Default'
    },
    privacy: {
        screenFrameEnabled: false,
        screenFrameColor: '#ff6666',
        screenFrameOpacity: 50,
        screenFrameWidth: 5
    },
    display: {
        quality: 'best_quality',
        visualHelpers: 'show_on_movement',
        followCursor: false,
        followWindowFocus: false,
        showSizeHelper: false,
        viewMode: 'optimize_shrink',
        edgeScrolling: false,
        fullScreenMode: 'automatic',
        hardwareAcceleration: 'direct3d',
        use16BitRenderer: false,
        rememberPerDesk: true
    },
    audio: {
        transmission: 'audio_output',
        outputDevice: 'default',
        exclusivePlayback: false
    },
    recording: {
        autoRecordIncoming: false,
        autoRecordOutgoing: false,
        sessionRecordPath: path.join(defaultUserProfile, 'Videos', 'jDroid-X')
    },
    printer: {
        incomingPrintJobs: 'default_printer',
        autoPrint: false
    },
    account: {
        email: '',
        licenseKey: 'free-1',
        licenseType: 'Free Non-Commercial'
    }
};

const DEFAULT_SERVER_CONFIG = {
    activeEngine: 'jdroidRelay',
    processingLimits: {
        maxConcurrentSessions: 10,
        bandwidthLimitMbps: 50,
        maxFps: 60,
        idleTimeoutMinutes: 15,
        logRetentionDays: 7,
        hardwareAcceleration: 'nvenc_cuda'
    },
    engines: {
        rustdesk: {
            id: 'rustdesk',
            name: 'jDroid-X RustDesk P2P (rustdesk/rustdesk)',
            type: 'Direct Match P2P Engine',
            version: 'v1.3.x',
            enabled: true,
            status: 'Ready',
            probePort: 21116,
            description: 'Direct peer-to-peer streaming with built-in STUN/TURN rendezvous encryption.',
            binaryStream: true,
            cudaActive: true,
            manualConfigRequired: 'Zero manual config. Runs automatically out-of-the-box.'
        },
        sunshine: {
            id: 'sunshine',
            name: 'jDroid-X Sunshine & Moonlight (LizardByte/Sunshine)',
            type: 'Direct GPU VRAM Capture Engine',
            version: 'v0.23.x',
            enabled: true,
            status: 'Active jDroid-Engine',
            probePort: 47990,
            description: 'Sub-5ms ultra low-latency game & workstation streaming via NVENC/DirectX Desktop Duplication.',
            binaryStream: true,
            cudaActive: true,
            manualConfigRequired: '1-time Web GUI admin user setup & 4-digit PIN pairing.'
        },
        guacamole: {
            id: 'guacamole',
            name: 'jDroid-X Guacamole HTML5 Gateway (apache/guacamole-server)',
            type: 'Clientless Web Browser Engine',
            version: 'v1.5.x',
            enabled: true,
            status: 'Ready',
            probePort: 8080,
            description: 'Zero-install browser remote access supporting RDP, VNC, and SSH over WebSockets.',
            binaryStream: true,
            cudaActive: false,
            manualConfigRequired: 'Docker container or Tomcat server required for full web client gateway.'
        },
        jdroidRelay: {
            id: 'jdroidRelay',
            name: 'jDroid-X Google Colab Cloud Relay',
            type: 'High-Bandwidth Cloud Relay Engine',
            version: 'v3.0.x',
            enabled: true,
            status: 'Ready',
            probePort: 8088,
            description: 'Google Colab NVIDIA T4 GPU-accelerated cloud relay bridging NAT & firewalls.',
            binaryStream: true,
            cudaActive: true,
            manualConfigRequired: 'Zero manual config. Runs automatically out-of-the-box with Cloudflare tunnel.'
        }
    }
};

const DEFAULT_SCHEMA = {
    safeZones: [
        { id: "SZ-01", name: "Corporate HQ Subnet", ip: "192.168.1.0/24" },
        { id: "SZ-02", name: "Secure VPN Perimeter", ip: "10.0.0.0/16" }
    ],
    groups: [
        { id: "grp-1", name: "Core Workstations", description: "Primary enterprise production nodes", members: 2 },
        { id: "grp-2", name: "Field Fleet", description: "Mobile client workstations and support endpoints", members: 1 }
    ],
    sessions: [],
    invites: [],
    devices: [
        { id: "dev-1", name: "HQ Primary Workstation", ip: "192.168.1.44", groupId: "grp-1", group: "Core Workstations", status: "ONLINE", accessLevel: "Admin" },
        { id: "dev-2", name: "Lab Render Station", ip: "192.168.1.105", groupId: "grp-1", group: "Core Workstations", status: "ONLINE", accessLevel: "Full Control" },
        { id: "dev-3", name: "Field Mobile Node", ip: "10.0.4.120", groupId: "grp-2", group: "Field Fleet", status: "STANDBY", accessLevel: "Standard" }
    ],
    guardians: [
        { id: "grd-1", name: "jDroid-X Systems Architect", role: "Senior Systems Architect (Level 5)", contact: "jeet@jdroid-x.enterprise", status: "Active" },
        { id: "grd-2", name: "Enterprise Security Auditor", role: "Security & Compliance Officer", contact: "sec-ops@jdroid-x.enterprise", status: "Active" }
    ],
    contacts: [
        { id: "cnt-1", name: "Lead Architect JEET", remoteId: "984-102-391", groupId: "grp-1", group: "Core Workstations" },
        { id: "cnt-2", name: "Support Specialist Alpha", remoteId: "520-310-884", groupId: "grp-2", group: "Field Fleet" }
    ],
    notifications: [
        { id: "notif-1", title: "System Security Baseline Verified", message: "All workstations running AES-GCM-256 encrypted peer-to-peer tunnels.", type: "success", read: false, timestamp: "2026-10-16T15:20:00.000Z" },
        { id: "notif-2", title: "Google Colab Relay Standby", message: "Cloud relay bridge available for high-bandwidth remote screen acceleration.", type: "info", read: true, timestamp: "2026-10-16T14:45:00.000Z" }
    ],
    settings: DEFAULT_SETTINGS,
    serverConfig: DEFAULT_SERVER_CONFIG,
    logs: []
};

const RETENTION_MS = 7 * 24 * 60 * 60 * 1000; // 7 Days in Milliseconds

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
        const jsonStr = JSON.stringify(data, null, 2);
        fs.writeFileSync(tmpPath, jsonStr, 'utf8');
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
        // Mirror to models/database.json as encapsulated store
        try {
            fs.writeFileSync(MODELS_DB_PATH, jsonStr, 'utf8');
        } catch (_) {}
    } catch (e) {
        if (fs.existsSync(tmpPath)) { try { fs.unlinkSync(tmpPath); } catch (_) {} }
        throw e;
    }
}

function readDB() {
    if (!fs.existsSync(DB_PATH)) {
        if (fs.existsSync(MODELS_DB_PATH)) {
            fs.copyFileSync(MODELS_DB_PATH, DB_PATH);
        } else {
            fs.writeFileSync(DB_PATH, JSON.stringify(DEFAULT_SCHEMA, null, 2));
        }
    }
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

function deepMerge(target, source) {
    if (!source || typeof source !== 'object' || Array.isArray(source)) {
        return source !== undefined ? source : target;
    }
    const result = { ...(target || {}) };
    for (const key of Object.keys(source)) {
        if (source[key] !== null && typeof source[key] === 'object' && !Array.isArray(source[key])) {
            result[key] = deepMerge(result[key] || {}, source[key]);
        } else if (source[key] !== undefined) {
            result[key] = source[key];
        }
    }
    return result;
}

module.exports = {
    ROOT_DIR,
    DB_PATH,
    MODELS_DB_PATH,
    DEFAULT_SETTINGS,
    DEFAULT_SERVER_CONFIG,
    DEFAULT_SCHEMA,
    readDB,
    writeDB,
    purgeOldLogs,
    deepMerge
};
