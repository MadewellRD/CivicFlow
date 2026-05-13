using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Abstractions;

public interface IAuditWriter
{
    Task WriteAsync(Guid actorUserId, AuditActionType actionType, string entityName, Guid entityId, string summary, CancellationToken cancellationToken);
}
