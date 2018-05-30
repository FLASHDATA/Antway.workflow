using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Model;

namespace AntWay.Persistence.Provider
{
    public interface IDALWFSchema: IDAL
    {
        List<WorkflowSchemaView> GetWorkflowSchemes();
    }
}
