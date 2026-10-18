# ==============================================================================
# jDroid-X-Desk | Google Colab Autonomous Watchdog & Keep-Alive Daemon v1.0
# Subsidiary of jDroid-X Studio | Remote Desktop Cloud Bridge
# Purpose: Prevents Colab runtime idle disconnects, auto-recovers relay,
#          monitors Cloudflare Tunnel health, and processes incoming pings.
# ==============================================================================

import os
import sys
import time
import urllib.request
import subprocess

RELAY_PORT = int(os.environ.get("RELAY_PORT", 8088))
PING_INTERVAL_SECONDS = 60
PING_URL = f"http://127.0.0.1:{RELAY_PORT}/ping"

print("=" * 60)
print("  jDroid-X Colab Watchdog & Keep-Alive Daemon Active")
print(f"  Target Local Relay: {PING_URL}")
print(f"  Check Interval:     {PING_INTERVAL_SECONDS} seconds")
print("=" * 60)

def is_relay_alive():
    """Checks if the local relay server answers to ping."""
    try:
        req = urllib.request.Request(PING_URL, headers={'User-Agent': 'jDroid-X-Colab-Watchdog/1.0'})
        with urllib.request.urlopen(req, timeout=5) as response:
            return response.status == 200
    except Exception:
        return False

def check_process_running(proc_name):
    """Checks if a named process is active in Linux."""
    try:
        output = subprocess.check_output(["pgrep", "-f", proc_name]).decode()
        return bool(output.strip())
    except Exception:
        return False

def main_watchdog_loop():
    iteration = 0
    while True:
        iteration += 1
        current_time = time.strftime("%Y-%m-%d %H:%M:%S")

        # 1. Ping local relay server
        if is_relay_alive():
            print(f"[{current_time}] [WATCHDOG-PING #{iteration}] Local Relay on port {RELAY_PORT} is ACTIVE & AWAKE.")
        else:
            print(f"[{current_time}] [WATCHDOG-ALERT] Local Relay on port {RELAY_PORT} not responding! Attempting auto-recovery...")
            try:
                subprocess.Popen(["python3", "jDroid-X-Colab-Relay-Server.py", str(RELAY_PORT)])
                time.sleep(3)
            except Exception as e:
                print(f"[WATCHDOG-ERROR] Failed to start relay server: {e}")

        # 2. Check Cloudflared tunnel process
        cloudflared_active = check_process_running("cloudflared")
        if not cloudflared_active:
            print(f"[{current_time}] [WATCHDOG-WARN] Cloudflared tunnel process not detected. If running in Colab, start tunnel in foreground or tmux.")

        time.sleep(PING_INTERVAL_SECONDS)

if __name__ == "__main__":
    try:
        main_watchdog_loop()
    except KeyboardInterrupt:
        print("\n[WATCHDOG] Exiting watchdog daemon.")
