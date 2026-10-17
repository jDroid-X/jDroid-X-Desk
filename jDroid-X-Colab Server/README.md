# 🚀 jDroid-X-Colab Server | Centralized Cloud Relay & Remote Access Architecture

> **Subsidiary of jDroid-X Studio**  
> **Official Web Portal:** [https://www.jdroid-x.com](https://www.jdroid-x.com)  
> **Colab Reference Notebook:** [Google Colab jDroid-X Central Relay](https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai)

---

## 📌 1. Role of Google Colab in jDroid-X

In remote desktop and screen streaming setups, computers typically sit behind NAT (Network Address Translation), residential dynamic IPs, and strict corporate firewalls. Direct connections between **Desk 1** (client) and **Desk 2** (host) fail without complex router port-forwarding.

Google Colab fills this gap by acting as an **Out-of-Band High-Bandwidth Cloud Relay Broker**:
1. **Zero-Configuration NAT Traversal**: Host and client both make outbound HTTPS connections to the Colab relay. Because all connections are outbound, firewalls and router NATs do not block the stream.
2. **In-Memory Frame Buffer & Input Router**: Host pushes compressed screen frames (`/api/relay/push_frame`); Viewer pulls them (`/api/relay/pull_frame/<id>`). Mouse and keyboard keystrokes are enqueued in reverse (`/api/relay/push_input` -> `/api/relay/pull_input/<id>`).
3. **Public Ingress via Cloudflare Tunnel (`cloudflared`)**: The Colab runtime exposes port `8088` through an SSL-encrypted Cloudflare tunnel (`https://*.trycloudflare.com` or custom domain `https://www.jdroid-x.com`).
4. **Zero-Cost High-RAM Cloud Infrastructure**: No cloud server costs, AWS bills, or third-party TeamViewer/AnyDesk subscriptions needed.

---

## ⚡ 2. Is the Server Working Now?

- **Code & Components**: All server scripts, configurations, and notebooks are fully built, tested, and stored in this repository.
- **Cloud Execution Status**: Google Colab provides **on-demand, interactive sessions**. Colab virtual machines do not run permanently 24/7 in Google's cloud without an active notebook session. Free Colab sessions pause or disconnect if left idle for 15–30 minutes or after 12 hours maximum.
- **How to Activate**:
  1. Open the [Google Colab Notebook](https://colab.research.google.com/drive/1pQW73zTBEOWPpm0IffnrQrybB8khhLai) (or upload `jDroid-X-Colab-Notebook.ipynb`).
  2. Click **Runtime -> Run all** (or run Step 3).
  3. Copy the public HTTPS URL (e.g., `https://random-words.trycloudflare.com`).
  4. Paste this URL into `jDroid-X-Desk` settings or `app.js` under `Cloud Relay URL`.

---

## 📂 3. Directory Root Structure

```text
jDroid-X-Colab Server/
├── README.md                      # Architecture guide, live status, and ping protocol
├── jDroid-X-Colab-Relay-Server.py # v3.0 Central Cloud Relay Server (Wake-on-Ping, Frame Buffer, Input Queue)
├── colab-watchdog-keepalive.py    # Autonomous keep-alive & tunnel auto-recovery daemon
├── jDroid-X-Colab-Notebook.ipynb  # 1-Click execution notebook for Google Colab
├── colab-setup.sh                 # Bootstrap script for bash/Linux environment
└── windows_rdp_colab.yaml         # Mesh nodes, encryption, tunnel config, and endpoints
```

---

## 🔔 4. Wake-on-Ping & Remote Desktop Activity Protocol

When `jDroid-X-Desk` starts, connects, or clicks **Test Cloud Relay**:
1. **Ping Sent**: `jDroid-X-Desk` fires an HTTP GET or POST request to:
   - `/ping`
   - `/api/relay/ping`
   - `/api/relay/wake`
2. **Instant Wake Response**: The relay resets its activity timestamp (`LAST_ACTIVITY_TIME`), increments `PING_COUNT`, and responds with HTTP 200:
   ```json
   {
     "success": true,
     "status": "AWAKE",
     "service": "jDroid-X Colab Cloud Relay Server",
     "uptime_seconds": 128.4,
     "active_devices": 2,
     "total_pings": 14,
     "message": "Colab Relay awakened and active. Remote desktop session ready."
   }
   ```
3. **Watchdog Guard (`colab-watchdog-keepalive.py`)**: Runs every 60 seconds in Colab to verify the relay is responsive, emit heartbeat timestamps to prevent kernel idle timeouts, and auto-restart `cloudflared` if the tunnel drops.

---

## 🔒 5. Security & Encryption Standards

- **Encryption**: AES-256-GCM encrypted frame payloads between host and viewer.
- **Authentication**: 9-digit machine identifier (`Desk 1: 984-102-391`, `Desk 2: 710-224-900`) + 6-digit dynamic passcode.
- **Privacy**: Frames are held transiently in Colab RAM only; zero disk logging of remote screens.
