using Microsoft.AspNetCore.Diagnostics;
using StressTracker5001Server.DTOs.Common;
using System.Text.Json;

namespace StressTracker5001Server.Middleware
{
    /// <summary>
    /// Global exception handler middleware to catch unhandled exceptions and return consistent JSON responses
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred. Request: {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path
            );

            // Determine status code and error message based on exception type
            var (statusCode, errorMessage) = exception switch
            {
                UnauthorizedAccessException => (401, "Unauthorized access"),
                KeyNotFoundException => (404, "Resource not found"),
                InvalidOperationException => (400, exception.Message),
                ArgumentException => (400, exception.Message),
                _ => (500, "An internal server error occurred")
            };

            // Create consistent error response
            var result = ResultDto.CreateFailureResult(errorMessage, statusCode);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                cancellationToken
            );

            return true; // Exception was handled
        }
    }
}
