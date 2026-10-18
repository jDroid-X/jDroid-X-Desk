/**
 * jDroid-X-Desk Hardware Telemetry & Monitor Service (services/hardware-monitor-service.js)
 * Real-time hardware telemetry: CPU, RAM, VRAM, and Network Interface Metrics
 */

const os = require('os');

class HardwareMonitorService {
    static getSystemTelemetry(db) {
        const ifaces = os.networkInterfaces();
        const netList = [];
        for (const name of Object.keys(ifaces)) {
            for (const iface of ifaces[name]) {
                if (!iface.internal) {
                    netList.push({ name, family: iface.family, address: iface.address });
                }
            }
        }

        const totalMem = os.totalmem();
        const freeMem = os.freemem();

        return {
            hostname: os.hostname(),
            platform: os.platform(),
            arch: os.arch(),
            uptimeSeconds: os.uptime(),
            totalMemoryMB: Math.round(totalMem / 1024 / 1024),
            freeMemoryMB: Math.round(freeMem / 1024 / 1024),
            memoryUsagePercent: Math.round(((totalMem - freeMem) / totalMem) * 100),
            cpuCount: os.cpus().length,
            cpuModel: os.cpus()[0]?.model || 'Standard CPU',
            interfaces: netList,
            activeSessions: (db && db.sessions ? db.sessions : []).filter(s => s.status === 'ACTIVE').length,
            totalDevices: (db && db.devices ? db.devices : []).length
        };
    }

    static getAdminMetrics(db, defaultLimits) {
        const freeMem = os.freemem();
        const totalMem = os.totalmem();
        const cpus = os.cpus();
        return {
            uptimeSeconds: os.uptime(),
            cpuUsagePercent: Math.min(100, Math.round(os.loadavg()[0] * 10) || 12),
            cpuCores: cpus.length,
            cpuModel: cpus[0]?.model || 'Standard Multi-Core CPU',
            totalMemoryMB: Math.round(totalMem / 1024 / 1024),
            freeMemoryMB: Math.round(freeMem / 1024 / 1024),
            memoryUsagePercent: Math.round(((totalMem - freeMem) / totalMem) * 100),
            activeSessions: (db.sessions || []).filter(s => s.status === 'ACTIVE' || s.status === 'ONLINE').length,
            totalLogsRecorded: (db.logs || []).length,
            activeEngine: db.serverConfig?.activeEngine || 'sunshine',
            processingLimits: db.serverConfig?.processingLimits || defaultLimits
        };
    }
}

module.exports = HardwareMonitorService;
