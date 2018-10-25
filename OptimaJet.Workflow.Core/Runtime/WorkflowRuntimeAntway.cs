using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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


        public async void SetErrorState(ProcessInstance processInstance,
                                  string activityDescription,
                                  string identityId = null,
                                  string impersonatedIdentityId = null)
        {
            var activityError = new ActivityDefinition
            {
                Id = "Error",
                Name = $"Error {activityDescription}",
                State = $"Error {activityDescription}",
            };
            processInstance.ProcessScheme.Activities.Add(activityError);

            var activityToSet = processInstance.ProcessScheme.Activities
                                .FirstOrDefault(a => a.State == activityError.State);

            if (activityToSet == null)
                throw new ActivityNotFoundException();

            //SetActivityWithExecution(identityId, impersonatedIdentityId, parameters, activityToSet, processInstance);
            SetActivityWithoutExecution(activityToSet, processInstance);

            await SetProcessNewStatus(processInstance, ProcessStatus.Error);
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


        /// <summary>
        /// Set specified state for specified process 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="id">Activity Id to set</param>
        /// /// <param name="identityId">The user id which set the Activity</param>
        /// <param name="preventExecution">Actions due to transition process do not executed if true</param>
        //public void SetActivity(Guid processId, 
        //                        string id,
        //                        bool preventExecution = false,
        //                        string identityId = null)
        //{
        //    var processInstance = Builder.GetProcessInstance(processId);
        //    var activityToSet = processInstance
        //                        .ProcessScheme
        //                        .Activities.FirstOrDefault(a => a.Id == id);

        //    if (activityToSet == null)
        //        throw new ActivityNotFoundException();

        //    if (!preventExecution)
        //    {
        //       var parameters = new Dictionary<string, object>();

        //       SetActivityWithExecution(identityId, null, parameters,
        //                                activityToSet, processInstance);
        //    }
        //    else
        //        SetActivityWithoutExecution(activityToSet, processInstance);
        //}

    }
}
