# The Token Audit & Context Optimization Protocol (Antigravity Standard)

## Hierarchical Scope: Tier P2 (Context Hygiene & Optimization)

### Objective
Systematically eliminate token waste, maximize prompt cache reuse (>85%), suppress command output noise, and enforce lean context boundaries across all agents and projects.

---

> **Master Operational Router:** [GEMINI.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/rules/GEMINI.md)  
> **Master Workforce Registry:** [AGENTS.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/AGENTS.md)  
> **Master Entity Relationship Mapping:** [PROGRAM-MAPPING.md](file:///c:/Users/jiten/jAnitGravity/.kit/.agent/enterprise-22-agents/PROGRAM-MAPPING.md)  

---

### The 7 Pillars of Token Hygiene

#### 1. MEMORY (Rules & Context Hygiene)
- Audit every `AGENTS.md`, `GEMINI.md`, rules file, and `@imports` in scope.
- **Hard Thresholds:**
  - Single rule file limit: **< 5,000 tokens** (Flag if > 5k).
  - Total cumulative rule context: **< 10,000 tokens** (Flag if > 10k).
- **Pruning Action:** Consolidate redundant instructions; use reference links rather than repeating documentation.

#### 2. TOOLS (MCP & Tool Declaration Hygiene)
- List all connected MCP servers and count exposed tool definitions.
- Verify whether tool deferral / lazy tool loading is **ACTIVE** or **NOT**.
- Prune inactive, experimental, or duplicate MCP tools to prevent bloated system schemas.

#### 3. MODEL & REASONING (Tiering & Thinking Budget)
- Dynamically match model tier to task complexity:
  - **Flash / Low Thinking:** Single-file syntax edits, lint fixes, directory surveys, status pings.
  - **Pro / High Thinking:** Multi-file architectural design, system refactoring, complex root-cause debugging.
- Flag and prevent silent automatic model switching during a continuous session.

#### 4. HOOKS (PreToolUse Noise Suppression)
- Enforce command rewriting and output paging:
  - Add `PAGER=cat`, `git log -n 5`, `git status -s`, `head -n 20`, and specific file globs.
  - Suppress giant console dumps, massive stack traces, and verbose build logs.

#### 5. SUBAGENTS (Isolated Sub-Agent Execution)
- Audit all agent files in project and user agent directories.
- Isolate subagent tasks with dedicated lightweight models.
- Prevent subagent conversation histories from dumping raw uncompressed transcripts into the parent session.

#### 6. SCHEDULED WORK (Cron & Polling Cache Alignment)
- Audit every cron job and background polling interval.
- **Cache Lifetime Guard:** Flag any job whose polling interval exceeds the prompt cache lifetime or polls without data change.

#### 7. CACHE (Prompt Caching Maximization & Telemetry)
- Keep prompt prefixes (rules, tool schemas, base instructions) static and immutable to guarantee **>85% Prompt Cache Hit Rate**.
- For every audit turn, produce the standard Telemetry Finding Table:

| Finding | Severity | Evidence | Cost / Savings |
| :--- | :--- | :--- | :--- |
| *Rule file bloat* | *HIGH / MED / LOW* | *File > 5k tokens* | *Saves ~40% input tokens* |
| *Noisy CLI command* | *HIGH / MED / LOW* | *Unpaged git log* | *Saves ~5k tokens/turn* |
| *Unused MCP tools* | *HIGH / MED / LOW* | *20+ schemas in prompt* | *Saves ~3k tokens/turn* |

---

### Closed-Loop Feedback & Circuit Breaker
- If a Token Audit detects rule or tool bloat:
  1. Trigger auto-pruning / encapsulation.
  2. Re-calculate context size.
  3. Merge back into the lean workflow.
  4. If context still exceeds threshold after 2 attempts -> Raise alert to user and proceed with compressed mode.
