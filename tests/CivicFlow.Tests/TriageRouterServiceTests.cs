using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Dtos;
using CivicFlow.Application.Services;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using CivicFlow.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CivicFlow.Tests;

public sealed class TriageRouterServiceTests
{
    [Fact]
    public async Task RecommendsQueueAndRecordsAuditEntry()
    {
        var actorId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var request = new Request(
            "Reconcile legacy AFRS export",
            RequestCategory.LegacyIntegrationIssue,
            agencyId,
            requesterId,
            null,
            null,
            500m,
            "Stuck records in the legacy outbound queue need re-export.");
        request.SetRequestNumber("CF-DEMO-TRIAGE1");

        var audit = new RecordingAuditWriter();
        var service = new TriageRouterService(
            new SingleRequestRepository(request),
            new DeterministicMockAdapter(NullLogger<DeterministicMockAdapter>.Instance),
            new PromptSchemaRegistry(),
            audit);

        var rec = await service.RecommendAsync(request.Id, actorId, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(rec.RecommendedQueue));
        Assert.True(rec.ServedFromMock);
        Assert.Contains(audit.Summaries, s => s.Contains("Triage recommendation generated"));
    }

    [Fact]
    public async Task DefaultsToSafeQueueWhenKillSwitchEngaged()
    {
        var actorId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var request = new Request(
            "Million-dollar supplemental",
            RequestCategory.BudgetChange,
            agencyId,
            requesterId,
            null,
            null,
            5_000_000m,
            "Large supplemental for caseload growth.");
        request.SetRequestNumber("CF-DEMO-TRIAGE2");

        var audit = new RecordingAuditWriter();
        var mock = new DeterministicMockAdapter(NullLogger<DeterministicMockAdapter>.Instance);
        var killSwitchAdapter = new KillSwitchAdapter(
            mock,
            new StaticOptionsMonitor(new CivicFlow.Application.Ai.AiOptions { KillSwitchEngaged = true }),
            NullLogger<KillSwitchAdapter>.Instance);

        var service = new TriageRouterService(
            new SingleRequestRepository(request),
            killSwitchAdapter,
            new PromptSchemaRegistry(),
            audit);

        var rec = await service.RecommendAsync(request.Id, actorId, CancellationToken.None);

        Assert.True(rec.ServedFromKillSwitch);
        Assert.True(rec.HumanReviewRequired);
        Assert.Equal("low", rec.Confidence);
        Assert.Equal("Budget Operations", rec.RecommendedQueue);
    }

    private sealed class SingleRequestRepository : IRequestRepository
    {
        private readonly Request _request;
        public SingleRequestRepository(Request r) => _request = r;
        public Task AddAsync(Request request, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<Request?>(id == _request.Id ? _request : null);
        public Task<IReadOnlyCollection<Request>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Request>>(new[] { _request });
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class RecordingAuditWriter : IAuditWriter
    {
        public List<string> Summaries { get; } = new();
        public Task WriteAsync(Guid actorUserId, AuditActionType actionType, string entityName, Guid entityId, string summary, CancellationToken cancellationToken)
        {
            Summaries.Add(summary);
            return Task.CompletedTask;
        }
    }

    private sealed class StaticOptionsMonitor : Microsoft.Extensions.Options.IOptionsMonitor<CivicFlow.Application.Ai.AiOptions>
    {
        public StaticOptionsMonitor(CivicFlow.Application.Ai.AiOptions value) { CurrentValue = value; }
        public CivicFlow.Application.Ai.AiOptions CurrentValue { get; }
        public CivicFlow.Application.Ai.AiOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<CivicFlow.Application.Ai.AiOptions, string?> listener) => null;
    }
}
