@echo off
setlocal
TITLE jDroid-X Gateway & Node.js Server Launcher (Port 8080)
cd /d "%~dp0"

echo ========================================================
echo   jDroid-X-Desk Unified Gateway & Node.js Server
echo ========================================================
echo.

set GATEWAY_PORT=7700
if not "%~1"=="" set GATEWAY_PORT=%~1

where node >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Node.js is not found in your PATH!
    echo Please install Node.js from https://nodejs.org
    echo.
    pause
    exit /b 1
)

:: Check if server is already running on gateway port
netstat -ano | findstr /R /C:":%GATEWAY_PORT% *LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [INFO] Gateway server is already active on port %GATEWAY_PORT%
    goto OPEN_BROWSER
)

echo [INFO] Starting Node.js Server on Gateway Port %GATEWAY_PORT%...
set PORT=%GATEWAY_PORT%
start "jDroid-X Gateway" /min cmd /c "node server.js"

echo [INFO] Waiting for Gateway to listen on port %GATEWAY_PORT%...
set /a COUNT=0
:WAIT_PORT
ping 127.0.0.1 -n 2 >nul
netstat -ano | findstr /R /C:":%GATEWAY_PORT% *LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 goto SERVER_READY
set /a COUNT+=1
if %COUNT% LSS 6 goto WAIT_PORT

:SERVER_READY
echo [PASS] Server is live on http://localhost:%GATEWAY_PORT%!

:OPEN_BROWSER
set GATEWAY_URL=http://localhost:%GATEWAY_PORT%/jdroid-x-desk/index.html#remote-console
echo [INFO] Launching Gateway Route: %GATEWAY_URL%
start msedge --app=%GATEWAY_URL% 2>nul
if %ERRORLEVEL% NEQ 0 (
    start %GATEWAY_URL%
)

echo.
echo ========================================================
echo   Gateway Mounted at: %GATEWAY_URL%
echo   Server Admin: http://localhost:%GATEWAY_PORT%/jdroid-x-desk/admin.html#server-admin
echo ========================================================
echo.
ping 127.0.0.1 -n 3 >nul
exit /b 0
