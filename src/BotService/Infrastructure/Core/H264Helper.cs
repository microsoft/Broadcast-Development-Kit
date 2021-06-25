using System;
using System.Runtime.InteropServices;

namespace BotService.Infrastructure.Core
{
    public static class H264Helper
    {
        /* In H264, the content of the stream is segmented in Network Abstraction Layer Units (or NAL Units / NALU for short).
         * The beginning of each NALU is prefixed by one of the following byte sequences (depending on the implementation of the encoder):
         *      0x00 00 01
         *      0x00 00 00 01
         *
         * The following byte contains the NALU type in the lower 5 bits of the value. As a simplification, if the value is 5 (0 0101) then the NALU correspond to an I-Frame.
         * A single frame in H264 is composed of multiple NALUs, but since we only care about detecting the presence of an I-frame
         * we can simply search for this byte sequence to identify if the frame is an I-Frame or not.
         *
         * Based on some tests, it seem that the NALU we are looking for starts at the ~28 byte of the buffer, but it can move over time as other NALUs change in size.
         * As an optimization we allow to limit how many position of the array we will check before assuming it's not an I-frame.
         * With a limit of 100, this method should take ~0.002ms to run.
         */
        public static bool IsKeyFrame(IntPtr frame, long frameLength, int limit = int.MaxValue)
        {
            int framePosition = 0;
            int zeroSlideLength = 0;

            limit = limit <= frameLength ? limit : (int)frameLength - 1;

            byte frameByte;
            while (framePosition < limit)
            {
                frameByte = Marshal.ReadByte(frame, framePosition);

                if (frameByte == 0x00)
                {
                    zeroSlideLength++;
                }
                else if (frameByte == 0x01 && zeroSlideLength >= 2)
                {
                    // We found a NAL unit. We can check the next byte to see what kind of NALU type this is.
                    frameByte = Marshal.ReadByte(frame, framePosition + 1);

                    // The NALU type is in the last 5 bits of this byte. We can extract those bits using a mask (0x1F = 0001 1111). If the value is 5 then this is a key frame.
                    if ((frameByte & 0x1F) == 5)
                    {
                        return true;
                    }
                    else
                    {
                        // This is not the NALU type we are looking for. Since the 0x01 means that the zero slide was cut, we reset this counter.
                        zeroSlideLength = 0;
                    }
                }
                else
                {
                    // This zero slide was not part of a NALU delimiter. Resetting counter.
                    zeroSlideLength = 0;
                }

                framePosition++;
            }

            return false;
        }
    }
}
