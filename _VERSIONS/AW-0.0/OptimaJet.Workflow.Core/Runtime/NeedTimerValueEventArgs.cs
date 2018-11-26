using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Event args for request timer value
    /// </summary>
    public sealed class NeedTimerValueEventArgs : EventArgs
    {
        public sealed class TimerValueRequest
        {
            private readonly List<TransitionDefinition> _triggeredTransitions = new List<TransitionDefinition>();

            /// <summary>
            /// The Timer name
            /// </summary>
            public string Name
            {
                get { return Definition.Name; }
            }

            /// <summary>
            /// The timer definition object
            /// </summary>
            public TimerDefinition Definition { get; private set; }


            /// <summary>
            /// The timer value that was specified in the scheme
            /// </summary>
            public string OriginalValue
            {
                get { return Definition.Value; }
            }

            /// <summary>
            /// New value of the timer
            /// </summary>
            public DateTime? NewValue { get; set; }

            public IEnumerable<TransitionDefinition> TriggeredTransitions
            {
                get { return _triggeredTransitions; }
            }

            internal void AddTriggeredTransition(TransitionDefinition triggeredTransition)
            {
                _triggeredTransitions.Add(triggeredTransition);
            }

            internal TimerValueRequest(TimerDefinition timerDefinition, TransitionDefinition triggeredTransition)
            {
                Definition = timerDefinition;
                _triggeredTransitions.Add(triggeredTransition);
            }
        }

        private readonly Dictionary<string, TimerValueRequest> _timerValueRequests = new Dictionary<string, TimerValueRequest>();

        /// <summary>
        /// Instance of the process
        /// </summary>
        public ProcessInstance ProcessInstance { get; private set; }

        /// <summary>
        /// Id of the process
        /// </summary>
        public Guid ProcessId
        {
            get { return ProcessInstance.ProcessId; }
        }



        public IEnumerable<TimerValueRequest> TimerValueRequests
        {
            get { return _timerValueRequests.Values; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processInstance">Instance of the process</param>
        /// <param name="timerTransitions">Timer triggered transitions</param>
        public NeedTimerValueEventArgs(ProcessInstance processInstance, List<TransitionDefinition> timerTransitions)
        {
        
            ProcessInstance = processInstance;
            foreach (var timerTransition in timerTransitions.Where(t => t.Trigger.Type == TriggerType.Timer))
            {
                var timerName = timerTransition.Trigger.Timer.Name;
                if (!_timerValueRequests.ContainsKey(timerName))
                    _timerValueRequests.Add(timerName, new TimerValueRequest(timerTransition.Trigger.Timer, timerTransition));
                else
                    _timerValueRequests[timerName].AddTriggeredTransition(timerTransition);
            }
        }
    }
}