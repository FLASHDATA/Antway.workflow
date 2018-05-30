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
        public IDALWFLocator IDALLocator { get; set; }
        public IDALWFSchema IDALSchema { get; set; }


        public List<WorkflowSchemaView> GetSchemes()
        {
            var schemes = IDALSchema.GetWorkflowSchemes();
            return schemes;
        }

        public WorkflowLocatorView GetWorkflowSchemes(Guid guid)
        {
            var dataView = IDALLocator.GetLocatorFromGuid(guid);
            return dataView;
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
