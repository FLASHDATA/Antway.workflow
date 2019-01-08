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
        LocatorView Fetch(string schemeCode, string locatorValue);

        LocatorView GetLocatorFromGuid(Guid guid);

        List<LocatorView> GetLocatorsFromScheme(string scheme);

        Guid? GetProcessIdFromRelation(string consumer, string entityId);
    }
}
