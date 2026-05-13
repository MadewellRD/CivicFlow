using CivicFlow.Domain.Common;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public sealed class AppUser : AuditableEntity
{
    private readonly List<UserGroup> _groups = [];

    private AppUser()
    {
    }

    public AppUser(string displayName, string email, UserRole primaryRole)
    {
        if (string.IsNullOrWhiteSpace(displayName)) throw new DomainException("Display name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required.");
        DisplayName = displayName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PrimaryRole = primaryRole;
    }

    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public UserRole PrimaryRole { get; private set; }
    public IReadOnlyCollection<UserGroup> Groups => _groups.AsReadOnly();
}
