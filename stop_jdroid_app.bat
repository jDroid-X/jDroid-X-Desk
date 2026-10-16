@echo off
TITLE Stop jDroid-X-Desk Server
cd /d "%~dp0"
ECHO Stopping jDroid-X-Desk background server on port 12345...

FOR /F "tokens=5" %%P IN ('netstat -aon ^| findstr :12345') DO (
    IF NOT "%%P"=="0" (
        ECHO Killing server process PID: %%P
        taskkill /F /PID %%P >nul 2>&1
    )
)

ECHO Stopping background System Tray icon...
powershell -NoProfile -Command "Get-CimInstance Win32_Process | Where-Object CommandLine -Match 'tray_icon.ps1' | ForEach-Object { Stop-Process -Id $_.ProcessId -Force }" >nul 2>&1

ECHO Server and System Tray stopped successfully.
ping 127.0.0.1 -n 2 >nul

