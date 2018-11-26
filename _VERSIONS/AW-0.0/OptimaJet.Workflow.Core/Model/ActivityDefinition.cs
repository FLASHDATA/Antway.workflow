using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent an activity in a process scheme
    /// </summary>
    public partial class ActivityDefinition : BaseDefinition
    {
        /// <summary>
        /// Name of the state
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// If true specifies that the activity is initial. There can be only one initial activity in scheme. Initial activity is the entry point of the process.
        /// </summary>
        public bool IsInitial { get; set; }
        /// <summary>
        /// If true specifies that the activity is final. The process is marked as finalized after execution of the activity marked as final.
        /// </summary>
        public bool IsFinal { get; set; }
        /// <summary>
        /// If true specifies that the activity is entry point for a state and possible to set the state with the <see cref="State"/> name via <see cref="WorkflowRuntime.SetState"/> method
        /// </summary>
        public bool IsForSetState { get; set; }
        /// <summary>
        /// If true specifies that  if process scheme obsolete than Workflow Runtime will try upgrade it automatically if this activity is current
        /// </summary>
        public bool IsAutoSchemeUpdate { get; set; }
        
        /// <summary>
        /// Specifies that activity have an implementation
        /// </summary>
        public bool HaveImplementation
        {
            get { return Implementation.Count > 0; }
        }

        /// <summary>
        /// Specifies that activity have a pre-execution implementation
        /// </summary>
        public bool HavePreExecutionImplementation
        {
            get { return PreExecutionImplementation.Count > 0; }
        }

        /// <summary>
        /// List of <see cref="ActionDefinitionReference"/> which are executed at standard workflow execution
        /// </summary>
        public List<ActionDefinitionReference> Implementation { get; set; }

        /// <summary>
        /// List of <see cref="ActionDefinitionReference"/> which are executed at pre-execution
        /// </summary>
        public List<ActionDefinitionReference> PreExecutionImplementation { get; set; } 

        /// <summary>
        /// Specifies that state is assigned
        /// </summary>
        public bool IsState
        {
            get { return !string.IsNullOrEmpty(State); }
        }

        /// <summary>
        ///  Create ActivityDefinition object
        /// </summary>
        /// <param name="name">Name of the activity</param>
        /// <param name="stateName">Name of the state</param>
        /// <param name="isInitial">If true specifies that the activity is final. The process is marked as finalized after execution of the activity marked as final.</param>
        /// <param name="isFinal">If true specifies that the activity is final. The process is marked as finalized after execution of the activity marked as final.</param>
        /// <param name="isForSetState">If true specifies that the activity is entry point for a state and possible to set the state with the <see cref="State"/> name via <see cref="WorkflowRuntime.SetState"/> method</param>
        /// <param name="isAutoSchemeUpdate">If true specifies that  if process scheme obsolete than Workflow Runtime will try upgrade it automatically if this activity is current</param>
        /// <returns>ActivityDefinition object</returns>
        public static ActivityDefinition Create(string name, string stateName, string isInitial, string isFinal, string isForSetState, string isAutoSchemeUpdate)
        {
            return new ActivityDefinition()
                       {
                           IsFinal = !string.IsNullOrEmpty(isFinal) && bool.Parse(isFinal),
                           IsInitial = !string.IsNullOrEmpty(isInitial) && bool.Parse(isInitial),
                           IsForSetState = !string.IsNullOrEmpty(isForSetState) && bool.Parse(isForSetState),
                           IsAutoSchemeUpdate = !string.IsNullOrEmpty(isAutoSchemeUpdate) && bool.Parse(isAutoSchemeUpdate),
                           Name = name,
                           State = stateName,
                           Implementation = new List<ActionDefinitionReference>(),
                           PreExecutionImplementation = new List<ActionDefinitionReference>()
                       };
        }

        /// <summary>
        ///  Create ActivityDefinition object
        /// </summary>
        /// <param name="name">Name of the activity</param>
        /// <param name="stateName">Name of the state</param>
        /// <param name="isInitial">If true specifies that the activity is final. The process is marked as finalized after execution of the activity marked as final.</param>
        /// <param name="isFinal">If true specifies that the activity is final. The process is marked as finalized after execution of the activity marked as final.</param>
        /// <param name="isForSetState">If true specifies that the activity is entry point for a state and possible to set the state with the <see cref="State"/> name via <see cref="WorkflowRuntime.SetState"/> method</param>
        /// <param name="isAutoSchemeUpdate">If true specifies that  if process scheme obsolete than Workflow Runtime will try upgrade it automatically if this activity is current</param>
        /// <returns>ActivityDefinition object</returns>
        public static ActivityDefinition Create(string name, string stateName, bool isInitial, bool isFinal, bool isForSetState, bool isAutoSchemeUpdate)
        {
            return new ActivityDefinition()
            {
                IsFinal = isFinal,
                IsInitial = isInitial,
                IsForSetState = isForSetState,
                IsAutoSchemeUpdate = isAutoSchemeUpdate,
                Name = name,
                State = stateName,
                Implementation = new List<ActionDefinitionReference>(),
                PreExecutionImplementation = new List<ActionDefinitionReference>()
            };
        }

        /// <summary>
        /// Add <see cref="ActionDefinitionReference"/> to implementation list
        /// </summary>
        /// <param name="action">Action reference</param>
        public void AddAction(ActionDefinitionReference action)
        {
            Implementation.Add(action);
        }

        /// <summary>
        /// Add <see cref="ActionDefinitionReference"/> to pre-execution implementation list
        /// </summary>
        /// <param name="action">Action reference</param>
        public void AddPreExecutionAction(ActionDefinitionReference action)
        {
            PreExecutionImplementation.Add(action);
        }

        /// <summary>
        /// Nesting level of subprocess, 0 - root process, 1-... -subprocesses, null - not assigned
        /// </summary>
        public int? NestingLevel { get; set; }

        public new ActivityDefinition Clone()
        {
            var newActivity = base.Clone() as ActivityDefinition;

            if (newActivity == null)
                throw new Exception("Cloning of ActivityDefinition fails");

            newActivity.Implementation = new List<ActionDefinitionReference>();
            Implementation.ForEach(a => newActivity.Implementation.Add(a.Clone()));

            newActivity.PreExecutionImplementation = new List<ActionDefinitionReference>();
            PreExecutionImplementation.ForEach(a => newActivity.PreExecutionImplementation.Add(a.Clone()));

            return newActivity;
        }
    }
}
