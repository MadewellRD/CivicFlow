using CivicFlow.Domain.Common;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public sealed class RequestStatusHistory : Entity
{
    private RequestStatusHistory()
    {
    }

    public RequestStatusHistory(Guid requestId, RequestStatus status, Guid actorUserId, string reason)
    {
        RequestId = requestId;
        Status = status;
        ActorUserId = actorUserId;
        Reason = string.IsNullOrWhiteSpace(reason) ? "No reason supplied." : reason.Trim();
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public Guid RequestId { get; private set; }
    public Request? Request { get; private set; }
    public RequestStatus Status { get; private set; }
    public Guid ActorUserId { get; private set; }
    public AppUser? ActorUser { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
}
