// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Runtime.Serialization;

namespace Domain.Exceptions
{
    [Serializable]
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(string message)
            : base(message)
        {
        }

        public EntityNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public EntityNotFoundException(string name, string id)
            : base($"An {name} entity with key \"{id}\" was not found.")
        {
        }

        protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
