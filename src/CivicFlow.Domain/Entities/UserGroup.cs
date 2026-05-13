using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class UserGroup : Entity
{
    private UserGroup()
    {
    }

    public UserGroup(Guid userId, Guid groupId)
    {
        UserId = userId;
        GroupId = groupId;
    }

    public Guid UserId { get; private set; }
    public AppUser? User { get; private set; }
    public Guid GroupId { get; private set; }
    public Group? Group { get; private set; }
}
