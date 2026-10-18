# Master Program Mapping & Relationship Matrix

**VERSION 2.0 (Gemini 3.x Multi-Model Enterprise Architecture)**  
**Classification:** Enterprise Operating System Governance Matrix  
**Architecture:** Dynamic OOPS-based MVC & 7-Stage Closed-Loop Waterfall  

---

> **Master Operational Router:** [GEMINI.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/rules/GEMINI.md)  
> **Master Workforce Registry:** [AGENTS.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/AGENTS.md)  
> **Token Hygiene Protocol:** [TOKEN_AUDIT_OPTIMIZATION.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/rules/TOKEN_AUDIT_OPTIMIZATION.md)  

---

## 1. Governance Architecture Overview

This document serves as the **Single Source of Truth (SSOT)** master relationship table across all programs, modules, and agents within the .kit Enterprise Architecture. Any modification to code, architecture, or workflow must reference this matrix to verify upstream dependencies, downstream integrations, pre-flight programming contracts, and closed-loop feedback convergence.

### Screen-First Outside-In OOPS-based MVC Governance Pipeline Sequence
Development strictly follows the human-centered Screen-First MVC flow (User ➔ View ⇄ Controller ⇄ Model). The frontend is the primary interface where humans understand code; backend and data models are constructed strictly in service of verified screen requirements:
```
Master Orchestrator Agent (Governance / Prompt Classifier)
      ↓
UI/UX Agent (View: Layout Hierarchy, Design Tokens & Affordance Matrix)
      ↓
Frontend Agent (View: Component Structure, Reactive States & DOM Event Listeners)
      ↓
Business Analyst Agent (Model/View Contract: Screen Functional Specs & Acceptance Criteria in requirement/{screen}.txt)
      ↓
Enterprise Architect Agent (Controller: Class Architecture, Contracts & Screen Action Handlers)
      ↓
Backend Agent (Controller: Business Logic, Services & APIs strictly serving the View)
      ↓
Database Architect Agent (Model: Persistence, Schema & Storage derived from Screen Payload)
      ↓
Dependency Agent (Model: Packages, SDKs & Compatibility)
      ↓
Integration Agent (Controller: Adapters, Connectors & Cross-Module Binds)
      ↓
Workflow Agent (Controller: State Machine & Operating Rules)
      ↓
Security Agent (Governance: AES-256, Threat Defense & Access Rules)
      ↓
Testing Agent & QA Agent (Governance: Screen User Journey, Unit, E2E & Verification Gates)
      ↓
Optimization Agent (Controller: Profiling, Memory, FPS & Token Reduction)
      ↓
Deployment Agent & Publishing Agent (DevOps: CI/CD, Builds & Releases)
      ↓
Monitoring Agent & Analytics Agent (Telemetry: Runtime Logs & Health)
      ↓
Audit Agent (Closed-Loop Sign-off & Defect Ticket Escalation to Orchestrator)
```

### 1.1 Ponytail Optimization Firewall (Stages 2 & 3 Gatekeepers) & Mid-Project Pruning Protocol
To prevent premature class generation and eliminate code bloat before implementation starts, the **Ponytail Decision Ladder** is embedded as an active architectural gatekeeper across Stage 2 and Stage 3:

1. **Stage 2: Scope Gate (Business Analyst & UI/UX Lead)**:
   - *Step 1: Need to exist?* ➔ Verify human-facing screen necessity. Prune speculative feature creep (YAGNI).
   - *Step 2: Reuse existing?* ➔ Audit existing screens and services; enforce call-by-reference reuse.
2. **Stage 3: Architecture Gate (Enterprise Architect & Controller Lead)**:
   - *Step 3: Stdlib built-ins?* ➔ Handle data parsing and structures with language built-ins.
   - *Step 4: Platform native?* ➔ Leverage browser DOM/Web APIs or Android OS native handles before external code.
   - *Step 5: Installed dependency?* ➔ Inspect `package.json`/`build.gradle`; prohibit adding redundant libraries.
   - *Step 6: One-liner / Lambda?* ➔ If a functional helper or lambda solves it, do not construct a class.
   - *Step 7: Lean OOPS MVC Class?* ➔ Synthesize formal MVC classes ONLY when Steps 1–6 are genuinely exhausted.

#### 🛡️ Mid-Project Zero-Regression Pruning Action Plan (Halfway-Completed Code)
When optimizing or shrinking a codebase that is already running or halfway built:
- **Rule of Invariance**: *Prune internal implementation, never destroy the interface contract.*
- **Step 1 (Baseline Lock)**: Record inputs, outputs, and characterization test assertions (`UPTIME_200`, `STATE_HYDRATED`).
- **Step 2 (Reverse Audit)**: Grep all upstream call-sites; identify state side-effects.
- **Step 3 (Strangler Adapter)**: Retain legacy class/method signatures as an adapter; forward calls internally to the lean native/stdlib one-liner.
- **Step 4 (Parity Verification)**: Confirm 100% input/output parity and zero frame drops (60 FPS).
- **Step 5 (Progressive Inlining)**: Deprecate legacy wrappers and inline call-sites file-by-file during feature updates.

---

## 2. Master Relationship Table & Model Allocation (22 Enterprise Agents)

| # | Enterprise Agent (Screen-First) | Physical Directory | MVC Layer | 7-Stage Step | Assigned Gemini 3.x Model | Direct Upstream Dependency | Direct Downstream Integration | Primary Deliverables | Token Optimization Rationale |
|---|---|---|---|---|---|---|---|---|---|
| **01** | **Master Orchestrator Agent** | `01_Master_Orchestrator` | Governance / Meta | Stage 1: Initiate & Plan | **Gemini 3.8 (High)** | JEET / User Request | UI/UX Agent, Business Analyst | Project Plan, Scope Matrix, Model Dispatch Orders, REA Recon Routing | Full cognitive reasoning for intent analysis, multi-agent dispatch, and token budgeting. |
| **02** | **UI/UX Pro Max Agent** | `18_UI_UX` | View (Design System & Affordances) | Stage 2: Screen Discovery | **Gemini 3.8 (High)** | Master Orchestrator, User Request | Frontend Agent, Business Analyst | Screen Layout Hierarchy, CSS Tokens, Micro-animations, Wireframes, Affordance Inventory | Design token symmetry, responsive layouts, micro-interactions, and visual elegance. |
| **03** | **Frontend Agent** | `06_Frontend` | View (Presentation & Action Matrix) | Stage 2: Specs & 4: Code | **Gemini 3.8 (Medium)** | UI/UX Agent, User Request | Business Analyst, Enterprise Architect | Screen Component Tree, Reactive States, DOM Event Listeners, Action Matrix | Clean DOM lifecycle binding, reactive UI states, zero UI-thread blocking. |
| **04** | **Business Analyst Agent** | `02_Business_Analyst` | Model (Domain / Specs) | Stage 2: Requirements Analysis | **Gemini 3.7 (Medium)** | Frontend Agent, UI/UX Agent | Enterprise Architect, Backend Agent | SRS Document, Use Case Specs, Screen Specs in `requirement/{screen}.txt` | Balanced synthesis of functional scope and edge-case discovery without heavy token overhead. |
| **05** | **Enterprise Architect Agent** | `03_Enterprise_Architect` | Controller (System) | Stage 3: System Design | **Gemini 3.8 (High)** | Frontend Agent, Business Analyst | Backend Agent, Database Architect | System Design Document (SDD), Class Diagrams, Screen Event Interface Contracts | Deep architectural modeling, boundary validation, and zero-conflict module isolation. |
| **06** | **Backend Agent** | `05_Backend` | Controller (API / Logic) | Stage 4: Implementation | **Gemini 3.8 (Medium)** | Frontend Action Contract, Enterprise Architect | Integration Agent, Testing Agent | API Handlers, Controllers, Business Services serving Screen Actions | Production-grade OOPS controller logic, error recovery, and robust state pipelines. |
| **07** | **Database Architect Agent** | `04_Database_Architect` | Model (Persistence) | Stage 3: Design & 4: Code | **Gemini 3.7 (Medium)** | Enterprise Architect, Frontend Data Contract | Backend Agent, Dependency Agent | ER Diagrams, Schemas, ORM Models, Migration Scripts | Robust data normalization, index optimization, and local storage sandbox schemas derived from screen state. |
| **08** | **Dependency Agent** | `16_Dependency` | Model (Dependencies) | Stage 4: Implementation | **Gemini 3.7 (Low)** | Enterprise Architect, Database | Integration Agent, Backend Agent | Dependency Maps, SDK Compatibility Reports, License Audits | Rapid dependency resolution, version conflict detection, and CVE scanning. |
| **09** | **Integration Agent** | `19_Integration` | Controller (Adapters) | Stage 4: Implementation | **Gemini 3.7 (Medium)** | Backend Agent, Frontend Agent | Security Agent, Testing Agent | API Adapters, SDK Integrations, Webhook Connectors, REA MCP Bridges | Clean adapter patterns, protocol conversion, defensive error wrapping. |
| **10** | **Workflow Agent** | `07_Workflow` | Controller (State Engine) | Stages 3 to 7 (Lifecycle) | **Gemini 3.7 (Low)** | Enterprise Architect | All Operational Agents | State Machines, 20-Rule Verification Checklists, Flow Guards | Fast deterministic validation of stage gates, rule checklists, and branch closures. |
| **11** | **Security Agent** | `11_Security` | Governance (SecOps) | Stage 3: Design & 5: Test | **Gemini 3.8 (High)** | Enterprise Architect | Backend, Integration, Deploy | Threat Models, AES-256 Rules, Token Storage Specs, REA Decompiled Code & Protocol Evidence | High-depth security inspection: XSS/CORS defense, token sandboxing, and audit logs. |
| **12** | **QA Agent** | `10_QA` | Governance (Quality) | Stage 5: Testing | **Gemini 3.8 (Medium)** | Business Analyst, Frontend Agent | Testing Agent, Audit Agent | QA Acceptance Matrix, User Journey Screen Tests, Traceability | Exhaustive acceptance verification and edge-case boundary validation against SRS and screen affordance. |
| **13** | **Testing Agent** | `20_Testing` | Governance (Validation) | Stage 5: Testing | **Gemini 3.6 (Medium)** | Backend, Frontend, Integration | QA Agent, Audit Agent | Unit Tests, Integration Tests, E2E Test Reports, Bug Tickets | Automated unit test suite execution, regression detection, and assertion checks. |
| **14** | **Audit & Inspector Agent** | `17_Audit` | Governance (Inspection & Audit) | Stage 5: Test & 7: Maint | **Gemini 3.8 (High)** | All Active Agents | Master Orchestrator (Feedback Loop) | 5-Pillar Inspector Scorecard, Zero-Orphan Audits, AST Diff Tickets | Comprehensive code/architectural compliance audit, zero-orphan validation, and sign-off. |
| **15** | **Optimization Agent** | `21_Optimization` | Controller (Performance) | Stage 5: Test & 7: Maint | **Gemini 3.7 (Medium)** | Testing Agent, Monitoring Agent | Backend, Frontend, Deployment | Profiling Reports, Memory/CPU Caps, Token Trimming | Algorithmic optimization, memory leak prevention, and asset caching strategies. |
| **16** | **Cost Optimization Agent** | `13_Cost_Optimization` | Governance (Finance) | Stages 3 to 7 | **Gemini 3.6 (Low)** | Master Orchestrator | AI Agent Manager, Backend Agent | Token Consumption Audits, Cost Reports, Quota Guards | Token usage profiling, context trimming, and computational budget enforcement. |
| **17** | **AI Agent Manager** | `09_AI_Agent_Manager` | Governance (Agentic) | Stages 4 to 6 (Runtime) | **Gemini 3.7 (Low)** | Master Orchestrator | Specialized Agents | Subagent Task Queues, Health Heartbeats, IPC Routing | Lightweight lifecycle management, task scheduling, and heartbeat monitoring. |
| **18** | **Prompt Engineering Agent** | `08_Prompt_Engineering` | Controller (Cognitive) | Stage 4: Implementation | **Gemini 3.7 (High)** | Master Orchestrator | Backend Agent, AI Providers | Optimized System Prompts, Few-shot Contexts, Token Boundaries | Precision prompt crafting, persona anchoring, anti-drift constraints, and context efficiency. |
| **19** | **Local Model Agent** | `ollama_client_model.py` | Controller (Local AI) | Stages 3 to 7 | **Local Ollama MCP** | Master Orchestrator | Backend Agent, Testing Agent | Offline autocompletion, air-gapped unit tests, private data processing | Zero token cost, local offline execution, air-gapped security. |
| **20** | **NodeJS Server Agent** | `24_NodeJS_Server` | Controller (Runtime Gateway) | Stage 4: Code & 6: Deploy | **Gemini 3.7 (Medium)** | Enterprise Architect, Backend | Frontend Agent, Deployment Agent | Holistic Port Registry, Multi-Tenant Gateway, Route Mounts | **Selective Activation:** Spawned ONLY when persistent multi-tenant backend gateway services, WebSocket streams, or isolated runtime servers are architecturally necessary. Prohibited for routine static HTML/JS pages or trivial development. |
| **21** | **Deployment Agent** | `22_Deployment` | Controller (DevOps) | Stage 6: Deployment | **Gemini 3.7 (Medium)** | Optimization Agent, Audit Agent | Monitoring Agent, Publishing Agent | CI/CD Pipelines, Dockerfiles, Production Builds, Smoke Tests | Robust deployment automation, zero-downtime migrations, and smoke testing. |
| **22** | **Publishing Agent** | `14_Publishing` | View / Release | Stage 6: Deployment | **Gemini 3.6 (Low)** | Deployment Agent, UI/UX Agent | Master Orchestrator | Release Bundles, Store Metadata, Distribution Artifacts | Release packaging, version stamping, and distribution manifest generation. |
| **23** | **Monitoring Agent** | `12_Monitoring` | Controller (Telemetry) | Stage 6: Deploy & 7: Maint | **Gemini 3.1 (Low)** | Backend Agent, Integration | Optimization Agent, Orchestrator | Telemetry Feeds, Error Logs, Heartbeat Monitors | Ultra-lightweight log parsing and runtime telemetry monitoring with minimal tokens. |
| **24** | **Analytics Agent** | `15_Analytics` | Model (Insights) | Stage 7: Maintenance & Support | **Gemini 3.6 (Medium)** | Monitoring Agent | Business Analyst, Orchestrator | KPI Telemetry, User Funnel Analysis, Performance Dashboards | Statistical analysis of usage logs, retention metrics, and telemetry aggregation. |

---

### 2.1 Unified Node.js Server & Multi-Tenant Port Allocation Matrix
All web applications and tools MUST adhere to the following port and route segregation standard.
**Absolute Selective Activation Rule:** Add a new Node.js server ONLY IF strictly necessary (isolated sandbox or separate deployment package). Never invoke Agent 24 or spin up dedicated Node.js servers for routine static HTML/CSS/JS mockups or trivial development scripts. Otherwise, mount to the designated port under `localhost:<port>/<app_slug>/<page_url>#<heading>`.

| Port | Allocation Scope | Hosted Applications / Duties | Route Pattern |
|---|---|---|---|
| **3000** | Core Enterprise Applications | Main portals, FreeModelsClub, SpritualKarim | `http://localhost:3000/<app_slug>/<page>#<heading>` |
| **8080** | Web Tools & Extractors | GitExtract, FindMe, URL Scrapers, Converters | `http://localhost:8080/<app_slug>/<page>#<heading>` |
| **8088** | Python & AI Bridges | FastAPI microservices, speech-to-text, local models | `http://localhost:8088/<api_slug>/<endpoint>` |
| **5678** | Workflow Automation *(Optional / Dormant)* | n8n automations & webhook receivers *(Wake On-Demand Only; Silent by Default)* | `http://localhost:5678/webhook/<flow_id>` |


---

## 3. Mandatory Pre-Flight Programming Standards Checklist (Rule 2 Contract)

Before ANY code change, refactor, or new file creation, the responsible agent MUST compile and declare the **Pre-Flight 6-Point Declaration Block**:

```markdown
### Pre-Flight Code Modification Declaration
1. [Variables]:
   - State variables affected or mutated
   - Local vs global scope boundaries
   - Dynamic binding targets (e.g. this.results, activeProfile)
2. [Constants]:
   - Config parameters, thresholds, timeouts, and fixed lookup tables
   - Immutable styling tokens or math constants
3. [Dependencies]:
   - Required imports, modules, utility classes, and math engines
   - SDK compatibility and version locks
4. [Integration Points]:
   - DOM event listeners, API endpoints, IPC messaging channels
   - Upstream callers and downstream listeners
5. [Hardcode to Valuebase / Toggle Switch Context Config]:
   - Replacement of hardcoded strings, endpoints, and magic numbers with dynamic valuebases
   - Context toggle switches (e.g. mock vs real API, dev vs prod, threshold sliders, theme switches)
   - Configuration models and runtime setting flags (e.g. user_settings.json)
6. [Optimized Performance]:
   - Call-by-reference reuse to eliminate redundant calculation and memory leaks
   - UI Thread preservation (60 FPS, non-blocking asynchronous event loops)
```

### Post-Change Verification Standard (Zero-Defect Rule):
1. **No Unresolved References**: Verify every variable and function call has a defined declaration.
2. **Call-by-Reference Reuse**: Never duplicate identical calculation or formatting logic; reference the shared utility/service.
3. **Preserve Existing Code**: Do not delete earlier working logic; update deprecated signatures or merge new extensions cleanly.
4. **Closed-Loop Feedback**: If tests or audit fail, generate an AST Diff Ticket and route back to the responsible agent.

---

## 4. Closed-Loop Defect Routing & Branch Convergence (Rule 1 & 17)

1. **Branch Convergence**: Every branching condition must either:
   - Close logically with structured feedback, or
   - Merge back into the main pipeline with verified state parameters.
2. **Defect Ticket Escalation**: If an error occurs in Stage 5 (Testing/Audit/Security):
   - The detecting agent produces an **AST Diff Defect Ticket** detailing: Exact file, line range, failing assertion, and root-cause variable.
   - Master Orchestrator routes the ticket to the responsible agent (Backend, Frontend, Database, or Dependency).
   - Hotfix is applied and re-verified through Stage 5 before proceeding to Stage 6 (Deployment).
3. **Zero Orphan Rule**: The **Audit Agent** validates that no class, variable, service, or asset is generated without an active entry in this Program Mapping matrix.

---

## 5. AnyDesk Parity & Remote Desktop Domain Mapping

| Component | Layer | File Reference | Upstream Integration | Downstream Consumers |
| :--- | :--- | :--- | :--- | :--- |
| **Session Invites Engine** | Model / API | [server.js](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/server.js), [database.json](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/database.json) | `/api/invites` CRUD | `#anydesk-invite-modal`, `#anydesk-incoming-modal` |
| **AnyDesk Settings Studio** | View / Controller | [index.html](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/index.html), [app.js](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/app.js) | `/api/settings` PUT/GET | 12 Sub-panel DOM elements (`#ad-tab-*`) |
| **Portable Admin Client** | Executable Tool | [jDroid-Client.bat](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/jDroid-Client.bat) | `/api/download-client` | Remote Technician / Host machine |
| **Colab Cloud Relay Bridge** | Network Relay | Google Colab Notebook | WebSocket `/relay` | WebRTC P2P ICE Traversal across NAT |

---

## 6. Open-Source Remote Desktop Integration & Master Issue Mapping

### 6.1 Upstream Open-Source Code Repositories (`jDroid-X-Desk-Server/Github/`)
| Source Project | Path Location | Architecture Focus | Enterprise Agent Owner | Targeted Master Issues (P0-P3) |
| :--- | :--- | :--- | :--- | :--- |
| **RustDesk** | [`jDroid-X-Desk-Server/Github/rustdesk-master`](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/jDroid-X-Desk-Server/Github/rustdesk-master) | Rust/C++ Rendezvous (`hbbs`), Relay (`hbbr`), DXGI screen capture, KCP UDP stream, Virtual displays | Agent 01 (Orchestrator), Agent 09 (Integration), Agent 11 (Security) | CON-01..12, SEC-01..10, DSP-05, INP-03..05 |
| **Sunshine & Moonlight** | [`jDroid-X-Desk-Server/Github/Sunshine-master`](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/jDroid-X-Desk-Server/Github/Sunshine-master) & [`moonlight-qt-master`](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/jDroid-X-Desk-Server/Github/moonlight-qt-master) | Ultra-low latency (<15ms, 60-120 FPS), NVENC GPU acceleration, WebRTC/NVStream, IDR keyframes | Agent 15 (Optimization), Agent 06 (Backend), Agent 19 (Integration) | PERF-01..12, DSP-01..03, AV-01..08 |
| **Apache Guacamole + FreeRDP** | [`jDroid-X-Desk-Server/Github/guacamole-server`](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/jDroid-X-Desk-Server/Github/guacamole-server) | WebSockets binary protocol, RDP/FreeRDP dirty rect tile caching, input synchronization, audit recording | Agent 06 (Backend), Agent 03 (Frontend), Agent 17 (Audit) | SEC-09, PERF-03, DSP-04, INP-01..02, FILE-01..12 |

### 6.2 Master Issue Registry & Traceability Matrix
- **Master Specification**: [`requirement/remote_access_master_issue_registry.md`](file:///c:/Users/jiten/jAnitGravity/jDroid-X-Desk/requirement/remote_access_master_issue_registry.md)
- **Activity 1 (UI Space & Viewport)**: Immersive Zen Mode (auto-collapsing chrome, floating MS RDP pin-bar, zero-margin canvas fit, DPI coordinate mapping).
- **Activity 2 (Lag Elimination & Colab Compute)**: Multi-threaded binary relay, NVENC GPU hooks, AVX-512 CPU SIMD encoding, adaptive 60 FPS pacing.


