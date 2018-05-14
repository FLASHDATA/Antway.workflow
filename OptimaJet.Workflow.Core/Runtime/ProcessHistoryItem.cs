using System;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represents a record of transition of process from one Activity to another
    /// </summary>
    public class ProcessHistoryItem
    {
        /// <summary>
        /// Id of a process instance
        /// </summary>
        public Guid ProcessId { get; set; }
        /// <summary>
        /// An user id for whom a command was executed
        /// </summary>
        public string ActorIdentityId { get; set; }
        /// <summary>
        /// An user id who executed a command
        /// </summary>
        public string ExecutorIdentityId { get; set; }
        /// <summary>
        /// Source activity name
        /// </summary>
        public string FromActivityName { get; set; }
        /// <summary>
        /// Source state name if the state name was specified in source activity
        /// </summary>
        public string FromStateName { get; set; }
        /// <summary>
        /// If true specifies that process was finalized by this transition.
        /// </summary>
        public bool IsFinalised { get; set; }
        /// <summary>
        /// Destination activity name
        /// </summary>
        public string ToActivityName { get; set; }
        /// <summary>
        /// Source state name if the state name was specified in destination activity
        /// </summary>
        public string ToStateName { get; set; }
        /// <summary>
        /// Classifier of a transition direction
        /// </summary>
        public TransitionClassifier TransitionClassifier { get; set; }
        /// <summary>
        /// Time of an execution of transition
        /// </summary>
        public DateTime TransitionTime { get; set; }
        /// <summary>
        /// Name of a command or timer an execution of which led to an execution of transition
        /// </summary>
        public string TriggerName { get; set; }
    }
}