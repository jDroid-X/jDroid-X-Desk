# Master Logical Prompt & Architecture Blueprint: Node.js Server Gateway

**Author:** jDroid-X / Enterprise Technology Workforce  
**Agent Authority:** [24_NodeJS_Server](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/24_NodeJS_Server/AGENT-SPEC.md)  
**Governance Mapping:** [PROGRAM-MAPPING.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/PROGRAM-MAPPING.md)  

---

## 1. Executive Summary & Problem Definition

In typical rapid development, independent applications (e.g. `GitExtract`, `FreeModelsClub`, `jFindMe`, `SpritualKarim`) repeatedly spawn isolated `server.js` instances on arbitrary ports (`3000`, `8080`, `8088`, etc.). This causes:
- Port collisions (`EADDRINUSE`).
- Memory waste and orphaned background node processes.
- Fractured routing and lack of unified telemetry.

### The Core Architectural Rule
> **"Add a new Node.js server ONLY if it is strictly necessary (e.g. completely isolated native sandbox, conflicting Node runtime requirements, or stand-alone production container). In all other cases, segregate duties and activities holistically on a single gateway instance by: `localhost:<port>/<app_slug>/<page_url>#<heading>`."**

---

## 2. Duty & Activity Segregation Hierarchy

| Hierarchy Level | Representation | Standard Mapping | Example |
|---|---|---|---|
| **Port (`<port>`)** | Execution Domain | **3000**: Core Enterprise Apps<br>**8080**: Tools, Extractors & Utilities<br>**8088**: Python/AI Bridge Services | `http://localhost:8080` |
| **App Slug (`/<app_slug>/`)** | Application Domain | Modular App Container | `http://localhost:8080/gitextract/` |
| **Page URL (`/<page_url>`)** | Functional Page | Sub-page or Controller View | `http://localhost:8080/gitextract/hub` |
| **Heading / Hash (`#<heading>`)** | Duty & Activity State | Interactive Section / Mode | `http://localhost:8080/gitextract/hub#transcript`<br>`http://localhost:8080/gitextract/hub#reverse-prompt`<br>`http://localhost:8080/gitextract/hub#api-routes` |

---

## 3. The Master Logical Prompt for Node.js Server Gateway

Use this exact prompt specification when instructing any agent, subagent, or developer to configure, extend, or run a Node.js server:

```markdown
### SYSTEM LOGICAL PROMPT: UNIFIED NODE.JS MULTI-TENANT GATEWAY

Role: You are Agent 24 (Node.js Server & Gateway Orchestrator).
Mission: Implement or extend the local Node.js server following the Enterprise Holistic Architecture.

[PRIME RULES]
1. CHECK BEFORE CREATING: Never spin up a new independent Node.js server if an existing gateway is already running on the allocated port.
2. PORT ALLOCATION MATRIX:
   - Port 3000: Primary Business & Web Applications (e.g., FreeModelsClub, Portals)
   - Port 8080: Web Tools, Extractors & Utilities (e.g., GitExtract, FindMe, Scrapers)
   - Port 8088: AI / Python Microservice Bridges & Proxies
3. URL & ROUTE CONTRACT:
   Segregate every duty and activity strictly along:
   http://localhost:<PORT>/<app_slug>/<page_url>#<heading>
   - <app_slug>: Registered name of the application.
   - <page_url>: Specific tool view or sub-route.
   - #<heading>: Section anchor representing current user activity/mode (e.g., #transcript, #diagram, #api-routes).
4. ARCHITECTURE (OOPS-based MVC):
   - Controller: Dedicated router module per app in `routes/<app_slug>.js`.
   - View: Static frontend assets served from public/web directory under `/<app_slug>/`.
   - Shared Middlewares: Call-by-reference CORS, body parser, and logging.
5. HEALTH & DISCOVERY ENDPOINT:
   Expose `GET /api/gateway/routes` returning JSON of all mounted apps, target paths, and their activity headings.
6. LIFECYCLE DISCIPLINE:
   Trap SIGINT and SIGTERM gracefully to release port bindings without leaving zombie processes.
```

---

## 4. Canonical OOPS-based MVC Implementation Template

```javascript
/**
 * Unified Node.js Gateway Controller (OOPS MVC Standard)
 * Call-by-reference dynamic mounting of multi-app routes
 */
const http = require('http');
const fs = require('fs');
const path = require('path');
const url = require('url');

class UnifiedGatewayServer {
  constructor(port = 8080) {
    this.port = port;
    this.routes = new Map(); // slug -> { handler, staticDir, activities }
    this.server = null;
  }

  registerApp(slug, { staticDir = null, apiHandler = null, activities = [] }) {
    this.routes.set(slug, { staticDir, apiHandler, activities });
    console.log(`[Gateway] Registered App: /${slug} (Activities: ${activities.join(', ')})`);
  }

  start() {
    this.server = http.createServer((req, res) => {
      const parsedUrl = url.parse(req.url, true);
      const pathname = parsedUrl.pathname;

      // Enable standard local CORS
      res.setHeader('Access-Control-Allow-Origin', '*');
      res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
      res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
      if (req.method === 'OPTIONS') { res.writeHead(200); res.end(); return; }

      // 1. Gateway Route & Telemetry Directory
      if (pathname === '/' || pathname === '/api/gateway/routes') {
        const directory = [];
        for (const [slug, meta] of this.routes.entries()) {
          directory.push({
            app: slug,
            baseUrl: `http://localhost:${this.port}/${slug}/`,
            activities: meta.activities.map(act => `http://localhost:${this.port}/${slug}/#${act}`)
          });
        }
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ gatewayPort: this.port, apps: directory }, null, 2));
        return;
      }

      // 2. Resolve App Slug (e.g. /gitextract/...)
      const parts = pathname.split('/').filter(Boolean);
      const appSlug = parts[0];

      if (this.routes.has(appSlug)) {
        const appMeta = this.routes.get(appSlug);

        // Check if API route
        if (parts[1] === 'api' && appMeta.apiHandler) {
          return appMeta.apiHandler(req, res, parsedUrl);
        }

        // Serve Static Files for App
        if (appMeta.staticDir) {
          const subPath = parts.slice(1).join('/') || 'index.html';
          const filePath = path.join(appMeta.staticDir, subPath);
          return this.serveStatic(res, filePath);
        }
      }

      res.writeHead(404, { 'Content-Type': 'text/plain' });
      res.end(`[Gateway 404] No registered application for path: ${pathname}`);
    });

    this.server.listen(this.port, () => {
      console.log(`[Gateway] Holistic Node.js Server listening on http://localhost:${this.port}`);
    });

    // Graceful Shutdown
    process.on('SIGINT', () => this.shutdown());
    process.on('SIGTERM', () => this.shutdown());
  }

  serveStatic(res, filePath) {
    fs.readFile(filePath, (err, content) => {
      if (err) {
        res.writeHead(404, { 'Content-Type': 'text/plain' });
        res.end('Asset Not Found');
        return;
      }
      const ext = path.extname(filePath).toLowerCase();
      const mimeTypes = {
        '.html': 'text/html',
        '.js': 'application/javascript',
        '.css': 'text/css',
        '.json': 'application/json',
        '.png': 'image/png',
        '.jpg': 'image/jpeg'
      };
      res.writeHead(200, { 'Content-Type': mimeTypes[ext] || 'application/octet-stream' });
      res.end(content);
    });
  }

  shutdown() {
    console.log('[Gateway] Gracefully closing server...');
    if (this.server) this.server.close(() => process.exit(0));
    else process.exit(0);
  }
}

module.exports = { UnifiedGatewayServer };
```
