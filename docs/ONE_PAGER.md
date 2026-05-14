# CivicFlow — One-Page Summary

**What it is.** A portfolio-grade .NET 8 / SQL Server / Angular 18 application that mirrors the kind of system Washington State OFM application developers maintain: budget request intake, workflow routing, finance data correction, legacy integration repair, full audit and notification trails, ServiceNow-shape business-rule / transform-map / UI-policy engines, and two AI-assisted features built around a governed model-adapter pipeline.

**Who built it.** William Madewell. Designed and implemented over two days as the WOW project for the OFM IT Application Developer (Journey-in-Training) interview on 2026-05-15. Built using AI-orchestrated development — the same skill set documented in the resume (PROMETHEUS, ROGUE: OPS, Society) applied to a clean-room .NET application.

**Why it matters for the job.** The JD asks for .NET 8 with SQL Server, T-SQL stored procedures, JavaScript/HTML/CSS frontend work, REST integrations, Agile/DevOps practice, mission-critical 24/7 support skills, and ServiceNow developer fluency at the Journey level. CivicFlow exercises every one of those bullets in code that runs, with tests that pass, in an architecture that scales beyond the demo.

**What's in the box.**

- 13-state request workflow with deterministic transition guards.
- 11 role-based authorization policies enforced on every endpoint.
- 10-rule import validation pipeline with field-level error capture.
- 3 T-SQL stored procedures (validation, batch summary, aging report).
- ServiceNow-shape platform layer: BusinessRule engine with Before/After/Async phases, TransformMap and UIPolicy abstractions, all exposed at `/api/platform/*` for introspection.
- IModelAdapter pipeline with schema-enforced JSON output, cost telemetry per call, kill-switch decorator, deterministic mock for offline demos, real Anthropic client.
- Two AI features: import error explainer (translates validator output into agency-facing guidance) and triage router (recommends queue, complexity, human-review flag, similar past requests).
- Idempotent demo seeder: 5 agencies, 5 funds, 4 programs, 12 users across 6 roles, 15 sample requests covering every workflow state.
- Multi-stage Dockerfiles for API and SPA, docker-compose for the hosted demo, nginx reverse-proxy for same-origin operation.

**Numbers that matter.**

- 29/29 xUnit tests passing.
- 0 warnings, 0 errors at Release configuration.
- 13 statuses in the workflow state machine.
- 6 user roles, 11 policies, 0 endpoints without a policy.
- 1 line in `appsettings.json` flips AI from real Anthropic calls to deterministic mock.
- 1 line in `appsettings.json` engages the kill-switch and short-circuits every model call with a logged event.

**Architecture in one sentence.** Clean-architecture .NET 8 (Domain → Application → Infrastructure → Api) with EF Core 8 to SQL Server, an in-process state machine, a ServiceNow-shape platform layer that mirrors the vocabulary OFM uses today, and an AI adapter pipeline that brings the resume's governance-first MLOps patterns into a system whose stack matches the JD.

**Demo.** Live at https://waofm-demo.madewellrd.com. Local in two commands: `docker compose -f docker-compose.demo.yml up -d --build`, then open the SPA at port 8080.
