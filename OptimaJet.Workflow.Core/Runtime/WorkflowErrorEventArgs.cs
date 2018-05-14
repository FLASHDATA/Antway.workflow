using System;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represent information about an error occurred due to execution of a workflow process
    /// </summary>
    public class WorkflowErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processInstance">Instance of the process the execution of which caused the error</param>
        /// <param name="processStatus">Status of the process</param>
        /// <param name="executedTransition">Transition the execution of which caused the error</param>
        /// <param name="exception">Exception which was thrown</param>
        public WorkflowErrorEventArgs(ProcessInstance processInstance, ProcessStatus processStatus, TransitionDefinition executedTransition, Exception exception)
        {
            ProcessInstance = processInstance;
            ProcessStatus = processStatus;
            ExecutedTransition = executedTransition;
            Exception = exception;
        }

        /// <summary>
        /// Returns the exception which was thrown
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Returns the instance of the process the execution of which caused the error
        /// </summary>
        public ProcessInstance ProcessInstance { get; set; }

        /// <summary>
        /// Status of the process
        /// </summary>
        public ProcessStatus ProcessStatus { get; set; }

        /// <summary>
        /// Returns the transition the execution of which caused the error
        /// </summary>
        public TransitionDefinition ExecutedTransition { get; set; }

    }
}
