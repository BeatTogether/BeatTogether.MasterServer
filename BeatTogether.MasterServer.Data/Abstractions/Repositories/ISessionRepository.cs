using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public interface ISessionRepository
    {
        void AddSessionCookie();
    }
}
