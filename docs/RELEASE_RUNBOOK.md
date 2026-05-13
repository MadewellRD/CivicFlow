# Release Runbook

## Version

Initial portfolio MVP: `v0.1.0`

## Preconditions

- `dotnet restore CivicFlow.sln` passes.
- `dotnet build CivicFlow.sln --configuration Release` passes.
- `dotnet test CivicFlow.sln --configuration Release` passes.
- `python3 scripts/verify_static.py` passes.
- SQL Server starts through Docker Compose.
- EF migration applies cleanly.

## Release steps

```bash
git checkout main
git pull
git checkout -b release/v0.1.0
dotnet restore CivicFlow.sln
dotnet build CivicFlow.sln --configuration Release
dotnet test CivicFlow.sln --configuration Release
python3 scripts/verify_static.py
git tag v0.1.0
```

## Rollback

This is a portfolio/local release. Rollback means deleting the local database volume and returning to the previous git tag.

```bash
docker compose down -v
git checkout <previous-tag>
```

## Known release caveats

- Real authentication is deferred.
- ServiceNow integration is simulated.
- Migration should be regenerated with `dotnet ef` in the target environment.
