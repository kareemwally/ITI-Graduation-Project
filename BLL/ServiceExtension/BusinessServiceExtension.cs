using BLL.AI;
using BLL.AI.Abstractions;
using BLL.AI.Providers;
using BLL.Managers;
using BLL.Managers.Authentication;
using BLL.Managers.AiManager;
using BLL.Managers.CloudinaryManager;
using BLL.Managers.Documents;
using BLL.Managers.EmailService;
using BLL.Managers.UsersDashboard;
using BLL.Managers.Verification;
using BLL.Validators.Listings;
using DAL.ServiceExtension;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.ServiceExtension
{
    public static class BusinessServiceExtension
    {
        public static IServiceCollection AddBusinessLogicLayer(
            this IServiceCollection services, IConfiguration configuration)
        {
            // ضيف السطر ده عشان الـ Controller يعرف ينده على الـ PurchaseOfferManager
            services.AddScoped<IPurchaseOfferManager, PurchaseOfferManager>();
            //----------
            services.AddDataAccessLayer(configuration);

            services.AddScoped<ICategoryManager, CategoryManager>();
            services.AddScoped<IListingManager, ListingManager>();

            // Order management
            services.AddScoped<IOfferManager, OfferManager>();
            services.AddScoped<IOrderManager, OrderManager>();

            // Chat
            services.AddScoped<IChatManager, ChatManager>();

            // Notifications
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationManager, NotificationManager>();

            // Contracts & Payments
            services.AddScoped<IContractManager, ContractManager>();

            // Disputes
            services.AddScoped<IDisputeManager, DisputeManager>();

            // Profile
            services.AddScoped<IProfileManager, ProfileManager>();

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

            // Payment service — switch between Simulated and Paymob here
            var usePaymob = configuration.GetValue<bool>("PaymobSettings:UsePaymob");
            if (usePaymob)
                services.AddScoped<IPaymentService, PaymobPaymentService>();
            else
                services.AddScoped<IPaymentService, SimulatedPaymentService>();

            services.AddValidatorsFromAssemblyContaining<CreateListingDtoValidator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}