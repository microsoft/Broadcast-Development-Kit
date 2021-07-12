// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class StartStreamInjectionException : Exception
    {
        public StartStreamInjectionException()
            : base(null)
        {
        }

        public StartStreamInjectionException(string message)
            : base(message, null)
        {
        }

        public StartStreamInjectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected StartStreamInjectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
