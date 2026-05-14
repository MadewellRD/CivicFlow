namespace CivicFlow.Application.Ai;

/// <summary>
/// Configuration for the AI subsystem. Bound from configuration section "Ai".
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>One of: "Anthropic", "Mock". Case-insensitive.</summary>
    public string Provider { get; set; } = "Mock";

    /// <summary>If true, every invocation short-circuits with a KillSwitchEngaged failure. ROGUE:OPS pattern.</summary>
    public bool KillSwitchEngaged { get; set; }

    /// <summary>Anthropic API base URL. Override for proxies.</summary>
    public string AnthropicBaseUrl { get; set; } = "https://api.anthropic.com";

    /// <summary>Anthropic model to invoke.</summary>
    public string AnthropicModel { get; set; } = "claude-haiku-4-5-20251001";

    /// <summary>Anthropic API key. Required when Provider = Anthropic.</summary>
    public string? AnthropicApiKey { get; set; }

    /// <summary>Hard cap on tokens per call so a single prompt cannot run away.</summary>
    public int MaxOutputTokensHardCap { get; set; } = 2048;
}
