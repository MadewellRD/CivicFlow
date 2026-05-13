using CivicFlow.Application.Abstractions;
using CivicFlow.Domain.Entities;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Repositories;

public sealed class RequestRepository : IRequestRepository
{
    private readonly CivicFlowDbContext _dbContext;

    public RequestRepository(CivicFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Requests
            .Include(request => request.StatusHistory)
            .Include(request => request.Comments)
            .FirstOrDefaultAsync(request => request.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Request>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Requests
            .OrderByDescending(request => request.CreatedAt)
            .Take(100)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(Request request, CancellationToken cancellationToken)
    {
        await _dbContext.Requests.AddAsync(request, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
