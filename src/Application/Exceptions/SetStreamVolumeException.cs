// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Runtime.Serialization;

namespace Application.Exceptions
{
    [Serializable]
    public class SetStreamVolumeException : CustomBaseException
    {
        public SetStreamVolumeException(string title, string message)
            : base(title, message)
        {
        }

        protected SetStreamVolumeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
