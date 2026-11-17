# Task 36: UltraViewer Deep REA Integration, Gap Analysis & Enterprise Parity Specification

## 1. Executive Directive & Scope
Following Operator JEET's directive, this task incorporates the Reverse Engineering Analysis (REA) of **UltraViewer 6.6.124** (`UltraViewer_setup_6.6.124_en.exe`) to update and close all technical and architectural gaps in `jDroid-X-Desk`.

## 2. Reverse-Engineered UltraViewer Subsystem Breakdown & Parity Matrix

| Subsystem | UltraViewer 6.6 Implementation | jDroid-X Existing Status | Gap Resolution & Code Action |
| :--- | :--- | :--- | :--- |
| **Session Elevation** | `UltraViewer_Service.exe` + `WTSQueryUserToken` + `tscon.exe` session switching for UAC & Winlogon | User-mode C# Process with native GDI | Integrate Windows Service Session 0 monitor to bridge UAC consent desktop via token duplication. |
| **Screen Codec** | `UVCPlus.dll`: 32x32 Tile Differentials + SIMD TurboJPEG / Discrete Cosine Transform | Full frame JPEG + Base64 websocket | Implement Tile Differential Caching (`clsStoredFrame` equivalent in `ScreenCaptureEngine.cs`). Send only dirty blocks. |
| **Authentication** | Ephemeral 4-digit `RandomPassword` + Registry Salted `FixedPassword` | 6-digit PIN / Permission consent | Support both Ephemeral 4/6-digit random password and Unattended Fixed Password mode. |
| **Network Path** | Dual-channel: `VUDPClient` P2P STUN Hole Punching with automatic `VTCPClient` Relay fallback | WebRTC P2P + WebSocket Relay fallback | Align signaling protocol to attempt direct UDP P2P punch prior to tunnel fallback. |
| **Privacy Curtain** | `FullScreenLocker_Lock` & `FullScreenLocker_UnLock` (Blackout target monitor) | Standard Viewport | Add Host Screen Curtain toggle in floating HUD to disable target monitor output during maintenance. |
| **Audio Loopback** | `NAudio.dll`: WASAPI Loopback capture + WaveIn mic stream | WebSocket PCM chunk streaming | Synchronize audio chunks with video frames in embedded relay server. |
| **Power Management** | `RemoteControl.VWakeUp`: Subnet UDP Broadcast Wake-on-LAN | Planned | Expose WOL Magic Packet generator on port 9 in `LanDiscoveryEngine.cs`. |

## 3. Verified Artifacts
- Canonical Architecture: `docs/JDROID_X_BRD_ULTRAVIEWER_PARITY_MASTER.md`
- Screen Action Contract: `requirement/unified-screen-contract.txt`
