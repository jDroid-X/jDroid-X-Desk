@echo off
setlocal
TITLE jDroid-X Remote Support Client (Instant Share Forwarder)
cd /d "%~dp0"

:: Strangler Forwarder (Chesterton's Fence) -> Canonical jDroid-X-Share.bat
call "%~dp0jDroid-X-Share.bat" %*
exit /b %ERRORLEVEL%
