using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class BudgetProgram : AuditableEntity
{
    private BudgetProgram()
    {
    }

    public BudgetProgram(string code, string name, Guid agencyId)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Program code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Program name is required.");
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        AgencyId = agencyId;
        IsActive = true;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid AgencyId { get; private set; }
    public Agency? Agency { get; private set; }
    public bool IsActive { get; private set; }
}
