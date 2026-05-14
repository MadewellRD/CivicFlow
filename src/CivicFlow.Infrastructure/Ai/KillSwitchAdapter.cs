using System.Diagnostics;
using CivicFlow.Application.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivicFlow.Infrastructure.Ai;

/// <summary>
/// Decorator that short-circuits LLM invocations when the kill-switch is
/// engaged. Returns a deterministic failure with zero cost and zero latency
/// recorded as a kill-switch event so audit logs make the cause obvious.
/// Mirrors the ROGUE:OPS non-overridable execution boundary pattern.
/// </summary>
public sealed class KillSwitchAdapter : IModelAdapter
{
    private readonly IModelAdapter _inner;
    private readonly IOptionsMonitor<AiOptions> _options;
    private readonly ILogger<KillSwitchAdapter> _logger;

    public KillSwitchAdapter(IModelAdapter inner, IOptionsMonitor<AiOptions> options, ILogger<KillSwitchAdapter> logger)
    {
        _inner = inner;
        _options = options;
        _logger = logger;
    }

    public Task<ModelResponse<TResult>> InvokeAsync<TResult>(
        ModelRequest<TResult> request,
        CancellationToken cancellationToken) where TResult : class
    {
        if (_options.CurrentValue.KillSwitchEngaged)
        {
            _logger.LogWarning(
                "Kill-switch engaged: rejecting prompt {PromptTemplateId}",
                request.PromptTemplateId);
            var telemetry = new ModelInvocationTelemetry(
                ProviderName: "kill-switch",
                ModelName: "n/a",
                InputTokens: 0,
                OutputTokens: 0,
                EstimatedCostUsd: 0m,
                Latency: TimeSpan.Zero,
                ServedFromKillSwitch: true,
                ServedFromMock: false);
            return Task.FromResult(
                ModelResponse<TResult>.Failure("AI kill-switch is engaged.", telemetry));
        }

        return _inner.InvokeAsync(request, cancellationToken);
    }
}
