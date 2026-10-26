@echo off
setlocal
TITLE "jDroid-X-Desk Cloud Ingress & Tunnel Supervisor (desk.jdroid-x.com)"
cd /d "%~dp0.."

echo ========================================================
echo   jDroid-X-Desk Public Cloud Ingress (desk.jdroid-x.com)
echo   Target: Cloudflare Tunnel ad79a58a-f017-49a0-a560-7f654565a482
echo   Local Origin: http://127.0.0.1:7700
echo ========================================================
echo.

:: 1. Ensure Node.js Gateway is active on Port 7700
netstat -ano | findstr /R /C:":7700 *LISTENING" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [1/3] Starting jDroid-X Node.js Gateway on Port 7700...
    start "jDroid-X Gateway (7700)" /min cmd /c "node server.js"
    timeout /t 2 /nobreak >nul 2>&1
) else (
    echo [1/3] Node.js Gateway already active on Port 7700 [OK]
)

:: 2. Check for cloudflared binary
set "CF_BIN=%~dp0..\cloudflared.exe"
if not exist "%CF_BIN%" (
    echo [!] cloudflared.exe not found in workspace. Checking PATH...
    where cloudflared >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        set "CF_BIN=cloudflared"
    ) else (
        echo [ERROR] cloudflared.exe not found. Run compile_native.bat or download it.
        pause
        exit /b 1
    )
)

:: 3. Check if tunnel is already active
netstat -ano | findstr /I "cloudflared" >nul 2>&1

echo [2/3] Connecting Cloudflare Edge Tunnel to desk.jdroid-x.com...
start "Cloudflare Tunnel" /min "%CF_BIN%" tunnel run ad79a58a-f017-49a0-a560-7f654565a482

timeout /t 3 /nobreak >nul 2>&1

:: 4. Verify public endpoint
echo [3/3] Probing public URL: https://desk.jdroid-x.com...
curl.exe -I -m 6 https://desk.jdroid-x.com >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo   [SUCCESS] desk.jdroid-x.com is LIVE and healthy!
    echo   Public Portal: https://desk.jdroid-x.com
    echo   Health Endpoint: https://desk.jdroid-x.com/api/health
    echo ========================================================
) else (
    echo.
    echo [NOTICE] Tunnel connecting to Cloudflare Edge. Re-check in a few seconds.
)

exit /b 0
