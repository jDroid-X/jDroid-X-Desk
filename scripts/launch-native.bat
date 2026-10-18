@echo off
setlocal
TITLE jDroid-X-Desk Native Launcher
cd /d "%~dp0.."

echo ========================================================
echo   jDroid-X-Desk Pure Native Windows Machine Console
echo ========================================================
echo.

set REBUILD_NEEDED=0

if "%1"=="/rebuild" set REBUILD_NEEDED=1
if "%1"=="/compile" set REBUILD_NEEDED=1

if not exist "jDroid-X-Desk.exe" (
    set REBUILD_NEEDED=1
) else (
    for /f %%i in ('powershell -NoProfile -Command "if ((Get-Item 'JDroidXDeskNative.cs').LastWriteTime -gt (Get-Item 'jDroid-X-Desk.exe').LastWriteTime) { Write-Host '1' } else { Write-Host '0' }"') do set REBUILD_NEEDED=%%i
)

if "%REBUILD_NEEDED%"=="1" (
    echo [INFO] Detected source updates in JDroidXDeskNative.cs. Compiling native canonical binary...
    C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /platform:anycpu /optimize+ /debug- /win32manifest:app.manifest /r:System.dll,System.Windows.Forms.dll,System.Drawing.dll,System.Web.Extensions.dll,System.Net.dll,System.Data.dll,System.Xml.dll,System.Core.dll /out:jDroid-X-Desk.exe JDroidXDeskNative.cs
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
