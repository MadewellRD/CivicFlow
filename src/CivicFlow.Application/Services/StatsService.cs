using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Dtos;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Services;

public sealed class StatsService
{
    private readonly IRequestRepository _requests;
    private readonly IAuditQueryService _auditQuery;
    private readonly IClock _clock;

    public StatsService(IRequestRepository requests, IAuditQueryService auditQuery, IClock clock)
    {
        _requests = requests;
        _auditQuery = auditQuery;
        _clock = clock;
    }

    public async Task<OverviewStatsDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var requests = await _requests.ListAsync(cancellationToken);
        var openStatuses = new[]
        {
            RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage,
            RequestStatus.AnalystReview, RequestStatus.TechnicalReview,
            RequestStatus.Approved, RequestStatus.Blocked, RequestStatus.ReturnedForCorrection,
            RequestStatus.Reopened
        };
        var openSet = openStatuses.ToHashSet();

        var countByStatus = requests
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());
        var countByCategory = requests
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var since = _clock.UtcNow.AddHours(-24);
        var auditCounts = await _auditQuery.CountByActionSinceAsync(since, cancellationToken);

        var open = requests.Where(r => openSet.Contains(r.Status)).ToArray();
        var recent = requests
            .OrderByDescending(r => r.CreatedAt)
            .Take(8)
            .Select(r => new RecentRequestDto(r.Id, r.RequestNumber, r.Title, r.Status.ToString(), r.Category.ToString(), r.EstimatedAmount, r.CreatedAt))
            .ToArray();

        return new OverviewStatsDto(
            TotalRequests: requests.Count,
            OpenRequests: open.Length,
            ClosedRequests: requests.Count(r => r.Status == RequestStatus.Closed),
            TotalEstimatedValueOpen: open.Sum(r => r.EstimatedAmount),
            CountByStatus: countByStatus,
            CountByCategory: countByCategory,
            RecentRequests: recent,
            BusinessRuleExecutionsLast24h: auditCounts.GetValueOrDefault(AuditActionType.BusinessRuleExecuted, 0),
            AiInvocationsLast24h: auditCounts.GetValueOrDefault(AuditActionType.AiTriageGenerated, 0)
                + auditCounts.GetValueOrDefault(AuditActionType.AiExplanationGenerated, 0));
    }
}
