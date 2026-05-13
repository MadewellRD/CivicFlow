using CivicFlow.Domain.Common;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public sealed class AuditLog : Entity
{
    private AuditLog()
    {
    }

    public AuditLog(Guid actorUserId, AuditActionType actionType, string entityName, Guid entityId, string summary, string? beforeJson, string? afterJson)
    {
        ActorUserId = actorUserId;
        ActionType = actionType;
        EntityName = string.IsNullOrWhiteSpace(entityName) ? throw new DomainException("Entity name is required.") : entityName.Trim();
        EntityId = entityId;
        Summary = string.IsNullOrWhiteSpace(summary) ? throw new DomainException("Audit summary is required.") : summary.Trim();
        BeforeJson = beforeJson;
        AfterJson = afterJson;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public Guid ActorUserId { get; private set; }
    public AppUser? ActorUser { get; private set; }
    public AuditActionType ActionType { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
}
