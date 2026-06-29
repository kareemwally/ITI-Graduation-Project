using BLL.Managers.AuthnticationManager;
using BLL.Managers.CloudinaryManager;
using BLL.Managers.EmailService;
using BLL.ServiceExtension;
using DAL.Data;
using DAL.Models;
using Fayed_API.CustomeMiddleWares;
using Fayed_API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using System.Threading.Tasks;

namespace Fayed_API
{
    public class Program
    {
        private const string FrontendCorsPolicy = "FrontendCors";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Presentation services.
            builder.Services.AddControllers();

            // Swagger + a JWT "Authorize" button so protected endpoints can be tested from the UI.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste your JWT here (no 'Bearer ' prefix needed)."
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

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

            // JWT bearer authentication — validates the tokens AuthService issues on login.
            // Without this the tokens are never checked and [Authorize] does not work.
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // CORS so the front end can call the API from the browser.
            var frontendUrl = builder.Configuration["FrontendUrl"] ?? "https://fayedplatform.com";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(FrontendCorsPolicy, policy =>
                    policy.WithOrigins(frontendUrl)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });

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
                // تشغيل واجهة السواجر (الشاشة الخضرا اللي إنت متعود عليها)
                app.UseSwagger();
                app.UseSwaggerUI();

                // تشغيل واجهة المطورين الجديدة (Scalar - شغل زميلك)
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.WithTitle("منصة فايد - API Documentation")
                           .WithTheme(ScalarTheme.DeepSpace)
                           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                });
            }

            app.UseHttpsRedirection();

            app.UseCors(FrontendCorsPolicy);

            // Order matters: authentication must run before authorization.
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
