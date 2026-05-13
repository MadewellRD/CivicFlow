using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class Agency : AuditableEntity
{
    private Agency()
    {
    }

    public Agency(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Agency code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Agency name is required.");
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        IsActive = true;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
}
