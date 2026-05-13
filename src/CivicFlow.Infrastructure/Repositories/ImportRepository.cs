using CivicFlow.Application.Abstractions;
using CivicFlow.Domain.Entities;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Repositories;

public sealed class ImportRepository : IImportRepository
{
    private readonly CivicFlowDbContext _dbContext;

    public ImportRepository(CivicFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddBatchAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        await _dbContext.ImportBatches.AddAsync(batch, cancellationToken);
    }

    public async Task<ImportBatch?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken)
    {
        return await _dbContext.ImportBatches
            .Include(batch => batch.Rows)
            .ThenInclude(row => row.Errors)
            .FirstOrDefaultAsync(batch => batch.Id == batchId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
