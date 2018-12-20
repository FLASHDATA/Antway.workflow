using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AntWay.Persistence.Provider.Model
{
    public class LocatorPersistence
    {
        public IDALLocators IDALocators { get; set; }


        public LocatorView GetWorkflowLocatorFromGuid(Guid guid)
        {
            var dataView = IDALocators.GetLocatorFromGuid(guid);
            return dataView;
        }

        public LocatorView GetWorkflowByLocator(string schemeCode, string locator)
        {
            var dataView = IDALocators.Fetch(schemeCode, locator);
            return dataView;
        }
        
        public List<LocatorView> GetLocatorsFromScheme(string scheme)
        {
            var result = IDALocators.GetLocatorsFromScheme(scheme);
            return result;
        }

        public LocatorView AddWorkflowLocator(LocatorView wfLocatorView)
        {
            var result = IDALocators.Insert(wfLocatorView);
            return result;
        }
    }
}
