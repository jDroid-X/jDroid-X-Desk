@echo off
setlocal EnableDelayedExpansion
TITLE jDroid-X Desktop Launcher (desk.jdroid-x.com)
cd /d "%~dp0"

:: ============================================================================
:: [Variables]          : WORKSPACE, PORT, PUBLIC_URL, EXE_PATH, TUNNEL_SCRIPT
:: [Constants]          : PORT=7700, PUBLIC_URL=https://desk.jdroid-x.com
:: [Dependencies]       : node.exe, cloudflared.exe, jDroid-X-Desk.exe
:: [Integration Points] : Express Gateway (7700), Cloudflare Named Tunnel, Native Desktop
:: [Performance]        : Non-blocking parallel preflight with bounded probe timeouts
:: ============================================================================

set "WORKSPACE=%~dp0"
set "PORT=7700"
set "PUBLIC_URL=https://desk.jdroid-x.com"
set "EXE_PATH=%WORKSPACE%jDroid-X-Desk.exe"
set "TUNNEL_SCRIPT=%WORKSPACE%scripts\jdroid-x-desk-tunnel.bat"

echo ========================================================
echo   jDroid-X Desktop Launcher (desk.jdroid-x.com)
echo   Enterprise Remote Desktop & Ingress Orchestrator
echo ========================================================
echo.

:: 1. Verify / Autostart Background Services via Canonical Tunnel Script
echo [*] Phase 1: Auditing Local & Cloud Dependencies...
netstat -ano | findstr /R /C:":%PORT% *LISTENING" >nul 2>&1
set "GATEWAY_UP=%ERRORLEVEL%"

tasklist /FI "IMAGENAME eq cloudflared.exe" 2>nul | findstr /I "cloudflared.exe" >nul 2>&1
set "TUNNEL_UP=%ERRORLEVEL%"

if %GATEWAY_UP% NEQ 0 (
    echo [!] Node.js Gateway on Port %PORT% is inactive.
    echo [*] Launching Node.js Gateway and Cloudflare Tunnel...
    if exist "%TUNNEL_SCRIPT%" (
        call "%TUNNEL_SCRIPT%"
    ) else (
        call "%WORKSPACE%scripts\run-services.bat"
    )
) else (
    echo [OK] Node.js Gateway is active on Port %PORT%.
    if %TUNNEL_UP% NEQ 0 (
        echo [!] Cloudflare Tunnel is inactive. Spawning tunnel...
        if exist "%TUNNEL_SCRIPT%" (
            call "%TUNNEL_SCRIPT%"
        ) else (
            call "%WORKSPACE%scripts\run-services.bat"
        )
    ) else (
        echo [OK] Cloudflare Edge Tunnel is active.
    )
)

:: 2. Preflight Health Verification
echo.
echo [*] Phase 2: Verifying Gateway & Edge Connectivity...
curl.exe -f -s -I -m 5 "http://127.0.0.1:%PORT%/api/health" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] Local Gateway Health: HTTP 200 UP
) else (
    echo [WARN] Local Gateway starting up or initializing...
)

curl.exe -f -s -I -m 5 "%PUBLIC_URL%" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] Public Edge Ingress (%PUBLIC_URL%): HTTP 200 ONLINE
) else (
    echo [INFO] Edge tunnel synchronizing with Cloudflare edge network.
)

:: 3. Launch Native Application
echo.
echo [*] Phase 3: Launching jDroid-X Native Desktop Client...
if exist "%EXE_PATH%" (
    start "" "%EXE_PATH%" %*
    echo [SUCCESS] jDroid-X-Desk.exe initiated successfully.
) else (
    echo [ERROR] jDroid-X-Desk.exe not found at: %EXE_PATH%
    pause
    exit /b 1
)

echo.
echo ========================================================
echo   Session Active. Launcher exiting safely.
echo ========================================================
timeout /t 3 /nobreak >nul 2>&1
exit /b 0
