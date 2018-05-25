using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Views;

namespace AntWay.EFDAL
{
    public interface IDALSchema: IDAL
    {
        List<WorkflowSchemaView> GetWorkflowSchemes();
    }
}
