# API Contract

Base URL: `/api`

## Requests

`GET /requests` returns the latest 100 requests.

`GET /requests/{id}` returns one request.

`POST /requests` creates a draft request.

Request body:

```json
{
  "title": "Agency budget data correction",
  "category": 2,
  "agencyId": "20000000-0000-0000-0000-000000000001",
  "requesterId": "10000000-0000-0000-0000-000000000001",
  "fundId": null,
  "budgetProgramId": null,
  "estimatedAmount": 1000,
  "businessJustification": "Correct a legacy fund code."
}
```

Workflow endpoints:

- `POST /requests/{id}/submit?actorUserId={guid}`
- `POST /requests/{id}/triage?actorUserId={guid}`
- `POST /requests/{id}/analyst-review?actorUserId={guid}`
- `POST /requests/{id}/technical-review?actorUserId={guid}`
- `POST /requests/{id}/approve?actorUserId={guid}`
- `POST /requests/{id}/implemented?actorUserId={guid}`
- `POST /requests/{id}/close?actorUserId={guid}`
- `POST /requests/{id}/reject`

## Imports

`POST /imports/budget-requests` creates and validates an import batch.

`GET /imports/{id}` returns an import batch summary.

`GET /imports/{id}/errors` returns rejected rows for an import batch.

`POST /imports/{id}/validate` revalidates an existing batch.

`POST /imports/{id}/transform` creates submitted CivicFlow requests from valid import rows and marks those rows transformed.

Request body:

```json
{
  "actorUserId": "10000000-0000-0000-0000-000000000001"
}
```

## Reference data

- `GET /reference/agencies`
- `GET /reference/funds`

## Integration mock

`GET /integrations/legacy-budget/{agencyCode}/{fundCode}` returns a mock legacy budget lookup response.

## Health

`GET /health` checks API and SQL Server connectivity.
