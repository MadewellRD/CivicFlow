# CivicFlow Architecture

## Executive summary

CivicFlow is a modular monolith. The backend is an ASP.NET Core 8 Web API separated into Domain, Application, Infrastructure, and API projects. SQL Server is the persistence layer. EF Core handles normal entity persistence. T-SQL stored procedures handle batch-oriented import validation and operational reports.

## Scope

In scope:

- Budget/HR/finance request intake and workflow.
- ServiceNow-inspired catalog, role/group, business rule, UI policy, and transform-map concepts.
- Data Integration Repair Center.
- SQL-backed audit history and reporting.
- Angular request and import UI.
- Local Docker SQL Server deployment.

Out of scope for v1:

- Real ServiceNow API integration.
- Real identity provider integration.
- Production cloud deployment.
- Full WCAG accessibility audit.

## Component boundaries

| Component | Responsibility |
|---|---|
| CivicFlow.Domain | Entities, enums, invariants, request workflow transitions |
| CivicFlow.Application | DTOs, service orchestration, repository abstractions |
| CivicFlow.Infrastructure | EF Core DbContext, repositories, SQL Server integration, audit/notification persistence |
| CivicFlow.Api | HTTP endpoints, health checks, composition root |
| civicflow-web | Angular dashboard and import repair UI |

## Key design decisions

- Modular monolith over microservices to maximize delivery speed and keep enterprise workflow/data integrity visible.
- SQL Server over SQLite because T-SQL and stored procedures are part of the target role stack.
- EF Core plus stored procedures to demonstrate both modern ORM use and database-level reasoning.
- Seeded users/roles for v1, with ASP.NET Identity deferred.
- Local ServiceNow concept simulation rather than a ServiceNow dependency.

## Data flow

Request creation enters through Angular or API, passes through RequestWorkflowService, persists through RequestRepository and CivicFlowDbContext, writes AuditLog, and enqueues NotificationMessage.

Import creation accepts staged rows, validates against reference data and duplicate constraints, writes row-level validation errors, and returns a summary for repair. Transform turns valid rows into submitted CivicFlow requests, marks staging rows transformed, and writes audit history.

## Security considerations

- Server-side authorization must be enforced before v1 is production-like.
- Client validation is only a usability aid.
- CSV inputs are treated as untrusted.
- Audit logs are append-only by convention.
- Stored procedures use parameters, not dynamic SQL.

## Verification implications

- Domain transition tests prove allowed and forbidden lifecycle paths.
- Application service tests prove audit and notification side effects.
- Import validation tests prove accepted/rejected row behavior.
- Import transform tests prove valid rows create submitted requests.
- API integration tests should be added after the first successful database migration.
