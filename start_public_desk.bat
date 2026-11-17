@echo off
:: Legacy Public Ingress Launcher forwarded to canonical Desk.jdroid-x_launch.bat (Chesterton's Fence)
call "%~dp0Desk.jdroid-x_launch.bat" %*
exit /b %ERRORLEVEL%
