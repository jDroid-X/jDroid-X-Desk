@echo off
setlocal
TITLE jDroid-X-Desk Native Launcher
cd /d "%~dp0"

:: Strangler Forwarder: Delegate to encapsulated scripts/launch-native.bat
if exist "scripts\launch-native.bat" (
    call "scripts\launch-native.bat" %*
    exit /b %ERRORLEVEL%
)

echo ========================================================
echo   jDroid-X-Desk Pure Native Windows Machine Console
echo ========================================================
echo.

set REBUILD_NEEDED=0

if "%1"=="/rebuild" set REBUILD_NEEDED=1
if "%1"=="/compile" set REBUILD_NEEDED=1

if not exist "jDroid-X-Desk.exe" (
    set REBUILD_NEEDED=1
)

if "%REBUILD_NEEDED%"=="1" (
    echo [INFO] Compiling modular native solution from Native/ directory...
    call compile_native.bat
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] Native compilation failed.
        pause
        exit /b 1
    )
    echo [PASS] Compiled jDroid-X-Desk.exe successfully!
)

echo [PASS] Launching jDroid-X-Desk.exe...
start "" "jDroid-X-Desk.exe"
exit /b 0
