// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Domain.Enums
{
    public enum StreamState
    {
        Disconnected,
        Starting,
        Ready,
        Receiving,
        NotReceiving,
        Stopping,
        StartingError,
        StoppingError,
    }
}
