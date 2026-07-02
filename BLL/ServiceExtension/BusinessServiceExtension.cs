using BLL.Managers;
using BLL.Managers.Authentication;
using BLL.Managers.CloudinaryManager;
using BLL.Managers.EmailService;
using BLL.Managers.UsersDashboard;
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