namespace Infrastructure.Core.CosmosDbData
{
    public static class CosmosDbConstants
    {
        public const string AuditContainer = "Audit";
        public const string AuditPartitionKey = "/EntityId";

        public const string CallContainer = "Call";
        public const string CallPartitionKey = "/id";

        public const string StreamContainer = "Stream";
        public const string StreamPartitionKey = "/id";

        public const string ParticipantStreamContainer = "ParticipantStream";
        public const string ParticipantStreamPartitionKey = "/id";

        public const string ServiceContainer = "Service";
        public const string ServicePartitionKey = "/id";
    }
}
