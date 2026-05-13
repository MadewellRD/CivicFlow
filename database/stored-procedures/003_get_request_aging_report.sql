CREATE OR ALTER PROCEDURE dbo.GetRequestAgingReport
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Status,
        COUNT(*) AS RequestCount,
        AVG(DATEDIFF(day, CreatedAt, SYSUTCDATETIME())) AS AverageAgeDays,
        MAX(DATEDIFF(day, CreatedAt, SYSUTCDATETIME())) AS OldestAgeDays
    FROM dbo.Requests
    WHERE Status NOT IN ('Closed', 'Cancelled', 'Rejected')
    GROUP BY Status
    ORDER BY OldestAgeDays DESC;
END;
