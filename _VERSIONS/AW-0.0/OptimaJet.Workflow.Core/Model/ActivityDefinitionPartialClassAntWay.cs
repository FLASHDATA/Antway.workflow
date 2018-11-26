using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent an activity in a process scheme
    /// </summary>
    public partial class ActivityDefinition 
    {
        public string Id { get; set; }

        /// <summary>
        /// If true specifies that the activity is previous to a call to a service.
        /// </summary>
        public bool IsScheme { get; set; }

        public bool IsCondition { get; set; }

        /// <summary>
        ///  Create ActivityDefinition object
        /// </summary>
        /// <param name="name">Name of the activity</param>
        /// <param name="stateName">Name of the state</param>
        /// <param name="id">Id of the activity</param>
        /// <param name="isInitial">If true specifies that the activity is final. The process is marked as finalized after execution of the activity marked as final.</param>
        /// <param name="isFinal">If true specifies that the activity is final. The process is marked as finalized after execution of the activity marked as final.</param>
        /// <param name="isForSetState">If true specifies that the activity is entry point for a state and possible to set the state with the <see cref="State"/> name via <see cref="WorkflowRuntime.SetState"/> method</param>
        /// <param name="isAutoSchemeUpdate">If true specifies that  if process scheme obsolete than Workflow Runtime will try upgrade it automatically if this activity is current</param>
        /// <param name="isScheme">It is previous from a call to a scheme</param>
        /// <param name="isCondition">It is a condition</param>
        /// <returns>ActivityDefinition object</returns>
        public static ActivityDefinition Create(string name, string stateName, 
                                                string id,
                                                string isInitial, string isFinal, 
                                                string isForSetState, string isAutoSchemeUpdate,
                                                string isScheme,
                                                string isCondition)
        {
            return new ActivityDefinition()
            {
                Name = name,
                State = stateName,

                Id = id,

                IsInitial = !string.IsNullOrEmpty(isInitial) && bool.Parse(isInitial),
                IsFinal = !string.IsNullOrEmpty(isFinal) && bool.Parse(isFinal),

                IsForSetState = !string.IsNullOrEmpty(isForSetState) && bool.Parse(isForSetState),
                IsAutoSchemeUpdate = !string.IsNullOrEmpty(isAutoSchemeUpdate) && bool.Parse(isAutoSchemeUpdate),

                IsScheme = !string.IsNullOrEmpty(isScheme) && bool.Parse(isScheme),
                IsCondition = !string.IsNullOrEmpty(isCondition) && bool.Parse(isCondition),

                Implementation = new List<ActionDefinitionReference>(),
                PreExecutionImplementation = new List<ActionDefinitionReference>()
            };
        }
    }
}
