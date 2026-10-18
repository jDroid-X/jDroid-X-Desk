@echo off
TITLE jDroid-X Engine Launcher: Sunshine & Moonlight
echo ========================================================
echo   jDroid-X Engine: Sunshine & Moonlight (Ultra-Low Latency)
echo ========================================================
echo.

tasklist /FI "IMAGENAME eq sunshine.exe" 2>NUL | find /I /N "sunshine.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [✓ ACTIVE] Sunshine process is already running on port 47990.
    echo Opening Web UI Configuration Portal...
    start "" "https://localhost:47990"
    exit /b 0
)

echo [INFO] Searching for Sunshine installation...
set "SUNSHINE_PATH="
if exist "%ProgramFiles%\LizardByte\Sunshine\sunshine.exe" set "SUNSHINE_PATH=%ProgramFiles%\LizardByte\Sunshine\sunshine.exe"
if exist "%LocalAppData%\LizardByte\Sunshine\sunshine.exe" set "SUNSHINE_PATH=%LocalAppData%\LizardByte\Sunshine\sunshine.exe"
if exist "%~dp0..\jDroid-X-Desk-Server\Github\Sunshine-master\build\sunshine.exe" set "SUNSHINE_PATH=%~dp0..\jDroid-X-Desk-Server\Github\Sunshine-master\build\sunshine.exe"

if defined SUNSHINE_PATH (
    echo [LAUNCH] Starting Sunshine from: %SUNSHINE_PATH%
    start "" "%SUNSHINE_PATH%"
    timeout /t 2 >NUL
    start "" "https://localhost:47990"
    echo [PASS] Sunshine started successfully!
    exit /b 0
) else (
    echo [WARN] Sunshine binary not found in standard paths.
    echo Attempting Windows Service start...
    net start Sunshine 2>NUL
    if "%ERRORLEVEL%"=="0" (
        echo [PASS] Sunshine Windows Service started.
        start "" "https://localhost:47990"
        exit /b 0
    )
    echo [NOTICE] Opening Sunshine Web Configuration / Setup Guide...
    start "" "https://localhost:47990"
    exit /b 0
)
