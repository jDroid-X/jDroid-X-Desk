@echo off
TITLE jDroid-X - Smart App Control Developer Mode Configurator
cd /d "%~dp0.."

echo ========================================================
echo    jDroid-X Smart App Control (SAC) Configurator
echo ========================================================
echo.
echo Windows 11 Smart App Control (SAC) blocks all freshly
echo compiled local binaries (Event ID 3077/3033) unless SAC
echo is set to Off (0) or Evaluation Mode (2).
echo.

:: Check for Administrative elevation
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [!] Administrator elevation required to update Kernel CI Policy.
    echo [!] Requesting UAC elevation...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c \"\"%~dp0configure-smart-app-control.bat\"\"' -Verb RunAs"
    exit /b
)

echo [+] Running with Administrator privileges.
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$PolicyPath = 'HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy';" ^
    "if (-not (Test-Path $PolicyPath)) { New-Item -Path $PolicyPath -Force | Out-Null };" ^
    "Set-ItemProperty -Path $PolicyPath -Name 'VerifiedAndReputablePolicyState' -Value 0 -Type DWord -Force;" ^
    "Write-Host '[SUCCESS] Smart App Control policy set to Off (0).' -ForegroundColor Green;" ^
    "if (Get-Command 'citool.exe' -ErrorAction SilentlyContinue) {" ^
    "    & citool.exe -r;" ^
    "    Write-Host '[SUCCESS] Code Integrity refreshed via CiTool.' -ForegroundColor Green;" ^
    "} else {" ^
    "    Write-Host '[NOTICE] CiTool not found, restart may be required for full effect.' -ForegroundColor Yellow;" ^
    "}"

echo.
echo ========================================================
echo [+] Opening Windows Security to verify Smart App Control status...
start windowsdefender://appbrowser
echo.
echo All locally compiled binaries (jDroid-X-Desk.exe) can now run
echo without Windows Code Integrity or Device Guard blocking!
echo ========================================================
echo.
pause
