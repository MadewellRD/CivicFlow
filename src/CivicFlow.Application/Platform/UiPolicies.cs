namespace CivicFlow.Application.Platform;

/// <summary>
/// ServiceNow UI Policy analogue. Declarative client-side rules consumed by
/// the Angular form layer. The catalog exposes one policy per form so the
/// SPA can render the same conditional visibility / required / read-only
/// behaviour an OFM ServiceNow form would.
/// </summary>
public sealed record UiPolicy(
    string FormName,
    string Field,
    string Behavior,
    string WhenExpression);

public sealed class UiPolicyCatalog
{
    public IReadOnlyCollection<UiPolicy> Policies { get; } = new[]
    {
        new UiPolicy("new-request", "estimatedAmount", "mandatory", "category in (BudgetChange, HrFundingChange)"),
        new UiPolicy("new-request", "businessJustification", "mandatory", "always"),
        new UiPolicy("new-request", "fundId", "visible", "category != SecurityAccessChange"),
        new UiPolicy("new-request", "estimatedAmount", "readonly", "status in (Approved, Implemented, Closed)"),
        new UiPolicy("import-row-fix", "amount", "mandatory", "always"),
        new UiPolicy("import-row-fix", "amount", "warn", "amount > 5000000")
    };
}
