using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Model;

namespace AntWay.Persistence.Provider
{
    public class SchemesPersistence
    {
        public IDALWFSchema IDALSchema { get; set; }

        public List<WorkflowSchemaView> GetSchemes()
        {
            var schemes = IDALSchema.GetWorkflowSchemes();
            return schemes;
        }
    }
}
