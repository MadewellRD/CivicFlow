using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Persistence;

public static class ModelBuilderExtensions
{
    public static void SeedReferenceData(this ModelBuilder modelBuilder)
    {
        var requesterId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var analystId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var developerId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var approverId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        var agencyId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var fundId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var programId = Guid.Parse("40000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<AppUser>().HasData(
            new { Id = requesterId, DisplayName = "Riley Requester", Email = "requester@example.gov", PrimaryRole = UserRole.Requester, CreatedAt = DateTimeOffset.UnixEpoch, CreatedByUserId = requesterId },
            new { Id = analystId, DisplayName = "Bailey Analyst", Email = "analyst@example.gov", PrimaryRole = UserRole.BudgetAnalyst, CreatedAt = DateTimeOffset.UnixEpoch, CreatedByUserId = analystId },
            new { Id = developerId, DisplayName = "Casey Developer", Email = "developer@example.gov", PrimaryRole = UserRole.ApplicationDeveloper, CreatedAt = DateTimeOffset.UnixEpoch, CreatedByUserId = developerId },
            new { Id = approverId, DisplayName = "Avery Approver", Email = "approver@example.gov", PrimaryRole = UserRole.Approver, CreatedAt = DateTimeOffset.UnixEpoch, CreatedByUserId = approverId });

        modelBuilder.Entity<Agency>().HasData(new { Id = agencyId, Code = "OFM", Name = "Office of Financial Management", IsActive = true, CreatedAt = DateTimeOffset.UnixEpoch, CreatedByUserId = approverId });
        modelBuilder.Entity<Fund>().HasData(new { Id = fundId, Code = "GF-S", Name = "General Fund-State", IsActive = true, CreatedAt = DateTimeOffset.UnixEpoch, CreatedByUserId = approverId });
        modelBuilder.Entity<BudgetProgram>().HasData(new { Id = programId, Code = "BUD", Name = "Budget Operations", AgencyId = agencyId, IsActive = true, CreatedAt = DateTimeOffset.UnixEpoch, CreatedByUserId = approverId });
    }
}
