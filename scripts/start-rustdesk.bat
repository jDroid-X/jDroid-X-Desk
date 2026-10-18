@echo off
TITLE jDroid-X Engine Launcher: RustDesk
echo ========================================================
echo   jDroid-X Engine: RustDesk (P2P NAT Traversal)
echo ========================================================
echo.

tasklist /FI "IMAGENAME eq rustdesk.exe" 2>NUL | find /I /N "rustdesk.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [✓ ACTIVE] RustDesk is already running!
    exit /b 0
)

echo [INFO] Searching for RustDesk executable...
set "RUSTDESK_PATH="
if exist "%ProgramFiles%\RustDesk\rustdesk.exe" set "RUSTDESK_PATH=%ProgramFiles%\RustDesk\rustdesk.exe"
if exist "%LocalAppData%\RustDesk\rustdesk.exe" set "RUSTDESK_PATH=%LocalAppData%\RustDesk\rustdesk.exe"
if exist "%~dp0..\rustdesk.exe" set "RUSTDESK_PATH=%~dp0..\rustdesk.exe"

if defined RUSTDESK_PATH (
    echo [LAUNCH] Starting RustDesk from: %RUSTDESK_PATH%
    start "" "%RUSTDESK_PATH%"
    echo [PASS] RustDesk launched successfully.
    exit /b 0
) else (
    echo [INFO] Opening RustDesk source folder in Explorer...
    start explorer.exe "%~dp0..\jDroid-X-Desk-Server\Github\rustdesk-master"
    exit /b 0
)
