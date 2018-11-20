using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Runtime
{
    /// <summary>
    /// Represent simple synchronous <see cref="IWorkflowBus"/>
    /// </summary>
    public class AntWayBus : IWorkflowBus
    {
        private WorkflowRuntime _runtime;

        /// <summary>
        /// Initialize the bus
        /// </summary>
        /// <param name="runtime">WorkflowRuntime instance which owned the bus</param>
        public void Initialize(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Starts the bus
        /// </summary>
        public void Start()  { }

        /// <summary>
        /// Queue execution with the list of <see cref="ExecutionRequestParameters"/>
        /// </summary>
        /// <param name="requestParameters">List of <see cref="ExecutionRequestParameters"/></param>
        /// <param name="token">Cancellation token</param>
        /// <param name="notFireExecutionComplete">If true - the Bus must execute the Request without firing ExecutionComplete</param>
        public async Task<bool> QueueExecution(IEnumerable<ExecutionRequestParameters> requestParameters, CancellationToken token, bool notFireExecutionComplete = false)
        {
            var executor = new ActivityExecutor(_runtime, (pi) =>
            {
                BeforeExecution?.Invoke(_runtime, new BeforeActivityExecutionEventArgs(pi));
            });

            //
            var execRequestParam = requestParameters
                                   .ToList()
                                   .FirstOrDefault();

            var response = await executor.Execute(requestParameters, token).ConfigureAwait(false);
            string currentStateName = WorkflowClient.AntWayRunTime
                                      .GetCurrentStateName(execRequestParam.ProcessId);
            if ((currentStateName??"").ToLower().IndexOf("error") >= 0) return false;

            if (ExecutionComplete != null && !notFireExecutionComplete)
            {
                var args = new ExecutionResponseEventArgs(response, token);
                await ExecutionComplete(this, args).ConfigureAwait(false);
            }
            else if (response.IsError)
            {
                var executionErrorParameters = response as ExecutionResponseParametersError;
                if (executionErrorParameters != null) throw executionErrorParameters.Exception;
            }


            return !response.IsEmplty;
        }

        /// <summary>
        /// Queue execution with the <see cref="ExecutionRequestParameters"/>
        /// </summary>
        /// <param name="requestParameters">Instance of <see cref="ExecutionRequestParameters"/></param>
        /// <param name="token">Cancellation token</param>
        /// <param name="notFireExecutionComplete">If true - the Bus must execute the Request without firing ExecutionComplete</param>
        public Task<bool> QueueExecution(ExecutionRequestParameters requestParameters, CancellationToken token, bool notFireExecutionComplete = false)
        {
            return QueueExecution(new[] {requestParameters}, token, notFireExecutionComplete);
        }

        /// <summary>
        /// Raises after the execution was complete
        /// </summary>
        // public event EventHandler<ExecutionResponseEventArgs> ExecutionComplete;
        public event Func<IWorkflowBus,ExecutionResponseEventArgs, Task> ExecutionComplete;

        /// <summary>
        /// Raises before execution of choosen activity
        /// </summary>
        public event EventHandler<BeforeActivityExecutionEventArgs> BeforeExecution;

        ///// <summary>
        ///// Returns true if the bus supports asynchronous model of execution
        ///// </summary>
        //public bool IsAsync => true;
    }
}
