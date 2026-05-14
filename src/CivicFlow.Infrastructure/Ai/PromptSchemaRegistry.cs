using CivicFlow.Application.Ai;

namespace CivicFlow.Infrastructure.Ai;

/// <summary>
/// Static schema registry. New AI features register their schemas here so
/// every prompt is paired with a contract before it ever reaches a model.
/// </summary>
public sealed class PromptSchemaRegistry : IPromptSchemaRegistry
{
    private static readonly Dictionary<string, string> Schemas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["import-error-explainer"] = """
        {
          "type": "object",
          "required": ["rowNumber", "summary", "fieldGuidance", "agencyMessage", "confidence"],
          "properties": {
            "rowNumber": { "type": "integer" },
            "summary": { "type": "string" },
            "fieldGuidance": {
              "type": "array",
              "items": {
                "type": "object",
                "required": ["field", "problem", "fix"],
                "properties": {
                  "field": { "type": "string" },
                  "problem": { "type": "string" },
                  "fix": { "type": "string" }
                }
              }
            },
            "agencyMessage": { "type": "string" },
            "confidence": { "type": "string", "enum": ["low", "medium", "high"] }
          }
        }
        """,
        ["triage-router"] = """
        {
          "type": "object",
          "required": ["recommendedQueue", "complexity", "humanReviewRequired", "rationale", "similarPastRequests", "confidence"],
          "properties": {
            "recommendedQueue": {
              "type": "string",
              "enum": ["Budget Operations", "HR Funding", "Application Development", "Data Integration", "Audit and Compliance"]
            },
            "complexity": { "type": "string", "enum": ["low", "medium", "high"] },
            "humanReviewRequired": { "type": "boolean" },
            "rationale": { "type": "string" },
            "similarPastRequests": {
              "type": "array",
              "items": {
                "type": "object",
                "required": ["requestNumber", "title", "similarityScore"],
                "properties": {
                  "requestNumber": { "type": "string" },
                  "title": { "type": "string" },
                  "similarityScore": { "type": "number" }
                }
              }
            },
            "confidence": { "type": "string", "enum": ["low", "medium", "high"] }
          }
        }
        """
    };

    public string GetSchema(string promptTemplateId)
    {
        return Schemas.TryGetValue(promptTemplateId, out var schema)
            ? schema
            : throw new InvalidOperationException($"No schema registered for prompt template '{promptTemplateId}'.");
    }
}
