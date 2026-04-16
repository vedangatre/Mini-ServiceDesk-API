using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mini_ServiceDesk_API.Models;

namespace Mini_ServiceDesk_API.Filters
{
    public class ErrorResponseFilter : IAsyncResultFilter
    {
        private readonly ILogger<ErrorResponseFilter> _logger;

        public ErrorResponseFilter(ILogger<ErrorResponseFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var result = context.Result;
            int? statusCode = result switch
            {
                ObjectResult objectResult => objectResult.StatusCode ?? StatusCodes.Status200OK,
                StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
                _ => null
            };

            if (statusCode is >= 400)
            {
                var traceId = context.HttpContext.Items["TraceId"]?.ToString() ?? Guid.NewGuid().ToString("N");
                var message = GetMessage(result) ?? "Request failed";

                if (result is ObjectResult { Value: ErrorResponse })
                {
                    await next();
                    return;
                }

                context.Result = new ObjectResult(new ErrorResponse
                {
                    Error = new ErrorInfo
                    {
                        Code = statusCode == StatusCodes.Status404NotFound ? "NOT_FOUND" : "REQUEST_FAILED",
                        Message = message,
                        Details = [],
                        TraceId = traceId
                    }
                })
                {
                    StatusCode = statusCode
                };

                _logger.LogError(
                    "Request error traceId={TraceId} method={Method} path={Path} status={StatusCode}",
                    traceId,
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    statusCode
                );
            }

            await next();
        }

        private static string? GetMessage(IActionResult result)
        {
            if (result is ObjectResult objectResult)
            {
                return objectResult.Value switch
                {
                    ProblemDetails problemDetails => problemDetails.Title ?? problemDetails.Detail,
                    string message => message,
                    _ => null
                };
            }

            return null;
        }
    }
}
