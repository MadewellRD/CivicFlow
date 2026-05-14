namespace CivicFlow.Application.Platform;

/// <summary>
/// ServiceNow Business Rule analogue. Marks any submitted Request whose
/// estimated amount exceeds a configured threshold as requiring an analyst
/// flag. Phase=After, runs on every status change into Submitted.
/// </summary>
public sealed class OversightThresholdBusinessRule : IBusinessRule
{
    private const decimal Threshold = 1_000_000m;
    public string Name => "Oversight threshold flag";
    public BusinessRuleTable Table => BusinessRuleTable.Request;
    public BusinessRulePhase Phase => BusinessRulePhase.After;
    public int Order => 100;

    public bool AppliesTo(BusinessRuleContext context)
    {
        return context.Trigger == BusinessRuleTrigger.StatusChanged
            && context.Request is not null
            && context.Request.EstimatedAmount >= Threshold;
    }

    public Task<BusinessRuleOutcome> RunAsync(BusinessRuleContext context, CancellationToken cancellationToken)
    {
        var amount = context.Request!.EstimatedAmount;
        var summary = $"Request {context.Request.RequestNumber} flagged for oversight (estimated ${amount:N0} >= ${Threshold:N0}).";
        return Task.FromResult(new BusinessRuleOutcome(Name, true, summary));
    }
}
