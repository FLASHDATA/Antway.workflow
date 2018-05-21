using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.EFDAL
{
    public interface IDALAntWay
    {
        T Fetch<T>(object pk);
        T Update<T>(T objectView);
        T Insert<T>(T objectView);
    }
}
