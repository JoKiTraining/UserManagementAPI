using System.Text.Json;

namespace MiddlewareError
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // Weiter zur nächsten Middleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var result = JsonSerializer.Serialize(new { error = "Internal server error." });
            return context.Response.WriteAsync(result);
        }
    }
}