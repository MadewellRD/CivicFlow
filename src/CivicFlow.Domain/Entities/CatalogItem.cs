using CivicFlow.Domain.Common;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public sealed class CatalogItem : AuditableEntity
{
    private readonly List<CatalogFieldDefinition> _fields = [];

    private CatalogItem()
    {
    }

    public CatalogItem(string name, RequestCategory category, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Catalog item name is required.");
        Name = name.Trim();
        Category = category;
        Description = description.Trim();
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;
    public RequestCategory Category { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<CatalogFieldDefinition> Fields => _fields.AsReadOnly();
}
