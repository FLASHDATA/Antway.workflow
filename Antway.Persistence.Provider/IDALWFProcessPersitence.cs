using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Model;

namespace AntWay.Persistence.Provider
{
    public interface IDALProcessPersistence: IDAL
    {
        ProcessPersistenceView GetLocatorFromGuid(Guid guid);

        List<ProcessHistoryDataTableView> GeProccessHistoryDataTableView(ProcessHistoryFilter filter);
        int GeProccessHistoryTotalRegistros(ProcessHistoryFilter filter);

        List<ProcessHistoryDetailDataTableView>
            GetProcessHistorryDetailTableView(ProcessHistoryDetailFilter filter);
        int GeProccessHistoryDetailTotalRegistros(ProcessHistoryDetailFilter filter);
    }
}
