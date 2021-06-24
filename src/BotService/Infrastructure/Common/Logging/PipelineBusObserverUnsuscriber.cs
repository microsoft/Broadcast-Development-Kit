using System;

namespace BotService.Infrastructure.Common.Logging
{
    public class PipelineBusObserverUnsuscriber : IDisposable
    {
        private IObserver<BusEventPayload> observer;

        public PipelineBusObserverUnsuscriber(IObserver<BusEventPayload> observer)
        {
            this.observer = observer;
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
                observer = null;
            }
        }
    }
}
