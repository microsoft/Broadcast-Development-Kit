// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Domain.Entities.Parts
{
    public class ParticipantStreamErrorDetails
    {
        public ParticipantStreamErrorDetails()
        {
        }

        public ParticipantStreamErrorDetails(ParticipantStreamErrorType type)
        {
            Type = type;
            Message = GetDefaultMessage();
        }

        public ParticipantStreamErrorDetails(ParticipantStreamErrorType type, string message)
        {
            Type = type;
            Message = message;
        }

        public ParticipantStreamErrorType Type { get; set; }

        public string Message { get; set; }

        private string GetDefaultMessage()
        {
            var message = $"Error while trying to {Type}";
            return message;
        }
    }
}
