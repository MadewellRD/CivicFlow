namespace CivicFlow.Application.Dtos;

public sealed record TriageRecommendationDto(
    Guid RequestId,
    string RecommendedQueue,
    string Complexity,
    bool HumanReviewRequired,
    string Rationale,
    IReadOnlyCollection<SimilarPastRequestDto> SimilarPastRequests,
    string Confidence,
    string ProviderName,
    bool ServedFromMock,
    bool ServedFromKillSwitch,
    int InputTokens,
    int OutputTokens,
    decimal EstimatedCostUsd,
    int LatencyMs);

public sealed record SimilarPastRequestDto(string RequestNumber, string Title, double SimilarityScore);

internal sealed class TriageRouterLlmPayload
{
    public string RecommendedQueue { get; set; } = string.Empty;
    public string Complexity { get; set; } = "medium";
    public bool HumanReviewRequired { get; set; }
    public string Rationale { get; set; } = string.Empty;
    public List<SimilarPastRequestLlmPayload> SimilarPastRequests { get; set; } = new();
    public string Confidence { get; set; } = "medium";
}

internal sealed class SimilarPastRequestLlmPayload
{
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
}
