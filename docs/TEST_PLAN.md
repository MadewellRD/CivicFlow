# Test Plan

## Unit tests

- Verify valid request transitions.
- Verify invalid transitions throw DomainException.
- Verify submit writes audit and notification side effects.
- Verify valid import row is accepted.
- Verify invalid import row is rejected with field errors.

## Integration tests to add next

- Create request through API and verify persisted request.
- Submit request through API and verify status history row.
- Create import batch through API and verify import errors.
- Verify health check fails when SQL Server is unavailable.

## Manual smoke test

1. Start SQL Server with Docker Compose.
2. Apply EF migration.
3. Run API.
4. Open Angular UI.
5. Create a request.
6. Submit the request.
7. Run sample import.
8. Confirm one valid row and two rejected rows appear.
