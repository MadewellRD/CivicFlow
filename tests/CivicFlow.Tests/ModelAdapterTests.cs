using CivicFlow.Application.Ai;
using CivicFlow.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CivicFlow.Tests;

/// <summary>
/// Locks in the contract of the AI invocation pipeline:
/// - kill-switch short-circuits before any model is called;
/// - mock adapter returns schema-shaped JSON for every registered template;
/// - unknown templates return a controlled failure instead of an exception.
/// </summary>
public sealed class ModelAdapterTests
{
    [Fact]
    public async Task KillSwitchShortCircuitsBeforeInner()
    {
        var monitor = MakeMonitor(new AiOptions { KillSwitchEngaged = true });
        var inner = new ThrowingAdapter();
        var adapter = new KillSwitchAdapter(inner, monitor, NullLogger<KillSwitchAdapter>.Instance);

        var response = await adapter.InvokeAsync(MakeRequest(), CancellationToken.None);

        Assert.False(response.Succeeded);
        Assert.True(response.Telemetry.ServedFromKillSwitch);
        Assert.Equal(0m, response.Telemetry.EstimatedCostUsd);
        Assert.False(inner.WasCalled);
    }

    [Fact]
    public async Task MockReturnsParsedImportExplainerPayload()
    {
        var mock = new DeterministicMockAdapter(NullLogger<DeterministicMockAdapter>.Instance);
        var request = new ModelRequest<ImportExplainerPayload>(
            PromptTemplateId: "import-error-explainer",
            SystemPrompt: "stub",
            UserPrompt: "stub",
            ResponseJsonSchema: "{}");

        var response = await mock.InvokeAsync(request, CancellationToken.None);

        Assert.True(response.Succeeded);
        Assert.NotNull(response.Value);
        Assert.NotEmpty(response.Value!.FieldGuidance);
        Assert.True(response.Telemetry.ServedFromMock);
    }

    [Fact]
    public async Task MockFailsCleanlyForUnknownTemplate()
    {
        var mock = new DeterministicMockAdapter(NullLogger<DeterministicMockAdapter>.Instance);
        var request = new ModelRequest<ImportExplainerPayload>(
            PromptTemplateId: "no-such-template",
            SystemPrompt: "stub",
            UserPrompt: "stub",
            ResponseJsonSchema: "{}");

        var response = await mock.InvokeAsync(request, CancellationToken.None);

        Assert.False(response.Succeeded);
        Assert.Contains("no-such-template", response.FailureReason);
    }

    [Fact]
    public void PromptSchemaRegistryReturnsSchemaForKnownTemplate()
    {
        var registry = new PromptSchemaRegistry();
        var schema = registry.GetSchema("triage-router");
        Assert.Contains("recommendedQueue", schema);
    }

    [Fact]
    public void PromptSchemaRegistryThrowsForUnknownTemplate()
    {
        var registry = new PromptSchemaRegistry();
        Assert.Throws<InvalidOperationException>(() => registry.GetSchema("does-not-exist"));
    }

    private static ModelRequest<ImportExplainerPayload> MakeRequest() =>
        new("import-error-explainer", "sys", "user", "{}");

    private static IOptionsMonitor<AiOptions> MakeMonitor(AiOptions value)
    {
        return new StaticOptions(value);
    }

    private sealed class StaticOptions : IOptionsMonitor<AiOptions>
    {
        public StaticOptions(AiOptions value) { CurrentValue = value; }
        public AiOptions CurrentValue { get; }
        public AiOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<AiOptions, string?> listener) => null;
    }

    private sealed class ThrowingAdapter : IModelAdapter
    {
        public bool WasCalled { get; private set; }
        public Task<ModelResponse<TResult>> InvokeAsync<TResult>(ModelRequest<TResult> request, CancellationToken cancellationToken) where TResult : class
        {
            WasCalled = true;
            throw new InvalidOperationException("Inner should not have been called when the kill-switch is engaged.");
        }
    }

    public sealed class ImportExplainerPayload
    {
        public int RowNumber { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<FieldGuidance> FieldGuidance { get; set; } = new();
        public string AgencyMessage { get; set; } = string.Empty;
        public string Confidence { get; set; } = string.Empty;
    }

    public sealed class FieldGuidance
    {
        public string Field { get; set; } = string.Empty;
        public string Problem { get; set; } = string.Empty;
        public string Fix { get; set; } = string.Empty;
    }
}
