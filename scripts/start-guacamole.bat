@echo off
TITLE jDroid-X Engine Launcher: Apache Guacamole
echo ========================================================
echo   jDroid-X Engine: Apache Guacamole + FreeRDP
echo ========================================================
echo.

echo [INFO] Checking Docker daemon for guacamole-server (guacd)...
docker ps 2>NUL | find /I "guacamole" >NUL
if "%ERRORLEVEL%"=="0" (
    echo [✓ ACTIVE] Apache Guacamole container is running on port 8080/4822!
    start http://localhost:8080/guacamole
    exit /b 0
)

echo [INFO] Attempting to start Guacamole Docker stack or inspect local source...
if exist "%~dp0..\docker-compose.guacamole.yml" (
    echo [LAUNCH] Starting Guacamole via docker-compose...
    docker-compose -f "%~dp0..\docker-compose.guacamole.yml" up -d
    exit /b 0
)

echo [INFO] Guacamole container not active. Opening Guacamole Server source directory...
if exist "%~dp0..\jDroid-X-Desk-Server\Github\guacamole-server-master" (
    start explorer.exe "%~dp0..\jDroid-X-Desk-Server\Github\guacamole-server-master"
)
exit /b 0
