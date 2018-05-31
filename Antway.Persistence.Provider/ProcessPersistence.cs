using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Model;

namespace AntWay.Persistence.Provider
{
    public class ProcessPersistence
    {
        public IDALProcessPersistence IDALProcessPersistence { get; set; }

        public ProcessPersistenceView GetWorkflowLocatorFromGuid(Guid guid)
        {
            var dataView = IDALProcessPersistence.GetLocatorFromGuid(guid);
            return dataView;
        }

        public ProcessPersistenceView GetWorkflowLocator(string locator)
        {
            var dataView = IDALProcessPersistence.Fetch<ProcessPersistenceView>(locator);
            return dataView;
        }

        public ProcessPersistenceView AddWorkflowLocator(ProcessPersistenceView wfLocatorView)
        {
            var result = IDALProcessPersistence.Insert(wfLocatorView);
            return result;
        }
    }
}
