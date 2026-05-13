# CivicFlow Budget Operations Platform

CivicFlow is a portfolio-grade .NET 8, SQL Server, and Angular application that models state-agency budget request intake, workflow routing, audit history, and data import validation.

It is intentionally aligned to an enterprise application developer stack: ASP.NET Core 8, C#, SQL Server, EF Core, T-SQL stored procedures, JavaScript/HTML/CSS through Angular, REST endpoints, a mock legacy integration surface, role/group modeling, auditability, and production-support documentation.

## What is implemented

- Modular monolith solution structure.
- Domain model for requests, users, groups, reference data, imports, audit logs, incidents, and notifications.
- Request lifecycle state machine.
- Request workflow service with audit and notification side effects.
- Import validation service with duplicate, reference-data, fiscal-year, amount, and date checks.
- SQL Server EF Core infrastructure and repository implementations.
- SQL stored procedures for import validation, import summary, and request aging report.
- ASP.NET Core API endpoints for requests, workflow transitions, imports, reference data, health, and mock legacy lookup.
- Angular UI scaffold for request dashboard and import repair center.
- xUnit tests for workflow and import validation service behavior.
- Local CI validation script.
- Architecture, API, database, runbook, test plan, and incident case-study documentation.

## Local validation

The project intentionally uses local CI/CD instead of GitHub Actions:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\local_ci.ps1
```

Equivalent commands:

```bash
dotnet restore CivicFlow.sln
dotnet build CivicFlow.sln --configuration Release --no-restore
dotnet test CivicFlow.sln --configuration Release --no-build
python scripts/verify_static.py
```

## Local run

Requirements:

- .NET 8 SDK
- Docker Desktop or Docker Engine
- Node.js 20+ for the Angular UI

```bash
docker compose up -d
dotnet restore CivicFlow.sln
dotnet build CivicFlow.sln
dotnet test CivicFlow.sln
dotnet ef database update --project src/CivicFlow.Infrastructure --startup-project src/CivicFlow.Api
dotnet run --project src/CivicFlow.Api
```

API health check:

```bash
curl http://localhost:5000/health
```

Angular UI:

```bash
cd frontend/civicflow-web
npm install
npm start
```

## Demo users

| Role | User ID | Email |
|---|---|---|
| Requester | `10000000-0000-0000-0000-000000000001` | requester@example.gov |
| Budget Analyst | `10000000-0000-0000-0000-000000000002` | analyst@example.gov |
| Application Developer | `10000000-0000-0000-0000-000000000003` | developer@example.gov |
| Approver | `10000000-0000-0000-0000-000000000004` | approver@example.gov |

## Verification note

Backend tests and static repository verification are intended to run locally through `scripts/local_ci.ps1`.
