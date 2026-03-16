using System;
using System.Net;

namespace KalolCommunity.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when client request is malformed or invalid
    /// </summary>
    public sealed class BadRequestException : BaseException
    {
        public BadRequestException(string message)
            : base(message, HttpStatusCode.BadRequest)
        {
            ErrorCode = "BAD_REQUEST";
        }

        public BadRequestException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.BadRequest)
        {
            ErrorCode = "BAD_REQUEST";
        }

        public BadRequestException(string message, string errorCode)
            : base(message, HttpStatusCode.BadRequest)
        {
            ErrorCode = errorCode;
        }
    }
}