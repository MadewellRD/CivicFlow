using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Dtos;
using CivicFlow.Application.Services;
using CivicFlow.Domain.Common;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using Xunit;

namespace CivicFlow.Tests;

public sealed class WorkflowTests
{
    [Fact]
    public void DraftRequestCanMoveToSubmittedButNotApproved()
    {
        Assert.True(RequestWorkflow.CanTransition(RequestStatus.Draft, RequestStatus.Submitted));
        Assert.False(RequestWorkflow.CanTransition(RequestStatus.Draft, RequestStatus.Approved));
    }

    [Fact]
    public async Task SubmitRequestWritesAuditAndNotification()
    {
        var actorId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
        var agencyId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
        var repository = new FakeRequestRepository();
        var audit = new FakeAuditWriter();
        var notifications = new FakeNotificationService();
        var service = new RequestWorkflowService(repository, audit, notifications, new FixedClock());

        var created = await service.CreateAsync(new CreateRequestDto(
            "Budget data correction",
            RequestCategory.FinanceDataCorrection,
            agencyId,
            actorId,
            null,
            null,
            1000m,
            "Correct a fund code loaded from a legacy agency file."), CancellationToken.None);

        var submitted = await service.SubmitAsync(created.Id, actorId, CancellationToken.None);

        Assert.Equal(RequestStatus.Submitted, submitted.Status);
        Assert.Contains(audit.Summaries, summary => summary.Contains("status changed", StringComparison.OrdinalIgnoreCase));
        Assert.Single(notifications.Messages);
    }

    [Fact]
    public async Task InvalidTransitionThrowsDomainException()
    {
        var actorId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
        var agencyId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
        var repository = new FakeRequestRepository();
        var service = new RequestWorkflowService(repository, new FakeAuditWriter(), new FakeNotificationService(), new FixedClock());
        var created = await service.CreateAsync(new CreateRequestDto(
            "Budget request",
            RequestCategory.BudgetChange,
            agencyId,
            actorId,
            null,
            null,
            100m,
            "Business justification"), CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(() => service.ApproveAsync(created.Id, actorId, CancellationToken.None));
    }

    private sealed class FakeRequestRepository : IRequestRepository
    {
        private readonly Dictionary<Guid, Request> _requests = [];
        public Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_requests.GetValueOrDefault(id));
        public Task<IReadOnlyCollection<Request>> ListAsync(CancellationToken cancellationToken) => Task.FromResult((IReadOnlyCollection<Request>)_requests.Values.ToArray());
        public Task AddAsync(Request request, CancellationToken cancellationToken) { _requests[request.Id] = request; return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeAuditWriter : IAuditWriter
    {
        public List<string> Summaries { get; } = [];
        public Task WriteAsync(Guid actorUserId, AuditActionType actionType, string entityName, Guid entityId, string summary, CancellationToken cancellationToken) { Summaries.Add(summary); return Task.CompletedTask; }
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public List<string> Messages { get; } = [];
        public Task EnqueueAsync(Guid recipientUserId, string subject, string body, CancellationToken cancellationToken) { Messages.Add($"{subject}: {body}"); return Task.CompletedTask; }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 5, 13, 12, 0, 0, TimeSpan.Zero);
    }
}
