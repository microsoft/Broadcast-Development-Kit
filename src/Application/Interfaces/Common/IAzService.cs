using Microsoft.Azure.Management.Fluent;

namespace Application.Interfaces.Common
{
    public interface IAzService
    {
        IAzure GetAzure();
    }
}
