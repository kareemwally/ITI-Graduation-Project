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

            // 1. تسجيل خدمات السواجر هنا بدل الـ OpenApi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Wire up the application layers (BLL pulls in the DAL).
            // This is the single composition root for IoC — each layer registers its own services.
            builder.Services.AddBusinessLogicLayer(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // 2. تشغيل واجهة السواجر الخضرا
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}