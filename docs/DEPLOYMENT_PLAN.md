# Deployment Plan

## Target environment

Local development and portfolio review environment.

## Components

- SQL Server 2022 Developer container.
- ASP.NET Core 8 API running locally.
- Angular development server running locally.

## Deployment flow

```bash
docker compose up -d
dotnet restore CivicFlow.sln
dotnet build CivicFlow.sln
dotnet ef database update --project src/CivicFlow.Infrastructure --startup-project src/CivicFlow.Api
dotnet run --project src/CivicFlow.Api
```

In another terminal:

```bash
cd frontend/civicflow-web
npm install
npm start
```

## Post-deploy checks

- `GET /health` returns healthy.
- `GET /api/reference/agencies` returns seeded OFM agency data.
- Request dashboard loads.
- Sample import returns one accepted row and two rejected rows.

## Rollback

```bash
docker compose down -v
```

Then restore the previous git revision.
