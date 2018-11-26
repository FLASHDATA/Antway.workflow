using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Provider.Model.DataTable;

namespace AntWay.Persistence.Provider.Model
{
    public class ProcessPersistence
    {
        public IDALProcessPersistence IDALProcessPersistence { get; set; }


        public List<WorkFlowDataTableView> GetWorkFlowsDataTableView(DataTableFilters filter)
        {
            var result = IDALProcessPersistence.GetWorkFlowsDataTableView(filter);
            return result;
        }

        public int GetProccessesHistoryTotalRegistros(DataTableFilters filter)
        {
            var result = IDALProcessPersistence.GetProccessesHistoryTotalRegistros(filter);
            return result;
        }

        public List<ProcessHistoryDataTableView> GetProccessesHistoryDataTableView(DataTableFilters filter)
        {
            var result = IDALProcessPersistence.GetProccessesHistoryDataTableView(filter);
            return result;
        }

        public int GetProccessHistoryDetailTotalRegistros(ProcessHistoryDetailFilter filter)
        {
            var result = IDALProcessPersistence.GetProccessHistoryDetailTotalRegistros(filter);
            return result;
        }

        public List<ProcessHistoryDetailDataTableView> GetProccessHistoryDetailDataTableView(ProcessHistoryDetailFilter filter)
        {
            var result = IDALProcessPersistence.GetProcessHistoryDetailTableView(filter);
            return result;
        }
    }
}
