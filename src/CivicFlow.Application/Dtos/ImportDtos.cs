using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Dtos;

public sealed record ImportRowDto(
    int RowNumber,
    string RequestNumber,
    string AgencyCode,
    string FundCode,
    string ProgramCode,
    int FiscalYear,
    decimal Amount,
    string Title,
    string EffectiveDateText);

public sealed record ImportBatchSummaryDto(
    Guid Id,
    string FileName,
    string Status,
    int TotalRows,
    int AcceptedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportRowSummaryDto> Rows)
{
    public static ImportBatchSummaryDto FromEntity(ImportBatch batch)
    {
        return new ImportBatchSummaryDto(
            batch.Id,
            batch.FileName,
            batch.Status,
            batch.TotalRows,
            batch.AcceptedRows,
            batch.RejectedRows,
            batch.Rows.Select(ImportRowSummaryDto.FromEntity).ToArray());
    }
}

public sealed record ImportRowSummaryDto(
    int RowNumber,
    string RequestNumber,
    ImportRowStatus Status,
    IReadOnlyCollection<string> Errors)
{
    public static ImportRowSummaryDto FromEntity(ImportStagingRow row)
    {
        return new ImportRowSummaryDto(
            row.RowNumber,
            row.RequestNumber,
            row.RowStatus,
            row.Errors.Select(error => $"{error.FieldName}: {error.Message}").ToArray());
    }
}
