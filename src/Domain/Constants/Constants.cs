using System;

namespace Domain.Constants
{
    public static class Constants
    {
        public const string StorageAccountSettingName = "AzureWebJobsStorage";
        public static class AzureQueueNames
        {
            public const string InitializeServiceQueue = "initialize-service-queue";
            public const string ShutDownServiceQueue = "shutdown-service-queue";
            public const string StartVirtualMachineQueue = "start-virtual-machine-queue";
            public const string StopVirtualMachineQueue = "stop-virtual-machine-queue";
        }

        public static class MediaInjectionUrl
        {
            public static class Rtmp
            {
                public static class Push
                {
                    public const string Client = "rtmp://{0}:1936/ingest/{1}?callId={2}";
                    public const string Gstreamer = "rtmp://{0}:1936/ingest/{1}";

                }
            }

            public static class Rtmps
            {
                public static class Push
                {
                    public const string Client = "rtmps://{0}:2936/secure-ingest/{1}?callId={2}";
                    public const string Gstreamer = "rtmp://{0}:29361/secure-ingest/{1}";

                }
            }

            public static class Srt
            {
                public static class Listener
                {
                    public const string Client = "srt://{0}:9000?mode=caller";
                    public const string Gstreamer = "srt://{0}:9000?mode=listener";

                }
            }
        }
        
        public static class AzureEventGid
        {
            public static class EventTypes
            {
                public const string ResourceActionSuccessEvent = "Microsoft.Resources.ResourceActionSuccess";
            }

            public static class VirtualMachineOperationType
            {
                public const string Start = "Microsoft.Compute/virtualMachines/start/action";
                public const string Deallocate = "Microsoft.Compute/virtualMachines/deallocate/action";
            }
        }

        public static class EnvironmentDefaults
        {
            public const string ServiceId = "00000000-0000-0000-0000-000000000000";
            public const string VirtualMachineName = "localhost";
            public const string VirtualMachineResourceId = "localhost";
            public const string IpAddress = "localhost:9442";
        }

        public static class DefaultParticipantsDisplayNames
        {
            public const string PrimarySpeaker = "Primary Speaker";
            public const string ScreenShare = "Screen Share";
        }

        public static class Messages
        {
            public static class StartExtraction
            {
                public const string Error = "Error while trying to start an extraction";
            }

            public static class StopExtraction
            {
                public const string Error = "Error while trying to stop an extraction";
            }

            public static class StartInjection
            {
                public const string Error = "Error while trying to start an injection";
            }

            public static class StopInjection
            {
                public const string Error = "Error while trying to stop an injection";
            }
        }
    }
}
