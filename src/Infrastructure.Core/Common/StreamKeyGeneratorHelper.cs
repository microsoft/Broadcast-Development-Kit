using Application.Interfaces.Common;
using System;

namespace Infrastructure.Core.Common
{
    public class StreamKeyGeneratorHelper : IStreamKeyGeneratorHelper
    {
        public string GetNewStreamKey()
        {
            return GuidToBase64();
        }

        private string GuidToBase64()
        {
            var guid = Guid.NewGuid();
            var streamKey = Convert.ToBase64String(guid.ToByteArray())
                .Replace("/", "")
                .Replace("=", "")
                .Replace("+", "")
                .Replace("&", "");

            return streamKey;
        }
    }
}
