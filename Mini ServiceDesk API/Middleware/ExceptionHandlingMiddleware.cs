using System.Text.Json;
using Mini_ServiceDesk_API.Models;

namespace Mini_ServiceDesk_API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.Items["TraceId"]?.ToString() ?? Guid.NewGuid().ToString("N");

                _logger.LogError(ex, "Unhandled error traceId={TraceId}", traceId);

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new ErrorResponse
                {
                    Error = new ErrorInfo
                    {
                        Code = "INTERNAL_ERROR",
                        Message = "Something went wrong",
                        Details = [],
                        TraceId = traceId
                    }
                };

                var json = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(json);
            }
        }
    }
}