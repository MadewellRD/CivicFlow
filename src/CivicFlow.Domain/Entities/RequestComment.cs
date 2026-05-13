using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class RequestComment : Entity
{
    private RequestComment()
    {
    }

    public RequestComment(Guid requestId, Guid authorUserId, string body, DateTimeOffset createdAt)
    {
        RequestId = requestId;
        AuthorUserId = authorUserId;
        Body = string.IsNullOrWhiteSpace(body) ? throw new DomainException("Comment body is required.") : body.Trim();
        CreatedAt = createdAt;
    }

    public Guid RequestId { get; private set; }
    public Request? Request { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public AppUser? AuthorUser { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
