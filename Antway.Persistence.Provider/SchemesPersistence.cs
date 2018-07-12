using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Provider.Model.DataTable;

namespace AntWay.Persistence.Provider.Model
{
    public class SchemesPersistence
    {
        public IDALWFSchemes IDALSchemes { get; set; }


        public List<SchemeDataTableView> GetSchemesDataTableView(DataTableFilters filter)
        {
            var result = IDALSchemes.GetSchemesDataTableView(filter);
            return result;
        }

        public WorkflowSchemeView InsertScheme(WorkflowSchemeView schemeView)
        {
            var result = IDALSchemes.Insert(schemeView);
            return result;
        }


        public WorkflowSchemeView UpdateScheme(WorkflowSchemeView schemeView)
        {
            var result = IDALSchemes.Update(schemeView);
            return result;
        }

        public List<WorkflowSchemeView> GetSchemes()
        {
            var schemes = IDALSchemes.GetWorkflowSchemes();
            return schemes;
        }

        public WorkflowSchemeView GetScheme(string id)
        {
            var scheme = IDALSchemes.Fetch<WorkflowSchemeView>(id);
            return scheme;
        }
    }
}
