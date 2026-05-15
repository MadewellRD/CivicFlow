using CivicFlow.Application.Abstractions;
using CivicFlow.Domain.Enums;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Services;

public sealed class EfAuditQueryService : IAuditQueryService
{
    private readonly CivicFlowDbContext _db;

    public EfAuditQueryService(CivicFlowDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyDictionary<AuditActionType, int>> CountByActionSinceAsync(DateTimeOffset since, CancellationToken cancellationToken)
    {
        var rows = await _db.AuditLogs
            .Where(l => l.OccurredAt >= since)
            .GroupBy(l => l.ActionType)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToArrayAsync(cancellationToken);
        return rows.ToDictionary(r => r.Key, r => r.Count);
    }
}
