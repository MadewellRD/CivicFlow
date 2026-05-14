using CivicFlow.Application.Abstractions;
using CivicFlow.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CivicFlow.Application.Platform;

/// <summary>
/// Runs the business-rule pipeline. Equivalent of the ServiceNow rule engine
/// for one entity event. Maintains the same firing semantics: a Before rule
/// can mutate the entity in-flight, an After rule observes the committed
/// state, an Async rule is fire-and-forget for non-critical side effects.
/// </summary>
public sealed class BusinessRuleEngine
{
    private readonly IEnumerable<IBusinessRule> _rules;
    private readonly IAuditWriter _auditWriter;
    private readonly ILogger<BusinessRuleEngine> _logger;

    public BusinessRuleEngine(
        IEnumerable<IBusinessRule> rules,
        IAuditWriter auditWriter,
        ILogger<BusinessRuleEngine> logger)
    {
        _rules = rules;
        _auditWriter = auditWriter;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<BusinessRuleOutcome>> RunPhaseAsync(
        BusinessRulePhase phase,
        BusinessRuleContext context,
        CancellationToken cancellationToken)
    {
        var matched = _rules
            .Where(rule => rule.Phase == phase && rule.Table == context.Table)
            .OrderBy(rule => rule.Order)
            .ToArray();

        var outcomes = new List<BusinessRuleOutcome>(matched.Length);

        foreach (var rule in matched)
        {
            if (!rule.AppliesTo(context))
            {
                continue;
            }
            try
            {
                var outcome = await rule.RunAsync(context, cancellationToken);
                outcomes.Add(outcome);

                if (outcome.Executed)
                {
                    var entityId = context.Request?.Id ?? context.Batch?.Id ?? Guid.Empty;
                    await _auditWriter.WriteAsync(
                        context.ActorUserId,
                        AuditActionType.BusinessRuleExecuted,
                        context.Table.ToString(),
                        entityId,
                        $"Business rule '{rule.Name}' [{phase}] executed: {outcome.Summary}",
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Business rule {RuleName} threw during {Phase}", rule.Name, phase);
                outcomes.Add(new BusinessRuleOutcome(rule.Name, false, $"Threw: {ex.Message}"));
            }
        }

        return outcomes;
    }
}
