@echo off
TITLE jDroid-X-Desk Smart Launcher
ECHO ========================================================
ECHO       jDroid-X-Desk Smart Launcher
ECHO ========================================================
ECHO.

:: Bypassed installation checks as requested
ECHO Launching Remote Desktop Console...

:: Launch System Tray Icon securely in the visible user session
powershell -Command "$p = Get-CimInstance Win32_Process; if (($p | Where-Object CommandLine -Match 'tray_icon.ps1').Count -gt 0) { exit 0 } else { exit 1 }"
IF %ERRORLEVEL% EQU 0 (
    ECHO [INFO] jDroid-X-Desk is already active in the system tray.
    ECHO Opening the app interface...
    start msedge --app=http://localhost:12345
    EXIT /B 0
) ELSE (
    start "" powershell -WindowStyle Hidden -ExecutionPolicy Bypass -File "%~dp0tray_icon.ps1"
)

:: Launch the background VBScript
wscript.exe "%~dp0start_jdroid_app.vbs"

:: Exit immediately so the command prompt doesn't stay open
EXIT /B 0
