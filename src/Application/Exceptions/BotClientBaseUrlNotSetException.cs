// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class BotClientBaseUrlNotSetException : Exception
    {
        public BotClientBaseUrlNotSetException()
        {
        }

        public BotClientBaseUrlNotSetException(string message)
            : base(message)
        {
        }

        protected BotClientBaseUrlNotSetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
