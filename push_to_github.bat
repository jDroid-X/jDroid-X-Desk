@echo off
TITLE Sync jDroid-X-Desk to GitHub
ECHO ========================================================
ECHO      Pushing jDroid-X-Desk Repository to GitHub
ECHO ========================================================
ECHO Target: https://github.com/jDroid-X/jDroid-X-Desk.git
ECHO Branch: main
ECHO.
ECHO Staging files...
git add -A
git commit -m "jDroid-X 3-Tier Cloud Sync: GitHub Backend, Colab Relay & Remote Desks"
ECHO.
ECHO Pushing commits...
git push -u origin main
ECHO.
IF %ERRORLEVEL% EQU 0 (
    ECHO ========================================================
    ECHO [SUCCESS] Push Completed! Opening GitHub Repository...
    ECHO ========================================================
    start https://github.com/jDroid-X/jDroid-X-Desk
) ELSE (
    ECHO.
    ECHO ========================================================
    ECHO [NOTE] Retrying push with force option...
    ECHO ========================================================
    git push -u origin main --force
    start https://github.com/jDroid-X/jDroid-X-Desk
)
PAUSE
