using CivicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Infrastructure.Persistence.Configurations;

public sealed class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("Agencies");
        builder.HasKey(agency => agency.Id);
        builder.Property(agency => agency.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(agency => agency.Code).IsUnique();
        builder.Property(agency => agency.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class FundConfiguration : IEntityTypeConfiguration<Fund>
{
    public void Configure(EntityTypeBuilder<Fund> builder)
    {
        builder.ToTable("Funds");
        builder.HasKey(fund => fund.Id);
        builder.Property(fund => fund.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(fund => fund.Code).IsUnique();
        builder.Property(fund => fund.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class BudgetProgramConfiguration : IEntityTypeConfiguration<BudgetProgram>
{
    public void Configure(EntityTypeBuilder<BudgetProgram> builder)
    {
        builder.ToTable("BudgetPrograms");
        builder.HasKey(program => program.Id);
        builder.Property(program => program.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(program => new { program.AgencyId, program.Code }).IsUnique();
        builder.Property(program => program.Name).HasMaxLength(200).IsRequired();
    }
}
