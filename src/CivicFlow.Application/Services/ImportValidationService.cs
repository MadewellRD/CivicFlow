using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Dtos;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Services;

public sealed class ImportValidationService
{
    private readonly IImportRepository _imports;
    private readonly IRequestRepository _requests;
    private readonly IReferenceDataProvider _referenceDataProvider;
    private readonly IAuditWriter _auditWriter;
    private readonly IClock _clock;

    public ImportValidationService(
        IImportRepository imports,
        IRequestRepository requests,
        IReferenceDataProvider referenceDataProvider,
        IAuditWriter auditWriter,
        IClock clock)
    {
        _imports = imports;
        _requests = requests;
        _referenceDataProvider = referenceDataProvider;
        _auditWriter = auditWriter;
        _clock = clock;
    }

    public async Task<ImportBatchSummaryDto> CreateAndValidateBatchAsync(
        string fileName,
        Guid uploadedByUserId,
        IReadOnlyCollection<ImportRowDto> rows,
        CancellationToken cancellationToken)
    {
        var batch = new ImportBatch(fileName, uploadedByUserId);
        foreach (var row in rows)
        {
            batch.AddRow(new ImportStagingRow(
                batch.Id,
                row.RowNumber,
                row.RequestNumber,
                row.AgencyCode,
                row.FundCode,
                row.ProgramCode,
                row.FiscalYear,
                row.Amount,
                row.Title,
                row.EffectiveDateText));
        }

        await _imports.AddBatchAsync(batch, cancellationToken);
        await ValidateBatchAsync(batch, cancellationToken);
        await _imports.SaveChangesAsync(cancellationToken);
        return ImportBatchSummaryDto.FromEntity(batch);
    }

    public async Task<ImportBatchSummaryDto> ValidateExistingBatchAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await LoadBatchAsync(batchId, cancellationToken);
        await ValidateBatchAsync(batch, cancellationToken);
        await _imports.SaveChangesAsync(cancellationToken);
        return ImportBatchSummaryDto.FromEntity(batch);
    }

    public async Task<ImportBatchSummaryDto> GetBatchSummaryAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await LoadBatchAsync(batchId, cancellationToken);
        return ImportBatchSummaryDto.FromEntity(batch);
    }

    public async Task<ImportBatchSummaryDto> TransformBatchAsync(Guid batchId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await LoadBatchAsync(batchId, cancellationToken);
        var referenceData = await _referenceDataProvider.GetTransformSnapshotAsync(cancellationToken);
        var requestNumbers = referenceData.ExistingRequestNumbers.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var transformedRows = 0;

        foreach (var row in batch.Rows.Where(row => row.RowStatus == ImportRowStatus.Valid).OrderBy(row => row.RowNumber))
        {
            if (!TryResolveReferences(row, referenceData, out var agencyId, out var fundId, out var budgetProgramId))
            {
                continue;
            }

            if (!requestNumbers.Add(row.RequestNumber))
            {
                row.AddError("RequestNumber", "Request number already exists in CivicFlow.");
                continue;
            }

            var request = new Request(
                row.Title,
                RequestCategory.FinanceDataCorrection,
                agencyId,
                actorUserId,
                fundId,
                budgetProgramId,
                row.Amount,
                $"Imported from batch {batch.Id}, row {row.RowNumber}.");

            request.SetRequestNumber(row.RequestNumber);
            request.TransitionTo(RequestStatus.Submitted, actorUserId, "Created from validated import row.", _clock.UtcNow);
            await _requests.AddAsync(request, cancellationToken);
            row.MarkTransformed();
            transformedRows++;
        }

        if (transformedRows > 0)
        {
            batch.MarkTransformed();
            await _auditWriter.WriteAsync(
                actorUserId,
                AuditActionType.ImportTransformed,
                nameof(ImportBatch),
                batch.Id,
                $"Import batch transformed {transformedRows} valid rows into requests.",
                cancellationToken);
        }

        await _imports.SaveChangesAsync(cancellationToken);
        return ImportBatchSummaryDto.FromEntity(batch);
    }

    private async Task<ImportBatch> LoadBatchAsync(Guid batchId, CancellationToken cancellationToken)
    {
        return await _imports.GetBatchAsync(batchId, cancellationToken)
            ?? throw new InvalidOperationException($"Import batch {batchId} was not found.");
    }

    private async Task ValidateBatchAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        var referenceData = await _referenceDataProvider.GetSnapshotAsync(cancellationToken);
        var duplicateNumbers = batch.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.RequestNumber))
            .GroupBy(row => row.RequestNumber.Trim().ToUpperInvariant())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var row in batch.Rows)
        {
            ValidateRow(row, referenceData, duplicateNumbers);
            if (!row.Errors.Any())
            {
                row.MarkValid();
            }
        }

        batch.MarkValidated();
        await _auditWriter.WriteAsync(batch.CreatedByUserId, AuditActionType.ImportValidated, nameof(ImportBatch), batch.Id, "Import batch validated.", cancellationToken);
    }

    private static bool TryResolveReferences(
        ImportStagingRow row,
        TransformReferenceDataSnapshot referenceData,
        out Guid agencyId,
        out Guid fundId,
        out Guid budgetProgramId)
    {
        agencyId = Guid.Empty;
        fundId = Guid.Empty;
        budgetProgramId = Guid.Empty;

        if (!referenceData.AgencyIdsByCode.TryGetValue(row.AgencyCode, out agencyId))
        {
            row.AddError("AgencyCode", "Agency code was not found or is inactive.");
            return false;
        }

        if (!referenceData.FundIdsByCode.TryGetValue(row.FundCode, out fundId))
        {
            row.AddError("FundCode", "Fund code was not found or is inactive.");
            return false;
        }

        var programKey = TransformReferenceDataSnapshot.ProgramKey(agencyId, row.ProgramCode);
        if (!referenceData.ProgramIdsByAgencyAndCode.TryGetValue(programKey, out budgetProgramId))
        {
            row.AddError("ProgramCode", "Budget program code was not found or is inactive for the agency.");
            return false;
        }

        return true;
    }

    private static void ValidateRow(ImportStagingRow row, ReferenceDataSnapshot referenceData, IReadOnlySet<string> duplicateNumbers)
    {
        if (string.IsNullOrWhiteSpace(row.RequestNumber)) row.AddError("RequestNumber", "Request number is required.");
        if (duplicateNumbers.Contains(row.RequestNumber)) row.AddError("RequestNumber", "Request number is duplicated within the import batch.");
        if (referenceData.ExistingRequestNumbers.Contains(row.RequestNumber)) row.AddError("RequestNumber", "Request number already exists in CivicFlow.");
        if (string.IsNullOrWhiteSpace(row.AgencyCode) || !referenceData.AgencyCodes.Contains(row.AgencyCode)) row.AddError("AgencyCode", "Agency code was not found or is inactive.");
        if (string.IsNullOrWhiteSpace(row.FundCode) || !referenceData.FundCodes.Contains(row.FundCode)) row.AddError("FundCode", "Fund code was not found or is inactive.");
        if (string.IsNullOrWhiteSpace(row.ProgramCode) || !referenceData.ProgramCodes.Contains(row.ProgramCode)) row.AddError("ProgramCode", "Budget program code was not found or is inactive.");
        if (row.FiscalYear < 2024 || row.FiscalYear > 2035) row.AddError("FiscalYear", "Fiscal year must be between 2024 and 2035.");
        if (row.Amount < 0) row.AddError("Amount", "Amount cannot be negative.");
        if (row.Amount > 5_000_000m) row.AddError("Amount", "Amount exceeds automatic import threshold and requires manual review.");
        if (string.IsNullOrWhiteSpace(row.Title)) row.AddError("Title", "Title is required.");
        if (!DateOnly.TryParse(row.EffectiveDateText, out _)) row.AddError("EffectiveDate", "Effective date is not a valid date.");
    }
}
