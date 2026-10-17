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
        if self.path in ['/', '/status']:
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            status = {
                'status': 'ONLINE',
                'service': 'jDroid-X Colab Cloud Relay Server',
                'active_devices': len(DEVICES),
                'devices': list(DEVICES.values()),
                'frames_buffered': len(FRAME_BUFFERS),
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

        # Poll incoming screen frames for a target device
        if self.path.startswith('/api/relay/pull/') or self.path.startswith('/api/relay/pull_frame/'):
            target_id = self.path.split('/')[-1]
            frame_data = FRAME_BUFFERS.get(target_id, None)
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

        # Push screen frame to buffer
        if self.path in ['/api/relay/push', '/api/relay/push_frame']:
            target_id = payload.get('targetId')
            frame = payload.get('frame')
            if target_id and frame:
                FRAME_BUFFERS[target_id] = frame
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
