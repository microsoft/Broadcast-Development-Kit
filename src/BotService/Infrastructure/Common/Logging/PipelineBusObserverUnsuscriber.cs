using System;

namespace BotService.Infrastructure.Common.Logging
{
    public class PipelineBusObserverUnsuscriber : IDisposable
    {
        private IObserver<BusEventPayload> _observer;

        public PipelineBusObserverUnsuscriber(IObserver<BusEventPayload> observer)
        {
            _observer = observer;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _observer = null;
            }
        }
    }
}
