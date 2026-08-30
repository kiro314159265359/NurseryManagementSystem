using NurseryManagementSystem.Application;
using NurseryManagementSystem.Infrastructure;
using NurseryManagementSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace NurseryManagementSystem.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter()));
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .ToDictionary(
                            entry => entry.Key,
                            entry => entry.Value!.Errors
                                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                    ? "The supplied value is invalid."
                                    : error.ErrorMessage)
                                .ToArray());
                    return new BadRequestObjectResult(new
                    {
                        type = "https://httpstatuses.io/400",
                        title = "Validation error",
                        status = 400,
                        detail = "One or more validation failures have occurred.",
                        code = "VALIDATION_FAILED",
                        errors
                    });
                };
            });

            // CORS configuration for frontend
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseExceptionHandler();

            // Public machine-readable API contract used by the mobile and admin clients.
            app.MapOpenApi("/swagger/v1/swagger.json");

            // CORS middleware
            app.UseCors("AllowAll");

            // Health check endpoint for Render
            app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>();
                await initializer.InitializeAsync();
                await initializer.SeedAsync();
            }

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
