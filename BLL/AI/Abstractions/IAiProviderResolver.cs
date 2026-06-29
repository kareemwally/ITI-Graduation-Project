namespace BLL.AI.Abstractions
{
    /// <summary>
    /// Resolves the AI provider that is currently active according to configuration.
    /// Lets callers stay decoupled from any specific vendor.
    /// </summary>
    public interface IAiProviderResolver
    {
        /// <summary>The provider selected by <c>Ai:Provider</c> (falls back to the first registered one).</summary>
        IAiProvider GetActiveProvider();
    }
}
