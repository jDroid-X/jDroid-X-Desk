@echo off
TITLE jDroid-X Enterprise Certificate Trust & Signer
cd /d "%~dp0"

echo ========================================================
echo    jDroid-X Enterprise Root Certificate Whitelister
echo ========================================================
echo.
echo Installing jDroid-X-Enterprise.cer to Windows Trusted Root
echo and Trusted Publisher stores for complete system whitelisting...
echo.

:: Check for Administrative elevation
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [!] Requesting Administrator elevation to register in System Root store...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c \"\"%~dp0TRUST_ENTERPRISE_CERT_AS_ADMIN.bat\"\"' -Verb RunAs"
    exit /b
)

if exist "%~dp0jDroid-X-Enterprise.cer" (
    certutil -addstore -f "Root" "%~dp0jDroid-X-Enterprise.cer"
    certutil -addstore -f "TrustedPublisher" "%~dp0jDroid-X-Enterprise.cer"
    certutil -user -addstore "TrustedPublisher" "%~dp0jDroid-X-Enterprise.cer"
    echo.
    echo [+] Certificate installed into Machine Root & TrustedPublisher stores!
    echo [+] Microsoft Authenticode status will now evaluate to VALID.
    echo.
    if exist "%~dp0scripts\sign-exe.ps1" (
        echo Signing jDroid-X-Desk.exe with trusted enterprise credentials...
        powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\sign-exe.ps1" -TargetExe "%~dp0jDroid-X-Desk.exe" -Force
    )
) else (
    echo [ERROR] jDroid-X-Enterprise.cer not found at %~dp0jDroid-X-Enterprise.cer
)

pause
