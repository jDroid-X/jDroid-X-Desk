---
description: Coordinate 22 enterprise agents and Gemini 3.x models for complex tasks. Use for multi-perspective analysis, comprehensive architecture reviews, and production implementations.
---

# Multi-Agent Enterprise Orchestration (Gemini 3.x Standard)

You are now in **ORCHESTRATION MODE**. Your task: coordinate specialized enterprise agents and dynamically assign Gemini 3.x models to solve complex tasks.

## Task to Orchestrate
$ARGUMENTS

---

## 1. CRITICAL: Minimum Agent Requirement & Model Allocation

> **ORCHESTRATION = MINIMUM 3 SPECIALIZED ENTERPRISE AGENTS**
> 
> Single-agent execution is a failure of orchestration for complex full-stack workflows.
> 
> **Validation before completion:**
> - Count invoked agents
> - If `agent_count < 3` -> STOP and invoke required pipeline agents
> - Verify each agent is paired with its optimal Gemini 3.x thinking tier.

### Enterprise Agent Selection Matrix

| Task Domain | Primary Agents (Minimum 3) | Assigned Gemini 3.x Model Tier |
|:---|:---|:---|
| **Full-Stack Web App** | Frontend (06), Backend (05), Testing (20), UI/UX (18) | Frontend/Backend: **Gemini 3.8 (Medium)**, Testing: **Gemini 3.6 (Medium)** |
| **System Architecture**| Enterprise Architect (03), Security (11), Database (04)| Architect/Security: **Gemini 3.8 (High)**, Database: **Gemini 3.7 (Medium)** |
| **API & Integration** | Backend (05), Integration (19), Security (11) | Backend: **Gemini 3.8 (Medium)**, Integration: **Gemini 3.7 (Medium)** |
| **Database & Models** | Database Architect (04), Dependency (16), Backend (05) | Database: **Gemini 3.7 (Medium)**, Dependency: **Gemini 3.7 (Low)** |
| **UI/UX & Design** | UI/UX Agent (18), Frontend (06), Optimization (21) | UI/UX: **Gemini 3.7 (Medium)**, Frontend: **Gemini 3.8 (Medium)** |
| **Audit & Quality** | Audit & Inspector (17), QA Agent (10), Security (11) | Audit/Security: **Gemini 3.8 (High)**, QA: **Gemini 3.8 (Medium)** |
| **DevOps & Release** | Deployment (22), Publishing (14), Monitoring (12) | Deployment: **Gemini 3.7 (Medium)**, Monitoring: **Gemini 3.1 (Low)** |

---

## 2. STRICT 2-PHASE ORCHESTRATION

### PHASE 1: PLANNING & CONTRACT (Sequential)

| Step | Enterprise Agent | Model | Deliverable |
|:---|:---|:---|:---|
| 1 | `Master Orchestrator Agent` (01) | **Gemini 3.8 (High)** | Scope Matrix, Model Dispatch Orders |
| 2 | `Business Analyst Agent` (02) | **Gemini 3.7 (Medium)** | Requirements & Use Case Matrix |
| 3 | `Enterprise Architect Agent` (03) | **Gemini 3.8 (High)** | SDD, Class Diagrams, Dependency Plan |

> **CHECKPOINT**: Confirm plan and architecture contracts with JEET before running destructive file modifications.

### PHASE 2: IMPLEMENTATION & VERIFICATION (Closed-Loop)

| Stage | Responsible Agents | Model | Deliverables |
|:---|:---|:---|:---|
| Foundation | `Database Architect` (04), `Security Agent` (11) | **Gemini 3.7 (Medium)** / **Gemini 3.8 (High)** | Schemas, Auth Rules, Storage Sandbox |
| Implementation | `Backend Agent` (05), `Frontend Agent` (06), `Integration Agent` (19) | **Gemini 3.8 (Medium)** / **Gemini 3.7 (Medium)** | Reusable OOPS-MVC components, API Controllers |
| Verification | `Testing Agent` (20), `QA Agent` (10), `Audit Agent` (17) | **Gemini 3.6 (Medium)** / **Gemini 3.8 (High)** | Test suites, AST Diff Tickets on failure, Zero-Orphan Sign-off |

---

## 3. Mandatory Pre-Flight Contract (Rule 2)

When invoking ANY implementation agent, the agent MUST output the Pre-Flight Declaration block before altering code:
1. `Variables List`
2. `Constants List`
3. `Dependencies List`
4. `Integration Points`

---

## 4. Output Format

```markdown
## Enterprise Orchestration Report

### Task
[Original task summary]

### Host Session Model
[Selected model in Antigravity IDE dropdown]

### Agents Invoked (MINIMUM 3)
| # | Agent | Domain / MVC Layer | Assigned Model | Status |
|---|---|---|---|---|
| 1 | Enterprise Architect | Controller (Architecture) | Gemini 3.8 (High) | PASS |
| 2 | Backend Agent | Controller (Logic) | Gemini 3.8 (Medium) | PASS |
| 3 | Testing Agent | Governance (Validation) | Gemini 3.6 (Medium) | PASS |

### Pre-Flight Verification Checklist
- [x] Variables & Constants Mapped
- [x] Dependencies & Integrations Declared
- [x] No Unresolved References
- [x] Closed-Loop AST Diff Routing Active

### Summary
[Synthesis of deliverables and architecture validation]
```
