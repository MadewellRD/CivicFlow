using System.Diagnostics;
using System.Text.Json;
using CivicFlow.Application.Ai;
using Microsoft.Extensions.Logging;

namespace CivicFlow.Infrastructure.Ai;

/// <summary>
/// Returns canned JSON responses keyed by prompt template id. Used for:
/// - offline demo recording (zero network, zero cost);
/// - automated tests of services that consume IModelAdapter;
/// - production fallback when the upstream provider degrades.
///
/// The canned responses are written to satisfy the schema attached to each
/// prompt, so downstream validation does not need a branch for "mock mode".
/// </summary>
public sealed class DeterministicMockAdapter : IModelAdapter
{
    private static readonly Dictionary<string, string> CannedResponses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["import-error-explainer"] = """
        {
          "rowNumber": 0,
          "summary": "Row references an unknown agency code and an amount above the auto-import threshold.",
          "fieldGuidance": [
            { "field": "AgencyCode", "problem": "AgencyCode is not in the active reference list.", "fix": "Use one of: OFM, DSHS, DOH, DOL, DCYF." },
            { "field": "Amount", "problem": "Amount exceeds the $5,000,000 automatic-import threshold.", "fix": "Split into multiple rows, each under $5,000,000, or escalate to manual analyst review." }
          ],
          "agencyMessage": "Please use a valid agency code (OFM, DSHS, DOH, DOL, DCYF) and keep each line at or below $5,000,000 — anything larger needs analyst review.",
          "confidence": "high"
        }
        """,
        ["triage-router"] = """
        {
          "recommendedQueue": "Budget Operations",
          "complexity": "medium",
          "humanReviewRequired": false,
          "rationale": "Routine fund code reclassification within OFM with full justification provided.",
          "similarPastRequests": [
            { "requestNumber": "CF-DEMO-DEMO01", "title": "Q2 BARS code retirement cleanup", "similarityScore": 0.82 },
            { "requestNumber": "CF-DEMO-DEMO02", "title": "DSHS case management correction batch 0042", "similarityScore": 0.71 }
          ],
          "confidence": "high"
        }
        """
    };

    private readonly ILogger<DeterministicMockAdapter> _logger;

    public DeterministicMockAdapter(ILogger<DeterministicMockAdapter> logger)
    {
        _logger = logger;
    }

    public Task<ModelResponse<TResult>> InvokeAsync<TResult>(
        ModelRequest<TResult> request,
        CancellationToken cancellationToken) where TResult : class
    {
        var stopwatch = Stopwatch.StartNew();

        if (!CannedResponses.TryGetValue(request.PromptTemplateId, out var json))
        {
            stopwatch.Stop();
            var telemetry = BuildTelemetry(0, 0, stopwatch.Elapsed);
            return Task.FromResult(ModelResponse<TResult>.Failure(
                $"No mock response is registered for prompt template '{request.PromptTemplateId}'.",
                telemetry));
        }

        try
        {
            var value = JsonSerializer.Deserialize<TResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            stopwatch.Stop();
            var telemetry = BuildTelemetry(
                inputTokens: request.UserPrompt.Length / 4,
                outputTokens: json.Length / 4,
                latency: stopwatch.Elapsed);

            if (value is null)
            {
                return Task.FromResult(ModelResponse<TResult>.Failure(
                    "Mock JSON deserialised to null.",
                    telemetry));
            }

            return Task.FromResult(ModelResponse<TResult>.Success(value, telemetry));
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Mock JSON deserialisation failed for {PromptTemplateId}", request.PromptTemplateId);
            return Task.FromResult(ModelResponse<TResult>.Failure(
                $"Mock JSON deserialisation failed: {ex.Message}",
                BuildTelemetry(0, 0, stopwatch.Elapsed)));
        }
    }

    private static ModelInvocationTelemetry BuildTelemetry(int inputTokens, int outputTokens, TimeSpan latency) =>
        new(
            ProviderName: "mock",
            ModelName: "deterministic",
            InputTokens: inputTokens,
            OutputTokens: outputTokens,
            EstimatedCostUsd: 0m,
            Latency: latency,
            ServedFromKillSwitch: false,
            ServedFromMock: true);
}
