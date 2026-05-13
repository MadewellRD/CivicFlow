using CivicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Infrastructure.Persistence.Configurations;

public sealed class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("Requests");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.RequestNumber).HasMaxLength(32).IsRequired();
        builder.HasIndex(request => request.RequestNumber).IsUnique();
        builder.Property(request => request.Title).HasMaxLength(200).IsRequired();
        builder.Property(request => request.BusinessJustification).HasMaxLength(4000).IsRequired();
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(request => request.Category).HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(request => request.EstimatedAmount).HasPrecision(18, 2);
    }
}
