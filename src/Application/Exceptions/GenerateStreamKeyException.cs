// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Net;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class GenerateStreamKeyException : CustomBaseException
    {
        public GenerateStreamKeyException(string title, string message)
            : base(message)
        {
            Title = title;
            StatusCode = HttpStatusCode.InternalServerError;
        }

        protected GenerateStreamKeyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
