using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infrastructure.Core.CosmosDbData
{
    public static class CosmosDbSchema
    {
        public const string CallContainer = "Call";
        public const string CallPartitionKey = "/id";

        public const string StreamContainer = "Stream";
        public const string StreamPartitionKey = "/id";

        public const string ParticipantStreamContainer = "ParticipantStream";
        public const string ParticipantStreamPartitionKey = "/id";

        public const string ServiceContainer = "Service";
        public const string ServicePartitionKey = "/id";

        public static readonly ReadOnlyDictionary<string, string> Containers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            { CallContainer, CallPartitionKey },
            { ParticipantStreamContainer, ParticipantStreamPartitionKey },
            { StreamContainer, StreamPartitionKey },
            { ServiceContainer, ServicePartitionKey },
        });
    }
}
