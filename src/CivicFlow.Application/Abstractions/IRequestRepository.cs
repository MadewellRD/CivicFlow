using CivicFlow.Domain.Entities;

namespace CivicFlow.Application.Abstractions;

public interface IRequestRepository
{
    Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Request>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Request request, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
