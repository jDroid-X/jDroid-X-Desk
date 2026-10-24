@echo off
setlocal
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Error: .NET Framework csc.exe compiler not found at %CSC%
    exit /b 1
)

echo [1/2] Generating C# compiler arguments file...
(
    echo /target:winexe
    echo /platform:anycpu
    echo /optimize+
    echo /debug-
    echo /win32manifest:app.manifest
    echo /win32icon:assets\app.ico
    echo /r:System.dll,System.Windows.Forms.dll,System.Drawing.dll,System.Web.Extensions.dll,System.Net.dll,System.Data.dll,System.Xml.dll,System.Core.dll
    echo /res:database.json,database.json
    echo /res:version.json,version.json
    echo /res:colab_relay_server.py,colab_relay_server.py
    echo /res:jDroid-X-Enterprise.cer,jDroid_X_Enterprise.cer
    echo /res:jDroid-X-Desk.exe.config,jDroid_X_Desk.exe.config
    echo /res:app.manifest,app.manifest
    echo /res:assets\app.ico,assets.app.ico
    echo /res:assets\jdroidxlogo_trans.png,assets.jdroidxlogo_trans.png
    echo /res:assets\jdroid-x-desk-presentation.png,assets.jdroid_x_desk_presentation.png
    echo /res:scripts\configure-smart-app-control.bat,configure_smart_app_control.bat
    echo /res:scripts\trust-root-cert.bat,trust_root_cert.bat
    echo /out:jDroid-X-Desk.exe
) > native_compile.rsp

for /R "Native" %%F in (*.cs) do (
    echo %%F >> native_compile.rsp
)

echo [2/2] Compiling jDroid-X-Desk Modular Native Solution...
"%CSC%" @native_compile.rsp
set RESULT=%ERRORLEVEL%

del native_compile.rsp 2>nul

if %RESULT% EQU 0 (
    echo Compilation Succeeded: jDroid-X-Desk.exe
    if exist "%~dp0scripts\sign-exe.ps1" (
        echo [3/3] Checking Microsoft Authenticode & Code Integrity Whitelist...
        powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\sign-exe.ps1" -TargetExe "%~dp0jDroid-X-Desk.exe"
    )
) else (
    echo Compilation Failed with error code %RESULT%
)
exit /b %RESULT%
