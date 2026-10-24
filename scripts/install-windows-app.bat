@echo off
TITLE jDroid-X-Desk Windows Universal App Installer
setlocal enabledelayedexpansion

:: Resolve project root directory
set "SOURCE_DIR=%~dp0.."
if exist "%~dp0jDroid-X-Desk.exe" (
    set "SOURCE_DIR=%~dp0"
)
pushd "%SOURCE_DIR%"
set "SOURCE_DIR=%CD%"
popd

set "INSTALL_DIR=%LOCALAPPDATA%\Programs\jDroid-X-Desk"

ECHO ========================================================
ECHO    jDroid-X-Desk Universal Windows Installer
ECHO    Target: Standard User Application Directory
ECHO    Folder: %INSTALL_DIR%
ECHO ========================================================
ECHO.

:: [1/7] Verify / Compile executable if needed
if not exist "%SOURCE_DIR%\jDroid-X-Desk.exe" (
    ECHO [!] jDroid-X-Desk.exe not found in source. Compiling native solution...
    call "%SOURCE_DIR%\compile_native.bat"
    if !ERRORLEVEL! NEQ 0 (
        ECHO [ERROR] Compilation failed. Aborting installation.
        pause
        exit /b 1
    )
)

:: [2/7] Create standard user installation folder structure
ECHO [1/6] Creating standard application directories...
taskkill /f /im jDroid-X-Desk.exe >nul 2>&1
timeout /t 1 /nobreak >nul 2>&1
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%INSTALL_DIR%\assets" mkdir "%INSTALL_DIR%\assets"
ECHO [+] Target directory verified: %INSTALL_DIR%

:: Ensure binary is signed before deploying
if exist "%SOURCE_DIR%\scripts\sign-exe.ps1" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%SOURCE_DIR%\scripts\sign-exe.ps1" -TargetExe "%SOURCE_DIR%\jDroid-X-Desk.exe" >nul 2>&1
)

:: [3/7] Copy application binaries, configs, and assets
ECHO [2/6] Deploying application files to %INSTALL_DIR%...
copy /Y "%SOURCE_DIR%\jDroid-X-Desk.exe" "%INSTALL_DIR%\" >nul
if exist "%SOURCE_DIR%\jDroid-X-Desk.exe.config" copy /Y "%SOURCE_DIR%\jDroid-X-Desk.exe.config" "%INSTALL_DIR%\" >nul
if exist "%SOURCE_DIR%\app.manifest" copy /Y "%SOURCE_DIR%\app.manifest" "%INSTALL_DIR%\" >nul
if exist "%SOURCE_DIR%\jDroid-X-Enterprise.cer" copy /Y "%SOURCE_DIR%\jDroid-X-Enterprise.cer" "%INSTALL_DIR%\" >nul
if exist "%SOURCE_DIR%\version.json" copy /Y "%SOURCE_DIR%\version.json" "%INSTALL_DIR%\" >nul
if exist "%SOURCE_DIR%\colab_relay_server.py" copy /Y "%SOURCE_DIR%\colab_relay_server.py" "%INSTALL_DIR%\" >nul
if exist "%SOURCE_DIR%\assets" xcopy /E /I /Y "%SOURCE_DIR%\assets" "%INSTALL_DIR%\assets" >nul 2>&1

:: Preserve existing database.json if already present, or initialize fresh
if not exist "%INSTALL_DIR%\database.json" (
    if exist "%SOURCE_DIR%\database.json" (
        copy /Y "%SOURCE_DIR%\database.json" "%INSTALL_DIR%\" >nul
    )
)
ECHO [+] Core application files installed successfully.

:: [4/7] Certificate Authority Whitelisting
ECHO [3/6] Installing Authenticode certificate to Trusted Publishers...
if exist "%INSTALL_DIR%\jDroid-X-Enterprise.cer" (
    certutil -user -addstore TrustedPublisher "%INSTALL_DIR%\jDroid-X-Enterprise.cer" >nul 2>&1
    ECHO [+] Certificate authority registered in Trusted Publishers.
)

:: [5/7] Windows SmartScreen MOTW Unblock & Windows Defender
ECHO [4/6] Unblocking Mark-of-the-Web (MOTW) and setting Defender exclusions...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%INSTALL_DIR%' -Recurse | Unblock-File -ErrorAction SilentlyContinue"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Add-MpPreference -ExclusionPath '%INSTALL_DIR%' -ExclusionProcess 'jDroid-X-Desk.exe' -ErrorAction SilentlyContinue" >nul 2>&1
ECHO [+] Windows SmartScreen flags cleared.

:: [6/7] Windows Firewall Rules for P2P Screen Sharing
netsh advfirewall firewall add rule name="jDroid-X-Desk TCP" dir=in action=allow protocol=TCP localport=7700,8088,12345 enable=yes >nul 2>&1
netsh advfirewall firewall add rule name="jDroid-X-Desk UDP" dir=in action=allow protocol=UDP localport=7707,8089,21116 enable=yes >nul 2>&1
ECHO [+] Windows Firewall rules registered.

:: [7/7] Create Desktop & Start Menu Shortcuts, Register Uninstaller
ECHO [5/6] Creating Desktop and Start Menu shortcuts...
SET SCRIPT="%TEMP%\CreateShortcut_%RANDOM%.vbs"
(
    echo Set oWS = WScript.CreateObject("WScript.Shell"^)
    echo sDesktop = oWS.SpecialFolders("Desktop"^) ^& "\jDroid-X-Desk.lnk"
    echo Set oLink1 = oWS.CreateShortcut(sDesktop^)
    echo oLink1.TargetPath = "%INSTALL_DIR%\jDroid-X-Desk.exe"
    echo oLink1.WorkingDirectory = "%INSTALL_DIR%"
    echo oLink1.Description = "jDroid-X-Desk Enterprise Remote Desktop Console"
    if exist "%INSTALL_DIR%\assets\app.ico" (
        echo oLink1.IconLocation = "%INSTALL_DIR%\assets\app.ico,0"
    )
    echo oLink1.Save
    echo sStartMenu = oWS.SpecialFolders("StartMenu"^) ^& "\Programs\jDroid-X-Desk.lnk"
    echo Set oLink2 = oWS.CreateShortcut(sStartMenu^)
    echo oLink2.TargetPath = "%INSTALL_DIR%\jDroid-X-Desk.exe"
    echo oLink2.WorkingDirectory = "%INSTALL_DIR%"
    echo oLink2.Description = "jDroid-X-Desk Enterprise Remote Desktop Console"
    if exist "%INSTALL_DIR%\assets\app.ico" (
        echo oLink2.IconLocation = "%INSTALL_DIR%\assets\app.ico,0"
    )
    echo oLink2.Save
) > %SCRIPT%
cscript //nologo %SCRIPT%
del %SCRIPT% >nul 2>&1
ECHO [+] Desktop shortcut 'jDroid-X-Desk' created.
ECHO [+] Start Menu shortcut 'jDroid-X-Desk' created.

:: Register in Windows Installed Apps Registry (HKCU)
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk" /v "DisplayName" /t REG_SZ /d "jDroid-X-Desk Remote Workstation" /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk" /v "DisplayVersion" /t REG_SZ /d "2.5.0" /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk" /v "Publisher" /t REG_SZ /d "jDroid-X Studio" /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk" /v "InstallLocation" /t REG_SZ /d "%INSTALL_DIR%" /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk" /v "DisplayIcon" /t REG_SZ /d "%INSTALL_DIR%\jDroid-X-Desk.exe,0" /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk" /v "UninstallString" /t REG_SZ /d "\"%INSTALL_DIR%\uninstall.bat\"" /f >nul 2>&1

:: Create uninstaller script
(
    echo @echo off
    echo title Uninstalling jDroid-X-Desk...
    echo taskkill /f /im jDroid-X-Desk.exe ^>nul 2^>^&1
    echo timeout /t 1 /nobreak ^>nul
    echo del /f /q "%%USERPROFILE%%\Desktop\jDroid-X-Desk.lnk" ^>nul 2^>^&1
    echo del /f /q "%%APPDATA%%\Microsoft\Windows\Start Menu\Programs\jDroid-X-Desk.lnk" ^>nul 2^>^&1
    echo reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\jDroid-X-Desk" /f ^>nul 2^>^&1
    echo echo jDroid-X-Desk has been successfully removed from your computer.
    echo pause
    echo ^(goto^) 2^>nul ^& rmdir /s /q "%INSTALL_DIR%"
) > "%INSTALL_DIR%\uninstall.bat"

ECHO.
ECHO ========================================================
ECHO [SUCCESS] Installation and Registration Complete!
ECHO Location: %INSTALL_DIR%
ECHO Zero SmartScreen Roadblocks. Desktop Icon Active.
ECHO Launching jDroid-X-Desk from installed standard folder...
ECHO ========================================================
start "" "%INSTALL_DIR%\jDroid-X-Desk.exe"
exit /b 0
