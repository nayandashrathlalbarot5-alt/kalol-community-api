using System;
using System.Net;

namespace KalolCommunity.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource is not found
    /// </summary>
    public sealed class NotFoundException : BaseException
    {
        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with identifier '{key}' was not found.", HttpStatusCode.NotFound)
        {
            ErrorCode = "RESOURCE_NOT_FOUND";
        }

        public NotFoundException(string message)
            : base(message, HttpStatusCode.NotFound)
        {
            ErrorCode = "RESOURCE_NOT_FOUND";
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.NotFound)
        {
            ErrorCode = "RESOURCE_NOT_FOUND";
        }
    }
}