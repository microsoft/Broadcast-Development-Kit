// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Application.Common.Models;

namespace Application.Interfaces.Common
{
    public interface IExtractionUrlHelper
    {
        string GetSrtStreamUrl(StartSrtStreamExtractionResponse startSrtStreamExtractionResponse, string serviceDns);

        string GetRtmpStreamUrl(StartRtmpStreamExtractionResponse startRtmpStreamExtractionResponse, string callId, string serviceDns);
    }
}