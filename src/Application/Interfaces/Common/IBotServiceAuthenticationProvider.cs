using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Common
{
    public interface IBotServiceAuthenticationProvider
    {
        Task<string> GetTokenAsync();
    }
}
