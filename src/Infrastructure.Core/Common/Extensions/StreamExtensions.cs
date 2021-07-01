// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.IO;

namespace Infrastructure.Core.Common.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
