# [imp_plan_12] Windows Batch Installer Line Ending Remediation & Timeout Update

**Date**: 2026-08-02  
**Goal**: Resolve issue where `install_windows_app.bat` exited instantly on fresh laptops due to Unix `LF` line endings breaking the CMD multi-line `IF` statement parser.

## Summary of Changes
1. **Line Ending Conversion**:
   - Converted `install_windows_app.bat`, `push_to_github.bat`, and `stop_jdroid_app.bat` from `LF` to standard Windows `CRLF` line endings.
2. **Timeout Update**:
   - Replaced final `PAUSE` command in `install_windows_app.bat` with `timeout /t 10` for automated 10-second terminal closing.

## Verification Plan
- Verified CRLF match using PowerShell `Get-Content -Raw`.
