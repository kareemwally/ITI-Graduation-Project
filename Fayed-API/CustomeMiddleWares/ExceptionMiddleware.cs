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

                // حماية إضافية: لو الـ Response بدأ يتبعت للعميل بالفعل، ما ينفعش نعدل فيه
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the exception middleware will not execute.");
                    throw;
                }

                context.Response.ContentType = "application/json";
                string json;

                string message;
                List<string> errors;

                if (ex is BaseBusinessException businessEx)
                {
                    context.Response.StatusCode = (int)businessEx.StatusCode;
                    message = businessEx.Message;

                    // التعديل الأول: استبدال [] بـ new List<string> صريحة
                    errors = businessEx.Errors ?? new List<string>();
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    message = _env.IsDevelopment() ? ex.Message : "حدث خطأ غير متوقع في الخادم، يرجى المحاولة لاحقاً.";

                    // التعديل الثاني: استبدال [] عشان الـ Serialization ما يضربش
                    errors = _env.IsDevelopment() && !string.IsNullOrEmpty(ex.StackTrace)
                        ? new List<string> { ex.StackTrace }
                        : new List<string>();
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