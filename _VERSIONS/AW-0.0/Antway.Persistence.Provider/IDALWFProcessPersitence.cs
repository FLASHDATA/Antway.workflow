using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Provider.Model.DataTable;

namespace AntWay.Persistence.Provider
{
    public interface IDALProcessPersistence
    {
        List<WorkFlowDataTableView> GetWorkFlowsDataTableView(DataTableFilters filter);

        List<ProcessHistoryDataTableView> GetProccessesHistoryDataTableView(DataTableFilters filter);
        int GetProccessesHistoryTotalRegistros(DataTableFilters filter);

        List<ProcessHistoryDetailDataTableView>
            GetProcessHistoryDetailTableView(ProcessHistoryDetailFilter filter);
        int GetProccessHistoryDetailTotalRegistros(ProcessHistoryDetailFilter filter);

        List<string> GetDBSchemes();
        List<string> GetSchemes();
    }
}
