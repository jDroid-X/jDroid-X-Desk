---
description: Execute the 7-Stage Simple Waterfall (OOPS based MVC) Closed Loop Workflow from Idea to Final Product with continuous improvement feedback loops.
---

# /mvc-closed-loop-waterfall: 7-Stage Closed Loop OOPS based MVC Workflow

Execute the comprehensive **Simple Waterfall (OOPS based MVC) – Closed Loop Workflow From Idea To Final Product** as defined in [ANTIGRAVITY_FLOW.md](file:///c:/Users/jiten/jAnitGravity/.kit/ANTIGRAVITY_FLOW.md).

## Workflow Execution Stages

### Stage 0: PONYTAIL CHECK
- **Execute**: Run the Ponytail Decision Ladder. If resolved via native solution or minimum lines (Steps 1-6), bypass the 7-stage waterfall. If custom architecture is required (Step 7), proceed to Stage 1.

### Stage 1: INITIATE & PLAN
- Identify Problem → Market / User Research → Define Goals → Feasibility Analysis → Define Scope → Create Project Plan → Budget & Timeline → Approval & Kickoff.
- **Deliverables**: Project Plan, Feasibility Report.

### Stage 2: REQUIREMENTS ANALYSIS
- Stakeholder Interviews → Requirement Gathering → Analyze & Document → Requirements Validation → Prioritize Requirements → SRS Sign-off.
- **Deliverables**: SRS Document, Use Case Document.

### Stage 3: SYSTEM DESIGN (SCREEN-FIRST OOPS MVC ARCHITECTURE)
- Screen Layout Hierarchy & UI/UX Design (View & Affordance Matrix) → Controller & Action Logic Design → Database Design (Model derived strictly from Screen State) → Class Diagram → Architecture Sign-off.
- **Deliverables**: UI/UX Design & Affordance Matrix, System Design Doc, Database Schema.

### Stage 4: IMPLEMENTATION (CODING - OUTSIDE-IN SEQUENCE)
- Setup Development Env → Develop Views & Reactive UI Components (HTML/CSS/DOM) → Develop Controllers & Action Handlers → Develop Models & DB Tables → Integrate Components → Code Review → Build / Compile.
- **Deliverables**: UI Views & Components, Controllers, Database & Models, Source Code.
- **Dynamic OOPS based MVC Mandate (Screen-First)**:
  - `User ➔ Screen (View) ➔ User Action Trigger ➔ Controller (Handler) ➔ Request Data ➔ Model (Persistence)`
  - `Model ➔ Return Data ➔ Controller ➔ Process & Update ➔ Screen (View) ➔ Human Feedback`
  - Use Program Mapping as master relationship table. Avoid duplicate code via call-by-reference.

### Stage 5: TESTING
- Test Plan Creation → Unit Testing (Model/View/Controller) → Integration Testing → System Testing → UAT → Fix Defects → Test Sign-off.
- **Deliverables**: Test Cases, Test Reports, Bug Reports.

### Stage 6: DEPLOYMENT
- Server Setup → App Build / Package → Configuration Setup → Database Migration → Deploy Application → Smoke Test → Go Live.
- **Deliverables**: Deployed Application, User Manual, Release Notes.

### Stage 7: MAINTENANCE & SUPPORT
- Monitor Application → Log & Error Tracking → Issue / Bug Reporting → Fixes & Patches → Optimize Performance → User Support → Enhancements.
- **Deliverables**: Support Reports, Change Logs, Updates.

---

## Closed Loop Feedback Convergence
Whenever feedback or bugs are identified at any stage:
`User Feedback -> Analyze -> Identify Issues & Improvements -> Update Plan & Requirements -> Implement Changes -> Re-Test & Deploy`
Every branch must either close logically or merge back into the main waterfall workflow.
