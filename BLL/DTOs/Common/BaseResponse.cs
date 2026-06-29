using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Common
{
    public class BaseResponse
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = null!;
        public List<string>? Errors { get; set; }

        // دالة مساعدة لحالة النجاح بدون داتا (مثلاً: تم التسجيل بنجاح)
        public static BaseResponse Success(string message = "Operation completed successfully.", int statusCode = 200)
        {
            return new BaseResponse
            {
                StatusCode = statusCode,
                IsSuccess = true,
                Message = message,
                Errors = null
            };
        }

        // دالة مساعدة لحالة الفشل العام (Bad Request / Validation)
        public static BaseResponse Failure(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new BaseResponse
            {
                StatusCode = statusCode,
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    public class BaseResponse<T> : BaseResponse
    {
        public T? Data { get; set; }

        // دالة مساعدة للنجاح مع إرجاع داتا
        public static BaseResponse<T> Success(T data, string message = "Operation completed successfully.", int statusCode = 200)
        {
            return new BaseResponse<T>
            {
                StatusCode = statusCode,
                IsSuccess = true,
                Message = message,
                Data = data,
                Errors = null
            };
        }

        // بنورث دالة الفشل برضه عشان لو السيرفس اللي بترجع داتا ضربت في النص
        public static new BaseResponse<T> Failure(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new BaseResponse<T>
            {
                StatusCode = statusCode,
                IsSuccess = false,
                Message = message,
                Data = default,
                Errors = errors ?? new List<string>()
            };
        }
    }



}
