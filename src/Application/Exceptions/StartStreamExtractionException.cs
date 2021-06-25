using System;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class StartStreamExtractionException : Exception
    {
        public StartStreamExtractionException()
        {
        }

        public StartStreamExtractionException(string message)
            : base(message)
        {
        }

        protected StartStreamExtractionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
