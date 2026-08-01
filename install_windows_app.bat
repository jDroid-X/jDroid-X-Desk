@echo off
TITLE jDroid-X-Desk Windows App Installer
ECHO ========================================================
ECHO       jDroid-X-Desk Standalone App Installer
ECHO ========================================================
ECHO.
ECHO [1/3] Verifying Node.js & NPM installation...
node -v >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO [ERROR] Node.js is not installed! Please install Node.js from https://nodejs.org
    PAUSE
    EXIT /B 1
)

ECHO [2/3] Installing background web server dependencies...
call npm install --no-audit >nul 2>&1

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
PAUSE
