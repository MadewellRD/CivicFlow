using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class Group : AuditableEntity
{
    private readonly List<UserGroup> _members = [];

    private Group()
    {
    }

    public Group(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Group name is required.");
        Name = name.Trim();
        Description = description.Trim();
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IReadOnlyCollection<UserGroup> Members => _members.AsReadOnly();
}
