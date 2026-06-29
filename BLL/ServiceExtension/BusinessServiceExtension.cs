using BLL.Managers;
using BLL.ServiceExtension; // السطر ده ضفناه عشان يشوف فولدر الـ Services
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
            // Bring in the DAL (DbContext, repositories, unit of work).
            services.AddDataAccessLayer(configuration);

            // Managers (business services) — depend on abstractions only.
            services.AddScoped<ICategoryManager, CategoryManager>();
            services.AddScoped<IListingManager, ListingManager>();
            services.AddScoped<IOrderManager, OrderManager>();

            // AI Service & HttpClient
            services.AddHttpClient();
            services.AddScoped<IAiSearchService, AiSearchService>();

            // FluentValidation validators.
            services.AddValidatorsFromAssemblyContaining<CreateListingDtoValidator>();

            return services;
        }
    }
}