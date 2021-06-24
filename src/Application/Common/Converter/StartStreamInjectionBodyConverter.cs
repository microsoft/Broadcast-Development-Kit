using Application.Common.Models;
using Domain.Enums;
using Newtonsoft.Json.Linq;
using System;

namespace Application.Common.Converter
{
    public class StartStreamInjectionBodyConverter: JsonCreationConverter<StartStreamInjectionBody>
    {
        protected override StartStreamInjectionBody Create(Type objectType, JObject jObject)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));

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
