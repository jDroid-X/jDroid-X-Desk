#!/bin/bash
# ========================================================
#       jDroid-X-Desk Smart Launcher (UNIX)
# ========================================================

echo "========================================================"
echo "      jDroid-X-Desk Smart Launcher"
echo "========================================================"
echo ""

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"

# Check if Node.js is installed
if ! command -v node >/dev/null 2>&1; then
    echo "[INFO] Node.js is missing. Initiating First-Time Setup..."
    bash "$SCRIPT_DIR/install_unix_app.sh"
    exit 0
fi

# Check if dependencies (node_modules) are installed
if [ ! -d "$SCRIPT_DIR/node_modules" ]; then
    echo "[INFO] Dependencies are missing. Initiating First-Time Setup..."
    bash "$SCRIPT_DIR/install_unix_app.sh"
    exit 0
fi

echo "[SUCCESS] jDroid-X-Desk is already installed."
echo "Launching Remote Desktop Console..."

# Navigate to project directory
cd "$SCRIPT_DIR" || exit 1

# Start the Node server in the background
nohup node server.js > server_unix.log 2>&1 &
NODE_PID=$!

echo "[INFO] Server started in background with PID: $NODE_PID"

# Wait a second for the server to bind to port
sleep 1.5

# Open browser (Mac: open, Linux: xdg-open)
if command -v xdg-open >/dev/null 2>&1; then
    xdg-open "http://localhost:12345"
elif command -v open >/dev/null 2>&1; then
    open "http://localhost:12345"
else
    echo "[WARNING] Could not detect browser opener. Please manually open http://localhost:12345"
fi

exit 0
