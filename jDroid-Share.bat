@echo off
TITLE jDroid-X Remote Support Client (Instant Share)
COLOR 0A
MODE CON: COLS=75 LINES=28

:MENU
CLS
ECHO ===========================================================================
ECHO            jDroid-X-Desk | Instant Remote Screen Share Client
ECHO            Lightweight Native Windows Client (Zero Dependencies)
ECHO ===========================================================================
ECHO.
ECHO   [SYSTEM IDENTITY]
ECHO   Computer Name : %COMPUTERNAME%
ECHO   User Account  : %USERNAME%
ECHO   Windows Build : 
powershell -NoProfile -Command "(Get-CimInstance Win32_OperatingSystem).Caption" 2>nul
ECHO.
ECHO ===========================================================================
ECHO   SELECT REMOTE DESKTOP SHARING OPTION:
ECHO ===========================================================================
ECHO.
ECHO   [1] Windows Quick Assist (Recommended - 1-Click Direct Cloud Relay)
ECHO       * Traverses all firewalls and routers with zero port forwarding.
ECHO       * Enter the 6-digit code provided by your technician.
ECHO.
ECHO   [2] Microsoft Remote Assistance (MSRA Invitation)
ECHO       * Generates encrypted one-time remote incident invitation.
ECHO.
ECHO   [3] Display Network Identity & Local IPs
ECHO       * Shows your local and network IP addresses to share with technician.
ECHO.
ECHO   [4] Launch jDroid-X Web Console (If Host / Technician Node)
ECHO.
ECHO   [5] Exit
ECHO.
ECHO ===========================================================================
SET /P CHOICE="Enter your choice [1-5]: "

IF "%CHOICE%"=="1" GOTO QUICK_ASSIST
IF "%CHOICE%"=="2" GOTO MSRA_LAUNCH
IF "%CHOICE%"=="3" GOTO SHOW_IP
IF "%CHOICE%"=="4" GOTO LAUNCH_CONSOLE
IF "%CHOICE%"=="5" GOTO QUIT

ECHO [ERROR] Invalid selection. Please choose 1, 2, 3, 4, or 5.
PAUSE
GOTO MENU

:QUICK_ASSIST
CLS
ECHO ===========================================================================
ECHO   LAUNCHING WINDOWS QUICK ASSIST...
ECHO ===========================================================================
ECHO.
ECHO [INFO] Opening native Windows Quick Assist tunnel...
ECHO [STEP 1] Ask your technician for the 6-digit security code.
ECHO [STEP 2] Enter the code into the Quick Assist window.
ECHO [STEP 3] Click 'Allow' to share your screen securely.
ECHO.
start ms-quick-assist:
ECHO [SUCCESS] Quick Assist process triggered.
ECHO.
PAUSE
GOTO MENU

:MSRA_LAUNCH
CLS
ECHO ===========================================================================
ECHO   LAUNCHING MICROSOFT REMOTE ASSISTANCE (MSRA)...
ECHO ===========================================================================
ECHO.
ECHO [INFO] Starting msra.exe...
start msra.exe /expert
ECHO [SUCCESS] Remote Assistance opened.
ECHO.
PAUSE
GOTO MENU

:SHOW_IP
CLS
ECHO ===========================================================================
ECHO   YOUR NETWORK IDENTITY & IP ADDRESSES
ECHO ===========================================================================
ECHO.
powershell -NoProfile -Command "Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notmatch 'Loopback' } | Select-Object InterfaceAlias, IPAddress | Format-Table -AutoSize"
ECHO.
PAUSE
GOTO MENU

:LAUNCH_CONSOLE
CLS
ECHO [INFO] Opening jDroid-X-Desk Web Portal...
start http://localhost:12345
GOTO MENU

:QUIT
CLS
ECHO Thank you for using jDroid-X-Desk Remote Sharing.
TIMEOUT /T 2 >nul
EXIT /B 0
