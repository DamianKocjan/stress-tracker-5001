namespace StressTracker5001Server.DTOs.Common
{
    public class ResultDto<T>
    {
        public required bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string>? Errors { get; set; }
        public int? StatusCode { get; set; }

        public static ResultDto<T> CreateSuccessResult(T data, int statusCode = 200)
        {
            return new ResultDto<T>
            {
                Success = true,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ResultDto<T> CreateFailureResult(string errorMessage, int statusCode = 400)
        {
            return new ResultDto<T>
            {
                Success = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            };
        }

        public static ResultDto<T> CreateFailureResult(List<string> errors, int statusCode = 400)
        {
            return new ResultDto<T>
            {
                Success = false,
                Errors = errors,
                ErrorMessage = errors.FirstOrDefault(),
                StatusCode = statusCode
            };
        }

        public static ResultDto<T> NotFound(string message = "Resource not found")
        {
            return new ResultDto<T>
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = 404
            };
        }

        public static ResultDto<T> Unauthorized(string message = "Unauthorized access")
        {
            return new ResultDto<T>
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = 401
            };
        }

        public static ResultDto<T> Forbidden(string message = "Forbidden")
        {
            return new ResultDto<T>
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = 403
            };
        }

        public static ResultDto<T> ValidationError(List<string> errors)
        {
            return CreateFailureResult(errors, 422);
        }
    }

    // Non-generic version for operations that don't return data
    public class ResultDto : ResultDto<object>
    {
        public static ResultDto CreateSuccess(int statusCode = 200)
        {
            return new ResultDto
            {
                Success = true,
                StatusCode = statusCode
            };
        }

        public static new ResultDto CreateFailureResult(string errorMessage, int statusCode = 400)
        {
            return new ResultDto
            {
                Success = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            };
        }

        public static new ResultDto NotFound(string message = "Resource not found")
        {
            return new ResultDto
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = 404
            };
        }

        public static new ResultDto Unauthorized(string message = "Unauthorized access")
        {
            return new ResultDto
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = 401
            };
        }

        public static new ResultDto Forbidden(string message = "Forbidden")
        {
            return new ResultDto
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = 403
            };
        }
    }
}
