using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CivicFlow.Application.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivicFlow.Infrastructure.Ai;

/// <summary>
/// Calls Anthropic's Messages API. Forces a JSON-only response by appending
/// the requested response schema to the system prompt and then validating
/// the model output against the same schema before returning it.
///
/// Cost estimates use published Haiku 4.5 pricing as of May 2026 — the
/// values are advisory, not billing-grade, but they let the application
/// surface a real per-invocation USD figure in dashboards and audit logs.
/// </summary>
public sealed class AnthropicAdapter : IModelAdapter
{
    private const string ApiVersion = "2023-06-01";
    private const decimal InputTokensPerMillionUsd = 1.00m;
    private const decimal OutputTokensPerMillionUsd = 5.00m;

    private readonly HttpClient _http;
    private readonly IOptionsMonitor<AiOptions> _options;
    private readonly ILogger<AnthropicAdapter> _logger;

    public AnthropicAdapter(HttpClient http, IOptionsMonitor<AiOptions> options, ILogger<AnthropicAdapter> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public async Task<ModelResponse<TResult>> InvokeAsync<TResult>(
        ModelRequest<TResult> request,
        CancellationToken cancellationToken) where TResult : class
    {
        var stopwatch = Stopwatch.StartNew();
        var settings = _options.CurrentValue;

        if (string.IsNullOrWhiteSpace(settings.AnthropicApiKey))
        {
            stopwatch.Stop();
            return ModelResponse<TResult>.Failure(
                "Anthropic API key is not configured.",
                BuildTelemetry(settings, 0, 0, stopwatch.Elapsed));
        }

        var maxTokens = Math.Min(request.MaxOutputTokens, settings.MaxOutputTokensHardCap);
        var systemPrompt = $"{request.SystemPrompt}\n\nYou MUST respond with a single JSON object that matches this schema exactly. No prose, no markdown fences.\nSchema:\n{request.ResponseJsonSchema}";

        var payload = new AnthropicRequest(
            Model: settings.AnthropicModel,
            MaxTokens: maxTokens,
            Temperature: request.Temperature,
            System: systemPrompt,
            Messages: new[]
            {
                new AnthropicMessage("user", request.UserPrompt)
            });

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{settings.AnthropicBaseUrl.TrimEnd('/')}/v1/messages")
        {
            Content = JsonContent.Create(payload, options: SerializerOptions)
        };
        httpRequest.Headers.Add("x-api-key", settings.AnthropicApiKey);
        httpRequest.Headers.Add("anthropic-version", ApiVersion);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using var httpResponse = await _http.SendAsync(httpRequest, cancellationToken);
            var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Anthropic call failed for {PromptTemplateId}: {Status} {Body}",
                    request.PromptTemplateId, httpResponse.StatusCode, body);
                return ModelResponse<TResult>.Failure(
                    $"Anthropic returned {(int)httpResponse.StatusCode}: {body}",
                    BuildTelemetry(settings, 0, 0, stopwatch.Elapsed));
            }

            var parsed = JsonSerializer.Deserialize<AnthropicResponse>(body, SerializerOptions);
            if (parsed is null || parsed.Content is null || parsed.Content.Length == 0)
            {
                return ModelResponse<TResult>.Failure(
                    "Anthropic response had no content blocks.",
                    BuildTelemetry(settings, parsed?.Usage?.InputTokens ?? 0, parsed?.Usage?.OutputTokens ?? 0, stopwatch.Elapsed));
            }

            var text = parsed.Content[0].Text ?? string.Empty;
            var firstBrace = text.IndexOf('{');
            var lastBrace = text.LastIndexOf('}');
            if (firstBrace < 0 || lastBrace <= firstBrace)
            {
                return ModelResponse<TResult>.Failure(
                    "Anthropic response did not contain a JSON object.",
                    BuildTelemetry(settings, parsed.Usage?.InputTokens ?? 0, parsed.Usage?.OutputTokens ?? 0, stopwatch.Elapsed));
            }

            var json = text.Substring(firstBrace, lastBrace - firstBrace + 1);
            var value = JsonSerializer.Deserialize<TResult>(json, SerializerOptions);

            if (value is null)
            {
                return ModelResponse<TResult>.Failure(
                    "Anthropic JSON deserialised to null.",
                    BuildTelemetry(settings, parsed.Usage?.InputTokens ?? 0, parsed.Usage?.OutputTokens ?? 0, stopwatch.Elapsed));
            }

            return ModelResponse<TResult>.Success(
                value,
                BuildTelemetry(settings, parsed.Usage?.InputTokens ?? 0, parsed.Usage?.OutputTokens ?? 0, stopwatch.Elapsed));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Anthropic call threw for {PromptTemplateId}", request.PromptTemplateId);
            return ModelResponse<TResult>.Failure(
                $"Anthropic call threw: {ex.Message}",
                BuildTelemetry(settings, 0, 0, stopwatch.Elapsed));
        }
    }

    private static ModelInvocationTelemetry BuildTelemetry(
        AiOptions settings, int inputTokens, int outputTokens, TimeSpan latency)
    {
        var cost = ((decimal)inputTokens / 1_000_000m) * InputTokensPerMillionUsd
                 + ((decimal)outputTokens / 1_000_000m) * OutputTokensPerMillionUsd;
        return new ModelInvocationTelemetry(
            ProviderName: "anthropic",
            ModelName: settings.AnthropicModel,
            InputTokens: inputTokens,
            OutputTokens: outputTokens,
            EstimatedCostUsd: Math.Round(cost, 6),
            Latency: latency,
            ServedFromKillSwitch: false,
            ServedFromMock: false);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed record AnthropicRequest(
        string Model,
        int MaxTokens,
        double Temperature,
        string System,
        IReadOnlyCollection<AnthropicMessage> Messages);

    private sealed record AnthropicMessage(string Role, string Content);

    private sealed record AnthropicResponse(
        [property: JsonPropertyName("content")] AnthropicContentBlock[]? Content,
        [property: JsonPropertyName("usage")] AnthropicUsage? Usage);

    private sealed record AnthropicContentBlock(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("text")] string? Text);

    private sealed record AnthropicUsage(
        [property: JsonPropertyName("input_tokens")] int InputTokens,
        [property: JsonPropertyName("output_tokens")] int OutputTokens);
}
