using CivicFlow.Application.Abstractions;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Services;

public sealed class EfReferenceDataProvider : IReferenceDataProvider
{
    private readonly CivicFlowDbContext _dbContext;

    public EfReferenceDataProvider(CivicFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReferenceDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var agencyCodes = await _dbContext.Agencies.Where(agency => agency.IsActive).Select(agency => agency.Code).ToArrayAsync(cancellationToken);
        var fundCodes = await _dbContext.Funds.Where(fund => fund.IsActive).Select(fund => fund.Code).ToArrayAsync(cancellationToken);
        var programCodes = await _dbContext.BudgetPrograms.Where(program => program.IsActive).Select(program => program.Code).ToArrayAsync(cancellationToken);
        var requestNumbers = await _dbContext.Requests.Select(request => request.RequestNumber).ToArrayAsync(cancellationToken);

        return new ReferenceDataSnapshot(
            agencyCodes.ToHashSet(StringComparer.OrdinalIgnoreCase),
            fundCodes.ToHashSet(StringComparer.OrdinalIgnoreCase),
            programCodes.ToHashSet(StringComparer.OrdinalIgnoreCase),
            requestNumbers.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }
}
