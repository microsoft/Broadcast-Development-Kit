// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Application.Common.Models;
using Application.Interfaces.Common;
using Domain.Constants;
using Domain.Enums;

namespace Infrastructure.Core.Common
{
    public class ExtractionUrlHelper : IExtractionUrlHelper
    {
        public string GetSrtStreamUrl(StartSrtStreamExtractionResponse startSrtStreamExtractionResponse, string serviceDns)
        {
            // RtmpStreamInjectionBody
            var streamUrl = startSrtStreamExtractionResponse.Url;

            if (startSrtStreamExtractionResponse.Mode == SrtMode.Listener)
            {
                var template = Constants.MediaExtractionUrl.Srt.Listener.Client;

                streamUrl = string.Format(template, serviceDns, startSrtStreamExtractionResponse.Port);
            }

            return streamUrl;
        }

        public string GetRtmpStreamUrl(StartRtmpStreamExtractionResponse startRtmpStreamExtractionResponse, string callId,  string serviceDns)
        {
            // RtmpStreamInjectionBody
            var streamUrl = startRtmpStreamExtractionResponse.StreamUrl;
            if (startRtmpStreamExtractionResponse.Mode == RtmpMode.Pull)
            {
                var template = startRtmpStreamExtractionResponse.EnableSsl ? Constants.MediaExtractionUrl.Rtmps.Pull.Client : Constants.MediaExtractionUrl.Rtmp.Pull.Client;

                streamUrl = string.Format(template, serviceDns, startRtmpStreamExtractionResponse.Port, startRtmpStreamExtractionResponse.StreamKey, callId);
            }

            return streamUrl;
        }
    }
}
