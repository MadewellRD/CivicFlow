using CivicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.ActionType).HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(log => log.EntityName).HasMaxLength(120).IsRequired();
        builder.Property(log => log.Summary).HasMaxLength(1000).IsRequired();
        builder.Property(log => log.BeforeJson).HasColumnType("nvarchar(max)");
        builder.Property(log => log.AfterJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(log => new { log.EntityName, log.EntityId, log.OccurredAt });
    }
}
