---
description: Holistic Node.js Server Management, Port Allocation, and Multi-Tenant Gateway Orchestration
---

# /nodejs-server-gateway

Workflow for orchestrating local Node.js servers, preventing unmanaged server sprawl, avoiding port collisions, and hosting applications holistically via the Enterprise Multi-Tenant Gateway.

---

## 🏛️ Operating Principles & Gating

1. **Anti-Sprawl Gate**: Add a new Node.js server **ONLY IF** strictly necessary (e.g. completely isolated sandbox or standalone external deployment container).
2. **Multi-Tenant Gateway Preference**: Segregate duties and activities on the designated gateway by:
   `http://localhost:<PORT>/<app_slug>/<page_url>#<heading>`
3. **Port Matrix Standard**:
   - `3000`: Core Enterprise Web Applications & Portals
   - `8080`: Web Tools, Extractors, Converters & Scrapers
   - `8088`: AI / Python Microservices & Local LLM Bridges
   - `5678`: n8n Workflow Automation
4. **Zero-Zombie Rule**: Always check active port bindings before launch and register `SIGINT`/`SIGTERM` handlers.

---

## Step-by-Step Execution Workflow

### Step 1: Pre-Flight Port & Process Inspection
Before launching any server, query active listeners and running node processes:

```powershell
# Check if target port is already in use
netstat -ano | findstr :8080

# Check running node processes
Get-CimInstance Win32_Process -Filter "name = 'node.exe'" | Select-Object ProcessId, CommandLine
```

- **If the gateway is already running on the port**: Do NOT launch a second server. Mount the new application route dynamically into the existing gateway.
- **If the port is occupied by an unrelated zombie process**: Report to operator or terminate cleanly before binding.

---

### Step 2: Duty & Activity Segregation Mapping
Map the target application into the standard URL contract:
- **Port**: Choose domain port (`3000`, `8080`, or `8088`).
- **App Slug**: Unique kebab-case name (e.g. `gitextract`, `freemodels`, `jfindme`).
- **Page URL**: Functional view (e.g. `hub`, `dashboard`, `settings`).
- **Heading / Anchor**: Active duty section (e.g. `#transcript`, `#reverse-prompt`, `#diagram`).

Example:
`http://localhost:8080/gitextract/hub#transcript`
`http://localhost:8080/gitextract/hub#reverse-prompt`

---

### Step 3: Mount Route in Gateway Controller (OOPS MVC)
Register the application into the gateway controller using call-by-reference:

```javascript
// Register route in gateway
gateway.registerApp('gitextract', {
  staticDir: 'c:/Users/jiten/jAnitGravity/jGitHubExtract/public',
  apiHandler: gitExtractApiRouter,
  activities: ['transcript', 'reverse-prompt', 'diagram-viewer']
});
```

---

### Step 4: Verification & Smoke Test
1. Test Gateway Telemetry:
   `curl http://localhost:8080/api/gateway/routes`
2. Test App Static Mount:
   `curl http://localhost:8080/<app_slug>/`
3. Test Deep-link Anchors in Browser:
   Verify page loads cleanly with `#heading` state active.
