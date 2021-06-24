using Application.Exceptions.Models;
using System;
using System.Net;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class NotSuccessfulRequestException : Exception
    { 
        public ExceptionDetails RequestDetails { get; private set; }
        public NotSuccessfulRequestException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
        public NotSuccessfulRequestException(HttpStatusCode statusCode, ExceptionDetails details):base(details.Detail)
        {
            StatusCode = statusCode;
            RequestDetails = details;
        }
        protected NotSuccessfulRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
