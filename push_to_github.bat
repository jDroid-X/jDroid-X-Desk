@echo off
TITLE Sync jDroid-X-Desk to GitHub
ECHO ========================================================
ECHO      Pushing jDroid-X-Desk Repository to GitHub
ECHO ========================================================
ECHO Target: https://github.com/jDroid-X/jDroid-X-Desk.git
ECHO Branch: main
ECHO.
ECHO Pushing commits...
git push -u origin main --force
ECHO.
IF %ERRORLEVEL% EQU 0 (
    ECHO ========================================================
    ECHO [SUCCESS] Push Completed! Opening GitHub Repository...
    ECHO ========================================================
    start https://github.com/jDroid-X/jDroid-X-Desk
) ELSE (
    ECHO.
    ECHO ========================================================
    ECHO [NOTE] If prompted for GitHub login, please complete the login in your browser.
    ECHO Retrying push with master branch...
    ECHO ========================================================
    git push -u origin main:master --force
    start https://github.com/jDroid-X/jDroid-X-Desk
)
PAUSE
