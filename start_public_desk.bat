@echo off
setlocal
TITLE jDroid-X Public Cloud Ingress Launcher (desk.jdroid-x.com)
cd /d "%~dp0"
call "scripts\run-services.bat"
echo.
echo ========================================================
echo   jDroid-X Public Cloud Ingress Initiated!
echo   Public URL: https://desk.jdroid-x.com
echo   Checking status in 5 seconds...
echo ========================================================
timeout /t 5 /nobreak >nul 2>&1
curl.exe -I --max-time 10 https://desk.jdroid-x.com
pause
