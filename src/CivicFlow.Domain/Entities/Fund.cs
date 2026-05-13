using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class Fund : AuditableEntity
{
    private Fund()
    {
    }

    public Fund(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Fund code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Fund name is required.");
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        IsActive = true;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
}
