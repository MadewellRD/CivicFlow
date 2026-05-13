namespace CivicFlow.Application.Abstractions;

public sealed record ReferenceDataSnapshot(
    IReadOnlySet<string> AgencyCodes,
    IReadOnlySet<string> FundCodes,
    IReadOnlySet<string> ProgramCodes,
    IReadOnlySet<string> ExistingRequestNumbers);

public sealed record TransformReferenceDataSnapshot(
    IReadOnlyDictionary<string, Guid> AgencyIdsByCode,
    IReadOnlyDictionary<string, Guid> FundIdsByCode,
    IReadOnlyDictionary<string, Guid> ProgramIdsByAgencyAndCode,
    IReadOnlySet<string> ExistingRequestNumbers)
{
    public static string ProgramKey(Guid agencyId, string programCode)
    {
        return $"{agencyId:N}:{programCode.Trim().ToUpperInvariant()}";
    }
}

public interface IReferenceDataProvider
{
    Task<ReferenceDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
    Task<TransformReferenceDataSnapshot> GetTransformSnapshotAsync(CancellationToken cancellationToken);
}
