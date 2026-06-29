namespace BLL.AI.Abstractions
{
    /// <summary>
    /// Abstraction over a Large Language Model provider. Swapping Gemini for Claude (or any future
    /// provider) is a matter of adding an implementation and flipping <c>Ai:Provider</c> in config —
    /// no business code changes (Open/Closed + Dependency Inversion).
    /// </summary>
    public interface IAiProvider
    {
        /// <summary>Stable key used to select this provider from configuration (e.g. "Gemini", "Claude").</summary>
        string Name { get; }

        /// <summary>The concrete model identifier this provider is configured to call (for audit/logging).</summary>
        string ModelVersion { get; }

        /// <summary>Sends a completion request and returns the model's raw text response.</summary>
        Task<string> CompleteAsync(AiCompletionRequest request, CancellationToken cancellationToken = default);
    }
}
