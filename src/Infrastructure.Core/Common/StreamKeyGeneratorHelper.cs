// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Application.Interfaces.Common;

namespace Infrastructure.Core.Common
{
    public class StreamKeyGeneratorHelper : IStreamKeyGeneratorHelper
    {
        public string GetNewStreamKey()
        {
            return GuidToBase64();
        }

        private static string GuidToBase64()
        {
            var guid = Guid.NewGuid();
            var streamKey = Convert.ToBase64String(guid.ToByteArray())
                .Replace("/", string.Empty)
                .Replace("=", string.Empty)
                .Replace("+", string.Empty)
                .Replace("&", string.Empty);

            return streamKey;
        }
    }
}
