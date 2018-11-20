using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Bus
{

    /// <summary>
    /// Represent execution request which contains conditions and actions
    /// </summary>
public sealed class ExecutionRequestParameters
    {
        /// <summary>
        /// Returns Process instance which requested execution 
        /// </summary>
        public ProcessInstance ProcessInstance { get; private set; }

        /// <summary>
        /// Returns id of the process instance
        /// </summary>
        public Guid ProcessId
        {
            get { return ProcessInstance.ProcessId; }
        }

        /// <summary>
        /// List of actions to execute in the case of conditions are satisfied
        /// </summary>
        public ActionDefinitionReference[] Methods { get; set; }

        /// <summary>
        /// List of conditions to check
        /// </summary>
        public ConditionDefinition[] Conditions { get; set; }

        /// <summary>
        /// Executed transition name
        /// </summary>
        public string TransitionName { get; set; }

        /// <summary>
        /// Executed activity name <see cref="TransitionDefinition.To"/>
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Type of concatenation for conditions
        /// </summary>
        public ConcatenationType ConditionsConcatenationType { get; set; }
        
        /// <summary>
        /// Create ExecutionRequestParameters object
        /// </summary>
        /// <param name="processInstance">Process instance which requested execution</param>
        /// <param name="transition">Executed transition</param>
        /// <returns>ExecutionRequestParameters object</returns>
        public static ExecutionRequestParameters Create(ProcessInstance processInstance, TransitionDefinition transition)
        {
            return Create(processInstance, transition, false);
        }

        /// <summary>
        /// Create ExecutionRequestParameters object
        /// </summary>
        /// <param name="processInstance">Process instance which requested execution</param>
        /// <param name="transition">Executed transition</param>
        /// <param name="isPreExecution">If true pre-execution implementation will be requested to execute <see cref="ActivityDefinition.PreExecutionImplementation"/>, if false  implementation will be requested to execute <see cref="ActivityDefinition.Implementation"/></param>
        /// <returns>ExecutionRequestParameters object</returns>
        public static ExecutionRequestParameters Create(ProcessInstance processInstance, TransitionDefinition transition,
            bool isPreExecution)
        {
            var ret = Create(processInstance, transition.To, transition.Conditions, isPreExecution);
            ret.TransitionName = transition.Name;
            ret.ActivityName = transition.To.Name;
            ret.ConditionsConcatenationType = transition.ConditionsConcatenationType;
            return ret;
        }

        /// <summary>
        /// Create ExecutionRequestParameters object
        /// </summary>
        /// <param name="processInstance">Process instance which requested execution</param>
        /// <param name="activityToExecute">Activity to execute</param>
        /// <param name="conditions">List of conditions to check</param>
        /// <returns>ExecutionRequestParameters object</returns>
        public static ExecutionRequestParameters Create(ProcessInstance processInstance,
            ActivityDefinition activityToExecute, List<ConditionDefinition> conditions)
        {
            return Create(processInstance, activityToExecute, conditions, false);
        }

        /// <summary>
        /// Create ExecutionRequestParameters object
        /// </summary>
        /// <param name="processInstance">Process instance which requested execution</param>
        /// <param name="activityToExecute">Activity to execute</param>
        /// <param name="conditions">List of conditions to check</param>
        /// <param name="isPreExecution">If true pre-execution implementation will be requested to execute <see cref="ActivityDefinition.PreExecutionImplementation"/>, if false  implementation will be requested to execute <see cref="ActivityDefinition.Implementation"/></param>
        /// <returns>ExecutionRequestParameters object</returns>
        public static ExecutionRequestParameters Create(ProcessInstance processInstance,
            ActivityDefinition activityToExecute, List<ConditionDefinition> conditions, bool isPreExecution)
        {
            List<ActionDefinitionReference> implementation = isPreExecution
                ? activityToExecute.PreExecutionImplementation
                : activityToExecute.Implementation;

            if (processInstance == null) throw new ArgumentNullException("processInstance");

            var executionParameters = new ExecutionRequestParameters
            {
                ProcessInstance = processInstance,
                Conditions = conditions.ToArray(),
                Methods = implementation.ToArray(),
                ActivityName = activityToExecute.Name,
                Activity = activityToExecute,
                ConditionsConcatenationType = ConcatenationType.And

            };

            return executionParameters;

        }

        /// <summary>
        /// Activity to execute
        /// </summary>
        public ActivityDefinition Activity { get; private set; }

        /// <summary>
        /// Returns true if condition type of the transition is equal "Always" <see cref="ConditionType.Always"/>
        /// </summary>
        public bool IsAlways
        {
            get { return Conditions.Any(c => c.Type == ConditionType.Always); }
        }

        /// <summary>
        /// Returns true if condition type of the transition is equal "Otherwise" <see cref="ConditionType.Otherwise"/>
        /// </summary>
        public bool IsOtherwise
        {
            get { return Conditions.Any(c => c.Type == ConditionType.Otherwise); }
        }

        /// <summary>
        /// Returns true if condition type of the transition is equal "Action" <see cref="ConditionType.Action"/>
        /// </summary>
        public bool IsCondition
        {
            get { return Conditions.All(c => c.Type == ConditionType.Action); }
        }
    }
}
