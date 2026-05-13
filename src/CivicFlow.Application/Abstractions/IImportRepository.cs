using CivicFlow.Domain.Entities;

namespace CivicFlow.Application.Abstractions;

public interface IImportRepository
{
    Task AddBatchAsync(ImportBatch batch, CancellationToken cancellationToken);
    Task<ImportBatch?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
