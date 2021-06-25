using System;
using System.Net;
using System.Runtime.Serialization;
using Application.Exceptions.Models;

namespace Application.Exceptions
{
    [Serializable]
    public class NotSuccessfulRequestException : Exception
    {
        public NotSuccessfulRequestException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public NotSuccessfulRequestException(HttpStatusCode statusCode, ExceptionDetails details)
            : base(details.Detail)
        {
            StatusCode = statusCode;
            RequestDetails = details;
        }

        protected NotSuccessfulRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ExceptionDetails RequestDetails { get; private set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
