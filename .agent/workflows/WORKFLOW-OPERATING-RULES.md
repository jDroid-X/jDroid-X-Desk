# Enterprise Workflow Operating Rules
**Senior Architect Instructions for Workflow Integrity**  
**Classification:** Universal Operational Invariant  
**Enforcement Agent:** [07_Workflow Agent](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/07_Workflow/AGENT-SPEC.md) & [17_Audit Agent](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/17_Audit/AGENT-SPEC.md)

---

## Mission Statement
Your responsibility is NOT to build isolated features. Your responsibility is to ensure every feature, workflow, process, integration, screen, API, database transaction, AI agent, and user action reaches a valid end state without failure, ambiguity, duplication, or abandonment.

---

### RULE 1 - NO OPEN ENDS
Every workflow must follow:
`START → PROCESS → VALIDATION → DECISION → SUCCESS OR FAILURE → AUDIT → NOTIFICATION → END`
Never leave a workflow hanging. If a step starts, it must complete, fail, retry, rollback, escalate, or terminate. No sixth option exists.

### RULE 2 - EVERY ACTION HAS AN OWNER
Every workflow step must explicitly identify its owner: User, System, Agent, Service, or External Platform. No anonymous responsibility.

### RULE 3 - EVERY INPUT MUST BE VALIDATED
Before processing, validate: mandatory fields, formats, data types, length, permissions, business rules, and dependencies. Trust nothing. Validate everything.

### RULE 4 - EVERY FAILURE MUST HAVE A RESPONSE
For every step, identify failure modes (API unavailable, invalid data, timeout, token expiry, AI hallucination). Define detection, retry, escalation, recovery, and notification. No silent failures.

### RULE 5 - NO DIRECT DEPENDENCIES
Components must not be tightly coupled. Use events, queues, services, and interfaces (`Module A → Event → Subscribers`). Reduce coupling.

### RULE 6 - DESIGN FOR RETRY
Every external action must support retry with finite limits: maximum attempts, delay strategy (exponential backoff), and terminal failure states. Never allow infinite loops.

### RULE 7 - EVERY WORKFLOW IS TRACKABLE
Every execution must produce: Workflow ID, Run ID, Stage ID, Timestamp, and Status (`Running`, `Waiting`, `Completed`, `Failed`, `Cancelled`, `Retried`). Nothing executes invisibly.

### RULE 8 - BUILD FEEDBACK LOOPS
Every workflow must report: Expected Result, Actual Result, and Variance. Measure outcomes. Never assume success.

### RULE 9 - PREVENT DUPLICATION
Before creating a screen, API, database table, service, agent, or workflow: search existing implementations first. Reuse before creating. One source of truth.

### RULE 10 - EVERY DECISION REQUIRES RULES
Avoid ambiguous branching. Define Decision Criteria, Conditions, Expected Outcomes, and Failure Outcomes for all conditional branches.

### RULE 11 - AUDIT TRAIL REQUIRED
Record: Who, What, When, Where, Why, and Outcome for every business-critical action.

### RULE 12 - DESIGN FOR RECOVERY
Assume server crashes, network drops, or agent stalls. Recovery paths must be defined before implementation.

### RULE 13 - NO HIDDEN BUSINESS LOGIC
Business rules belong in workflow engines, rule engines, or configurations—never buried in UI view layers or random hardcoded conditions.

### RULE 14 - STATE IS KING
Every workflow stage must maintain explicit state (`Draft`, `Pending`, `Review`, `Approved`, `Processing`, `Completed`, `Failed`). Never rely on implicit assumptions.

### RULE 15 - EVERY INTEGRATION IS UNTRUSTED
Treat all external APIs as unstable. Defend against rate limits, downtime, and schema shifts. Always validate integration responses.

### RULE 16 - MONITOR EVERYTHING
Track execution time, failure rate, retry count, queue size, API latency, and resource usage. If it cannot be monitored, it cannot be managed.

### RULE 17 - CLOSE FEEDBACK LOOPS
Every workflow must answer: What was requested? What was executed? What was produced? Was it accepted or rejected? What should happen next? No workflow ends without feedback.

### RULE 18 - ZERO ORPHANS
No orphan records, files, media, workflows, jobs, notifications, or users. Everything must have strict lifecycle management and explicit cleanup.

### RULE 19 - THINK PRODUCTION FIRST
Design systems capable of scaling to 10, 100, 1,000, and 10,000 users. Do not build fragile prototypes; build production-ready software.

### RULE 20 - FINAL ARCHITECT CHECKPOINT
Before approving any workflow or release, verify:
- [x] Start and End exist
- [x] Owner exists
- [x] Input validation exists
- [x] Audit trail exists
- [x] Recovery & finite retry exist
- [x] Monitoring & alerts exist
- [x] Security & encryption exist
- [x] Feedback loop exists
- [x] Zero duplication exists
- [x] No open ends exist
