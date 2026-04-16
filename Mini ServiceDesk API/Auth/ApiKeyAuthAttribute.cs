using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mini_ServiceDesk_API.Auth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        public string? Role { get; set; }

        private static readonly IDictionary<string, string> ApiKeys = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "agent-secret-000", "agent" },
            { "reporter-secret-000", "reporter" }
        };

        public ApiKeyAuthAttribute()
        {
        }

        public ApiKeyAuthAttribute(string role)
        {
            Role = role;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new { error = "API key missing" });
                return;
            }

            var key = extractedApiKey.ToString();
            if (string.IsNullOrWhiteSpace(key) || !ApiKeys.TryGetValue(key, out var role))
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid API key" });
                return;
            }


            if (!string.IsNullOrWhiteSpace(Role) && !string.Equals(Role, role, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new { error = "Forbidden" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }


            context.HttpContext.Items["ApiRole"] = role;

            await next();
        }
    }
}
