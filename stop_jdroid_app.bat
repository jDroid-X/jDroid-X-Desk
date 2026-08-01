@echo off
TITLE Stop jDroid-X-Desk Server
ECHO Stopping jDroid-X-Desk background server on port 12345...

FOR /F "tokens=5" %%P IN ('netstat -aon ^| findstr :12345') DO (
    IF NOT "%%P"=="0" (
        ECHO Killing process PID: %%P
        taskkill /F /PID %%P >nul 2>&1
    )
)

ECHO Server stopped successfully.
PAUSE
