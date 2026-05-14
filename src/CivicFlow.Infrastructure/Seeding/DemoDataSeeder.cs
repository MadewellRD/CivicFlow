using CivicFlow.Application.Abstractions;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CivicFlow.Infrastructure.Seeding;

/// <summary>
/// Idempotent demo data seeder. Runs on application start. Safe to invoke
/// against an existing populated database: each row is keyed by deterministic
/// Guid so re-running is a no-op.
///
/// The dataset is deliberately shaped to exercise every workflow state, every
/// validation rule in <c>ImportValidationService</c>, and every role policy
/// so that an interviewer can click through the system without first having
/// to author content.
/// </summary>
public sealed class DemoDataSeeder
{
    private readonly CivicFlowDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(CivicFlowDbContext db, IClock clock, ILogger<DemoDataSeeder> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        await SeedUsersAsync(cancellationToken);
        await SeedReferenceAsync(cancellationToken);
        await SeedRequestsAsync(cancellationToken);

        _logger.LogInformation(
            "Demo seed complete. Users={UserCount} Agencies={AgencyCount} Requests={RequestCount}",
            await _db.Users.CountAsync(cancellationToken),
            await _db.Agencies.CountAsync(cancellationToken),
            await _db.Requests.CountAsync(cancellationToken));
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var users = new[]
        {
            new SeedUser("10000000-0000-0000-0000-000000000005", "Sage Admin", "admin@example.gov", UserRole.Admin),
            new SeedUser("10000000-0000-0000-0000-000000000006", "Quinn Auditor", "auditor@example.gov", UserRole.ReadOnlyAuditor),
            new SeedUser("10000000-0000-0000-0000-000000000007", "Morgan Requester", "morgan@dshs.example.gov", UserRole.Requester),
            new SeedUser("10000000-0000-0000-0000-000000000008", "Drew Analyst", "drew@example.gov", UserRole.BudgetAnalyst),
            new SeedUser("10000000-0000-0000-0000-000000000009", "Parker Developer", "parker@example.gov", UserRole.ApplicationDeveloper),
            new SeedUser("10000000-0000-0000-0000-00000000000a", "Reese Approver", "reese@example.gov", UserRole.Approver),
            new SeedUser("10000000-0000-0000-0000-00000000000b", "Jamie Requester", "jamie@doh.example.gov", UserRole.Requester),
            new SeedUser("10000000-0000-0000-0000-00000000000c", "Taylor Analyst", "taylor@example.gov", UserRole.BudgetAnalyst)
        };

        var existing = await _db.Users.Select(u => u.Id).ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            if (existing.Contains(user.Id)) continue;
            var entity = new AppUser(user.DisplayName, user.Email, user.Role);
            SetPrivateProperty(entity, "Id", user.Id);
            _db.Users.Add(entity);
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedReferenceAsync(CancellationToken cancellationToken)
    {
        var approver = Guid.Parse("10000000-0000-0000-0000-000000000004");

        var agencies = new[]
        {
            new SeedAgency("20000000-0000-0000-0000-000000000002", "DSHS", "Department of Social and Health Services"),
            new SeedAgency("20000000-0000-0000-0000-000000000003", "DOH", "Department of Health"),
            new SeedAgency("20000000-0000-0000-0000-000000000004", "DOL", "Department of Licensing"),
            new SeedAgency("20000000-0000-0000-0000-000000000005", "DCYF", "Department of Children, Youth, and Families")
        };
        var existingAgencies = await _db.Agencies.Select(a => a.Id).ToListAsync(cancellationToken);
        foreach (var a in agencies)
        {
            if (existingAgencies.Contains(a.Id)) continue;
            var entity = new Agency(a.Code, a.Name);
            SetPrivateProperty(entity, "Id", a.Id);
            SetPrivateProperty(entity, "CreatedByUserId", approver);
            _db.Agencies.Add(entity);
        }

        var funds = new[]
        {
            new SeedFund("30000000-0000-0000-0000-000000000002", "GF-F", "General Fund-Federal"),
            new SeedFund("30000000-0000-0000-0000-000000000003", "GF-P", "General Fund-Private/Local"),
            new SeedFund("30000000-0000-0000-0000-000000000004", "TRANSPO", "Transportation Fund"),
            new SeedFund("30000000-0000-0000-0000-000000000005", "CAPITAL", "Capital Projects Fund")
        };
        var existingFunds = await _db.Funds.Select(f => f.Id).ToListAsync(cancellationToken);
        foreach (var f in funds)
        {
            if (existingFunds.Contains(f.Id)) continue;
            var entity = new Fund(f.Code, f.Name);
            SetPrivateProperty(entity, "Id", f.Id);
            SetPrivateProperty(entity, "CreatedByUserId", approver);
            _db.Funds.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Budget programs are agency-scoped so we add them after agencies are persisted.
        var ofmId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var programs = new[]
        {
            new SeedProgram("40000000-0000-0000-0000-000000000002", "REV", "Revenue Operations", ofmId),
            new SeedProgram("40000000-0000-0000-0000-000000000003", "FORE", "Forecasting", ofmId),
            new SeedProgram("40000000-0000-0000-0000-000000000004", "HRMS", "HR Management Systems", ofmId)
        };
        var existingPrograms = await _db.BudgetPrograms.Select(p => p.Id).ToListAsync(cancellationToken);
        foreach (var p in programs)
        {
            if (existingPrograms.Contains(p.Id)) continue;
            var entity = new BudgetProgram(p.Code, p.Name, p.AgencyId);
            SetPrivateProperty(entity, "Id", p.Id);
            SetPrivateProperty(entity, "CreatedByUserId", approver);
            _db.BudgetPrograms.Add(entity);
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRequestsAsync(CancellationToken cancellationToken)
    {
        if (await _db.Requests.CountAsync(cancellationToken) >= 15)
        {
            return;
        }

        var requester1 = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var requester2 = Guid.Parse("10000000-0000-0000-0000-000000000007");
        var requester3 = Guid.Parse("10000000-0000-0000-0000-00000000000b");
        var analyst = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var developer = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var approver = Guid.Parse("10000000-0000-0000-0000-000000000004");
        var ofm = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var dshs = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var doh = Guid.Parse("20000000-0000-0000-0000-000000000003");
        var dol = Guid.Parse("20000000-0000-0000-0000-000000000004");
        var gfs = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var gff = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var transpo = Guid.Parse("30000000-0000-0000-0000-000000000004");
        var bud = Guid.Parse("40000000-0000-0000-0000-000000000001");
        var hrms = Guid.Parse("40000000-0000-0000-0000-000000000004");

        var seed = new[]
        {
            // 3 in Draft
            BuildRequest("Q3 forecast adjustment for caseload growth", RequestCategory.BudgetChange, ofm, requester1, gfs, bud, 425_000m, "Revised caseload projections require allotment re-baseline.", new[] { RequestStatus.Draft }),
            BuildRequest("HR rate table update for new step increases", RequestCategory.HrFundingChange, dshs, requester2, gfs, hrms, 1_250_000m, "Collective bargaining adjustment effective next FY.", new[] { RequestStatus.Draft }),
            BuildRequest("DCYF kinship care funding split correction", RequestCategory.FinanceDataCorrection, dshs, requester2, gff, hrms, 87_500m, "Two grant lines posted to wrong AFRS account.", new[] { RequestStatus.Draft }),

            // 2 Submitted
            BuildRequest("Reconcile legacy AFRS export for May close", RequestCategory.LegacyIntegrationIssue, ofm, requester1, gfs, bud, 0m, "Stuck records in the legacy outbound queue need re-export.", new[] { RequestStatus.Draft, RequestStatus.Submitted }),
            BuildRequest("DOH licensing fee revenue retag", RequestCategory.FinanceDataCorrection, doh, requester3, gff, bud, 220_000m, "Misposted revenue from licensing renewals needs fund reclass.", new[] { RequestStatus.Draft, RequestStatus.Submitted }),

            // 3 Triage / Analyst Review
            BuildRequest("Transportation fund supplemental request", RequestCategory.BudgetChange, dol, requester2, transpo, bud, 3_400_000m, "Emergency bridge inspection program funding.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage }),
            BuildRequest("OFM intern stipend reclassification", RequestCategory.HrFundingChange, ofm, requester1, gfs, hrms, 42_000m, "Move stipends from object N to object H.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.AnalystReview }),
            BuildRequest("DSHS case management correction batch 0042", RequestCategory.FinanceDataCorrection, dshs, requester2, gfs, bud, 96_300m, "Bulk correction of misclassified caseload data points.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.AnalystReview }),

            // 2 Technical Review
            BuildRequest("Legacy DAWN-to-AFRS export schema upgrade", RequestCategory.LegacyIntegrationIssue, ofm, requester1, gfs, bud, 175_000m, "DAWN payload no longer matches AFRS contract; transform needed.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.TechnicalReview }),
            BuildRequest("HRMS deduction code mapping fix", RequestCategory.HrFundingChange, dshs, requester2, gfs, hrms, 64_000m, "New deduction codes from PEBB need to be mapped before payroll.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.AnalystReview, RequestStatus.TechnicalReview }),

            // 2 Approved
            BuildRequest("FY27 statewide salary survey roll-up", RequestCategory.HrFundingChange, ofm, requester1, gfs, hrms, 0m, "Annual salary survey publication for legislative session prep.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.AnalystReview, RequestStatus.Approved }),
            BuildRequest("DSHS caseload growth supplemental", RequestCategory.BudgetChange, dshs, requester2, gfs, bud, 2_900_000m, "Caseload exceeds baseline; supplemental authorization needed.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.AnalystReview, RequestStatus.Approved }),

            // 1 Implemented
            BuildRequest("Q2 BARS code retirement cleanup", RequestCategory.FinanceDataCorrection, ofm, requester1, gfs, bud, 12_500m, "Retire deprecated BARS codes and remap open transactions.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.TechnicalReview, RequestStatus.Approved, RequestStatus.Implemented }),

            // 1 Closed
            BuildRequest("Legacy file watcher restart automation", RequestCategory.LegacyIntegrationIssue, ofm, requester1, gfs, bud, 0m, "Auto-restart of the AFRS file watcher when it dies.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.TechnicalReview, RequestStatus.Approved, RequestStatus.Implemented, RequestStatus.Closed }),

            // 1 Rejected
            BuildRequest("Personal vendor invoice misclassification", RequestCategory.FinanceDataCorrection, ofm, requester1, gfs, bud, 5_000m, "Vendor invoice posted to wrong cost center.", new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Triage, RequestStatus.Rejected })
        };

        foreach (var seedRequest in seed)
        {
            _db.Requests.Add(seedRequest);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private Request BuildRequest(
        string title,
        RequestCategory category,
        Guid agencyId,
        Guid requesterId,
        Guid? fundId,
        Guid? programId,
        decimal estimatedAmount,
        string justification,
        RequestStatus[] statusTrajectory)
    {
        var request = new Request(title, category, agencyId, requesterId, fundId, programId, estimatedAmount, justification);
        request.SetRequestNumber($"CF-DEMO-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}");
        var now = _clock.UtcNow;
        var actor = requesterId;
        foreach (var target in statusTrajectory.Skip(1))
        {
            request.TransitionTo(target, actor, $"Seed transition to {target}.", now);
        }
        return request;
    }

    private static void SetPrivateProperty(object instance, string propertyName, object value)
    {
        var prop = instance.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (prop is null || prop.SetMethod is null)
        {
            var field = instance.GetType().GetField($"<{propertyName}>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
            return;
        }
        prop.SetValue(instance, value);
    }

    private sealed record SeedUser(string IdText, string DisplayName, string Email, UserRole Role)
    {
        public Guid Id => Guid.Parse(IdText);
    }

    private sealed record SeedAgency(string IdText, string Code, string Name)
    {
        public Guid Id => Guid.Parse(IdText);
    }

    private sealed record SeedFund(string IdText, string Code, string Name)
    {
        public Guid Id => Guid.Parse(IdText);
    }

    private sealed record SeedProgram(string IdText, string Code, string Name, Guid AgencyId)
    {
        public Guid Id => Guid.Parse(IdText);
    }
}
