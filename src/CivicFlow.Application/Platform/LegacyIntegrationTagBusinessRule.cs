using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Platform;

/// <summary>
/// ServiceNow Business Rule analogue. Whenever a Legacy Integration Issue
/// request is inserted, emit an audit summary that the developer team should
/// be notified. Phase=Async, fires after persistence; in real ServiceNow this
/// would post to the developer SNS or email subscription.
/// </summary>
public sealed class LegacyIntegrationTagBusinessRule : IBusinessRule
{
    public string Name => "Legacy integration triage tag";
    public BusinessRuleTable Table => BusinessRuleTable.Request;
    public BusinessRulePhase Phase => BusinessRulePhase.Async;
    public int Order => 200;

    public bool AppliesTo(BusinessRuleContext context)
    {
        return context.Trigger == BusinessRuleTrigger.Inserted
            && context.Request is not null
            && context.Request.Category == RequestCategory.LegacyIntegrationIssue;
    }

    public Task<BusinessRuleOutcome> RunAsync(BusinessRuleContext context, CancellationToken cancellationToken)
    {
        var summary = $"Tagged {context.Request!.RequestNumber} for the legacy integration developer queue.";
        return Task.FromResult(new BusinessRuleOutcome(Name, true, summary));
    }
}
