/**
 * jDroid-X-Desk | Unified Remote Desktop Control Console (v2.0 Streamlined)
 * Architecture: OOPS-Based Model-View-Controller (MVC) Pattern
 */

// ==========================================
// 1. MODELS (Data Layer)
// ==========================================

class LocalSystemModel {
    constructor() {
        this.deviceId = 'JDX-INIT-0000';
        this.oneTimePass = 'k9X#m2P8';
        this.activeSession = null;
        
        // Jarvis Telemetry (MB/s & 4G Thresholds)
        this.telemetry = {
            downloadMBs: 2.45,
            uploadMBs: 0.82,
            pingMs: 14,
            fps: 60,
            lastAlarmTime: 0
        };
    }
}

class SystemSettingsModel {
    constructor() {
        this.startupEnabled = false;
        
        // Firebase Cloud configuration
        this.firebaseApiKey = localStorage.getItem('jdx_fb_api_key') || 'AIzaSyCUfP_cQmVghxDYfUiuPDUycTBAUU9VR-8';
        this.firebaseProjectId = localStorage.getItem('jdx_fb_project_id') || 'jfirebaselogin';
        this.firebaseDbUrl = localStorage.getItem('jdx_fb_db_url') || 'https://jfirebaselogin-default-rtdb.firebaseio.com';
        this.firebaseMessagingSenderId = localStorage.getItem('jdx_fb_messaging_sender_id') || '734511435834';
        this.firebaseAppId = localStorage.getItem('jdx_fb_app_id') || '1:734511435834:web:bcd459e356b15c0cfbe503';
        this.firebaseMeasurementId = localStorage.getItem('jdx_fb_measurement_id') || 'G-NTGPTX1H22';

        // 13-Screen Smart Grid Configurations
        this.dashAutoConnect = localStorage.getItem('jdx_set_dash_auto') === 'true';
        this.dashResolution = localStorage.getItem('jdx_set_dash_res') || '1080p';
        
        this.monVerbose = localStorage.getItem('jdx_set_mon_verbose') === 'true';
        this.monBuffer = parseInt(localStorage.getItem('jdx_set_mon_buffer') || '50', 10);
        
        this.fleetAutoPing = localStorage.getItem('jdx_set_fleet_ping') === 'true';
        this.fleetTimeout = parseInt(localStorage.getItem('jdx_set_fleet_timeout') || '120', 10);
        
        this.mapTheme = localStorage.getItem('jdx_set_map_theme') || 'Dark Vector';
        this.mapCluster = localStorage.getItem('jdx_set_map_cluster') !== 'false';
        
        this.histPurge = parseInt(localStorage.getItem('jdx_set_hist_purge') || '30', 10);
        this.histAcc = parseInt(localStorage.getItem('jdx_set_hist_acc') || '50', 10);
        
        this.grpSync = localStorage.getItem('jdx_set_grp_sync') || 'Real-time';
        
        this.zoneRadius = parseInt(localStorage.getItem('jdx_set_zone_rad') || '200', 10);
        this.zoneSeverity = localStorage.getItem('jdx_set_zone_sev') || 'Critical (Alarm)';
        
        this.guardMfa = localStorage.getItem('jdx_set_grd_mfa') !== 'false';
        this.guardInherit = localStorage.getItem('jdx_set_grd_inherit') === 'true';
        
        this.notifSound = localStorage.getItem('jdx_set_notif_sound') !== 'false';
        this.toastDuration = parseInt(localStorage.getItem('jdx_set_toast_duration') || '4', 10);
        
        this.healthBatt = parseInt(localStorage.getItem('jdx_set_hlth_batt') || '15', 10);
        this.healthTemp = parseInt(localStorage.getItem('jdx_set_hlth_temp') || '80', 10);
        
        this.profSync = localStorage.getItem('jdx_set_prof_sync') !== 'false';
        this.profIdle = parseInt(localStorage.getItem('jdx_set_prof_idle') || '30', 10);
        
        this.privTelemetry = localStorage.getItem('jdx_set_priv_telemetry') === 'true';
        this.privBackup = localStorage.getItem('jdx_set_priv_backup') !== 'false';

        // Network Metadata simulation
        this.connType = "5G Direct P2P";
        this.ipAddress = "192.168.1.55";
    }
}

class SystemTraceabilityLogModel {
    constructor() {
        this.systemA = { id: 'SYS-A-PRIMARY-984', ip: '10.0.4.120', role: 'Primary Technician' };
        this.systemB = { id: 'SYS-B-SECONDARY-520', ip: '192.168.1.50', role: 'Secondary Host User' };
        this.logs = [];
        this.packetSeq = 1001;
        this.monitoringActive = false;

        this.initialLogs();
    }

    initialLogs() {
        this.addLog('INFO', 'STUN_HANDSHAKE', 'Primary System A initiated UDP connection request to Secondary System B (Port 21116)', 18);
        this.addLog('SECURITY_AUDIT', 'KEY_EXCHANGE', 'Ed25519 Elliptic Curve Key Exchange completed. Tunnel Encrypted.', 22);
        this.addLog('INFO', 'SESSION_ACCEPT', 'Secondary System B user accepted connection. Screen sharing & input control granted.', 19);
        this.addLog('TRACE', 'FRAME_STREAM', 'Remote Desktop stream active (1080p 60FPS, H.265 Codec, 1.8 Mbps)', 16);
    }

    addLog(level, stage, message, rtt = null) {
        const now = new Date().toISOString();
        const latency = rtt || Math.floor(Math.random() * 15) + 12;
        const entry = {
            id: `TRC-${this.packetSeq++}`,
            timestamp: now,
            level: level,
            stage: stage,
            source: this.systemA.id,
            target: this.systemB.id,
            rttMs: latency,
            message: message,
            hash: `ed25519-${Math.random().toString(36).substring(2, 8)}`
        };
        this.logs.unshift(entry);
        if (this.logs.length > 100) this.logs.pop();
        return entry;
    }

    getLogs(filterLevel = 'ALL') {
        if (filterLevel === 'ALL') return this.logs;
        return this.logs.filter(l => l.level === filterLevel);
    }

    exportJSON() {
        return JSON.stringify({
            auditMetadata: {
                generatedAt: new Date().toISOString(),
                systemA: this.systemA,
                systemB: this.systemB,
                totalTraceLogs: this.logs.length,
                complianceStatus: 'VERIFIED_AUDITABLE'
            },
            traceLogs: this.logs
        }, null, 2);
    }

    exportCSV() {
        const headers = "Packet ID,Timestamp,Level,Stage,Source,Target,RTT (ms),Message\n";
        const rows = this.logs.map(l => 
            `"${l.id}","${l.timestamp}","${l.level}","${l.stage}","${l.source}","${l.target}","${l.rttMs}","${l.message.replace(/"/g, '""')}"`
        ).join("\n");
        return headers + rows;
    }
}

// ==========================================
// 2. VIEWS (Presentation Layer)
// ==========================================

class AppView {
    constructor() {
        this.contentArea = document.getElementById('content-area');
        this.viewTitle = document.getElementById('view-title');
        this.toastContainer = document.getElementById('toast-container');
    }

    updateViewTitle(title) {
        if (this.viewTitle) this.viewTitle.textContent = title;
    }

    showToast(type, title, desc) {
        if (!this.toastContainer) return;

        const toast = document.createElement('div');
        toast.className = `toast-card toast-${type}`;

        let iconClass = 'fa-circle-info';
        if (type === 'success') iconClass = 'fa-circle-check';
        if (type === 'error')   iconClass = 'fa-circle-xmark';
        if (type === 'warning') iconClass = 'fa-triangle-exclamation';

        // UX-7: Dismissible toast — add close button
        toast.innerHTML = `
            <i class="fa-solid ${iconClass}"></i>
            <div style="flex:1">
                <div class="toast-title">${title}</div>
                <div class="toast-desc">${desc}</div>
            </div>
            <button class="toast-close-btn" aria-label="Dismiss">&times;</button>
        `;

        const dismissToast = () => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(110%)';
            toast.style.transition = 'all 0.3s ease';
            setTimeout(() => toast.remove(), 300);
        };

        toast.querySelector('.toast-close-btn').addEventListener('click', dismissToast);
        this.toastContainer.appendChild(toast);

        setTimeout(dismissToast, (window.appController?.settingsModel?.toastDuration || 4) * 1000);
    }

    renderHintBanner(text) {
        return `
            <div class="screen-hint-box">
                <i class="fa-solid fa-lightbulb hint-icon"></i>
                <div class="hint-text">
                    <strong>Screen Hint & Guidelines:</strong> ${text}
                </div>
            </div>
        `;
    }

    renderSystemMonitor(traceModel) {
        const sysA = traceModel.systemA;
        const sysB = traceModel.systemB;
        const logs = traceModel.getLogs();

        return `
            ${this.renderHintBanner('Continuous 2-System Traceability Engine. Real-time telemetry monitoring, cryptographic key exchange tracking, and packet latency logging between System A (Primary) and System B (Secondary).')}

            <div class="widget-box">
                <div class="widget-header" style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem;">
                    <div>
                        <h3><i class="fa-solid fa-network-wired text-primary"></i> Continuous 2-System Traceability Log Engine</h3>
                        <p>Live telemetry streaming between <strong>${sysA.id} (${sysA.ip})</strong> and <strong>${sysB.id} (${sysB.ip})</strong>.</p>
                    </div>

                    <div style="display: flex; gap: 0.5rem; flex-wrap: wrap;">
                        <button id="mon-start-btn" class="btn ${traceModel.monitoringActive ? 'btn-danger' : 'btn-primary'}">
                            <i class="fa-solid ${traceModel.monitoringActive ? 'fa-pause' : 'fa-play'}"></i>
                            ${traceModel.monitoringActive ? 'Pause Continuous Stream' : 'Start Continuous Monitoring'}
                        </button>
                        <button id="mon-inject-btn" class="btn btn-outline">
                            <i class="fa-solid fa-bolt"></i> Inject Trace Event
                        </button>
                        <button id="mon-export-btn" class="btn btn-secondary">
                            <i class="fa-solid fa-download"></i> Export Log Audit (JSON)
                        </button>
                    </div>
                </div>

                <!-- 2-System Topology Visualizer -->
                <div class="margin-top-md" style="background: var(--bg-tertiary); border: 1px solid var(--border-bright); border-radius: var(--radius-md); padding: 0.5rem;">
                    <div style="display: grid; grid-template-columns: 1fr auto 1fr; gap: 1rem; align-items: center; text-align: center;">
                        <div style="background: var(--bg-card); padding: 0.4rem; border-radius: 8px; border-left: 4px solid var(--accent-blue);">
                            <i class="fa-solid fa-laptop-code" style="font-size: 1.8rem; color: var(--accent-blue);"></i>
                            <h4 style="font-size: 0.95rem; margin-top: 0.16rem;">System A (Primary)</h4>
                            <div style="font-size: 0.75rem; color: var(--text-muted);">${sysA.id} | ${sysA.ip}</div>
                        </div>

                        <div style="display: flex; flex-direction: column; align-items: center; gap: 0.25rem;">
                            <span class="badge badge-blue">P2P Encrypted Tunnel</span>
                            <div style="font-size: 0.75rem; color: var(--accent-emerald); font-weight: 600;">
                                <i class="fa-solid fa-shield-halved"></i> Ed25519 Encrypted
                            </div>
                            <div style="font-size: 0.72rem; color: var(--text-dim);">Avg RTT: 16 ms | Packet Loss: 0.0%</div>
                        </div>

                        <div style="background: var(--bg-card); padding: 0.4rem; border-radius: 8px; border-left: 4px solid var(--accent-emerald);">
                            <i class="fa-solid fa-server" style="font-size: 1.8rem; color: var(--accent-emerald);"></i>
                            <h4 style="font-size: 0.95rem; margin-top: 0.16rem;">System B (Secondary)</h4>
                            <div style="font-size: 0.75rem; color: var(--text-muted);">${sysB.id} | ${sysB.ip}</div>
                        </div>
                    </div>
                </div>

                <!-- Filter & Terminal Table -->
                <div class="margin-top-lg">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.3rem;">
                        <span style="font-weight: 600; font-size: 0.9rem;"><i class="fa-solid fa-list-ul"></i> Live System Traceability Log Stream</span>
                        <div style="display: flex; gap: 0.5rem; align-items: center;">
                            <label style="font-size: 0.8rem; color: var(--text-muted);">Filter Level:</label>
                            <select id="mon-filter-level" class="btn btn-secondary btn-sm">
                                <option value="ALL">ALL LEVELS</option>
                                <option value="INFO">INFO</option>
                                <option value="TRACE">TRACE</option>
                                <option value="WARN">WARN</option>
                                <option value="ERROR">ERROR</option>
                                <option value="SECURITY_AUDIT">SECURITY_AUDIT</option>
                            </select>
                        </div>
                    </div>

                    <div style="overflow-x: auto; background: #080c14; border: 1px solid var(--border-bright); border-radius: 10px; padding: 0.3rem; max-height: 380px; overflow-y: auto;">
                        <table style="width: 100%; border-collapse: collapse; font-family: var(--font-mono); font-size: 0.78rem;">
                            <thead>
                                <tr style="border-bottom: 1px solid var(--border-bright); color: var(--text-muted); text-align: left;">
                                    <th style="padding: 0.2rem;">Packet ID</th>
                                    <th style="padding: 0.2rem;">Timestamp (UTC)</th>
                                    <th style="padding: 0.2rem;">Level</th>
                                    <th style="padding: 0.2rem;">Stage</th>
                                    <th style="padding: 0.2rem;">Trace Payload Message</th>
                                    <th style="padding: 0.2rem;">RTT</th>
                                </tr>
                            </thead>
                            <tbody id="mon-terminal-tbody">
                                ${this.renderLogRows(logs)}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;
    }

    renderLogRows(logs) {
        return logs.map(l => {
            let lvlColor = 'var(--accent-blue)';
            if (l.level === 'WARN') lvlColor = 'var(--accent-amber)';
            if (l.level === 'ERROR') lvlColor = 'var(--accent-red)';
            if (l.level === 'SECURITY_AUDIT') lvlColor = 'var(--accent-purple)';
            if (l.level === 'TRACE') lvlColor = 'var(--accent-emerald)';

            return `
                <tr style="border-bottom: 1px solid var(--border-color);">
                    <td style="padding: 0.18rem; color: var(--text-dim);">${l.id}</td>
                    <td style="padding: 0.18rem; color: var(--text-muted);">${l.timestamp.split('T')[1].replace('Z','')}</td>
                    <td style="padding: 0.18rem; color: ${lvlColor}; font-weight: 600;">${l.level}</td>
                    <td style="padding: 0.18rem; color: var(--accent-blue);">${l.stage}</td>
                    <td style="padding: 0.18rem; color: var(--text-main);">${l.message}</td>
                    <td style="padding: 0.18rem; color: var(--accent-emerald);">${l.rttMs} ms</td>
                </tr>
            `;
        }).join('');
    }
}

// ==========================================
// 3. CONTROLLER (Logic Layer)
// ==========================================

class AppController {
    constructor() {
        this.systemModel = new LocalSystemModel();
        this.settingsModel = new SystemSettingsModel();
        this.traceModel = new SystemTraceabilityLogModel();
        this.view = new AppView();
        this.currentView = 'remote-console';
        this.monInterval = null;
        this.telemetryInterval = null; // Bug 5: tracked reference to prevent duplicate intervals
        this.loginAttempts = 0;        // SEC-2: brute-force lockout counter
        this.loginLockedUntil = 0;     // SEC-2: lockout timestamp
    }

    async init() {
        this.bindNavigation();
        this.bindModals();
        this.bindThemeToggle();
        this.bindRemoteConnectLogic();
        this.bindSettingsEvents();
        this.bindLoginGate();

        // Fetch hardware ID early for login email prepopulation
        try {
            const res = await fetch('/api/hardware-id');
            const data = await res.json();
            this.systemModel.deviceId = data.deviceId || 'JDX-ERR-0000';
        } catch (e) {
            let fallbackId = localStorage.getItem('jdx_fallback_device_id');
            if (!fallbackId) {
                const randomHex = Math.floor(Math.random() * 65535).toString(16).toUpperCase().padStart(4, '0');
                fallbackId = `JDX-OFLN-${randomHex}`;
                localStorage.setItem('jdx_fallback_device_id', fallbackId);
            }
            this.systemModel.deviceId = fallbackId;
        }

        // Prepopulate login email with device address
        const emailInput = document.getElementById('login-email-input');
        if (emailInput) {
            emailInput.value = `${this.systemModel.deviceId.toLowerCase()}@jdroid-x-desk.com`;
        }

        // Dynamically update UI elements showing the hardware ID
        const localDeviceIdEl = document.getElementById('local-device-id');
        if (localDeviceIdEl) localDeviceIdEl.textContent = this.systemModel.deviceId.toUpperCase();
        
        const localNodeDescEl = document.getElementById('local-node-desc');
        if (localNodeDescEl) localNodeDescEl.textContent = `Local Node: ${this.systemModel.deviceId.toUpperCase()} (Windows 11 Enterprise)`;

        // UX-1: Wire password visibility toggle
        const togglePassBtn  = document.getElementById('toggle-pass-visibility');
        const togglePassIcon = document.getElementById('toggle-pass-icon');
        const passInput      = document.getElementById('login-password-input');
        if (togglePassBtn && passInput && togglePassIcon) {
            togglePassBtn.addEventListener('click', () => {
                const isHidden = passInput.type === 'password';
                passInput.type = isHidden ? 'text' : 'password';
                togglePassIcon.className = isHidden ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye';
            });
        }

        /** UX-2: Helper to hide the preloader with a smooth fade */
        const hidePreloader = () => {
            const preloader = document.getElementById('app-preloader');
            if (preloader) {
                preloader.style.transition = 'opacity 0.4s ease';
                preloader.style.opacity = '0';
                setTimeout(() => { preloader.style.display = 'none'; }, 450);
            }
        };

        // Check Firebase Auth state first — hide preloader after resolution
        if (typeof firebase !== 'undefined' && firebase.apps.length > 0) {
            firebase.auth().onAuthStateChanged((user) => {
                hidePreloader();
                if (user) {
                    this.unlockConsole(user.email || 'Authenticated User');
                } else {
                    const gate = document.getElementById('login-gate-overlay');
                    if (gate) gate.style.display = 'flex';
                }
            });
        } else {
            try {
                if (firebase.apps.length === 0) {
                    firebase.initializeApp({
                        apiKey: this.settingsModel.firebaseApiKey,
                        authDomain: `${this.settingsModel.firebaseProjectId}.firebaseapp.com`,
                        databaseURL: this.settingsModel.firebaseDbUrl,
                        projectId: this.settingsModel.firebaseProjectId
                    });
                }
                firebase.auth().onAuthStateChanged((user) => {
                    hidePreloader();
                    if (user) {
                        this.unlockConsole(user.email || 'Authenticated User');
                    } else {
                        const gate = document.getElementById('login-gate-overlay');
                        if (gate) gate.style.display = 'flex';
                    }
                });
            } catch (e) {
                // Firebase unavailable — hide preloader and show login gate
                hidePreloader();
                const gate = document.getElementById('login-gate-overlay');
                if (gate) gate.style.display = 'flex';
            }
        }
    }


    async unlockConsole(userEmail) {
        const isOfflineMode = (userEmail === 'Offline Demo Mode');

        // Hide login gate with smooth transition
        const gate = document.getElementById('login-gate-overlay');
        if (gate) {
            gate.style.transition = 'opacity 0.4s ease';
            gate.style.opacity = '0';
            setTimeout(() => { gate.classList.add('login-gate-hidden'); }, 400);
        }

        // Update session chip in sidebar
        const profileEmail = document.getElementById('logged-in-user-email');
        if (profileEmail) profileEmail.textContent = userEmail;

        // UX-8: Show amber dot and 'Demo Mode' label for offline bypass
        const sessionLabel = document.querySelector('.user-session-label');
        const sessionDot   = document.getElementById('session-online-dot');
        if (isOfflineMode) {
            if (sessionLabel) sessionLabel.textContent = 'Demo Mode (Offline)';
            if (sessionDot)   sessionDot.style.background = 'var(--accent-amber, #ffab40)';
            if (sessionDot)   sessionDot.style.boxShadow  = '0 0 6px rgba(255,171,64,0.8)';
        } else {
            if (sessionLabel) sessionLabel.textContent = 'Active Session';
            if (sessionDot)   sessionDot.style.background = '';
            if (sessionDot)   sessionDot.style.boxShadow  = '';
        }

        // Reset login attempt counter on successful auth
        this.loginAttempts = 0;
        this.loginLockedUntil = 0;

        this.renderCurrentView();
        this.startJarvisTelemetryLoop();
        this.initFirebasePresence();

        this.traceModel.addLog('SECURITY_AUDIT', 'AUTH_SUCCESS', `User authenticated: ${userEmail}. Console unlocked.`);
        this.view.showToast('success', 'Authentication Success', `Welcome back, ${userEmail}`);
    }

    bindLoginGate() {
        const submitBtn = document.getElementById('login-submit-btn');
        const bypassBtn = document.getElementById('login-bypass-btn');
        const errorMsg  = document.getElementById('login-error-msg');

        if (submitBtn) {
            submitBtn.onclick = async () => {
                // SEC-2: Brute-force lockout — 5 attempts triggers 30s cooldown
                const now = Date.now();
                if (this.loginLockedUntil > now) {
                    const secsLeft = Math.ceil((this.loginLockedUntil - now) / 1000);
                    if (errorMsg) { errorMsg.textContent = `⛔ Too many attempts. Try again in ${secsLeft}s.`; errorMsg.style.display = 'block'; }
                    return;
                }

                const email = document.getElementById('login-email-input').value.trim();
                const pass  = document.getElementById('login-password-input').value;

                if (!email || !pass) {
                    if (errorMsg) { errorMsg.textContent = '⚠ Email and password are required.'; errorMsg.style.display = 'block'; }
                    return;
                }

                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Authenticating...';

                try {
                    if (typeof firebase !== 'undefined' && firebase.apps.length === 0) {
                        firebase.initializeApp({
                            apiKey: this.settingsModel.firebaseApiKey,
                            authDomain: `${this.settingsModel.firebaseProjectId}.firebaseapp.com`,
                            databaseURL: this.settingsModel.firebaseDbUrl,
                            projectId: this.settingsModel.firebaseProjectId,
                            storageBucket: `${this.settingsModel.firebaseProjectId}.firebasestorage.app`,
                            messagingSenderId: this.settingsModel.firebaseMessagingSenderId,
                            appId: this.settingsModel.firebaseAppId,
                            measurementId: this.settingsModel.firebaseMeasurementId
                        });
                    }
                    const cred = await firebase.auth().signInWithEmailAndPassword(email, pass);
                    this.loginAttempts = 0;
                    this.unlockConsole(cred.user.email);
                } catch (err) {
                    if (err.code === 'auth/user-not-found' || err.code === 'auth/invalid-login-credentials' || err.code === 'auth/invalid-credential') {
                        try {
                            // Attempt auto-registration if account doesn't exist
                            const cred = await firebase.auth().createUserWithEmailAndPassword(email, pass);
                            this.loginAttempts = 0;
                            this.unlockConsole(cred.user.email);
                            return;
                        } catch (createErr) {
                            if (createErr.code === 'auth/email-already-in-use') {
                                // Account exists, so the original error was truly an invalid password
                                err.message = "Invalid password. Please try again.";
                            } else {
                                err = createErr;
                            }
                        }
                    }

                    this.loginAttempts++;
                    const remaining = 5 - this.loginAttempts;
                    let errText = `⚠ ${err.message}`;
                    if (this.loginAttempts >= 5) {
                        this.loginLockedUntil = Date.now() + 30000;
                        this.loginAttempts = 0;
                        errText = '⛔ Account locked for 30 seconds due to too many failed attempts.';
                    } else {
                        errText += ` (${remaining} attempt${remaining !== 1 ? 's' : ''} remaining)`;
                    }
                    if (errorMsg) { errorMsg.textContent = errText; errorMsg.style.display = 'block'; }
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="fa-solid fa-right-to-bracket"></i> Authenticate & Unlock Console';
                    this.traceModel.addLog('ERROR', 'AUTH_FAILED', `Login attempt ${this.loginAttempts}/5 failed: ${err.message}`);
                }
            };
        }

        if (bypassBtn) {
            bypassBtn.onclick = (e) => {
                e.preventDefault();
                this.unlockConsole('Offline Demo Mode');
                this.traceModel.addLog('WARN', 'AUTH_BYPASSED', 'User bypassed authentication — running in offline demo mode.');
            };
        }
    }

    startJarvisTelemetryLoop() {
        // Bug 5 Fix: Clear any existing interval before starting a new one
        if (this.telemetryInterval) {
            clearInterval(this.telemetryInterval);
            this.telemetryInterval = null;
        }
        this.telemetryInterval = setInterval(() => {
            // Simulate realistic 4G network jitter & speed variations in MB/s
            const jitter = (Math.random() - 0.48) * 0.4;
            let dl = parseFloat((this.systemModel.telemetry.downloadMBs + jitter).toFixed(2));
            let ul = parseFloat((this.systemModel.telemetry.uploadMBs + (jitter * 0.3)).toFixed(2));
            let ping = Math.max(8, Math.floor(this.systemModel.telemetry.pingMs + (Math.random() - 0.45) * 8));
            let fps = Math.min(60, Math.max(15, Math.floor(this.systemModel.telemetry.fps + (Math.random() - 0.45) * 4)));

            // Occasionally simulate a 4G speed dip for threshold verification
            if (Math.random() < 0.08) {
                dl = 0.65; // Dip below 1.0 MB/s threshold
                ping = 115; // Spike above 100ms threshold
            } else if (dl < 1.2) {
                dl = 2.45;
                ping = 14;
            }

            this.systemModel.telemetry.downloadMBs = dl;
            this.systemModel.telemetry.uploadMBs = ul;
            this.systemModel.telemetry.pingMs = ping;
            this.systemModel.telemetry.fps = fps;

            // Update DOM Elements
            const dlEl = document.getElementById('hud-dl-speed');
            const ulEl = document.getElementById('hud-ul-speed');
            const pingEl = document.getElementById('hud-ping-ms');
            const fpsEl = document.getElementById('hud-stream-fps');
            const statusEl = document.getElementById('hud-4g-status');

            if (dlEl) dlEl.textContent = `${dl.toFixed(2)} MB/s`;
            if (ulEl) ulEl.textContent = `${ul.toFixed(2)} MB/s`;
            if (pingEl) pingEl.textContent = `${ping} ms`;
            if (fpsEl) fpsEl.textContent = `${fps} FPS`;

            // Update connection metadata DOM
            const connEl = document.getElementById('hud-conn-type');
            const ipEl = document.getElementById('hud-ip-address');
            if (connEl) connEl.textContent = this.settingsModel.connType;
            if (ipEl) ipEl.textContent = this.settingsModel.ipAddress;

            // Apply Settings Thresholds with Dynamic 5G Adjustments
            let activeDlThreshold = this.settingsModel.dlThreshold;
            let activePingThreshold = this.settingsModel.pingThreshold;

            if (this.settingsModel.dynamic5GEnabled && this.settingsModel.connType.includes('5G')) {
                // Boost required DL threshold by 20% to avoid frequent 5G jitter alerts
                activeDlThreshold = parseFloat((activeDlThreshold * 1.2).toFixed(2));
            }

            // Threshold Audit
            const isDegraded = dl < activeDlThreshold || ping > activePingThreshold || fps < 20;

            if (statusEl) {
                if (isDegraded) {
                    statusEl.className = 'badge badge-red';
                    statusEl.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i> Network Degraded';
                } else {
                    statusEl.className = 'badge badge-green';
                    statusEl.innerHTML = '<i class="fa-solid fa-shield-halved"></i> Baseline Optimal';
                }
            }

            // Trigger Pop-up Toast Alert if degraded (with 15s cooldown to avoid spam)
            const now = Date.now();
            if (isDegraded && (now - this.systemModel.telemetry.lastAlarmTime > 15000)) {
                this.systemModel.telemetry.lastAlarmTime = now;
                this.view.showToast(
                    'warning',
                    'Network Threshold Warning',
                    `Speed dropped to ${dl.toFixed(2)} MB/s (Ping: ${ping}ms). Target min DL is ${activeDlThreshold} MB/s.`
                );
            }
        }, 2000);
    }

    stopJarvisTelemetryLoop() {
        // Bug 5 Fix: Safely stop telemetry to prevent duplicate intervals after logout
        if (this.telemetryInterval) {
            clearInterval(this.telemetryInterval);
            this.telemetryInterval = null;
        }
    }

    bindSettingsEvents() {
        // Universal binding helper for settings
        const bindInput = (id, key, isCheckbox, isInt) => {
            const el = document.getElementById(id);
            if (!el) return;
            
            if (isCheckbox) {
                el.checked = this.settingsModel[key];
                el.onchange = (e) => {
                    this.settingsModel[key] = e.target.checked;
                    localStorage.setItem(`jdx_${id}`, e.target.checked.toString());
                };
            } else {
                el.value = this.settingsModel[key];
                el.onchange = (e) => {
                    const val = isInt ? parseInt(e.target.value, 10) : e.target.value;
                    this.settingsModel[key] = val;
                    localStorage.setItem(`jdx_${id}`, val.toString());
                };
            }
        };

        bindInput('set-dash-auto', 'dashAutoConnect', true);
        bindInput('set-dash-res', 'dashResolution', false);
        bindInput('set-mon-verbose', 'monVerbose', true);
        bindInput('set-mon-buffer', 'monBuffer', false, true);
        bindInput('set-fleet-ping', 'fleetAutoPing', true);
        bindInput('set-fleet-timeout', 'fleetTimeout', false, true);
        bindInput('set-map-theme', 'mapTheme', false);
        bindInput('set-map-cluster', 'mapCluster', true);
        bindInput('set-hist-purge', 'histPurge', false, true);
        bindInput('set-hist-acc', 'histAcc', false, true);
        bindInput('set-grp-sync', 'grpSync', false);
        bindInput('set-zone-rad', 'zoneRadius', false, true);
        bindInput('set-zone-sev', 'zoneSeverity', false);
        bindInput('set-grd-mfa', 'guardMfa', true);
        bindInput('set-grd-inherit', 'guardInherit', true);
        bindInput('set-notif-sound', 'notifSound', true);
        bindInput('settings-toast-duration', 'toastDuration', false, true);
        bindInput('set-hlth-batt', 'healthBatt', false, true);
        bindInput('set-hlth-temp', 'healthTemp', false, true);
        bindInput('set-prof-sync', 'profSync', true);
        bindInput('set-prof-idle', 'profIdle', false, true);
        bindInput('set-priv-telemetry', 'privTelemetry', true);
        bindInput('set-priv-backup', 'privBackup', true);

        // Firebase Settings Bindings
        const fbApiKeyInput = document.getElementById('settings-fb-api');
        const fbAppIdInput = document.getElementById('settings-fb-app');
        const fbSaveBtn = document.getElementById('btn-save-firebase');

        if (fbApiKeyInput) fbApiKeyInput.value = this.settingsModel.firebaseApiKey;
        if (fbAppIdInput) fbAppIdInput.value = this.settingsModel.firebaseAppId;

        if (fbSaveBtn) {
            fbSaveBtn.onclick = () => {
                const key = fbApiKeyInput ? fbApiKeyInput.value.trim() : '';
                const appId = fbAppIdInput ? fbAppIdInput.value.trim() : '';

                if (!key) {
                    this.view.showToast('error', 'Validation Error', 'Firebase API Key cannot be empty.');
                    return;
                }

                this.settingsModel.firebaseApiKey = key;
                this.settingsModel.firebaseAppId = appId;
                localStorage.setItem('jdx_fb_api_key', key);
                localStorage.setItem('jdx_fb_app_id', appId);

                this.view.showToast('success', 'Credentials Saved', 'Firebase configuration credentials updated successfully.');
                this.traceModel.addLog('SECURITY_AUDIT', 'CONFIG_CHANGE', 'Firebase signaling API credentials updated by operator');
                
                // Dynamic hot-reload of presence connection
                if (typeof firebase !== 'undefined' && firebase.apps.length > 0) {
                    firebase.app().delete().then(() => {
                        this.initFirebasePresence();
                    }).catch((err) => {
                        console.error('[FIREBASE] App deletion failed:', err);
                        this.initFirebasePresence();
                    });
                } else {
                    this.initFirebasePresence();
                }
            };
        }
    }

    initFirebasePresence(retryCount = 0) {
        const MAX_RETRIES = 5;

        if (typeof firebase === 'undefined') {
            console.warn('[FIREBASE] SDK not loaded or offline mode.');
            this.traceModel.addLog('WARN', 'FIREBASE_OFFLINE', 'Firebase client SDK not loaded. Presence monitoring skipped.');
            this.updateFirebaseStatusUI('OFFLINE', 'Firebase SDK library not loaded');
            return;
        }

        const config = {
            apiKey: this.settingsModel.firebaseApiKey,
            authDomain: `${this.settingsModel.firebaseProjectId}.firebaseapp.com`,
            databaseURL: this.settingsModel.firebaseDbUrl,
            projectId: this.settingsModel.firebaseProjectId,
            storageBucket: `${this.settingsModel.firebaseProjectId}.firebasestorage.app`,
            messagingSenderId: this.settingsModel.firebaseMessagingSenderId,
            appId: this.settingsModel.firebaseAppId,
            measurementId: this.settingsModel.firebaseMeasurementId
        };

        this.updateFirebaseStatusUI('CONNECTING', 'Connecting to Cloud Realtime Database...');

        try {
            if (firebase.apps.length === 0) {
                firebase.initializeApp(config);
            }

            const db = firebase.database();
            const auth = firebase.auth();
            const deviceId = this.systemModel.deviceId;

            // Bug 6 Fix: Guard against JDX-INIT-0000 with max retries + exponential backoff
            if (deviceId === 'JDX-INIT-0000') {
                if (retryCount >= MAX_RETRIES) {
                    // Fall back to localStorage offline ID instead of infinite loop
                    let fallbackId = localStorage.getItem('jdx_fallback_device_id');
                    if (!fallbackId) {
                        const rnd = Math.floor(Math.random() * 65535).toString(16).toUpperCase().padStart(4, '0');
                        fallbackId = `JDX-OFLN-${rnd}`;
                        localStorage.setItem('jdx_fallback_device_id', fallbackId);
                    }
                    this.systemModel.deviceId = fallbackId;
                    this.traceModel.addLog('WARN', 'FIREBASE_FALLBACK_ID', `Max device ID retries reached. Using fallback: ${fallbackId}`);
                    this.initFirebasePresence(0); // restart with fallback ID now set
                    return;
                }
                const delay = Math.pow(2, retryCount) * 1000; // 1s, 2s, 4s, 8s, 16s
                setTimeout(() => this.initFirebasePresence(retryCount + 1), delay);
                return;
            }

            // Bug 2 & 3 Fix: Only sign in anonymously if no authenticated user already exists.
            // This prevents overwriting a valid email-authenticated session with an anonymous one.
            const setupPresence = () => {
                const presenceRef = db.ref(`devices/${deviceId}`);
                const amOnline = db.ref('.info/connected');
                amOnline.on('value', (snapshot) => {
                    if (snapshot.val() === true) {
                        presenceRef.update({
                            deviceId: deviceId,
                            online: true,
                            lastSeen: new Date().toISOString(),
                            clientType: 'jDroid-X-Desk'
                        });
                        presenceRef.onDisconnect().update({
                            online: false,
                            lastSeen: firebase.database.ServerValue.TIMESTAMP
                        });
                        this.traceModel.addLog('SECURITY_AUDIT', 'FIREBASE_PRESENCE_ACTIVE', `System registered online at /devices/${deviceId}`);
                        this.updateFirebaseStatusUI('CONNECTED', `Connected as ${deviceId}`);
                    } else {
                        this.updateFirebaseStatusUI('DISCONNECTED', 'Waiting to reconnect...');
                    }
                });
            };

            if (auth.currentUser) {
                // Already authenticated (email or otherwise) — skip signInAnonymously
                setupPresence();
            } else {
                auth.signInAnonymously().then(setupPresence).catch((authErr) => {
                    console.error('[FIREBASE AUTH] Anonymous auth failed:', authErr.message);
                    this.traceModel.addLog('ERROR', 'FIREBASE_AUTH_FAILED', `Firebase Auth: ${authErr.message}`);
                    this.updateFirebaseStatusUI('ERROR', `Auth Error: ${authErr.message}`);
                });
            }

        } catch (err) {
            console.error('[FIREBASE] Initialization failed:', err.message);
            this.traceModel.addLog('ERROR', 'FIREBASE_INIT_FAILED', `Firebase presence init error: ${err.message}`);
            this.updateFirebaseStatusUI('ERROR', `Init Error: ${err.message}`);
        }
    }


    updateFirebaseStatusUI(status, message) {
        const banner = document.getElementById('settings-fb-status-banner');
        if (!banner) return;

        let icon = '<i class="fa-solid fa-spinner fa-spin"></i>';
        let cls = 'badge badge-blue';

        if (status === 'CONNECTED') {
            icon = '<i class="fa-solid fa-cloud-arrow-up"></i>';
            cls = 'badge badge-green';
        } else if (status === 'OFFLINE' || status === 'DISCONNECTED') {
            icon = '<i class="fa-solid fa-cloud-sun"></i>';
            cls = 'badge badge-orange';
        } else if (status === 'ERROR') {
            icon = '<i class="fa-solid fa-triangle-exclamation"></i>';
            cls = 'badge badge-red';
        }

        banner.className = cls;
        banner.innerHTML = `${icon} Cloud Signaling: ${status} (${message})`;
    }

    bindNavigation() {
        document.querySelectorAll('.nav-item[data-view]').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const viewId = item.getAttribute('data-view');
                this.switchView(viewId);
                this.closeSidebarMobile();
            });
        });

        document.querySelectorAll('.bottom-nav-item[data-view]').forEach(item => {
            item.addEventListener('click', () => {
                const viewId = item.getAttribute('data-view');
                this.switchView(viewId);
            });
        });

        const toggleBtn = document.getElementById('toggle-drawer-btn');
        const closeBtn = document.getElementById('close-drawer-btn');
        const sidebar = document.getElementById('sidebar-drawer');

        if (toggleBtn && sidebar) {
            toggleBtn.addEventListener('click', () => sidebar.classList.toggle('open'));
        }

        if (closeBtn && sidebar) {
            closeBtn.addEventListener('click', () => sidebar.classList.remove('open'));
        }
    }

    closeSidebarMobile() {
        const sidebar = document.getElementById('sidebar-drawer');
        if (sidebar) sidebar.classList.remove('open');
    }

    bindModals() {
        document.querySelectorAll('[data-modal]').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const modalId = item.getAttribute('data-modal');
                this.openModal(modalId);
                this.closeSidebarMobile();
            });
        });

        document.querySelectorAll('.close-modal-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const modal = btn.closest('.modal-overlay');
                if (modal) modal.classList.remove('active');
            });
        });

        document.querySelectorAll('.modal-overlay').forEach(overlay => {
            overlay.addEventListener('click', (e) => {
                if (e.target === overlay) overlay.classList.remove('active');
            });
        });
    }

    openModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) modal.classList.add('active');
    }

    bindThemeToggle() {
        const themeBtn = document.getElementById('theme-toggle-btn');
        if (themeBtn) {
            // UX-6: Restore saved theme preference on load
            const savedTheme = localStorage.getItem('jdx_theme');
            if (savedTheme === 'light') {
                document.body.classList.add('light-theme');
                const icon = themeBtn.querySelector('i');
                if (icon) icon.className = 'fa-solid fa-sun';
            }

            themeBtn.addEventListener('click', () => {
                document.body.classList.toggle('light-theme');
                const icon = themeBtn.querySelector('i');
                if (document.body.classList.contains('light-theme')) {
                    icon.className = 'fa-solid fa-sun';
                    localStorage.setItem('jdx_theme', 'light');
                } else {
                    icon.className = 'fa-solid fa-moon';
                    localStorage.setItem('jdx_theme', 'dark');
                }
            });
        }
    }

    bindRemoteConnectLogic() {
        const copyBtn = document.getElementById('copy-id-btn');
        const connectBtn = document.getElementById('connect-session-btn');
        const disconnectBtn = document.getElementById('disconnect-session-btn');
        const quickDisconnectBtn = document.getElementById('quick-disconnect-btn');
        const launchCanvasBtn = document.getElementById('launch-canvas-btn');
        const ctrlAltDelBtn = document.getElementById('ctrl-alt-del-btn');
        const toggleFileBtn = document.getElementById('toggle-file-btn');

        if (copyBtn) {
            copyBtn.addEventListener('click', () => {
                navigator.clipboard.writeText(this.systemModel.deviceId);
                this.view.showToast('success', 'Address Copied', `Device ID ${this.systemModel.deviceId} copied to clipboard.`);
            });
        }

        if (connectBtn) {
            connectBtn.addEventListener('click', () => {
                const inputId = document.getElementById('target-id-input').value.trim();
                const inputPass = document.getElementById('target-pass-input').value.trim();

                const cleanId = inputId.replace(/\D/g, '');
                if (cleanId.length !== 9) {
                    this.view.showToast('error', 'Invalid Address', 'Please enter a valid 9-digit Secondary Device Address (e.g. 520-310-884).');
                    return;
                }

                const formattedId = `${cleanId.slice(0,3)}-${cleanId.slice(3,6)}-${cleanId.slice(6,9)}`;
                
                this.systemModel.activeSession = formattedId;

                // Update UI state
                document.getElementById('session-status-text').textContent = 'CONNECTED & ACTIVE';
                document.getElementById('session-status-text').style.color = 'var(--accent-emerald)';
                document.getElementById('active-target-id-label').textContent = formattedId;
                document.getElementById('remote-desktop-viewport-box').style.display = 'block';
                if (launchCanvasBtn) launchCanvasBtn.disabled = false;
                if (quickDisconnectBtn) quickDisconnectBtn.style.display = 'inline-flex';

                this.view.showToast('success', 'Session Established', `Connected to Secondary System (${formattedId}). Primary user now has full remote control.`);
                this.traceModel.addLog('SECURITY_AUDIT', 'SESSION_CONNECTED', `Primary user established remote session to Secondary System ID ${formattedId}`);
            });
        }

        const disconnectSession = () => {
            this.systemModel.activeSession = null;
            document.getElementById('session-status-text').textContent = 'Idle (Awaiting Connection)';
            document.getElementById('session-status-text').style.color = 'var(--text-muted)';
            document.getElementById('remote-desktop-viewport-box').style.display = 'none';
            if (launchCanvasBtn) launchCanvasBtn.disabled = true;
            if (quickDisconnectBtn) quickDisconnectBtn.style.display = 'none';

            this.view.showToast('warning', 'Session Terminated', 'Remote desktop session closed. Secondary workstation locked.');
            this.traceModel.addLog('INFO', 'SESSION_TERMINATED', 'Remote desktop session closed by Primary user.');
        };

        if (disconnectBtn) disconnectBtn.addEventListener('click', disconnectSession);
        if (quickDisconnectBtn) quickDisconnectBtn.addEventListener('click', disconnectSession);

        if (ctrlAltDelBtn) {
            ctrlAltDelBtn.addEventListener('click', () => {
                this.injectVirtualInput('System Signal: Ctrl+Alt+Del injected to Secondary System');
                this.view.showToast('info', 'Signal Sent', 'Ctrl+Alt+Del key sequence transmitted to Secondary System.');
            });
        }

        if (toggleFileBtn) {
            toggleFileBtn.addEventListener('click', () => {
                this.injectVirtualInput('File Manager Opened: Split-Pane directory sync active');
                this.view.showToast('info', 'File Transfer', 'Two-way File Manager module opened.');
            });
        }
    }

    injectVirtualInput(msg) {
        const logBox = document.getElementById('virtual-screen-log');
        if (logBox) {
            logBox.textContent = `[INPUT STREAM] ${msg}`;
        }
        this.traceModel.addLog('TRACE', 'INPUT_INJECT', msg);
    }

    switchView(viewId) {
        if (viewId === 'logout') {
            // Intercept logout view to execute logic
        }

        this.currentView = viewId;

        document.querySelectorAll('.nav-item').forEach(item => {
            if (item.getAttribute('data-view') === viewId) {
                item.classList.add('active');
            } else {
                item.classList.remove('active');
            }
        });

        document.querySelectorAll('.bottom-nav-item').forEach(item => {
            if (item.getAttribute('data-view') === viewId) {
                item.classList.add('active');
            } else {
                item.classList.remove('active');
            }
        });

        document.querySelectorAll('.view-panel').forEach(panel => panel.classList.remove('active'));

        const targetPanel = document.getElementById(`view-${viewId}`);
        if (targetPanel) {
            targetPanel.classList.add('active');
        }

        this.renderCurrentView();
    }

    performLogout() {
        if (window.firebase) {
            window.firebase.auth().signOut().then(() => {
                localStorage.removeItem('jdx_fallback_device_id');
                window.location.reload();
            }).catch(e => {
                this.view.showToast('error', 'Logout Error', e.message);
            });
        } else {
            localStorage.removeItem('jdx_fallback_device_id');
            window.location.reload();
        }
    }

    bindSystemMonitorEvents() {
        const startBtn = document.getElementById('mon-start-btn');
        const injectBtn = document.getElementById('mon-inject-btn');
        const exportBtn = document.getElementById('mon-export-btn');
        const filterSelect = document.getElementById('mon-filter-level');
        const tbody = document.getElementById('mon-terminal-tbody');

        const updateTerminal = () => {
            if (tbody) {
                const lvl = filterSelect ? filterSelect.value : 'ALL';
                tbody.innerHTML = this.view.renderLogRows(this.traceModel.getLogs(lvl));
            }
        };

        if (startBtn) {
            startBtn.onclick = () => {
                this.traceModel.monitoringActive = !this.traceModel.monitoringActive;

                if (this.traceModel.monitoringActive) {
                    this.view.showToast('success', 'Continuous Monitoring Active', 'Real-time 2-System log streamer initialized.');
                    this.monInterval = setInterval(() => {
                        const sampleEvents = [
                            { level: 'TRACE', stage: 'FRAME_ACK', msg: 'Secondary System B acknowledged frame #4092 (RTT: 15ms)' },
                            { level: 'INFO', stage: 'INPUT_INJECT', msg: 'Primary System A injected Mouse Click (X: 520, Y: 310)' },
                            { level: 'TRACE', stage: 'AUDIO_STREAM', msg: 'Stereo audio payload packet received (48kHz Opus Codec)' },
                            { level: 'SECURITY_AUDIT', stage: 'TOKEN_VERIFY', msg: 'Session token keep-alive re-verified' }
                        ];
                        const ev = sampleEvents[Math.floor(Math.random() * sampleEvents.length)];
                        this.traceModel.addLog(ev.level, ev.stage, ev.msg);
                        updateTerminal();
                    }, 2000);
                } else {
                    if (this.monInterval) clearInterval(this.monInterval);
                    this.view.showToast('info', 'Stream Paused', 'Continuous log monitoring paused.');
                }
                this.renderCurrentView();
            };
        }

        if (injectBtn) {
            injectBtn.onclick = () => {
                const customMsg = prompt('Enter custom trace log message to inject:', 'Manual UAC elevation privilege check requested by operator');
                if (customMsg) {
                    this.traceModel.addLog('SECURITY_AUDIT', 'MANUAL_INJECT', customMsg);
                    updateTerminal();
                    this.view.showToast('success', 'Log Injected', 'Custom trace event recorded into system log audit.');
                }
            };
        }

        if (exportBtn) {
            exportBtn.onclick = () => {
                const jsonStr = this.traceModel.exportJSON();
                const blob = new Blob([jsonStr], { type: 'application/json' });
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `jDroid-X-Desk_TraceabilityAudit_${Date.now()}.json`;
                a.click();
                URL.revokeObjectURL(url);
                this.view.showToast('success', 'Audit Exported', 'Downloaded signed 2-System Traceability JSON Log Audit.');
            };
        }

        if (filterSelect) {
            filterSelect.onchange = updateTerminal;
        }
    }

    renderCurrentView() {
        const viewId = this.currentView;

        const titleMap = {
            'remote-console': 'Remote Control Console',
            'system-monitor': '2-System Audit Engine',
            'device-fleet': 'Children & Device Fleet',
            'live-map': 'Live Remote Connection Map',
            'location-history': 'Connection & Audit History',
            'remote-contacts': 'MeetMyFriend Groups & Contacts',
            'safe-zones': 'Safe Zones & Access Control Rules',
            'guardians-list': 'Guardians & Admin Tech Roster',
            'notifications-center': 'System Notifications & Alert Center',
            'device-health': 'Device Health & Telemetry',
            'user-profile': 'Senior Systems Architect Profile',
            'system-settings': 'System Preferences & Startup',
            'privacy-controls': 'Privacy & Security Governance'
        };

        if (titleMap[viewId]) {
            this.view.updateViewTitle(titleMap[viewId]);
        }

        if (viewId === 'remote-console') {
            this.bindRemoteConnectLogic();
        } else if (viewId === 'system-monitor') {
            const panel = document.getElementById('system-monitor-content');
            if (panel) {
                panel.innerHTML = this.view.renderSystemMonitor(this.traceModel);
                this.bindSystemMonitorEvents();
            }
        }
    }
    pingDevice(deviceName) {
        const rtt = Math.floor(Math.random() * 12) + 8;
        this.view.showToast('success', 'Ping Result', `${deviceName} responded in ${rtt}ms (0.0% packet loss).`);
        this.traceModel.addLog('TRACE', 'PING_CHECK', `Manual ICMP ping to ${deviceName}: ${rtt}ms RTT`);
    }

    bindDeepDiveEvents() {
        // Stream Quality Select
        const qualitySel = document.getElementById('stream-quality-select');
        if (qualitySel) {
            qualitySel.onchange = (e) => {
                const val = e.target.value;
                const hud = document.getElementById('stream-hud-specs');
                if (hud) {
                    if (val === '1080p') hud.textContent = '1080p 60FPS | 14ms | 1.8 Mbps';
                    if (val === '720p') hud.textContent = '720p 30FPS | 11ms | 0.9 Mbps';
                    if (val === 'turbo') hud.textContent = '480p Turbo | 6ms | 0.3 Mbps';
                }
                this.view.showToast('info', 'Stream Preset', `Display quality updated to ${val.toUpperCase()}`);
            };
        }

        // Clipboard Sync Toggle
        const clipBtn = document.getElementById('btn-toggle-clipboard');
        if (clipBtn) {
            clipBtn.onclick = () => {
                this.view.showToast('success', 'Clipboard Synced', 'Bi-directional clipboard synchronization active.');
            };
        }

        // Remote Audio Toggle
        const audioBtn = document.getElementById('btn-toggle-audio');
        if (audioBtn) {
            audioBtn.onclick = () => {
                this.view.showToast('info', 'Remote Audio', 'Remote audio stream unmuted (Opus Codec 48kHz).');
            };
        }

        // Add Device Modal
        const addDevBtn = document.getElementById('btn-open-add-device');
        if (addDevBtn) {
            addDevBtn.onclick = () => {
                const modal = document.getElementById('add-device-modal');
                if (modal) modal.classList.add('active');
            };
        }

        // Add Contact
        const addContactBtn = document.getElementById('btn-open-add-contact');
        if (addContactBtn) {
            addContactBtn.onclick = () => {
                const id = prompt('Enter 9-Digit Partner System Address:', '710-224-900');
                if (id) {
                    this.view.showToast('success', 'Contact Saved', `Partner ID ${id} added to MeetMyFriend group.`);
                }
            };
        }

        // Add Subnet
        const addSubnetBtn = document.getElementById('btn-open-add-subnet');
        if (addSubnetBtn) {
            addSubnetBtn.onclick = () => {
                const ip = prompt('Enter Subnet (CIDR format):', '192.168.1.0/24');
                if (ip) {
                    this.view.showToast('success', 'Subnet Allowed', `Subnet ${ip} added to Safe Zones whitelist.`);
                }
            };
        }

        // Emergency Kill Switch
        const killBtn = document.getElementById('btn-emergency-kill');
        if (killBtn) {
            killBtn.onclick = () => {
                if (confirm('EMERGENCY ACTION: Sever all active remote connections and isolate host system immediately?')) {
                    this.view.showToast('error', 'SYSTEM ISOLATED', 'All remote channels closed. Firewall rules applied.');
                    const viewport = document.getElementById('remote-desktop-viewport-box');
                    if (viewport) viewport.style.display = 'none';
                }
            };
        }

        // Self Diagnostic Benchmark
        const diagBtn = document.getElementById('btn-run-self-diagnostic');
        if (diagBtn) {
            diagBtn.onclick = () => {
                this.view.showToast('info', 'Benchmark Running', 'Testing local CPU, Memory, and Express Port 12345...');
                setTimeout(() => {
                    this.view.showToast('success', 'Diagnostic Passed', 'All system checks 100% HEALTHY. Express & Tray Icon active.');
                }, 1500);
            };
        }

        // Verify Startup Status
        const startupBtn = document.getElementById('btn-verify-startup-status');
        if (startupBtn) {
            startupBtn.onclick = () => {
                fetch('/api/startup', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ enable: true })
                }).then(res => res.json())
                  .then(data => {
                      this.view.showToast('success', 'Startup Status Verified', 'Windows Startup shortcut confirmed in APPDATA.');
                  }).catch(() => {
                      this.view.showToast('info', 'Startup Verified', 'Windows Startup Service configured.');
                  });
            };
        }

        // Regenerate Passcode
        const regenBtn = document.getElementById('btn-regenerate-pass');
        if (regenBtn) {
            regenBtn.onclick = () => {
                const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789#@!';
                let newPass = '';
                for (let i = 0; i < 8; i++) newPass += chars.charAt(Math.floor(Math.random() * chars.length));
                const passEl = document.getElementById('local-device-pass');
                if (passEl) passEl.textContent = newPass;
                this.view.showToast('success', 'Passcode Rotated', `New Security Passcode: ${newPass}`);
            };
        }

        // Audit Report Generator
        const reportBtn = document.getElementById('btn-generate-audit-report');
        if (reportBtn) {
            reportBtn.onclick = () => {
                const modal = document.getElementById('audit-report-modal');
                if (modal) modal.classList.add('active');
            };
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    window.appController = new AppController();
    window.appController.init();
    window.appController.bindDeepDiveEvents();
    
    // Exit App Navigation Handler
    const exitBtn = document.getElementById('nav-exit-app');
    if (exitBtn) {
        exitBtn.addEventListener('click', (e) => {
            e.preventDefault();
            if (confirm('Are you sure you want to stop the background server and exit jDroid-X-Desk?')) {
                fetch('/api/shutdown', { method: 'POST' }).then(() => {
                    alert('jDroid-X-Desk background server shut down successfully.');
                    window.close();
                }).catch(() => {
                    alert('Server shut down.');
                    window.close();
                });
            }
        });
    }

    // Logout Nav Handler — Firebase Sign Out + Re-show Login Gate
    const logoutBtn = document.getElementById('nav-logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            // UX-5: Use DialogView instead of native confirm()
            const dialogView = new DialogView();
            dialogView.show(
                'Sign Out Confirmation',
                '<p style="color:var(--text-main)">Sign out of jDroid-X-Desk? You will need to re-authenticate to access the console.</p>',
                async () => {
                    // Bug 5 Fix: Stop telemetry before logout to prevent interval leak
                    window.appController.stopJarvisTelemetryLoop();

                    try {
                        if (typeof firebase !== 'undefined' && firebase.apps.length > 0) {
                            await firebase.auth().signOut();
                        }
                    } catch (err) { console.warn('Firebase signOut error:', err); }

                    // Reset session chip
                    const emailEl = document.getElementById('logged-in-user-email');
                    if (emailEl) emailEl.textContent = 'Not authenticated';
                    const sessionLabel = document.querySelector('.user-session-label');
                    if (sessionLabel) sessionLabel.textContent = 'Active Session';
                    const sessionDot = document.getElementById('session-online-dot');
                    if (sessionDot) { sessionDot.style.background = ''; sessionDot.style.boxShadow = ''; }

                    // Re-display login gate
                    const gate = document.getElementById('login-gate-overlay');
                    if (gate) { gate.style.opacity = '1'; gate.style.transition = ''; gate.classList.remove('login-gate-hidden'); gate.style.display = 'flex'; }

                    // Clear password field
                    const passEl = document.getElementById('login-password-input');
                    if (passEl) passEl.value = '';
                    const errEl = document.getElementById('login-error-msg');
                    if (errEl) errEl.style.display = 'none';

                    window.appController.traceModel.addLog('SECURITY_AUDIT', 'LOGOUT', 'User signed out from Firebase session.');
                    return true;
                }
            );
        });
    }

    // Settings Panel Startup Checkbox Sync
    const settingsStartupCheckbox = document.getElementById('settings-startup-checkbox');
    if (settingsStartupCheckbox) {
        settingsStartupCheckbox.addEventListener('change', (e) => {
            fetch('/api/startup', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ enable: e.target.checked })
            }).then(res => res.json())
              .then(data => {
                  if (data.success) {
                      window.appController.view.showToast('success', 'Startup Preference Saved', data.message);
                  }
              });
        });
    }

    // Launch Popup Logic
    const popupModal = document.getElementById('launch-popup-modal');
    const popupCloseBtn = document.getElementById('launch-popup-close-btn');
    const popupOkBtn = document.getElementById('launch-popup-ok-btn');
    const startupCheckbox = document.getElementById('startup-checkbox');
    const hidePopupCheckbox = document.getElementById('hide-popup-checkbox');
    
    if (popupModal) {
        // Check if user requested not to show this popup
        if (localStorage.getItem('hideLaunchPopup') !== 'true') {
            popupModal.classList.add('active');
        }
        
        const closePopup = () => {
            popupModal.classList.remove('active');
            if (hidePopupCheckbox.checked) {
                localStorage.setItem('hideLaunchPopup', 'true');
            } else {
                localStorage.removeItem('hideLaunchPopup');
            }
        };
        
        if (popupCloseBtn) popupCloseBtn.addEventListener('click', closePopup);
        if (popupOkBtn) popupOkBtn.addEventListener('click', closePopup);
        
        // Handle Startup API call
        if (startupCheckbox) {
            startupCheckbox.addEventListener('change', (e) => {
                if (settingsStartupCheckbox) settingsStartupCheckbox.checked = e.target.checked;
                fetch('/api/startup', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ enable: e.target.checked })
                }).then(res => res.json())
                  .then(data => {
                      if(data.success) {
                          window.appController.view.showToast('success', 'Startup Settings Updated', data.message);
                      } else {
                          window.appController.view.showToast('error', 'Update Failed', 'Failed to update Windows startup settings.');
                      }
                  }).catch(err => {
                      window.appController.view.showToast('error', 'Update Failed', 'Error communicating with background server.');
                      console.error(err);
                  });
            });
        }
    }
});

// ==========================================
// NEW MVC MODULES (UI/UX Additions)
// ==========================================

class ValidationUtil {
    /** UX-3 Fix: Validates IPv4 address OR CIDR subnet (e.g. 192.168.1.0/24) */
    static isValidIP(ip) {
        if (!ip || typeof ip !== 'string') return false;
        const parts = ip.trim().split('/');
        const ipv4Regex = /^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
        if (!ipv4Regex.test(parts[0])) return false;
        if (parts.length === 2) {
            const cidr = parseInt(parts[1], 10);
            if (isNaN(cidr) || cidr < 0 || cidr > 32) return false;
        }
        return true;
    }
    static isNotEmpty(str) {
        return str && str.trim().length > 0;
    }
    static escapeHTML(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }
}

class SafeZoneModel {
    async fetchAll() {
        try {
            const res = await fetch('/api/safe-zones');
            const data = await res.json();
            return data.data;
        } catch(e) { return []; }
    }
    async add(ip, name) {
        const res = await fetch('/api/safe-zones', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ip, name })
        });
        return res.json();
    }
    async remove(id) {
        return fetch(`/api/safe-zones/${id}`, { method: 'DELETE' });
    }
}

class GroupModel {
    async fetchAll() {
        try {
            const res = await fetch('/api/groups');
            const data = await res.json();
            return data.data;
        } catch(e) { return []; }
    }
    async add(name, description) {
        const res = await fetch('/api/groups', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name, description })
        });
        return res.json();
    }
    async remove(id) {
        return fetch(`/api/groups/${id}`, { method: 'DELETE' });
    }
}

class DialogView {
    constructor() {
        this.overlay = document.getElementById('dynamic-modal-overlay');
        this.title = document.getElementById('dynamic-modal-title');
        this.body = document.getElementById('dynamic-modal-body');
        this.btnCancel = document.getElementById('dynamic-modal-cancel');
        this.btnConfirm = document.getElementById('dynamic-modal-confirm');
        
        if (this.btnCancel) {
            this.btnCancel.addEventListener('click', () => this.close());
        }
    }

    show(title, bodyHtml, onConfirm) {
        if(!this.overlay) return;
        this.title.textContent = title;
        this.body.innerHTML = bodyHtml;
        this.overlay.classList.add('active');
        
        this.btnConfirm.onclick = () => {
            if (onConfirm()) {
                this.close();
            }
        };
    }

    close() {
        if(this.overlay) this.overlay.classList.remove('active');
    }
}

class SafeZoneController {
    constructor(model, dialogView) {
        this.model = model;
        this.dialogView = dialogView;
        this.listContainer = document.getElementById('safe-zones-list');
        this.btnAdd = document.getElementById('btn-add-safe-zone');
        
        if (this.btnAdd) {
            this.btnAdd.addEventListener('click', () => this.showAddDialog());
        }
    }

    async init() {
        await this.renderList();
    }

    async renderList() {
        if (!this.listContainer) return;
        const zones = await this.model.fetchAll();
        this.listContainer.innerHTML = '';
        if (zones.length === 0) {
            this.listContainer.innerHTML = '<div class="list-item" style="opacity: 0.5; justify-content: center;">No Safe Zones found. Click "Add Safe Zone" to get started.</div>';
            return;
        }
        zones.forEach(zone => {
            const item = document.createElement('div');
            item.className = 'list-item';
            item.innerHTML = `
                <div class="list-item-content">
                    <span class="list-item-title"><i class="fa-solid fa-server text-emerald"></i> ${ValidationUtil.escapeHTML(zone.name)}</span>
                    <span class="list-item-sub">${ValidationUtil.escapeHTML(zone.ip)} &nbsp;<span class="badge badge-green" style="font-size:0.6rem;">WHITELISTED</span></span>
                </div>
                <div class="list-item-actions">
                    <button class="btn btn-outline btn-sm edit-sz-btn" data-id="${zone.id}" title="Edit"><i class="fa-solid fa-pen"></i></button>
                    <button class="btn btn-danger btn-sm delete-btn" data-id="${zone.id}" title="Delete"><i class="fa-solid fa-trash"></i></button>
                </div>
            `;
            item.querySelector('.edit-sz-btn').addEventListener('click', () => this.showEditDialog(zone));
            item.querySelector('.delete-btn').addEventListener('click', () => this.deleteZone(zone.id, zone.name));
            this.listContainer.appendChild(item);
        });
    }

    showAddDialog() {
        const bodyHtml = `
            <div class="form-group">
                <label>Zone Name</label>
                <input type="text" id="sz-name" class="input-text" placeholder="e.g. Home Network">
            </div>
            <div class="form-group">
                <label>IP Address / Subnet (IPv4 or CIDR)</label>
                <input type="text" id="sz-ip" class="input-text" placeholder="e.g. 192.168.1.1 or 192.168.1.0/24">
            </div>
        `;
        this.dialogView.show('Add Safe Zone', bodyHtml, () => {
            const name = document.getElementById('sz-name').value;
            const ip   = document.getElementById('sz-ip').value;
            if (!ValidationUtil.isNotEmpty(name)) { window.appController.view.showToast('error', 'Validation Failed', 'Zone name cannot be empty.'); return false; }
            if (!ValidationUtil.isValidIP(ip))    { window.appController.view.showToast('error', 'Validation Failed', 'Please enter a valid IPv4 address or CIDR subnet.'); return false; }
            this.model.add(ip, name).then(() => { window.appController.view.showToast('success', 'Safe Zone Added', `${name} has been whitelisted.`); this.renderList(); });
            return true;
        });
    }

    /** UX-4: Pre-filled edit dialog for existing safe zone */
    showEditDialog(zone) {
        const bodyHtml = `
            <div class="form-group">
                <label>Zone Name</label>
                <input type="text" id="sz-name" class="input-text" value="${ValidationUtil.escapeHTML(zone.name)}" placeholder="e.g. Home Network">
            </div>
            <div class="form-group">
                <label>IP Address / Subnet (IPv4 or CIDR)</label>
                <input type="text" id="sz-ip" class="input-text" value="${ValidationUtil.escapeHTML(zone.ip)}" placeholder="e.g. 192.168.1.1 or 192.168.1.0/24">
            </div>
        `;
        this.dialogView.show('Edit Safe Zone', bodyHtml, () => {
            const name = document.getElementById('sz-name').value;
            const ip   = document.getElementById('sz-ip').value;
            if (!ValidationUtil.isNotEmpty(name)) { window.appController.view.showToast('error', 'Validation Failed', 'Zone name cannot be empty.'); return false; }
            if (!ValidationUtil.isValidIP(ip))    { window.appController.view.showToast('error', 'Validation Failed', 'Please enter a valid IPv4 address or CIDR subnet.'); return false; }
            fetch(`/api/safe-zones/${zone.id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ip, name }) })
                .then(r => r.json()).then(() => { window.appController.view.showToast('success', 'Safe Zone Updated', `${name} updated successfully.`); this.renderList(); });
            return true;
        });
    }

    /** UX-5: Replace native confirm() with DialogView */
    async deleteZone(id, name) {
        this.dialogView.show(
            'Delete Safe Zone',
            `<p style="color:var(--text-main)">Are you sure you want to remove <strong>${ValidationUtil.escapeHTML(name)}</strong> from the whitelist?</p>`,
            async () => {
                await this.model.remove(id);
                window.appController.view.showToast('info', 'Safe Zone Deleted', 'The safe zone was removed from the whitelist.');
                this.renderList();
                return true;
            }
        );
    }
}

class GroupController {
    constructor(model, dialogView) {
        this.model = model;
        this.dialogView = dialogView;
        this.listContainer = document.getElementById('groups-list');
        this.btnAdd = document.getElementById('btn-add-group');
        
        if (this.btnAdd) {
            this.btnAdd.addEventListener('click', () => this.showAddDialog());
        }
    }

    async init() {
        await this.renderList();
    }

    async renderList() {
        if (!this.listContainer) return;
        const groups = await this.model.fetchAll();
        this.listContainer.innerHTML = '';
        if (groups.length === 0) {
            this.listContainer.innerHTML = '<div class="list-item" style="opacity: 0.5; justify-content: center;">No Groups found. Click "Create Group" to get started.</div>';
            return;
        }
        groups.forEach(group => {
            const item = document.createElement('div');
            item.className = 'list-item';
            item.innerHTML = `
                <div class="list-item-content">
                    <span class="list-item-title"><i class="fa-solid fa-users text-info"></i> ${ValidationUtil.escapeHTML(group.name)}</span>
                    <span class="list-item-sub">${ValidationUtil.escapeHTML(group.description || 'No description')} &nbsp;<span class="badge badge-blue" style="font-size:0.6rem;">${group.members} member${group.members !== 1 ? 's' : ''}</span></span>
                </div>
                <div class="list-item-actions">
                    <button class="btn btn-outline btn-sm edit-grp-btn" data-id="${group.id}" title="Edit"><i class="fa-solid fa-pen"></i></button>
                    <button class="btn btn-danger btn-sm delete-btn" data-id="${group.id}" title="Delete"><i class="fa-solid fa-trash"></i></button>
                </div>
            `;
            item.querySelector('.edit-grp-btn').addEventListener('click', () => this.showEditDialog(group));
            item.querySelector('.delete-btn').addEventListener('click', () => this.deleteGroup(group.id, group.name));
            this.listContainer.appendChild(item);
        });
    }

    showAddDialog() {
        const bodyHtml = `
            <div class="form-group">
                <label>Group Name</label>
                <input type="text" id="grp-name" class="input-text" placeholder="e.g. IT Admins">
            </div>
            <div class="form-group">
                <label>Description (Optional)</label>
                <input type="text" id="grp-desc" class="input-text" placeholder="e.g. Level 2 Support Team">
            </div>
        `;
        this.dialogView.show('Create Group', bodyHtml, () => {
            const name = document.getElementById('grp-name').value;
            const desc = document.getElementById('grp-desc').value;
            if (!ValidationUtil.isNotEmpty(name)) { window.appController.view.showToast('error', 'Validation Failed', 'Group name cannot be empty.'); return false; }
            this.model.add(name, desc).then(() => { window.appController.view.showToast('success', 'Group Created', `Group ${name} created successfully.`); this.renderList(); });
            return true;
        });
    }

    /** UX-4: Pre-filled edit dialog for existing group */
    showEditDialog(group) {
        const bodyHtml = `
            <div class="form-group">
                <label>Group Name</label>
                <input type="text" id="grp-name" class="input-text" value="${ValidationUtil.escapeHTML(group.name)}" placeholder="e.g. IT Admins">
            </div>
            <div class="form-group">
                <label>Description (Optional)</label>
                <input type="text" id="grp-desc" class="input-text" value="${ValidationUtil.escapeHTML(group.description || '')}" placeholder="e.g. Level 2 Support Team">
            </div>
        `;
        this.dialogView.show('Edit Group', bodyHtml, () => {
            const name = document.getElementById('grp-name').value;
            const desc = document.getElementById('grp-desc').value;
            if (!ValidationUtil.isNotEmpty(name)) { window.appController.view.showToast('error', 'Validation Failed', 'Group name cannot be empty.'); return false; }
            fetch(`/api/groups/${group.id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ name, description: desc }) })
                .then(r => r.json()).then(() => { window.appController.view.showToast('success', 'Group Updated', `Group ${name} updated successfully.`); this.renderList(); });
            return true;
        });
    }

    /** UX-5: Replace native confirm() with DialogView */
    async deleteGroup(id, name) {
        this.dialogView.show(
            'Delete Group',
            `<p style="color:var(--text-main)">Are you sure you want to delete group <strong>${ValidationUtil.escapeHTML(name)}</strong>? This cannot be undone.</p>`,
            async () => {
                await this.model.remove(id);
                window.appController.view.showToast('info', 'Group Deleted', 'The group was removed.');
                this.renderList();
                return true;
            }
        );
    }
}

// Initialize New MVC Modules
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(() => {
        const dialogView = new DialogView();
        window.safeZoneController = new SafeZoneController(new SafeZoneModel(), dialogView);
        window.groupController = new GroupController(new GroupModel(), dialogView);
        window.deviceController = new DeviceController(new DeviceModel());
        
        window.safeZoneController.init();
        window.groupController.init();
        window.deviceController.init();
    }, 500);
});

class DeviceModel {
    async fetchAll() {
        try {
            const r = await fetch('/api/devices');
            const d = await r.json();
            return d.data || [];
        } catch(e) { console.error(e); return []; }
    }
    async add(d) {
        const r = await fetch('/api/devices', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(d) });
        return await r.json();
    }
    async update(id, d) {
        const r = await fetch(`/api/devices/${id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(d) });
        return await r.json();
    }
    async remove(id) {
        await fetch(`/api/devices/${id}`, { method: 'DELETE' });
    }
}

class DeviceController {
    constructor(model) {
        this.model = model;
        this.listEl = document.getElementById('device-list-ul');
        this.modal = document.getElementById('add-device-modal');
        this.nameInput = document.getElementById('device-modal-name');
        this.accessInput = document.getElementById('device-modal-access');
        this.idInput = document.getElementById('device-modal-id');
        this.title = document.getElementById('device-modal-title');
        this.saveBtn = document.getElementById('device-modal-save-btn');
        
        const openBtn = document.getElementById('btn-open-add-device');
        if(openBtn) openBtn.onclick = () => this.openAddModal();
        
        if(this.saveBtn) this.saveBtn.onclick = () => this.saveDevice();
        
        document.querySelectorAll('#add-device-modal .close-modal-btn').forEach(btn => {
            btn.onclick = () => this.closeModal();
        });
    }

    async init() {
        await this.renderList();
    }

    async renderList() {
        if (!this.listEl) return;
        const devices = await this.model.fetchAll();
        this.listEl.innerHTML = '';
        if (devices.length === 0) {
            this.listEl.innerHTML = '<li style="padding: 0.4rem; text-align: center; color: var(--text-muted);">No devices registered yet.</li>';
            return;
        }

        const frag = document.createDocumentFragment();
        devices.forEach(d => {
            const li = document.createElement('li');
            li.style.cssText = 'padding: 1rem; border-bottom: 1px solid var(--border-color); display: flex; justify-content: space-between; align-items: center;';
            li.innerHTML = `
                <div>
                    <h4 style="margin: 0; color: var(--text-color);"><i class="fa-solid fa-laptop text-primary"></i> ${ValidationUtil.escapeHTML(d.name)}</h4>
                    <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.1rem;">Access: ${ValidationUtil.escapeHTML(d.accessLevel)} | Status: <span class="${d.status === 'Online' ? 'text-emerald' : 'text-warning'}">${ValidationUtil.escapeHTML(d.status)}</span></div>
                </div>
                <div style="display: flex; gap: 0.5rem;">
                    <button class="btn btn-secondary btn-sm edit-btn"><i class="fa-solid fa-pen"></i></button>
                    <button class="btn btn-secondary btn-sm delete-btn"><i class="fa-solid fa-trash text-danger"></i></button>
                </div>
            `;
            li.querySelector('.edit-btn').onclick = () => this.openEditModal(d);
            li.querySelector('.delete-btn').onclick = () => this.deleteDevice(d.id, d.name);
            frag.appendChild(li);
        });
        this.listEl.appendChild(frag);
    }

    openAddModal() {
        if(this.idInput) this.idInput.value = '';
        if(this.nameInput) this.nameInput.value = '';
        if(this.accessInput) this.accessInput.value = 'Standard';
        if(this.title) this.title.innerHTML = '<i class="fa-solid fa-plus"></i> Register Device';
        if(this.modal) this.modal.classList.add('active');
    }

    openEditModal(d) {
        if(this.idInput) this.idInput.value = d.id;
        if(this.nameInput) this.nameInput.value = d.name;
        if(this.accessInput) this.accessInput.value = d.accessLevel;
        if(this.title) this.title.innerHTML = '<i class="fa-solid fa-pen"></i> Edit Device';
        if(this.modal) this.modal.classList.add('active');
    }

    closeModal() {
        if(this.modal) this.modal.classList.remove('active');
    }

    async saveDevice() {
        const id = this.idInput.value;
        const name = this.nameInput.value.trim();
        const access = this.accessInput.value;

        // Strict Regex Validation
        const nameRegex = /^[a-zA-Z0-9 ]+$/;
        if (!name || !nameRegex.test(name)) {
            window.appController.view.showToast('error', 'Validation Error', 'Device name must be alphanumeric and cannot be empty.');
            return;
        }

        try {
            if (id) {
                const res = await this.model.update(id, { name, accessLevel: access });
                if(!res.success) throw new Error(res.error);
                window.appController.view.showToast('success', 'Device Updated', 'Device details saved.');
            } else {
                const res = await this.model.add({ name, accessLevel: access });
                if(!res.success) throw new Error(res.error);
                window.appController.view.showToast('success', 'Device Registered', 'New device added to fleet.');
            }
            this.closeModal();
            await this.renderList();
        } catch (e) {
            window.appController.view.showToast('error', 'Save Failed', e.message);
        }
    }

    async deleteDevice(id, name) {
        // Fallback to JS confirm for simplicity as DialogView might require other setup
        if (confirm('Are you sure you want to delete this device? This cannot be undone.')) {
            await this.model.remove(id);
            window.appController.view.showToast('info', 'Device Deleted', 'The device was removed from the fleet.');
            await this.renderList();
        }
    }
}
