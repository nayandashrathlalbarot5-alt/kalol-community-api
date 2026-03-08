using System;
using System.Net;

namespace KalolCommunity.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication is required but not provided or invalid
    /// </summary>
    public sealed class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message)
            : base(message, HttpStatusCode.Unauthorized)
        {
            ErrorCode = "UNAUTHORIZED";
        }

        public UnauthorizedException()
            : base("Authentication is required to access this resource.", HttpStatusCode.Unauthorized)
        {
            ErrorCode = "UNAUTHORIZED";
        }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.Unauthorized)
        {
            ErrorCode = "UNAUTHORIZED";
        }
    }
}