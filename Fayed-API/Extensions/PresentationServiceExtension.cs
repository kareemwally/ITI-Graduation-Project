using BLL.DTOs.Common;
using BLL.Settings;
using DAL.Data;
using DAL.Models;
using Fayed_API.Middlewares;
using Fayed_API.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;

namespace Fayed_API.Extensions
{
    public static class PresentationServiceExtension
    {
        public static IServiceCollection AddPresentationLayer(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddFluentValidationAutoValidation();

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .ToList();

                        var response = BaseResponse.Failure("فشل التحقق من صحة البيانات.", errors, 400);
                        return new BadRequestObjectResult(response);
                    };
                });

            services.AddSignalR();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "منصة فايد B2B - API Documentation",
                    Version = "v1",
                    Description = "دليل المطورين لربط الفرونت إند بنظام العقود والدفع والنزاعات"
                });

                c.IncludeXmlComments(Assembly.GetExecutingAssembly());

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "أدخل التوكن الخاص بك بالصيغة: Bearer {token}"
                });

                c.AddSecurityRequirement((document) => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            services.AddHostedService<OrderCompletionBackgroundService>();

            // ===== Strongly-typed Options with startup validation =====
            services.AddOptions<GeneralSettings>()
                .Bind(configuration.GetSection("GeneralSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection("JwtSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<EmailSettings>()
                .Bind(configuration.GetSection("EmailSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<CloudinarySettings>()
                .Bind(configuration.GetSection("Cloudinary"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ContractSettings>()
                .Bind(configuration.GetSection("ContractSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<PaymobSettings>()
                .Bind(configuration.GetSection("PaymobSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // ===== Identity =====
            services.AddIdentity<User, IdentityRole<int>>(options =>
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

            services.AddScoped<IAuthorizationHandler, VerifiedFactoryHandler>();
            services.AddScoped<IAuthorizationHandler, AdminOnlyHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("VerifiedFactoryOnly", policy =>
                    policy.Requirements.Add(new VerifiedFactoryRequirement()));
                options.AddPolicy("AdminOnly", policy =>
                    policy.Requirements.Add(new AdminOnlyRequirement()));
            });

            var jwtSettings = configuration.GetSection("JwtSettings");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs/chat") || path.StartsWithSegments("/hubs/notifications")))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}
