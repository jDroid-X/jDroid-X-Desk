@echo off
setlocal
TITLE jDroid-X-Desk Launcher
cd /d "%~dp0.."

echo ========================================================
echo          jDroid-X-Desk Remote Desktop Console
echo ========================================================
echo.

:: 1. Verify Node.js
where node >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Node.js is not found in your PATH!
    echo Please install Node.js from https://nodejs.org
    echo.
    pause
    exit /b 1
)

:: 2. Check if server is already running on port 8080 or 12345
set LAUNCH_URL=http://localhost:8080/jdroid-x-desk/index.html#remote-console
netstat -ano | findstr /R /C:":8080 *LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [INFO] Gateway server is already active on port 8080
    goto OPEN_BROWSER
)

netstat -ano | findstr /R /C:":12345 *LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [INFO] Server is already active on http://localhost:12345
    set LAUNCH_URL=http://localhost:12345
    goto OPEN_BROWSER
)

:: 3. Start server in a background/minimized process
echo [INFO] Starting background gateway server...
start "jDroid-X Server" /min cmd /c "node server.js"

:: 4. Wait for server to become active (up to 5 seconds)
echo [INFO] Waiting for server on gateway port 8080...
set /a COUNT=0
:WAIT_PORT
ping 127.0.0.1 -n 2 >nul
netstat -ano | findstr /R /C:":8080 *LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 goto SERVER_READY
netstat -ano | findstr /R /C:":12345 *LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    set LAUNCH_URL=http://localhost:12345
    goto SERVER_READY
)
set /a COUNT+=1
if %COUNT% LSS 5 goto WAIT_PORT

:SERVER_READY
echo [PASS] Server is listening!

:OPEN_BROWSER
echo [INFO] Launching user interface in browser (%LAUNCH_URL%)...
start msedge --app=%LAUNCH_URL% 2>nul
if %ERRORLEVEL% NEQ 0 (
    start %LAUNCH_URL%
)

echo.
echo ========================================================
echo   jDroid-X-Desk is running at %LAUNCH_URL%
echo   To stop the server, run: stop_jdroid_app.bat
echo ========================================================
echo.
ping 127.0.0.1 -n 3 >nul
exit /b 0
