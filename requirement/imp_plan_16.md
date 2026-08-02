# [imp_plan_16] Jarvis-Style HUD Sidebar Telemetry & 4G Threshold Warning Alarms

**Date**: 2026-08-02  
**Goal**: Add sci-fi Jarvis HUD widget to bottom left sidebar displaying live streaming telemetry (Download/Upload speed in MB/s, RTT Ping, FPS) and emit automatic side toast popup alarms when speeds drop below 4G standards.

## Summary of Changes
1. **Sidebar Footer (`index.html`)**: Added `.jarvis-hud-card` widget with live Download (MB/s), Upload (MB/s), Ping (ms), and FPS readout.
2. **Sci-Fi Styles (`styles.css`)**: Glowing cyan/emerald borders, pulsating radar dot, JetBrains Mono fonts.
3. **Telemetry & Threshold Loop (`app.js`)**: Real-time MB/s telemetry ticker, 4G baseline auditor (Min 1.0 MB/s Download, Max 100ms Ping), automatic side toast warning popups on degradation.
