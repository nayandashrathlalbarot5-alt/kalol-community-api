using System;
using System.Net;

namespace KalolCommunity.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a request conflicts with the current state of the resource
    /// </summary>
    public sealed class ConflictException : BaseException
    {
        public ConflictException(string message)
            : base(message, HttpStatusCode.Conflict)
        {
            ErrorCode = "RESOURCE_CONFLICT";
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.Conflict)
        {
            ErrorCode = "RESOURCE_CONFLICT";
        }

        public ConflictException(string resourceName, string reason)
            : base($"{resourceName} conflict: {reason}", HttpStatusCode.Conflict)
        {
            ErrorCode = "RESOURCE_CONFLICT";
        }
    }
}