// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class StopStreamInjectionException : Exception
    {
        public StopStreamInjectionException()
            : base(null)
        {
        }

        public StopStreamInjectionException(string message)
            : base(message, null)
        {
        }

        public StopStreamInjectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected StopStreamInjectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
