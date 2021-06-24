using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Common
{
    public interface IAzStorageHandler
    {
        Task AddQueueMessageAsync(string queue, object message);
    }
}
