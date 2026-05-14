# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore as a distinct layer for cache friendliness.
COPY CivicFlow.sln Directory.Build.props global.json ./
COPY src/CivicFlow.Domain/CivicFlow.Domain.csproj src/CivicFlow.Domain/
COPY src/CivicFlow.Application/CivicFlow.Application.csproj src/CivicFlow.Application/
COPY src/CivicFlow.Infrastructure/CivicFlow.Infrastructure.csproj src/CivicFlow.Infrastructure/
COPY src/CivicFlow.Api/CivicFlow.Api.csproj src/CivicFlow.Api/
COPY tests/CivicFlow.Tests/CivicFlow.Tests.csproj tests/CivicFlow.Tests/
RUN dotnet restore CivicFlow.sln

COPY . .
RUN dotnet publish src/CivicFlow.Api/CivicFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true
EXPOSE 8080

# Healthcheck uses curl; install it before dropping privileges.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && useradd --uid 10001 --create-home --shell /usr/sbin/nologin civicflow \
    && mkdir -p /app && chown -R civicflow:civicflow /app
USER civicflow

COPY --from=build --chown=civicflow:civicflow /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=5 \
    CMD curl --fail --silent --show-error http://localhost:8080/health > /dev/null || exit 1

ENTRYPOINT ["dotnet", "CivicFlow.Api.dll"]
