using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using AntWay.Core.Providers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace Scheme.Model.Providers
{
    /// <summary>
    /// Solo es un ejemplo para ver que se puede sobreescribir AntWayActionProvider
    /// No se utiliza
    /// </summary>
    public class ExpedientesActionProvider: AntWayActionProvider
    {
        protected override void RunAntWayActivity(ProcessInstance processInstance,
                                                  WorkflowRuntime runtime,
                                                  string actionParameter)
        {
            base.RunAntWayActivity(processInstance, runtime, actionParameter);
        }

        protected override void RunAntWayActivityAsync(ProcessInstance processInstance,
                                          WorkflowRuntime runtime,
                                          string actionParameter)
        {
            base.RunAntWayActivityAsync(processInstance, runtime, actionParameter);
        }
    }
}
