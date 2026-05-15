using CivicFlow.Application.Abstractions;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using CivicFlow.Infrastructure.Persistence;

namespace CivicFlow.Infrastructure.Services;

public sealed class EfAuditWriter : IAuditWriter
{
    private readonly CivicFlowDbContext _dbContext;

    public EfAuditWriter(CivicFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(Guid actorUserId, AuditActionType actionType, string entityName, Guid entityId, string summary, CancellationToken cancellationToken)
    {
        await _dbContext.AuditLogs.AddAsync(new AuditLog(actorUserId, actionType, entityName, entityId, summary, null, null), cancellationToken);
    }
}
