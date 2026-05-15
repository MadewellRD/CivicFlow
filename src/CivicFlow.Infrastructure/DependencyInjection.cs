using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Ai;
using CivicFlow.Infrastructure.Persistence;
using CivicFlow.Infrastructure.Repositories;
using CivicFlow.Infrastructure.Seeding;
using CivicFlow.Infrastructure.Ai;
using CivicFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CivicFlow")
            ?? throw new InvalidOperationException("Connection string 'CivicFlow' is required.");

        services.AddDbContext<CivicFlowDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IImportRepository, ImportRepository>();
        services.AddScoped<IReferenceDataProvider, EfReferenceDataProvider>();
        services.AddScoped<IAuditWriter, EfAuditWriter>();
        services.AddScoped<IAuditQueryService, EfAuditQueryService>();
        services.AddScoped<INotificationService, EfNotificationService>();
        services.AddCivicFlowAi(configuration);
        services.AddScoped<DemoDataSeeder>();
        return services;
    }
}
