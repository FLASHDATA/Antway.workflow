using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Model
{
    public interface IActivityModel
    {
        object DeserializePersistedObject(string jsonPersisted);
    }
}
