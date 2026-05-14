using System.Text;
using System.Text.Json;
using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Ai;
using CivicFlow.Application.Dtos;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Services;

/// <summary>
/// AI-assisted import error explainer. For each invalid staging row, asks the
/// configured model adapter to translate raw validator errors into a plain-
/// English, agency-facing explanation with concrete next steps.
///
/// The service is intentionally small: it owns prompt assembly, schema
/// invocation, audit logging, and DTO shaping. It does NOT own model
/// transport, retry, or governance — those live in the adapter pipeline.
/// </summary>
public sealed class ImportErrorExplainerService
{
    private const string PromptTemplateId = "import-error-explainer";

    private readonly IImportRepository _imports;
    private readonly IModelAdapter _model;
    private readonly IPromptSchemaRegistry _schemaRegistry;
    private readonly IAuditWriter _auditWriter;

    public ImportErrorExplainerService(
        IImportRepository imports,
        IModelAdapter model,
        IPromptSchemaRegistry schemaRegistry,
        IAuditWriter auditWriter)
    {
        _imports = imports;
        _model = model;
        _schemaRegistry = schemaRegistry;
        _auditWriter = auditWriter;
    }

    public async Task<ImportErrorExplanationBatchDto> ExplainAsync(
        Guid batchId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var batch = await _imports.GetBatchAsync(batchId, cancellationToken)
            ?? throw new InvalidOperationException($"Import batch {batchId} was not found.");

        var schema = _schemaRegistry.GetSchema(PromptTemplateId);
        var explanations = new List<ImportErrorExplanationDto>();
        var skipped = 0;
        decimal totalCost = 0m;

        foreach (var row in batch.Rows.OrderBy(r => r.RowNumber))
        {
            if (row.RowStatus != ImportRowStatus.Rejected || row.Errors.Count == 0)
            {
                skipped++;
                continue;
            }

            var request = new ModelRequest<ImportExplainerLlmPayload>(
                PromptTemplateId: PromptTemplateId,
                SystemPrompt: BuildSystemPrompt(),
                UserPrompt: BuildUserPrompt(row),
                ResponseJsonSchema: schema,
                MaxOutputTokens: 768,
                Temperature: 0.1);

            var response = await _model.InvokeAsync(request, cancellationToken);
            totalCost += response.Telemetry.EstimatedCostUsd;

            if (!response.Succeeded || response.Value is null)
            {
                explanations.Add(BuildFailureExplanation(row, response));
                continue;
            }

            explanations.Add(new ImportErrorExplanationDto(
                RowNumber: row.RowNumber,
                Summary: response.Value.Summary,
                FieldGuidance: response.Value.FieldGuidance
                    .Select(fg => new FieldGuidanceDto(fg.Field, fg.Problem, fg.Fix))
                    .ToArray(),
                AgencyMessage: response.Value.AgencyMessage,
                Confidence: response.Value.Confidence,
                ProviderName: response.Telemetry.ProviderName,
                ServedFromMock: response.Telemetry.ServedFromMock,
                ServedFromKillSwitch: response.Telemetry.ServedFromKillSwitch,
                InputTokens: response.Telemetry.InputTokens,
                OutputTokens: response.Telemetry.OutputTokens,
                EstimatedCostUsd: response.Telemetry.EstimatedCostUsd,
                LatencyMs: (int)response.Telemetry.Latency.TotalMilliseconds));
        }

        await _auditWriter.WriteAsync(
            actorUserId,
            AuditActionType.AiExplanationGenerated,
            nameof(ImportBatch),
            batch.Id,
            $"Generated {explanations.Count} AI explanations for batch {batch.FileName}. Estimated cost ${totalCost:F4}.",
            cancellationToken);

        return new ImportErrorExplanationBatchDto(
            BatchId: batch.Id,
            FileName: batch.FileName,
            Explanations: explanations,
            RowsExplained: explanations.Count,
            RowsSkipped: skipped,
            TotalEstimatedCostUsd: Math.Round(totalCost, 6));
    }

    private static string BuildSystemPrompt()
    {
        return """
        You are CivicFlow's import error explainer. You translate raw validator output
        into clear, agency-friendly guidance for Washington State budget analysts.

        Constraints:
        - Be specific. Reference field names and the literal allowed values when possible.
        - Be brief. Each "fix" should be one sentence an analyst can act on without a meeting.
        - Never invent reference codes. If you do not know what a valid value looks like,
          say "consult the active reference list" instead of guessing.
        - Confidence is "high" only if you have direct evidence in the row data and validator
          errors. Otherwise "medium" or "low".
        - Output JSON only, matching the provided schema exactly. No prose, no markdown.
        """;
    }

    private static string BuildUserPrompt(ImportStagingRow row)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Failed import row context:");
        sb.AppendLine($"  rowNumber: {row.RowNumber}");
        sb.AppendLine($"  requestNumber: {row.RequestNumber}");
        sb.AppendLine($"  agencyCode: {row.AgencyCode}");
        sb.AppendLine($"  fundCode: {row.FundCode}");
        sb.AppendLine($"  programCode: {row.ProgramCode}");
        sb.AppendLine($"  fiscalYear: {row.FiscalYear}");
        sb.AppendLine($"  amount: {row.Amount}");
        sb.AppendLine($"  title: {row.Title}");
        sb.AppendLine($"  effectiveDateText: {row.EffectiveDateText}");
        sb.AppendLine();
        sb.AppendLine("Validator errors:");
        foreach (var error in row.Errors)
        {
            sb.AppendLine($"  - {error.FieldName}: {error.Message}");
        }
        sb.AppendLine();
        sb.AppendLine("Produce the explanation JSON object now.");
        return sb.ToString();
    }

    private static ImportErrorExplanationDto BuildFailureExplanation(ImportStagingRow row, ModelResponse<ImportExplainerLlmPayload> response)
    {
        return new ImportErrorExplanationDto(
            RowNumber: row.RowNumber,
            Summary: response.Telemetry.ServedFromKillSwitch
                ? "AI explanations are currently disabled (kill-switch engaged). Showing raw validator errors."
                : $"AI explanation unavailable: {response.FailureReason}",
            FieldGuidance: row.Errors
                .Select(e => new FieldGuidanceDto(e.FieldName, e.Message, "Fix the value and resubmit the row."))
                .ToArray(),
            AgencyMessage: "Please review the field-by-field errors and resubmit the row.",
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
