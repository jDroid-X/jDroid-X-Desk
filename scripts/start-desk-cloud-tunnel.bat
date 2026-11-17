@echo off
:: Legacy wrapper forwarded to canonical jdroid-x-desk-tunnel.bat (Prime Directive 1: Chesterton's Fence)
call "%~dp0jdroid-x-desk-tunnel.bat" %*
exit /b %ERRORLEVEL%
