using BLL.ServiceExtension;
using Fayed_API.Extensions;
using Serilog;

namespace Fayed_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt",
                               rollingInterval: RollingInterval.Day,
                               retainedFileCountLimit: 30)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddPresentationLayer(builder.Configuration);
            builder.Services.AddBusinessLogicLayer(builder.Configuration);

            var app = builder.Build();

            await app.SeedDataAsync();
            app.ConfigurePipeline();

            app.Run();
        }
    }
}
