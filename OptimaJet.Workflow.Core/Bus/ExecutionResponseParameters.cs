using System;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Bus
{
    /// <summary>
    /// Represent execution result which contains information about execution
    /// </summary>
    public class ExecutionResponseParameters
    {
        /// <summary>
        /// Returns id of the process instance
        /// </summary>
        public Guid ProcessId => ProcessInstance.ProcessId;

        /// <summary>
        /// Returns Process instance which requested execution 
        /// </summary>
        public ProcessInstance ProcessInstance { get; set; }
        /// <summary>
        /// Transition name which was executed
        /// </summary>
        public string ExecutedTransitionName { get; set; }
        /// <summary>
        /// Activity name which was executed
        /// </summary>
        public string ExecutedActivityName { get; set; }
        /// <summary>
        /// If True means execution error
        /// </summary>
        public virtual bool IsError => false;

        /// <summary>
        /// Create ExecutionResponseParametersComplete object
        /// </summary>
        /// <param name="processInstance">Process instance which requested execution </param>
        /// <param name="executedActivityName">Activity name which was executed</param>
        /// <returns>ExecutionResponseParametersComplete object</returns>
        public static ExecutionResponseParametersComplete Create(ProcessInstance processInstance,
                                                                 string executedActivityName
                                                                 )
        {
            return new ExecutionResponseParametersComplete
                       {
                           ProcessInstance = processInstance,
                           ExecutedActivityName = executedActivityName
                       };
        }

        /// <summary>
        /// Create ExecutionResponseParametersComplete object
        /// </summary>
        /// <param name="processInstance">Process instance which requested execution </param>
        /// <param name="executedActivityName">Activity name which was executed</param>
        /// <param name="executedTransitionName">Transition name which was executed</param>
        /// <returns>ExecutionResponseParametersComplete object</returns>
        public static ExecutionResponseParametersComplete Create(ProcessInstance processInstance,
                                                                 string executedActivityName,
                                                                 string executedTransitionName
                                                                 )
        {
            return new ExecutionResponseParametersComplete
                       {
                           ProcessInstance = processInstance,
                           ExecutedTransitionName = executedTransitionName,
                           IsEmplty = false,
                           ExecutedActivityName = executedActivityName
                       };
        }

        /// <summary>
        /// Create ExecutionResponseParametersError object
        /// </summary>
        /// <param name="processInstance">Process instance which requested execution </param>
        /// <param name="executedActivityName">Activity name which was executed</param>
        /// <param name="exception">Exception which occured in time of execution</param>
        /// <returns>ExecutionResponseParametersError object</returns>
        public static ExecutionResponseParametersError Create(ProcessInstance processInstance,
                                                               string executedActivityName,
                                                               Exception exception)
        {
            return new ExecutionResponseParametersError
            {
                ProcessInstance = processInstance,
                Exception = exception,
                ExecutedActivityName = executedActivityName
            };
        }


        /// <summary>
        /// Create ExecutionResponseParametersError object
        /// </summary>
        /// <param name="processInstance">Process instance which requested execution </param>
        /// <param name="executedActivityName">Activity name which was executed</param>
        /// <param name="executedTransitionName">Transition name which was executed</param>
        /// <param name="exception">Exception which occured in time of execution</param>
        /// <returns>ExecutionResponseParametersError object</returns>
        public static ExecutionResponseParametersError Create(ProcessInstance processInstance,
                                                                 string executedActivityName,
                                                                 string executedTransitionName,
                                                                 Exception exception)
        {
            return new ExecutionResponseParametersError
            {
                ProcessInstance = processInstance,
                ExecutedTransitionName = executedTransitionName,
                IsEmplty = false,
                Exception = exception,
                ExecutedActivityName = executedActivityName
            };
        }

        /// <summary>
        /// Return True if ExecutionResponseParameters is empty
        /// </summary>
        public bool IsEmplty { get; set; }

        /// <summary>
        /// Returns instane of empty ExecutionResponseParameters
        /// </summary>
        public static ExecutionResponseParameters Empty => new ExecutionResponseParameters {IsEmplty = true};
    }

    /// <summary>
    /// Represent execution result completed without errors
    /// </summary>
    public sealed class ExecutionResponseParametersComplete : ExecutionResponseParameters
    {
        
    }

    /// <summary>
    /// Represent execution result completed with errors
    /// </summary>
    public sealed class ExecutionResponseParametersError : ExecutionResponseParameters
    {
        /// <summary>
        /// If True means execution error
        /// </summary>
        public override bool IsError => true;

        /// <summary>
        /// Exception which occured in time of execution
        /// </summary>
        public Exception Exception { get; set; }
    }
}
