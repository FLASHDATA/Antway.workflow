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

        public int GeProccessHistoryTotalRegistros(ProcessHistoryFilter filter)
        {
            var result = IDALProcessPersistence.GeProccessHistoryTotalRegistros(filter);
            return result;
        }

        public List<ProcessHistoryDataTableView> GeProccessHistoryDataTableView(ProcessHistoryFilter filter)
        {
            var result = IDALProcessPersistence.GeProccessHistoryDataTableView(filter);
            return result;
        }

        public int GeProccessHistoryDetailTotalRegistros(ProcessHistoryDetailFilter filter)
        {
            var result = IDALProcessPersistence.GeProccessHistoryDetailTotalRegistros(filter);
            return result;
        }

        public List<ProcessHistoryDetailDataTableView> GeProccessHistoryDetailDataTableView(ProcessHistoryDetailFilter filter)
        {
            var result = IDALProcessPersistence.GetProcessHistorryDetailTableView(filter);
            return result;
        }


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
