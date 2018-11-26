using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represent information about case when we try to update a subprocess's scheme but there are no starting transition in a new sccheme of a root process
    /// </summary>
    public class StartingTransitionNotFoundEventArgs : EventArgs
    {
        internal enum SubprocessUpdateDecision
        {
            DropProcess,
            Ignore,
            StartWithNewTransition
        }

        /// <summary>
        /// Id of the process which scheme is impossible to update
        /// </summary>
        public Guid ProcessId { get; internal set; }

        /// <summary>
        /// Id of the root process
        /// </summary>
        public Guid RootProcessId { get; internal set; }

        /// <summary>
        /// The old scheme of the root process
        /// </summary>
        public ProcessDefinition OldRootScheme { get; internal set; }

        /// <summary>
        /// The new scheme of the root process
        /// </summary>
        public ProcessDefinition NewRootScheme { get; internal set; }




        internal StartingTransitionNotFoundEventArgs(Guid processId, Guid rootProcessId, ProcessDefinition oldRootScheme,
            ProcessDefinition newRootScheme, string oldTransitionName)
        {
            ProcessId = processId;
            RootProcessId = rootProcessId;
            OldRootScheme = oldRootScheme;
            NewRootScheme = newRootScheme;
            OldTransitionName = oldTransitionName;
        }

        internal SubprocessUpdateDecision Decision { get;  set; }
        
        /// <summary>
        /// User decide to drop the subprocess
        /// </summary>
        public void DropProcess()
        {
            Decision = SubprocessUpdateDecision.DropProcess;
        }

        /// <summary>
        /// User decide to ignore update of the scheme of the subprocess
        /// </summary>
        public void Ignore()
        {
            Decision = SubprocessUpdateDecision.Ignore;
        }

        /// <summary>
        /// User decide to create the new scheme of the process starting from new transition
        /// </summary>
        /// <param name="transitionName"></param>
        public void StartWithNewTransition(string transitionName)
        {
            Decision = SubprocessUpdateDecision.StartWithNewTransition;
            NewTransitionName = transitionName;
        }

        /// <summary>
        /// New starting transition name
        /// </summary>
        public string NewTransitionName { get; internal set; }

        /// <summary>
        /// Old staring transition name
        /// </summary>
        public string OldTransitionName { get; internal set; }
    }
}
