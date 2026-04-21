using DataAccess.Constants;
using Microsoft.AspNetCore.Http;

namespace DataAccess.ResponseModel
{
    public class BaseResponseModel<T> where T : class
    {
        public T? Data { get; set; }
        public object? AdditionalData { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public string Code { get; set; }

        public BaseResponseModel(int statusCode, string code, T? data, object? additionalData = null, string? message = null)
        {
            this.StatusCode = statusCode;
            this.Code = code;
            this.Data = data;
            this.AdditionalData = additionalData;
            this.Message = message;
        }

        public BaseResponseModel(int statusCode, string code, string? message)
        {
            this.StatusCode = statusCode;
            this.Code = code;
            this.Message = message;
        }

        public static BaseResponseModel<T> OkResponseModel(T data, string code = ResponseCodeConstants.SUCCESS, string? message = null, object? additionalData = null)
        {
            return new BaseResponseModel<T>(StatusCodes.Status200OK, code, data, additionalData, message);
        }

		public static BaseResponseModel<T> UnauthorizedResponseModel(T data, string code = ResponseCodeConstants.UNAUTHORIZED, string? message = null, object? additionalData = null)
		{
			return new BaseResponseModel<T>(StatusCodes.Status401Unauthorized, code, data, additionalData, message);
		}

		public static BaseResponseModel<T> ForbiddenResponseModel(T data, string code = ResponseCodeConstants.FORBIDDEN, string? message = null, object? additionalData = null)
		{
			return new BaseResponseModel<T>(StatusCodes.Status403Forbidden, code, data, additionalData, message);
		}

		public static BaseResponseModel<T> NotFoundResponseModel(T? data, string code = ResponseCodeConstants.NOT_FOUND, string? message = null, object? additionalData = null)
        {
            return new BaseResponseModel<T>(StatusCodes.Status404NotFound, code, data, additionalData, message);
        }

        public static BaseResponseModel<T> BadRequestResponseModel(T? data, string code = ResponseCodeConstants.BAD_REQUEST, string? message = null, object? additionalData = null)
        {
            return new BaseResponseModel<T>(StatusCodes.Status400BadRequest, code, data, additionalData, message);
        }

        public static BaseResponseModel<T> InternalErrorResponseModel(T? data, string code = ResponseCodeConstants.INTERNAL_SERVER_ERROR, string? message = null, object? additionalData = null)
        {
            return new BaseResponseModel<T>(StatusCodes.Status500InternalServerError, code, data, additionalData, message);
        }
    }

    public class BaseResponseModel : BaseResponseModel<object>
    {
        public BaseResponseModel(int statusCode, string code, object? data, object? additionalData = null, string? message = null) : base(statusCode, code, data, additionalData, message)
        {
        }

        public BaseResponseModel(int statusCode, string code, string? message) : base(statusCode, code, message)
        {
        }
    }
}
