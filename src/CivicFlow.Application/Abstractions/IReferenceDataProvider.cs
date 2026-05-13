namespace CivicFlow.Application.Abstractions;

public sealed record ReferenceDataSnapshot(
    IReadOnlySet<string> AgencyCodes,
    IReadOnlySet<string> FundCodes,
    IReadOnlySet<string> ProgramCodes,
    IReadOnlySet<string> ExistingRequestNumbers);

public interface IReferenceDataProvider
{
    Task<ReferenceDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}
