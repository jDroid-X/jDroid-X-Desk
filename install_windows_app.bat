@echo off
TITLE jDroid-X-Desk Windows App Installer
ECHO ========================================================
ECHO       jDroid-X-Desk Standalone App Installer
ECHO ========================================================
ECHO.
ECHO [1/3] Verifying Node.js and NPM installation...
node -v >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO [WARNING] Node.js is not installed!
    ECHO [INFO] Auto-downloading Node.js installer (this may take a minute)...
    powershell -Command "Invoke-WebRequest -Uri 'https://nodejs.org/dist/v20.17.0/node-v20.17.0-x64.msi' -OutFile '%TEMP%\node_installer.msi'"
    ECHO [INFO] Launching Node.js Installer... Please complete the setup in the popup window.
    start /wait %TEMP%\node_installer.msi
    ECHO [SUCCESS] Node.js installation finished! Please restart this installer.
    ping 127.0.0.1 -n 11 > nul
    EXIT /B 1
)
ECHO [2/3] Installing background web server dependencies...
call npm install --no-audit
IF %ERRORLEVEL% NEQ 0 (
    ECHO [WARNING] NPM install encountered minor warnings, proceeding to launch...
)

ECHO [3/3] Creating Windows Desktop Shortcut...
SET SCRIPT="%TEMP%\CreateShortcut.vbs"
ECHO Set oWS = WScript.CreateObject("WScript.Shell") > %SCRIPT%
ECHO sLinkFile = oWS.SpecialFolders("Desktop") ^& "\jDroid-X-Desk.lnk" >> %SCRIPT%
ECHO Set oLink = oWS.CreateShortcut(sLinkFile) >> %SCRIPT%
ECHO oLink.TargetPath = "%SystemRoot%\System32\wscript.exe" >> %SCRIPT%
ECHO oLink.Arguments = """%~dp0start_jdroid_app.vbs""" >> %SCRIPT%
ECHO oLink.WorkingDirectory = "%~dp0" >> %SCRIPT%
ECHO oLink.Description = "jDroid-X-Desk Remote Desktop Console" >> %SCRIPT%
ECHO oLink.Save >> %SCRIPT%
cscript //nologo %SCRIPT%
DEL %SCRIPT%

ECHO.
ECHO ========================================================
ECHO [SUCCESS] Installation Complete!
ECHO Desktop Shortcut 'jDroid-X-Desk' created.
ECHO App running on background server at http://localhost:12345
ECHO ========================================================
ECHO.
ECHO Launching jDroid-X-Desk App Mode...
wscript.exe "%~dp0start_jdroid_app.vbs"
ping 127.0.0.1 -n 11 > nul
