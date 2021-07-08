// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
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
