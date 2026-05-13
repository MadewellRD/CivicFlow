# Observability Readiness

## Implemented

- ASP.NET Core logging configuration.
- Health check endpoint at `/health`.
- SQL Server health check through `AddDbContextCheck`.
- Audit log entity for material business changes.
- Notification message entity for workflow side effects.

## Recommended next telemetry work

- Add correlation ID middleware.
- Add structured logging event IDs for workflow transitions and imports.
- Add request duration metrics.
- Add import validation failure metrics by field.
- Add dashboard widgets for request aging and rejected import row count.

## Operational signals

| Signal | Source | Action |
|---|---|---|
| SQL health check fails | `/health` | verify Docker SQL Server and connection string |
| Import rejection spike | ImportValidationErrors | check agency/fund/program reference data |
| Workflow stuck in review | Request aging report | notify analyst/developer group |
| Unauthorized transition attempts | future auth logs | review role policy and actor |
