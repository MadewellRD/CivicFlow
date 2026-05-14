namespace CivicFlow.Application.Dtos;

public sealed record ImportErrorExplanationDto(
    int RowNumber,
    string Summary,
    IReadOnlyCollection<FieldGuidanceDto> FieldGuidance,
    string AgencyMessage,
    string Confidence,
    string ProviderName,
    bool ServedFromMock,
    bool ServedFromKillSwitch,
    int InputTokens,
    int OutputTokens,
    decimal EstimatedCostUsd,
    int LatencyMs);

public sealed record FieldGuidanceDto(string Field, string Problem, string Fix);

public sealed record ImportErrorExplanationBatchDto(
    Guid BatchId,
    string FileName,
    IReadOnlyCollection<ImportErrorExplanationDto> Explanations,
    int RowsExplained,
    int RowsSkipped,
    decimal TotalEstimatedCostUsd);

internal sealed class ImportExplainerLlmPayload
{
    public int RowNumber { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<FieldGuidanceLlmPayload> FieldGuidance { get; set; } = new();
    public string AgencyMessage { get; set; } = string.Empty;
    public string Confidence { get; set; } = "medium";
}

internal sealed class FieldGuidanceLlmPayload
{
    public string Field { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string Fix { get; set; } = string.Empty;
}
