using System;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represent information about changed status of a process 
    /// </summary>
    public class ProcessStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Id of the process whose status was changed
        /// </summary>
        public Guid ProcessId { get; private set; }
        /// <summary>
        /// If true means that the status was changed for subprocess
        /// </summary>
        public bool IsSubprocess { get; private set; }
        /// <summary>
        /// Previous status of the process <see cref="ProcessStatus"/>
        /// </summary>
        public ProcessStatus OldStatus { get; private set; }

        /// <summary>
        /// Actual status of the process <see cref="ProcessStatus"/>
        /// </summary>
        public ProcessStatus NewStatus { get; private set; }

        /// <summary>
        /// Code of the scheme of the process
        /// </summary>
        public string SchemeCode { get; internal set; }

        /// <summary>
        /// Instance of the process
        /// </summary>
        public ProcessInstance ProcessInstance { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processId">Id of the process whose status was changed</param>
        /// <param name="oldStatus">Previous status of the process</param>
        /// <param name="newStatus">Actual status of the process</param>
        public ProcessStatusChangedEventArgs(Guid processId, bool isSubprocess, ProcessStatus oldStatus, ProcessStatus newStatus)
        {
            ProcessId = processId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            IsSubprocess = isSubprocess;
        }
    }
}
