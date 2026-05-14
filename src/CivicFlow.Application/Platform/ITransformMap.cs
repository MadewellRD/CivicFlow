namespace CivicFlow.Application.Platform;

/// <summary>
/// ServiceNow Transform Map analogue. Each field map declares how a source
/// staging field becomes a target field on the destination entity, optionally
/// running a transform script.
///
/// Note that the production import flow uses
/// <c>CivicFlow.Application.Services.ImportValidationService.TransformBatchAsync</c>
/// for the persistence cutover. The TransformMap engine here is the
/// declarative shape an analyst can introspect to understand or extend the
/// mapping without changing C# code — exactly what an OFM ServiceNow admin
/// would do with the Transform Map studio.
/// </summary>
public interface ITransformMap
{
    string Name { get; }
    string SourceTable { get; }
    string TargetTable { get; }
    IReadOnlyCollection<FieldMap> FieldMaps { get; }
}

public sealed record FieldMap(string SourceField, string TargetField, string? TransformScript);

public sealed class BudgetImportTransformMap : ITransformMap
{
    public string Name => "Budget request import";
    public string SourceTable => "ImportStagingRow";
    public string TargetTable => "Request";
    public IReadOnlyCollection<FieldMap> FieldMaps { get; } = new[]
    {
        new FieldMap("RequestNumber", "RequestNumber", "trim, uppercase"),
        new FieldMap("AgencyCode", "AgencyId", "reference lookup against Agencies by Code"),
        new FieldMap("FundCode", "FundId", "reference lookup against Funds by Code"),
        new FieldMap("ProgramCode", "BudgetProgramId", "reference lookup against BudgetPrograms by Code and AgencyId"),
        new FieldMap("Title", "Title", "trim"),
        new FieldMap("Amount", "EstimatedAmount", "passthrough"),
        new FieldMap("FiscalYear", "FiscalYear", "passthrough; reject if outside [2024, 2035]"),
        new FieldMap("EffectiveDateText", "EffectiveDate", "DateOnly.Parse; reject if not parseable")
    };
}
