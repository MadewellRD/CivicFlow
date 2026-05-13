using CivicFlow.Application.Abstractions;
using CivicFlow.Domain.Entities;
using CivicFlow.Infrastructure.Persistence;

namespace CivicFlow.Infrastructure.Services;

public sealed class EfNotificationService : INotificationService
{
    private readonly CivicFlowDbContext _dbContext;

    public EfNotificationService(CivicFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnqueueAsync(Guid recipientUserId, string subject, string body, CancellationToken cancellationToken)
    {
        await _dbContext.NotificationMessages.AddAsync(new NotificationMessage(recipientUserId, subject, body), cancellationToken);
    }
}
