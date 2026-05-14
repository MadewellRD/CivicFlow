# CivicFlow Interview Talking Points — STAR cheat sheet

A compact, scannable companion to the deck. Each row maps a likely interview question to:

- a **STAR story** (Situation / Task / Action / Result) from prior work,
- the **CivicFlow artifact** that demonstrates the same pattern,
- a **bridge sentence** to use out loud.

Keep this open on a second screen during the interview. Don't read from it — it's a confidence anchor, not a script.

---

## Self-positioning anchor (lead with this)

> I'm a self-taught AI platform engineer with 25 years of enterprise IT behind me, applying for an IT Application Developer role. The honest read of my resume is that I haven't shipped a career's worth of production .NET. So I built CivicFlow over two days — a .NET 8 / SQL Server / Angular 18 app with the exact shape of an OFM application — to prove I can write the code the team owns, not just talk about it. The AI features in it are the same patterns I shipped at Providence and the City of Seattle, ported to your stack with schema enforcement, cost telemetry, and a governance kill-switch.

---

## Likely questions → STAR + CivicFlow bridge

### Q1 — "Walk me through a complex .NET project you've built."

**Story.** CivicFlow itself is the answer. Treat the project as the STAR.

- **Situation.** Job posting calls for .NET 8 + SQL + Angular + ServiceNow-shape work. My resume foregrounds AI/MLOps. I needed to make the JD-stack fluency visible.
- **Task.** Build a portfolio-grade application in two days that exercises every Journey-level JD bullet, with a real architecture, not a CRUD demo.
- **Action.** Clean-architecture .NET 8 (Domain/Application/Infrastructure/Api), EF Core with raw-SQL initial migration, T-SQL stored procedures, in-process state machine with 13 statuses, 11 role-based authz policies, idempotent demo seeder, Dockerized stack, two AI features over a governed adapter pipeline, ServiceNow-shape platform layer.
- **Result.** 29 passing tests, zero warnings at Release, end-to-end Dockerized demo, all major JD bullets visibly addressed in code that builds clean.

**Bridge.** "Happy to share screen and click through any layer — domain, services, AI pipeline, platform layer."

---

### Q2 — "Tell me about a time you supported a mission-critical 24/7 system."

**Story — Providence Health & Services real-time monitoring (2024–2025).**

- **S.** Wallero Technologies contracted me to Providence to maintain 24/7 reliability of real-time monitoring systems across active hospital environments. HIPAA-adjacent. PHI-safe design requirements. Mean time to resolution under strict SLA.
- **T.** Standardize incident classification, accelerate fault diagnosis across hardware/software/network failure modes, keep audit documentation tight.
- **A.** Built AI-augmented runbook automation using LLM-assisted triage prompts to classify incidents, surface resolution precedents from historical tickets, and generate audit-ready resolution notes. Enforced PHI-safe data governance through the prompt pipeline so model inputs never leaked regulated content.
- **R.** Cut MTTR enough to stay inside SLA on hospital life-safety platforms. Pattern formalized into a reusable triage system later applied to the GPT-3 task force work.

**Bridge.** "Same pattern is alive in CivicFlow as the Triage Router — same shape, schema-enforced output, grounded in past tickets, defaults to safe + human review when confidence is low."

---

### Q3 — "Describe a high-volume backlog you cleared on a tight timeline."

**Story — City of Seattle NTFS Permissions Analyst (Jun–Nov 2023).**

- **S.** 9-month backlog of 1,800 access requests. Manual review process. No SLA.
- **T.** Establish a 3-day SLA, clear the backlog, prevent recurrence.
- **A.** Built a custom AI governance tool with the OpenAI API and Python that automated request classification, approval/denial routing, and audit-trail generation. Designed prompt schemas for consistent JSON output. Validated model decisions against policy rules before any access change landed. Iterated prompts to drive misclassification down.
- **R.** Cleared all 1,800 backlogged requests in 30 days. Established a 3-day SLA. Surfaced least-privilege violations across SharePoint Online, Teams, OneDrive, and Azure AD as a byproduct.

**Bridge.** "Translating policy into deterministic technical enforcement — exactly the pattern CivicFlow's Business Rule engine implements. Each rule has a clear condition, a clear action, and writes an audit entry on every execution."

---

### Q4 — "How do you handle AI safely in production?"

**Story — ROGUE: OPS governance framework + CivicFlow adapter pipeline.**

- **S.** Designing AI systems for high-risk decision domains where a bad model output has real-world cost. Healthcare, access control, financial workflows.
- **T.** Make safety properties enforceable in code, not in policy documents.
- **A.** Designed ROGUE: OPS with deterministic constraints, non-overridable execution boundaries, tiered authority (LAW/PB/REF), and explicit kill-switch logic. Carried the pattern into CivicFlow as a four-layer adapter pipeline: schema-enforced output via `IPromptSchemaRegistry`, cost telemetry on every invocation, a `KillSwitchAdapter` decorator that short-circuits with a deterministic safe default and audit event, a `DeterministicMockAdapter` for offline demos and CI.
- **R.** In CivicFlow, every AI invocation is observable (provider, tokens, USD, latency, mock/kill-switch flags), governable (one config flag disables it), and recoverable (safe defaults flow through to the UI when the model fails). 29 tests lock down the contract.

**Bridge.** "The interview demo will show the kill-switch flipping live. The model says nothing, the audit log records why, the UI keeps working."

---

### Q5 — "Have you done ServiceNow development?"

**Honest answer.** No. Use this directly.

- **S.** JD calls out ServiceNow at the Journey level: UI Actions, UI Policies, Business Rules, Client Scripts, Script Includes, Transform Maps.
- **T.** Demonstrate that I think in the right vocabulary even without a year of SN tickets behind me.
- **A.** Built `CivicFlow.Application.Platform`: an `IBusinessRule` contract with Name, Table, Phase (Before/After/Async), Order, Condition, Run; a `BusinessRuleEngine` that runs the matching set at each phase of a request transition; two concrete rules (oversight threshold flag, legacy integration tag) that fire on real transitions and write audit entries. `ITransformMap` declares source/target tables and a FieldMaps list with optional transform scripts. `UiPolicyCatalog` exposes form-name-scoped policies the SPA consumes.
- **R.** Three GET endpoints under `/api/platform/*` expose every rule, map, and policy for introspection. Comments in the code map every concept back to its ServiceNow analogue.

**Bridge.** "I won't pretend I've owned a SN instance. I will pick it up fast because I already think in its model. Slide 8 walks the panel through this."

---

### Q6 — "What's your approach to data integration and reconciliation?"

**Story — CivicFlow Data Integration Repair Center + Providence M365 governance work.**

- **S.** OFM imports data from agency systems and legacy budget tools. Bad rows can't reach production tables silently — they need to be caught, explained, and either repaired or quarantined.
- **T.** Build an import pipeline that is observable, idempotent, and analyst-friendly.
- **A — CivicFlow.** ImportValidationService runs ten validation rules per row (request number uniqueness, reference data hit, fiscal year band, amount band, title, effective-date parse). Failed rows stay in staging with field-level errors. Valid rows can be transformed into Requests in a separate operation. AI Import Error Explainer translates raw validator errors into agency-facing guidance.
- **A — Providence.** Same shape at scale: 52 hospital sites, 16,000+ mailboxes, 100TB+ migrated, scripted automation, rule-based classification, policy-driven enforcement. The reusable compliance-as-code foundation built there is what made the AI patterns possible later.
- **R.** Reconciliation that can be audited, paused, resumed, and explained to a non-developer.

**Bridge.** "Demo will show three rows: one valid, one with bad reference codes, one over the threshold. We'll watch the AI explainer turn `AgencyCode: Agency code was not found or is inactive` into `Please use one of: OFM, DSHS, DOH, DOL, DCYF.`"

---

### Q7 — "Tell me about mentoring or technical guidance you've provided."

**Story — Goodrich / UTC Aerospace executive desktop support team (2011–2014).**

- **S.** Six Level-2 engineers across four states supporting the executive leadership team.
- **T.** Maintain a 96%+ on-time delivery rate on a Windows 7 migration for 13,000 devices.
- **A.** Built standardized SCCM/WDS/PowerShell templates for the team. Documented runbooks. Ran weekly office hours with the L2 group, pair-debugged hard tickets, set the bar for documentation quality so that the playbook outlived me.
- **R.** Hit 96.4% on-time. The team operated independently in the regions I wasn't physically in.

**Bridge.** "I lead by writing the runbook, not by tribal knowledge. The CivicFlow docs directory is in the same spirit — ARCHITECTURE, API_CONTRACT, RUNBOOK, INCIDENT_CASE_STUDY, SECURITY_THREAT_MODEL — so a new developer on the team can be productive in a day."

---

### Q8 — "What would you do in your first 30 / 60 / 90 days at OFM?"

**Day 1–30 — listen and ramp.**

- Read the production codebase. Map the deployment topology. Sit with two senior developers and one analyst.
- Inherit a low-risk bug or small feature; ship it end-to-end so I learn the release pipeline.
- Document the actual rather than aspirational architecture.

**Day 31–60 — earn small.**

- Pick one repeated operational pain point and fix it with a runbook + small automation.
- Pair with the ServiceNow developer on a real script include or transform map; learn the platform on a real ticket, not a sandbox.
- Stand up unit-test coverage where it's missing, no big rewrites.

**Day 61–90 — propose something useful.**

- Bring one of CivicFlow's patterns into the production system if it earns its place — most likely the schema-enforced AI adapter or a structured audit-log query view, depending on what's already there.
- Mentor a junior developer through one workflow start to finish.
- Take an on-call shift before the quarter ends.

**Bridge.** "I'm not coming in to rewrite anything. I'm coming in to learn what's load-bearing, fix one operational hotspot, and earn my standing in the team."

---

### Q9 — "What's the hardest production incident you've handled?"

**Story — Providence GPT-3 incident triage task force (2020–2022).**

- **S.** Specific quarter, six-figure incident backlog across four ITSM queues, 6,800 tickets, a deadline driven by leadership. Critical-care site reliability impact downstream.
- **T.** Cut resolution time enough to clear the backlog within the quarter.
- **A.** Built an AI-assisted triage system on GPT-3 that classified incidents by severity, category, and escalation path. Tuned prompts to reduce misrouting. Validated outputs against policy. Kept humans in the loop for low-confidence decisions.
- **R.** Cleared the 6,800-incident backlog in 60 days. Pattern reused in two subsequent task forces.

**Bridge.** "The CivicFlow triage router is the same shape: classify, route, ground in past tickets, default safe on low confidence."

---

### Q10 — "What's a project that didn't go well? What did you learn?"

**Story — Early local LLM inference rollout.**

- **S.** Self-hosted llama.cpp/BitNet stack on Ubuntu for RAG experimentation. First attempt assumed I could split inference between a heavy reasoning model and a lightweight chat model with naive round-robin routing.
- **T.** Hit acceptable latency for an interactive workflow.
- **A.** Naive round-robin produced unpredictable latency tail. I had to learn the relationship between context length, token limits, OpenBLAS build flags, and OMP thread tuning. Rebuilt the stack with explicit workload routing (reasoning vs. chat) via OpenWebUI, with systemd-managed services and persistent caches.
- **R.** Stable latency. The lesson: model selection is half the work; the other half is matching workload shape to runtime configuration.

**Bridge.** "Same lesson shows up in CivicFlow: the IModelAdapter contract is explicit about input/output schemas, max tokens, and temperature — and there's a hard cap from config so a single prompt can't run away on cost."

---

## Things to say out loud at least once

- "Schema-enforced output." Two words, says volumes.
- "Kill-switch as a config flag, audit event written on every short-circuit."
- "Same pattern, ported to .NET 8."
- "I'd rather show you the code."

## Things not to say

- "I'm self-taught." (True, but the work proves it. Lead with the work, not the framing.)
- "I haven't done ServiceNow." (Reframe as "I haven't owned an instance, but here's me modeling the concepts.")
- "Just an AI guy." (You are not.)
- Anything that sounds like reading from this document. They have to hear *you*.

## Pre-interview ritual (30 minutes before)

1. Reset CivicFlow demo state on the host.
2. Run a smoke triage on Riley Requester — confirm the AI feature still responds.
3. Flip kill-switch on, off, on, off — make sure the toggle works.
4. Have the deck PDF open in tab 1, the demo URL in tab 2, the GitHub-style repo browser in tab 3, this cheat sheet on the second monitor.
5. Two glasses of water. One pen and one notepad on the desk for live notes.
