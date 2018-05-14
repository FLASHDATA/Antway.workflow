using System;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represent information about an activity which was planned to execute, contains ProcessInstance object
    /// </summary>
    public class BeforeActivityExecutionEventArgs : EventArgs
    {
          /// <summary>
        /// Id of the process whose current activity was changed
        /// </summary>
        public Guid ProcessId
        {
            get { return ProcessInstance.ProcessId; }
        }

        /// <summary>
        /// If true means that the current activity was changed for subprocess
        /// </summary>
        public bool IsSubprocess
        {
            get { return ProcessInstance.IsSubprocess; }
        }

        /// <summary>
        /// Code of the scheme of the process
        /// </summary>
        public string SchemeCode
        {
            get { return ProcessInstance.ProcessScheme.Name; }
        }


        /// <summary>
        /// Instance of the process
        /// </summary>
        public ProcessInstance ProcessInstance { get; private set; }

        /// <summary>
        /// Returns the name of executed activity.
        /// </summary>
        public string ExecutedActivityName
        {
            get { return ProcessInstance.ExecutedActivity.Name; }
        }

        /// <summary>
        /// Returns the state name of executed activity.
        /// </summary>
        public string ExecutedActivityState
        {
            get { return ProcessInstance.ExecutedActivity.State; }
        }



        /// <summary>
        /// Returns the executed activity.
        /// </summary>
        public ActivityDefinition ExecutedActivity
        {
            get { return ProcessInstance.ExecutedActivity; }
            
        }

        /// <summary>
        /// Returns the executed transition.
        /// </summary>
        public TransitionDefinition ExecutedTransition
        {
            get { return ProcessInstance.ExecutedTransition; }

        }


        /// <summary>
        /// Returns the current activity. Activity which was final for last executed transition <see cref="TransitionDefinition.From"/>
        /// </summary>
        public ActivityDefinition CurrentActivity
        {
            get { return ProcessInstance.CurrentActivity; }

        }


        /// <summary>
        /// Returns the name of current activity. Activity which was final for last executed transition <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string CurrentActivityName
        {
            get { return ProcessInstance.CurrentActivityName; }

        }

        /// <summary>
        /// Returns the current state.
        /// </summary>
        public string CurrentState
        {
            get { return ProcessInstance.CurrentState; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        public BeforeActivityExecutionEventArgs(ProcessInstance processInstance)
        {
            ProcessInstance = processInstance;
        }
    }
}
