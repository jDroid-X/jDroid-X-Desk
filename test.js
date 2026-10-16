/**
 * jDroid-X-Desk Automated Verification & Smoke Test Suite
 * Stage 5 Verification Loop (Syntax -> Schema -> DOM -> API Integration)
 */

const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');
const http = require('http');

let failedTests = 0;
let passedTests = 0;

function assert(condition, testName) {
    if (condition) {
        console.log(`  [PASS] ${testName}`);
        passedTests++;
    } else {
        console.error(`  [FAIL] ${testName}`);
        failedTests++;
    }
}

async function runTests() {
    console.log('====================================================');
    console.log(' jDroid-X-Desk Stage 5 Automated Verification Suite');
    console.log('====================================================\n');

    // 1. FILE EXISTENCE & SYNTAX
    console.log('[1/4] Verifying Code Integrity & Static Files...');
    assert(fs.existsSync('server.js'), 'server.js exists');
    assert(fs.existsSync('app.js'), 'app.js exists');
    assert(fs.existsSync('index.html'), 'index.html exists');
    assert(fs.existsSync('database.json'), 'database.json exists');
    assert(fs.existsSync('jDroid-Share.bat'), 'jDroid-Share.bat (standalone client) exists');
    assert(fs.existsSync('jDroid-Client.bat'), 'jDroid-Client.bat (portable admin client) exists');
    assert(fs.existsSync('JDroidXDeskNative.cs'), 'JDroidXDeskNative.cs (pure native C# source) exists');
    assert(fs.existsSync('jDroid-X-Desk-Native.exe'), 'jDroid-X-Desk-Native.exe (pure native Windows machine executable) exists');


    // 2. DATABASE SCHEMA INTEGRITY
    console.log('\n[2/4] Verifying Database Schema Tables...');
    try {
        const db = JSON.parse(fs.readFileSync('database.json', 'utf8'));
        const requiredTables = ['safeZones', 'groups', 'sessions', 'devices', 'guardians', 'contacts', 'notifications', 'settings', 'logs'];
        for (const t of requiredTables) {
            assert(db[t] !== undefined, `Database schema contains table: "${t}"`);
        }
    } catch (e) {
        assert(false, `Database JSON parsing failed: ${e.message}`);
    }

    // 3. DOM AFFORDANCE & ELEMENT MAPPING
    console.log('\n[3/4] Verifying DOM Affordances & Element IDs...');
    const html = fs.readFileSync('index.html', 'utf8');
    const criticalDomIds = [
        'local-device-id',
        'local-node-desc',
        'local-device-pass',
        'target-id-input',
        'target-pass-input',
        'connect-session-btn',
        'btn-native-quick-assist',
        'btn-native-msra',
        'btn-download-share-bat',
        'session-status-text',
        'remote-desktop-viewport-box',
        'settings-fb-status-banner',
        'settings-fb-app',
        'settings-fb-api'
    ];
    for (const id of criticalDomIds) {
        assert(html.includes(`id="${id}"`), `DOM element exists: #${id}`);
    }

    // 4. BACKEND API INTEGRATION TEST (EPHEMERAL PORT)
    console.log('\n[4/4] Starting Server & Testing API Endpoints...');
    const TEST_PORT = 12399;
    const serverProc = spawn('node', ['server.js'], {
        env: { ...process.env, PORT: String(TEST_PORT) },
        stdio: 'pipe'
    });

    // Wait for server to boot reliably
    await new Promise(res => {
        let booted = false;
        serverProc.stdout.on('data', d => {
            if (d.toString().includes('running on http')) {
                if (!booted) { booted = true; res(); }
            }
        });
        setTimeout(() => { if (!booted) res(); }, 4000);
    });

    function makeRequest(method, pathName, body = null) {
        return new Promise((resolve, reject) => {
            const req = http.request({
                hostname: '127.0.0.1',
                port: TEST_PORT,
                path: pathName,
                method: method,
                headers: {
                    'Content-Type': 'application/json'
                }
            }, res => {
                let data = '';
                res.on('data', chunk => data += chunk);
                res.on('end', () => {
                    try {
                        resolve({ statusCode: res.statusCode, body: JSON.parse(data) });
                    } catch (_) {
                        resolve({ statusCode: res.statusCode, body: data });
                    }
                });
            });
            req.on('error', reject);
            if (body) req.write(JSON.stringify(body));
            req.end();
        });
    }

    try {
        // Test Settings
        const settingsRes = await makeRequest('GET', '/api/settings');
        assert(settingsRes.statusCode === 200 && settingsRes.body.success, 'GET /api/settings returns HTTP 200');

        // Test Hardware ID
        const hwRes = await makeRequest('GET', '/api/hardware-id');
        assert(hwRes.statusCode === 200 && hwRes.body.deviceId, 'GET /api/hardware-id returns unique device ID');

        // Test System Telemetry
        const telemRes = await makeRequest('GET', '/api/system-telemetry');
        assert(telemRes.statusCode === 200 && telemRes.body.data.platform, 'GET /api/system-telemetry returns real OS metrics');

        // Test Sessions CRUD
        const postSession = await makeRequest('POST', '/api/sessions', { targetId: 'JDX-TEST-9999' });
        assert(postSession.statusCode === 200 && postSession.body.data.id, 'POST /api/sessions creates active session in DB');

        const sessionId = postSession.body.data?.id;
        if (sessionId) {
            const putSession = await makeRequest('PUT', `/api/sessions/${sessionId}`, { status: 'TERMINATED' });
            assert(putSession.statusCode === 200 && putSession.body.data.status === 'TERMINATED', 'PUT /api/sessions/:id terminates session with duration');

            const delSession = await makeRequest('DELETE', `/api/sessions/${sessionId}`);
            assert(delSession.statusCode === 200, 'DELETE /api/sessions/:id cleans up test session');
        }

        // Test Logs
        const postLog = await makeRequest('POST', '/api/logs', { level: 'INFO', stage: 'TEST', msg: 'Automated test suite probe' });
        assert(postLog.statusCode === 200 && postLog.body.success, 'POST /api/logs records event into database');

        const getLogs = await makeRequest('GET', '/api/logs');
        assert(getLogs.statusCode === 200 && Array.isArray(getLogs.body.data), 'GET /api/logs returns audit log array');

    } catch (err) {
        assert(false, `API integration error: ${err.message}`);
    } finally {
        serverProc.kill();
    }

    console.log('\n====================================================');
    console.log(` VERIFICATION RESULTS: ${passedTests} PASSED, ${failedTests} FAILED`);
    console.log('====================================================\n');

    process.exit(failedTests > 0 ? 1 : 0);
}

runTests();
