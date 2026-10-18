#!/bin/bash
# ========================================================
#       jDroid-X-Desk Server Stopper (UNIX)
# ========================================================

echo "Attempting to stop jDroid-X-Desk server on port 12345..."

# Find PID using lsof or netstat/ss
PID=""
if command -v lsof >/dev/null 2>&1; then
    PID=$(lsof -ti :12345)
elif command -v ss >/dev/null 2>&1; then
    PID=$(ss -lptn 'sport = :12345' 2>/dev/null | grep -o 'pid=[0-9]*' | cut -d= -f2)
fi

if [ -z "$PID" ]; then
    echo "[INFO] No server found running on port 12345."
else
    echo "[INFO] Found server running with PID: $PID"
    kill $PID
    if [ $? -eq 0 ]; then
        echo "[SUCCESS] Server stopped gracefully."
    else
        echo "[WARNING] Could not stop server. You may need to use 'kill -9 $PID'."
    fi
fi
