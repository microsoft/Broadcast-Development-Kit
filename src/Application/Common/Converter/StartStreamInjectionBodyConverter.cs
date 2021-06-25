using System;
using Application.Common.Models;
using Domain.Enums;
using Newtonsoft.Json.Linq;

namespace Application.Common.Converter
{
    public class StartStreamInjectionBodyConverter : JsonCreationConverter<StartStreamInjectionBody>
    {
        protected override StartStreamInjectionBody Create(Type objectType, JObject jObject)
        {
            jObject = jObject ?? throw new ArgumentNullException(nameof(jObject));

            var value = jObject["protocol"] == null ? jObject["Protocol"].ToString() : jObject["protocol"].ToString();
            var protocol = (Protocol)Enum.Parse(typeof(Protocol), value);

            if (protocol == Protocol.RTMP)
            {
                return new RtmpStreamInjectionBody();
            }
            else if (protocol == Protocol.SRT)
            {
                return new SrtStreamInjectionBody();
            }
            else
            {
                return new StartStreamInjectionBody();
            }
        }
    }
}
