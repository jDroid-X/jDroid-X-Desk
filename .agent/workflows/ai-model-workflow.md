# AI Model Workflow Strategy (Gemini 3.x Multi-Model Enterprise Standard)

**VERSION 2.0**  
**Classification:** Enterprise Operating System AI Governance  
**Supported Stack:** Antigravity Gemini 3.x Series (Gemini 3.8, 3.7, 3.6, 3.1) across Thinking Levels (Low, Medium, High)  

---

## 1. Antigravity Gemini 3.x Architecture Overview

This workflow defines the runtime delegation engine and token-budget strategy across the Antigravity Gemini 3.x model family. Models are dynamically selected based on task complexity, cognitive reasoning requirements, and strict token efficiency.

### Available Gemini 3.x Model Stack

| Model Tier | Primary Capabilities | Thinking Level Options | Token Economy Rate | Optimal Agent / Task Domain |
| :--- | :--- | :--- | :--- | :--- |
| **Gemini 3.8** | State-of-the-Art Deep Reasoning, Multi-agent Architecture, Root-cause Analysis, Full-stack Core Implementation | **High**, **Medium**, **Low** | Premium (High cognitive yield) | Master Orchestrator, Enterprise Architect, Security Agent, Audit Agent, Core Backend/Frontend |
| **Gemini 3.7** | Hybrid Reasoning & Production Speed, Balanced Code Generation, AST Refactoring, UI/UX Design Token Systems | **High**, **Medium**, **Low** | Moderate (High speed & reliability) | Business Analyst, Database Architect, Prompt Engineer, Optimization Agent, Integration Agent |
| **Gemini 3.6** | High-Throughput Engineering, Deterministic Code Tasks, Automated Test Suites, Cost Profiling, Data Analytics | **Medium**, **Low** | Low (Token-saving workhorse) | Testing Agent, Cost Optimization Agent, Analytics Agent, Publishing Agent |
| **Gemini 3.1** | Ultra-Fast Execution, Simple File Ops, Real-time Telemetry Parsing, Health Heartbeats, Markdown Formatting | **Low** | Micro (Minimal token footprint) | Monitoring Agent, Heartbeat Monitors, File Formatters, Telemetry Scrapers |

---

## 2. Dynamic Model Delegation Engine (Agent Orchestrator Matrix)

When a prompt or task is dispatched, the **Master Orchestrator Agent** assigns work to specialized subagents according to this classification matrix:

| Task Complexity & Context | Recommended Model & Thinking | Assigned Enterprise Agent(s) | Expected Token Budget |
| :--- | :--- | :--- | :--- |
| **Strategic Planning & Intent Classification** | **Gemini 3.8 (High)** | Master Orchestrator (01) | ~1,500 - 3,500 tokens |
| **System Architecture & Interface Contracts** | **Gemini 3.8 (High)** | Enterprise Architect (03), Security Agent (11) | ~3,000 - 6,000 tokens |
| **Core Full-Stack Implementation (OOPS-MVC)** | **Gemini 3.8 (Medium)** | Backend Agent (05), Frontend Agent (06) | ~2,500 - 5,000 tokens |
| **Database Schema, Storage Sandbox & Migrations**| **Gemini 3.7 (Medium)** | Database Architect (04) | ~1,500 - 3,000 tokens |
| **UI/UX Design Systems & CSS Token Symmetry** | **Gemini 3.7 (Medium)** | UI/UX Agent (18) | ~1,500 - 3,000 tokens |
| **API Adapters, Connectors & SDK Integrations** | **Gemini 3.7 (Medium)** | Integration Agent (19) | ~1,500 - 3,000 tokens |
| **Prompt Engineering & Cognitive Anchoring** | **Gemini 3.7 (High)** | Prompt Engineering Agent (08) | ~2,000 - 4,000 tokens |
| **Automated Testing, Unit Tests & Regression** | **Gemini 3.6 (Medium)** | Testing Agent (20) | ~1,000 - 2,500 tokens |
| **Comprehensive Audit, Compliance & Code Sign-off**| **Gemini 3.8 (High)** | Audit & Inspector Agent (17) | ~2,500 - 5,000 tokens |
| **Workflow State Machine & Rule Verification** | **Gemini 3.7 (Low)** | Workflow Agent (07) | ~800 - 1,500 tokens |
| **Dependency Mapping & CVE Verification** | **Gemini 3.7 (Low)** | Dependency Agent (16) | ~800 - 1,500 tokens |
| **Token Auditing & Cost Optimization** | **Gemini 3.6 (Low)** | Cost Optimization Agent (13) | ~600 - 1,200 tokens |
| **Release Bundling & Distribution Manifests** | **Gemini 3.6 (Low)** | Publishing Agent (14) | ~600 - 1,200 tokens |
| **Runtime Telemetry Parsing & Health Checks** | **Gemini 3.1 (Low)** | Monitoring Agent (12) | ~300 - 800 tokens |

---

## 3. Host Session Model vs. Subagent Delegation Architecture

### Critical Operational Rule:
1. **Host Chat Model Selection:** The model dropdown in the Antigravity chat UI sets the **Host Session Model** (the environment orchestrator).
2. **Recommended Host Default:** Keep the chat tool on **Gemini 3.8 (Medium)**. This provides the universal sweet spot for high-level conversation, tool coordination, and fast turnarounds without burning excessive thinking tokens on casual queries.
3. **Internal Subagent Delegation:** When the Orchestrator spawns subagents or background tasks, it directs specialized tasks using the assigned Gemini 3.x model tier and thinking level.
4. **Manual Switching Rule:** If you are embarking on a massive multi-file architecture refactor or deep debugging across 10+ interdependent modules, manually switch the chat dropdown to **Gemini 3.8 (High)** for maximum cognitive power. For lightweight maintenance or documentation review, switch to **Gemini 3.7 (Medium)** or **Gemini 3.6 (Medium)**.

---

## 4. Token Conservation Directives (Zero-Waste Protocol)

To eliminate token waste and guarantee zero-redo execution:
1. **Pre-Flight Declaration First:** Every code-changing agent must declare variables, constants, dependencies, and integration points *before* writing code. This eliminates syntax breaks and unresolved reference loops.
2. **AST Diffs Over Full Rewrites:** Agents must emit targeted line-range diffs (
eplace_file_content / multi_replace_file_content) instead of full-file reprints.
3. **Call-by-Reference Code Reuse:** Avoid duplicate algorithms across different modules; call shared utility methods by reference.
4. **Closed-Loop Defect Routing:** On test failure, Testing Agent sends a structured defect ticket (with exact file, line, and failing assertion) to the responsible agent rather than asking the user to re-explain the prompt.

---

## 5. Fallback Sequence (Quota Depletion Protocol)

If quota exhaustion occurs during intensive development:
1. **Tier 1 Fallback:** If Gemini 3.8 (High) hits rate limits, route immediately to Gemini 3.7 (High) with zero interruption.
2. **Tier 2 Fallback:** For testing and verification, fallback from Gemini 3.6 to Gemini 3.1 (Low).
3. **Session State Retention:** All conversation context and program mapping states are preserved in .agent memory, preventing context loss upon quota refresh.
