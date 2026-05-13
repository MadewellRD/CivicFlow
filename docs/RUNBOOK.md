# CivicFlow Support Runbook

## API will not start

Check whether SQL Server is running:

```bash
docker compose ps
docker compose logs sqlserver
```

Verify the connection string in `src/CivicFlow.Api/appsettings.json`.

## Database migration fails

Confirm the .NET EF tool is installed:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/CivicFlow.Infrastructure --startup-project src/CivicFlow.Api
```

## Import batch rejects rows unexpectedly

Check:

- Agency code exists and is active.
- Fund code exists and is active.
- Program code exists and is active.
- Request number is not duplicated.
- Fiscal year is between 2024 and 2035.
- Amount is not negative and not over the automatic threshold.
- Effective date is parseable.

Use `dbo.GetImportBatchSummary` for a database-level summary.

## Import batch will not transform valid rows

Check:

- Batch has rows with `RowStatus = 'Valid'`.
- Agency, fund, and budget program codes still exist and are active.
- Request numbers do not already exist in CivicFlow.
- API actor user ID is a valid seeded or future authenticated user.

Use `POST /api/imports/{id}/transform` for application-level transform so audit and request history are preserved. Use `dbo.TransformValidImportRows` only for database-level support diagnostics or controlled maintenance.

## Request is stuck in workflow

Review allowed transitions in `RequestWorkflow.cs`. Do not update status directly in SQL unless recovering a non-production demo database. Use the API workflow endpoints so audit history is preserved.

## Legacy integration lookup fails

The v1 endpoint is a mock. If this were production, check outbound network connectivity, credentials, SOAP envelope shape, timeout settings, and correlation ID logs.
