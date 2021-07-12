// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Calls;
using Newtonsoft.Json;

namespace BotService.Infrastructure.Extensions
{
    public static class SerializationExtensions
    {
        public static string ToJson(this IParticipant participant)
        {
            ParticipantDataToLog participantDataToLog = new ParticipantDataToLog
            {
                Id = participant.Id,
                Client = new ParticipantClientDataToLog
                {
                    AppName = participant.Client.AppName,
                    AppId = participant.Client.AppId,
                    BaseUrl = participant.Client.BaseUrl,
                },
                Resource = participant.Resource,
                ResourcePath = participant.ResourcePath,
                CreatedDateTime = participant.CreatedDateTime,
                ModifiedDateTime = participant.ModifiedDateTime,
            };

            return JsonConvert.SerializeObject(participantDataToLog, Formatting.Indented);
        }

        public class ParticipantDataToLog
        {
            public string Id { get; set; }

            public Participant Resource { get; set; }

            public string ResourcePath { get; set; }

            public DateTimeOffset CreatedDateTime { get; set; }

            public DateTimeOffset ModifiedDateTime { get; set; }

            public ParticipantClientDataToLog Client { get; set; }
        }

        public class ParticipantClientDataToLog
        {
            public string AppName { get; set; }

            public string AppId { get; set; }

            public string BaseUrl { get; set; }
        }
    }
}
