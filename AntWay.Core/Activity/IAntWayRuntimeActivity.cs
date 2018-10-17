using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Model;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Activity
{
    public interface IAntWayRuntimeActivity
    {
        string ActivityId { get; set; }
        string ActivityName { get; set; }
        object ParametersBind { get; set; }

        Task<ActivityExecution> RunAsync(ProcessInstance pi, WorkflowRuntime runtime, object[] parameters = null);
        ActivityExecution Run(ProcessInstance pi, WorkflowRuntime runtime, object[] parameters = null);
    }
}
