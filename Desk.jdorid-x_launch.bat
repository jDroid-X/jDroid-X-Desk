@echo off
:: Alias wrapper pointing to canonical Desk.jdroid-x_launch.bat
call "%~dp0Desk.jdroid-x_launch.bat" %*
exit /b %ERRORLEVEL%
