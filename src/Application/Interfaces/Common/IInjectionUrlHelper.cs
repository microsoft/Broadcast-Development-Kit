using Application.Common.Models;

namespace Application.Interfaces.Common
{
    public interface IInjectionUrlHelper
    {
        string GetStreamUrl(StartStreamInjectionBody startStreamInjectionBody, string serviceDns = null);
    }
}
