using System;
using System.Net;

namespace KalolCommunity.Application.Exceptions
{
    /// <summary>
    /// Base exception class for all application-specific exceptions.
    /// Provides consistent error handling with HTTP status code mapping.
    /// </summary>
    public abstract class BaseException : Exception
    {
        /// <summary>
        /// HTTP status code associated with this exception
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Optional error code for client-side error handling
        /// </summary>
        public string? ErrorCode { get; protected set; }

        protected BaseException(
            string message,
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }

        protected BaseException(
            string message,
            Exception innerException,
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}