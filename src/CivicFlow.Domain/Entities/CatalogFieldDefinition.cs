using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class CatalogFieldDefinition : Entity
{
    private CatalogFieldDefinition()
    {
    }

    public CatalogFieldDefinition(Guid catalogItemId, string fieldKey, string label, string fieldType, bool isRequired)
    {
        CatalogItemId = catalogItemId;
        FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? throw new DomainException("Field key is required.") : fieldKey.Trim();
        Label = string.IsNullOrWhiteSpace(label) ? throw new DomainException("Field label is required.") : label.Trim();
        FieldType = string.IsNullOrWhiteSpace(fieldType) ? throw new DomainException("Field type is required.") : fieldType.Trim();
        IsRequired = isRequired;
    }

    public Guid CatalogItemId { get; private set; }
    public CatalogItem? CatalogItem { get; private set; }
    public string FieldKey { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string FieldType { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }
}
