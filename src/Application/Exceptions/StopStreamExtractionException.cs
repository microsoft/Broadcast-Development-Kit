// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class StopStreamExtractionException : Exception
    {
        public StopStreamExtractionException()
        {
        }

        public StopStreamExtractionException(string message)
            : base(message)
        {
        }

        protected StopStreamExtractionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
