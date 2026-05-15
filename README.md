# CivicFlow Budget Operations Platform

CivicFlow is a portfolio-grade .NET 8 / SQL Server / Angular 18 application that models how a state-agency budget operations group would intake, route, audit, and reconcile budget change requests, HR funding changes, finance data corrections, and legacy integration issues.

It is built to look and feel like an application a Washington State Office of Financial Management developer would actually own: clean architecture, EF Core to SQL Server, T-SQL stored procedures, a real state machine, role-based authorization on every endpoint, ServiceNow-shape business-rule / transform-map / UI-policy abstractions, and two AI-assisted features that exercise schema-enforced LLM outputs, cost telemetry, and a governance kill-switch.

Live demo: https://waofm-demo.madewellrd.com  
Author: William Madewell — [william@madewellrd.com](mailto:william@madewellrd.com)

## What it does

- **Submit a request.** A requester drafts a budget change, HR funding change, data correction, integration issue, security/access change, or reporting request, picks an agency, fund, and program, and submits.
- **Route it.** The request flows through a 13-state workflow: Draft → Submitted → Triage → Analyst Review → Technical Review → Approved → Implemented → Closed, with side branches for ReturnedForCorrection, Blocked, Rejected, Cancelled, and Reopened. Every transition is gated by an in-memory state machine and audited.
- **Triage with AI.** On any open request, an analyst can ask the AI triage router for a queue assignment, complexity estimate, human-review flag, and three grounded "similar past requests" pulled from the same category. Every response is schema-validated, costed in USD, and labelled with provenance (real model, mock, kill-switch).
- **Repair imported data.** The Data Integration Repair Center accepts staged CSV-shaped batches, runs ten validation rules, lists failures by row, and offers an "Explain failures with AI" pass that returns agency-friendly guidance per row.
- **Watch the platform fire.** Server-side business rules (oversight threshold, legacy integration tag) fire on entity events and write to the audit log. Transform maps and UI policies are exposed declaratively at `/api/platform/*` for inspection.

## Stack

| Layer | Tech |
|---|---|
| API | ASP.NET Core 8 minimal APIs, claims-based authz, header-driven demo auth handler |
| Domain | Plain C# 12 records and aggregates, no framework dependencies |
| Persistence | EF Core 8 + SQL Server 2022, T-SQL stored procedures, raw-SQL initial migration |
| AI | Adapter pattern: Anthropic Messages API, deterministic mock, kill-switch decorator; cost telemetry baked in |
| Frontend | Angular 18 standalone components, functional HTTP interceptors, role switcher |
| Tests | xUnit on .NET 8 — 35 tests covering workflow, validation, auth policies, AI adapters, AI services, and business rules |
| Ops | Multi-stage Dockerfiles, docker-compose with healthchecks, env-driven config |

## Repository layout

```
src/
  CivicFlow.Domain/         Aggregates, value objects, enums, state machine
  CivicFlow.Application/    Use-case services, AI adapter contract, Platform layer
    Platform/               ServiceNow-shape BusinessRule, TransformMap, UIPolicy
    Ai/                     IModelAdapter, ModelRequest/Response, options
    Services/               RequestWorkflowService, ImportValidationService,
                            ImportErrorExplainerService, TriageRouterService
  CivicFlow.Infrastructure/ EF Core, repositories, model adapters
    Ai/                     AnthropicAdapter, DeterministicMockAdapter,
                            KillSwitchAdapter, PromptSchemaRegistry
    Seeding/                Idempotent demo data seeder
  CivicFlow.Api/            Minimal-API host, auth handler, policy table
frontend/civicflow-web/     Angular 18 SPA
database/stored-procedures/ T-SQL artefacts
docs/                       Architecture, runbook, threat model, observability
tests/CivicFlow.Tests/      xUnit suite
```

## Quick start

```bash
# 1. start the SQL Server container
docker compose up -d
# 2. restore, build, test
dotnet restore CivicFlow.sln
dotnet build CivicFlow.sln --configuration Release
dotnet test CivicFlow.sln --configuration Release
# 3. run the API (applies migrations and seeds demo data on startup)
dotnet run --project src/CivicFlow.Api
# 4. in another terminal, run the SPA
cd frontend/civicflow-web && npm install && npm start
```

API on http://localhost:5000, SPA on http://localhost:4200.

## Run the hosted demo locally

```bash
cp .env.example .env
# edit .env: at minimum set AI_PROVIDER=Mock (or Anthropic with a real key)
docker compose -f docker-compose.demo.yml up -d --build
```

The SPA is served on port 8080 (nginx) and reverse-proxies `/api` to the API container, so production behaviour matches the local stack.

## Demo identities

The seeder loads twelve users so an interviewer can switch roles without authoring data. Pick one in the SPA's role switcher (top-right).

| Role | Display name | Email |
|---|---|---|
| Requester | Riley Requester | requester@example.gov |
| Requester | Morgan Requester | morgan@dshs.example.gov |
| Requester | Jamie Requester | jamie@doh.example.gov |
| BudgetAnalyst | Bailey Analyst | analyst@example.gov |
| BudgetAnalyst | Drew Analyst | drew@example.gov |
| BudgetAnalyst | Taylor Analyst | taylor@example.gov |
| ApplicationDeveloper | Casey Developer | developer@example.gov |
| ApplicationDeveloper | Parker Developer | parker@example.gov |
| Approver | Avery Approver | approver@example.gov |
| Approver | Reese Approver | reese@example.gov |
| Admin | Sage Admin | admin@example.gov |
| ReadOnlyAuditor | Quinn Auditor | auditor@example.gov |

## AI configuration

The `Ai` section in `appsettings.json` (or env vars on the demo host) controls behaviour:

```jsonc
"Ai": {
  "Provider": "Mock",                      // "Mock" or "Anthropic"
  "KillSwitchEngaged": false,              // true short-circuits every call
  "AnthropicBaseUrl": "https://api.anthropic.com",
  "AnthropicModel": "claude-haiku-4-5-20251001",
  "MaxOutputTokensHardCap": 2048
}
```

Set `Ai__AnthropicApiKey` via env var (never check it in). Toggling `KillSwitchEngaged` to `true` immediately short-circuits both AI features with a deterministic safe-default response and writes a kill-switch event to the audit log.

## Tests

```bash
dotnet test CivicFlow.sln --configuration Release
```

35 tests across:

- workflow state machine and side effects
- import validation and transformation
- auth policy → role matrix
- IModelAdapter kill-switch, mock determinism, schema registry
- ImportErrorExplainerService end-to-end
- TriageRouterService end-to-end
- Business rules firing on entity events

## Documentation

| Doc | Purpose |
|---|---|
| `docs/ARCHITECTURE.md` | High-level decomposition and design decisions |
| `docs/API_CONTRACT.md` | Endpoint reference |
| `docs/DATABASE_SCHEMA.md` | Tables, indexes, FKs |
| `docs/RUNBOOK.md` | Day-2 operations |
| `docs/RELEASE_RUNBOOK.md` | Release procedure |
| `docs/DEPLOYMENT_PLAN.md` | Hosting and rollout |
| `docs/OBSERVABILITY_READINESS.md` | Logging, metrics, traces, alerts |
| `docs/SECURITY_THREAT_MODEL.md` | STRIDE walk-through |
| `docs/INCIDENT_CASE_STUDY.md` | Worked recovery scenario |
| `docs/TEST_PLAN.md` | Coverage strategy |
| `docs/ONE_PAGER.md` | One-page summary for non-technical readers |
