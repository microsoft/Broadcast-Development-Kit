// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Net;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class NotValidStreamKeyException : CustomBaseException
    {
        public NotValidStreamKeyException(string title, string message)
            : base(message)
        {
            Title = title;
            StatusCode = HttpStatusCode.Unauthorized;
        }

        protected NotValidStreamKeyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
