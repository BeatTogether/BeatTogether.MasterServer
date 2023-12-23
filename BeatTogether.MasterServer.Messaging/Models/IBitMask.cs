using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public interface IBitMask<T>
    {
        int bitCount { get; }

        T SetBits(int offset, ulong bits);

        ulong GetBits(int offset, int count);
    }
}
