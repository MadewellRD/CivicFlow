CREATE OR ALTER PROCEDURE dbo.ValidateImportBatch
    @ImportBatchId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    DELETE e
    FROM dbo.ImportValidationErrors e
    INNER JOIN dbo.ImportStagingRows r ON r.Id = e.ImportStagingRowId
    WHERE r.ImportBatchId = @ImportBatchId;

    INSERT INTO dbo.ImportValidationErrors (Id, ImportStagingRowId, FieldName, Message)
    SELECT NEWID(), r.Id, 'AgencyCode', 'Agency code was not found or is inactive.'
    FROM dbo.ImportStagingRows r
    LEFT JOIN dbo.Agencies a ON a.Code = r.AgencyCode AND a.IsActive = 1
    WHERE r.ImportBatchId = @ImportBatchId AND a.Id IS NULL;

    INSERT INTO dbo.ImportValidationErrors (Id, ImportStagingRowId, FieldName, Message)
    SELECT NEWID(), r.Id, 'FundCode', 'Fund code was not found or is inactive.'
    FROM dbo.ImportStagingRows r
    LEFT JOIN dbo.Funds f ON f.Code = r.FundCode AND f.IsActive = 1
    WHERE r.ImportBatchId = @ImportBatchId AND f.Id IS NULL;

    INSERT INTO dbo.ImportValidationErrors (Id, ImportStagingRowId, FieldName, Message)
    SELECT NEWID(), r.Id, 'Amount', 'Amount exceeds automatic import threshold and requires manual review.'
    FROM dbo.ImportStagingRows r
    WHERE r.ImportBatchId = @ImportBatchId AND r.Amount > 5000000;

    UPDATE r
        SET RowStatus = CASE WHEN EXISTS (SELECT 1 FROM dbo.ImportValidationErrors e WHERE e.ImportStagingRowId = r.Id) THEN 'Rejected' ELSE 'Valid' END
    FROM dbo.ImportStagingRows r
    WHERE r.ImportBatchId = @ImportBatchId;

    UPDATE dbo.ImportBatches SET Status = 'Validated' WHERE Id = @ImportBatchId;
END;
