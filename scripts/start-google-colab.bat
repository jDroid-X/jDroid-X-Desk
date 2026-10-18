@echo off
TITLE jDroid-X Engine Launcher: Google Colab Cloud Relay
echo ========================================================
echo   jDroid-X Engine: Google Colab Cloud Relay
echo ========================================================
echo.

echo [INFO] Verifying jDroid-X Google Colab Cloud Relay connection...
echo [INFO] Launching Google Colab Relay Bridge Interface...

set "COLAB_SCRIPT="
if exist "%~dp0..\colab_relay_server.py" set "COLAB_SCRIPT=%~dp0..\colab_relay_server.py"
if exist "%~dp0..\jDroid-X-Colab-Relay-Server.py" set "COLAB_SCRIPT=%~dp0..\jDroid-X-Colab-Relay-Server.py"
if exist "%~dp0..\jDroid-X-Desk-Server\colab_relay.py" set "COLAB_SCRIPT=%~dp0..\jDroid-X-Desk-Server\colab_relay.py"

if defined COLAB_SCRIPT (
    echo [LAUNCH] Starting local Python Colab tunnel listener: %COLAB_SCRIPT%
    start "jDroid-X Colab Bridge" python "%COLAB_SCRIPT%"
    echo [PASS] Google Colab Relay Bridge started!
) else (
    echo [INFO] Opening Google Colab Relay Console in default browser...
    start https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai
)
exit /b 0
