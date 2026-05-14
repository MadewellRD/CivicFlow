using System.Net;
using System.Text;
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

    [Fact]
    public async Task AnthropicDeserializesCamelCaseModelPayload()
    {
        var handler = new RecordingAnthropicHandler("""
        {
          "content": [
            {
              "type": "text",
              "text": "{\"recommendedQueue\":\"Budget Operations\",\"humanReviewRequired\":true}"
            }
          ],
          "usage": {
            "input_tokens": 20,
            "output_tokens": 5
          }
        }
        """);
        using var http = new HttpClient(handler);
        var adapter = new AnthropicAdapter(
            http,
            MakeMonitor(new AiOptions
            {
                AnthropicApiKey = "test-key",
                AnthropicModel = "claude-haiku-4-5-20251001"
            }),
            NullLogger<AnthropicAdapter>.Instance);

        var response = await adapter.InvokeAsync(
            new ModelRequest<AnthropicTriagePayload>("triage-router", "system", "user", "{}"),
            CancellationToken.None);

        Assert.True(response.Succeeded, response.FailureReason);
        Assert.Equal("Budget Operations", response.Value!.RecommendedQueue);
        Assert.True(response.Value.HumanReviewRequired);
        Assert.Equal("claude-haiku-4-5-20251001", response.Telemetry.ModelName);
        Assert.Contains("\"max_tokens\"", handler.RequestBody);
        Assert.DoesNotContain("\"MaxTokens\"", handler.RequestBody);
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

    public sealed class AnthropicTriagePayload
    {
        public string RecommendedQueue { get; set; } = string.Empty;
        public bool HumanReviewRequired { get; set; }
    }

    private sealed class RecordingAnthropicHandler : HttpMessageHandler
    {
        private readonly string _responseBody;

        public RecordingAnthropicHandler(string responseBody)
        {
            _responseBody = responseBody;
        }

        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}
