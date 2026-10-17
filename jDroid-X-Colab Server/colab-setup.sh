#!/usr/bin/env bash
# ==============================================================================
# jDroid-X-Desk | Google Colab Relay & Remote Desktop Setup Bootstrap
# Subsidiary of jDroid-X Studio
# ==============================================================================

set -e

echo "============================================================"
echo "  Setting up jDroid-X Colab Cloud Relay Server Environment  "
echo "============================================================"

# 1. Download latest Cloudflare Tunnel binary if not present
if [ ! -f "cloudflared-linux-amd64" ]; then
    echo "[1/4] Downloading Cloudflare Tunnel binary (cloudflared)..."
    wget -q -nc https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64
    chmod +x cloudflared-linux-amd64
    echo "      Done."
else
    echo "[1/4] cloudflared-linux-amd64 already present."
fi

# 2. Ensure Python 3 is installed
echo "[2/4] Checking Python 3 runtime..."
python3 --version

# 3. Launch Central Relay Server in background
echo "[3/4] Starting jDroid-X Cloud Relay Server on port 8088..."
python3 jDroid-X-Colab-Relay-Server.py 8088 > relay.log 2>&1 &
RELAY_PID=$!
echo "      Relay Server running with PID: $RELAY_PID"

# 4. Start Autonomous Watchdog in background
echo "[4/4] Starting Colab Watchdog & Keep-Alive Daemon..."
python3 colab-watchdog-keepalive.py > watchdog.log 2>&1 &
WATCHDOG_PID=$!
echo "      Watchdog running with PID: $WATCHDOG_PID"

sleep 2

echo "============================================================"
echo "  jDroid-X Cloud Relay is ONLINE!                           "
echo "  Launching Cloudflare Tunnel to expose Public URL...       "
echo "============================================================"
./cloudflared-linux-amd64 tunnel --url http://127.0.0.1:8088
