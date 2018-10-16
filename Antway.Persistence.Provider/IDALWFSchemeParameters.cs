using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider.Model
{
    public interface IDALWFSchemeParameters
    {
        List<WorkflowSchemeParameterView>
            GetWorkflowSchemeParameters(string schemCode = null);

        List<WorkflowSchemeParameterValuesView> 
            GetWorkflowSchemeParameterValues(string schemCode = null);

        List<WorkflowSchemeParameterValuesView> 
            UpdateScheme(List<WorkflowSchemeParameterValuesView> schemePV);
    }
}
