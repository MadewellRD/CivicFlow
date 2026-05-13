using CivicFlow.Domain.Common;
using CivicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Persistence;

public sealed class CivicFlowDbContext : DbContext
{
    public CivicFlowDbContext(DbContextOptions<CivicFlowDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<BudgetProgram> BudgetPrograms => Set<BudgetProgram>();
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    public DbSet<CatalogFieldDefinition> CatalogFieldDefinitions => Set<CatalogFieldDefinition>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestStatusHistory> RequestStatusHistory => Set<RequestStatusHistory>();
    public DbSet<RequestComment> RequestComments => Set<RequestComment>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportStagingRow> ImportStagingRows => Set<ImportStagingRow>();
    public DbSet<ImportValidationError> ImportValidationErrors => Set<ImportValidationError>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<IncidentReport> IncidentReports => Set<IncidentReport>();
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CivicFlowDbContext).Assembly);
        ConfigureOwnedCollections(modelBuilder);
        modelBuilder.SeedReferenceData();
    }

    private static void ConfigureOwnedCollections(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Request>()
            .HasMany(request => request.StatusHistory)
            .WithOne(history => history.Request)
            .HasForeignKey(history => history.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasMany(request => request.Comments)
            .WithOne(comment => comment.Request)
            .HasForeignKey(comment => comment.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ImportBatch>()
            .HasMany(batch => batch.Rows)
            .WithOne(row => row.ImportBatch)
            .HasForeignKey(row => row.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ImportStagingRow>()
            .HasMany(row => row.Errors)
            .WithOne(error => error.ImportStagingRow)
            .HasForeignKey(error => error.ImportStagingRowId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserGroup>()
            .HasIndex(userGroup => new { userGroup.UserId, userGroup.GroupId })
            .IsUnique();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
