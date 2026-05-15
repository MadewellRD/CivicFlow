using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Abstractions;

public interface IAuditQueryService
{
    Task<IReadOnlyDictionary<AuditActionType, int>> CountByActionSinceAsync(DateTimeOffset since, CancellationToken cancellationToken);
}
