@echo off
:: Strangler Forwarder: Delegates to encapsulated scripts/stop-jdroid-app.bat
call "%~dp0scripts\stop-jdroid-app.bat" %*
