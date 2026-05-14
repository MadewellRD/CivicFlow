namespace CivicFlow.Application.Ai;

/// <summary>
/// Sole contract through which CivicFlow application services talk to any
/// large language model. The contract is intentionally narrow:
///
/// - inputs are always a typed prompt with a JSON schema for the response;
/// - outputs are always schema-validated before they reach a caller;
/// - every invocation must return cost and latency telemetry;
/// - every invocation can be short-circuited by a kill-switch with no caller change.
///
/// This is the same "schema-enforced output, governed execution boundary"
/// pattern used in PROMETHEUS and ROGUE: OPS, ported to .NET 8 minimal API.
/// </summary>
public interface IModelAdapter
{
    Task<ModelResponse<TResult>> InvokeAsync<TResult>(
        ModelRequest<TResult> request,
        CancellationToken cancellationToken) where TResult : class;
}

public sealed record ModelRequest<TResult>(
    string PromptTemplateId,
    string SystemPrompt,
    string UserPrompt,
    string ResponseJsonSchema,
    int MaxOutputTokens = 1024,
    double Temperature = 0.0) where TResult : class;

public sealed record ModelResponse<TResult>(
    TResult? Value,
    bool Succeeded,
    string? FailureReason,
    ModelInvocationTelemetry Telemetry) where TResult : class
{
    public static ModelResponse<TResult> Success(TResult value, ModelInvocationTelemetry telemetry) =>
        new(value, true, null, telemetry);
    public static ModelResponse<TResult> Failure(string reason, ModelInvocationTelemetry telemetry) =>
        new(null, false, reason, telemetry);
}

public sealed record ModelInvocationTelemetry(
    string ProviderName,
    string ModelName,
    int InputTokens,
    int OutputTokens,
    decimal EstimatedCostUsd,
    TimeSpan Latency,
    bool ServedFromKillSwitch,
    bool ServedFromMock);
