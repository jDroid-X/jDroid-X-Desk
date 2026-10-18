@echo off
:: Strangler Forwarder: Delegates to encapsulated scripts/test.bat
call "%~dp0scripts\test.bat" %*
