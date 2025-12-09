using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.Common;
using StressTracker5001Server.DTOs.Common;

namespace StressTracker5001Server.Extensions
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts a Result<T> from the service layer to a ResultDto<TDto> for API responses
        /// </summary>
        public static ResultDto<TDto> ToResultDto<T, TDto>(this Result<T> result, Func<T, TDto> mapper)
        {
            if (result.IsSuccess && result.Value != null)
            {
                return ResultDto<TDto>.CreateSuccessResult(mapper(result.Value), result.StatusCode);
            }

            return result.StatusCode switch
            {
                404 => ResultDto<TDto>.NotFound(result.Error ?? "Resource not found"),
                403 => ResultDto<TDto>.Forbidden(result.Error ?? "Forbidden"),
                401 => ResultDto<TDto>.Unauthorized(result.Error ?? "Unauthorized access"),
                422 => ResultDto<TDto>.ValidationError(new List<string> { result.Error ?? "Validation error" }),
                _ => ResultDto<TDto>.CreateFailureResult(result.Error ?? "An error occurred", result.StatusCode)
            };
        }

        /// <summary>
        /// Converts a Result<T> to a ResultDto<T> (when T is already a DTO)
        /// </summary>
        public static ResultDto<T> ToResultDto<T>(this Result<T> result)
        {
            if (result.IsSuccess && result.Value != null)
            {
                return ResultDto<T>.CreateSuccessResult(result.Value, result.StatusCode);
            }

            return result.StatusCode switch
            {
                404 => ResultDto<T>.NotFound(result.Error ?? "Resource not found"),
                403 => ResultDto<T>.Forbidden(result.Error ?? "Forbidden"),
                401 => ResultDto<T>.Unauthorized(result.Error ?? "Unauthorized access"),
                422 => ResultDto<T>.ValidationError(new List<string> { result.Error ?? "Validation error" }),
                _ => ResultDto<T>.CreateFailureResult(result.Error ?? "An error occurred", result.StatusCode)
            };
        }

        /// <summary>
        /// Converts a Result<T> to a ResultDto<List<TDto>> for collections
        /// </summary>
        public static ResultDto<List<TDto>> ToResultDto<T, TDto>(this Result<List<T>> result, Func<T, TDto> mapper)
        {
            if (result.IsSuccess && result.Value != null)
            {
                var dtoList = result.Value.Select(mapper).ToList();
                return ResultDto<List<TDto>>.CreateSuccessResult(dtoList, result.StatusCode);
            }

            return result.StatusCode switch
            {
                404 => ResultDto<List<TDto>>.NotFound(result.Error ?? "Resource not found"),
                403 => ResultDto<List<TDto>>.Forbidden(result.Error ?? "Forbidden"),
                401 => ResultDto<List<TDto>>.Unauthorized(result.Error ?? "Unauthorized access"),
                422 => ResultDto<List<TDto>>.ValidationError(new List<string> { result.Error ?? "Validation error" }),
                _ => ResultDto<List<TDto>>.CreateFailureResult(result.Error ?? "An error occurred", result.StatusCode)
            };
        }

        /// <summary>
        /// Converts a Result<T> directly to IActionResult with proper HTTP status codes
        /// </summary>
        public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, object> mapper)
        {
            if (result.IsSuccess && result.Value != null)
            {
                var dto = mapper(result.Value);
                var resultDto = ResultDto<object>.CreateSuccessResult(dto, result.StatusCode);
                return new ObjectResult(resultDto) { StatusCode = result.StatusCode };
            }

            var errorDto = result.StatusCode switch
            {
                404 => ResultDto<object>.NotFound(result.Error ?? "Resource not found"),
                403 => ResultDto<object>.Forbidden(result.Error ?? "Forbidden"),
                401 => ResultDto<object>.Unauthorized(result.Error ?? "Unauthorized access"),
                422 => ResultDto<object>.ValidationError(new List<string> { result.Error ?? "Validation error" }),
                _ => ResultDto<object>.CreateFailureResult(result.Error ?? "An error occurred", result.StatusCode)
            };

            return new ObjectResult(errorDto) { StatusCode = result.StatusCode };
        }

        /// <summary>
        /// Converts a Result<T> directly to IActionResult (when T is already suitable for response)
        /// </summary>
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess && result.Value != null)
            {
                var resultDto = ResultDto<T>.CreateSuccessResult(result.Value, result.StatusCode);
                return new ObjectResult(resultDto) { StatusCode = result.StatusCode };
            }

            var errorDto = result.StatusCode switch
            {
                404 => ResultDto<T>.NotFound(result.Error ?? "Resource not found"),
                403 => ResultDto<T>.Forbidden(result.Error ?? "Forbidden"),
                401 => ResultDto<T>.Unauthorized(result.Error ?? "Unauthorized access"),
                422 => ResultDto<T>.ValidationError(new List<string> { result.Error ?? "Validation error" }),
                _ => ResultDto<T>.CreateFailureResult(result.Error ?? "An error occurred", result.StatusCode)
            };

            return new ObjectResult(errorDto) { StatusCode = result.StatusCode };
        }

        /// <summary>
        /// Converts a non-generic Result to IActionResult for operations that don't return data
        /// </summary>
        public static IActionResult ToActionResult(this Result<object> result)
        {
            if (result.IsSuccess)
            {
                var resultDto = ResultDto.CreateSuccess(result.StatusCode);
                return new ObjectResult(resultDto) { StatusCode = result.StatusCode };
            }

            var errorDto = result.StatusCode switch
            {
                404 => ResultDto.NotFound(result.Error ?? "Resource not found"),
                403 => ResultDto.Forbidden(result.Error ?? "Forbidden"),
                401 => ResultDto.Unauthorized(result.Error ?? "Unauthorized access"),
                422 => ResultDto.ValidationError(new List<string> { result.Error ?? "Validation error" }),
                _ => ResultDto.CreateFailureResult(result.Error ?? "An error occurred", result.StatusCode)
            };

            return new ObjectResult(errorDto) { StatusCode = result.StatusCode };
        }

        /// <summary>
        /// Converts a Result<List<T>> to IActionResult for collection responses
        /// </summary>
        public static IActionResult ToActionResult<T>(this Result<List<T>> result, Func<T, object> mapper)
        {
            if (result.IsSuccess && result.Value != null)
            {
                var dtoList = result.Value.Select(mapper).ToList();
                var resultDto = ResultDto<List<object>>.CreateSuccessResult(dtoList, result.StatusCode);
                return new ObjectResult(resultDto) { StatusCode = result.StatusCode };
            }

            var errorDto = result.StatusCode switch
            {
                404 => ResultDto<List<object>>.NotFound(result.Error ?? "Resource not found"),
                403 => ResultDto<List<object>>.Forbidden(result.Error ?? "Forbidden"),
                401 => ResultDto<List<object>>.Unauthorized(result.Error ?? "Unauthorized access"),
                422 => ResultDto<List<object>>.ValidationError(new List<string> { result.Error ?? "Validation error" }),
                _ => ResultDto<List<object>>.CreateFailureResult(result.Error ?? "An error occurred", result.StatusCode)
            };

            return new ObjectResult(errorDto) { StatusCode = result.StatusCode };
        }
    }
}
