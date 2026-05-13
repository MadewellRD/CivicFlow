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
        var requests = new FakeRequestRepository();
        var referenceData = new FakeReferenceDataProvider();
        var service = new ImportValidationService(repository, requests, referenceData, new FakeAuditWriter(), new FixedClock());
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
        var service = new ImportValidationService(repository, new FakeRequestRepository(), new FakeReferenceDataProvider(), new FakeAuditWriter(), new FixedClock());
        var userId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

        var summary = await service.CreateAndValidateBatchAsync("good.csv", userId, [
            new ImportRowDto(1, "REQ-2", "OFM", "GF-S", "BUD", 2026, 10m, "Good row", "2026-07-01")
        ], CancellationToken.None);

        Assert.Equal(1, summary.AcceptedRows);
        Assert.Equal(0, summary.RejectedRows);
    }

    [Fact]
    public async Task TransformBatchCreatesSubmittedRequestAndMarksRowTransformed()
    {
        var repository = new FakeImportRepository();
        var requests = new FakeRequestRepository();
        var audit = new FakeAuditWriter();
        var service = new ImportValidationService(repository, requests, new FakeReferenceDataProvider(), audit, new FixedClock());
        var userId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

        var summary = await service.CreateAndValidateBatchAsync("good.csv", userId, [
            new ImportRowDto(1, "REQ-3", "OFM", "GF-S", "BUD", 2026, 10m, "Good row", "2026-07-01")
        ], CancellationToken.None);

        var transformed = await service.TransformBatchAsync(summary.Id, userId, CancellationToken.None);
        var created = Assert.Single(requests.Requests);

        Assert.Equal("Transformed", transformed.Status);
        Assert.Equal(1, transformed.AcceptedRows);
        Assert.Equal(ImportRowStatus.Transformed, Assert.Single(transformed.Rows).Status);
        Assert.Equal("REQ-3", created.RequestNumber);
        Assert.Equal(RequestStatus.Submitted, created.Status);
        Assert.Contains(audit.Summaries, summary => summary.Contains("transformed 1 valid rows", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FakeImportRepository : IImportRepository
    {
        private readonly Dictionary<Guid, ImportBatch> _batches = [];
        public Task AddBatchAsync(ImportBatch batch, CancellationToken cancellationToken) { _batches[batch.Id] = batch; return Task.CompletedTask; }
        public Task<ImportBatch?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken) => Task.FromResult(_batches.GetValueOrDefault(batchId));
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeRequestRepository : IRequestRepository
    {
        private readonly Dictionary<Guid, Request> _requests = [];
        public IReadOnlyCollection<Request> Requests => _requests.Values.ToArray();
        public Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_requests.GetValueOrDefault(id));
        public Task<IReadOnlyCollection<Request>> ListAsync(CancellationToken cancellationToken) => Task.FromResult((IReadOnlyCollection<Request>)_requests.Values.ToArray());
        public Task AddAsync(Request request, CancellationToken cancellationToken) { _requests[request.Id] = request; return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeReferenceDataProvider : IReferenceDataProvider
    {
        private static readonly Guid AgencyId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
        private static readonly Guid FundId = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
        private static readonly Guid ProgramId = Guid.Parse("dddddddd-0000-0000-0000-000000000001");

        public Task<ReferenceDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new ReferenceDataSnapshot(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "OFM" },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "GF-S" },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BUD" },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "REQ-EXISTING" }));
        }

        public Task<TransformReferenceDataSnapshot> GetTransformSnapshotAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new TransformReferenceDataSnapshot(
                new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase) { ["OFM"] = AgencyId },
                new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase) { ["GF-S"] = FundId },
                new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase) { [TransformReferenceDataSnapshot.ProgramKey(AgencyId, "BUD")] = ProgramId },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "REQ-EXISTING" }));
        }
    }

    private sealed class FakeAuditWriter : IAuditWriter
    {
        public List<string> Summaries { get; } = [];
        public Task WriteAsync(Guid actorUserId, AuditActionType actionType, string entityName, Guid entityId, string summary, CancellationToken cancellationToken) { Summaries.Add(summary); return Task.CompletedTask; }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 5, 13, 12, 0, 0, TimeSpan.Zero);
    }
}
