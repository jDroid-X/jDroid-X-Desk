# Antigravity Workflows

> Workflow playbooks to orchestrate multiple skills with less friction.

## What Is a Workflow?

A workflow is a guided, step-by-step execution path that combines multiple skills for one concrete outcome.

- **Bundles** tell you which skills are relevant for a role.
- **Workflows** tell you how to use those skills in sequence to complete a real objective.

If bundles are your toolbox, workflows are your execution playbook.

---
## How to Use Workflows

1. Install the repository once (`npx antigravity-awesome-skills`).
2. Pick a workflow matching your immediate goal.
3. Execute steps in order and invoke the listed skills in each step.
4. Keep output artifacts at each step (plan, decisions, tests, validation evidence).

You can combine workflows with bundles from [bundles.md](bundles.md) when you need broader coverage.
---

## Workflow: Ship a SaaS MVP

Build and ship a minimal but production-minded SaaS product.

**Related bundles:** `Essentials`, `Full-Stack Developer`, `QA & Testing`, `DevOps & Cloud`

### Prerequisites

- Local repository and runtime configured.
- Clear user problem and MVP scope.
- Basic deployment target selected.

### Steps

1. **Plan the scope**
   - **Goal:** Define MVP boundaries and acceptance criteria.
   - **Skills:** [`@brainstorming`](../../skills/06_Core/AI/brainstorming/), [`@concise-planning`](../../skills/12_Deploy/CICD/concise-planning/), [`@writing-plans`](../../skills/01_Reqs/Content/writing-plans/)
   - **Prompt example:** `Use @concise-planning to define milestones and acceptance criteria for my SaaS MVP.`

2. **Build backend and API**
   - **Goal:** Implement core entities, APIs, and auth baseline.
   - **Skills:** [`@backend-dev-guidelines`](../../skills/07_UI/Web/backend-dev-guidelines/), [`@api-patterns`](../../skills/05_API/REST/api-patterns/), [`@database-design`](../../skills/04_DB/SQL/database-design/)
   - **Prompt example:** `Use @backend-dev-guidelines to create APIs and services for the billing domain.`

3. **Build frontend**
   - **Goal:** Ship core user flow with clear UX states.
   - **Skills:** [`@frontend-developer`](../../skills/07_UI/Web/frontend-developer/), [`@react-patterns`](../../skills/07_UI/Web/react-patterns/), [`@frontend-design`](../../skills/07_UI/Web/frontend-design/)
   - **Prompt example:** `Use @frontend-developer to implement onboarding, the empty state, and the initial dashboard.`

4. **Test and validate**
   - **Goal:** Cover critical user journeys before release.
   - **Skills:** [`@test-driven-development`](../../skills/08_Feat/Logic/test-driven-development/), [`@browser-automation`](../../skills/06_Core/Auto/browser-automation/), `@go-playwright` (optional, Go stack)
   - **Prompt example:** `Use @browser-automation to create E2E tests for the signup and checkout flows.`
   - **Go note:** If the QA project and tooling are in Go, prefer `@go-playwright`.

5. **Ship safely**
   - **Goal:** Release with observability and rollback plan.
   - **Skills:** [`@deployment-procedures`](../../skills/12_Deploy/CICD/deployment-procedures/), [`@observability-engineer`](../../skills/12_Deploy/CICD/observability-engineer/)
   - **Prompt example:** `Use @deployment-procedures for a release checklist with rollback steps.`

---

## Workflow: Security Audit for a Web App

Run a focused security review from scope definition to remediation validation.

**Related bundles:** `Security Engineer`, `Security Developer`, `Observability & Monitoring`

### Prerequisites

- Explicit authorization for testing.
- In-scope targets documented.
- Logging and environment details available.

### Steps

1. **Define scope and threat model**
   - **Goal:** Identify assets, trust boundaries, and attack paths.
   - **Skills:** [`@ethical-hacking-methodology`](../../skills/10_Test/Pentest/ethical-hacking-methodology/), [`@threat-modeling-expert`](../../skills/02_Arch/Threat/threat-modeling-expert/), [`@attack-tree-construction`](../../skills/02_Arch/Threat/attack-tree-construction/)
   - **Prompt example:** `Use @threat-modeling-expert to map critical assets and trust boundaries for my web app.`

2. **Review auth and access control**
   - **Goal:** Detect account takeover and authorization flaws.
   - **Skills:** [`@broken-authentication`](../../skills/05_API/Auth/broken-authentication/), [`@auth-implementation-patterns`](../../skills/05_API/Auth/auth-implementation-patterns/), [`@idor-testing`](../../skills/10_Test/Unit/idor-testing/)
   - **Prompt example:** `Use @idor-testing to verify unauthorized access on multitenant endpoints.`

3. **Assess API and input security**
   - **Goal:** Uncover high-impact API and injection vulnerabilities.
   - **Skills:** [`@api-security-best-practices`](../../skills/05_API/REST/api-security-best-practices/), [`@api-fuzzing-bug-bounty`](../../skills/05_API/REST/api-fuzzing-bug-bounty/), [`@top-web-vulnerabilities`](../../skills/07_UI/Web/top-web-vulnerabilities/)
   - **Prompt example:** `Use @api-security-best-practices to audit auth, billing, and admin endpoints.`

4. **Harden and verify**
   - **Goal:** Convert findings into fixes and verify evidence of mitigation.
   - **Skills:** [`@security-auditor`](../../skills/10_Test/Pentest/security-auditor/), [`@sast-configuration`](../../skills/03_Deps/EnvSetup/sast-configuration/), [`@verification-before-completion`](../../skills/10_Test/Unit/verification-before-completion/)
   - **Prompt example:** `Use @verification-before-completion to prove that the mitigations are effective.`

---

## Workflow: Build an AI Agent System

Design and deliver a production-grade agent with measurable reliability.

**Related bundles:** `Agent Architect`, `LLM Application Developer`, `Data Engineering`

### Prerequisites

- Narrow use case with measurable outcomes.
- Access to model provider(s) and observability tooling.
- Initial dataset or knowledge corpus.

### Steps

1. **Define target behavior and KPIs**
   - **Goal:** Set quality, latency, and failure thresholds.
   - **Skills:** [`@ai-agents-architect`](../../skills/06_Core/AI/ai-agents-architect/), [`@agent-evaluation`](../../skills/06_Core/AI/agent-evaluation/), [`@product-manager-toolkit`](../../skills/01_Reqs/Business/product-manager-toolkit/)
   - **Prompt example:** `Use @agent-evaluation to define benchmarks and success criteria for my agent.`

2. **Design retrieval and memory**
   - **Goal:** Build reliable retrieval and context architecture.
   - **Skills:** [`@llm-app-patterns`](../../skills/06_Core/AI/llm-app-patterns/), [`@rag-implementation`](../../skills/04_DB/Vector/rag-implementation/), [`@vector-database-engineer`](../../skills/04_DB/SQL/vector-database-engineer/)
   - **Prompt example:** `Use @rag-implementation to design chunking, embedding, and retrieval pipelines.`

3. **Implement orchestration**
   - **Goal:** Implement deterministic orchestration and tool boundaries.
   - **Skills:** [`@langgraph`](../../skills/06_Core/AI/langgraph/), [`@mcp-builder`](../../skills/07_UI/Web/mcp-builder/), [`@workflow-automation`](../../skills/06_Core/Auto/workflow-automation/)
   - **Prompt example:** `Use @langgraph to implement the agent graph with fallbacks and human-in-the-loop flows.`

4. **Evaluate and iterate**
   - **Goal:** Improve weak points with a structured loop.
   - **Skills:** [`@agent-evaluation`](../../skills/06_Core/AI/agent-evaluation/), [`@langfuse`](../../skills/06_Core/AI/langfuse/), [`@kaizen`](../../skills/06_Core/AI/kaizen/)
   - **Prompt example:** `Use @kaizen to prioritize fixes for the failure modes identified by testing.`

---

## Workflow: QA and Browser Automation

Create resilient browser automation with deterministic execution in CI.

**Related bundles:** `QA & Testing`, `Full-Stack Developer`

### Prerequisites

- Test environments and stable credentials.
- Critical user journeys identified.
- CI pipeline available.

### Steps

1. **Prepare test strategy**
   - **Goal:** Scope journeys, fixtures, and execution environments.
   - **Skills:** [`@e2e-testing-patterns`](../../skills/10_Test/E2E/e2e-testing-patterns/), [`@test-driven-development`](../../skills/08_Feat/Logic/test-driven-development/)
   - **Prompt example:** `Use @e2e-testing-patterns to define a minimal but high-impact E2E suite.`

2. **Implement browser tests**
   - **Goal:** Build robust test coverage with stable selectors.
   - **Skills:** [`@browser-automation`](../../skills/06_Core/Auto/browser-automation/), `@go-playwright` (optional, Go stack)
   - **Prompt example:** `Use @go-playwright to implement browser automation in a Go project.`

3. **Triage and harden**
   - **Goal:** Remove flaky behavior and enforce repeatability.
   - **Skills:** [`@systematic-debugging`](../../skills/10_Test/Unit/systematic-debugging/), [`@test-fixing`](../../skills/10_Test/Unit/test-fixing/), [`@verification-before-completion`](../../skills/10_Test/Unit/verification-before-completion/)
   - **Prompt example:** `Use @systematic-debugging to classify and resolve flaky behavior in CI.`

---

## Workflow: Design a DDD Core Domain

Model a complex domain coherently, then implement tactical and evented patterns only where justified.

**Related bundles:** `Architecture & Design`, `DDD & Evented Architecture`

### Prerequisites

- Access to at least one domain expert or product owner proxy.
- Current system context and integration landscape available.
- Agreement on business goals and key domain outcomes.

### Steps

1. **Assess DDD fit and scope**
   - **Goal:** Decide whether full DDD, partial DDD, or simple modular architecture is appropriate.
   - **Skills:** [`@domain-driven-design`](../../skills/02_Arch/SysDesign/domain-driven-design/), [`@architecture-decision-records`](../../skills/02_Arch/SysDesign/architecture-decision-records/)
   - **Prompt example:** `Use @domain-driven-design to evaluate if full DDD is justified for our billing and fulfillment platform.`

2. **Create strategic model**
   - **Goal:** Define subdomains, bounded contexts, and ubiquitous language.
   - **Skills:** [`@ddd-strategic-design`](../../skills/02_Arch/SysDesign/ddd-strategic-design/)
   - **Prompt example:** `Use @ddd-strategic-design to classify subdomains and propose bounded contexts with ownership.`

3. **Map context relationships**
   - **Goal:** Define upstream/downstream contracts and anti-corruption boundaries.
   - **Skills:** [`@ddd-context-mapping`](../../skills/02_Arch/SysDesign/ddd-context-mapping/)
   - **Prompt example:** `Use @ddd-context-mapping to model Checkout, Billing, and Inventory interactions with clear contract ownership.`

4. **Implement tactical model**
   - **Goal:** Encode invariants with aggregates, value objects, and domain events.
   - **Skills:** [`@ddd-tactical-patterns`](../../skills/02_Arch/SysDesign/ddd-tactical-patterns/), [`@test-driven-development`](../../skills/08_Feat/Logic/test-driven-development/)
   - **Prompt example:** `Use @ddd-tactical-patterns to design aggregates and invariants for order lifecycle transitions.`

5. **Adopt evented patterns selectively**
   - **Goal:** Apply CQRS, event store, projections, and sagas only where complexity and scale require them.
   - **Skills:** [`@cqrs-implementation`](../../skills/02_Arch/SysDesign/cqrs-implementation/), [`@event-store-design`](../../skills/07_UI/Web/event-store-design/), [`@projection-patterns`](../../skills/02_Arch/SysDesign/projection-patterns/), [`@saga-orchestration`](../../skills/02_Arch/SysDesign/saga-orchestration/)
   - **Prompt example:** `Use @cqrs-implementation and @projection-patterns to scale read-side reporting without compromising domain invariants.`

---

## Machine-Readable Workflows

For tooling and automation, workflow metadata is available in [data/workflows.json](../../data/workflows.json).
