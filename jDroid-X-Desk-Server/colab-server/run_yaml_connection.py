# ==============================================================================
# jDroid-X-Desk | YAML-Driven Connection Executor
# Executes relay server and tunnel directly from windows_rdp.yaml configuration
# Supports both Google Colab (Linux) and Windows local execution
# ==============================================================================

import os
import sys
import re
import json
import subprocess
import time
import threading

def parse_simple_yaml(file_path):
    """Parses windows_rdp.yaml with zero external dependencies."""
    config = {}
    if not os.path.exists(file_path):
        return config
    
    current_section = None
    sub_section = None
    
    with open(file_path, 'r', encoding='utf-8') as f:
        for line in f:
            raw = line.split('#')[0].rstrip()
            if not raw:
                continue
            
            indent = len(line) - len(line.lstrip())
            stripped = raw.strip()
            
            if indent == 0 and stripped.endswith(':'):
                current_section = stripped[:-1].strip()
                config[current_section] = {}
                sub_section = None
            elif indent == 2 and stripped.endswith(':'):
                sub_section = stripped[:-1].strip()
                if current_section:
                    config[current_section][sub_section] = {}
            elif ':' in stripped:
                k, v = stripped.split(':', 1)
                k = k.strip()
                v = v.strip().strip('"').strip("'")
                
                if v.isdigit():
                    v = int(v)
                elif v.lower() == 'true':
                    v = True
                elif v.lower() == 'false':
                    v = False
                    
                if sub_section and current_section:
                    config[current_section][sub_section][k] = v
                elif current_section:
                    config[current_section][k] = v
                else:
                    config[k] = v
    return config

def main():
    print("=" * 70)
    print("   jDroid-X-Desk | Automated Cloud Relay & Connection Engine")
    print("   Architecture: Enterprise YAML-Driven Micro-Broker")
    print("=" * 70)

    yaml_path = "windows_rdp.yaml"
    if not os.path.exists(yaml_path):
        alt_path = os.path.join(os.path.dirname(__file__), "windows_rdp.yaml")
        if os.path.exists(alt_path):
            yaml_path = alt_path

    cfg = parse_simple_yaml(yaml_path)
    colab_cfg = cfg.get('colab', {})
    conn_cfg = cfg.get('connection_pipeline', {})

    relay_port = colab_cfg.get('relay_port', 8088)
    custom_domain = colab_cfg.get('custom_domain', 'https://www.jdroid-x.com')
    desk1_id = conn_cfg.get('desk1', {}).get('default_id', '984-102-391')
    desk2_id = conn_cfg.get('desk2', {}).get('default_id', '710-224-900')

    print(f"[*] Configuration Loaded:")
    print(f"    - Target Domain / Web:  {custom_domain}")
    print(f"    - Relay Port:           {relay_port}")
    print(f"    - Desk 1 (Viewer):      {desk1_id}")
    print(f"    - Desk 2 (Target Host): {desk2_id}")
    print("-" * 70)

    # 1. Start Relay Server in Background
    relay_script = "colab_relay_server.py"
    if not os.path.exists(relay_script):
        alt_script = os.path.join(os.path.dirname(__file__), "colab_relay_server.py")
        if os.path.exists(alt_script):
            relay_script = alt_script

    print(f"[*] Launching Central Relay Server on port {relay_port}...")
    server_proc = subprocess.Popen([sys.executable, relay_script, str(relay_port)])
    time.sleep(2)
    print(f"[+] Central Relay Server is active on PID {server_proc.pid}")

    # 2. Check Platform
    is_linux = sys.platform.startswith('linux')

    if is_linux:
        print("[*] Environment: Linux (Google Colab Container)")
        tunnel_bin = "./cloudflared-linux-amd64"
        
        if not os.path.exists(tunnel_bin):
            print("[*] Downloading secure cloudflared tunneling engine...")
            subprocess.run(["wget", "-q", "-nc", "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64"])
            subprocess.run(["chmod", "+x", tunnel_bin])

        tunnel_token = os.environ.get("CLOUDFLARE_TUNNEL_TOKEN", "").strip()
        if not tunnel_token and 'tunnel_token' in colab_cfg:
            tunnel_token = colab_cfg['tunnel_token']

        if tunnel_token:
            print("=" * 70)
            print(f"🚀 ROUTING THROUGH CUSTOM DOMAIN: {custom_domain}")
            print(f"   All jDroid-X-Desk instances can connect directly via: {custom_domain}")
            print("=" * 70)
            cmd = [tunnel_bin, "tunnel", "--protocol", "http2", "run", "--token", tunnel_token]
            subprocess.run(cmd)
        else:
            print("[*] Initializing secure HTTPS tunnel over HTTP/2 protocol...")
            cmd = [tunnel_bin, "tunnel", "--protocol", "http2", "--url", f"http://127.0.0.1:{relay_port}"]
            
            # Run cloudflared and capture the public URL cleanly
            proc = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, bufsize=1)
            tunnel_url = None
            url_pattern = re.compile(r'https://[a-zA-Z0-9-]+\.trycloudflare\.com')

            for line in iter(proc.stdout.readline, ''):
                if not tunnel_url:
                    match = url_pattern.search(line)
                    if match:
                        tunnel_url = match.group(0)
                        print("\n" + "=" * 70)
                        print("🎉 jDroid-X CLOUD RELAY SERVER IS ONLINE & READY!")
                        print("=" * 70)
                        print(f"\n   PUBLIC RELAY URL:\n   👉  {tunnel_url}  👈\n")
                        print("   STEPS TO CONNECT IN jDroid-X-Desk:")
                        print("   1. Open jDroid-X-Desk on Desk 1 and Desk 2")
                        print("   2. In the top header, click the 'Colab Relay' status pill")
                        print(f"   3. Enter: {tunnel_url}")
                        print("   4. Click 'Save & Apply' -> Header turns GREEN with live meters!")
                        print("=" * 70 + "\n")
                        print("[*] Server is streaming active frame relays. Press Ctrl+C to stop.\n")
                elif "error" in line.lower() and "failed" in line.lower():
                    # Filter benign logs, only print genuine errors
                    if "hard_fail=false" not in line and "precheck" not in line:
                        print("[Tunnel Note]:", line.strip())

            proc.wait()
    else:
        print("[*] Environment: Windows Desktop / Local Machine")
        print(f"[+] Local Relay Server is active at http://127.0.0.1:{relay_port}")
        print(f"[+] Direct connections can connect via your Local IP: http://localhost:{relay_port}")
        print(f"[+] Or via your configured domain: {custom_domain}")
        try:
            while True:
                time.sleep(1)
        except KeyboardInterrupt:
            server_proc.terminate()
            print("\n[*] Relay Server stopped.")

if __name__ == '__main__':
    main()
