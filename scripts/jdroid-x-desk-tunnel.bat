@echo off
setlocal EnableDelayedExpansion
TITLE "jDroid-X-Desk Cloud Ingress & Tunnel Supervisor (desk.jdroid-x.com)"
cd /d "%~dp0.."

:: [Variables] | [Constants] | [Dependencies] | [Integration Points]
:: Target Tunnel: ad79a58a-f017-49a0-a560-7f654565a482
:: Domain: desk.jdroid-x.com
:: Port: 7700
:: Dependencies: cloudflared.exe, node.exe, server.js

for %%I in ("%~dp0..") do set "WORKSPACE=%%~fI"
set "TUNNEL_ID=ad79a58a-f017-49a0-a560-7f654565a482"
set "LOCAL_PORT=7700"
set "PUBLIC_URL=https://desk.jdroid-x.com"
set "CF_BIN=%WORKSPACE%\cloudflared.exe"

:: Dynamic Settings Model Binding (Eliminates hardcoded values via database.json)
if exist "%WORKSPACE%\database.json" (
    for /f "tokens=1,2,3 delims=|" %%A in ('node -e "try{const d=require('./database.json');const c=(d.settings&&d.settings.connection)||{};const p=c.tunnelPort||7700;const u=c.customDomain||'https://desk.jdroid-x.com';const t=c.tunnelId||c.tunnelToken||'ad79a58a-f017-49a0-a560-7f654565a482';console.log(p+'|'+u+'|'+t)}catch(e){console.log('7700|https://desk.jdroid-x.com|ad79a58a-f017-49a0-a560-7f654565a482')}" 2^>nul') do (
        set "LOCAL_PORT=%%A"
        set "PUBLIC_URL=%%B"
        set "TUNNEL_ID=%%C"
    )
)

if not exist "!CF_BIN!" (
    where cloudflared >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        for /f "delims=" %%i in ('where cloudflared') do set "CF_BIN=%%i"
    ) else (
        echo [ERROR] cloudflared.exe not found in workspace or PATH.
        exit /b 1
    )
)

if "%1"=="--stop" (
    echo [*] Stopping jDroid-X Tunnel and Gateway...
    taskkill /F /IM cloudflared.exe >nul 2>&1
    for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":!LOCAL_PORT! *LISTENING"') do (
        taskkill /F /PID %%p >nul 2>&1
    )
    echo [OK] Stopped.
    exit /b 0
)

if "%1"=="--status" (
    echo [*] Checking jDroid-X Tunnel Status...
    netstat -ano | findstr ":!LOCAL_PORT! *LISTENING" >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo [OK] Node.js Gateway listening on Port !LOCAL_PORT!
    ) else (
        echo [FAIL] Node.js Gateway is NOT listening on Port !LOCAL_PORT!
    )
    tasklist /FI "IMAGENAME eq cloudflared.exe" 2>nul | findstr /I "cloudflared.exe" >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo [OK] Cloudflare Tunnel Process is ACTIVE
    ) else (
        echo [FAIL] Cloudflare Tunnel Process is NOT ACTIVE
    )
    curl.exe -f -I -m 5 !PUBLIC_URL! >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo [OK] Public URL !PUBLIC_URL! is HTTP 200 ONLINE
    ) else (
        echo [FAIL] Public URL !PUBLIC_URL! is OFFLINE
    )
    exit /b 0
)

if "%1"=="--install-startup" (
    echo [*] Installing jDroid-X Silent Background Tunnel to Windows Startup...
    powershell -NoProfile -ExecutionPolicy Bypass -File "!WORKSPACE!\scripts\install-startup.ps1"
    exit /b 0
)

echo ========================================================
echo   jDroid-X-Desk Public Cloud Ingress (desk.jdroid-x.com)
echo   Target: Cloudflare Tunnel !TUNNEL_ID!
echo   Local Origin: http://127.0.0.1:!LOCAL_PORT!
echo ========================================================
echo.

:: 1. Ensure Node.js Gateway is active on Port 7700 [Detached / Persistent]
netstat -ano | findstr /R /C:":!LOCAL_PORT! *LISTENING" >nul 2>&1
if !ERRORLEVEL! NEQ 0 (
    echo [1/3] Starting jDroid-X Node.js Gateway on Port !LOCAL_PORT! [Background]
    start "jDroid-X Gateway" /min cmd /c "node server.js"
    timeout /t 3 /nobreak >nul 2>&1
) else (
    echo [1/3] Node.js Gateway already active on Port !LOCAL_PORT! [OK]
)

:: 2. Ensure Cloudflare Tunnel is active [Detached / Persistent]
tasklist /FI "IMAGENAME eq cloudflared.exe" 2>nul | findstr /I "cloudflared.exe" >nul 2>&1
if !ERRORLEVEL! NEQ 0 (
    if exist "!USERPROFILE!\.cloudflared\config.yml" (
        echo [2/3] Connecting Cloudflare Edge Named Tunnel to desk.jdroid-x.com [Background]
        start "jDroid-X Cloudflare Tunnel" /min "!CF_BIN!" --config "!USERPROFILE!\.cloudflared\config.yml" tunnel run !TUNNEL_ID!
    ) else (
        echo [2/3] Connecting Zero-Config Cloudflare Quick Tunnel to 127.0.0.1:!LOCAL_PORT! [Background]
        start "jDroid-X Cloudflare Tunnel" /min "!CF_BIN!" tunnel --protocol http2 --url http://127.0.0.1:!LOCAL_PORT!
    )
    timeout /t 5 /nobreak >nul 2>&1
) else (
    echo [2/3] Cloudflare Tunnel already active [OK]
)

:: 3. Verify public endpoint with retry
echo [3/3] Probing public endpoint: !PUBLIC_URL!...
set "IS_ONLINE=0"
for /L %%i in (1,1,5) do (
    curl.exe -f -I -m 5 !PUBLIC_URL! >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        set "IS_ONLINE=1"
        goto :ONLINE_SUCCESS
    )
    timeout /t 2 /nobreak >nul 2>&1
)

:ONLINE_SUCCESS
if "!IS_ONLINE!"=="1" (
    echo.
    echo ========================================================
    echo   [SUCCESS] desk.jdroid-x.com is LIVE and healthy [HTTP 200]
    echo   Public Portal: !PUBLIC_URL!
    echo   Health Endpoint: !PUBLIC_URL!/api/health
    echo ========================================================
) else (
    echo.
    echo [NOTICE] Tunnel connecting to Cloudflare Edge. Re-check in a few moments.
)

exit /b 0
