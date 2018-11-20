using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Runtime
{
    public class EmptyAsembly : IAssemblies
    {
        public List<Type> GetTypes()
        {
            return new List<Type>();
        }

        public void RegisterAssembliesForServiceLocator()
        {
            
        }

        public void RegisterAssembliesForWorkflowDesigner(WorkflowRuntime runtime)
        {
            
        }
    }
}
