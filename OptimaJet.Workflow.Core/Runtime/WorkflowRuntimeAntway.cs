using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    public sealed partial class WorkflowRuntime
    {
        public ICommandsMapping CommandMapping { get; set; }


        /// <summary>
        /// Return the current activity name of specified process
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>Current activity name</returns>
        public string GetCurrentActivityId(Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);

            PersistenceProvider.FillSystemProcessParameters(processInstance);

            return processInstance.GetParameter(DefaultDefinitions.ParameterCurrentActivity.Name).Value.ToString();
        }


        /// <summary>
        /// Set specified state for specified process 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="id">Activity Id to set</param>
        /// /// <param name="identityId">The user id which set the Activity</param>
        /// <param name="preventExecution">Actions due to transition process do not executed if true</param>
        public void SetActivity(Guid processId, 
                                string id,
                                bool preventExecution = false,
                                string identityId = null)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            var activityToSet = processInstance
                                .ProcessScheme
                                .Activities.FirstOrDefault(a => a.Id == id);

            if (activityToSet == null)
                throw new ActivityNotFoundException();

            if (!preventExecution)
            {
               var parameters = new Dictionary<string, object>();

               SetActivityWithExecution(identityId, null, parameters,
                                        activityToSet, processInstance);
            }
            else
                SetActivityWithoutExecution(activityToSet, processInstance);
        }


        public async Task<bool> ExecuteTriggeredTransitions(ProcessInstance processInstance, 
                                                List<TransitionDefinition> transitions)
        {
            try
            {
                processInstance.SetStartTransitionalProcessActivity();

                var newExecutionParameters = new List<ExecutionRequestParameters>();
                newExecutionParameters.AddRange(transitions.Select(at => ExecutionRequestParameters.Create(processInstance, at)));
                return await  Bus.QueueExecution(newExecutionParameters, CancellationToken.None)
                        .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                //throw new Exception($"Error Execute {type:G} Workflow Id={processInstance.ProcessId}", ex);
            }

            return false;
        }
    }
}
