using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class NotificationMessage : Entity
{
    private NotificationMessage()
    {
    }

    public NotificationMessage(Guid recipientUserId, string subject, string body)
    {
        RecipientUserId = recipientUserId;
        Subject = string.IsNullOrWhiteSpace(subject) ? throw new DomainException("Notification subject is required.") : subject.Trim();
        Body = string.IsNullOrWhiteSpace(body) ? throw new DomainException("Notification body is required.") : body.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid RecipientUserId { get; private set; }
    public AppUser? RecipientUser { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsSent { get; private set; }

    public void MarkSent()
    {
        IsSent = true;
    }
}
