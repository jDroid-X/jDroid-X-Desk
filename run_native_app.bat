@echo off
TITLE jDroid-X-Desk Direct Host Runner
cd /d "%~dp0"

echo ========================================================
echo    jDroid-X-Desk Direct Runtime Console (SAC Bypass)
echo ========================================================
echo.

if not exist "jDroid-X-Desk.exe" (
    echo [INFO] Compiling jDroid-X-Desk.exe...
    call compile_native.bat
)

echo [+] Launching jDroid-X-Desk inside trusted Windows CLR host...
start "" powershell -NoProfile -ExecutionPolicy Bypass -STA -WindowStyle Hidden -Command "$p=(Resolve-Path 'jDroid-X-Desk.exe').Path; [System.Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($p)).EntryPoint.Invoke($null, @([string[]]@()))"

exit /b 0
