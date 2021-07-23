namespace BotService.Infrastructure.Pipelines
{
    public class GstreamerClockProvider
    {
        private ulong _baseTime;

        public GstreamerClockProvider()
        {
            Clock = new GstreamerCustomClock();
        }

        public ulong BaseTime => GetBaseTime();

        public GstreamerCustomClock Clock { get; }

        public void ResetBaseTime()
        {
            _baseTime = 0;
        }

        private ulong GetBaseTime()
        {
            if (_baseTime == 0)
            {
                _baseTime = (ulong)((System.DateTime.UtcNow - new System.DateTime(1900, 1, 1)).Ticks * 100);
            }

            return _baseTime;
        }
    }
}
