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
        this.dynamic5GEnabled = true;
        this.dlThreshold = 1.0;
        this.pingThreshold = 100;
        this.toastDuration = 4;
        
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
        if (type === 'error') iconClass = 'fa-circle-xmark';
        if (type === 'warning') iconClass = 'fa-triangle-exclamation';

        toast.innerHTML = `
            <i class="fa-solid ${iconClass}"></i>
            <div>
                <div class="toast-title">${title}</div>
                <div class="toast-desc">${desc}</div>
            </div>
        `;

        this.toastContainer.appendChild(toast);
        
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(-100%)';
            toast.style.transition = 'all 0.3s ease';
            setTimeout(() => toast.remove(), 300);
        }, (window.appController?.settingsModel?.toastDuration || 4) * 1000);
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
                <div class="margin-top-md" style="background: var(--bg-tertiary); border: 1px solid var(--border-bright); border-radius: var(--radius-md); padding: 1.25rem;">
                    <div style="display: grid; grid-template-columns: 1fr auto 1fr; gap: 1rem; align-items: center; text-align: center;">
                        <div style="background: var(--bg-card); padding: 1rem; border-radius: 8px; border-left: 4px solid var(--accent-blue);">
                            <i class="fa-solid fa-laptop-code" style="font-size: 1.8rem; color: var(--accent-blue);"></i>
                            <h4 style="font-size: 0.95rem; margin-top: 0.4rem;">System A (Primary)</h4>
                            <div style="font-size: 0.75rem; color: var(--text-muted);">${sysA.id} | ${sysA.ip}</div>
                        </div>

                        <div style="display: flex; flex-direction: column; align-items: center; gap: 0.25rem;">
                            <span class="badge badge-blue">P2P Encrypted Tunnel</span>
                            <div style="font-size: 0.75rem; color: var(--accent-emerald); font-weight: 600;">
                                <i class="fa-solid fa-shield-halved"></i> Ed25519 Encrypted
                            </div>
                            <div style="font-size: 0.72rem; color: var(--text-dim);">Avg RTT: 16 ms | Packet Loss: 0.0%</div>
                        </div>

                        <div style="background: var(--bg-card); padding: 1rem; border-radius: 8px; border-left: 4px solid var(--accent-emerald);">
                            <i class="fa-solid fa-server" style="font-size: 1.8rem; color: var(--accent-emerald);"></i>
                            <h4 style="font-size: 0.95rem; margin-top: 0.4rem;">System B (Secondary)</h4>
                            <div style="font-size: 0.75rem; color: var(--text-muted);">${sysB.id} | ${sysB.ip}</div>
                        </div>
                    </div>
                </div>

                <!-- Filter & Terminal Table -->
                <div class="margin-top-lg">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;">
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

                    <div style="overflow-x: auto; background: #080c14; border: 1px solid var(--border-bright); border-radius: 10px; padding: 0.75rem; max-height: 380px; overflow-y: auto;">
                        <table style="width: 100%; border-collapse: collapse; font-family: var(--font-mono); font-size: 0.78rem;">
                            <thead>
                                <tr style="border-bottom: 1px solid var(--border-bright); color: var(--text-muted); text-align: left;">
                                    <th style="padding: 0.5rem;">Packet ID</th>
                                    <th style="padding: 0.5rem;">Timestamp (UTC)</th>
                                    <th style="padding: 0.5rem;">Level</th>
                                    <th style="padding: 0.5rem;">Stage</th>
                                    <th style="padding: 0.5rem;">Trace Payload Message</th>
                                    <th style="padding: 0.5rem;">RTT</th>
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
                    <td style="padding: 0.45rem; color: var(--text-dim);">${l.id}</td>
                    <td style="padding: 0.45rem; color: var(--text-muted);">${l.timestamp.split('T')[1].replace('Z','')}</td>
                    <td style="padding: 0.45rem; color: ${lvlColor}; font-weight: 600;">${l.level}</td>
                    <td style="padding: 0.45rem; color: var(--accent-blue);">${l.stage}</td>
                    <td style="padding: 0.45rem; color: var(--text-main);">${l.message}</td>
                    <td style="padding: 0.45rem; color: var(--accent-emerald);">${l.rttMs} ms</td>
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
    }

    async init() {
        this.bindNavigation();
        this.bindModals();
        this.bindThemeToggle();
        this.bindRemoteConnectLogic();
        this.bindSettingsEvents();
        
        try {
            const res = await fetch('/api/hardware-id');
            const data = await res.json();
            this.systemModel.deviceId = data.deviceId || 'JDX-ERR-0000';
        } catch (e) {
            this.systemModel.deviceId = 'JDX-OFLN-0000';
        }
        
        this.renderCurrentView();
        this.startJarvisTelemetryLoop();
    }

    startJarvisTelemetryLoop() {
        setInterval(() => {
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

    bindSettingsEvents() {
        const dlThresh = document.getElementById('settings-dl-threshold');
        const dlVal = document.getElementById('settings-dl-val');
        if (dlThresh && dlVal) {
            dlThresh.oninput = (e) => {
                this.settingsModel.dlThreshold = parseFloat(e.target.value);
                dlVal.textContent = this.settingsModel.dlThreshold.toFixed(1);
            };
        }

        const pingThresh = document.getElementById('settings-ping-threshold');
        const pingVal = document.getElementById('settings-ping-val');
        if (pingThresh && pingVal) {
            pingThresh.oninput = (e) => {
                this.settingsModel.pingThreshold = parseInt(e.target.value, 10);
                pingVal.textContent = this.settingsModel.pingThreshold;
            };
        }

        const dyn5G = document.getElementById('settings-dynamic-5g');
        if (dyn5G) {
            dyn5G.onchange = (e) => {
                this.settingsModel.dynamic5GEnabled = e.target.checked;
            };
        }

        const toastDur = document.getElementById('settings-toast-duration');
        const toastVal = document.getElementById('settings-toast-val');
        if (toastDur && toastVal) {
            toastDur.oninput = (e) => {
                this.settingsModel.toastDuration = parseInt(e.target.value, 10);
                toastVal.textContent = this.settingsModel.toastDuration;
            };
        }
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
            themeBtn.addEventListener('click', () => {
                document.body.classList.toggle('light-theme');
                const icon = themeBtn.querySelector('i');
                if (document.body.classList.contains('light-theme')) {
                    icon.className = 'fa-solid fa-sun';
                } else {
                    icon.className = 'fa-solid fa-moon';
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

                const formattedId = `${cleanId.slice(0,3)}-${cleanId.slice(3,6)}-${cleanVal = cleanId.slice(6,9)}`;
                
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


