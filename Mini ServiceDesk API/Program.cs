using Microsoft.EntityFrameworkCore;
using Mini_ServiceDesk_API.Data;
using Mini_ServiceDesk_API.Services;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Mini_ServiceDesk_API.Middleware;
using Microsoft.AspNetCore.Mvc;
using Mini_ServiceDesk_API.Filters;
using Mini_ServiceDesk_API.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Add services
// --------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ErrorResponseFilter>();
}).ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var traceId = context.HttpContext.Items["TraceId"]?.ToString() ?? Guid.NewGuid().ToString("N");
        var details = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new
            {
                field = entry.Key,
                issue = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "invalid" : error.ErrorMessage
            }))
            .ToArray();

        var message = details.FirstOrDefault()?.issue ?? "Validation failed";
        return new BadRequestObjectResult(new ErrorResponse
        {
            Error = new ErrorInfo
            {
                Code = "VALIDATION_ERROR",
                Message = message,
                Details = details,
                TraceId = traceId
            }
        });
    };
});
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
}).AddMvc(options =>
{
    options.Conventions.Add(new VersionByNamespaceConvention());
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// Register application services
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mini ServiceDesk API");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();