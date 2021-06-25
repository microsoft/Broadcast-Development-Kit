using System;
using Application.Common.Models;
using Domain.Enums;
using Newtonsoft.Json.Linq;

namespace Application.Common.Converter
{
    public class StartStreamExtractionBodyConverter : JsonCreationConverter<StartStreamExtractionBody>
    {
        protected override StartStreamExtractionBody Create(Type objectType, JObject jObject)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            var value = jObject["protocol"] == null ? jObject["Protocol"].ToString() : jObject["protocol"].ToString();
            var protocol = (Protocol)Enum.Parse(typeof(Protocol), value);

            if (protocol == Protocol.RTMP)
            {
                return new RtmpStreamExtractionBody();
            }
            else if (protocol == Protocol.SRT)
            {
                return new SrtStreamExtractionBody();
            }
            else
            {
                return new StartStreamExtractionBody();
            }
        }
    }
}
