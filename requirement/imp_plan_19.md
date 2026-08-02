# [imp_plan_19] Telemetry Settings Model Integration & View 12 Config

**Date**: 2026-08-02  
**Goal**: Centralize hardcoded parameters into SystemSettingsModel (View 12) with custom threshold/toast configs, theme-aware telemetry footer (jDroid-X Matrix), and repositioned bottom-left toasts.

## Summary of Changes
1. **OOPS Model (`app.js`)**:
   - Added `SystemSettingsModel` capturing `Alert Thresholds`, `Toast Durations`, and `Dynamic 5G Logic` (which increases bandwidth thresholds by 20% to prevent notification spam on high-speed lines).
2. **Telemetry Footer / View 12 (`index.html` & `app.js`)**:
   - Renamed telemetry footer to "jDroid-X Matrix" containing dynamic theme indicators, live IP, and Tunnel metadata.
   - Wired inputs in Settings View (View 12) to update `SystemSettingsModel` dynamically.
3. **UI / Styling Refinements (`styles.css` & `app.js`)**:
   - Repositioned the toast notification engine to display in the bottom-left corner.
   - Removed brand logo background decoration from the toast notifications to maximize visibility.
   - Ensured footer is light/dark theme compatible.

## Verification Plan
- Tested changes by updating thresholds in View 12 settings panel and verified toast behavior.
