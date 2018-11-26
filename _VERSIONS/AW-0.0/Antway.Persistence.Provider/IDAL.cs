using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider
{
    public interface IDAL
    {
        T Fetch<T>(object pk);
        T Update<T>(T objectView);
        T Insert<T>(T objectView);
    }
}
