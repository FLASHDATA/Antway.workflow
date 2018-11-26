using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Oracle.Provider.Data;
using AntWay.Persistence.Provider.Model;
using AntWay.Persistence.Provider.Model.DataTable;
using Devart.Data.Oracle;

namespace AntWay.Oracle.Provider
{
    public class WFSchemesParametersEFDAL : IDALWFSchemeParameters
    {
        public List<WorkflowSchemeParameterView> GetWorkflowSchemeParameters(string schemCode = null)
        {
            using (var ctx = new Model1())
            {
                var result = ctx.WF_SCHEMES_PARAMETERS
                             .ToList()
                             .Select(s => new WorkflowSchemeParameterView
                             {
                                 SchemCode = s.SCHEME_CODE,
                                 SchemeParameter = s.SCHEME_PARAMETER
                             })
                             .ToList();

                return result;
            }
        }

        public List<WorkflowSchemeParameterValuesView> GetWorkflowSchemeParameterValues(string schemCode = null)
        {
            using (var ctx = new Model1())
            {
                var result = ctx.WF_SCHEME_PARAMETERS_VALUES
                             .ToList()
                             .Select(s => new WorkflowSchemeParameterValuesView
                             {
                                Value = s.VALUE,
                                SchemeParameter = new WorkflowSchemeParameterView
                                {
                                    SchemCode = s.SCHEME_CODE,
                                    SchemeParameter = s.SCHEME_PARAMETER
                                }
                             })
                             .ToList();

                return result;
            }
        }

        public List<WorkflowSchemeParameterValuesView> UpdateScheme(List<WorkflowSchemeParameterValuesView> schemePV)
        {
            using (var ctx = new Model1())
            {
                ctx.Database.ExecuteSqlCommand("DELETE FROM WF_SCHEME_PARAMETERS_VALUES");
                ctx.Database.ExecuteSqlCommand("DELETE FROM WF_SCHEMES_PARAMETERS");

                var schemeParameters = schemePV
                                        .GroupBy(s => new
                                          {
                                            SchemeCode = s.SchemeParameter.SchemCode,
                                            SchemParameter = s.SchemeParameter.SchemeParameter
                                            }
                                        )
                                        .ToList();
                                        

                foreach(var sp in schemeParameters)
                {
                    var value = new WF_SCHEMES_PARAMETERS
                    {
                        SCHEME_CODE = sp.Key.SchemeCode,
                        SCHEME_PARAMETER = sp.Key.SchemParameter,
                    };

                    ctx.WF_SCHEMES_PARAMETERS.Attach(value);
                    ctx.Entry(value).State = System.Data.Entity.EntityState.Added;
                    int i = ctx.SaveChanges();
                }

                foreach (var spv in schemePV)
                {
                    var value = new WF_SCHEME_PARAMETERS_VALUES
                    {
                        SCHEME_CODE = spv.SchemeParameter.SchemCode,
                        SCHEME_PARAMETER = spv.SchemeParameter.SchemeParameter,
                        VALUE = spv.Value
                    };

                    ctx.WF_SCHEME_PARAMETERS_VALUES.Attach(value);
                    ctx.Entry(value).State = System.Data.Entity.EntityState.Added;
                    int i = ctx.SaveChanges();
                }
            }

            return schemePV;
        }
    }
}
