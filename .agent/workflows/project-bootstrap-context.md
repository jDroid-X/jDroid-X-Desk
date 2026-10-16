# Project Bootstrap & Context Initialization Workflow

## Purpose
Execute the mandatory **Phase 0 Protocol (Tier P1)** for any new project, workspace, or session to establish complete context, map dependencies, enforce OOPS MVC standards, and configure token hygiene before coding begins.

---

## 4-Stage Waterfall Sequence (Phase 0)

```
[Stage 0.1: READ] -> Ingest Directory, Configs, Schemas, AGENTS.md, GEMINI.md
      ↓
[Stage 0.2: UNDERSTAND] -> Program Mapping, Routes, Models, Views, Data Flows
      ↓
[Stage 0.3: USE] -> Apply Dynamic OOPS MVC, Ponytail Decision Ladder, 7-Stage Waterfall
      ↓
[Stage 0.4: SET] -> Configure Token Audit Guardrails, Noise Hooks, requirement/*.txt
```

---

### Step 0.1: READ (Context Discovery)
- Run `list_dir` on workspace root.
- Read `package.json`, `AGENTS.md`, `GEMINI.md`, and database configuration.
- Check documentation in `Docs/` and rule files in `.agents/rules/`.
- **Branch Check:** If project files are uninitialized -> Scaffold base structure and proceed.

### Step 0.2: UNDERSTAND (Dependency & Program Mapping)
- Identify all Controllers, Models, Views, and Services.
- Map relationships into the Program Mapping master table.
- Verify API contracts and database schema integrity.

### Step 0.3: USE (Architecture Alignment)
- Enforce the 7-Stage Waterfall closed-loop lifecycle.
- Apply the Ponytail Decision Ladder (reuse existing logic and standard libraries first).
- Verify zero unresolved references and zero duplicate logic.

### Step 0.4: SET (Token Hygiene & Tracking Setup)
- Execute Token Audit: ensure rule files are < 5k tokens each and total prompt < 10k tokens.
- Configure command noise-suppression rules.
- Initialize screen-based change tracking files in `requirement/`.

---

### Closed-Loop Convergence
- **Success:** Context fully established -> Transition to Stage 1 (Initiate & Plan).
- **Missing Information:** Prompt user with specific discovery questions -> Ingest answers -> Transition to Stage 1.
- **Circuit Breaker:** Max 2 discovery cycles; then initialize with default safe enterprise template.
