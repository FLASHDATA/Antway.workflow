using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Providers
{
    public class EmptyAsembly : IAssemblies
    {
        public void RegisterAssembliesForServiceLocator()
        {
            
        }

        public void RegisterAssembliesForWorkflowDesigner(WorkflowRuntime runtime)
        {
            
        }
    }
}
