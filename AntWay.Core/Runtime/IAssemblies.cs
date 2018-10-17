using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Runtime
{
    public interface IAssemblies
    {
        void RegisterAssembliesForWorkflowDesigner(WorkflowRuntime runtime);
        void RegisterAssembliesForServiceLocator();
    }
}
