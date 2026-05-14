using CivicFlow.Domain.Entities;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicFlow.Tests;

public sealed class PersistenceConfigurationTests
{
    [Fact]
    public void RequestStatusHistoryStatusUsesStringConversion()
    {
        var options = new DbContextOptionsBuilder<CivicFlowDbContext>()
            .UseSqlServer("Server=(local);Database=CivicFlow_ModelOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new CivicFlowDbContext(options);

        var entity = context.Model.FindEntityType(typeof(RequestStatusHistory));
        Assert.NotNull(entity);

        var property = entity.FindProperty(nameof(RequestStatusHistory.Status));
        Assert.NotNull(property);

        var converter = property.GetTypeMapping().Converter;
        Assert.NotNull(converter);
        Assert.Equal(typeof(string), converter.ProviderClrType);
    }
}
