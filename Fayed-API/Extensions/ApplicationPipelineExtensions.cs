using Fayed_API.Middlewares;
using Fayed_API.Hubs;

namespace Fayed_API.Extensions
{
    public static class ApplicationPipelineExtensions
    {
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "منصة فايد API v1");
                    c.RoutePrefix = "swagger";
                    c.DocumentTitle = "منصة فايد - API Documentation";
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<ChatHub>("/hubs/chat");
            app.MapHub<NotificationHub>("/hubs/notifications");

            return app;
        }
    }
}
