using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum BusMessageType
    {
        Unknown,
        Error,
        StateChanged,
        StreamStatus,
        Buffering,
        Qos,
        Eos
    }
}
