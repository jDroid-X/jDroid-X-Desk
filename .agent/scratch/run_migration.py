import os
import re
import shutil
import sys

# Base Paths
BASE_DIR = r"c:\Users\dell\jAnitGravity\.kit\.agent"
SKILLS_DIR = os.path.join(BASE_DIR, "skills")
BRAIN_DIR = r"C:\Users\dell\.gemini\antigravity\brain\2eb8cc55-93ed-4644-98a4-21509872c97d"
SCRATCH_DIR = os.path.join(BRAIN_DIR, "scratch")

# Define Categories and Subcategories with associated keywords
CATEGORY_RULES = [
    {
        "l1": "01_Reqs",
        "name": "Requirements & Business Analysis",
        "subcategories": {
            "Business": [
                "business", "product-owner", "agile", "startup", "kpi", "growth", "pricing", 
                "customer", "conversion", "cro", "onboarding", "pitch", "jobs-to-be-done", 
                "loss-aversion", "scarcity-urgency", "trust-calibrator", "product-manager", 
                "ab-test", "saas-mvp", "sales-enablement", "pricing-strategy", "competitive-landscape", 
                "competitor-alternatives", "free-tool-strategy", "launch-strategy", "lead-magnets", 
                "revops", "risk-manager", "risk-metrics-calculation", "sred-project-organizer", 
                "team-collaboration-issue", "team-collaboration-standup-notes", "team-composition-analysis", 
                "planning-with-files", "objection-preemptor", "last30days", "nerdzao-elite", 
                "wellally-tech", "analytics-product", "analytics-tracking", "analyze-project", 
                "bill-gates", "elon-musk", "steve-jobs", "acceptance-orchestrator", "ad-creative",
                "churn-prevention", "defi-protocol-templates", "executing-plans", "hr-pro", 
                "interview-coach", "inventory-demand-planning", "oss-hunter", "adhx",
                "cc-skill-strategic-compact", "energy-procurement", "goal-analyzer", "product-inventor",
                "production-scheduling", "progressive-estimation", "referral-program", "quant-analyst",
                "puzzle-activity-planner"
            ],
            "Psych": [
                "psychology", "psychologist", "perception", "emotional-arc", "visual-emotion", 
                "ux-persuasion", "brand-perception", "onboarding-psychologist", "price-psychology", 
                "identity-mirror", "moyu"
            ],
            "Content": [
                "content", "copywriting", "copy-editing", "writing", "article", "newsletter", 
                "blog", "social-content", "resume", "book", "script", "translation", "draft", 
                "writing-plans", "writing-skills", "readme", "beautiful-prose", "code-documentation", 
                "docs", "documentation", "yes-md", "humanize-chinese", "internal-comms", 
                "tutorial-engineer", "wiki-page-writer", "wiki-qa", "data-storytelling",
                "professional-proofreader"
            ],
            "Legal": [
                "legal", "law", "contract", "lex", "gdpr", "compliance", "regulatory", 
                "license", "patent", "copyright", "trademark", "advogado", "fda-food-safety-auditor",
                "junta-leiloeiros", "leiloeiro-edital", "leiloeiro-ia", "leiloeiro-juridico", 
                "leiloeiro-mercado", "leiloeiro-risco"
            ]
        }
    },
    {
        "l1": "02_Arch",
        "name": "Architecture & System Design",
        "subcategories": {
            "C4": ["c4", "c4-container", "c4-code", "c4-component", "c4-context"],
            "SysDesign": [
                "architecture", "design-patterns", "architectural", "domain-driven", "ddd", 
                "event-sourcing", "cqrs", "service-mesh", "monorepo", "microservices", 
                "saga-orchestration", "system-design", "architect-review", "senior-architect", 
                "blueprint", "cc-skill-backend-patterns", "data-structure-protocol", "saas-multi-tenant", 
                "workflow-orchestration-patterns", "workflow-patterns", "architecture-decision-records",
                "nx-workspace-patterns", "projection-patterns"
            ],
            "Threat": [
                "threat", "threat-modeling", "attack-tree", "stride", "secure-by-design", 
                "security-requirement", "security-requirements"
            ]
        }
    },
    {
        "l1": "03_Deps",
        "name": "Dependencies & Environment Setup",
        "subcategories": {
            "PkgMgmt": [
                "dependency", "dependency-management", "deps", "package", "npm", "pip", 
                "bundler", "gem", "yarn", "pnpm", "uv", "cargo", "gradle", "maven", 
                "nuget", "package-manager", "dependency-analyzer", "speckit-updater", 
                "audit-skills"
            ],
            "EnvSetup": [
                "setup", "environment-setup", "config", "configuration", "settings", 
                "installer", "variables", "varlock", "env", "init", "boilerplate", 
                "scaffold", "devcontainer", "project-skill-audit", "bun-development", 
                "filesystem-context", "manage-skills", "skill-creator", "skill-creator-ms", 
                "skill-developer", "skill-improver", "skill-optimizer", "skill-seekers", 
                "skill-sentinel", "skill-writer", "skills_backup", "superpowers-lab", 
                "using-superpowers", "powershell-windows", "skill-check"
            ],
            "Docker": [
                "docker", "compose", "dockerfile", "containerization", "container", 
                "vagrant"
            ]
        }
    },
    {
        "l1": "04_DB",
        "name": "Database & Storage",
        "subcategories": {
            "SQL": [
                "sql", "postgres", "mysql", "sqlite", "oracle", "mariadb", "migrations", 
                "sql-pro", "sqlmap", "claimed-postgres", "neon-postgres", "drizzle-orm-expert", 
                "using-neon", "cc-skill-clickhouse-io", "database", "database-admin", 
                "database-migration", "database-optimizer", "prisma-expert"
            ],
            "NoSQL": [
                "nosql", "mongo", "mongodb", "redis", "cassandra", "dynamodb", "couchdb", 
                "firebase", "supabase", "convex", "dbos-golang", "dbos-python"
            ],
            "Vector": [
                "vector", "embedding", "chroma", "pincone", "qdrant", "milvus", 
                "weaviate", "similarity-search", "vector-database", "rag-engineer", 
                "rag-implementation"
            ],
            "ETL": [
                "etl", "data-pipeline", "airflow", "dbt", "transformation", "pipeline", 
                "data-engineer", "database-architect", "data-quality", "snowflake-development"
            ]
        }
    },
    {
        "l1": "05_API",
        "name": "API Design & Backend Contracts",
        "subcategories": {
            "REST": [
                "rest", "api", "apis", "routes", "endpoints", "router", "fastapi", 
                "flask", "express", "hono", "controller", "spring-boot", "webflux", 
                "client-sdk", "pydantic-models-py", "fastapi-router-py", "nestjs-expert", 
                "zod-validation-expert", "native-data-fetching", "dotnet-backend"
            ],
            "GraphQL": ["graphql", "apollo", "relay", "query", "mutation"],
            "gRPC": ["grpc", "protobuf", "rpc", "thrift"],
            "Auth": [
                "auth", "authentication", "authorization", "oauth", "oauth2", "login", 
                "signup", "passport", "clerk", "security-rules", "token", "session", 
                "jwt", "nextjs-supabase-auth", "clerk-auth", "cred-omega"
            ]
        }
    },
    {
        "l1": "06_Core",
        "name": "Core Services & Logic",
        "subcategories": {
            "AI": [
                "ai", "ml", "machine-learning", "model", "hugging-face", "gemini", 
                "claude", "openai", "prompt", "llm", "agent", "crewai", "vision", 
                "nlp", "bot", "voice", "text-to-speech", "speech-to-text", "translation", 
                "langchain", "langgraph", "fal", "perplexity", "transformers", "rag", 
                "visual", "generative", "deep-research", "notebooklm", "azure-ai", 
                "sam-altman", "yann-lecun", "satori", "antigravity-skill-orchestrator", 
                "bdistill-knowledge-extraction", "context-compression", "context-degradation", 
                "context-driven-development", "context-manager", "conversation-memory", 
                "copilot-sdk", "defuddle", "evolution", "geoffrey-hinton", "ilya-sutskever", 
                "intelligent-routing", "jobgpt", "langfuse", "maxia", "multi-advisor", 
                "task-intelligence", "vexor", "vexor-cli", "helium-mcp", "family-health-analyzer", 
                "fitness-analyzer", "health-trend-analyzer", "mental-health-analyzer", "nutrition-analyzer", 
                "occupational-health-analyzer", "oral-health-analyzer", "rehabilitation-analyzer", 
                "sexual-health-analyzer", "skin-health-analyzer", "sleep-analyzer", "tcm-constitution-analyzer", 
                "travel-health-analyzer", "weightloss-analyzer", "context-fundamentals", "protect-mcp-governance"
            ],
            "Auto": [
                "automation", "integrate", "integrations", "zapier", "make", "n8n", 
                "webhook", "slack", "discord", "telegram", "whatsapp", "email", 
                "gmail", "calendar", "activecampaign", "brevo", "mailchimp", "sendgrid", 
                "odoo", "jira", "linear", "trello", "asana", "monday", "hubspot", 
                "salesforce", "wrike", "box", "dropbox", "google-docs", "google-drive", 
                "google-sheets", "google-slides", "outlook", "work-summary", "daily-news", 
                "office-productivity", "m365-agents", "gmail-automation", "slack-gif", 
                "codebase-to-wordpress-converter", "faf-expert", "file-organizer", "google-workspace-ops", 
                "inngest", "instagram", "linkedin-cli", "linkedin-profile-optimizer", "mmx-cli", 
                "obsidian-bases", "obsidian-cli", "obsidian-clipper-template-creator", "sales-automator", 
                "trigger-dev", "jq"
            ],
            "Research": [
                "research", "academic", "literature", "paper", "arxiv", "biorxiv", 
                "medrxiv", "openalex", "europepmc", "pubmed", "citation", "search-specialist", 
                "search-usr", "exa", "crawl", "scrape", "web-scraper", "apify", 
                "firecrawl", "tavily", "apify-lead-generation", "citation-management", 
                "literature-search", "astropy", "biopython", "matematico-tao", "molykit", 
                "sympy", "uniprot-database", "qiskit"
            ]
        }
    },
    {
        "l1": "07_UI",
        "name": "UI Framework & Design Systems",
        "subcategories": {
            "Web": [
                "frontend", "ui", "ux", "react", "angular", "vue", "svelte", 
                "css", "html", "tailwind", "design", "component", "web", 
                "nextjs", "next", "next.js", "vite", "webpack", "rollup", 
                "parcel", "javascript", "typescript", "web-artifacts", "page-cro", 
                "form-cro", "signup-flow-cro", "popup-cro", "scroll-experience", "canvas-design", 
                "favicon", "presenting", "slides", "markdown", "d3js", "matplotlib", 
                "networkx", "plotly", "seaborn", "visualization", "rayden", "shadcn", 
                "flow-node", "react-patterns", "react-best-practices", "typescript-expert", 
                "typescript-pro", "javascript-pro", "frontend-developer", "senior-frontend", 
                "animejs-animation", "chat-widget", "electron-development", "fixing-accessibility", 
                "i18n-localization", "interactive-portfolio", "json-canvas", "landing-page-generator", 
                "stitch-loop", "wordpress", "wordpress-plugin-development", "astro", "vizcom", 
                "wcag-audit-patterns"
            ],
            "Mobile": [
                "mobile", "ios", "android", "flutter", "react-native", "expo", 
                "swift", "kotlin", "jetpack", "swiftui", "liquid-glass", "view-refactor", 
                "mobile-design", "android-cli", "adb", "mobile-developer", "android_ui_verification", 
                "multi-platform-apps-multi-platform", "robius-event-action", "robius-matrix-integration", 
                "robius-widget-patterns"
            ],
            "DesignSys": [
                "design-system", "ui-tokens", "theme", "palettes", "styles", 
                "fonts", "iconsax", "brand-guidelines", "hig-foundations", "hig-patterns", 
                "hig-components", "hig-inputs", "hig-platforms", "makepad-font", "makepad-layout", 
                "makepad-widgets", "makepad-basics", "makepad-skills", "makepad-animation", 
                "makepad-deployment", "makepad-dsl", "makepad-event-action", "makepad-splash", 
                "avalonia-layout-zafiro", "avalonia-zafiro-development", "hig-project-context", 
                "hig-technologies"
            ]
        }
    },
    {
        "l1": "08_Feat",
        "name": "Feature Implementation",
        "subcategories": {
            "Logic": [
                "feature", "logic", "core", "services", "repository", "business-logic", 
                "viewmodel", "state", "behavior", "state-management", "state-machine", 
                "zustand", "redux", "mobx", "rx", "reactive", "clean-code", "fp-async", 
                "fp-refactor", "fp-backend", "fp-data-transforms", "fp-errors", "fp-option-ref", 
                "fp-pipe-ref", "fp-pragmatic", "fp-react", "fp-taskeither-ref", "fp-ts", 
                "functional-programming", "java-pro", "go-pro", "rust-pro", "python-pro", 
                "c-pro", "cpp-pro", "csharp-pro", "php-pro", "ruby-pro", "haskell-pro", 
                "scala-pro", "julia-pro", "elixir-pro", "kotlin-coroutines-expert", "swift-concurrency-expert", 
                "saga-orchestration", "conductor-implement", "conductor-manage", "conductor-new-track", 
                "conductor-revert", "conductor-setup", "conductor-status", "conductor-validator", 
                "async-python-patterns", "bash-defensive-patterns", "bash-pro", "chrome-extension-developer", 
                "code-refactoring-refactor-clean", "code-refactoring-tech-debt", "code-simplifier", 
                "development", "dotnet-backend-patterns", "fp-either-ref", "fp-types-ref", 
                "framework-migration-code-migrate", "framework-migration-legacy-modernize", 
                "legacy-modernizer", "go-concurrency-patterns", "golang-pro", "go-rod-master", 
                "loki-mode", "memory-safety-patterns", "memory-systems", "nodejs-backend-patterns", 
                "nodejs-best-practices", "orchestrate-batch-refactor", "posix-shell-pro", 
                "rust-async-patterns", "senior-fullstack", "temporal-golang-pro", "uncle-bob-craft", 
                "windows-shell-reliability", "python-patterns", "simplify-code", "lightning-channel-factories"
            ],
            "Games": [
                "game", "unity", "unreal", "godot", "bevy", "minecraft", "bukkit", 
                "spigot", "paper-api", "gdscript", "game-development", "minecraft-bukkit-pro"
            ],
            "XR": [
                "xr", "vr", "ar", "spline", "3d", "threejs", "three.js", 
                "shader", "glsl", "webgl", "threejs-animation", "threejs-fundamentals", 
                "threejs-geometry", "threejs-interaction", "threejs-lighting", "threejs-loaders", 
                "threejs-materials", "threejs-postprocessing", "threejs-shaders", "threejs-textures"
            ],
            "Media": [
                "media", "audio", "video", "videodb", "image", "remotion", 
                "generate-image", "fal-audio", "fal-generate", "fal-image-edit", "fal-upscale", 
                "fal-workflow", "speech", "transcript", "podcast", "slack-gif-creator", 
                "magic-animator", "videodb-skills", "podcast-generation", "docx", 
                "libreoffice", "pdf", "pptx", "xlsx", "nanobanana-ppt-skills", 
                "python-pptx-generator", "screenshots"
            ]
        }
    },
    {
        "l1": "09_Integ",
        "name": "Integration & Third-Party APIs",
        "subcategories": {
            "Cloud": [
                "cloud", "aws", "azure", "gcp", "firebase", "supabase", 
                "cloud-run", "cloudflare", "serverless", "s3", "dynamodb", 
                "lambda", "eventhub", "servicebus", "keyvault", "active-directory", 
                "fabric", "cognito", "azure-eventhub-dotnet", "azure-eventhub-ts", 
                "azure-identity-java", "azure-identity-ts", "azure-mgmt-apimanagement-dotnet", 
                "azure-mgmt-fabric-dotnet", "azure-resource-manager-cosmosdb-dotnet", 
                "azure-resource-manager-redis-dotnet", "azure-resource-manager-sql-dotnet", 
                "azure-servicebus-ts", "azure-storage-blob-java", "azure-web-pubsub-ts", 
                "network-101", "network-engineer"
            ],
            "Payment": [
                "payment", "stripe", "paypal", "checkout", "billing", 
                "monetization", "paywall", "commerce", "ecommerce", "transaction", 
                "fintech", "plaid", "stripe-integration", "paypal-integration", "plaid-fintech", 
                "shopify-apps", "shopify-development", "alpha-vantage"
            ],
            "3rdParty": [
                "maps", "location", "geofence", "geolocation", "notification", 
                "push", "messaging", "fcm", "mail", "twilio", "apify", 
                "clerk", "postmark", "mixpanel", "posthog", "amplitude", 
                "twilio-communications", "hubspot-integration", "algolia-search", 
                "unsplash-integration", "upstash-qstash"
            ]
        }
    },
    {
        "l1": "10_Test",
        "name": "Testing, QA & Validation",
        "subcategories": {
            "E2E": [
                "e2e", "playwrite", "playwright", "cypress", "selenium", 
                "pupeteer", "puppeteer", "webapp-testing", "integration-testing", 
                "automation-testing", "go-playwright", "playwright-skill", "playwright-java"
            ],
            "Unit": [
                "unit", "test", "testing", "tdd", "jest", "vitest", 
                "mocha", "chai", "pytest", "junit", "spec", "mock", 
                "assertions", "coverage", "test-runner", "test-fixing", "verification", 
                "testing-patterns", "tdd-workflow", "tdd-workflows-tdd-cycle", 
                "tdd-workflows-tdd-green", "tdd-workflows-tdd-red", "tdd-workflows-tdd-refactor", 
                "test-driven-development", "pypict-skill", "code-review-checklist", 
                "code-review-excellence", "code-reviewer", "codex-review", "comprehensive-review-full-review", 
                "comprehensive-review-pr-enhance", "debug-buttercup", "debugger", "debugging-strategies", 
                "distributed-debugging-debug-trace", "phase-gated-debugging", "systematic-debugging", 
                "error-debugging-error-analysis", "error-debugging-error-trace", "error-detective", 
                "error-diagnostics-error-analysis", "error-diagnostics-error-trace", "error-handling-patterns", 
                "evaluation", "fix-review", "quality-nonconformance", "differential-review", 
                "django-access-review", "django-perf-review", "gh-review-requests", "lint-and-validate", 
                "receiving-code-review", "requesting-code-review", "vibe-code-auditor", "vibers-code-review"
            ],
            "Pentest": [
                "pentest", "penetration", "vulnerability", "scanner", "scan", 
                "owasp", "xss", "injection", "sqli", "csrf", "idor", 
                "bypass", "fuzzing", "ffuf", "burp", "burpsuite", "sqlmap", 
                "exploit", "fuzz", "nmap", "nikto", "dirb", "vulnerability-scanner", 
                "ethical-hacking-methodology", "burp-suite-testing", "api-security-best-practices", 
                "api-fuzzing-bug-bounty", "top-web-vulnerabilities", "broken-authentication", 
                "security-auditor", "backend-security-coder", "cc-skill-security-review", 
                "file-path-traversal", "file-uploads", "find-bugs", "gha-security-review", 
                "metasploit-framework", "security", "security-audit", "semgrep-rule-creator", 
                "solidity-security", "zeroize-audit", "bug-hunter", "production-code-audit"
            ],
            "RedTeam": [
                "red-team", "attack", "defense", "evasion", "mitre", 
                "privilege-escalation", "active-directory-attacks", "malware", "anti-reversing", 
                "reverse", "reversing", "binary", "dwarf", "gdb", 
                "decompilation", "reversing-engineering", "red-team-tactics", "linux-privilege-escalation", 
                "windows-privilege-escalation", "malware-analyst", "reverse-engineer", "memory-forensics",
                "007"
            ]
        }
    },
    {
        "l1": "11_Opt",
        "name": "Optimization (Perf & SEO)",
        "subcategories": {
            "Perf": [
                "optimization", "performance", "speed", "vitals", "core-web-vitals", 
                "bundle", "size", "compile", "build-optimization", "profiler", 
                "profiling", "benchmark", "caching", "lighthouse", "lazy-loading", 
                "react-component-performance", "swiftui-performance-audit", "odoo-performance-tuner", 
                "zipai-optimizer", "performance-profiling", "performance-engineer", "constant-time-analysis", 
                "dx-optimizer", "axiom", "viboscope"
            ],
            "SEO": [
                "seo", "ranking", "keywords", "metadata", "crawlers", 
                "indexation", "sitemap", "backlinks", "seo-audit", "geo-fundamentals", 
                "seo-fundamentals", "programmatic-seo", "local-legal-seo", "seo-checker", 
                "indexing-issue-auditor", "keyword-extractor"
            ],
            "Cost": [
                "cost", "cleanup", "optimization-cost", "aws-cost", "azure-cost", 
                "cloud-cost", "cost-optimization", "cost-cleanup", "aws-cost-cleanup"
            ]
        }
    },
    {
        "l1": "12_Deploy",
        "name": "Deployment & Operations",
        "subcategories": {
            "CICD": [
                "cicd", "ci-cd", "ci", "cd", "pipeline", "actions", 
                "github-actions", "gitlab-ci", "workflows", "devops", "release", 
                "changelog", "semantic-release", "app-store-changelog", "github-workflow-automation", 
                "address-github-comments", "appdeploy", "closed-loop-delivery", "commit", 
                "create-branch", "create-issue-gate", "create-pr", "finishing-a-development-branch", 
                "iterate-pr", "pr-writer", "github-issue-creator", "gitops-workflow", 
                "manifest", "on-call-handoff-patterns", "technical-change-tracker", "trigger-dev", 
                "observability-engineer", "observability-monitoring-slo-implement", "slo-implementation",
                "deploy", "github", "issues", "using-git-worktrees"
            ],
            "K8s": [
                "kubernetes", "k8s", "helm", "cluster", "kubectl", "ingress", 
                "service-mesh", "istio", "linkerd"
            ],
            "Stores": [
                "stores", "store", "app-store", "play-store", "appstore", 
                "playstore", "release-notes", "aso", "app-store-optimization", "packaging", 
                "signing", "codesign", "dmg", "apk", "bundle", "eas", 
                "expo-deployment"
            ],
            "IaC": [
                "iac", "terraform", "cloudformation", "ansible", "puppet", "chef", 
                "vagrant", "bicep", "arm-template", "azd", "terraform-specialist", 
                "terraform-module-library", "blockrun", "vercel-deployment"
            ]
        }
    },
    {
        "l1": "13_Misc",
        "name": "Miscellaneous Skills",
        "subcategories": {
            "Misc": []
        }
    }
]

def get_categorization(skill_name):
    skill_lower = skill_name.lower()
    for rule in CATEGORY_RULES[:-1]: # skip misc
        l1 = rule["l1"]
        for subcat, keywords in rule["subcategories"].items():
            for kw in keywords:
                if kw.lower() in skill_lower:
                    return l1, subcat
    return "13_Misc", "Misc"

def parse_skill_metadata(skill_dir_path):
    skill_file = os.path.join(skill_dir_path, "SKILL.md")
    name_val = os.path.basename(skill_dir_path)
    description = ""
    
    if os.path.exists(skill_file):
        try:
            with open(skill_file, "r", encoding="utf-8") as f:
                content = f.read()
            fm_match = re.match(r'^---\s*\n(.*?)\n---\s*\n', content, re.DOTALL)
            if fm_match:
                fm_text = fm_match.group(1)
                desc_match = re.search(r'^description:\s*(.*)', fm_text, re.MULTILINE)
                if desc_match:
                    description = desc_match.group(1).strip().strip('"').strip("'")
            if not description:
                desc_match = re.search(r'^\*\*(.*?)\*\*', content, re.MULTILINE)
                if desc_match:
                    description = desc_match.group(1).strip()
        except Exception:
            pass
            
    if not description:
        description = f"Knowledge base for {name_val}"
        
    return description

def replace_in_frontmatter(match, mapping):
    frontmatter = match.group(1)
    lines = frontmatter.splitlines()
    new_lines = []
    in_skills_list = False
    for line in lines:
        if line.startswith("skills:"):
            parts = line.split(":", 1)
            skill_part = parts[1].strip()
            if skill_part:
                skill_names = [s.strip() for s in skill_part.split(",")]
                new_skill_names = []
                for s in skill_names:
                    if s in mapping:
                        new_skill_names.append(f"{mapping[s][0]}/{mapping[s][1]}/{s}")
                    else:
                        new_skill_names.append(s)
                line = f"skills: {', '.join(new_skill_names)}"
            else:
                in_skills_list = True
        elif in_skills_list:
            if line.startswith("  - "):
                s = line[4:].strip()
                if s in mapping:
                    line = f"  - {mapping[s][0]}/{mapping[s][1]}/{s}"
            elif not line.startswith(" ") and line.strip() != "":
                in_skills_list = False
        new_lines.append(line)
    return "---\n" + "\n".join(new_lines) + "\n---\n"


def replace_at_refs(match, mapping):
    prefix = match.group(1) or ""
    sname = match.group(2)
    if sname in mapping:
        l1, l2 = mapping[sname]
        return f"@[{prefix}{l1}/{l2}/{sname}]"
    return match.group(0)

def update_file_references(content, filepath, mapping, sorted_skills):
    changed = False
    
    # 1. Replace path references like: skills/skill-name or skills\skill-name
    def path_replace(match):
        prefix = match.group(1)
        sname = match.group(2)
        if sname in mapping:
            l1, l2 = mapping[sname]
            separator = "\\" if "\\" in prefix else "/"
            return f"skills{separator}{l1}{separator}{l2}{separator}{sname}"
        return match.group(0)
        
    pattern_path = r'\bskills(/|\\)([\w\-\.]+)'
    if re.search(pattern_path, content):
        new_content = re.sub(pattern_path, path_replace, content)
        if new_content != content:
            content = new_content
            changed = True

    # 2. Replace @[skills/skill-name] or @[skill-name]
    def at_replace(m):
        return replace_at_refs(m, mapping)
        
    pattern_at = r'@\[(skills/)?([\w\-\.]+)\]'
    if re.search(pattern_at, content):
        new_content = re.sub(pattern_at, at_replace, content)
        if new_content != content:
            content = new_content
            changed = True

    # 3. Replace in Frontmatter YAML
    pattern_fm = r'^---\s*\n(.*?)\n---[^\r\n]*\n'
    fm_match = re.search(pattern_fm, content, re.DOTALL | re.MULTILINE)
    if fm_match:
        def fm_replace(m):
            return replace_in_frontmatter(m, mapping)
        new_content = re.sub(pattern_fm, fm_replace, content, count=1, flags=re.DOTALL | re.MULTILINE)
        if new_content != content:
            content = new_content
            changed = True
            
    # 4. Special replacement for ARCHITECTURE.md
    if os.path.basename(filepath) == "ARCHITECTURE.md":
        for skill_name in sorted_skills:
            l1, l2 = mapping[skill_name]
            target_path = f"{l1}/{l2}/{skill_name}"
            pattern_word = r'\b' + re.escape(skill_name) + r'\b'
            if target_path not in content:
                content = re.sub(pattern_word, target_path, content)
                changed = True
                
    return content, changed

def generate_skill_index(mapping):
    lines = []
    lines.append("# Antigravity Skills Master Index (SKILL_INDEX.md)")
    lines.append("")
    lines.append("> Unified Program Mapping & Alias Glossary for the Refactored 3-Level Skills Hierarchy.")
    lines.append("")
    
    grouped = {}
    for rule in CATEGORY_RULES:
        grouped[rule["l1"]] = {sub: [] for sub in rule["subcategories"]}
        
    for skill_name, (l1, l2) in mapping.items():
        if l1 not in grouped:
            grouped[l1] = {}
        if l2 not in grouped[l1]:
            grouped[l1][l2] = []
        grouped[l1][l2].append(skill_name)
        
    for rule in CATEGORY_RULES:
        l1 = rule["l1"]
        l1_name = rule["name"]
        lines.append(f"## {l1} - {l1_name}")
        lines.append("")
        
        for subcat in rule["subcategories"]:
            lines.append(f"### {subcat} Domain")
            lines.append("")
            skills_in_sub = grouped[l1][subcat]
            if not skills_in_sub:
                lines.append("*No skills mapped to this subcategory yet.*")
                lines.append("")
                continue
                
            lines.append("| Skill Short Path | Alias Name | Description | Link |")
            lines.append("| :--- | :--- | :--- | :--- |")
            
            for s in sorted(skills_in_sub):
                skill_path = os.path.join(SKILLS_DIR, l1, subcat, s)
                desc = parse_skill_metadata(os.path.join(SKILLS_DIR, s) if os.path.exists(os.path.join(SKILLS_DIR, s)) else skill_path)
                rel_link = f".agent/skills/{l1}/{subcat}/{s}/SKILL.md"
                lines.append(f"| `{l1}/{subcat}/{s}` | `{s}` | {desc} | [Open SKILL.md]({rel_link}) |")
                
            lines.append("")
        lines.append("---")
        lines.append("")
        
    return "\n".join(lines)

def run_dry_run():
    skills = [d for d in os.listdir(SKILLS_DIR) if os.path.isdir(os.path.join(SKILLS_DIR, d))]
    skills = [s for s in skills if s not in [r["l1"] for r in CATEGORY_RULES]]
    
    mapping = {s: get_categorization(s) for s in skills}
    sorted_skills = sorted(skills, key=len, reverse=True)
    
    print(f"Loaded {len(skills)} skills for migration mapping.")
    
    # We scan all directories in .agent EXCEPT skills/ for fast check first,
    # and then scan skills/ separately.
    directories_to_scan = ["agents", "workflows", "rules", "scripts"]
    files_to_update = []
    
    for d in directories_to_scan:
        dir_path = os.path.join(BASE_DIR, d)
        if not os.path.exists(dir_path):
            continue
        for root, _, files in os.walk(dir_path):
            for file in files:
                if file.endswith((".md", ".py", ".sh", ".json", ".txt")):
                    filepath = os.path.join(root, file)
                    if "scratch" in filepath or "temp" in filepath:
                        continue
                    files_to_update.append(filepath)
                    
    arch_file = os.path.join(BASE_DIR, "ARCHITECTURE.md")
    if os.path.exists(arch_file):
        files_to_update.append(arch_file)
        
    print(f"Scanning references in {len(files_to_update)} core files...")
    
    reference_changes = {}
    
    for filepath in files_to_update:
        try:
            with open(filepath, "r", encoding="utf-8") as f:
                content = f.read()
            new_content, changed = update_file_references(content, filepath, mapping, sorted_skills)
            if changed:
                rel_path = os.path.relpath(filepath, BASE_DIR)
                diff_lines = []
                lines = content.splitlines()
                new_lines = new_content.splitlines()
                for idx, (line, new_line) in enumerate(zip(lines, new_lines)):
                    if line != new_line:
                        diff_lines.append(f"  Line {idx+1}:")
                        diff_lines.append(f"    - {line.strip()}")
                        diff_lines.append(f"    + {new_line.strip()}")
                reference_changes[rel_path] = diff_lines
        except Exception as e:
            print(f"Error reading file {filepath}: {str(e)}")
            
    # Now scan skills folder files quickly by only checking if they contain any reference to other skills
    # Since there are 1400 skills, we will walk the directory and scan files
    print("Scanning references in skills folder files...")
    skills_files_scanned = 0
    skills_files_changed = 0
    
    # We compile a quick regex of all skill names to check if a file even mentions any of them
    # before doing the full update_file_references
    skill_mention_pattern = re.compile(r'\bskills(/|\\)|@\[')
    
    for skill_name in skills:
        skill_path = os.path.join(SKILLS_DIR, skill_name)
        for root, _, files in os.walk(skill_path):
            for file in files:
                if file.endswith((".md", ".py", ".sh", ".json", ".txt")):
                    filepath = os.path.join(root, file)
                    skills_files_scanned += 1
                    try:
                        with open(filepath, "r", encoding="utf-8") as f:
                            content = f.read()
                        # Quick check
                        if skill_mention_pattern.search(content):
                            new_content, changed = update_file_references(content, filepath, mapping, sorted_skills)
                            if changed:
                                skills_files_changed += 1
                                rel_path = os.path.relpath(filepath, BASE_DIR)
                                diff_lines = []
                                lines = content.splitlines()
                                new_lines = new_content.splitlines()
                                for idx, (line, new_line) in enumerate(zip(lines, new_lines)):
                                    if line != new_line:
                                        diff_lines.append(f"  Line {idx+1}:")
                                        diff_lines.append(f"    - {line.strip()}")
                                        diff_lines.append(f"    + {new_line.strip()}")
                                reference_changes[rel_path] = diff_lines
                    except Exception:
                        pass
                        
    print(f"Scanned {skills_files_scanned} files inside skills directory. Found references in {skills_files_changed} files.")
            
    print("\nDRY RUN REFERENCE REPLACEMENT REPORT:")
    print("=" * 40)
    print(f"Total files with references to change: {len(reference_changes)}")
    for file, diff in list(reference_changes.items())[:10]:
        print(f"\nFile: {file}")
        for line in diff[:6]:
            print(line)
        if len(diff) > 6:
            print(f"  ... and {len(diff) - 6} more lines.")
            
    report_file = os.path.join(SCRATCH_DIR, "dry_run_report.txt")
    with open(report_file, "w", encoding="utf-8") as f:
        f.write("MIGRATION DRY RUN REPORT\n")
        f.write("=" * 50 + "\n")
        for skill_name, (l1, l2) in sorted(mapping.items()):
            f.write(f"MOVE: skills/{skill_name} -> skills/{l1}/{l2}/{skill_name}\n")
        f.write("\nREFERENCE UPDATES:\n")
        f.write("=" * 50 + "\n")
        for file, diff in reference_changes.items():
            f.write(f"\nFile: {file}\n")
            for line in diff:
                f.write(line + "\n")
                
    print(f"\nWrote full dry-run report to {report_file}")
    
    index_content = generate_skill_index(mapping)
    index_preview_file = os.path.join(SCRATCH_DIR, "SKILL_INDEX_PREVIEW.md")
    with open(index_preview_file, "w", encoding="utf-8") as f:
        f.write(index_content)
    print(f"Generated preview of SKILL_INDEX.md at {index_preview_file}")

def run_execute():
    skills = [d for d in os.listdir(SKILLS_DIR) if os.path.isdir(os.path.join(SKILLS_DIR, d))]
    skills = [s for s in skills if s not in [r["l1"] for r in CATEGORY_RULES]]
    
    mapping = {s: get_categorization(s) for s in skills}
    sorted_skills = sorted(skills, key=len, reverse=True)
    
    print(f"Starting Physical Migration of {len(skills)} skills...")
    
    # 1. Create target directories first
    for rule in CATEGORY_RULES:
        l1 = rule["l1"]
        for subcat in rule["subcategories"]:
            target_sub_dir = os.path.join(SKILLS_DIR, l1, subcat)
            os.makedirs(target_sub_dir, exist_ok=True)
        if l1 == "13_Misc":
            os.makedirs(os.path.join(SKILLS_DIR, "13_Misc", "Misc"), exist_ok=True)
            
    # 2. Physical folder move
    moves_success = 0
    moves_fail = 0
    for skill_name, (l1, l2) in mapping.items():
        src = os.path.join(SKILLS_DIR, skill_name)
        dst = os.path.join(SKILLS_DIR, l1, l2, skill_name)
        
        try:
            shutil.move(src, dst)
            moves_success += 1
        except Exception as e:
            print(f"Error moving {src} to {dst}: {str(e)}")
            moves_fail += 1
            
    print(f"Physical Moves complete: {moves_success} Succeeded, {moves_fail} Failed.")
    
    # 3. Reference replacement in files
    directories_to_scan = ["agents", "workflows", "rules", "scripts"]
    files_to_update = []
    
    for d in directories_to_scan:
        dir_path = os.path.join(BASE_DIR, d)
        if not os.path.exists(dir_path):
            continue
        for root, _, files in os.walk(dir_path):
            for file in files:
                if file.endswith((".md", ".py", ".sh", ".json", ".txt")):
                    filepath = os.path.join(root, file)
                    if "scratch" in filepath or "temp" in filepath:
                        continue
                    files_to_update.append(filepath)
                    
    arch_file = os.path.join(BASE_DIR, "ARCHITECTURE.md")
    if os.path.exists(arch_file):
        files_to_update.append(arch_file)
        
    replacements_count = 0
    for filepath in files_to_update:
        try:
            with open(filepath, "r", encoding="utf-8") as f:
                content = f.read()
            new_content, changed = update_file_references(content, filepath, mapping, sorted_skills)
            if changed:
                with open(filepath, "w", encoding="utf-8") as f:
                    f.write(new_content)
                replacements_count += 1
        except Exception as e:
            print(f"Error replacing references in file {filepath}: {str(e)}")
            
    # Also update references in Moved Skills themselves
    skill_mention_pattern = re.compile(r'\bskills(/|\\)|@\[')
    skills_files_updated = 0
    for skill_name, (l1, l2) in mapping.items():
        skill_path = os.path.join(SKILLS_DIR, l1, l2, skill_name)
        for root, _, files in os.walk(skill_path):
            for file in files:
                if file.endswith((".md", ".py", ".sh", ".json", ".txt")):
                    filepath = os.path.join(root, file)
                    try:
                        with open(filepath, "r", encoding="utf-8") as f:
                            content = f.read()
                        if skill_mention_pattern.search(content):
                            new_content, changed = update_file_references(content, filepath, mapping, sorted_skills)
                            if changed:
                                with open(filepath, "w", encoding="utf-8") as f:
                                    f.write(new_content)
                                skills_files_updated += 1
                    except Exception:
                        pass
                        
    print(f"Updated reference paths in {replacements_count} core files and {skills_files_updated} skill files.")
    
    # 4. Generate actual SKILL_INDEX.md
    skill_index_path = os.path.join(BASE_DIR, "SKILL_INDEX.md")
    index_content = generate_skill_index(mapping)
    with open(skill_index_path, "w", encoding="utf-8") as f:
        f.write(index_content)
    print(f"Successfully generated {skill_index_path}.")
    
    # 5. Update ARCHITECTURE.md
    try:
        with open(arch_file, "r", encoding="utf-8") as f:
            arch_content = f.read()
        if "SKILL_INDEX.md" not in arch_content:
            arch_content = arch_content.replace(
                "## 🧩 Skills (36)",
                "## 🧩 Skills\n\nAll skills have been refactored into the 12-stage Waterfall structure. See the complete [SKILL_INDEX.md](file:///c:/Users/dell/jAnitGravity/.kit/.agent/SKILL_INDEX.md) for alias mapping.\n\n## 🧩 Skills (36)"
            )
            with open(arch_file, "w", encoding="utf-8") as f:
                f.write(arch_content)
            print("Updated ARCHITECTURE.md to point to SKILL_INDEX.md")
    except Exception as e:
        print(f"Error modifying ARCHITECTURE.md: {str(e)}")

    print("\nMigration physically executed successfully! Complete closed-loop feedback verification recommended.")

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--execute":
        run_execute()
    else:
        run_dry_run()
