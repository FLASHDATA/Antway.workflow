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
        public IDALWFSchemeParameters IDALSchemeParameters { get; set; }

        public List<WorkflowSchemeParameterValuesView> GetParametersList(string schemeCode)
        {
            var result = IDALSchemeParameters.GetWorkflowSchemeParameterValues(schemeCode);
            return result;
        }

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

        public List<WorkflowSchemeParameterValuesView>
                UpdateSchemes(string schemeCode,
                              List<KeyValuePair<string, List<string>>>
                              schemeParametersKVP)
        {
            var schemePV = new List<WorkflowSchemeParameterValuesView>();
            
            foreach(var sp in schemeParametersKVP)
            {
                var items = sp.Value
                          .Select(f => new WorkflowSchemeParameterValuesView
                          {
                              SchemeParameter = new WorkflowSchemeParameterView
                              {
                                  SchemCode = schemeCode,
                                  SchemeParameter = sp.Key
                              },
                              Value = f
                          })
                          .ToList();

                schemePV.AddRange(items);
            }


            var result = IDALSchemeParameters.UpdateScheme(schemePV);
            return result;
        }

        //public List<WorkflowSchemeParameterValuesView>
        //        UpdateSchemes(List<WorkflowSchemeParameterValuesView> schemePV)
        //{
        //    var result = IDALSchemeParameters.UpdateScheme(schemePV);
        //    return result;
        //}
    }
}
