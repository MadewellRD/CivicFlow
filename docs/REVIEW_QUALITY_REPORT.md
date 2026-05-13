# Review Quality Report

## Scope reviewed

- Solution structure and project references.
- Domain model and request workflow state machine.
- Application service orchestration for request workflow and import validation.
- SQL Server persistence layer and EF Core repository boundaries.
- API endpoint surface.
- Angular UI scaffold.
- Tests and documentation artifacts.

## Findings

### RQ-001: Runtime verification requires Docker SQL Server

Status: open runtime limitation.

Local .NET build/test verification passes through `scripts/local_ci.ps1`. SQL-backed runtime verification still requires Docker SQL Server and EF migration execution.

Mitigation: exact local commands are included in `README.md` and deployment docs.

### RQ-002: Authorization is modeled but not enforced on every endpoint

Status: known v1 gap.

Seeded user IDs and roles exist, but endpoint-level authorization attributes and ASP.NET Identity are intentionally deferred. This is acceptable for portfolio MVP but should be called out in interview discussion.

Mitigation: add Issue 17 to implement ASP.NET Identity or a lightweight demo auth handler.

### RQ-003: Migration snapshot is skeletal

Status: known v1 technical debt.

The initial migration uses raw SQL to provide deterministic table creation. The model snapshot is intentionally minimal. A real EF-generated migration should replace it once dotnet-ef is available.

Mitigation: run `dotnet ef migrations remove`, then regenerate `InitialCreate` with `dotnet ef migrations add InitialCreate` in a .NET 8 environment.

## Approval recommendation

Approve for portfolio MVP packaging with one explicit caveat: runtime verification is pending on a .NET 8 machine.
