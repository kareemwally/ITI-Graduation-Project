using DAL.Data;
using DAL.Repos;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.ServiceExtension
{
    /// <summary>
    /// Composition root for the Data Access Layer. The API project calls this so it never has to
    /// reference EF Core types directly — the DAL owns its own wiring (IoC).
    /// </summary>
    public static class DataAccessServiceExtension
    {
        public static IServiceCollection AddDataAccessLayer(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FayedDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly(typeof(FayedDbContext).Assembly.FullName)));

            // Open generic registration: one line covers every entity repository (OCP).
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();


            return services;
        }
    }
}
