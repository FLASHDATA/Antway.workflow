using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Model;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using static AntWay.Core.Manager.Checksum;

namespace AntWay.Core.Activity
{
    public interface IAntWayRuntimeActivity
    {
        string ActivityId { get; set; }
        string ActivityName { get; set; }

        /// <summary>
        /// Run activity implementation and persists it's execution
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="runtime"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        ActivityExecution Run(ProcessInstance pi, WorkflowRuntime runtime, object[] parameters = null);

        /// <summary>
        /// Asyn run implementation
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="runtime"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<ActivityExecution> RunAsync(ProcessInstance pi, WorkflowRuntime runtime, object[] parameters = null);


        /// <summary>
        /// Takes (from your implementation) and store (through persistence provider) input parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object SetParametersInput(object[] parameters);

        /// <summary>
        /// Gets input parameters from persistence provider
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object InputBinding(object[] parameters);

        /// <summary>
        /// Takes (from your implementation) and store (through persistence provider) output parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object SetParametersOutput(object[] parameters);

        /// <summary>
        /// Gets output parameters from persistence provider
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object OutputBinding(object[] parameters);
    }
}
