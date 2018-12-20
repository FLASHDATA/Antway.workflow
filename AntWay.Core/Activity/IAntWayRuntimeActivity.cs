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
    public interface IAntWayRuntimeActivity//<B> where B : IActivityBinding
    {
        string ActivityId { get; set; }
        string ActivityName { get; set; }

        Task<ActivityExecution> RunAsync(ProcessInstance pi, WorkflowRuntime runtime, object[] parameters = null);
        ActivityExecution Run(ProcessInstance pi, WorkflowRuntime runtime, object[] parameters = null);

        List<string> DifferenceBetweenPersistedAndBindedObject(Guid processId,
                                                               string jsonPersisted,
                                                               ChecksumType checksumType);
        /// <summary>
        /// To store input parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object SetParametersInput(object[] parameters);
        /// <summary>
        /// To get input parameters from persistence provider
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object InputBinding(object[] parameters);

        /// <summary>
        /// To store output parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object SetParametersOutput(object[] parameters);
        /// <summary>
        ///  To get output parameters from persistence provider
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object OutputBinding(object[] parameters);
    }
}
