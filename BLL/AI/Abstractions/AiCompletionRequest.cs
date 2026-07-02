namespace BLL.AI.Abstractions
{
    /// <summary>
    /// Provider-neutral completion request. Every concrete <see cref="IAiProvider"/> translates this
    /// into its own wire format (Gemini contents, Anthropic messages, ...).
    /// </summary>
    public class AiCompletionRequest
    {
        /// <summary>High-level instructions / persona for the model.</summary>
        public string SystemPrompt { get; set; } = string.Empty;

        /// <summary>The user-facing prompt or task input.</summary>
        public string UserPrompt { get; set; } = string.Empty;

        /// <summary>Optional binary inputs (KYB documents) for multimodal extraction.</summary>
        public IReadOnlyList<AiAttachment> Attachments { get; set; } = Array.Empty<AiAttachment>();

        /// <summary>Hint that the caller expects a raw JSON object back (no prose, no markdown fences).</summary>
        public bool ExpectJson { get; set; }

        public int MaxOutputTokens { get; set; } = 1024;

        /// <summary>
        /// Optional per-call model id. When set it overrides the provider's configured default —
        /// e.g. a cheap fast model for smart search vs a stronger one for document verification,
        /// both through the same gateway.
        /// </summary>
        public string? ModelOverride { get; set; }
    }
}
