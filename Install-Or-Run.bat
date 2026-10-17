@echo off
title jDroid-X-Desk Universal Launcher
cd /d "%~dp0"
echo ======================================================================
echo   jDroid-X-Desk Portable Workstation Launcher (jDroid-X Studio)
echo ======================================================================
echo [1/2] Verifying application integrity and unblocking files...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%~dp0' -Recurse | Unblock-File -ErrorAction SilentlyContinue"
echo [2/2] Launching jDroid-X-Desk Enterprise Console...
start "" "%~dp0jDroid-X-Desk.exe"
exit /b 0
