using CivicFlow.Application.Services;

namespace CivicFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<RequestWorkflowService>();
        services.AddScoped<ImportValidationService>();
        services.AddSingleton<CivicFlow.Application.Abstractions.IClock, SystemClock>();
        return services;
    }
}
