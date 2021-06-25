using System;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException()
        {
        }

        public ServiceUnavailableException(string message)
            : base(message)
        {
        }

        protected ServiceUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
