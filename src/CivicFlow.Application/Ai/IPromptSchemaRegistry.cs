namespace CivicFlow.Application.Ai;

/// <summary>
/// Registry of JSON schemas for each prompt template. Application services
/// look up the schema by template id instead of hand-coding it, so a single
/// schema change does not require touching every call site. This is the
/// CivicFlow port of the schema-as-contract pattern from PROMETHEUS.
/// </summary>
public interface IPromptSchemaRegistry
{
    /// <summary>Returns the JSON schema (as a JSON string) for the named template.</summary>
    string GetSchema(string promptTemplateId);
}
