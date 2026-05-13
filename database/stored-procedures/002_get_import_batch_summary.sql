CREATE OR ALTER PROCEDURE dbo.GetImportBatchSummary
    @ImportBatchId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.Id,
        b.FileName,
        b.Status,
        COUNT(r.Id) AS TotalRows,
        SUM(CASE WHEN r.RowStatus IN ('Valid', 'Transformed') THEN 1 ELSE 0 END) AS AcceptedRows,
        SUM(CASE WHEN r.RowStatus = 'Rejected' THEN 1 ELSE 0 END) AS RejectedRows
    FROM dbo.ImportBatches b
    LEFT JOIN dbo.ImportStagingRows r ON r.ImportBatchId = b.Id
    WHERE b.Id = @ImportBatchId
    GROUP BY b.Id, b.FileName, b.Status;
END;
