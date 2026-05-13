using CivicFlow.Domain.Common;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public sealed class ImportBatch : AuditableEntity
{
    private readonly List<ImportStagingRow> _rows = [];

    private ImportBatch()
    {
    }

    public ImportBatch(string fileName, Guid uploadedByUserId)
    {
        FileName = string.IsNullOrWhiteSpace(fileName) ? throw new DomainException("File name is required.") : fileName.Trim();
        UploadedByUserId = uploadedByUserId;
        CreatedByUserId = uploadedByUserId;
        Status = "Received";
    }

    public string FileName { get; private set; } = string.Empty;
    public Guid UploadedByUserId { get; private set; }
    public AppUser? UploadedByUser { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public int TotalRows => _rows.Count;
    public int AcceptedRows => _rows.Count(row => row.RowStatus is ImportRowStatus.Valid or ImportRowStatus.Transformed);
    public int RejectedRows => _rows.Count(row => row.RowStatus == ImportRowStatus.Rejected);
    public IReadOnlyCollection<ImportStagingRow> Rows => _rows.AsReadOnly();

    public void AddRow(ImportStagingRow row)
    {
        _rows.Add(row);
    }

    public void MarkValidated()
    {
        Status = "Validated";
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkTransformed()
    {
        Status = "Transformed";
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
