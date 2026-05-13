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

    public async Task<TransformReferenceDataSnapshot> GetTransformSnapshotAsync(CancellationToken cancellationToken)
    {
        var agencies = await _dbContext.Agencies
            .Where(agency => agency.IsActive)
            .Select(agency => new { agency.Code, agency.Id })
            .ToArrayAsync(cancellationToken);
        var funds = await _dbContext.Funds
            .Where(fund => fund.IsActive)
            .Select(fund => new { fund.Code, fund.Id })
            .ToArrayAsync(cancellationToken);
        var programs = await _dbContext.BudgetPrograms
            .Where(program => program.IsActive)
            .Select(program => new { program.AgencyId, program.Code, program.Id })
            .ToArrayAsync(cancellationToken);
        var requestNumbers = await _dbContext.Requests.Select(request => request.RequestNumber).ToArrayAsync(cancellationToken);

        return new TransformReferenceDataSnapshot(
            agencies.ToDictionary(agency => agency.Code, agency => agency.Id, StringComparer.OrdinalIgnoreCase),
            funds.ToDictionary(fund => fund.Code, fund => fund.Id, StringComparer.OrdinalIgnoreCase),
            programs.ToDictionary(program => TransformReferenceDataSnapshot.ProgramKey(program.AgencyId, program.Code), program => program.Id, StringComparer.OrdinalIgnoreCase),
            requestNumbers.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }
}
