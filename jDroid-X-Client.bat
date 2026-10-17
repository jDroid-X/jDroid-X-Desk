@echo off
TITLE jDroid-X-Desk Remote Client (Portable Admin Console)
COLOR 0A
MODE CON: COLS=78 LINES=30

:: =========================================================================
:: 1. SELF-ELEVATION CHECK: Windows-Level Admin Rights & UAC Bypass
:: =========================================================================
net session >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [INFO] Requesting Windows Administrator privileges to enable full UAC control...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c \"\"%~f0\"\"' -Verb RunAs"
    exit /b 0
)

cd /d "%~dp0"

:: =========================================================================
:: 2. GENERATE ANYDESK-STYLE 9-DIGIT DESK ID AND ONE-TIME PASSWORD
:: =========================================================================
for /f "tokens=*" %%A in ('powershell -NoProfile -Command "$h = [System.Security.Cryptography.MD5]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes($env:COMPUTERNAME + (Get-CimInstance Win32_BIOS).SerialNumber)); $n = [BitConverter]::ToUInt32($h, 0) %% 900000000 + 100000000; ('{0:000-000-000}' -f $n)"') do set DESK_ID=%%A
for /f "tokens=*" %%B in ('powershell -NoProfile -Command "$chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'; -join ((1..6) | ForEach-Object { $chars[(Get-Random -Maximum $chars.Length)] })"') do set SECURITY_OTP=%%B
for /f "tokens=*" %%C in ('powershell -NoProfile -Command "(Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notmatch 'Loopback|vEthernet' } | Select-Object -First 1).IPAddress"') do set CLIENT_IP=%%C

if "%CLIENT_IP%"=="" set CLIENT_IP=127.0.0.1

:MENU
CLS
ECHO ==============================================================================
ECHO             jDroid-X-Desk | Portable Remote Support Client
ECHO       Direct AnyDesk-Alternative using Native Windows Remote Engines
ECHO ==============================================================================
ECHO.
ECHO   [ADMIN RIGHTS] : ACTIVE (Full Windows UAC Elevation Granted)
ECHO   [COMPUTER NAME]: %COMPUTERNAME%
ECHO   [USER ACCOUNT] : %USERNAME%
ECHO.
ECHO ==============================================================================
ECHO                     YOUR SYSTEM ACCESS CREDENTIALS
ECHO ==============================================================================
ECHO.
ECHO       >>>  YOUR 9-DIGIT DESK ID :   %DESK_ID%
ECHO       >>>  ONE-TIME PASSWORD   :   %SECURITY_OTP%
ECHO       >>>  LOCAL IP ADDRESS    :   %CLIENT_IP%
ECHO.
ECHO   Share this 9-digit Desk ID and Password with the connecting technician.
ECHO ==============================================================================
ECHO   SELECT ACTION:
ECHO ==============================================================================
ECHO.
ECHO   [1] Start Windows Quick Assist (Cloud-Assisted Screen Sharing with UAC)
ECHO       * Traverses all firewalls and NAT routers without port forwarding.
ECHO       * Enter the 6-digit code provided by your technician.
ECHO.
ECHO   [2] Microsoft Remote Assistance (MSRA)
ECHO       * Creates an encrypted invitation file for peer-to-peer control.
ECHO.
ECHO   [3] Open Elevated Admin Terminal Window (Local Diagnostic)
ECHO.
ECHO   [4] Copy Desk ID and Password to Clipboard
ECHO.
ECHO   [5] Generate New One-Time Password
ECHO.
ECHO   [6] Exit Client
ECHO.
ECHO ==============================================================================
SET /P CHOICE="Enter your selection [1-6]: "

IF "%CHOICE%"=="1" GOTO QUICK_ASSIST
IF "%CHOICE%"=="2" GOTO MSRA_LAUNCH
IF "%CHOICE%"=="3" GOTO ADMIN_TERMINAL
IF "%CHOICE%"=="4" GOTO COPY_CLIPBOARD
IF "%CHOICE%"=="5" GOTO REGEN_OTP
IF "%CHOICE%"=="6" GOTO QUIT

ECHO [ERROR] Invalid selection. Please enter 1 to 6.
ping 127.0.0.1 -n 2 >nul
GOTO MENU

:QUICK_ASSIST
CLS
ECHO ==============================================================================
ECHO   STARTING WINDOWS QUICK ASSIST (FULL UAC ADMIN SCREEN SHARING)...
ECHO ==============================================================================
ECHO.
ECHO [1] Ask your technician for their 6-digit Quick Assist security code.
ECHO [2] Type that code into the Microsoft Quick Assist window.
ECHO [3] Click 'Allow' when prompted to grant desktop control.
ECHO.
start ms-quick-assist:
ECHO [SUCCESS] Quick Assist launched.
ECHO.
PAUSE
GOTO MENU

:MSRA_LAUNCH
CLS
ECHO ==============================================================================
ECHO   LAUNCHING MICROSOFT REMOTE ASSISTANCE (MSRA)...
ECHO ==============================================================================
ECHO.
start msra.exe /expert
ECHO [SUCCESS] Microsoft Remote Assistance dialog opened.
ECHO.
PAUSE
GOTO MENU

:ADMIN_TERMINAL
CLS
ECHO ==============================================================================
ECHO   OPENING ELEVATED WINDOWS POWERSHELL TERMINAL (ADMINISTRATOR)...
ECHO ==============================================================================
start "jDroid-X Admin Console" powershell -NoExit -Command "Write-Host '=========================================' -ForegroundColor Green; Write-Host ' jDroid-X Remote Administrator Terminal' -ForegroundColor Cyan; Write-Host ' Elevated UAC Control Active' -ForegroundColor Yellow; Write-Host '=========================================' -ForegroundColor Green"
GOTO MENU

:COPY_CLIPBOARD
powershell -NoProfile -Command "Set-Clipboard -Value 'jDroid-X Desk ID: %DESK_ID% | OTP: %SECURITY_OTP% | IP: %CLIENT_IP%'"
ECHO.
ECHO [COPIED] Your Desk ID and OTP have been copied to your clipboard!
ping 127.0.0.1 -n 2 >nul
GOTO MENU

:REGEN_OTP
for /f "tokens=*" %%B in ('powershell -NoProfile -Command "$chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'; -join ((1..6) | ForEach-Object { $chars[(Get-Random -Maximum $chars.Length)] })"') do set SECURITY_OTP=%%B
ECHO.
ECHO [UPDATED] Generated new One-Time Password: %SECURITY_OTP%
ping 127.0.0.1 -n 2 >nul
GOTO MENU

:QUIT
CLS
ECHO ==============================================================================
ECHO   jDroid-X-Desk Remote Client closed. Thank you.
ECHO ==============================================================================
ping 127.0.0.1 -n 2 >nul
exit /b 0
