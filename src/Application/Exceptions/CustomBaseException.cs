using System;
using System.Net;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class CustomBaseException : Exception
    {
        public CustomBaseException(HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base("An error occurred while processing your request.")
        {
            StatusCode = statusCode;
        }

        public CustomBaseException(string message,  HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public CustomBaseException(string title, string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            Title = title;
            StatusCode = statusCode;
        }

        protected CustomBaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Title { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
