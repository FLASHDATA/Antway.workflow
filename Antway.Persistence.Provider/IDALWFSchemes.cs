 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Provider.Model.DataTable;

namespace AntWay.Persistence.Provider.Model
{
    public interface IDALWFSchemes: IDAL
    {
        List<SchemeDataTableView> GetSchemesDataTableView(DataTableFilters filter);

        List<WorkflowSchemeView> GetWorkflowSchemes();

        List<WorkflowSchemeView> GetWorkflowSchemesServices();
    }
}
