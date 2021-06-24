using System;

namespace Domain.Enums
{
    public enum SupportedAudioFormat
    {
        AAC_44100 = 0,
        AAC_48000 = 1
    }

    public static class SupportAudioExtensions
    {
        public static int ToAudioRate(this SupportedAudioFormat supportedAudio)
        {
            switch (supportedAudio)
            {
                case SupportedAudioFormat.AAC_44100:
                    return 44100;
                case SupportedAudioFormat.AAC_48000:
                    return 48000;
                default:
                    throw new ArgumentOutOfRangeException(nameof(supportedAudio), "Unsupported audio");
            }

        }
    }
}
