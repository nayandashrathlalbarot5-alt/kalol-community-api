using System;
using System.Net;

namespace KalolCommunity.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when user is authenticated but lacks permission
    /// </summary>
    public sealed class ForbiddenException : BaseException
    {
        public ForbiddenException(string message)
            : base(message, HttpStatusCode.Forbidden)
        {
            ErrorCode = "FORBIDDEN";
        }

        public ForbiddenException()
            : base("You do not have permission to access this resource.", HttpStatusCode.Forbidden)
        {
            ErrorCode = "FORBIDDEN";
        }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.Forbidden)
        {
            ErrorCode = "FORBIDDEN";
        }
    }
}