# jdroid-x-desk - jDroid-X-MindMapView Audit Report

> **Generated:** 2026-10-22 13:47:33  
> **App Slug:** `jdroid-x-desk` | **Version:** `v2.0.0`  
> **Health Score:** `88.5%` (Reflects AMBER God-Node & Credential warnings)  
> **Closed-Loop Convergence:** `75.0%` | **Orphan DOM Elements:** `75`  
> **Engine:** AST-Authoritative Event Resolver (Enterprise v3.0)

---

## 🏛️ Lens 1: Concept Tree (The "Why")

Full-stack application analyzed via AST-Authoritative Event Resolver.

### Primary Feature Modules:
- **Master Application Core & Gateway**: Operational console for jdroid-x-desk with verified AST event routing.
  - ✓ Entry: index.html
  - ✓ Total Monitored Files: 3618
  - ✓ Health Heartbeat Monitor
- **Screen View Affordances & Interactivity**: User screen containing 395 verified DOM elements (225 bound, 75 orphans).
  - ✓ Verified DOM Elements: 395
  - ✓ Theme Mode Selector
  - ✓ Responsive Grid Layout
- **Dynamic OOPS MVC Architecture**: Structural layers featuring 8 domain classes verified in code.
  - ✓ Classes: LocalSystemModel, SystemSettingsModel, SystemTraceabilityLogModel
  - ✓ DbContext State Contract
  - ✓ Call-by-Reference Flow
- **Dynamic Settings & Valuebase**: Central runtime configuration managing 8 persistent keys.
  - ✓ Active Keys: 8
  - ✓ Runtime Port Binding
  - ✓ Zero Hardcoded Values Protocol

---

## 🗄️ Lens 2: MS Access OOPS MVC Studio (The "How")

### Truthful AST Entity Cards:
| Card Type | Title | Items Count | Key Affordances |
| :--- | :--- | :---: | :--- |
| **VIEW** | Screen: View (HTML/UI) | 5 | `#toggle-pass-visibility`, `#launch-canvas-btn`, `#ad-proxy-port` |
| **CONTROLLER** | Controller (Logic & Handlers) | 5 | `renderLiveMap()`, `unlockConsole()`, `then()` |
| **SETTINGS** | Settings: Dynamic Valuebase | 4 | `jdx_fb_api_key`, `jdx_fb_project_id`, `jdx_fb_db_url` |
| **MODEL** | Model (Contracts & State) | 3 | `SystemTraceabilityLogModel`, `LocalSystemModel`, `SystemSettingsModel` |
| **DATABASE** | Database & Persistence | 2 | `database.json / RTDB`, `localStorage Client Cache` |

### AST-Verified Connectors (Real Call-Sites):
- `card_view::v_app-preloader` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_SystemSettingsModel`
- `card_view::v_login-gate-overlay` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_login-error-msg` ──[onClick (event)]──> `card_controller::c_bindLoginGate`
- `card_view::v_login-email-input` ──[onInput (event)]──> `card_controller::c_catch`
- `card_controller::c_catch` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_login-password-input` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_toggle-pass-visibility` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_toggle-pass-icon` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_login-submit-btn` ──[onClick (event)]──> `card_controller::c_bindLoginGate`
- `card_view::v_login-bypass-btn` ──[onClick (event)]──> `card_controller::c_bindLoginGate`
- `card_view::v_toast-container` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_sidebar-drawer` ──[onClick (event)]──> `card_controller::c_bindNavigation`
- `card_view::v_close-drawer-btn` ──[onClick (event)]──> `card_controller::c_bindNavigation`
- `card_view::v_nav-logout-btn` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_nav-exit-app` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_logged-in-user-email` ──[onAccess (event)]──> `card_controller::c_unlockConsole`
- `card_view::v_session-online-dot` ──[onAccess (event)]──> `card_controller::c_unlockConsole`
- `card_view::v_hud-conn-type` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_SystemSettingsModel`
- `card_view::v_hud-ip-address` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_SystemSettingsModel`
- `card_view::v_hud-dl-speed` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_hud-ul-speed` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_hud-ping-ms` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_hud-stream-fps` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_hud-4g-status` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_toggle-drawer-btn` ──[onClick (event)]──> `card_controller::c_bindNavigation`
- `card_view::v_view-title` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_web-gateway-status-text` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_web-header-engine-badge` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_web-header-engine-name` ──[onAccess (event)]──> `card_controller::c_updateActiveEngineUI`
- `card_view::v_web-header-engine-status` ──[onAccess (event)]──> `card_controller::c_probeEngineStatus`
- `card_view::v_web-header-run-script-btn` ──[onClick (event)]──> `card_controller::c_bindEvents`
- `card_view::v_global-theme-selector` ──[onAccess (event)]──> `card_controller::c_bindThemeToggle`
- `card_view::v_theme-toggle-btn` ──[onAccess (event)]──> `card_controller::c_bindThemeToggle`
- `card_view::v_quick-disconnect-btn` ──[onClick (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_view::v_content-area` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_console-active-engine-title` ──[onAccess (event)]──> `card_controller::c_updateActiveEngineUI`
- `card_view::v_console-engine-probe-badge` ──[onAccess (event)]──> `card_controller::c_probeEngineStatus`
- `card_view::v_console-engine-probe-text` ──[onAccess (event)]──> `card_controller::c_probeEngineStatus`
- `card_view::v_console-run-script-btn` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_console-switch-engine-btn` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-download-share-bat` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_gateway-pill-status` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_gateway-url-display` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_local-device-id` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_copy-id-btn` ──[onAccess (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_view::v_btn-open-invite-modal` ──[onAccess (event)]──> `card_controller::c_catch`
- `card_view::v_local-node-desc` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_local-device-pass` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_session-status-text` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_target-id-input` ──[onInput (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_view::v_btn-clear-target-id` ──[onInput (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_view::v_target-pass-input` ──[onInput (event)]──> `card_controller::c_if`
- `card_view::v_connect-session-btn` ──[onAccess (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_view::v_launch-canvas-btn` ──[onClick (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_controller::c_bindRemoteConnectLogic` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_btn-native-quick-assist` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-native-terminal` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-native-msra` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_discovered-desks-count` ──[onAccess (event)]──> `card_controller::c_render`
- `card_view::v_btn-scan-lan-desks` ──[onClick (event)]──> `card_controller::c_init`
- `card_view::v_discovered-desks-container` ──[onAccess (event)]──> `card_controller::c_render`
- `card_view::v_remote-desktop-viewport-box` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_active-target-id-label` ──[onAccess (event)]──> `card_controller::c_catch`
- `card_view::v_stream-hud-specs` ──[onAccess (event)]──> `card_controller::c_startRemoteStreamLoop`
- `card_view::v_stream-quality-select` ──[onAccess (event)]──> `card_controller::c_bindDeepDiveEvents`
- `card_view::v_btn-toggle-clipboard` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-toggle-audio` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-webrtc-share-screen` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-flip-viewport` ──[onAccess (event)]──> `card_controller::c_catch`
- `card_view::v_ctrl-alt-del-btn` ──[onClick (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_controller::c_bindRemoteConnectLogic` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_toggle-file-btn` ──[onClick (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_controller::c_bindRemoteConnectLogic` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_disconnect-session-btn` ──[onAccess (event)]──> `card_controller::c_bindRemoteConnectLogic`
- `card_view::v_viewport-flipper-card` ──[onAccess (event)]──> `card_controller::c_catch`
- `card_view::v_remote-stream-canvas` ──[onAccess (event)]──> `card_controller::c_startRemoteStreamLoop`
- `card_view::v_live-screen-video` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_virtual-screen-placeholder` ──[onAccess (event)]──> `card_controller::c_startRemoteStreamLoop`
- `card_view::v_virtual-screen-log` ──[onAccess (event)]──> `card_controller::c_injectVirtualInput`
- `card_controller::c_injectVirtualInput` ──[mutatesState (data)]──> `card_model::m_SystemTraceabilityLogModel`
- `card_view::v_btn-flip-back-screen` ──[onAccess (event)]──> `card_controller::c_catch`
- `card_view::v_remote-terminal-output` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_terminal-log-content` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_terminal-cmd-input` ──[onInput (event)]──> `card_controller::c_if`
- `card_view::v_terminal-send-btn` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_system-monitor-content` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_SystemTraceabilityLogModel`
- `card_view::v_fleet-filter-all` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_fleet-group-filter` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_btn-fleet-manage-groups` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_btn-open-add-device` ──[onClick (event)]──> `card_controller::c_catch`
- `card_view::v_device-list-ul` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_btn-test-mesh-route` ──[onClick (event)]──> `card_controller::c_renderLiveMap`
- `card_view::v_mesh-nodes-display` ──[onAccess (event)]──> `card_controller::c_renderLiveMap`
- `card_view::v_mesh-ping-result` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_history-search-input` ──[onInput (event)]──> `card_controller::c_if`
- `card_view::v_btn-generate-audit-report` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_session-history-tbody` ──[onAccess (event)]──> `card_controller::c_renderSessionHistory`
- `card_view::v_btn-add-group` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_groups-list` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_btn-open-add-contact` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_contacts-list-grid` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_btn-add-safe-zone` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_safe-zones-list` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_btn-emergency-kill` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-open-add-guardian` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_guardians-list-container` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_notif-filter-all` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-mark-all-read` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_notifications-list-container` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_btn-run-self-diagnostic` ──[onClick (event)]──> `card_controller::c_renderDeviceHealth`
- `card_view::v_health-cpu-val` ──[onAccess (event)]──> `card_controller::c_renderDeviceHealth`
- `card_view::v_health-cpu-model` ──[onAccess (event)]──> `card_controller::c_renderDeviceHealth`
- `card_view::v_health-mem-val` ──[onAccess (event)]──> `card_controller::c_renderDeviceHealth`
- `card_view::v_health-mem-sub` ──[onAccess (event)]──> `card_controller::c_renderDeviceHealth`
- `card_view::v_health-uptime-val` ──[onAccess (event)]──> `card_controller::c_renderDeviceHealth`
- `card_view::v_health-platform-sub` ──[onAccess (event)]──> `card_controller::c_renderDeviceHealth`
- `card_view::v_health-network-nics` ──[onAccess (event)]──> `card_controller::c_renderDeviceHealth`
- `card_view::v_profile-operator-name` ──[onAccess (event)]──> `card_controller::c_renderUserProfile`
- `card_controller::c_renderUserProfile` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_profile-operator-org` ──[onAccess (event)]──> `card_controller::c_renderUserProfile`
- `card_controller::c_renderUserProfile` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_profile-operator-role` ──[onAccess (event)]──> `card_controller::c_renderUserProfile`
- `card_controller::c_renderUserProfile` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_profile-console-id` ──[onAccess (event)]──> `card_controller::c_renderUserProfile`
- `card_view::v_btn-save-operator-profile` ──[onAccess (event)]──> `card_controller::c_renderUserProfile`
- `card_controller::c_renderUserProfile` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_btn-save-all-settings` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_ad-settings-search` ──[onInput (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-lang` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-theme` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-hide-taskbar` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-req-comment` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-open-addrbook` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-accept-chat` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-popup-invite` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-sound-conn` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-transmit-hotkeys` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-ss-path-display` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-chat-path-display` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-online-vis` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-set-allow-telemetry` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-conn-direct` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-conn-port` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-conn-lan-discovery` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-conn-discovery-port` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-proxy-type` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-proxy-addr` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-proxy-port` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_custom-domain-url` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-colab-relay-enabled` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_colab-relay-url` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_btn-test-colab-relay` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_ad-wol-clients-list` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-wol-add` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-wol-rem` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-ad-set-password` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-ad-clear-tokens` ──[onClick (event)]──> `card_controller::c_forEach`
- `card_view::v_ad-acl-enabled` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-acl-list` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-acl-add` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-acl-rem` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_ad-auto-disconnect-mins` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_btn-apply-timeout` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-add-profile` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-ad-prof-set-password` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_ad-priv-frame-enabled` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-priv-frame-color` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_ad-priv-frame-opacity` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_ad-priv-frame-width` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-apply-frame` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_ad-disp-follow-cursor` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-disp-follow-focus` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-disp-show-size` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-disp-edge-scroll` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_SystemSettingsModel`
- `card_view::v_ad-rec-incoming` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-rec-outgoing` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_ad-sess-rec-path-display` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_btn-install-printer` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_ad-acc-email` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_SystemSettingsModel`
- `card_view::v_ad-acc-pass` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_btn-ad-account-forgot` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-ad-account-login` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-ad-account-register` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-ad-account-sso` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-ad-account-remove-license` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_settings-fb-status-banner` ──[onAccess (event)]──> `card_controller::c_updateFirebaseStatusUI`
- `card_view::v_settings-fb-app` ──[onClick (event)]──> `card_controller::c_bindSettingsEvents`
- `card_controller::c_bindSettingsEvents` ──[mutatesState (data)]──> `card_model::m_SystemSettingsModel`
- `card_view::v_settings-fb-api` ──[onAccess (event)]──> `card_controller::c_bindSettingsEvents`
- `card_controller::c_bindSettingsEvents` ──[mutatesState (data)]──> `card_model::m_SystemSettingsModel`
- `card_view::v_btn-save-firebase` ──[onClick (event)]──> `card_controller::c_bindSettingsEvents`
- `card_controller::c_bindSettingsEvents` ──[mutatesState (data)]──> `card_model::m_SystemSettingsModel`
- `card_view::v_settings-startup-checkbox` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_btn-regenerate-pass` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_inapp-refresh-admin-btn` ──[onClick (event)]──> `card_controller::c_bindEvents`
- `card_view::v_inapp-save-limits-btn` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_inapp-lim-sessions` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_inapp-lim-bandwidth` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_inapp-lim-fps` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_inapp-lim-timeout` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_inapp-lim-retention` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_inapp-lim-hw` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_inapp-log-search` ──[onInput (event)]──> `card_controller::c_if`
- `card_view::v_inapp-log-level` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_inapp-purge-logs-btn` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_inapp-log-tbody` ──[onAccess (event)]──> `card_controller::c_filterAndRenderLogs`
- `card_view::v_web-marquee-ticker` ──[onAccess (event)]──> `card_controller::c_updateMarqueeTickerText`
- `card_view::v_launch-popup-modal` ──[onAccess (event)]──> `card_controller::c_then`
- `card_view::v_launch-popup-close-btn` ──[onAccess (event)]──> `card_controller::c_then`
- `card_view::v_startup-checkbox` ──[onAccess (event)]──> `card_controller::c_then`
- `card_view::v_hide-popup-checkbox` ──[onAccess (event)]──> `card_controller::c_then`
- `card_view::v_launch-popup-ok-btn` ──[onAccess (event)]──> `card_controller::c_then`
- `card_view::v_audit-report-modal` ──[onAccess (event)]──> `card_controller::c_if`
- `card_view::v_add-device-modal` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_device-modal-title` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_device-modal-id` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_device-modal-name` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_device-modal-ip` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_device-modal-group` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_device-modal-access` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_device-modal-save-btn` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_dynamic-modal-overlay` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_dynamic-modal-title` ──[onAccess (event)]──> `card_controller::c_constructor`
- `card_view::v_dynamic-modal-body` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_dynamic-modal-footer` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_dynamic-modal-cancel` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_dynamic-modal-confirm` ──[onClick (event)]──> `card_controller::c_constructor`
- `card_view::v_anydesk-invite-modal` ──[onAccess (event)]──> `card_controller::c_catch`
- `card_view::v_btn-close-invite-modal` ──[onClick (event)]──> `card_controller::c_catch`
- `card_view::v_invite-target-id` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_invite-profile-select` ──[onClick (event)]──> `card_controller::c_catch`
- `card_view::v_btn-cancel-invite` ──[onClick (event)]──> `card_controller::c_catch`
- `card_view::v_btn-submit-invite` ──[onClick (event)]──> `card_controller::c_catch`
- `card_view::v_anydesk-incoming-modal` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_incoming-source-id` ──[onClick (event)]──> `card_controller::c_if`
- `card_view::v_btn-reject-invite` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_view::v_btn-accept-invite` ──[onAccess (event)]──> `card_controller::c_if`
- `card_controller::c_if` ──[mutatesState (data)]──> `card_model::m_LocalSystemModel`
- `card_model::m_SystemTraceabilityLogModel` ──[syncsWith (persistence)]──> `card_db::db_main`
- `card_settings::s_jdx_fb_api_key` ──[persistsKey (persistence)]──> `card_db::db_local`

---

## 🔬 Lens 3: AST Reality & Risk Diagnostics (The "What")

### 🟡 AMBER Warnings: God-Node Centrality Shield
- ⚠️ **`JDroidXDeskNative.cs`** (11,249 lines | In-Degree: `16`)
  - **Root Cause:** File exceeds 4,000 lines (11,249 lines). Overloaded responsibilities violate Single Responsibility Principle.
  - **Mitigation:** Decompose into modular sub-controllers under `controllers/` and `models/`.
- ⚠️ **`app.js`** (4,689 lines | In-Degree: `16`)
  - **Root Cause:** File exceeds 4,000 lines (4,689 lines). Overloaded responsibilities violate Single Responsibility Principle.
  - **Mitigation:** Decompose into modular sub-controllers under `controllers/` and `models/`.
- ⚠️ **`controllers/app-controller.js`** (4,689 lines | In-Degree: `16`)
  - **Root Cause:** File exceeds 4,000 lines (4,689 lines). Overloaded responsibilities violate Single Responsibility Principle.
  - **Mitigation:** Decompose into modular sub-controllers under `controllers/` and `models/`.
- ⚠️ **`graphify-out/.graphify_labels.json`** (4,862 lines | In-Degree: `16`)
  - **Root Cause:** File exceeds 4,000 lines (4,862 lines). Overloaded responsibilities violate Single Responsibility Principle.
  - **Mitigation:** Decompose into modular sub-controllers under `controllers/` and `models/`.

### 🟡 AMBER Warnings: Hardcoded Credential & Secret Exposure
- ⚠️ **`app.js`**: Firebase API Key Fallback (Risk: `Medium`)
  - **Mitigation:** Move literal fallback to `/api/settings` or secure local storage valuebase.
- ⚠️ **`app.js`**: Hardcoded One-Time Pass (Risk: `High`)
  - **Mitigation:** Move literal fallback to `/api/settings` or secure local storage valuebase.
- ⚠️ **`controllers/app-controller.js`**: Firebase API Key Fallback (Risk: `Medium`)
  - **Mitigation:** Move literal fallback to `/api/settings` or secure local storage valuebase.
- ⚠️ **`controllers/app-controller.js`**: Hardcoded One-Time Pass (Risk: `High`)
  - **Mitigation:** Move literal fallback to `/api/settings` or secure local storage valuebase.

---

## 🔄 Lens 4: Multi-Branch Lifecycle Funnel (The "Where")

- **Closed-Loop Health:** `75.0%`
- **Orphan DOM Branches Detected:** `75`

### Verified Execution Pipelines:

### 🟢 Auth Journey: Login Gate ➔ Firebase Initializer ➔ Session Unlock (`CLOSED`)
- **Start Trigger:** User clicks #login-submit-btn
- **Flow Pipeline:** bindLoginGate() intercepts click event ➔ Validates email and password inputs ➔ Reads Firebase keys from SystemSettingsModel ➔ Firebase initializes app and verifies session ➔ Preloader fade-out triggered and Console unlocked
- **Delivery Terminal:** Desktop Console Active
- **Closed-Loop Feedback:** SystemTraceabilityLogModel audit log & toast notification

### 🟢 Affordance Journey: Password Visibility Toggle (`CLOSED`)
- **Start Trigger:** User clicks #toggle-pass-visibility
- **Flow Pipeline:** Click event listener fires in init() ➔ Reads current type of #login-password-input ➔ Toggles input type between 'password' and 'text' ➔ Updates #toggle-pass-icon class (eye / eye-slash)
- **Delivery Terminal:** Immediate DOM state feedback
- **Closed-Loop Feedback:** Instant visual icon toggle

### 🟢 Governance Journey: Zero-Hardcode Valuebase Sync (`CLOSED`)
- **Start Trigger:** User updates Firebase key or Local Port in settings
- **Flow Pipeline:** Settings controller captures input change ➔ SystemSettingsModel writes to localStorage ➔ Runtime rebinds active connection without code edit
- **Delivery Terminal:** Runtime config active
- **Closed-Loop Feedback:** Health check probe confirmation

---
*Automated Report compiled by `scripts/mindmap_builder.py` (Enterprise v3.0) • 0 LLM API Tokens Consumed.*
