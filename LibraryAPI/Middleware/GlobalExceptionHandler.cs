using LibraryAPI.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace LibraryAPI.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> Logger)
        {
            logger = Logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, exception.Message),
                BadRequestException => (StatusCodes.Status400BadRequest, exception.Message),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
                _ => (StatusCodes.Status500InternalServerError, "Something went wrong. Please try again later")
            };

            if (statusCode >= 500) logger.LogError(exception, "Server error: {Message}", exception.Message);
            else logger.LogWarning("Client error {StatusCode}: {Message}", statusCode, exception.Message);

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
            return true;
        }
    }
}
