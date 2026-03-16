using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace KalolCommunity.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when validation fails for one or more fields
    /// </summary>
    public sealed class ValidationException : BaseException
    {
        /// <summary>
        /// Dictionary of field names and their associated error messages
        /// </summary>
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
        {
            ErrorCode = "VALIDATION_ERROR";
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        public ValidationException(string field, string error)
            : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
        {
            ErrorCode = "VALIDATION_ERROR";
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { error } }
            };
        }

        public ValidationException(string field, string[] errors)
            : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
        {
            ErrorCode = "VALIDATION_ERROR";
            Errors = new Dictionary<string, string[]>
            {
                { field, errors }
            };
        }

        /// <summary>
        /// Creates a validation exception from a list of error messages
        /// </summary>
        public ValidationException(IEnumerable<string> errorMessages)
            : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
        {
            ErrorCode = "VALIDATION_ERROR";
            Errors = new Dictionary<string, string[]>
            {
                { "General", errorMessages.ToArray() }
            };
        }
    }
}