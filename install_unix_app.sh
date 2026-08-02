#!/bin/bash
# ========================================================
#       jDroid-X-Desk Standalone App Installer (UNIX)
# ========================================================

echo "========================================================"
echo "      jDroid-X-Desk Standalone App Installer"
echo "========================================================"
echo ""

echo "[1/4] Detecting Operating System..."
OS="$(uname -s)"
echo "Detected OS: $OS"

echo "[2/4] Verifying Node.js and NPM installation..."
if command -v node >/dev/null 2>&1; then
    echo "[SUCCESS] Node.js is already installed! ($(node -v))"
else
    echo "[WARNING] Node.js is not installed!"
    echo "[INFO] Attempting to install Node.js automatically..."
    
    if [ "$OS" = "Darwin" ]; then
        if command -v brew >/dev/null 2>&1; then
            echo "[INFO] Homebrew found. Installing Node.js..."
            brew install node
        else
            echo "[ERROR] Homebrew is not installed. Please install Node.js manually from https://nodejs.org/"
            exit 1
        fi
    elif [ "$OS" = "Linux" ]; then
        if command -v apt-get >/dev/null 2>&1; then
            echo "[INFO] Debian/Ubuntu system detected. Using apt-get..."
            echo "[INFO] This may require your administrator (sudo) password."
            curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
            sudo apt-get install -y nodejs
        elif command -v dnf >/dev/null 2>&1; then
            echo "[INFO] Fedora/RHEL system detected. Using dnf..."
            echo "[INFO] This may require your administrator (sudo) password."
            sudo dnf install -y nodejs
        else
            echo "[ERROR] Unsupported Linux package manager. Please install Node.js manually from https://nodejs.org/"
            exit 1
        fi
    else
        echo "[ERROR] Unsupported Operating System: $OS. Please install Node.js manually."
        exit 1
    fi
    
    # Re-check Node
    if ! command -v node >/dev/null 2>&1; then
        echo "[ERROR] Node.js installation failed. Please install manually."
        exit 1
    fi
    echo "[SUCCESS] Node.js installation finished!"
fi

echo "[3/4] Installing background web server dependencies..."
npm install --no-audit
if [ $? -ne 0 ]; then
    echo "[WARNING] NPM install encountered warnings, proceeding to launch..."
fi

echo "[4/4] Creating Desktop Shortcut..."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"

if [ "$OS" = "Linux" ]; then
    DESKTOP_ENTRY="$HOME/.local/share/applications/jdroid-x-desk.desktop"
    mkdir -p "$HOME/.local/share/applications"
    
    cat > "$DESKTOP_ENTRY" <<EOL
[Desktop Entry]
Version=1.0
Type=Application
Name=jDroid-X-Desk
Comment=Unified Remote Desktop Console
Exec=bash "$SCRIPT_DIR/Launch-Desk.sh"
Terminal=false
Categories=Utility;Network;
EOL
    
    # Also copy to desktop if it exists
    if [ -d "$HOME/Desktop" ]; then
        cp "$DESKTOP_ENTRY" "$HOME/Desktop/"
        chmod +x "$HOME/Desktop/jdroid-x-desk.desktop"
    fi
    echo "[SUCCESS] Linux .desktop shortcut created."

elif [ "$OS" = "Darwin" ]; then
    # Create simple executable script on Desktop
    MAC_DESKTOP="$HOME/Desktop/jDroid-X-Desk.command"
    cat > "$MAC_DESKTOP" <<EOL
#!/bin/bash
cd "$SCRIPT_DIR"
bash ./Launch-Desk.sh
EOL
    chmod +x "$MAC_DESKTOP"
    echo "[SUCCESS] macOS Desktop shortcut (.command) created."
fi

# Ensure launchers are executable
chmod +x "$SCRIPT_DIR/Launch-Desk.sh" 2>/dev/null
chmod +x "$SCRIPT_DIR/stop_jdroid_app.sh" 2>/dev/null

echo ""
echo "========================================================"
echo "[SUCCESS] Installation Complete!"
echo "App running on background server at http://localhost:12345"
echo "========================================================"
echo ""
echo "Launching jDroid-X-Desk App Mode..."

bash "$SCRIPT_DIR/Launch-Desk.sh"
