@echo off
setlocal EnableDelayedExpansion

:: jDroid-X Startup Background Service Launcher
cd /d "c:\Users\jiten\jAnitGravity\jDroid-X-Desk"
set "LOG=c:\Users\jiten\jAnitGravity\jDroid-X-Desk\scripts\startup.log"
echo [%date% %time%] Windows Logon Ingress Initiated >> "%LOG%"

:: 1. Grace period for Network / Wi-Fi adapter initialization
timeout /t 5 /nobreak >nul 2>&1

:: 2. Ensure Node.js Gateway is running on Port 7700
netstat -ano | findstr ":7700 *LISTENING" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [%date% %time%] Spawning Node Gateway on Port 7700 >> "%LOG%"
    start "" /min "C:\jDroid-X-AI-Tools\NodeJS\node.exe" server.js
    timeout /t 3 /nobreak >nul 2>&1
) else (
    echo [%date% %time%] Node Gateway already listening on 7700 >> "%LOG%"
)

:: 3. Ensure Cloudflare Tunnel is running
tasklist /FI "IMAGENAME eq cloudflared.exe" 2>nul | findstr /I "cloudflared.exe" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [%date% %time%] Spawning Cloudflare Edge Tunnel >> "%LOG%"
    start "" /min "c:\Users\jiten\jAnitGravity\jDroid-X-Desk\cloudflared.exe" --config "C:\Users\jiten\.cloudflared\config.yml" tunnel run ad79a58a-f017-49a0-a560-7f654565a482
) else (
    echo [%date% %time%] Cloudflare Tunnel already active >> "%LOG%"
)

echo [%date% %time%] Startup Ingress Completed >> "%LOG%"
exit /b 0
