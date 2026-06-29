using BLL.AI.Abstractions;
using Microsoft.Extensions.Configuration;

namespace BLL.AI
{
    /// <summary>
    /// Default resolver. Reads <c>Ai:Provider</c> and returns the matching registered provider,
    /// falling back to the first one if the configured name is missing or unknown.
    /// </summary>
    public class AiProviderResolver : IAiProviderResolver
    {
        private readonly IReadOnlyList<IAiProvider> _providers;
        private readonly string _activeName;

        public AiProviderResolver(IEnumerable<IAiProvider> providers, IConfiguration configuration)
        {
            _providers = providers.ToList();
            _activeName = configuration["Ai:Provider"] ?? "Gemini";
        }

        public IAiProvider GetActiveProvider()
        {
            if (_providers.Count == 0)
                throw new InvalidOperationException("No AI providers are registered.");

            return _providers.FirstOrDefault(
                       p => p.Name.Equals(_activeName, StringComparison.OrdinalIgnoreCase))
                   ?? _providers[0];
        }
    }
}
