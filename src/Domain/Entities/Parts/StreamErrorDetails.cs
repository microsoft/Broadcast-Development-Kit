// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Domain.Entities.Parts
{
    public class StreamErrorDetails
    {
        public StreamErrorDetails()
        {
        }

        public StreamErrorDetails(StreamErrorType type)
        {
            Type = type;
            Message = GetDefaultMessage();
        }

        public StreamErrorDetails(StreamErrorType type, string message)
        {
            Type = type;
            Message = message;
        }

        public StreamErrorType Type { get; set; }

        public string Message { get; set; }

        private string GetDefaultMessage()
        {
            var message = $"Error while trying to {Type}";
            return message;
        }
    }
}
