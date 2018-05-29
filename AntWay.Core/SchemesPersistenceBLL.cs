using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.EFDAL;
using AntWay.Views;

namespace AntWay.BLL
{
    public class SchemesPersistenceBLL
    {
        public IDALLocator IDALLocator { get; set; }
        public IDALSchema IDALSchema { get; set; }

        public List<WorkflowSchemaView> GetSchemes()
        {
            var schemes = IDALSchema.GetWorkflowSchemes();
            return schemes;
        }

        public WorkflowLocatorView GetWorkflowLocator(string locator)
        {
            var dataView = IDALLocator.Fetch<WorkflowLocatorView>(locator);
            return dataView;
        }

        public WorkflowLocatorView AddWorkflowLocator(WorkflowLocatorView wfLocatorView)
        {
            var result = IDALLocator.Insert(wfLocatorView);
            return result;
        }
    }
}
