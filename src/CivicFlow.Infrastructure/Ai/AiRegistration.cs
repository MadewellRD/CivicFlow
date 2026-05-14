using CivicFlow.Application.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFlow.Infrastructure.Ai;

public static class AiRegistration
{
    /// <summary>
    /// Wires the IModelAdapter pipeline. Provider selection is config-driven.
    /// Decorators (kill-switch) wrap the real provider so a single config
    /// flag can disable AI without touching application code.
    /// </summary>
    public static IServiceCollection AddCivicFlowAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.AddSingleton<IPromptSchemaRegistry, PromptSchemaRegistry>();

        var providerName = configuration[$"{AiOptions.SectionName}:Provider"] ?? "Mock";

        if (string.Equals(providerName, "Anthropic", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<AnthropicAdapter>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(45);
            });
            services.AddSingleton<IModelAdapter>(sp =>
            {
                var inner = sp.GetRequiredService<AnthropicAdapter>();
                return new KillSwitchAdapter(
                    inner,
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<AiOptions>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<KillSwitchAdapter>>());
            });
        }
        else
        {
            services.AddSingleton<DeterministicMockAdapter>();
            services.AddSingleton<IModelAdapter>(sp =>
            {
                var inner = sp.GetRequiredService<DeterministicMockAdapter>();
                return new KillSwitchAdapter(
                    inner,
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<AiOptions>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<KillSwitchAdapter>>());
            });
        }

        return services;
    }
}
