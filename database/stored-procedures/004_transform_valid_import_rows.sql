CREATE OR ALTER PROCEDURE dbo.TransformValidImportRows
    @ImportBatchId uniqueidentifier,
    @ActorUserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Created TABLE (
        RequestId uniqueidentifier NOT NULL,
        ImportStagingRowId uniqueidentifier NOT NULL,
        RequestNumber nvarchar(40) NOT NULL
    );

    INSERT INTO dbo.Requests (
        Id,
        RequestNumber,
        Title,
        Category,
        AgencyId,
        RequesterId,
        FundId,
        BudgetProgramId,
        EstimatedAmount,
        BusinessJustification,
        Status,
        SubmittedAt,
        CreatedAt,
        CreatedByUserId
    )
    OUTPUT inserted.Id, src.Id, inserted.RequestNumber INTO @Created (RequestId, ImportStagingRowId, RequestNumber)
    SELECT
        NEWID(),
        src.RequestNumber,
        src.Title,
        'FinanceDataCorrection',
        agency.Id,
        @ActorUserId,
        fund.Id,
        program.Id,
        src.Amount,
        CONCAT('Imported from batch ', CONVERT(nvarchar(36), @ImportBatchId), ', row ', src.RowNumber, '.'),
        'Submitted',
        SYSUTCDATETIME(),
        SYSUTCDATETIME(),
        @ActorUserId
    FROM dbo.ImportStagingRows AS src
    INNER JOIN dbo.Agencies AS agency ON agency.Code = src.AgencyCode AND agency.IsActive = 1
    INNER JOIN dbo.Funds AS fund ON fund.Code = src.FundCode AND fund.IsActive = 1
    INNER JOIN dbo.BudgetPrograms AS program ON program.Code = src.ProgramCode AND program.AgencyId = agency.Id AND program.IsActive = 1
    WHERE src.ImportBatchId = @ImportBatchId
      AND src.RowStatus = 'Valid'
      AND NOT EXISTS (SELECT 1 FROM dbo.Requests existing WHERE existing.RequestNumber = src.RequestNumber);

    INSERT INTO dbo.RequestStatusHistory (Id, RequestId, Status, ActorUserId, Reason, OccurredAt)
    SELECT NEWID(), RequestId, 'Submitted', @ActorUserId, 'Created from validated import row.', SYSUTCDATETIME()
    FROM @Created;

    UPDATE src
        SET RowStatus = 'Transformed'
    FROM dbo.ImportStagingRows AS src
    INNER JOIN @Created AS created ON created.ImportStagingRowId = src.Id;

    UPDATE dbo.ImportBatches
        SET Status = 'Transformed', UpdatedAt = SYSUTCDATETIME(), UpdatedByUserId = @ActorUserId
    WHERE Id = @ImportBatchId
      AND EXISTS (SELECT 1 FROM @Created);

    SELECT COUNT(*) AS TransformedRows FROM @Created;
END;
