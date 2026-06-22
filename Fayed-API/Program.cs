using BLL.ServiceExtension;

namespace Fayed_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Presentation services.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Wire up the application layers (BLL pulls in the DAL).
            // This is the single composition root for IoC — each layer registers its own services.
            builder.Services.AddBusinessLogicLayer(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
