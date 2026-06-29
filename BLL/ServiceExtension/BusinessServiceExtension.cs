using BLL.Managers;
using BLL.ServiceExtension;
using BLL.AI;
using BLL.AI.Abstractions;
using BLL.AI.Providers;
using BLL.Managers.AiManager;
using BLL.Managers.AuthenticationManager;
using BLL.Managers.AuthnticationManager;
using BLL.Managers.CloudinaryManager;
using BLL.Managers.Documents;
using BLL.Managers.EmailService;
using BLL.Managers.Verification;
using BLL.Validators;
using DAL.ServiceExtension;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.ServiceExtension
{
    /// <summary>
    /// Composition root for the Business Logic Layer. It also pulls in the Data Access Layer so the
    /// API only needs a single call (<c>AddBusinessLogicLayer</c>) — each layer owns its own wiring.
    /// </summary>
    public static class BusinessServiceExtension
    {
        public static IServiceCollection AddBusinessLogicLayer(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataAccessLayer(configuration);

            services.AddScoped<ICategoryManager, CategoryManager>();
            services.AddScoped<IListingManager, ListingManager>();
            services.AddScoped<IOrderManager, OrderManager>();

            // AI provider abstraction — swap Gemini/Claude/... purely via the "Ai:Provider" config key.
            services.AddHttpClient();
            services.AddScoped<IAiProvider, ItiGatewayAiProvider>();
            services.AddScoped<IAiProvider, GeminiAiProvider>();
            services.AddScoped<IAiProvider, ClaudeAiProvider>();
            services.AddScoped<IAiProviderResolver, AiProviderResolver>();
            services.AddScoped<IDocumentContentFetcher, HttpDocumentContentFetcher>();

            // AI features
            services.AddScoped<IAiSearchService, AiSearchService>();
            services.AddScoped<ISmartSearchManager, SmartSearchManager>();
            services.AddScoped<IVerificationManager, VerificationManager>();
            services.AddScoped<IDocumentManager, DocumentManager>();

            services.AddValidatorsFromAssemblyContaining<CreateListingDtoValidator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            return services;
        }
    }
}