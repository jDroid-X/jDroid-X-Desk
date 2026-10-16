# ==============================================================================
# jDroid-X-Desk | Google Colab Centralized Relay Unit
# Notebook / Cloud Relay Server for NAT Traversal & Video Frame Exchanging
# Run this script directly on Google Colab:
# https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai#scrollTo=zWve46xh2msm
# ==============================================================================

import os
import sys
import json
import time
import asyncio
from http.server import HTTPServer, BaseHTTPRequestHandler

print("""
============================================================
   jDroid-X-Desk Centralized Colab Cloud Relay Unit v2.0
   Exchanging High-Speed Frames between Desk 1 & Desk 2
============================================================
""")

# Store connected sessions and active frame buffers in Colab RAM
SESSIONS = {}
FRAME_BUFFERS = {}

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
        if self.path == '/' or self.path == '/status':
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            status = {
                'status': 'ONLINE',
                'service': 'jDroid-X Colab Cloud Relay',
                'active_channels': len(SESSIONS),
                'colab_host': 'Google Colab High-RAM Instance',
                'timestamp': time.time()
            }
            self.wfile.write(json.dumps(status).encode('utf-8'))
            return

        # Poll incoming frames for a target device
        if self.path.startswith('/api/relay/pull/'):
            target_id = self.path.split('/')[-1]
            frame_data = FRAME_BUFFERS.get(target_id, None)
            self.send_response(200)
            self._set_cors()
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            if frame_data:
                self.wfile.write(json.dumps({'success': True, 'frame': frame_data}).encode('utf-8'))
                # Clear buffer after consumed
                del FRAME_BUFFERS[target_id]
            else:
                self.wfile.write(json.dumps({'success': False, 'empty': True}).encode('utf-8'))
            return

        self.send_response(404)
        self.end_headers()

    def do_POST(self):
        # Push screen frame / control packet to Colab buffer
        if self.path == '/api/relay/push':
            content_length = int(self.headers.get('Content-Length', 0))
            body = self.rfile.read(content_length).decode('utf-8')
            try:
                payload = json.loads(body)
                target_id = payload.get('targetId')
                frame = payload.get('frame')
                if target_id and frame:
                    FRAME_BUFFERS[target_id] = frame
                self.send_response(200)
                self._set_cors()
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({'success': True}).encode('utf-8'))
            except Exception as e:
                self.send_response(400)
                self._set_cors()
                self.end_headers()
                self.wfile.write(json.dumps({'success': False, 'error': str(e)}).encode('utf-8'))
            return

        # Register channel handshake
        if self.path == '/api/relay/handshake':
            content_length = int(self.headers.get('Content-Length', 0))
            body = self.rfile.read(content_length).decode('utf-8')
            try:
                payload = json.loads(body)
                channel_id = payload.get('channelId')
                role = payload.get('role', 'peer')
                if channel_id:
                    SESSIONS[channel_id] = {'registered_at': time.time(), 'role': role}
                self.send_response(200)
                self._set_cors()
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({'success': True, 'activeChannels': len(SESSIONS)}).encode('utf-8'))
            except Exception as e:
                self.send_response(400)
                self._set_cors()
                self.end_headers()
                self.wfile.write(json.dumps({'success': False, 'error': str(e)}).encode('utf-8'))
            return

        self.send_response(404)
        self.end_headers()

def run_relay(port=8088):
    server = HTTPServer(('0.0.0.0', port), ColabRelayHandler)
    print(f"[READY] jDroid-X Colab Cloud Relay listening on port {port}")
    print("[INFO] Use cloudflared or ngrok in Colab to expose public URL:")
    print("       !wget https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64")
    print("       !chmod +x cloudflared-linux-amd64")
    print(f"       !./cloudflared-linux-amd64 tunnel --url http://127.0.0.1:{port}")
    server.serve_forever()

if __name__ == '__main__':
    run_relay()
