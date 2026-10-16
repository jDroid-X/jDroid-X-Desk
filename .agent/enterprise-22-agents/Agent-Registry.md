# Agent Registry

**VERSION 1.1**  
**Classification:** Enterprise Workforce & Role Hierarchy  
**Master Relationship Map:** [PROGRAM-MAPPING.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/PROGRAM-MAPPING.md)

---

## 1. High-Level Workforce Domains (Category Mapping)

The enterprise workforce is grouped into functional domains, each supervising specialized execution agents:

| Workforce Domain | Supervised Specialized Agents | Primary Responsibility | Typical Deliverable |
|---|---|---|---|
| **Orchestrator** | 01 Master Orchestrator, 09 AI Agent Manager | Priorities, gates, synthesis, workflow governance | Decision-ready brief, Project Plan |
| **Intelligence** | 02 Business Analyst, 15 Analytics | Market/customer/competitor evidence, telemetry | SRS, Evidence package, KPI insights |
| **Business** | 02 Business Analyst, 13 Cost Optimization | Offer, pricing, economics, ROI validation | Business Model, Feasibility report |
| **Product** | 02 Business Analyst, 18 UI/UX | User problem, scope, user journeys, UI design | Wireframes, Design System tokens |
| **Technology** | 03 Enterprise Architect, 04 Database Architect, 05 Backend, 06 Frontend, 16 Dependency, 19 Integration, 22 Deployment, 24 NodeJS Server | Clean architecture, OOPS-based MVC implementation, build pipeline & runtime gateway | SDD, Schemas, APIs, Codebases, CI/CD, Gateway routes |
| **AI** | 08 Prompt Engineering, 09 AI Agent Manager | Prompt engineering, evaluation, LLM orchestration | System prompts, Agent lifecycles |
| **Growth** | 14 Publishing, 15 Analytics | Distribution, release publishing, channel telemetry | Release bundles, Distribution pipeline |
| **Finance** | 13 Cost Optimization | Unit economics, token & infrastructure cost control | Financial model, Cost audit report |
| **Quality** | 07 Workflow, 10 QA, 11 Security, 17 Audit, 20 Testing, 21 Optimization | Testing, security, process integrity, zero orphans | Audit sign-off, Test suites, Security scans |

---

## 2. The 24 Specialized Enterprise AI Agents (Screen-First Outside-In Sequence)

Each agent has a dedicated folder and specification document (`AGENT-SPEC.md`) containing its role, input/output contracts, and governance rules. Numbered in canonical Screen-First lifecycle order:

| # | Screen-First Agent | Folder & Spec Link | Primary Mission | MVC Layer |
|---|---|---|---|---|
| **01** | **Master Orchestrator** | [01_Master_Orchestrator](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/01_Master_Orchestrator/AGENT-SPEC.md) | Central command, stage gating, multi-agent dispatch | Governance / Meta |
| **02** | **UI/UX Pro Max** | [18_UI_UX](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/18_UI_UX/AGENT-SPEC.md) | Screen layouts, Sunrise Orange design tokens, Affordance Matrix | View (Design System) |
| **03** | **Frontend Specialist** | [06_Frontend](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/06_Frontend/AGENT-SPEC.md) | View presentation, component trees, DOM event listeners | View (Presentation) |
| **04** | **Business Analyst** | [02_Business_Analyst](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/02_Business_Analyst/AGENT-SPEC.md) | Screen functional requirements & acceptance criteria in `requirement/{screen}.txt` | Model (Domain / Specs) |
| **05** | **Enterprise Architect** | [03_Enterprise_Architect](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/03_Enterprise_Architect/AGENT-SPEC.md) | Dynamic OOPS MVC class architecture, interface contracts & action handlers | Controller (Architecture) |
| **06** | **Backend Specialist** | [05_Backend](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/05_Backend/AGENT-SPEC.md) | API controllers, business services serving Screen Actions | Controller (Logic) |
| **07** | **Database Architect** | [04_Database_Architect](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/04_Database_Architect/AGENT-SPEC.md) | Schemas, relations & storage derived from Screen State | Model (Persistence) |
| **08** | **Dependency Agent** | [16_Dependency](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/16_Dependency/AGENT-SPEC.md) | Dependency map verification, unresolved reference check | Model (Dependencies) |
| **09** | **Integration Agent** | [19_Integration](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/19_Integration/AGENT-SPEC.md) | External APIs, webhooks, third-party connectors | Controller (Adapters) |
| **10** | **Workflow Agent** | [07_Workflow](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/07_Workflow/AGENT-SPEC.md) | Guardian of process integrity, 20 operating rules | Controller (State Engine) |
| **11** | **Security Auditor** | [11_Security](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/11_Security/AGENT-SPEC.md) | Threat modeling, zero trust, token security, AES-256 | Governance (SecOps) |
| **12** | **QA Specialist** | [10_QA](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/10_QA/AGENT-SPEC.md) | Screen user journey acceptance, quality gates | Governance (Quality) |
| **13** | **Test Engineer** | [20_Testing](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/20_Testing/AGENT-SPEC.md) | Unit, integration, and E2E automated test suites | Governance (Validation) |
| **14** | **Audit & Inspector** | [17_Audit](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/17_Audit/AGENT-SPEC.md) | 5-pillar checklist verification, HITL gates, zero-orphan sign-off | Governance (Audit & Inspection) |
| **15** | **Optimization Agent** | [21_Optimization](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/21_Optimization/AGENT-SPEC.md) | Profiling, 60 FPS UI, low memory, Web Vitals | Controller (Performance) |
| **16** | **Cost Optimization** | [13_Cost_Optimization](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/13_Cost_Optimization/AGENT-SPEC.md) | Token budgets, compute ROI, resource efficiency | Governance (Finance) |
| **17** | **AI Agent Manager** | [09_AI_Agent_Manager](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/09_AI_Agent_Manager/AGENT-SPEC.md) | Subagent orchestration, runtime health, concurrency | Governance (Agentic) |
| **18** | **Prompt Engineering** | [08_Prompt_Engineering](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/08_Prompt_Engineering/AGENT-SPEC.md) | LLM prompts, few-shot tuning, token efficiency | Controller (Cognitive) |
| **19** | **Local Model Agent** | [`ollama_client_model.py`](file:///c:/Users/jiten/jAnitGravity/.kit/ollama_client_model.py) | Offline autocompletion, local inference, air-gapped processing | Controller (Local AI) |
| **20** | **NodeJS Server Agent** | [24_NodeJS_Server](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/24_NodeJS_Server/AGENT-SPEC.md) | Holistic port management, multi-tenant gateway, route/heading duty segregation | Controller (Runtime Gateway) |
| **21** | **Deployment Agent** | [22_Deployment](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/22_Deployment/AGENT-SPEC.md) | CI/CD packaging, containerization, production rollout | Controller (DevOps) |
| **22** | **Publishing Agent** | [14_Publishing](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/14_Publishing/AGENT-SPEC.md) | Distribution packaging, release deployment, store assets | View / Release |
| **23** | **Monitoring Agent** | [12_Monitoring](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/12_Monitoring/AGENT-SPEC.md) | Telemetry, liveness, heartbeats, error tracking | Controller (Telemetry) |
| **24** | **Analytics Agent** | [15_Analytics](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/15_Analytics/AGENT-SPEC.md) | Funnel metrics, telemetry insights, KPI feedback | Model (Insights) |

---

## 3. Governance Protocol & Closed-Loop Execution Rules

1. **Governance Sequence**: Execution strictly transitions through the Screen-First Outside-In lifecycle:
   `Orchestrator ➔ UI/UX ➔ Frontend ➔ Business Analyst ➔ Enterprise Architect ➔ Backend ➔ Database ➔ Dependency ➔ Integration ➔ Workflow ➔ Security ➔ QA & Testing ➔ Optimization ➔ NodeJS Gateway & Deploy ➔ Audit & Telemetry`.
2. **Pre-Flight Check**: Prior to any code change, the **Dependency Agent** and **Integration Agent** verify variables, constants, dependencies, and integration points against [PROGRAM-MAPPING.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/PROGRAM-MAPPING.md).
3. **Closed-Loop Feedback**: If any verification or testing gate fails, an error package is dispatched back to the responsible agent. Work resumes only after defect remediation.
4. **Zero Orphan Rule**: The **Audit Agent** must verify that every generated class, file, or service has a declared purpose, valid import links, and usage references before final sign-off.

---

## 3. Governance Protocol & Closed-Loop Execution Rules

1. **Governance Sequence**: Execution must strictly transition through the sequential waterfall gates:
   `Architect → Frontend → Backend → Database → Dependency → Integration → Workflow → Security → UI/UX → Testing → Optimization → Deployment → Audit`.
2. **Pre-Flight Check**: Prior to any code change, the **Dependency Agent** and **Integration Agent** verify variables, constants, dependencies, and integration points against [PROGRAM-MAPPING.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/PROGRAM-MAPPING.md).
3. **Closed-Loop Feedback**: If any verification or testing gate fails, an error package is dispatched back to the responsible agent. Work resumes only after defect remediation.
4. **Zero Orphan Rule**: The **Audit Agent** must verify that every generated class, file, or service has a declared purpose, valid import links, and usage references before final sign-off.
