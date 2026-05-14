using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Ai;
using CivicFlow.Application.Services;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using CivicFlow.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CivicFlow.Tests;

public sealed class ImportErrorExplainerServiceTests
{
    [Fact]
    public async Task ExplainsOnlyRejectedRowsAndIgnoresValidOnes()
    {
        var batchId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var batch = new ImportBatch("agency-budget-2026.csv", actorId);
        var row1 = new ImportStagingRow(batch.Id, 1, "REQ-1", "BAD", "GF-S", "BUD", 2026, 1000, "Title", "2026-07-01");
        row1.AddError("AgencyCode", "Agency code was not found or is inactive.");
        row1.AddError("Amount", "Amount exceeds automatic import threshold and requires manual review.");
        batch.AddRow(row1);

        var row2 = new ImportStagingRow(batch.Id, 2, "REQ-2", "OFM", "GF-S", "BUD", 2026, 500, "Valid row", "2026-07-01");
        row2.MarkValid();
        batch.AddRow(row2);
        batch.MarkValidated();

        var service = new ImportErrorExplainerService(
            new FakeImportRepository(batch),
            new DeterministicMockAdapter(NullLogger<DeterministicMockAdapter>.Instance),
            new PromptSchemaRegistry(),
            new RecordingAuditWriter());

        var result = await service.ExplainAsync(batch.Id, actorId, CancellationToken.None);

        Assert.Equal(1, result.RowsExplained);
        Assert.Equal(1, result.RowsSkipped);
        var explanation = Assert.Single(result.Explanations);
        Assert.True(explanation.ServedFromMock);
        Assert.NotEmpty(explanation.FieldGuidance);
        Assert.Contains("Please use a valid agency code", explanation.AgencyMessage);
    }

    private sealed class FakeImportRepository : IImportRepository
    {
        private readonly ImportBatch _batch;
        public FakeImportRepository(ImportBatch batch) => _batch = batch;
        public Task AddBatchAsync(ImportBatch batch, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<ImportBatch?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken) =>
            Task.FromResult<ImportBatch?>(_batch.Id == batchId ? _batch : null);
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class RecordingAuditWriter : IAuditWriter
    {
        public List<string> Summaries { get; } = new();
        public Task WriteAsync(Guid actorUserId, AuditActionType actionType, string entityName, Guid entityId, string summary, CancellationToken cancellationToken)
        {
            Summaries.Add(summary);
            return Task.CompletedTask;
        }
    }
}
