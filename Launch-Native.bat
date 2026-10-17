@echo off
setlocal
TITLE jDroid-X-Desk Native Launcher
cd /d "%~dp0"

echo ========================================================
echo   jDroid-X-Desk Pure Native Windows Machine Console
echo ========================================================
echo.

if not exist "jDroid-X-Desk-Native.exe" (
    echo [INFO] Compiling native binary with 64-bit .NET compiler...
    C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /optimize+ /r:System.Windows.Forms.dll,System.Drawing.dll,System.Web.Extensions.dll,System.Core.dll /out:jDroid-X-Desk-Native.exe JDroidXDeskNative.cs
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] Native compilation failed.
        pause
        exit /b 1
    )
)

echo [PASS] Launching jDroid-X-Desk-Native.exe...
start "" "jDroid-X-Desk-Native.exe"
exit /b 0
