# Database Schema

## Core tables

- Users
- Groups
- UserGroups
- Agencies
- Funds
- BudgetPrograms
- CatalogItems
- CatalogFieldDefinitions
- Requests
- RequestStatusHistory
- RequestComments
- ImportBatches
- ImportStagingRows
- ImportValidationErrors
- AuditLogs
- IncidentReports
- NotificationMessages

## Stored procedures

- `dbo.ValidateImportBatch`
- `dbo.GetImportBatchSummary`
- `dbo.GetRequestAgingReport`

## Design notes

Requests have a unique `RequestNumber`, string-converted enum fields for readability, and child collections for comments and status history.

Import batches retain every staged row and every row-level validation error so a support analyst can diagnose data quality failures without losing the original submission context.

Audit logs preserve actor, action, entity, summary, and timestamp for operational traceability.
