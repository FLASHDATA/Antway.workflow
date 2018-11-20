using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

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


        public void SetErrorState(ProcessInstance processInstance,
                                      string activityId,
                                      string activityName,
                                      string activityState,
                                      List<string> errorsDescription,
                                      string identityId = null)
        {
            var activityError = new ActivityDefinition
            {
                Id = activityId,
                Name = $"{activityId} at {activityName}",
                State = activityState,
                IsForSetState = true,
            };
            processInstance.ProcessScheme.Activities.Add(activityError);

            var activityToSet = processInstance.ProcessScheme.Activities
                                .FirstOrDefault(a => a.State == activityError.State);

            if (activityToSet == null)
                throw new ActivityNotFoundException();

            SetActivityWithoutExecution(activityToSet, processInstance, true);

            processInstance.SetParameter(activityId,
                                         errorsDescription,
                                         ParameterPurpose.Persistence);
            PersistenceProvider.SavePersistenceParameters(processInstance);

            SetProcessNewStatus(processInstance, ProcessStatus.Error);
        }



        public async Task<bool> ExecuteTriggeredTransitions(ProcessInstance processInstance,
                                                            List<TransitionDefinition> transitions)
        {
            try
            {
                processInstance.SetStartTransitionalProcessActivity();

                var newExecutionParameters = new List<ExecutionRequestParameters>();
                newExecutionParameters.AddRange(transitions.Select(at => ExecutionRequestParameters.Create(processInstance, at)));
                return await Bus.QueueExecution(newExecutionParameters, CancellationToken.None)
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
