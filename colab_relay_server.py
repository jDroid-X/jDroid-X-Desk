# ==============================================================================
# jDroid-X-Desk | Google Colab Centralized Cloud Relay Server v2.0
# Bridges Desk 1, Desk 2, Desk 3 across Firewalls, NAT, and Mobile Hotspots
# Reference: https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai#scrollTo=zWve46xh2msm
# ==============================================================================

import os
import sys
import json
import time
from http.server import HTTPServer, BaseHTTPRequestHandler

print("""
============================================================
   jDroid-X-Desk Centralized Colab Cloud Relay Server v2.0
   Zero-Config Remote Access Bridge (Desk 1 <--> Desk 2)
============================================================
""")

# In-memory routing structures
DEVICES = {}
FRAME_BUFFERS = {}
INPUT_BUFFERS = {}
ACTIVE_SESSIONS = {}
PUBLIC_TUNNEL_URL = ""

# Auto-load tunnel URL if saved in colab_tunnel.txt
try:
    if os.path.exists("colab_tunnel.txt"):
        with open("colab_tunnel.txt", "r", encoding="utf-8") as f:
            t = f.read().strip()
            if t and ".trycloudflare.com" in t:
                PUBLIC_TUNNEL_URL = t
except Exception:
    pass

def get_system_telemetry():
    ram_str = "--- / ---"
    disk_str = "-- / --"
    gpu_str = "N/A (CPU Mode)"
    # Windows RAM detection via ctypes
    try:
        import ctypes
        class MEMORYSTATUSEX(ctypes.Structure):
            _fields_ = [
                ("dwLength", ctypes.c_ulong),
                ("dwMemoryLoad", ctypes.c_ulong),
                ("ullTotalPhys", ctypes.c_ulonglong),
                ("ullAvailPhys", ctypes.c_ulonglong),
                ("ullTotalPageFile", ctypes.c_ulonglong),
                ("ullAvailPageFile", ctypes.c_ulonglong),
                ("ullTotalVirtual", ctypes.c_ulonglong),
                ("ullAvailVirtual", ctypes.c_ulonglong),
                ("sullAvailExtendedVirtual", ctypes.c_ulonglong),
            ]
        stat = MEMORYSTATUSEX()
        stat.dwLength = ctypes.sizeof(MEMORYSTATUSEX)
        if ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(stat)):
            used = stat.ullTotalPhys - stat.ullAvailPhys
            ram_str = f"{used / (1024**3):.1f} / {stat.ullTotalPhys / (1024**3):.1f} GB"
    except Exception:
        pass

    # Linux RAM detection via /proc/meminfo
    try:
        if os.path.exists('/proc/meminfo'):
            with open('/proc/meminfo', 'r') as f:
                lines = f.readlines()
                mem_total = int([l.split()[1] for l in lines if 'MemTotal' in l][0])
                mem_avail = int([l.split()[1] for l in lines if 'MemAvailable' in l][0])
                ram_str = f"{(mem_total - mem_avail)/(1024*1024):.1f} / {mem_total/(1024*1024):.1f} GB"
    except Exception:
        pass

    # Disk usage (Windows drive or Linux root)
    try:
        import shutil
        target_dir = os.path.splitdrive(os.getcwd())[0] + os.path.sep if os.name == 'nt' else '/'
        total, used, free = shutil.disk_usage(target_dir)
        disk_str = f"{used/(1024**3):.1f} / {total/(1024**3):.1f} GB"
    except Exception:
        pass

    # GPU detection via nvidia-smi
    try:
        import subprocess
        out = subprocess.check_output(["nvidia-smi", "--query-gpu=memory.used,memory.total", "--format=csv,nounits,noheader"], stderr=subprocess.DEVNULL).decode().strip()
        parts = [p.strip() for p in out.split(',')]
        if len(parts) >= 2:
            gpu_str = f"{float(parts[0])/1024:.1f} / {float(parts[1])/1024:.1f} GB"
    except Exception:
        pass
    return {'system_ram': ram_str, 'gpu_ram': gpu_str, 'disk': disk_str}

class ColabRelayHandler(BaseHTTPRequestHandler):
    def _set_cors(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type, Authorization')

    def do_OPTIONS(self):
        self.send_response(200)
        self._set_cors()
        self.end_headers()

    def do_GET(self):
        # Wake-on-Ping & Heartbeat Probe (GET)
        if self.path in ['/ping', '/api/relay/ping', '/api/relay/wake', '/wake']:
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            telem = get_system_telemetry()
            self.wfile.write(json.dumps({
                'success': True,
                'status': 'AWAKE',
                'service': 'jDroid-X Colab Cloud Relay Server',
                'organization': 'jDroid-X Studio',
                'public_url': PUBLIC_TUNNEL_URL,
                'relay_server': PUBLIC_TUNNEL_URL if PUBLIC_TUNNEL_URL else 'http://127.0.0.1:8088',
                'tunnel_url': PUBLIC_TUNNEL_URL,
                'active_devices': len(DEVICES),
                'system_ram': telem['system_ram'],
                'gpu_ram': telem['gpu_ram'],
                'disk': telem['disk'],
                'timestamp': time.time(),
                'message': 'Colab Relay awakened and active.'
            }).encode('utf-8'))
            return

        if self.path in ['/', '/status', '/api/settings', '/api/relay/settings']:
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            telem = get_system_telemetry()
            status = {
                'status': 'ONLINE',
                'service': 'jDroid-X Colab Cloud Relay Server',
                'public_url': PUBLIC_TUNNEL_URL,
                'relay_server': PUBLIC_TUNNEL_URL if PUBLIC_TUNNEL_URL else 'http://127.0.0.1:8088',
                'tunnel_url': PUBLIC_TUNNEL_URL,
                'custom_domain': PUBLIC_TUNNEL_URL if PUBLIC_TUNNEL_URL else 'https://www.jdroid-x.com',
                'web_repo': 'https://github.com/jDroid-X/jDroid-X-Web',
                'active_devices': len(DEVICES),
                'devices': list(DEVICES.values()),
                'frames_buffered': len(FRAME_BUFFERS),
                'system_ram': telem['system_ram'],
                'gpu_ram': telem['gpu_ram'],
                'disk': telem['disk'],
                'colab_host': 'Google Colab High-RAM Instance',
                'timestamp': time.time()
            }
            self.wfile.write(json.dumps(status).encode('utf-8'))
            return

        # List all registered desktop nodes
        if self.path == '/api/relay/devices':
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({'success': True, 'devices': list(DEVICES.values())}).encode('utf-8'))
            return

        # List all active paired remote desktop sessions
        if self.path in ['/api/relay/sessions', '/api/relay/active_sessions']:
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({'success': True, 'sessions': list(ACTIVE_SESSIONS.values()), 'count': len(ACTIVE_SESSIONS)}).encode('utf-8'))
            return

        # Poll incoming screen frames for a target device (with memory leak protection & TTL)
        if self.path.startswith('/api/relay/pull/') or self.path.startswith('/api/relay/pull_frame/'):
            target_id = self.path.split('/')[-1]
            entry = FRAME_BUFFERS.get(target_id, None)
            frame_data = None
            if entry:
                if isinstance(entry, dict):
                    if time.time() - entry.get('ts', 0) <= 15:
                        frame_data = entry.get('frame')
                    else:
                        FRAME_BUFFERS.pop(target_id, None)
                else:
                    frame_data = entry

            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            if frame_data:
                self.wfile.write(json.dumps({'success': True, 'frame': frame_data}).encode('utf-8'))
            else:
                self.wfile.write(json.dumps({'success': False, 'empty': True}).encode('utf-8'))
            return

        # Poll queued input events for a host machine
        if self.path.startswith('/api/relay/pull_input/'):
            target_id = self.path.split('/')[-1]
            inputs = INPUT_BUFFERS.pop(target_id, [])
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({'success': True, 'inputs': inputs}).encode('utf-8'))
            return

        self.send_response(404)
        self.end_headers()

    def do_POST(self):
        content_length = int(self.headers.get('Content-Length', 0))
        body = self.rfile.read(content_length).decode('utf-8')
        try:
            payload = json.loads(body) if body else {}
        except Exception:
            payload = {}

        # Wake-on-Ping & Heartbeat Probe (POST)
        if self.path in ['/ping', '/api/relay/ping', '/api/relay/wake', '/wake']:
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({
                'success': True,
                'status': 'AWAKE',
                'service': 'jDroid-X Colab Cloud Relay Server',
                'timestamp': time.time(),
                'message': 'Colab Relay awakened by POST ping.'
            }).encode('utf-8'))
            return

        # Cloudflare Tunnel URL Registration & Update
        if self.path in ['/api/relay/set_tunnel', '/api/relay/tunnel', '/api/relay/update_url']:
            global PUBLIC_TUNNEL_URL
            t_url = payload.get('tunnel_url') or payload.get('url') or payload.get('public_url') or ''
            if t_url:
                PUBLIC_TUNNEL_URL = t_url.strip()
                try:
                    with open('colab_tunnel.txt', 'w', encoding='utf-8') as f:
                        f.write(PUBLIC_TUNNEL_URL)
                except Exception:
                    pass
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({'success': True, 'public_url': PUBLIC_TUNNEL_URL}).encode('utf-8'))
            return

        # Register machine ID (Desk 1, Desk 2, etc.)
        if self.path in ['/api/relay/register', '/api/relay/handshake']:
            channel_id = payload.get('channelId') or payload.get('id')
            role = payload.get('role', 'desktop')
            name = payload.get('name', 'Desk Workstation')
            passcode = payload.get('passcode', '')
            if channel_id:
                DEVICES[channel_id] = {
                    'id': channel_id,
                    'name': name,
                    'role': role,
                    'passcode': passcode,
                    'last_seen': time.time()
                }
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({'success': True, 'registered': channel_id, 'activeDevices': len(DEVICES)}).encode('utf-8'))
            return

        # Connect / Bridge Request (Viewer Desk 2 -> Host Desk 1)
        if self.path in ['/api/relay/connect', '/api/relay/pair']:
            source_id = payload.get('sourceId') or payload.get('from')
            target_id = payload.get('targetId') or payload.get('to')
            passcode = payload.get('passcode', '')

            if not target_id:
                self.send_response(400)
                self._set_cors()
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({'success': False, 'error': 'Missing targetId'}).encode('utf-8'))
                return

            target_dev = DEVICES.get(target_id)
            if not target_dev:
                session_id = f"SES-{int(time.time())}-{source_id}-{target_id}"
                ACTIVE_SESSIONS[session_id] = {'sessionId': session_id, 'sourceId': source_id, 'targetId': target_id, 'status': 'WAITING_HOST', 'createdAt': time.time()}
                self.send_response(200)
                self._set_cors()
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({'success': True, 'status': 'WAITING_HOST', 'sessionId': session_id, 'targetId': target_id}).encode('utf-8'))
                return

            target_pass = target_dev.get('passcode', '')
            if target_pass and passcode and target_pass != passcode:
                self.send_response(401)
                self._set_cors()
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({'success': False, 'error': 'INVALID_PASSCODE'}).encode('utf-8'))
                return

            session_id = f"SES-{int(time.time())}-{source_id}-{target_id}"
            ACTIVE_SESSIONS[session_id] = {'sessionId': session_id, 'sourceId': source_id, 'targetId': target_id, 'status': 'ACTIVE', 'createdAt': time.time()}
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({
                'success': True,
                'status': 'CONNECTED',
                'sessionId': session_id,
                'sourceId': source_id,
                'targetId': target_id,
                'targetHost': target_dev,
                'pull_frame_url': f'/api/relay/pull_frame/{target_id}',
                'push_input_url': '/api/relay/push_input'
            }).encode('utf-8'))
            return

        # Disconnect active paired session
        if self.path in ['/api/relay/disconnect', '/api/relay/close']:
            session_id = payload.get('sessionId')
            if session_id and session_id in ACTIVE_SESSIONS:
                ACTIVE_SESSIONS[session_id]['status'] = 'CLOSED'
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({'success': True, 'disconnected': session_id}).encode('utf-8'))
            return

        # Push screen frame to buffer (with TTL and automated memory hygiene)
        if self.path in ['/api/relay/push', '/api/relay/push_frame']:
            target_id = payload.get('targetId')
            frame = payload.get('frame')
            if target_id and frame:
                FRAME_BUFFERS[target_id] = {'frame': frame, 'ts': time.time()}
                now = time.time()
                stale = [k for k, v in list(FRAME_BUFFERS.items()) if isinstance(v, dict) and (now - v.get('ts', 0) > 60)]
                for k in stale:
                    FRAME_BUFFERS.pop(k, None)
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({'success': True}).encode('utf-8'))
            return

        # Push mouse / keyboard input event from client to host
        if self.path == '/api/relay/push_input':
            target_id = payload.get('targetId')
            ev = payload.get('event')
            if target_id and ev:
                if target_id not in INPUT_BUFFERS:
                    INPUT_BUFFERS[target_id] = []
                INPUT_BUFFERS[target_id].append(ev)
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({'success': True}).encode('utf-8'))
            return

        self.send_response(404)
        self.end_headers()

def run_relay(port=8088):
    server = HTTPServer(('0.0.0.0', port), ColabRelayHandler)
    print(f"[READY] jDroid-X Colab Cloud Relay listening on port {port}")
    print("[INFO] Expose public URL in Colab via:")
    print(f"       !./cloudflared-linux-amd64 tunnel --url http://127.0.0.1:{port}")
    server.serve_forever()

if __name__ == '__main__':
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 8088
    run_relay(port)
