# Antigravity Skill Bundles

> **Curated collections of skills organized by role and expertise level.** Don't know where to start? Pick a bundle below to get a curated set of skills for your role.

> These packs are curated starter recommendations for humans. Generated bundle ids in `data/bundles.json` are broader catalog/workflow groupings and do not need to map 1:1 to the editorial packs below.

> **Important:** bundles are installable plugin subsets and activation presets, not invokable mega-skills such as `@web-wizard` or `/essentials-bundle`. Use the individual skills listed in the pack, install the bundle as a dedicated marketplace plugin, or use the activation scripts if you want only that bundle's skills active in your live Antigravity directory.

> **Plugin compatibility:** root plugins and bundle plugins only publish plugin-safe skills. If a bundle shows `pending hardening`, the skills still exist in the repository, but that bundle is not yet published for that target. `Requires manual setup` means the bundle is installable, but one or more included skills need an explicit setup step before first use.

## Quick Start

1. **Install the repository or bundle plugin:**

   ```bash
   npx antigravity-awesome-skills
   # or clone manually
   git clone https://github.com/sickn33/antigravity-awesome-skills.git .agent/skills
   ```

2. **Choose your bundle** from the list below based on your role or interests.

3. **Use bundle plugins or individual skills** in your AI assistant:
   - Claude Code: install the matching marketplace bundle plugin, or invoke `>> /skill-name help me...`
   - Codex CLI / Codex app: install the matching bundle plugin where plugin marketplaces are available, or invoke `Use skill-name...`
   - Cursor: `@skill-name` in chat
   - Gemini CLI: `Use skill-name...`

If you want a bundle to behave like a focused active subset instead of a full install, use:

- macOS/Linux: `./scripts/activate-skills.sh --clear Essentials`
- macOS/Linux: `./scripts/activate-skills.sh --clear "Web Wizard"`
- Windows: `.\scripts\activate-skills.bat --clear Essentials`

---
## Essentials & Core

### 🚀 The "Essentials" Starter Pack

_For everyone. Install these first._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`concise-planning`](../../skills/12_Deploy/CICD/concise-planning/): Always start with a plan.
- [`lint-and-validate`](../../skills/10_Test/Unit/lint-and-validate/): Keep your code clean automatically.
- [`git-pushing`](../../skills/09_Integ/3rdParty/git-pushing/): Save your work safely.
- [`kaizen`](../../skills/06_Core/AI/kaizen/): Continuous improvement mindset.
- [`systematic-debugging`](../../skills/10_Test/Unit/systematic-debugging/): Debug like a pro.

---

## Security & Compliance

### 🛡️ The "Security Engineer" Pack

_For pentesting, auditing, and hardening._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`ethical-hacking-methodology`](../../skills/10_Test/Pentest/ethical-hacking-methodology/): The Bible of ethical hacking.
- [`burp-suite-testing`](../../skills/07_UI/Web/burp-suite-testing/): Web vulnerability scanning.
- [`top-web-vulnerabilities`](../../skills/07_UI/Web/top-web-vulnerabilities/): OWASP-aligned vulnerability taxonomy.
- [`linux-privilege-escalation`](../../skills/07_UI/Web/linux-privilege-escalation/): Advanced Linux security assessment.
- [`cloud-penetration-testing`](../../skills/09_Integ/Cloud/cloud-penetration-testing/): AWS/Azure/GCP security.
- [`security-auditor`](../../skills/10_Test/Pentest/security-auditor/): Comprehensive security audits.
- [`vulnerability-scanner`](../../skills/10_Test/Pentest/vulnerability-scanner/): Advanced vulnerability analysis.

### 🔐 The "Security Developer" Pack

_For building secure applications._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`api-security-best-practices`](../../skills/05_API/REST/api-security-best-practices/): Secure API design patterns.
- [`auth-implementation-patterns`](../../skills/05_API/Auth/auth-implementation-patterns/): JWT, OAuth2, session management.
- [`backend-security-coder`](../../skills/10_Test/Pentest/backend-security-coder/): Secure backend coding practices.
- [`frontend-security-coder`](../../skills/07_UI/Web/frontend-security-coder/): XSS prevention and client-side security.
- [`cc-skill-security-review`](../../skills/10_Test/Pentest/cc-skill-security-review/): Security checklist for features.
- [`pci-compliance`](../../skills/01_Reqs/Legal/pci-compliance/): Payment card security standards.


---

## 🌐 Web Development

### 🌐 The "Web Wizard" Pack

_For building modern, high-performance web apps._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`frontend-design`](../../skills/07_UI/Web/frontend-design/): UI guidelines and aesthetics.
- [`react-best-practices`](../../skills/07_UI/Web/react-best-practices/): React & Next.js performance optimization.
- [`react-patterns`](../../skills/07_UI/Web/react-patterns/): Modern React patterns and principles.
- [`nextjs-best-practices`](../../skills/07_UI/Web/nextjs-best-practices/): Next.js App Router patterns.
- [`tailwind-patterns`](../../skills/06_Core/AI/tailwind-patterns/): Tailwind CSS v4 styling superpowers.
- [`form-cro`](../../skills/01_Reqs/Business/form-cro/): Optimize your forms for conversion.
- [`seo-audit`](../../skills/11_Opt/SEO/seo-audit/): Get found on Google.

### 🖌️ The "Web Designer" Pack

_For pixel-perfect experiences._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`ui-ux-pro-max`](../../skills/07_UI/Web/ui-ux-pro-max/): Premium design systems and tokens.
- [`frontend-design`](../../skills/07_UI/Web/frontend-design/): The base layer of aesthetics.
- [`3d-web-experience`](../../skills/07_UI/Web/3d-web-experience/): Three.js & React Three Fiber magic.
- [`canvas-design`](../../skills/07_UI/Web/canvas-design/): Static visuals and posters.
- [`mobile-design`](../../skills/07_UI/Web/mobile-design/): Mobile-first design principles.
- [`scroll-experience`](../../skills/01_Reqs/Business/scroll-experience/): Immersive scroll-driven experiences.

### ⚡ The "Full-Stack Developer" Pack

_For end-to-end web application development._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`senior-fullstack`](../../skills/08_Feat/Logic/senior-fullstack/): Complete fullstack development guide.
- [`frontend-developer`](../../skills/07_UI/Web/frontend-developer/): React 19+ and Next.js 15+ expertise.
- [`backend-dev-guidelines`](../../skills/07_UI/Web/backend-dev-guidelines/): Node.js/Express/TypeScript patterns.
- [`api-patterns`](../../skills/05_API/REST/api-patterns/): REST vs GraphQL vs tRPC selection.
- [`database-design`](../../skills/04_DB/SQL/database-design/): Schema design and ORM selection.
- [`stripe-integration`](../../skills/09_Integ/Payment/stripe-integration/): Payments and subscriptions.


---

## 🤖 AI & Agents

### 🤖 The "Agent Architect" Pack

_For building AI systems and autonomous agents._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`agent-evaluation`](../../skills/06_Core/AI/agent-evaluation/): Test and benchmark your agents.
- [`langgraph`](../../skills/06_Core/AI/langgraph/): Build stateful agent workflows.
- [`mcp-builder`](../../skills/07_UI/Web/mcp-builder/): Create your own MCP tools.
- [`prompt-engineering`](../../skills/06_Core/AI/prompt-engineering/): Master the art of talking to LLMs.
- [`ai-agents-architect`](../../skills/06_Core/AI/ai-agents-architect/): Design autonomous AI agents.
- [`rag-engineer`](../../skills/04_DB/Vector/rag-engineer/): Build RAG systems with vector search.

### 🧠 The "LLM Application Developer" Pack

_For building production LLM applications._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`llm-app-patterns`](../../skills/06_Core/AI/llm-app-patterns/): Production-ready LLM patterns.
- [`rag-implementation`](../../skills/04_DB/Vector/rag-implementation/): Retrieval-Augmented Generation.
- [`prompt-caching`](../../skills/06_Core/AI/prompt-caching/): Cache strategies for LLM prompts.
- [`context-window-management`](../../skills/03_Deps/PkgMgmt/context-window-management/): Manage LLM context efficiently.
- [`langfuse`](../../skills/06_Core/AI/langfuse/): LLM observability and tracing.


---

## 🎮 Game Development

### 🎮 The "Indie Game Dev" Pack

_For building games with AI assistants._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`game-development/game-design`](../../skills/08_Feat/Logic/game-development/game-design/): Mechanics and loops.
- [`game-development/2d-games`](../../skills/08_Feat/Logic/game-development/2d-games/): Sprites and physics.
- [`game-development/3d-games`](../../skills/08_Feat/Logic/game-development/3d-games/): Models and shaders.
- [`unity-developer`](../../skills/08_Feat/Games/unity-developer/): Unity 6 LTS development.
- [`godot-gdscript-patterns`](../../skills/01_Reqs/Content/godot-gdscript-patterns/): Godot 4 GDScript patterns.
- [`algorithmic-art`](../../skills/08_Feat/XR/algorithmic-art/): Generate assets with code.


---

## 🐍 Backend & Languages

### 🐍 The "Python Pro" Pack

_For backend heavyweights and data scientists._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`python-pro`](../../skills/08_Feat/Logic/python-pro/): Master Python 3.12+ with modern features.
- [`python-patterns`](../../skills/08_Feat/Logic/python-patterns/): Idiomatic Python code.
- [`fastapi-pro`](../../skills/05_API/REST/fastapi-pro/): High-performance async APIs.
- [`fastapi-templates`](../../skills/05_API/REST/fastapi-templates/): Production-ready FastAPI projects.
- [`django-pro`](../../skills/08_Feat/Logic/django-pro/): The battery-included framework.
- [`python-testing-patterns`](../../skills/10_Test/Unit/python-testing-patterns/): Comprehensive testing with pytest.
- [`async-python-patterns`](../../skills/08_Feat/Logic/async-python-patterns/): Python asyncio mastery.

### 🟦 The "TypeScript & JavaScript" Pack

_For modern web development._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`typescript-expert`](../../skills/01_Reqs/Content/typescript-expert/): TypeScript mastery and advanced types.
- [`javascript-pro`](../../skills/01_Reqs/Content/javascript-pro/): Modern JavaScript with ES6+.
- [`react-best-practices`](../../skills/07_UI/Web/react-best-practices/): React performance optimization.
- [`nodejs-best-practices`](../../skills/08_Feat/Logic/nodejs-best-practices/): Node.js development principles.
- [`nextjs-app-router-patterns`](../../skills/05_API/REST/nextjs-app-router-patterns/): Next.js 14+ App Router.

### 🦀 The "Systems Programming" Pack

_For low-level and performance-critical code._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`rust-pro`](../../skills/08_Feat/Logic/rust-pro/): Rust 1.75+ with async patterns.
- [`go-concurrency-patterns`](../../skills/08_Feat/Logic/go-concurrency-patterns/): Go concurrency mastery.
- [`golang-pro`](../../skills/08_Feat/Logic/golang-pro/): Go development expertise.
- [`memory-safety-patterns`](../../skills/08_Feat/Logic/memory-safety-patterns/): Memory-safe programming.
- [`cpp-pro`](../../skills/08_Feat/Logic/cpp-pro/): Modern C++ development.


---

## 🦄 Product & Business

### 🦄 The "Startup Founder" Pack

_For building products, not just code._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`product-manager-toolkit`](../../skills/01_Reqs/Business/product-manager-toolkit/): RICE prioritization, PRD templates.
- [`competitive-landscape`](../../skills/01_Reqs/Business/competitive-landscape/): Competitor analysis.
- [`competitor-alternatives`](../../skills/01_Reqs/Business/competitor-alternatives/): Create comparison pages.
- [`launch-strategy`](../../skills/01_Reqs/Business/launch-strategy/): Product launch planning.
- [`copywriting`](../../skills/01_Reqs/Content/copywriting/): Marketing copy that converts.
- [`stripe-integration`](../../skills/09_Integ/Payment/stripe-integration/): Get paid from day one.

### 📊 The "Business Analyst" Pack

_For data-driven decision making._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`business-analyst`](../../skills/01_Reqs/Business/business-analyst/): AI-powered analytics and KPIs.
- [`startup-metrics-framework`](../../skills/01_Reqs/Business/startup-metrics-framework/): SaaS metrics and unit economics.
- [`startup-financial-modeling`](../../skills/01_Reqs/Business/startup-financial-modeling/): 3-5 year financial projections.
- [`market-sizing-analysis`](../../skills/08_Feat/XR/market-sizing-analysis/): TAM/SAM/SOM calculations.
- [`kpi-dashboard-design`](../../skills/01_Reqs/Business/kpi-dashboard-design/): Effective KPI dashboards.

### 📈 The "Marketing & Growth" Pack

_For driving user acquisition and retention._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`content-creator`](../../skills/01_Reqs/Content/content-creator/): SEO-optimized marketing content.
- [`seo-audit`](../../skills/11_Opt/SEO/seo-audit/): Technical SEO health checks.
- [`programmatic-seo`](../../skills/11_Opt/SEO/programmatic-seo/): Create pages at scale.
- [`analytics-tracking`](../../skills/01_Reqs/Business/analytics-tracking/): Set up GA4/PostHog correctly.
- [`ab-test-setup`](../../skills/01_Reqs/Business/ab-test-setup/): Validated learning experiments.
- [`email-sequence`](../../skills/06_Core/AI/email-sequence/): Automated email campaigns.


---

## DevOps & Infrastructure

### 🌧️ The "DevOps & Cloud" Pack

_For infrastructure and scaling._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`docker-expert`](../../skills/03_Deps/Docker/docker-expert/): Master containers and multi-stage builds.
- [`aws-serverless`](../../skills/09_Integ/Cloud/aws-serverless/): Serverless on AWS (Lambda, DynamoDB).
- [`kubernetes-architect`](../../skills/08_Feat/XR/kubernetes-architect/): K8s architecture and GitOps.
- [`terraform-specialist`](../../skills/10_Test/Unit/terraform-specialist/): Infrastructure as Code mastery.
- [`environment-setup-guide`](../../skills/03_Deps/EnvSetup/environment-setup-guide/): Standardization for teams.
- [`deployment-procedures`](../../skills/12_Deploy/CICD/deployment-procedures/): Safe rollout strategies.
- [`bash-linux`](../../skills/07_UI/Web/bash-linux/): Terminal wizardry.

### 📊 The "Observability & Monitoring" Pack

_For production reliability._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`observability-engineer`](../../skills/12_Deploy/CICD/observability-engineer/): Comprehensive monitoring systems.
- [`distributed-tracing`](../../skills/12_Deploy/CICD/distributed-tracing/): Track requests across microservices.
- [`slo-implementation`](../../skills/12_Deploy/CICD/slo-implementation/): Service Level Objectives.
- [`incident-responder`](../../skills/12_Deploy/CICD/incident-responder/): Rapid incident response.
- [`postmortem-writing`](../../skills/01_Reqs/Content/postmortem-writing/): Blameless postmortems.
- [`performance-engineer`](../../skills/11_Opt/Perf/performance-engineer/): Application performance optimization.


---

## 📊 Data & Analytics

### 📊 The "Data & Analytics" Pack

_For making sense of the numbers._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`analytics-tracking`](../../skills/01_Reqs/Business/analytics-tracking/): Set up GA4/PostHog correctly.
- [`claude-d3js-skill`](../../skills/06_Core/AI/claude-d3js-skill/): Beautiful custom visualizations with D3.js.
- [`sql-pro`](../../skills/04_DB/SQL/sql-pro/): Modern SQL with cloud-native databases.
- [`postgres-best-practices`](../../skills/04_DB/SQL/postgres-best-practices/): Postgres optimization.
- [`ab-test-setup`](../../skills/01_Reqs/Business/ab-test-setup/): Validated learning.
- [`database-architect`](../../skills/04_DB/SQL/database-architect/): Database design from scratch.

### 🔄 The "Data Engineering" Pack

_For building data pipelines._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`data-engineer`](../../skills/04_DB/ETL/data-engineer/): Data pipeline architecture.
- [`airflow-dag-patterns`](../../skills/04_DB/ETL/airflow-dag-patterns/): Apache Airflow DAGs.
- [`dbt-transformation-patterns`](../../skills/04_DB/ETL/dbt-transformation-patterns/): Analytics engineering.
- [`vector-database-engineer`](../../skills/04_DB/SQL/vector-database-engineer/): Vector databases for RAG.
- [`embedding-strategies`](../../skills/04_DB/Vector/embedding-strategies/): Embedding model selection.


---

## 🎨 Creative & Content

### 🎨 The "Creative Director" Pack

_For visuals, content, and branding._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`canvas-design`](../../skills/07_UI/Web/canvas-design/): Generate posters and diagrams.
- [`frontend-design`](../../skills/07_UI/Web/frontend-design/): UI aesthetics.
- [`content-creator`](../../skills/01_Reqs/Content/content-creator/): SEO-optimized blog posts.
- [`copy-editing`](../../skills/01_Reqs/Content/copy-editing/): Polish your prose.
- [`algorithmic-art`](../../skills/08_Feat/XR/algorithmic-art/): Code-generated masterpieces.
- [`interactive-portfolio`](../../skills/07_UI/Web/interactive-portfolio/): Portfolios that land jobs.


---

## 🐞 Quality Assurance

### 🐞 The "QA & Testing" Pack

_For breaking things before users do._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`test-driven-development`](../../skills/08_Feat/Logic/test-driven-development/): Red, Green, Refactor.
- [`systematic-debugging`](../../skills/10_Test/Unit/systematic-debugging/): Debug like Sherlock Holmes.
- [`browser-automation`](../../skills/06_Core/Auto/browser-automation/): End-to-end testing with Playwright.
- [`e2e-testing-patterns`](../../skills/10_Test/E2E/e2e-testing-patterns/): Reliable E2E test suites.
- [`ab-test-setup`](../../skills/01_Reqs/Business/ab-test-setup/): Validated experiments.
- [`code-review-checklist`](../../skills/10_Test/Unit/code-review-checklist/): Catch bugs in PRs.
- [`test-fixing`](../../skills/10_Test/Unit/test-fixing/): Fix failing tests systematically.


---

## 🔧 Specialized Packs

### 📱 The "Mobile Developer" Pack

_For iOS, Android, and cross-platform apps._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`mobile-developer`](../../skills/07_UI/Mobile/mobile-developer/): Cross-platform mobile development.
- [`react-native-architecture`](../../skills/02_Arch/SysDesign/react-native-architecture/): React Native with Expo.
- [`flutter-expert`](../../skills/07_UI/Mobile/flutter-expert/): Flutter multi-platform apps.
- [`ios-developer`](../../skills/07_UI/Mobile/ios-developer/): iOS development with Swift.
- [`app-store-optimization`](../../skills/11_Opt/Perf/app-store-optimization/): ASO for App Store and Play Store.

### 🔗 The "Integration & APIs" Pack

_For connecting services and building integrations._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`stripe-integration`](../../skills/09_Integ/Payment/stripe-integration/): Payments and subscriptions.
- [`twilio-communications`](../../skills/09_Integ/3rdParty/twilio-communications/): SMS, voice, WhatsApp.
- [`hubspot-integration`](../../skills/06_Core/Auto/hubspot-integration/): CRM integration.
- [`plaid-fintech`](../../skills/06_Core/AI/plaid-fintech/): Bank account linking and ACH.
- [`algolia-search`](../../skills/08_Feat/XR/algolia-search/): Search implementation.

### 🎯 The "Architecture & Design" Pack

_For system design and technical decisions._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`senior-architect`](../../skills/02_Arch/SysDesign/senior-architect/): Comprehensive software architecture.
- [`architecture-patterns`](../../skills/02_Arch/SysDesign/architecture-patterns/): Clean Architecture, DDD, Hexagonal.
- [`microservices-patterns`](../../skills/01_Reqs/Business/microservices-patterns/): Microservices architecture.
- [`event-sourcing-architect`](../../skills/02_Arch/SysDesign/event-sourcing-architect/): Event sourcing and CQRS.
- [`architecture-decision-records`](../../skills/02_Arch/SysDesign/architecture-decision-records/): Document technical decisions.

### 🧱 The "DDD & Evented Architecture" Pack

_For teams modeling complex domains and evolving toward evented systems._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`domain-driven-design`](../../skills/02_Arch/SysDesign/domain-driven-design/): Route DDD work from strategic modeling to implementation patterns.
- [`ddd-strategic-design`](../../skills/02_Arch/SysDesign/ddd-strategic-design/): Subdomains, bounded contexts, and ubiquitous language.
- [`ddd-context-mapping`](../../skills/02_Arch/SysDesign/ddd-context-mapping/): Cross-context integration and anti-corruption boundaries.
- [`ddd-tactical-patterns`](../../skills/02_Arch/SysDesign/ddd-tactical-patterns/): Aggregates, value objects, repositories, and domain events.
- [`cqrs-implementation`](../../skills/02_Arch/SysDesign/cqrs-implementation/): Read/write model separation.
- [`event-store-design`](../../skills/07_UI/Web/event-store-design/): Event persistence and replay architecture.
- [`saga-orchestration`](../../skills/02_Arch/SysDesign/saga-orchestration/): Cross-context long-running transaction coordination.
- [`projection-patterns`](../../skills/02_Arch/SysDesign/projection-patterns/): Materialized read models from event streams.

### 🤖 The "Automation Builder" Pack

_For connecting tools and building repeatable automated workflows._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`workflow-automation`](../../skills/06_Core/Auto/workflow-automation/): Design durable automation flows for AI and business systems.
- [`mcp-builder`](../../skills/07_UI/Web/mcp-builder/): Create tool interfaces agents can use reliably.
- [`make-automation`](../../skills/06_Core/Auto/make-automation/): Build automations in Make/Integromat.
- [`airtable-automation`](../../skills/06_Core/AI/airtable-automation/): Automate Airtable records, bases, and views.
- [`notion-automation`](../../skills/06_Core/Auto/notion-automation/): Automate Notion pages, databases, and blocks.
- [`slack-automation`](../../skills/06_Core/Auto/slack-automation/): Automate Slack messaging and channel workflows.
- [`googlesheets-automation`](../../skills/06_Core/Auto/googlesheets-automation/): Automate spreadsheet updates and data operations.

### 💼 The "RevOps & CRM Automation" Pack

_For revenue operations, support handoffs, and CRM-heavy automation._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`hubspot-automation`](../../skills/06_Core/Auto/hubspot-automation/): Automate contacts, companies, deals, and tickets.
- [`sendgrid-automation`](../../skills/06_Core/Auto/sendgrid-automation/): Automate email sends, contacts, and templates.
- [`zendesk-automation`](../../skills/06_Core/Auto/zendesk-automation/): Automate support tickets and reply workflows.
- [`google-calendar-automation`](../../skills/06_Core/Auto/google-calendar-automation/): Schedule events and manage availability.
- [`outlook-calendar-automation`](../../skills/06_Core/Auto/outlook-calendar-automation/): Automate Outlook meetings and invitations.
- [`stripe-automation`](../../skills/06_Core/Auto/stripe-automation/): Automate billing, invoices, and subscriptions.
- [`shopify-automation`](../../skills/06_Core/Auto/shopify-automation/): Automate products, orders, customers, and inventory.

### 💳 The "Commerce & Payments" Pack

_For monetization, payments, and commerce workflows._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`stripe-integration`](../../skills/09_Integ/Payment/stripe-integration/): Build robust checkout, subscription, and webhook flows.
- [`paypal-integration`](../../skills/09_Integ/Payment/paypal-integration/): Integrate PayPal payments and related flows.
- [`plaid-fintech`](../../skills/06_Core/AI/plaid-fintech/): Link bank accounts and handle ACH-related use cases.
- [`hubspot-integration`](../../skills/06_Core/Auto/hubspot-integration/): Connect CRM data into product and revenue workflows.
- [`algolia-search`](../../skills/08_Feat/XR/algolia-search/): Add search and discovery to commerce experiences.
- [`monetization`](../../skills/09_Integ/Payment/monetization/): Design pricing and monetization systems deliberately.

### 🏢 The "Odoo ERP" Pack

_For teams building or operating around Odoo-based business systems._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`odoo-module-developer`](../../skills/06_Core/Auto/odoo-module-developer/): Create custom Odoo modules cleanly.
- [`odoo-orm-expert`](../../skills/06_Core/Auto/odoo-orm-expert/): Work effectively with Odoo ORM patterns and performance.
- [`odoo-sales-crm-expert`](../../skills/06_Core/Auto/odoo-sales-crm-expert/): Optimize sales pipelines, leads, and forecasting.
- [`odoo-ecommerce-configurator`](../../skills/03_Deps/EnvSetup/odoo-ecommerce-configurator/): Configure storefront, catalog, and order flows.
- [`odoo-performance-tuner`](../../skills/06_Core/Auto/odoo-performance-tuner/): Diagnose and improve slow Odoo instances.
- [`odoo-security-rules`](../../skills/05_API/Auth/odoo-security-rules/): Apply secure access controls and rule design.
- [`odoo-docker-deployment`](../../skills/03_Deps/Docker/odoo-docker-deployment/): Deploy and run Odoo in Docker-based environments.

### ☁️ The "Azure AI & Cloud" Pack

_For building on Azure across cloud, AI, and platform services._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`azd-deployment`](../../skills/12_Deploy/CICD/azd-deployment/): Ship Azure apps with Azure Developer CLI workflows.
- [`azure-functions`](../../skills/09_Integ/Cloud/azure-functions/): Build serverless workloads with Azure Functions.
- [`azure-ai-openai-dotnet`](../../skills/06_Core/AI/azure-ai-openai-dotnet/): Use Azure OpenAI from .NET applications.
- [`azure-search-documents-py`](../../skills/08_Feat/XR/azure-search-documents-py/): Build search, hybrid search, and indexing in Python.
- [`azure-identity-py`](../../skills/09_Integ/Cloud/azure-identity-py/): Handle Azure authentication flows in Python services.
- [`azure-monitor-opentelemetry-ts`](../../skills/09_Integ/Cloud/azure-monitor-opentelemetry-ts/): Add telemetry and tracing from TypeScript apps.

### 📲 The "Expo & React Native" Pack

_For shipping mobile apps with Expo and React Native._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`react-native-architecture`](../../skills/02_Arch/SysDesign/react-native-architecture/): Structure production React Native apps well.
- [`expo-api-routes`](../../skills/05_API/REST/expo-api-routes/): Build API routes in Expo Router and EAS Hosting.
- [`expo-dev-client`](../../skills/07_UI/Mobile/expo-dev-client/): Build and distribute Expo development clients.
- [`expo-tailwind-setup`](../../skills/03_Deps/EnvSetup/expo-tailwind-setup/): Set up Tailwind and NativeWind in Expo apps.
- [`expo-cicd-workflows`](../../skills/07_UI/Mobile/expo-cicd-workflows/): Automate builds and releases with EAS workflows.
- [`expo-deployment`](../../skills/07_UI/Mobile/expo-deployment/): Deploy Expo apps and manage release flow.
- [`app-store-optimization`](../../skills/11_Opt/Perf/app-store-optimization/): Improve App Store and Play Store discoverability.

### 🍎 The "Apple Platform Design" Pack

_For teams designing native-feeling Apple platform experiences._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`hig-foundations`](../../skills/07_UI/DesignSys/hig-foundations/): Learn the core Apple Human Interface Guidelines.
- [`hig-patterns`](../../skills/07_UI/DesignSys/hig-patterns/): Apply Apple interaction and UX patterns correctly.
- [`hig-components-layout`](../../skills/07_UI/Web/hig-components-layout/): Use Apple layout and navigation components well.
- [`hig-inputs`](../../skills/07_UI/DesignSys/hig-inputs/): Design for gestures, keyboards, Pencil, focus, and controllers.
- [`hig-components-system`](../../skills/07_UI/Web/hig-components-system/): Work with widgets, live activities, and system surfaces.
- [`hig-platforms`](../../skills/07_UI/DesignSys/hig-platforms/): Adapt experiences across Apple device families.

### 🧩 The "Makepad Builder" Pack

_For building UI-heavy apps with the Makepad ecosystem._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`makepad-basics`](../../skills/06_Core/Auto/makepad-basics/): Start with Makepad fundamentals and mental model.
- [`makepad-layout`](../../skills/06_Core/Auto/makepad-layout/): Handle sizing, flow, alignment, and layout composition.
- [`makepad-widgets`](../../skills/06_Core/Auto/makepad-widgets/): Build interfaces from Makepad widgets.
- [`makepad-event-action`](../../skills/06_Core/Auto/makepad-event-action/): Wire interaction and event handling correctly.
- [`makepad-shaders`](../../skills/06_Core/Auto/makepad-shaders/): Create GPU-driven visual effects and custom drawing.
- [`makepad-deployment`](../../skills/06_Core/Auto/makepad-deployment/): Package and ship Makepad projects.

### 🔎 The "SEO Specialist" Pack

_For technical SEO, content structure, and search growth._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`seo-fundamentals`](../../skills/11_Opt/SEO/seo-fundamentals/): Build from sound SEO principles and search constraints.
- [`seo-content-planner`](../../skills/01_Reqs/Content/seo-content-planner/): Plan clusters, calendars, and content gaps.
- [`seo-content-writer`](../../skills/01_Reqs/Content/seo-content-writer/): Produce search-aware content drafts with intent alignment.
- [`seo-structure-architect`](../../skills/08_Feat/XR/seo-structure-architect/): Improve hierarchy, internal links, and structure.
- [`seo-cannibalization-detector`](../../skills/11_Opt/SEO/seo-cannibalization-detector/): Find overlapping pages competing for the same intent.
- [`seo-content-auditor`](../../skills/01_Reqs/Content/seo-content-auditor/): Audit existing content quality and optimization gaps.
- [`schema-markup`](../../skills/08_Feat/XR/schema-markup/): Add structured data to support richer search results.

### 📄 The "Documents & Presentations" Pack

_For document-heavy workflows, spreadsheets, PDFs, and presentations._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`office-productivity`](../../skills/06_Core/Auto/office-productivity/): Coordinate document, spreadsheet, and presentation workflows.
- [`docx-official`](../../skills/08_Feat/Media/docx-official/): Create and edit Word-compatible documents.
- [`pptx-official`](../../skills/08_Feat/Media/pptx-official/): Create and edit PowerPoint-compatible presentations.
- [`xlsx-official`](../../skills/08_Feat/Media/xlsx-official/): Create and analyze spreadsheet files with formulas and formatting.
- [`pdf-official`](../../skills/08_Feat/Media/pdf-official/): Extract, generate, and manipulate PDFs programmatically.
- [`google-slides-automation`](../../skills/06_Core/Auto/google-slides-automation/): Automate presentation updates in Google Slides.
- [`google-sheets-automation`](../../skills/06_Core/Auto/google-sheets-automation/): Automate reads and writes in Google Sheets.


---

## 🧰 Maintainer & OSS

### 🛠️ The "OSS Maintainer" Pack

_For shipping clean changes in public repositories._

**Plugin status:** Codex plugin-safe · Claude plugin-safe

- [`commit`](../../skills/12_Deploy/CICD/commit/): High-quality conventional commits.
- [`create-pr`](../../skills/12_Deploy/CICD/create-pr/): PR creation with review-ready context.
- [`requesting-code-review`](../../skills/10_Test/Unit/requesting-code-review/): Ask for targeted, high-signal reviews.
- [`receiving-code-review`](../../skills/10_Test/Unit/receiving-code-review/): Apply feedback with technical rigor.
- [`changelog-automation`](../../skills/06_Core/Auto/changelog-automation/): Keep release notes and changelogs consistent.
- [`git-advanced-workflows`](../../skills/12_Deploy/CICD/git-advanced-workflows/): Rebase, cherry-pick, bisect, recovery.
- [`documentation-templates`](../../skills/01_Reqs/Content/documentation-templates/): Standardize docs and handoffs.

### 🧱 The "Skill Author" Pack

_For creating and maintaining high-quality SKILL.md assets._

**Plugin status:** Codex pending hardening · Claude pending hardening

- [`skill-creator`](../../skills/03_Deps/EnvSetup/skill-creator/): Design effective new skills.
- [`skill-developer`](../../skills/03_Deps/EnvSetup/skill-developer/): Implement triggers, hooks, and skill lifecycle.
- [`writing-skills`](../../skills/01_Reqs/Content/writing-skills/): Improve clarity and structure of skill instructions.
- [`documentation-generation-doc-generate`](../../skills/01_Reqs/Content/documentation-generation-doc-generate/): Generate maintainable technical docs.
- [`lint-and-validate`](../../skills/10_Test/Unit/lint-and-validate/): Validate quality after edits.
- [`verification-before-completion`](../../skills/10_Test/Unit/verification-before-completion/): Confirm changes before claiming done.

## 📚 How to Use Bundles

### 1) Pick by immediate goal

- Need to ship a feature now: `Essentials` + one domain pack (`Web Wizard`, `Python Pro`, `DevOps & Cloud`).
- Need reliability and hardening: add `QA & Testing` + `Security Developer`.
- Need product growth: add `Startup Founder` or `Marketing & Growth`.

### 2) Start with 3-5 skills, not 20

Pick the minimum set for your current milestone. Expand only when you hit a real gap.

### 3) Invoke skills consistently

- **Claude Code**: install a bundle plugin or use `>> /skill-name help me...`
- **Codex CLI**: install a bundle plugin where marketplaces are available, or use `Use skill-name...`
- **Cursor**: `@skill-name` in chat
- **Gemini CLI**: `Use skill-name...`

### 4) Build your personal shortlist

Keep a small list of high-frequency skills and reuse it across tasks to reduce context switching.

## 🧩 Recommended Bundle Combos

### Ship a SaaS MVP (2 weeks)

`Essentials` + `Full-Stack Developer` + `QA & Testing` + `Startup Founder`

### Harden an existing production app

`Essentials` + `Security Developer` + `DevOps & Cloud` + `Observability & Monitoring`

### Build an AI product

`Essentials` + `Agent Architect` + `LLM Application Developer` + `Data Engineering`

### Grow traffic and conversions

`Web Wizard` + `Marketing & Growth` + `Data & Analytics`

### Launch and maintain open source

`Essentials` + `OSS Maintainer` + `Architecture & Design`

---

## Learning Paths

### Beginner → Intermediate → Advanced

**Web Development:**

1. Start: `Essentials` → `Web Wizard`
2. Grow: `Full-Stack Developer` → `Architecture & Design`
3. Master: `Observability & Monitoring` → `Security Developer`

**AI/ML:**

1. Start: `Essentials` → `Agent Architect`
2. Grow: `LLM Application Developer` → `Data Engineering`
3. Master: Advanced RAG and agent orchestration

**Security:**

1. Start: `Essentials` → `Security Developer`
2. Grow: `Security Engineer` → Advanced pentesting
3. Master: Red team tactics and threat modeling

**Open Source Maintenance:**

1. Start: `Essentials` → `OSS Maintainer`
2. Grow: `Architecture & Design` → `QA & Testing`
3. Master: `Skill Author` + release automation workflows

---

## Contributing

Found a skill that should be in a bundle? Or want to create a new bundle? [Open an issue](https://github.com/sickn33/antigravity-awesome-skills/12_Deploy/CICD/issues) or submit a PR!

---

## Related Documentation

- [Getting Started Guide](getting-started.md)
- [Full Skill Catalog](../../CATALOG.md)
- [Contributing Guide](../../CONTRIBUTING.md)

---

_Last updated: March 2026 | Total Skills: 1,412+ | Total Bundles: 37_
