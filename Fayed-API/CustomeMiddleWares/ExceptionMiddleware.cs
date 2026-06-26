using BLL.DTOs.Common;
using DAL.Models.ExceptionModels; 
using System.Net;
using System.Text.Json;

namespace Fayed_API.CustomeMiddleWares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.StatusCode == (int)HttpStatusCode.NotFound && !context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";

                    var notFoundResponse = BaseResponse.Failure(
                        message: "عذراً، هذا الرابط غير موجود في النظام.",
                        errors: null,
                        statusCode: 404
                    );

                    var json = JsonSerializer.Serialize(notFoundResponse, GetJsonOptions());
                    await context.Response.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.ContentType = "application/json";
                string json;

                string message;
                List<string> errors;

                if (ex is BaseBusinessException businessEx)
                {
                    context.Response.StatusCode = (int)businessEx.StatusCode;
                    message = businessEx.Message;
                    errors = businessEx.Errors ?? [];
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    message = _env.IsDevelopment() ? ex.Message : "حدث خطأ غير متوقع في الخادم، يرجى المحاولة لاحقاً.";
                    errors = _env.IsDevelopment() ? [ex.StackTrace?.ToString() ?? ""] : [];
                }

                var response = BaseResponse.Failure(message, errors, context.Response.StatusCode);
                json = JsonSerializer.Serialize(response, GetJsonOptions());

                await context.Response.WriteAsync(json);
            }
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }
}