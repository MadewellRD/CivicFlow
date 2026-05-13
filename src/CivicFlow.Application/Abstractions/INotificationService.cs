namespace CivicFlow.Application.Abstractions;

public interface INotificationService
{
    Task EnqueueAsync(Guid recipientUserId, string subject, string body, CancellationToken cancellationToken);
}
