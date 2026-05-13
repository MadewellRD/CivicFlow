using CivicFlow.Domain.Common;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public sealed class ImportStagingRow : Entity
{
    private readonly List<ImportValidationError> _errors = [];

    private ImportStagingRow()
    {
    }

    public ImportStagingRow(
        Guid importBatchId,
        int rowNumber,
        string requestNumber,
        string agencyCode,
        string fundCode,
        string programCode,
        int fiscalYear,
        decimal amount,
        string title,
        string effectiveDateText)
    {
        ImportBatchId = importBatchId;
        RowNumber = rowNumber;
        RequestNumber = requestNumber.Trim();
        AgencyCode = agencyCode.Trim().ToUpperInvariant();
        FundCode = fundCode.Trim().ToUpperInvariant();
        ProgramCode = programCode.Trim().ToUpperInvariant();
        FiscalYear = fiscalYear;
        Amount = amount;
        Title = title.Trim();
        EffectiveDateText = effectiveDateText.Trim();
        RowStatus = ImportRowStatus.Pending;
    }

    public Guid ImportBatchId { get; private set; }
    public ImportBatch? ImportBatch { get; private set; }
    public int RowNumber { get; private set; }
    public string RequestNumber { get; private set; } = string.Empty;
    public string AgencyCode { get; private set; } = string.Empty;
    public string FundCode { get; private set; } = string.Empty;
    public string ProgramCode { get; private set; } = string.Empty;
    public int FiscalYear { get; private set; }
    public decimal Amount { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string EffectiveDateText { get; private set; } = string.Empty;
    public ImportRowStatus RowStatus { get; private set; }
    public IReadOnlyCollection<ImportValidationError> Errors => _errors.AsReadOnly();

    public void AddError(string fieldName, string message)
    {
        _errors.Add(new ImportValidationError(Id, fieldName, message));
        RowStatus = ImportRowStatus.Rejected;
    }

    public void MarkValid()
    {
        if (_errors.Count > 0) throw new DomainException("Cannot mark a row valid while it has validation errors.");
        RowStatus = ImportRowStatus.Valid;
    }

    public void MarkTransformed()
    {
        if (RowStatus != ImportRowStatus.Valid) throw new DomainException("Only valid rows can be transformed.");
        RowStatus = ImportRowStatus.Transformed;
    }
}
