# Antigravity Kit Architecture

> Comprehensive AI Agent Capability Expansion Toolkit

---
## 📋 Overview

Antigravity Kit is a modular system consisting of:

- **20 Specialist Agents** - Role-based AI personas
- **36 Skills** - Domain-specific knowledge modules
- **11 Workflows** - Slash command procedures
---

## 🏗️ Directory Structure

```plaintext
.agent/
├── ARCHITECTURE.md          # This file
├── agents/                  # 20 Specialist Agents
├── skills/                  # 36 Skills
├── workflows/               # 11 Slash Commands
├── rules/                   # Global Rules
└── scripts/                 # Master Validation Scripts
```

---

## 🤖 Agents (20)

Specialist AI personas for different domains.

| Agent                    | Focus                      | Skills Used                                              |
| ------------------------ | -------------------------- | -------------------------------------------------------- |
| `orchestrator`           | Multi-agent coordination   | 06_Core/AI/parallel-agents, 08_Feat/Logic/behavioral-modes                        |
| `project-planner`        | Discovery, task planning   | 06_Core/AI/brainstorming, 01_Reqs/Content/plan-writing, 02_Arch/SysDesign/architecture                |
| `frontend-specialist`    | Web UI/UX                  | 07_UI/Web/frontend-design, 07_UI/Web/react-best-practices, 06_Core/AI/tailwind-patterns |
| `backend-specialist`     | API, business logic        | 05_API/REST/api-patterns, 08_Feat/Logic/nodejs-best-practices, 04_DB/SQL/database-design     |
| `04_DB/SQL/database-architect`     | Schema, SQL                | 04_DB/SQL/database-design, 04_DB/SQL/prisma-expert                           |
| `07_UI/Mobile/mobile-developer`       | iOS, Android, RN           | 07_UI/Web/mobile-design                                            |
| `game-developer`         | Game logic, mechanics      | 08_Feat/Logic/game-08_Feat/Logic/development                                         |
| `devops-engineer`        | CI/CD, Docker              | 12_Deploy/CICD/deployment-procedures, 03_Deps/Docker/docker-expert                     |
| `10_Test/Pentest/security-auditor`       | Security compliance        | 10_Test/Pentest/vulnerability-scanner, 10_Test/RedTeam/red-team-tactics                  |
| `penetration-tester`     | Offensive security         | 10_Test/RedTeam/red-team-tactics                                         |
| `test-engineer`          | Testing strategies         | 10_Test/Unit/testing-patterns, 10_Test/Unit/tdd-workflow, 07_UI/Web/webapp-testing           |
| `10_Test/Unit/debugger`               | Root cause analysis        | 10_Test/Unit/systematic-debugging                                     |
| `11_Opt/Perf/performance-optimizer`  | Speed, Web Vitals          | 11_Opt/Perf/performance-profiling                                    |
| `seo-specialist`         | Ranking, visibility        | 11_Opt/SEO/seo-fundamentals, 11_Opt/SEO/geo-fundamentals                       |
| `documentation-writer`   | Manuals, 01_Reqs/Content/docs              | 01_Reqs/Content/documentation-templates                                  |
| `01_Reqs/Business/product-manager`        | Requirements, user stories | 01_Reqs/Content/plan-writing, 06_Core/AI/brainstorming                              |
| `product-owner`          | Strategy, backlog, MVP     | 01_Reqs/Content/plan-writing, 06_Core/AI/brainstorming                              |
| `qa-automation-engineer` | E2E testing, CI pipelines  | 07_UI/Web/webapp-testing, 10_Test/Unit/testing-patterns                         |
| `code-archaeologist`     | Legacy code, refactoring   | 08_Feat/Logic/clean-code, 10_Test/Unit/code-review-checklist                        |
| `explorer-agent`         | Codebase analysis          | -                                                        |

---

## 🧩 Skills

All skills have been refactored into the 12-stage Waterfall structure. See the complete [SKILL_INDEX.md](file:///c:/Users/dell/jAnitGravity/.kit/.agent/SKILL_INDEX.md) for alias mapping.

## 🧩 Skills (36)

Modular knowledge domains that agents can load on-demand. based on task context.

### Frontend & UI

| Skill                   | Description                                                           |
| ----------------------- | --------------------------------------------------------------------- |
| `07_UI/Web/react-best-practices`  | React & Next.js performance optimization (Vercel - 57 rules)          |
| `07_UI/Web/web-design-guidelines` | Web UI audit - 100+ rules for accessibility, UX, performance (Vercel) |
| `06_Core/AI/tailwind-patterns`     | Tailwind CSS v4 utilities                                             |
| `07_UI/Web/frontend-design`       | UI/UX patterns, design systems                                        |
| `07_UI/Web/ui-ux-pro-max`         | 50 styles, 21 palettes, 50 fonts                                      |

### Backend & API

| Skill                   | Description                    |
| ----------------------- | ------------------------------ |
| `05_API/REST/api-patterns`          | REST, GraphQL, tRPC            |
| `05_API/REST/nestjs-expert`         | NestJS modules, DI, decorators |
| `08_Feat/Logic/nodejs-best-practices` | Node.js async, modules         |
| `08_Feat/Logic/python-patterns`       | Python standards, FastAPI      |

### Database

| Skill             | Description                 |
| ----------------- | --------------------------- |
| `04_DB/SQL/database-design` | Schema design, optimization |
| `04_DB/SQL/prisma-expert`   | Prisma ORM, migrations      |

### TypeScript/JavaScript

| Skill               | Description                         |
| ------------------- | ----------------------------------- |
| `01_Reqs/Content/typescript-expert` | Type-level programming, performance |

### Cloud & Infrastructure

| Skill                   | Description               |
| ----------------------- | ------------------------- |
| `03_Deps/Docker/docker-expert`         | Containerization, Compose |
| `12_Deploy/CICD/deployment-procedures` | CI/CD, deploy workflows   |
| `03_Deps/PkgMgmt/server-management`     | Infrastructure management |

### Testing & Quality

| Skill                   | Description              |
| ----------------------- | ------------------------ |
| `10_Test/Unit/testing-patterns`      | Jest, Vitest, strategies |
| `07_UI/Web/webapp-testing`        | E2E, Playwright          |
| `10_Test/Unit/tdd-workflow`          | Test-driven 08_Feat/Logic/development  |
| `10_Test/Unit/code-review-checklist` | Code review standards    |
| `10_Test/Unit/lint-and-validate`     | Linting, validation      |

### Security

| Skill                   | Description              |
| ----------------------- | ------------------------ |
| `10_Test/Pentest/vulnerability-scanner` | Security auditing, OWASP |
| `10_Test/RedTeam/red-team-tactics`      | Offensive security       |

### Architecture & Planning

| Skill           | Description                |
| --------------- | -------------------------- |
| `07_UI/Web/app-builder`   | Full-stack app scaffolding |
| `02_Arch/SysDesign/architecture`  | System design patterns     |
| `01_Reqs/Content/plan-writing`  | Task planning, breakdown   |
| `06_Core/AI/brainstorming` | Socratic questioning       |

### Mobile

| Skill           | Description           |
| --------------- | --------------------- |
| `07_UI/Web/mobile-design` | Mobile UI/UX patterns |

### Game Development

| Skill              | Description           |
| ------------------ | --------------------- |
| `08_Feat/Logic/game-08_Feat/Logic/development` | Game logic, mechanics |

### SEO & Growth

| Skill              | Description                   |
| ------------------ | ----------------------------- |
| `11_Opt/SEO/seo-fundamentals` | SEO, E-E-A-T, Core Web Vitals |
| `11_Opt/SEO/geo-fundamentals` | GenAI optimization            |

### Shell/CLI

| Skill                | Description               |
| -------------------- | ------------------------- |
| `07_UI/Web/bash-linux`         | Linux commands, scripting |
| `03_Deps/EnvSetup/powershell-windows` | Windows PowerShell        |

### Other

| Skill                     | Description               |
| ------------------------- | ------------------------- |
| `08_Feat/Logic/clean-code`              | Coding standards (Global) |
| `08_Feat/Logic/behavioral-modes`        | Agent personas            |
| `06_Core/AI/parallel-agents`         | Multi-agent patterns      |
| `07_UI/Web/mcp-builder`             | Model Context Protocol    |
| `01_Reqs/Content/documentation-templates` | Doc formats               |
| `07_UI/Web/i18n-localization`       | Internationalization      |
| `11_Opt/Perf/performance-profiling`   | Web Vitals, optimization  |
| `10_Test/Unit/systematic-debugging`    | Troubleshooting           |

---

## 🔄 Workflows (11)

Slash command procedures. Invoke with `/command`.

| Command          | Description              |
| ---------------- | ------------------------ |
| `/brainstorm`    | Socratic discovery       |
| `/create`        | Create new features      |
| `/debug`         | Debug 12_Deploy/CICD/issues             |
| `/deploy`        | Deploy application       |
| `/enhance`       | Improve existing code    |
| `/orchestrate`   | Multi-agent coordination |
| `/plan`          | Task breakdown           |
| `/preview`       | Preview changes          |
| `/status`        | Check project status     |
| `/test`          | Run tests                |
| `/07_UI/Web/ui-ux-pro-max` | Design with 50 styles    |

---

## 🎯 Skill Loading Protocol

```plaintext
User Request → Skill Description Match → Load SKILL.md
                                            ↓
                                    Read references/
                                            ↓
                                    Read scripts/
```

### Skill Structure

```plaintext
skill-name/
├── SKILL.md           # (Required) Metadata & instructions
├── scripts/           # (Optional) Python/Bash scripts
├── references/        # (Optional) Templates, 01_Reqs/Content/docs
└── assets/            # (Optional) Images, logos
```

### Enhanced Skills (with scripts/references)

| Skill               | Files | Coverage                            |
| ------------------- | ----- | ----------------------------------- |
| `07_UI/Web/ui-ux-pro-max`     | 27    | 50 styles, 21 palettes, 50 fonts    |
| `07_UI/Web/app-builder`       | 20    | Full-stack scaffolding              |

---

## � Scripts (2)

Master validation scripts that orchestrate skill-level scripts.

### Master Scripts

| Script          | Purpose                                 | When to Use              |
| --------------- | --------------------------------------- | ------------------------ |
| `checklist.py`  | Priority-based validation (Core checks) | Development, pre-12_Deploy/CICD/commit  |
| `verify_all.py` | Comprehensive verification (All checks) | Pre-deployment, releases |

### Usage

```bash
# Quick validation during 08_Feat/Logic/development
python .agent/scripts/checklist.py .

# Full verification before deployment
python .agent/scripts/verify_all.py . --url http://localhost:3000
```

### What They Check

**checklist.py** (Core checks):

- Security (vulnerabilities, secrets)
- Code Quality (lint, types)
- Schema Validation
- Test Suite
- UX Audit
- SEO Check

**verify_all.py** (Full suite):

- Everything in checklist.py PLUS:
- Lighthouse (Core Web Vitals)
- Playwright E2E
- Bundle Analysis
- Mobile Audit
- i18n Check

For details, see [scripts/README.md](scripts/README.md)

---

## 📊 Statistics

| Metric              | Value                         |
| ------------------- | ----------------------------- |
| **Total Agents**    | 20                            |
| **Total Skills**    | 36                            |
| **Total Workflows** | 11                            |
| **Total Scripts**   | 2 (master) + 18 (skill-level) |
| **Coverage**        | ~90% web/mobile 08_Feat/Logic/development   |

---

## 🔗 Quick Reference

| Need     | Agent                 | Skills                                |
| -------- | --------------------- | ------------------------------------- |
| Web App  | `frontend-specialist` | 07_UI/Web/react-best-practices, 07_UI/Web/frontend-design |
| API      | `backend-specialist`  | 05_API/REST/api-patterns, 08_Feat/Logic/nodejs-best-practices   |
| Mobile   | `07_UI/Mobile/mobile-developer`    | 07_UI/Web/mobile-design                         |
| Database | `04_DB/SQL/database-architect`  | 04_DB/SQL/database-design, 04_DB/SQL/prisma-expert        |
| Security | `10_Test/Pentest/security-auditor`    | 10_Test/Pentest/vulnerability-scanner                 |
| Testing  | `test-engineer`       | 10_Test/Unit/testing-patterns, 07_UI/Web/webapp-testing      |
| Debug    | `10_Test/Unit/debugger`            | 10_Test/Unit/systematic-debugging                  |
| Plan     | `project-planner`     | 06_Core/AI/brainstorming, 01_Reqs/Content/plan-writing           |
