using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Dtos;
using CivicFlow.Application.Services;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using Xunit;

namespace CivicFlow.Tests;

public sealed class ImportValidationTests
{
    [Fact]
    public async Task InvalidImportRowIsRejectedWithFieldErrors()
    {
        var repository = new FakeImportRepository();
        var referenceData = new FakeReferenceDataProvider();
        var service = new ImportValidationService(repository, referenceData, new FakeAuditWriter());
        var userId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

        var summary = await service.CreateAndValidateBatchAsync("bad.csv", userId, [
            new ImportRowDto(1, "REQ-1", "NOPE", "BAD", "BUD", 2026, 10m, "Bad row", "2026-07-01")
        ], CancellationToken.None);

        var row = Assert.Single(summary.Rows);
        Assert.Equal(ImportRowStatus.Rejected, row.Status);
        Assert.Contains(row.Errors, error => error.Contains("AgencyCode", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(row.Errors, error => error.Contains("FundCode", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ValidImportRowIsAccepted()
    {
        var repository = new FakeImportRepository();
        var service = new ImportValidationService(repository, new FakeReferenceDataProvider(), new FakeAuditWriter());
        var userId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

        var summary = await service.CreateAndValidateBatchAsync("good.csv", userId, [
            new ImportRowDto(1, "REQ-2", "OFM", "GF-S", "BUD", 2026, 10m, "Good row", "2026-07-01")
        ], CancellationToken.None);

        Assert.Equal(1, summary.AcceptedRows);
        Assert.Equal(0, summary.RejectedRows);
    }

    private sealed class FakeImportRepository : IImportRepository
    {
        private readonly Dictionary<Guid, ImportBatch> _batches = [];
        public Task AddBatchAsync(ImportBatch batch, CancellationToken cancellationToken) { _batches[batch.Id] = batch; return Task.CompletedTask; }
        public Task<ImportBatch?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken) => Task.FromResult(_batches.GetValueOrDefault(batchId));
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeReferenceDataProvider : IReferenceDataProvider
    {
        public Task<ReferenceDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new ReferenceDataSnapshot(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "OFM" },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "GF-S" },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BUD" },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "REQ-EXISTING" }));
        }
    }

    private sealed class FakeAuditWriter : IAuditWriter
    {
        public Task WriteAsync(Guid actorUserId, AuditActionType actionType, string entityName, Guid entityId, string summary, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
