@echo off
setlocal
TITLE jDroid-X Portable Technician Client (Admin Forwarder)
cd /d "%~dp0"

:: Strangler Forwarder (Chesterton's Fence) -> Canonical jDroid-X-Client.bat
call "%~dp0jDroid-X-Client.bat" %*
exit /b %ERRORLEVEL%
