using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class ImportValidationError : Entity
{
    private ImportValidationError()
    {
    }

    public ImportValidationError(Guid importStagingRowId, string fieldName, string message)
    {
        ImportStagingRowId = importStagingRowId;
        FieldName = string.IsNullOrWhiteSpace(fieldName) ? "Row" : fieldName.Trim();
        Message = string.IsNullOrWhiteSpace(message) ? throw new DomainException("Validation message is required.") : message.Trim();
    }

    public Guid ImportStagingRowId { get; private set; }
    public ImportStagingRow? ImportStagingRow { get; private set; }
    public string FieldName { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
}
