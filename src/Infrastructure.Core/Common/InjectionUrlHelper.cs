using Application.Common.Models;
using Application.Interfaces.Common;
using Domain.Constants;
using Domain.Enums;
using System;

namespace Infrastructure.Core.Common
{
    public class InjectionUrlHelper: IInjectionUrlHelper
    {
        public string GetStreamUrl(StartStreamInjectionBody startStreamInjectionBody, string serviceDns = null)
        {
            var streamUrl = startStreamInjectionBody.StreamUrl;

            switch (startStreamInjectionBody.Protocol)
            {
                case Protocol.SRT:
                    var srtBody = (SrtStreamInjectionBody)startStreamInjectionBody;
                    streamUrl = GetSrtStreamUrl(srtBody, serviceDns);

                    break;
                case Protocol.RTMP:
                    var rtmpBody = (RtmpStreamInjectionBody)startStreamInjectionBody;
                    streamUrl = GetRtmpStreamUrl(rtmpBody, serviceDns);

                    break;
                default:
                    break;
            }
            return streamUrl;
        }

        private static string GetSrtStreamUrl(SrtStreamInjectionBody streamInjectionBody, string serviceDns = null)
        {
            var streamUrl = streamInjectionBody.StreamUrl;
            if (streamInjectionBody.Mode == SrtMode.Listener)
            {
                streamUrl = string.Format(Constants.MediaInjectionUrl.Srt.Listener.Client, serviceDns);
            }

            return streamUrl;
        }

        private static string GetRtmpStreamUrl(RtmpStreamInjectionBody streamInjectionBody, string serviceDns = null)
        {
            var streamUrl = streamInjectionBody.StreamUrl;
            if (streamInjectionBody.Mode == RtmpMode.Push)
            {
                var template = streamInjectionBody.EnableSsl ? Constants.MediaInjectionUrl.Rtmps.Push.Client : Constants.MediaInjectionUrl.Rtmp.Push.Client;

                streamUrl = string.Format(template, serviceDns, streamInjectionBody.StreamKey, streamInjectionBody.CallId);
            }

            return streamUrl;
        }
    }
}
