using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KalolCommunity.Application.Exceptions;
using KalolCommunity.Contracts.DTO;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KalolCommunity.Api.Handlers
{
    /// <summary>
    /// Global exception handler implementing IExceptionHandler for .NET 8+
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
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
            var (statusCode, title) = MapException(exception);

            var payload = new ApiResponse<object>
            {
                Success = false,
                StatusCode = statusCode,
                Message = title,
                Data = new
                {
                    errorCode = (exception as BaseException)?.ErrorCode,
                    traceId = httpContext.TraceIdentifier,
                    path = httpContext.Request.Path.Value,
                    details = GetSafeErrorMessage(exception, httpContext),
                    errors = (exception as ValidationException)?.Errors,
                }
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled server exception. StatusCode: {StatusCode}, TraceId: {TraceId}, Path: {Path}, ErrorCode: {ErrorCode}",
                    statusCode,
                    httpContext.TraceIdentifier,
                    httpContext.Request.Path.Value,
                    (exception as BaseException)?.ErrorCode
                );
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Handled request exception. StatusCode: {StatusCode}, TraceId: {TraceId}, Path: {Path}, ErrorCode: {ErrorCode}",
                    statusCode,
                    httpContext.TraceIdentifier,
                    httpContext.Request.Path.Value,
                    (exception as BaseException)?.ErrorCode
                );
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);

            return true; // Exception handled successfully
        }

        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            BaseException appEx => ((int)appEx.StatusCode, appEx.Message),
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        private static string? GetSafeErrorMessage(Exception exception, HttpContext context)
        {
            var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
            if (env.IsDevelopment())
            {
                return exception.Message;
            }

            return exception is BaseException ? exception.Message : null;
        }
    }
}