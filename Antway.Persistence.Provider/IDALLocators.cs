using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Provider.Model;

namespace AntWay.Persistence.Provider.Model
{
    public interface IDALLocators: IDAL
    {
        ProcessPersistenceView GetLocatorFromGuid(Guid guid);

        List<ProcessPersistenceView> GetLocatorsFromScheme(string scheme);
    }
}
