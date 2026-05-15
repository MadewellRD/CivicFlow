namespace CivicFlow.Application.Dtos;

public sealed record OverviewStatsDto(
    int TotalRequests,
    int OpenRequests,
    int ClosedRequests,
    decimal TotalEstimatedValueOpen,
    IReadOnlyDictionary<string, int> CountByStatus,
    IReadOnlyDictionary<string, int> CountByCategory,
    IReadOnlyCollection<RecentRequestDto> RecentRequests,
    int BusinessRuleExecutionsLast24h,
    int AiInvocationsLast24h);

public sealed record RecentRequestDto(
    Guid Id,
    string RequestNumber,
    string Title,
    string Status,
    string Category,
    decimal EstimatedAmount,
    DateTimeOffset CreatedAt);
