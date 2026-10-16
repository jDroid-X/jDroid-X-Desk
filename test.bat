@echo off
TITLE jDroid-X-Desk Stage 5 Verification Suite
node test.js
IF %ERRORLEVEL% EQU 0 (
    ECHO [SUCCESS] All verification tests passed.
) ELSE (
    ECHO [ERROR] Verification suite detected failures.
)
