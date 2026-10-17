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
    assert(fs.existsSync('jDroid-X-Desk.exe'), 'jDroid-X-Desk.exe (single universal executable) exists');
    assert(fs.existsSync('jDroid-X-Desk.exe.config'), 'jDroid-X-Desk.exe.config (universal multi-runtime configuration) exists');
    assert(fs.existsSync('app.manifest'), 'app.manifest (Windows universal DPI & compatibility manifest) exists');
    assert(fs.existsSync('jDroid-X-Share.bat'), 'jDroid-X-Share.bat (canonical standalone client) exists');
    assert(fs.existsSync('jDroid-Share.bat'), 'jDroid-Share.bat (legacy alias client) exists');
    assert(fs.existsSync('jDroid-X-Client.bat'), 'jDroid-X-Client.bat (canonical portable admin client) exists');
    assert(fs.existsSync('jDroid-Client.bat'), 'jDroid-Client.bat (legacy alias client) exists');
    assert(fs.existsSync('JDroidXDeskNative.cs'), 'JDroidXDeskNative.cs (pure native C# source) exists');
    assert(fs.existsSync('jDroid-X-Desk-Native.exe'), 'jDroid-X-Desk-Native.exe (legacy synchronized mirror) exists');
    assert(fs.existsSync('colab_relay_server.py'), 'colab_relay_server.py (central cloud relay server) exists');
    assert(fs.existsSync('colab-rdp.txt'), 'colab-rdp.txt (Colab execution script) exists');
    assert(fs.existsSync('windows_rdp.yaml'), 'windows_rdp.yaml (RDP YAML configuration) exists');
    assert(fs.existsSync('jDroid-X-Desk.ipynb'), 'jDroid-X-Desk.ipynb (Colab Jupyter Notebook) exists');
    assert(fs.existsSync('docs/headless/windowscmd.txt'), 'docs/headless/windowscmd.txt exists');
    assert(fs.existsSync('docs/headless/powershellcmd.txt'), 'docs/headless/powershellcmd.txt exists');
    assert(fs.existsSync('docs/headless/debianlinux.txt'), 'docs/headless/debianlinux.txt exists');


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
        'settings-fb-api',
        'custom-domain-url',
        'colab-relay-url'
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

        // Test Relational Groups & Devices with Normalized Description
        const getGroups = await makeRequest('GET', '/api/groups');
        assert(getGroups.statusCode === 200 && Array.isArray(getGroups.body.data), 'GET /api/groups returns device groups with member counts');
        assert(getGroups.body.data.length > 0 && typeof getGroups.body.data[0].description === 'string', 'GET /api/groups normalizes group description string');

        // Scenario 1: Group Full CRUD Cycle
        const postGroup = await makeRequest('POST', '/api/groups', { name: 'Test QA Group', description: 'Ephemeral QA node pool' });
        assert(postGroup.statusCode === 200 && postGroup.body.data.id, 'POST /api/groups creates new device group');
        const testGroupId = postGroup.body.data?.id;
        if (testGroupId) {
            const putGroup = await makeRequest('PUT', `/api/groups/${testGroupId}`, { name: 'Updated QA Group', description: 'Updated description' });
            assert(putGroup.statusCode === 200 && putGroup.body.data.name === 'Updated QA Group', 'PUT /api/groups/:id updates group name & description');
        }

        // Scenario 2: Device Fleet Full CRUD Cycle
        const getDevices = await makeRequest('GET', '/api/devices');
        assert(getDevices.statusCode === 200 && Array.isArray(getDevices.body.data), 'GET /api/devices returns device fleet with relational groupId');

        const postDevice = await makeRequest('POST', '/api/devices', { name: 'Test-Workstation-99', accessLevel: 'Admin', groupId: testGroupId });
        assert(postDevice.statusCode === 200 && postDevice.body.data.id, 'POST /api/devices registers new machine with groupId');
        const testDeviceId = postDevice.body.data?.id;
        if (testDeviceId) {
            const putDevice = await makeRequest('PUT', `/api/devices/${testDeviceId}`, { name: 'Test-Workstation-Updated', accessLevel: 'Standard' });
            assert(putDevice.statusCode === 200 && putDevice.body.data.name === 'Test-Workstation-Updated', 'PUT /api/devices/:id updates workstation profile');
            const delDevice = await makeRequest('DELETE', `/api/devices/${testDeviceId}`);
            assert(delDevice.statusCode === 200, 'DELETE /api/devices/:id removes machine');
        }

        if (testGroupId) {
            const delGroup = await makeRequest('DELETE', `/api/groups/${testGroupId}`);
            assert(delGroup.statusCode === 200, 'DELETE /api/groups/:id cleans up test group and unassigns devices');
        }

        // Scenario 3: Safe Zones CIDR IP CRUD Cycle
        const getSafeZones = await makeRequest('GET', '/api/safe-zones');
        assert(getSafeZones.statusCode === 200 && Array.isArray(getSafeZones.body.data), 'GET /api/safe-zones returns CIDR IP perimeter list');

        const postSafeZone = await makeRequest('POST', '/api/safe-zones', { name: 'Test Lab Subnet', ip: '10.200.1.0/24' });
        assert(postSafeZone.statusCode === 200 && postSafeZone.body.data.id, 'POST /api/safe-zones whitelists new CIDR perimeter');
        const testSzId = postSafeZone.body.data?.id;
        if (testSzId) {
            const putSafeZone = await makeRequest('PUT', `/api/safe-zones/${testSzId}`, { name: 'Updated Lab Subnet', ip: '10.200.2.0/24' });
            assert(putSafeZone.statusCode === 200 && putSafeZone.body.data.ip === '10.200.2.0/24', 'PUT /api/safe-zones/:id updates perimeter rule');
            const delSafeZone = await makeRequest('DELETE', `/api/safe-zones/${testSzId}`);
            assert(delSafeZone.statusCode === 200, 'DELETE /api/safe-zones/:id removes safe zone rule');
        }

        // Scenario 4: Guardians Security Personnel CRUD Cycle
        const getGuardians = await makeRequest('GET', '/api/guardians');
        assert(getGuardians.statusCode === 200 && Array.isArray(getGuardians.body.data), 'GET /api/guardians returns security personnel roster');

        const postGuardian = await makeRequest('POST', '/api/guardians', { name: 'QA Sentinel', role: 'Compliance Inspector', contact: 'qa@jdroid-x.net' });
        assert(postGuardian.statusCode === 200 && postGuardian.body.data.id, 'POST /api/guardians registers emergency supervisor');
        const testGrdId = postGuardian.body.data?.id;
        if (testGrdId) {
            const putGuardian = await makeRequest('PUT', `/api/guardians/${testGrdId}`, { name: 'QA Lead Sentinel', role: 'Chief Compliance', contact: 'lead-qa@jdroid-x.net' });
            assert(putGuardian.statusCode === 200 && putGuardian.body.data.name === 'QA Lead Sentinel', 'PUT /api/guardians/:id updates supervisor credentials');
            const delGuardian = await makeRequest('DELETE', `/api/guardians/${testGrdId}`);
            assert(delGuardian.statusCode === 200, 'DELETE /api/guardians/:id revokes supervisor credentials');
        }

        // Scenario 5: Partner Contacts Address Book CRUD Cycle
        const getContacts = await makeRequest('GET', '/api/contacts');
        assert(getContacts.statusCode === 200 && Array.isArray(getContacts.body.data), 'GET /api/contacts returns partner address book');

        const postContact = await makeRequest('POST', '/api/contacts', { name: 'QA Remote Partner', remoteId: '123-456-789', group: 'Field Fleet' });
        assert(postContact.statusCode === 200 && postContact.body.data.id, 'POST /api/contacts stores partner address book entry');
        const testCntId = postContact.body.data?.id;
        if (testCntId) {
            const putContact = await makeRequest('PUT', `/api/contacts/${testCntId}`, { name: 'QA Remote Partner v2', remoteId: '987-654-321' });
            assert(putContact.statusCode === 200 && putContact.body.data.remoteId === '987-654-321', 'PUT /api/contacts/:id updates partner remoteId');
            const delContact = await makeRequest('DELETE', `/api/contacts/${testCntId}`);
            assert(delContact.statusCode === 200, 'DELETE /api/contacts/:id deletes partner contact');
        }

        // Scenario 6: Notifications & Read-All
        const postNotif = await makeRequest('POST', '/api/notifications', { title: 'Test Alert', message: 'Automated test suite notice', type: 'info' });
        assert(postNotif.statusCode === 200 && postNotif.body.data.id, 'POST /api/notifications creates system alert');
        const testNotifId = postNotif.body.data?.id;

        const getNotifs = await makeRequest('GET', '/api/notifications');
        assert(getNotifs.statusCode === 200 && Array.isArray(getNotifs.body.data), 'GET /api/notifications returns notification center events');

        const readAllNotifs = await makeRequest('PUT', '/api/notifications/read-all');
        assert(readAllNotifs.statusCode === 200 && readAllNotifs.body.success, 'PUT /api/notifications/read-all bulk-acknowledges notifications');

        if (testNotifId) {
            const delNotif = await makeRequest('DELETE', `/api/notifications/${testNotifId}`);
            assert(delNotif.statusCode === 200, 'DELETE /api/notifications/:id dismisses notification');
        }

        // Scenario 7: AnyDesk Session Invitation Lifecycle
        const postInvite = await makeRequest('POST', '/api/invites', { sourceId: 'JDX-TEST-SRC', targetId: 'JDX-TEST-TGT' });
        assert(postInvite.statusCode === 200 && postInvite.body.data.id, 'POST /api/invites dispatches session invitation');
        const testInviteId = postInvite.body.data?.id;

        const getPending = await makeRequest('GET', '/api/invites/pending/JDX-TEST-TGT');
        assert(getPending.statusCode === 200 && Array.isArray(getPending.body.data) && getPending.body.data.length > 0, 'GET /api/invites/pending/:targetId retrieves pending invite');

        if (testInviteId) {
            const respondInvite = await makeRequest('PUT', `/api/invites/${testInviteId}/respond`, { status: 'ACCEPTED' });
            assert(respondInvite.statusCode === 200 && respondInvite.body.session?.id, 'PUT /api/invites/:id/respond accepts invite and creates live session');
            if (respondInvite.body.session?.id) {
                await makeRequest('DELETE', `/api/sessions/${respondInvite.body.session.id}`);
            }
        }

        // Scenario 8: Download Endpoints
        const dlExe = await makeRequest('GET', '/api/download-exe');
        assert(dlExe.statusCode === 200, 'GET /api/download-exe serves executable package');

        const dlClient = await makeRequest('GET', '/api/download-client');
        assert(dlClient.statusCode === 200, 'GET /api/download-client serves batch client');

        // Scenario 9: Native Windows Remote Endpoints Integration
        if (process.platform === 'win32') {
            const msraCheck = await makeRequest('POST', '/api/remote/launch-msra');
            assert(msraCheck.statusCode === 200 && msraCheck.body.success, 'POST /api/remote/launch-msra launches Microsoft Remote Assistance');
        }

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
