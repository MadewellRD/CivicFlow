using CivicFlow.Domain.Entities;

namespace CivicFlow.Application.Platform;

/// <summary>
/// Server-side rule that fires on a CivicFlow entity event.
///
/// Modelled directly after ServiceNow's Business Rule pattern:
/// - Table (entity) the rule applies to.
/// - When the rule runs: Before, After, or Async relative to the persistence event.
/// - Order: deterministic execution order across the rule set.
/// - Condition: cheap predicate filter so the engine can skip rules early.
/// - Action: the work the rule does, returning a structured outcome record.
///
/// This is intentionally the same vocabulary a ServiceNow developer uses, so
/// porting a rule to or from a real ServiceNow instance is a syntax change,
/// not a redesign.
/// </summary>
public interface IBusinessRule
{
    string Name { get; }
    BusinessRuleTable Table { get; }
    BusinessRulePhase Phase { get; }
    int Order { get; }
    bool AppliesTo(BusinessRuleContext context);
    Task<BusinessRuleOutcome> RunAsync(BusinessRuleContext context, CancellationToken cancellationToken);
}

public enum BusinessRuleTable
{
    Request = 0,
    ImportBatch = 1
}

public enum BusinessRulePhase
{
    Before = 0,
    After = 1,
    Async = 2
}

public enum BusinessRuleTrigger
{
    Inserted = 0,
    Updated = 1,
    StatusChanged = 2,
    Validated = 3
}

public sealed record BusinessRuleContext(
    BusinessRuleTable Table,
    BusinessRuleTrigger Trigger,
    Guid ActorUserId,
    Request? Request,
    ImportBatch? Batch);

public sealed record BusinessRuleOutcome(string RuleName, bool Executed, string Summary);
