using System.Text;
using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Ai;
using CivicFlow.Application.Dtos;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Services;

/// <summary>
/// AI-assisted triage router. On request submission, looks at the request's
/// title, category, and business justification, surfaces a small set of
/// similar historical requests as grounding context, and asks the model to
/// recommend a queue, complexity, and whether human review is mandatory.
///
/// Modelled after the GPT-3 incident-triage pattern from Providence — same
/// shape (classify, route, ground in past tickets, flag low confidence for
/// human review), ported to .NET 8 with schema-enforced output.
/// </summary>
public sealed class TriageRouterService
{
    private const string PromptTemplateId = "triage-router";
    private const int GroundingSamplesPerCategory = 4;

    private readonly IRequestRepository _requests;
    private readonly IModelAdapter _model;
    private readonly IPromptSchemaRegistry _schemaRegistry;
    private readonly IAuditWriter _auditWriter;

    public TriageRouterService(
        IRequestRepository requests,
        IModelAdapter model,
        IPromptSchemaRegistry schemaRegistry,
        IAuditWriter auditWriter)
    {
        _requests = requests;
        _model = model;
        _schemaRegistry = schemaRegistry;
        _auditWriter = auditWriter;
    }

    public async Task<TriageRecommendationDto> RecommendAsync(
        Guid requestId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var request = await _requests.GetByIdAsync(requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Request {requestId} was not found.");

        var grounding = await BuildGroundingAsync(request, cancellationToken);
        var schema = _schemaRegistry.GetSchema(PromptTemplateId);

        var modelRequest = new ModelRequest<TriageRouterLlmPayload>(
            PromptTemplateId: PromptTemplateId,
            SystemPrompt: BuildSystemPrompt(),
            UserPrompt: BuildUserPrompt(request, grounding),
            ResponseJsonSchema: schema,
            MaxOutputTokens: 768,
            Temperature: 0.1);

        var response = await _model.InvokeAsync(modelRequest, cancellationToken);

        if (!response.Succeeded || response.Value is null)
        {
            var fallback = BuildFailureRecommendation(request, response);
            await _auditWriter.WriteAsync(
                actorUserId,
                AuditActionType.AiTriageGenerated,
                nameof(Request),
                request.Id,
                $"Triage fallback generated: queue='{fallback.RecommendedQueue}', kill_switch={fallback.ServedFromKillSwitch}, confidence='{fallback.Confidence}', reason='{response.FailureReason}'.",
                cancellationToken);
            await _requests.SaveChangesAsync(cancellationToken);

            return fallback;
        }

        await _auditWriter.WriteAsync(
            actorUserId,
            AuditActionType.AiTriageGenerated,
            nameof(Request),
            request.Id,
            $"Triage recommendation generated: queue='{response.Value.RecommendedQueue}', complexity='{response.Value.Complexity}', human_review={response.Value.HumanReviewRequired}, confidence='{response.Value.Confidence}'.",
            cancellationToken);
        await _requests.SaveChangesAsync(cancellationToken);

        return new TriageRecommendationDto(
            RequestId: request.Id,
            RecommendedQueue: response.Value.RecommendedQueue,
            Complexity: response.Value.Complexity,
            HumanReviewRequired: response.Value.HumanReviewRequired,
            Rationale: response.Value.Rationale,
            SimilarPastRequests: response.Value.SimilarPastRequests
                .Select(s => new SimilarPastRequestDto(s.RequestNumber, s.Title, s.SimilarityScore))
                .ToArray(),
            Confidence: response.Value.Confidence,
            ProviderName: response.Telemetry.ProviderName,
            ServedFromMock: response.Telemetry.ServedFromMock,
            ServedFromKillSwitch: response.Telemetry.ServedFromKillSwitch,
            InputTokens: response.Telemetry.InputTokens,
            OutputTokens: response.Telemetry.OutputTokens,
            EstimatedCostUsd: response.Telemetry.EstimatedCostUsd,
            LatencyMs: (int)response.Telemetry.Latency.TotalMilliseconds);
    }

    private async Task<IReadOnlyCollection<Request>> BuildGroundingAsync(Request request, CancellationToken cancellationToken)
    {
        var all = await _requests.ListAsync(cancellationToken);
        // Cheap retrieval-style filter: same category, not the same request, prefer the most recently updated.
        return all
            .Where(r => r.Id != request.Id && r.Category == request.Category)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .Take(GroundingSamplesPerCategory)
            .ToArray();
    }

    private static string BuildSystemPrompt()
    {
        return """
        You are CivicFlow's request triage router. You suggest which queue should handle a new
        Washington State OFM budget request, how complex it is, and whether a human reviewer must
        confirm the assignment before work begins.

        Valid recommendedQueue values (pick one):
        - "Budget Operations"
        - "HR Funding"
        - "Application Development"
        - "Data Integration"
        - "Audit and Compliance"

        complexity is one of: "low", "medium", "high".
        humanReviewRequired must be true when ANY of these are true:
        - estimated amount > $1,000,000
        - the request mentions PHI, PII, security, or audit findings
        - confidence in your own recommendation is below "high"

        Cite up to three similar past requests from the grounding context. Score similarity
        between 0.0 and 1.0. Do not invent request numbers — if none of the grounding samples
        are relevant, return an empty list.

        Output JSON only, matching the provided schema exactly. No prose, no markdown.
        """;
    }

    private static string BuildUserPrompt(Request request, IReadOnlyCollection<Request> grounding)
    {
        var sb = new StringBuilder();
        sb.AppendLine("New request to triage:");
        sb.AppendLine($"  requestNumber: {request.RequestNumber}");
        sb.AppendLine($"  title: {request.Title}");
        sb.AppendLine($"  category: {request.Category}");
        sb.AppendLine($"  estimatedAmount: {request.EstimatedAmount}");
        sb.AppendLine($"  businessJustification: {request.BusinessJustification}");
        sb.AppendLine();
        sb.AppendLine("Grounding context (similar past requests in same category):");
        if (grounding.Count == 0)
        {
            sb.AppendLine("  (no prior requests in this category)");
        }
        else
        {
            foreach (var r in grounding)
            {
                sb.AppendLine($"  - requestNumber={r.RequestNumber}, title={r.Title}, status={r.Status}, amount={r.EstimatedAmount}");
            }
        }
        sb.AppendLine();
        sb.AppendLine("Produce the triage recommendation JSON now.");
        return sb.ToString();
    }

    private static TriageRecommendationDto BuildFailureRecommendation(Request request, ModelResponse<TriageRouterLlmPayload> response)
    {
        return new TriageRecommendationDto(
            RequestId: request.Id,
            RecommendedQueue: "Budget Operations",
            Complexity: "medium",
            HumanReviewRequired: true,
            Rationale: response.Telemetry.ServedFromKillSwitch
                ? "AI triage is currently disabled (kill-switch engaged). Defaulting to safe queue with required human review."
                : $"AI triage unavailable: {response.FailureReason}. Defaulting to safe queue with required human review.",
            SimilarPastRequests: Array.Empty<SimilarPastRequestDto>(),
            Confidence: "low",
            ProviderName: response.Telemetry.ProviderName,
            ServedFromMock: response.Telemetry.ServedFromMock,
            ServedFromKillSwitch: response.Telemetry.ServedFromKillSwitch,
            InputTokens: response.Telemetry.InputTokens,
            OutputTokens: response.Telemetry.OutputTokens,
            EstimatedCostUsd: response.Telemetry.EstimatedCostUsd,
            LatencyMs: (int)response.Telemetry.Latency.TotalMilliseconds);
    }
}
