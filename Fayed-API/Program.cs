using BLL.Managers.AuthnticationManager;
using BLL.Managers.CloudinaryManager;
using BLL.Managers.EmailService;
using BLL.ServiceExtension;
using DAL.Data;
using DAL.Models;
using Fayed_API.CustomeMiddleWares;
using Fayed_API.Extensions;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using Serilog;
using System.Threading.Tasks;

namespace Fayed_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Presentation services.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Wire up the application layers (BLL pulls in the DAL).
            // This is the single composition root for IoC — each layer registers its own services.
            builder.Services.AddBusinessLogicLayer(builder.Configuration);

            //add identity services
            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<FayedDbContext>()
            .AddDefaultTokenProviders();


            //seriallog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", 
                               rollingInterval: RollingInterval.Day,
                               retainedFileCountLimit: 30) 
                .CreateLogger();

            builder.Host.UseSerilog();

         


            


            var app = builder.Build();

             await app.SeedDataAsync();
            app.UseMiddleware<ExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.WithTitle("منصة فايد - API Documentation")
                           .WithTheme(ScalarTheme.DeepSpace)
                           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
