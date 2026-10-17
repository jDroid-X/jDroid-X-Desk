@echo off
setlocal
TITLE jDroid-X-Desk Native Launcher
cd /d "%~dp0"

echo ========================================================
echo   jDroid-X-Desk Pure Native Windows Machine Console
echo ========================================================
echo.

if not exist "jDroid-X-Desk.exe" (
    echo [INFO] Compiling single canonical native binary (Universal AnyCPU + jDroid-X Studio Assembly + Icon + Manifest)...
    C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /platform:anycpu /optimize+ /win32icon:assets\app.ico /win32manifest:app.manifest /r:System.Windows.Forms.dll,System.Drawing.dll,System.Web.Extensions.dll,System.Core.dll /out:jDroid-X-Desk.exe JDroidXDeskNative.cs
    if exist "jDroid-X-Desk.exe" copy /y "jDroid-X-Desk.exe" "jDroid-X-Desk-Native.exe" >nul
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] Native compilation failed.
        pause
        exit /b 1
    )
)

echo [PASS] Launching jDroid-X-Desk.exe...
start "" "jDroid-X-Desk.exe"
exit /b 0
