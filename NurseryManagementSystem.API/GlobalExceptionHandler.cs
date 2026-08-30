using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Exceptions;
using ValidationException = NurseryManagementSystem.Application.Common.Exceptions.ValidationException;

namespace NurseryManagementSystem.API
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, code) = Map(exception);

            httpContext.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message
            };
            problemDetails.Extensions["code"] = code;

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Errors;
            }

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred while processing the request.");
                problemDetails.Detail = "An unexpected error occurred.";
            }

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
        }

        private static (int StatusCode, string Title, string Code) Map(Exception exception) => exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation error", "VALIDATION_FAILED"),
            NotFoundException e when e.Message.Contains("code", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status400BadRequest, "Invalid scan code", "INVALID_SCAN_CODE"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", "NOT_FOUND"),
            ConflictException e when e.Message.Contains("already checked in", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status409Conflict, "Conflict", "ALREADY_CHECKED_IN"),
            ConflictException e when e.Message.Contains("no open attendance", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status409Conflict, "Conflict", "NOT_CHECKED_IN"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", "CONFLICT"),
            ForbiddenAccessException e when e.Message.Contains("approval", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status403Forbidden, "Forbidden", "ACCOUNT_PENDING_APPROVAL"),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Forbidden", "FORBIDDEN_ROLE"),
            UnauthorizedAccessException e when e.Message.Contains("refresh token", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status401Unauthorized, "Unauthorized", "INVALID_REFRESH_TOKEN"),
            UnauthorizedAccessException e when e.Message.Contains("disabled", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status403Forbidden, "Forbidden", "ACCOUNT_DISABLED"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "INVALID_CREDENTIALS"),
            _ => (StatusCodes.Status500InternalServerError, "Server error", "INTERNAL_ERROR")
        };
    }
}
