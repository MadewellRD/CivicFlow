using CivicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Infrastructure.Persistence.Configurations;

public sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");
        builder.HasKey(batch => batch.Id);
        builder.Property(batch => batch.FileName).HasMaxLength(260).IsRequired();
        builder.Property(batch => batch.Status).HasMaxLength(40).IsRequired();
    }
}

public sealed class ImportStagingRowConfiguration : IEntityTypeConfiguration<ImportStagingRow>
{
    public void Configure(EntityTypeBuilder<ImportStagingRow> builder)
    {
        builder.ToTable("ImportStagingRows");
        builder.HasKey(row => row.Id);
        builder.Property(row => row.RowStatus).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(row => row.RequestNumber).HasMaxLength(40).IsRequired();
        builder.Property(row => row.AgencyCode).HasMaxLength(20).IsRequired();
        builder.Property(row => row.FundCode).HasMaxLength(20).IsRequired();
        builder.Property(row => row.ProgramCode).HasMaxLength(20).IsRequired();
        builder.Property(row => row.Amount).HasPrecision(18, 2);
        builder.Property(row => row.Title).HasMaxLength(200).IsRequired();
        builder.Property(row => row.EffectiveDateText).HasMaxLength(40).IsRequired();
    }
}

public sealed class ImportValidationErrorConfiguration : IEntityTypeConfiguration<ImportValidationError>
{
    public void Configure(EntityTypeBuilder<ImportValidationError> builder)
    {
        builder.ToTable("ImportValidationErrors");
        builder.HasKey(error => error.Id);
        builder.Property(error => error.FieldName).HasMaxLength(80).IsRequired();
        builder.Property(error => error.Message).HasMaxLength(500).IsRequired();
    }
}
