namespace Domain.Enums
{
    public enum BusMessageType
    {
        Unknown,
        Error,
        StateChanged,
        StreamStatus,
        Buffering,
        Qos,
        Eos,
    }
}
