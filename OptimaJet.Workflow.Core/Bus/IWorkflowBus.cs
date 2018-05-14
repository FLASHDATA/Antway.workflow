﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Bus
{
    /// <summary>
    ///  Interface of a bus, which provide control over execution of activities and actions
    /// </summary>
    public interface IWorkflowBus
    {
        /// <summary>
        /// Initialize the bus
        /// </summary>
        /// <param name="runtime">WorkflowRuntime instance which owned the bus</param>
        void Initialize(WorkflowRuntime runtime);
        /// <summary>
        /// Starts the bus
        /// </summary>
        void Start();

        /// <summary>
        /// Queue execution with the list of <see cref="ExecutionRequestParameters"/>
        /// </summary>
        /// <param name="requestParameters">List of <see cref="ExecutionRequestParameters"/></param>
        /// <param name="token">Cancellation token</param>
        /// <param name="notFireExecutionComplete">If true - the Bus must execute the Request without firing ExecutionComplete</param>
        /// <returns>True if some activity and actions were executed</returns>
        Task<bool> QueueExecution(IEnumerable<ExecutionRequestParameters> requestParameters, CancellationToken token, bool notFireExecutionComplete = false);

        /// <summary>
        /// Queue execution with the <see cref="ExecutionRequestParameters"/>
        /// </summary>
        /// <param name="requestParameters">Instance of <see cref="ExecutionRequestParameters"/></param>
        /// /// <param name="token">Cancellation token</param>
        /// <param name="notFireExecutionComplete">If true - the Bus must execute the Request without firing ExecutionComplete</param>
        /// <returns>True if some activity and actions were executed</returns>
        Task<bool> QueueExecution(ExecutionRequestParameters requestParameters, CancellationToken token, bool notFireExecutionComplete = false);

        /// <summary>
        /// Raises after the execution was complete
        /// </summary>
        // event EventHandler<ExecutionResponseEventArgs> ExecutionComplete;
        event Func<IWorkflowBus,ExecutionResponseEventArgs, Task> ExecutionComplete;

        /// <summary>
        /// Raises before execution of choosen activity
        /// </summary>
        event EventHandler<BeforeActivityExecutionEventArgs> BeforeExecution;

        ///// <summary>
        ///// Returns true if the bus supports asynchronous model of execution
        ///// </summary>
        //bool IsAsync { get; }
    }
}
