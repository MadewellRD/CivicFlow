using CivicFlow.Application.Platform;
using CivicFlow.Application.Services;

namespace CivicFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<RequestWorkflowService>();
        services.AddScoped<ImportValidationService>();
        services.AddScoped<ImportErrorExplainerService>();
        services.AddScoped<TriageRouterService>();
        services.AddScoped<StatsService>();
        services.AddScoped<IBusinessRule, OversightThresholdBusinessRule>();
        services.AddScoped<IBusinessRule, LegacyIntegrationTagBusinessRule>();
        services.AddScoped<BusinessRuleEngine>();
        services.AddSingleton<UiPolicyCatalog>();
        services.AddSingleton<ITransformMap, BudgetImportTransformMap>();
        services.AddSingleton<CivicFlow.Application.Abstractions.IClock, SystemClock>();
        return services;
    }
}
