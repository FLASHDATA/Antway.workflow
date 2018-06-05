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
    }
}
